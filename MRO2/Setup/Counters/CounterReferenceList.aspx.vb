Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

' ============================================================
' MRO2/Setup/Counters/CounterReferenceList.aspx.vb
' Lookup table: mro2.CounterReference (global, no FK to type)
' ============================================================
Partial Class MRO2_Setup_Counters_CounterReferenceList
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString
    Private Property SortColumn As String
        Get
            Dim val = TryCast(ViewState("SortColumn"), String)
            Return If(String.IsNullOrEmpty(val), "RefCategory", val)
        End Get
        Set(value As String)
            ViewState("SortColumn") = value
        End Set
    End Property

    Private Property SortDir As String
        Get
            Dim val = TryCast(ViewState("SortDir"), String)
            Return If(String.IsNullOrEmpty(val), "ASC", val)
        End Get
        Set(value As String)
            ViewState("SortDir") = value
        End Set
    End Property

    ' ────────────────────────────────────────────────────────
    ' PAGE LOAD
    ' ────────────────────────────────────────────────────────
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            SortColumn = "RefCategory"
            SortDir    = "ASC"
            BindGrid()
        End If
    End Sub

    ' ────────────────────────────────────────────────────────
    ' FILTER EVENTS
    ' ────────────────────────────────────────────────────────
    Protected Sub ddlFilterCategory_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles ddlFilterCategory.SelectedIndexChanged
        BindGrid()
    End Sub

    Protected Sub chkIncludeInactive_CheckedChanged(sender As Object, e As EventArgs) _
        Handles chkIncludeInactive.CheckedChanged
        BindGrid()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' GRID EVENTS
    ' ────────────────────────────────────────────────────────
    Protected Sub gvCR_Sorting(sender As Object, e As GridViewSortEventArgs) _
        Handles gvCR.Sorting
        SortDir    = If(SortColumn = e.SortExpression AndAlso SortDir = "ASC", "DESC", "ASC")
        SortColumn = e.SortExpression
        BindGrid()
    End Sub

    Protected Sub gvCR_RowCommand(sender As Object, e As GridViewCommandEventArgs) _
        Handles gvCR.RowCommand

        Select Case e.CommandName
            Case "EditRow"
                Dim id As Integer = Convert.ToInt32(e.CommandArgument)
                LoadForEdit(id)
                lblError.Visible   = False
                litModalTitle.Text = "Modifier la r&eacute;f&eacute;rence"
                ShowModal()

            Case "ToggleActive"
                ToggleActive(Convert.ToInt32(e.CommandArgument))
                BindGrid()
        End Select
    End Sub

    ' ────────────────────────────────────────────────────────
    ' NEW BUTTON
    ' ────────────────────────────────────────────────────────
    Protected Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        hfCounterReferenceId.Value    = ""
        txtCode.Text                  = ""
        txtName.Text                  = ""
        txtSortOrder.Text             = "99"
        ' Pre-select category from filter if active
        If ddlFilterCategory.SelectedValue <> "" Then
            ddlModalCategory.SelectedValue = ddlFilterCategory.SelectedValue
        Else
            ddlModalCategory.SelectedValue = "EVENT"
        End If
        lblError.Visible              = False
        litModalTitle.Text            = "Nouvelle r&eacute;f&eacute;rence"
        ShowModal()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' SAVE
    ' ────────────────────────────────────────────────────────
    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        lblError.Visible = False

        Dim code     As String = txtCode.Text.Trim().ToUpperInvariant()
        Dim name     As String = txtName.Text.Trim()
        Dim category As String = ddlModalCategory.SelectedValue
        Dim order    As Byte   = 99
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
                Using cmd As New SqlCommand("mro2.usp_CounterReference_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure

                    Dim idObj As Object = DBNull.Value
                    If hfCounterReferenceId.Value.Trim() <> "" Then
                        idObj = Convert.ToInt32(hfCounterReferenceId.Value)
                    End If

                    cmd.Parameters.AddWithValue("@CounterReferenceId", idObj)
                    cmd.Parameters.AddWithValue("@Code",               code)
                    cmd.Parameters.AddWithValue("@Name",               name)
                    cmd.Parameters.AddWithValue("@RefCategory",        category)
                    cmd.Parameters.AddWithValue("@SortOrder",          order)

                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using

            BindGrid()
            HideModal()
            ShowToast("R&eacute;f&eacute;rence enregistr&eacute;e.", "success")

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
            Using cmd As New SqlCommand("mro2.usp_CounterReference_List", cn)
                cmd.CommandType = CommandType.StoredProcedure

                Dim cat As Object = DBNull.Value
                If ddlFilterCategory.SelectedValue <> "" Then
                    cat = ddlFilterCategory.SelectedValue
                End If
                cmd.Parameters.AddWithValue("@RefCategory",     cat)
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
        gvCR.DataSource  = dt
        gvCR.DataBind()
    End Sub

    Private Sub LoadForEdit(ByVal id As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT CounterReferenceId, Code, Name, RefCategory, SortOrder " &
                "FROM mro2.CounterReference WHERE CounterReferenceId = @Id", cn)
                cmd.Parameters.AddWithValue("@Id", id)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfCounterReferenceId.Value    = rdr("CounterReferenceId").ToString()
                        txtCode.Text                  = rdr("Code").ToString()
                        txtName.Text                  = rdr("Name").ToString()
                        txtSortOrder.Text             = rdr("SortOrder").ToString()
                        ddlModalCategory.SelectedValue = rdr("RefCategory").ToString()
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub ToggleActive(ByVal id As Integer)
        Try
            Using cn As New SqlConnection(ConnStr)
                cn.Open()
                Dim current As Boolean = True
                Using cmdGet As New SqlCommand(
                    "SELECT IsActive FROM mro2.CounterReference WHERE CounterReferenceId=@Id", cn)
                    cmdGet.Parameters.AddWithValue("@Id", id)
                    Dim o As Object = cmdGet.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                        current = Convert.ToBoolean(o)
                    End If
                End Using
                Using cmd As New SqlCommand("mro2.usp_CounterReference_SetActive", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@CounterReferenceId", id)
                    cmd.Parameters.AddWithValue("@IsActive", If(current, 0, 1))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            ShowToast("Statut mis &agrave; jour.", "success")
        Catch ex As Exception
            ShowToast(Server.HtmlEncode(ex.Message), "error")
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' PROTECTED HELPER — called from ASPX databind
    ' ────────────────────────────────────────────────────────
    Protected Function CategoryBadge(ByVal cat As String) As String
        Select Case cat.ToUpperInvariant()
            Case "EVENT"
                Return "<span class='badge badge-info'>&Eacute;v&eacute;nement</span>"
            Case "DOCUMENT"
                Return "<span class='badge badge-secondary'>Document</span>"
            Case Else
                Return "<span class='badge badge-light'>" &
                       Server.HtmlEncode(cat) & "</span>"
        End Select
    End Function

    ' ────────────────────────────────────────────────────────
    ' UI HELPERS
    ' ────────────────────────────────────────────────────────
    Private Sub ShowModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "showCRModal_" & Guid.NewGuid().ToString("N"),
            "$('#crModal').modal('show');", True)
    End Sub

    Private Sub HideModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "hideCRModal_" & Guid.NewGuid().ToString("N"),
            "$('#crModal').modal('hide');", True)
    End Sub

    Private Sub ShowError(ByVal msg As String)
        lblError.Text    = msg
        lblError.Visible = True
    End Sub

    Private Sub ShowToast(ByVal message As String, ByVal kind As String)
        Dim k   As String = If(kind, "info").ToLowerInvariant()
        Dim ser As New JavaScriptSerializer()
        Dim js  As String = "if(window.toastr){toastr." & k & "(" &
                            ser.Serialize(If(message, "")) & ");}"
        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "toast_" & Guid.NewGuid().ToString("N"), js, True)
    End Sub

End Class
