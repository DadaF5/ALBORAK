Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

Partial Class MRO2_Setup_Components_PartNumberList
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    Private Property SortColumn As String
        Get
            Return If(TryCast(ViewState("SortColumn"), String), "PN")
        End Get
        Set(value As String)
            ViewState("SortColumn") = value
        End Set
    End Property

    Private Property SortDir As String
        Get
            Return If(TryCast(ViewState("SortDir"), String), "ASC")
        End Get
        Set(value As String)
            ViewState("SortDir") = value
        End Set
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            SortColumn = "PN"
            SortDir = "ASC"
            LoadAcMainGroup()
            LoadATA()
            LoadUOM()
            BindGrid()

        End If
    End Sub

    Private Sub LoadATA()
        ddlATA.Items.Clear()
        ddlATA.Items.Add(New ListItem("(none)", ""))

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_GetAtaList", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlATA.Items.Add(New ListItem(rdr("ATACode").ToString(), rdr("ATAId").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub LoadUOM()
        ddlUOM.Items.Clear()
        ddlUOM.Items.Add(New ListItem("-- select --", ""))

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("SELECT UnitOfMeasureId, Code, Name " & _
                                         "FROM mro2.UnitOfMeasure " & _
                                         "WHERE IsActive = 1 " & _
                                         "ORDER BY Code", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim code As String = rdr("Code").ToString().Trim()
                        Dim name As String = rdr("Name").ToString().Trim()
                        ddlUOM.Items.Add(New ListItem(code & " (" & name & ")", rdr("UnitOfMeasureId").ToString()))
                    End While
                End Using
            End Using
        End Using
        SelectUomByCode("EA")
    End Sub

    Protected Sub chkIncludeInactive_CheckedChanged(sender As Object, e As EventArgs) Handles chkIncludeInactive.CheckedChanged
        gvPN.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        gvPN.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        txtSearch.Text = ""
        gvPN.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub gvPN_PageIndexChanging(sender As Object, e As GridViewPageEventArgs) Handles gvPN.PageIndexChanging
        gvPN.PageIndex = e.NewPageIndex
        BindGrid()
    End Sub

    Protected Sub gvPN_Sorting(sender As Object, e As GridViewSortEventArgs) Handles gvPN.Sorting
        If SortColumn = e.SortExpression Then
            SortDir = If(SortDir = "ASC", "DESC", "ASC")
        Else
            SortColumn = e.SortExpression
            SortDir = "ASC"
        End If

        gvPN.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        hfPartNumberId.Value = ""
        txtPN.Text = ""
        txtNomenclature.Text = ""
        ddlATA.SelectedIndex = 0
        ddlIsSerialized.SelectedValue = "1"
        SelectUomByCode("EA") ' user must choose (or you can default to EA)
        lblError.Visible = False
        litModalTitle.Text = "New Part Number"
        ShowModal()
    End Sub

    Protected Sub gvPN_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvPN.RowCommand
        If e.CommandName = "EditRow" Then
            Dim id As Integer = Convert.ToInt32(e.CommandArgument)
            LoadRowForEdit(id)
            lblError.Visible = False
            litModalTitle.Text = "Edit Part Number"
            ShowModal()
            Return
        End If

        If e.CommandName = "ToggleActive" Then
            ToggleActive(Convert.ToInt32(e.CommandArgument))
            BindGrid()
            Return
        End If
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        lblError.Visible = False

        Dim pn As String = txtPN.Text.Trim().ToUpperInvariant()
        Dim nom As String = txtNomenclature.Text.Trim()

        Dim ataIdObj As Object = DBNull.Value
        If ddlATA.SelectedValue.Trim() <> "" Then ataIdObj = Convert.ToInt32(ddlATA.SelectedValue)

        Dim isSer As Boolean = (ddlIsSerialized.SelectedValue = "1")

        Dim uomId As Integer = 0
        Integer.TryParse(ddlUOM.SelectedValue, uomId)

        If pn = "" Then
            lblError.Text = "PN is required."
            lblError.Visible = True
            ShowModal()
            Exit Sub
        End If

        If uomId = 0 Then
            lblError.Text = "UOM is required."
            lblError.Visible = True
            ShowModal()
            Exit Sub
        End If
        Dim acMainGroupId As Integer = 0
        Integer.TryParse(ddlAcMainGroup.SelectedValue, acMainGroupId)
        If isSer AndAlso acMainGroupId = 0 Then
            lblError.Text = "AC Main Group is required for serialized part numbers."
            lblError.Visible = True
            ShowModal()
            Exit Sub
        End If

        txtPN.Text = pn

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand("mro2.usp_PartNumber_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure

                    Dim idObj As Object = DBNull.Value
                    If hfPartNumberId.Value.Trim() <> "" Then idObj = Convert.ToInt32(hfPartNumberId.Value)

                    cmd.Parameters.AddWithValue("@PartNumberId", idObj)
                    cmd.Parameters.AddWithValue("@PN", pn)
                    cmd.Parameters.AddWithValue("@Nomenclature", If(nom = "", CType(DBNull.Value, Object), nom))
                    cmd.Parameters.AddWithValue("@ATAId", ataIdObj)
                    cmd.Parameters.AddWithValue("@IsSerialized", If(isSer, 1, 0))
                    cmd.Parameters.AddWithValue("@UnitOfMeasureId", uomId)
                    cmd.Parameters.AddWithValue("@AcMainGroupID", If(acMainGroupId = 0, CType(DBNull.Value, Object), acMainGroupId))

                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using

            BindGrid()
            HideModal()
            ShowToast("Saved successfully.", "success")

        Catch ex As Exception
            lblError.Text = FriendlyDbMessage(ex)
            lblError.Visible = True
            ShowModal()
        End Try
    End Sub

    Private Sub BindGrid()
        Dim dt As New DataTable()

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_PartNumber_List", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@IncludeInactive", If(chkIncludeInactive.Checked, 1, 0))
                cmd.Parameters.AddWithValue("@Search", If(txtSearch.Text.Trim() = "", CType(DBNull.Value, Object), txtSearch.Text.Trim()))
                cmd.Parameters.AddWithValue("@SortColumn", SortColumn)
                cmd.Parameters.AddWithValue("@SortDir", SortDir)

                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using

        litRowCount.Text = dt.Rows.Count.ToString()
        gvPN.DataSource = dt
        gvPN.DataBind()
    End Sub

   Private Sub LoadRowForEdit(ByVal id As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand("SELECT PartNumberId, PN, Nomenclature, ATAId, IsSerialized, UnitOfMeasureId, AcMainGroupID " & _
                                         "FROM mro2.PartNumber " & _
                                         "WHERE PartNumberId = @Id", cn)
                cmd.Parameters.AddWithValue("@Id", id)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfPartNumberId.Value = rdr("PartNumberId").ToString()
                        txtPN.Text = rdr("PN").ToString()
                        txtNomenclature.Text = If(rdr("Nomenclature") Is DBNull.Value, "", rdr("Nomenclature").ToString())
                        ddlATA.SelectedValue = If(rdr("ATAId") Is DBNull.Value, "", rdr("ATAId").ToString())
                        ddlIsSerialized.SelectedValue = If(Convert.ToBoolean(rdr("IsSerialized")), "1", "0")

                        Dim uomId As String = If(rdr("UnitOfMeasureId") Is DBNull.Value, "", rdr("UnitOfMeasureId").ToString())
                        If ddlUOM.Items.FindByValue(uomId) IsNot Nothing Then
                            ddlUOM.SelectedValue = uomId
                        Else
                            ddlUOM.SelectedValue = ""
                        End If
                        Dim mgId As String = If(rdr("AcMainGroupID") Is DBNull.Value, "", rdr("AcMainGroupID").ToString())
                        If ddlAcMainGroup.Items.FindByValue(mgId) IsNot Nothing Then
                            ddlAcMainGroup.SelectedValue = mgId
                        Else
                            ddlAcMainGroup.SelectedIndex = 0
                        End If
                    End If
                End Using
            End Using
        End Using
       
        SelectUomByCode("EA")
    End Sub

    Private Sub ToggleActive(ByVal id As Integer)
        Try
            Dim currentActive As Boolean = True

            Using cn As New SqlConnection(ConnStr)
                cn.Open()

                Using cmdGet As New SqlCommand("SELECT IsActive FROM mro2.PartNumber WHERE PartNumberId=@Id", cn)
                    cmdGet.Parameters.AddWithValue("@Id", id)
                    Dim o As Object = cmdGet.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then currentActive = Convert.ToBoolean(o)
                End Using

                Using cmd As New SqlCommand("mro2.usp_PartNumber_SetActive", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@PartNumberId", id)
                    cmd.Parameters.AddWithValue("@IsActive", If(currentActive, 0, 1))
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            ShowToast("Status updated.", "success")

        Catch ex As Exception
            ShowToast(FriendlyDbMessage(ex), "error")
        End Try
    End Sub

    Private Sub ShowModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "showPNModal_" & Guid.NewGuid().ToString("N"),
            "$('#pnModal').modal('show');",
            True)
    End Sub

    Private Sub HideModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "hidePNModal_" & Guid.NewGuid().ToString("N"),
            "$('#pnModal').modal('hide');",
            True)
    End Sub

    Private Function FriendlyDbMessage(ByVal ex As Exception) As String
        Dim sqlEx As SqlException = TryCast(ex, SqlException)
        If sqlEx IsNot Nothing Then
            If sqlEx.Number = 2627 OrElse sqlEx.Number = 2601 Then
                Return "This PN already exists."
            End If
        End If
        Return Server.HtmlEncode(ex.Message)
    End Function

    Private Sub ShowToast(ByVal message As String, ByVal kind As String)
        Dim k As String = (If(kind, "info")).ToLowerInvariant()
        If k <> "success" AndAlso k <> "info" AndAlso k <> "warning" AndAlso k <> "error" Then k = "info"

        Dim ser As New JavaScriptSerializer()
        Dim msgJs As String = ser.Serialize(If(message, ""))

        Dim js As String = "if (window.toastr) { toastr." & k & "(" & msgJs & "); }"

        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "toast_" & Guid.NewGuid().ToString("N"),
            js,
            True)
    End Sub

    Private Sub SelectUomByCode(ByVal code As String)
        Dim target As String = (If(code, "")).Trim().ToUpperInvariant()
        If target = "" Then Exit Sub

        For Each it As ListItem In ddlUOM.Items
            ' item text is like "EA (Each)"
            If it.Text IsNot Nothing AndAlso it.Text.Trim().ToUpperInvariant().StartsWith(target & " ") OrElse it.Text.Trim().ToUpperInvariant() = target Then
                ddlUOM.SelectedValue = it.Value
                Exit Sub
            End If
        Next
    End Sub

    Private Sub LoadAcMainGroup()
        ddlAcMainGroup.Items.Clear()
        ddlAcMainGroup.Items.Add(New ListItem("-- select --", ""))

        Dim sql As String = _
            "SELECT AcMainGroupID, AcMainGroup " & _
            "FROM dbo.tblAcMainGroup WHERE Active = 1 " & _
            "ORDER BY AcMainGroup"

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(sql, cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlAcMainGroup.Items.Add(New ListItem( _
                            rdr("AcMainGroup").ToString(), _
                            rdr("AcMainGroupID").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub
End Class