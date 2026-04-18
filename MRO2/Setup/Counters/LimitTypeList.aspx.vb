Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization
Imports System.Text

' ============================================================
' MRO2/Setup/Counters/LimitTypeList.aspx.vb
' Manages mro2.LimitType.
' Grid shows valid ComputationReferences per LimitType inline
' (rendered from LimitTypeReferenceMap — read-only on this page,
'  managed via Setup/Default.aspx map management panel).
' ============================================================
Partial Class MRO2_Setup_Counters_LimitTypeList
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    ' Cache map data for the current request (one DB call,
    ' used repeatedly by RenderMapBadges during DataBind)
    Private _mapCache As DataTable = Nothing

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
            SortColumn = "SortOrder" : SortDir = "ASC"
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

    Protected Sub gvLT_Sorting(sender As Object,
            e As GridViewSortEventArgs) Handles gvLT.Sorting
        SortDir    = If(SortColumn = e.SortExpression _
                        AndAlso SortDir = "ASC", "DESC", "ASC")
        SortColumn = e.SortExpression
        BindGrid()
    End Sub

    Protected Sub gvLT_RowCommand(sender As Object,
            e As GridViewCommandEventArgs) Handles gvLT.RowCommand
        Select Case e.CommandName
            Case "EditRow"
                LoadForEdit(Convert.ToInt32(e.CommandArgument))
                lblError.Visible   = False
                litModalTitle.Text = "Modifier le type de limite"
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
        hfLimitTypeId.Value          = ""
        txtCode.Text                 = ""
        txtName.Text                 = ""
        txtDescription.Text          = ""
        txtSortOrder.Text            = "99"
        ddlBadgeColor.SelectedValue  = "secondary"
        lblError.Visible             = False
        litModalTitle.Text           = "Nouveau type de limite"
        ShowModal()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' SAVE
    ' ────────────────────────────────────────────────────────
    Protected Sub btnSave_Click(sender As Object,
            e As EventArgs) Handles btnSave.Click
        lblError.Visible = False

        Dim code   As String = txtCode.Text.Trim().ToUpperInvariant()
        Dim name   As String = txtName.Text.Trim()
        Dim desc   As String = txtDescription.Text.Trim()
        Dim color  As String = ddlBadgeColor.SelectedValue
        Dim order  As Byte   = 99
        Byte.TryParse(txtSortOrder.Text.Trim(), order)

        If code = "" Then
            ShowError("Le code est obligatoire.") : ShowModal() : Return
        End If
        If name = "" Then
            ShowError("La d&eacute;signation est obligatoire.") : ShowModal() : Return
        End If

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand("mro2.usp_LimitType_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    Dim idObj As Object = DBNull.Value
                    If hfLimitTypeId.Value <> "" Then
                        idObj = CInt(hfLimitTypeId.Value)
                    End If
                    cmd.Parameters.AddWithValue("@LimitTypeId",  idObj)
                    cmd.Parameters.AddWithValue("@Code",         code)
                    cmd.Parameters.AddWithValue("@Name",         name)
                    cmd.Parameters.AddWithValue("@Description",
                        If(desc = "", CType(DBNull.Value, Object), desc))
                    cmd.Parameters.AddWithValue("@BadgeColor",   color)
                    cmd.Parameters.AddWithValue("@SortOrder",    order)
                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using
            BindGrid()
            HideModal()
            ShowToast("Type de limite enregistr&eacute;.", "success")
        Catch ex As SqlException When ex.Number = 2627 OrElse ex.Number = 2601
            ShowError("Ce code existe d&eacute;j&agrave;.") : ShowModal()
        Catch ex As Exception
            ShowError(Server.HtmlEncode(ex.Message)) : ShowModal()
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DATA HELPERS
    ' ────────────────────────────────────────────────────────
    Private Sub BindGrid()
        ' Load map cache once before DataBind so RenderMapBadges
        ' doesn't fire a separate DB call per row
        LoadMapCache()

        Dim dt As New DataTable()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_LimitType_List", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@IncludeInactive",
                    If(chkIncludeInactive.Checked, 1, 0))
                cn.Open()
                Using da As New SqlDataAdapter(cmd) : da.Fill(dt) : End Using
            End Using
        End Using
        If dt.Columns.Contains(SortColumn) Then
            dt.DefaultView.Sort = SortColumn & " " & SortDir
        End If
        litRowCount.Text = dt.Rows.Count.ToString()
        gvLT.DataSource  = dt
        gvLT.DataBind()
    End Sub

    ' Loads all map rows once — reused per grid row render
    Private Sub LoadMapCache()
        _mapCache = New DataTable()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                    "mro2.usp_LimitTypeReferenceMap_List", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cn.Open()
                Using da As New SqlDataAdapter(cmd)
                    da.Fill(_mapCache)
                End Using
            End Using
        End Using
    End Sub

    Private Sub LoadForEdit(ByVal id As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT LimitTypeId, Code, Name, Description, " &
                "BadgeColor, SortOrder " &
                "FROM mro2.LimitType WHERE LimitTypeId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", id)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfLimitTypeId.Value         = rdr("LimitTypeId").ToString()
                        txtCode.Text                = rdr("Code").ToString()
                        txtName.Text                = rdr("Name").ToString()
                        txtDescription.Text         =
                            If(rdr("Description") Is DBNull.Value, "",
                               rdr("Description").ToString())
                        txtSortOrder.Text           = rdr("SortOrder").ToString()
                        ddlBadgeColor.SelectedValue = rdr("BadgeColor").ToString()
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
                    "SELECT IsActive FROM mro2.LimitType WHERE LimitTypeId=@Id", cn)
                    cmdGet.Parameters.AddWithValue("@Id", id)
                    Dim o As Object = cmdGet.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                        cur = Convert.ToBoolean(o)
                    End If
                End Using
                Using cmd As New SqlCommand("mro2.usp_LimitType_SetActive", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@LimitTypeId", id)
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
    ' DISPLAY HELPER — called per row during DataBind
    ' Renders the ComputationReference badges for this LimitType
    ' from the pre-loaded cache (zero extra DB calls).
    ' Default row gets a ★ star marker.
    ' ────────────────────────────────────────────────────────
    Protected Function RenderMapBadges(ByVal limitTypeId As Integer) As String
        If _mapCache Is Nothing Then Return ""

        Dim rows() As DataRow = _mapCache.Select(
            "LimitTypeId = " & limitTypeId & " AND IsActive = True")

        If rows.Length = 0 Then
            Return "<small class='text-muted'>- aucune -</small>"
        End If

        Dim sb As New StringBuilder()
        For Each row As DataRow In rows
            Dim code      As String  = row("CompRefCode").ToString()
            Dim isDef     As Boolean = Convert.ToBoolean(row("IsDefault"))
            Dim isActive  As Boolean = Convert.ToBoolean(row("IsActive"))
            If Not isActive Then Continue For

            Dim badgeClass As String = If(isDef, "badge-info", "badge-light border")
            Dim star       As String = If(isDef, " &#9733;", "")

            sb.Append("<span class='badge " & badgeClass & " mr-1' " &
                      "title='" & Server.HtmlEncode(row("CompRefName").ToString()) & "'" &
                      "style='font-size:.72rem;'>")
            sb.Append(Server.HtmlEncode(code) & star)
            sb.Append("</span>")
        Next

        sb.Append("<br/><small class='text-muted' style='font-size:.68rem;'>" &
                  "&#9733; = d&eacute;faut</small>")
        Return sb.ToString()
    End Function

    ' ── UI helpers ──────────────────────────────────────────
    Private Sub ShowModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "showLT_" & Guid.NewGuid().ToString("N"),
            "$('#ltModal').modal('show');", True)
    End Sub
    Private Sub HideModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "hideLT_" & Guid.NewGuid().ToString("N"),
            "$('#ltModal').modal('hide');", True)
    End Sub
    Private Sub ShowError(ByVal msg As String)
        lblError.Text = msg : lblError.Visible = True
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
