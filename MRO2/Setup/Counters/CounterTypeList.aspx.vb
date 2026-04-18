Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

Partial Class MRO2_Setup_Counters_CounterTypeList
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    Private Property SortColumn As String
        Get
            Dim val = TryCast(ViewState("SortCol"), String)
            Return If(String.IsNullOrEmpty(val), "SortOrder", val)
        End Get
        Set(value As String)
            ViewState("SortCol") = value
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

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            SortColumn = "SortOrder" : SortDir = "ASC"
            BindGrid()
        End If
    End Sub

    Protected Sub chkIncludeInactive_CheckedChanged(sender As Object, e As EventArgs) _
        Handles chkIncludeInactive.CheckedChanged
        BindGrid()
    End Sub

    Protected Sub gvCT_Sorting(sender As Object, e As GridViewSortEventArgs) _
        Handles gvCT.Sorting
        SortDir    = If(SortColumn = e.SortExpression AndAlso SortDir = "ASC", "DESC", "ASC")
        SortColumn = e.SortExpression
        BindGrid()
    End Sub

    Protected Sub gvCT_RowCommand(sender As Object, e As GridViewCommandEventArgs) _
        Handles gvCT.RowCommand
        Select Case e.CommandName
            Case "EditRow"
                LoadForEdit(Convert.ToInt32(e.CommandArgument))
                lblError.Visible   = False
                litModalTitle.Text = "Modifier le type"
                ShowModal()
            Case "ToggleActive"
                ToggleActive(Convert.ToInt32(e.CommandArgument))
                BindGrid()
        End Select
    End Sub

    Protected Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        hfCounterTypeId.Value        = ""
        txtCode.Text                 = ""
        txtName.Text                 = ""
        txtDisplayUnit.Text          = ""
        txtSortOrder.Text            = "99"
        ddlUnitStorage.SelectedValue = "COUNT"
        lblError.Visible             = False
        litModalTitle.Text           = "Nouveau type de compteur"
        ShowModal()
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        lblError.Visible = False
        Dim code        As String = txtCode.Text.Trim().ToUpperInvariant()
        Dim name        As String = txtName.Text.Trim()
        Dim displayUnit As String = txtDisplayUnit.Text.Trim()
        Dim unitStorage As String = ddlUnitStorage.SelectedValue
        Dim order       As Byte   = 99
        Byte.TryParse(txtSortOrder.Text.Trim(), order)

        If code = "" Then
            ShowError("Le code est obligatoire.") : ShowModal() : Return
        End If
        If name = "" Then
            ShowError("La d&eacute;signation est obligatoire.") : ShowModal() : Return
        End If
        If displayUnit = "" Then
            ShowError("L&apos;unit&eacute; d&apos;affichage est obligatoire.") : ShowModal() : Return
        End If

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand("mro2.usp_CounterType_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    Dim idObj As Object = DBNull.Value
                    If hfCounterTypeId.Value <> "" Then
                        idObj = Convert.ToInt32(hfCounterTypeId.Value)
                    End If
                    cmd.Parameters.AddWithValue("@CounterTypeId", idObj)
                    cmd.Parameters.AddWithValue("@Code",          code)
                    cmd.Parameters.AddWithValue("@Name",          name)
                    cmd.Parameters.AddWithValue("@UnitStorage",   unitStorage)
                    cmd.Parameters.AddWithValue("@DisplayUnit",   displayUnit)
                    cmd.Parameters.AddWithValue("@SortOrder",     order)
                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using
            BindGrid()
            HideModal()
            ShowToast("Type enregistr&eacute;.", "success")
        Catch ex As SqlException When ex.Number = 2627 OrElse ex.Number = 2601
            ShowError("Ce code existe d&eacute;j&agrave;.") : ShowModal()
        Catch ex As Exception
            ShowError(Server.HtmlEncode(ex.Message)) : ShowModal()
        End Try
    End Sub

    Private Sub BindGrid()
        Dim dt As New DataTable()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_CounterType_List", cn)
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
        gvCT.DataSource  = dt
        gvCT.DataBind()
    End Sub

    Private Sub LoadForEdit(ByVal id As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT CounterTypeId,Code,Name,UnitStorage,DisplayUnit,SortOrder " &
                "FROM mro2.CounterType WHERE CounterTypeId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", id)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfCounterTypeId.Value        = rdr("CounterTypeId").ToString()
                        txtCode.Text                 = rdr("Code").ToString()
                        txtName.Text                 = rdr("Name").ToString()
                        txtDisplayUnit.Text          = rdr("DisplayUnit").ToString()
                        txtSortOrder.Text            = rdr("SortOrder").ToString()
                        ddlUnitStorage.SelectedValue = rdr("UnitStorage").ToString()
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
                    "SELECT IsActive FROM mro2.CounterType WHERE CounterTypeId=@Id", cn)
                    cmdGet.Parameters.AddWithValue("@Id", id)
                    Dim o As Object = cmdGet.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then cur = Convert.ToBoolean(o)
                End Using
                Using cmd As New SqlCommand("mro2.usp_CounterType_SetActive", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@CounterTypeId", id)
                    cmd.Parameters.AddWithValue("@IsActive", If(cur, 0, 1))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            ShowToast("Statut mis &agrave; jour.", "success")
        Catch ex As Exception
            ShowToast(Server.HtmlEncode(ex.Message), "error")
        End Try
    End Sub

    Protected Function StorageBadge(ByVal storage As String) As String
        Select Case storage.ToUpperInvariant()
            Case "MINUTES"
                Return "<span class='badge badge-info'>MINUTES</span>"
            Case "COUNT"
                Return "<span class='badge badge-secondary'>COUNT</span>"
            Case Else
                Return "<span class='badge badge-light'>" & Server.HtmlEncode(storage) & "</span>"
        End Select
    End Function

    Private Sub ShowModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "showCT_" & Guid.NewGuid().ToString("N"),
            "$('#ctModal').modal('show');", True)
    End Sub
    Private Sub HideModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "hideCT_" & Guid.NewGuid().ToString("N"),
            "$('#ctModal').modal('hide');", True)
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
