Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

' ============================================================
' MRO2/Setup/Counters/CounterBasisList.aspx.vb
' Lookup table: mro2.CounterBasis
' 4 seeded values:
'   ABSOLUTE     — lifetime total, never resets
'   SINCE_INSTALL— resets on each installation
'   SINCE_NEW    — resets on return-to-new overhaul
'   SINCE_OH     — resets on each overhaul shop visit
' ============================================================
Partial Class MRO2_Setup_Counters_CounterBasisList
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

    Protected Sub gvCB_Sorting(sender As Object,
            e As GridViewSortEventArgs) Handles gvCB.Sorting
        SortDir    = If(SortColumn = e.SortExpression _
                        AndAlso SortDir = "ASC", "DESC", "ASC")
        SortColumn = e.SortExpression
        BindGrid()
    End Sub

    Protected Sub gvCB_RowCommand(sender As Object,
            e As GridViewCommandEventArgs) Handles gvCB.RowCommand
        Select Case e.CommandName
            Case "EditRow"
                LoadForEdit(Convert.ToInt32(e.CommandArgument))
                lblError.Visible   = False
                litModalTitle.Text = "Modifier la base de comptage"
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
        hfCounterBasisId.Value  = ""
        txtCode.Text            = ""
        txtName.Text            = ""
        txtDescription.Text     = ""
        txtSortOrder.Text       = "99"
        lblError.Visible        = False
        litModalTitle.Text      = "Nouvelle base de comptage"
        ShowModal()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' SAVE
    ' ────────────────────────────────────────────────────────
    Protected Sub btnSave_Click(sender As Object,
            e As EventArgs) Handles btnSave.Click
        lblError.Visible = False

        Dim code  As String = txtCode.Text.Trim().ToUpperInvariant()
        Dim name  As String = txtName.Text.Trim()
        Dim desc  As String = txtDescription.Text.Trim()
        Dim order As Byte   = 99
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
                Using cmd As New SqlCommand("mro2.usp_CounterBasis_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure

                    Dim idObj As Object = DBNull.Value
                    If hfCounterBasisId.Value <> "" Then
                        idObj = CInt(hfCounterBasisId.Value)
                    End If

                    cmd.Parameters.AddWithValue("@CounterBasisId", idObj)
                    cmd.Parameters.AddWithValue("@Code",           code)
                    cmd.Parameters.AddWithValue("@Name",           name)
                    cmd.Parameters.AddWithValue("@Description",
                        If(desc = "", CType(DBNull.Value, Object), desc))
                    cmd.Parameters.AddWithValue("@SortOrder",      order)

                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using

            BindGrid()
            HideModal()
            ShowToast("Base de comptage enregistr&eacute;e.", "success")

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
            Using cmd As New SqlCommand("mro2.usp_CounterBasis_List", cn)
                cmd.CommandType = CommandType.StoredProcedure
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
        gvCB.DataSource  = dt
        gvCB.DataBind()
    End Sub

    Private Sub LoadForEdit(ByVal id As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT CounterBasisId, Code, Name, Description, SortOrder " &
                "FROM mro2.CounterBasis WHERE CounterBasisId = @Id", cn)
                cmd.Parameters.AddWithValue("@Id", id)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfCounterBasisId.Value  = rdr("CounterBasisId").ToString()
                        txtCode.Text            = rdr("Code").ToString()
                        txtName.Text            = rdr("Name").ToString()
                        txtDescription.Text     =
                            If(rdr("Description") Is DBNull.Value, "",
                               rdr("Description").ToString())
                        txtSortOrder.Text       = rdr("SortOrder").ToString()
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
                    "SELECT IsActive FROM mro2.CounterBasis " &
                    "WHERE CounterBasisId=@Id", cn)
                    cmdGet.Parameters.AddWithValue("@Id", id)
                    Dim o As Object = cmdGet.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                        cur = Convert.ToBoolean(o)
                    End If
                End Using
                Using cmd As New SqlCommand("mro2.usp_CounterBasis_SetActive", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@CounterBasisId", id)
                    cmd.Parameters.AddWithValue("@IsActive", If(cur, 0, 1))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            ShowToast("Statut mis &agrave; jour.", "success")
        Catch ex As Exception
            ShowToast(Server.HtmlEncode(ex.Message), "error")
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DISPLAY HELPERS (called from ASPX databind)
    ' ────────────────────────────────────────────────────────

    ' Truncates description for grid display
    Protected Function TruncateDesc(ByVal o As Object) As String
        If o Is Nothing OrElse o Is DBNull.Value Then Return ""
        Dim s As String = o.ToString().Trim()
        If s.Length <= 90 Then Return Server.HtmlEncode(s)
        Return Server.HtmlEncode(s.Substring(0, 87)) & "..."
    End Function

    ' Reset badge: explains when the counter resets
    ' shown in the grid so users understand at a glance
    Protected Function ResetBadge(ByVal code As String) As String
        Select Case code.ToUpperInvariant()
            Case "ABSOLUTE"
                Return "<span class='badge badge-dark' " &
                       "title='Ne se r&eacute;initialise jamais'>" &
                       "Jamais</span>"
            Case "SINCE_INSTALL"
                Return "<span class='badge badge-info' " &
                       "title='R&eacute;initialis&eacute; &agrave; chaque installation'>" &
                       "Installation</span>"
            Case "SINCE_NEW"
                Return "<span class='badge badge-danger' " &
                       "title='R&eacute;initialis&eacute; au retour &agrave; l&apos;&eacute;tat neuf'>" &
                       "R&eacute;vision g&eacute;n&eacute;rale</span>"
            Case "SINCE_OH"
                Return "<span class='badge badge-warning text-dark' " &
                       "title='R&eacute;initialis&eacute; &agrave; chaque visite atelier'>" &
                       "Overhaul</span>"
            Case Else
                Return "<span class='badge badge-secondary'>" &
                       Server.HtmlEncode(code) & "</span>"
        End Select
    End Function

    ' ────────────────────────────────────────────────────────
    ' UI HELPERS
    ' ────────────────────────────────────────────────────────
    Private Sub ShowModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "showCB_" & Guid.NewGuid().ToString("N"),
            "$('#cbModal').modal('show');", True)
    End Sub

    Private Sub HideModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "hideCB_" & Guid.NewGuid().ToString("N"),
            "$('#cbModal').modal('hide');", True)
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
