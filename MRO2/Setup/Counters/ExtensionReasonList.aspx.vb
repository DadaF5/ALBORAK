Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

' ============================================================
' MRO2/Setup/Counters/ExtensionReasonList.aspx.vb
' Lookup table: mro2.ExtensionReason
' 5 seeded values:
'   MFR_TOL     — Manufacturer tolerance (CMM pre-approved)
'   DOC_REF     — Document reference (CMM, SB, AD, EO)
'   OPS_NEC     — Operational necessity (ops approval)
'   REG_AUTH    — Regulatory authority (MARC/DGAM)
'   PARTS_AVAIL — Spare parts unavailable
'
' Key feature: RequiresDocRef + RequiresApprover flags drive
' mandatory field validation in usp_SNTaskCounterExtension_Grant.
' Displayed as checkboxes in modal, shown as badges in grid.
' ============================================================
Partial Class MRO2_Setup_Counters_ExtensionReasonList
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    'Private Property SortColumn As String
    '    Get : Return If(TryCast(ViewState("SC"), String), "SortOrder") : End Get
    '    Set(v As String) : ViewState("SC") = v : End Set
    'End Property
    'Private Property SortDir As String
    '    Get : Return If(TryCast(ViewState("SD"), String), "ASC") : End Get
    '    Set(v As String) : ViewState("SD") = v : End Set
    'End Property
    Private Property SortColumn As String
        Get
            Dim val = TryCast(ViewState("SC"), String)
            Return If(String.IsNullOrEmpty(val), "SortOrder", val)
        End Get
        Set(value As String)
            ViewState("SC") = value
        End Set
    End Property

    Private Property SortDir As String
        Get
            Dim val = TryCast(ViewState("SD"), String)
            Return If(String.IsNullOrEmpty(val), "ASC", val)
        End Get
        Set(value As String)
            ViewState("SD") = value
        End Set
    End Property
    ' ────────────────────────────────────────────────────────
    ' PAGE LOAD
    ' ────────────────────────────────────────────────────────
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            SortColumn = "SortOrder"
            SortDir    = "ASC"
            BindGrid()
        End If
    End Sub

    ' ────────────────────────────────────────────────────────
    ' GRID EVENTS
    ' ────────────────────────────────────────────────────────
    Protected Sub chkIncludeInactive_CheckedChanged(
            sender As Object, e As EventArgs) _
            Handles chkIncludeInactive.CheckedChanged
        BindGrid()
    End Sub

    Protected Sub gvER_Sorting(sender As Object,
            e As GridViewSortEventArgs) Handles gvER.Sorting
        SortDir    = If(SortColumn = e.SortExpression _
                        AndAlso SortDir = "ASC", "DESC", "ASC")
        SortColumn = e.SortExpression
        BindGrid()
    End Sub

    Protected Sub gvER_RowCommand(sender As Object,
            e As GridViewCommandEventArgs) Handles gvER.RowCommand
        Select Case e.CommandName
            Case "EditRow"
                LoadForEdit(Convert.ToInt32(e.CommandArgument))
                lblError.Visible   = False
                litModalTitle.Text = "Modifier le motif"
                ShowModal()
            Case "ToggleActive"
                ToggleActive(Convert.ToInt32(e.CommandArgument))
                BindGrid()
        End Select
    End Sub

    ' ────────────────────────────────────────────────────────
    ' NEW BUTTON
    ' ────────────────────────────────────────────────────────
    Protected Sub btnNew_Click(sender As Object,
            e As EventArgs) Handles btnNew.Click
        hfExtensionReasonId.Value   = ""
        txtCode.Text                = ""
        txtName.Text                = ""
        txtDescription.Text         = ""
        txtSortOrder.Text           = "99"
        ddlBadgeColor.SelectedValue = "secondary"
        chkRequiresDocRef.Checked   = False
        chkRequiresApprover.Checked = False
        lblError.Visible            = False
        litModalTitle.Text          = "Nouveau motif de prolongation"
        ShowModal()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' SAVE
    ' ────────────────────────────────────────────────────────
    Protected Sub btnSave_Click(sender As Object,
            e As EventArgs) Handles btnSave.Click
        lblError.Visible = False

        Dim code        As String  = txtCode.Text.Trim().ToUpperInvariant()
        Dim name        As String  = txtName.Text.Trim()
        Dim desc        As String  = txtDescription.Text.Trim()
        Dim color       As String  = ddlBadgeColor.SelectedValue
        Dim reqDoc      As Boolean = chkRequiresDocRef.Checked
        Dim reqApprover As Boolean = chkRequiresApprover.Checked
        Dim order       As Byte    = 99
        Byte.TryParse(txtSortOrder.Text.Trim(), order)

        If code = "" Then
            ShowError("Le code est obligatoire.")
            ShowModal() : Return
        End If
        If name = "" Then
            ShowError("La d&eacute;signation est obligatoire.")
            ShowModal() : Return
        End If

        Try
            Using cn As New SqlConnection(ConnStr)
                ' ExtensionReason has no SP for Save in our schema —
                ' direct INSERT/UPDATE (table is admin-only, low volume)
                Dim idVal As Integer = 0
                Integer.TryParse(hfExtensionReasonId.Value, idVal)

                If idVal = 0 Then
                    ' INSERT
                    Using cmd As New SqlCommand(
                        "INSERT INTO mro2.ExtensionReason " &
                        "(Code, Name, Description, RequiresDocRef, " &
                        " RequiresApprover, BadgeColor, SortOrder) " &
                        "VALUES (@Code,@Name,@Desc,@ReqDoc," &
                        "        @ReqApprover,@Color,@Sort)", cn)
                        cmd.Parameters.AddWithValue("@Code",        code)
                        cmd.Parameters.AddWithValue("@Name",        name)
                        cmd.Parameters.AddWithValue("@Desc",
                            If(desc = "", CType(DBNull.Value, Object), desc))
                        cmd.Parameters.AddWithValue("@ReqDoc",      If(reqDoc, 1, 0))
                        cmd.Parameters.AddWithValue("@ReqApprover", If(reqApprover, 1, 0))
                        cmd.Parameters.AddWithValue("@Color",       color)
                        cmd.Parameters.AddWithValue("@Sort",        order)
                        cn.Open()
                        cmd.ExecuteNonQuery()
                    End Using
                Else
                    ' UPDATE
                    Using cmd As New SqlCommand(
                        "UPDATE mro2.ExtensionReason SET " &
                        "Code=@Code, Name=@Name, Description=@Desc, " &
                        "RequiresDocRef=@ReqDoc, RequiresApprover=@ReqApprover, " &
                        "BadgeColor=@Color, SortOrder=@Sort " &
                        "WHERE ExtensionReasonId=@Id", cn)
                        cmd.Parameters.AddWithValue("@Code",        code)
                        cmd.Parameters.AddWithValue("@Name",        name)
                        cmd.Parameters.AddWithValue("@Desc",
                            If(desc = "", CType(DBNull.Value, Object), desc))
                        cmd.Parameters.AddWithValue("@ReqDoc",      If(reqDoc, 1, 0))
                        cmd.Parameters.AddWithValue("@ReqApprover", If(reqApprover, 1, 0))
                        cmd.Parameters.AddWithValue("@Color",       color)
                        cmd.Parameters.AddWithValue("@Sort",        order)
                        cmd.Parameters.AddWithValue("@Id",          idVal)
                        cn.Open()
                        cmd.ExecuteNonQuery()
                    End Using
                End If
            End Using

            BindGrid()
            HideModal()
            ShowToast("Motif enregistr&eacute;.", "success")

        Catch ex As SqlException When ex.Number = 2627 OrElse ex.Number = 2601
            ShowError("Ce code existe d&eacute;j&agrave;. Choisissez un code diff&eacute;rent.")
            ShowModal()
        Catch ex As Exception
            ShowError(Server.HtmlEncode(ex.Message))
            ShowModal()
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DATA HELPERS
    ' ────────────────────────────────────────────────────────
    Private Sub BindGrid()
        Dim dt As New DataTable()

        Using cn As New SqlConnection(ConnStr)
            Dim sql As String =
                "SELECT ExtensionReasonId, Code, Name, Description, " &
                "RequiresDocRef, RequiresApprover, BadgeColor, " &
                "SortOrder, IsActive " &
                "FROM mro2.ExtensionReason " &
                "WHERE (@IncludeInactive=1 OR IsActive=1) " &
                "ORDER BY SortOrder, Code"

            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@IncludeInactive",
                    If(chkIncludeInactive.Checked, 1, 0))
                cn.Open()
                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using

        If dt.Columns.Contains(SortColumn) Then
            dt.DefaultView.Sort = SortColumn & " " & SortDir
        End If

        litRowCount.Text = dt.Rows.Count.ToString()
        gvER.DataSource  = dt
        gvER.DataBind()
    End Sub

    Private Sub LoadForEdit(ByVal id As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT ExtensionReasonId, Code, Name, Description, " &
                "RequiresDocRef, RequiresApprover, BadgeColor, SortOrder " &
                "FROM mro2.ExtensionReason WHERE ExtensionReasonId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", id)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfExtensionReasonId.Value   = rdr("ExtensionReasonId").ToString()
                        txtCode.Text                = rdr("Code").ToString()
                        txtName.Text                = rdr("Name").ToString()
                        txtDescription.Text         =
                            If(rdr("Description") Is DBNull.Value, "",
                               rdr("Description").ToString())
                        txtSortOrder.Text           = rdr("SortOrder").ToString()
                        ddlBadgeColor.SelectedValue = rdr("BadgeColor").ToString()
                        chkRequiresDocRef.Checked   = Convert.ToBoolean(rdr("RequiresDocRef"))
                        chkRequiresApprover.Checked = Convert.ToBoolean(rdr("RequiresApprover"))
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub ToggleActive(ByVal id As Integer)
        Try
            Using cn As New SqlConnection(ConnStr)
                cn.Open()
                Dim cur As Boolean = True
                Using cmdGet As New SqlCommand(
                    "SELECT IsActive FROM mro2.ExtensionReason " &
                    "WHERE ExtensionReasonId=@Id", cn)
                    cmdGet.Parameters.AddWithValue("@Id", id)
                    Dim o As Object = cmdGet.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                        cur = Convert.ToBoolean(o)
                    End If
                End Using
                Using cmd As New SqlCommand(
                    "UPDATE mro2.ExtensionReason SET IsActive=@IsActive " &
                    "WHERE ExtensionReasonId=@Id", cn)
                    cmd.Parameters.AddWithValue("@IsActive", If(cur, 0, 1))
                    cmd.Parameters.AddWithValue("@Id", id)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            ShowToast("Statut mis &agrave; jour.", "success")
        Catch ex As Exception
            ShowToast(Server.HtmlEncode(ex.Message), "error")
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DISPLAY HELPERS
    ' ────────────────────────────────────────────────────────

    ' Renders a green tick (required) or grey dash (optional)
    Protected Function RequiredBadge(ByVal required As Boolean) As String
        If required Then
            Return "<i class='fas fa-check-circle text-success' " &
                   "title='Obligatoire'></i>"
        Else
            Return "<i class='fas fa-minus-circle text-secondary' " &
                   "title='Optionnel'></i>"
        End If
    End Function

    ' ────────────────────────────────────────────────────────
    ' UI HELPERS
    ' ────────────────────────────────────────────────────────
    Private Sub ShowModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "showER_" & Guid.NewGuid().ToString("N"),
            "$('#erModal').modal('show');", True)
    End Sub

    Private Sub HideModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "hideER_" & Guid.NewGuid().ToString("N"),
            "$('#erModal').modal('hide');", True)
    End Sub

    Private Sub ShowError(ByVal msg As String)
        lblError.Text    = msg
        lblError.Visible = True
    End Sub

    Private Sub ShowToast(ByVal message As String, ByVal kind As String)
        Dim ser As New JavaScriptSerializer()
        Dim js As String = "if(window.toastr){toastr." &
                           If(kind, "info").ToLowerInvariant() & "(" &
                           ser.Serialize(If(message, "")) & ");}"
        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "toast_" & Guid.NewGuid().ToString("N"), js, True)
    End Sub

End Class
