Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Globalization
Imports System.Web.Script.Serialization

Partial Class MRO2_Setup_Components_SerializedItemList
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    Private Property SortColumn As String
        Get
            Return If(TryCast(ViewState("SortColumn"), String), "SerialNumber")
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

        txtMfgDate.Attributes("type") = "date"
        txtRecvDate.Attributes("type") = "date"

        If Not IsPostBack Then
            SortColumn = "SerialNumber"
            SortDir = "ASC"
            LoadPNSerializedOnly()
            LoadStatus()
            BindGrid()
        End If
    End Sub

    Private Sub LoadPNSerializedOnly()
        ddlPN.Items.Clear()
        ddlPN.Items.Add(New ListItem("-- select PN --", ""))

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("SELECT PartNumberId, PN, Nomenclature " & _
                                         "FROM mro2.PartNumber " & _
                                         "WHERE IsActive = 1 AND IsSerialized = 1 " & _
                                         "ORDER BY PN", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim text As String = rdr("PN").ToString()
                        Dim nom As String = If(rdr("Nomenclature") Is DBNull.Value, "", rdr("Nomenclature").ToString())
                        If nom.Trim() <> "" Then text &= " - " & nom.Trim()
                        ddlPN.Items.Add(New ListItem(text, rdr("PartNumberId").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub LoadStatus()
        ddlStatus.Items.Clear()
        ddlStatus.Items.Add(New ListItem("ACTIVE", "ACTIVE"))
        ddlStatus.Items.Add(New ListItem("SERVICEABLE", "SERVICEABLE"))
        ddlStatus.Items.Add(New ListItem("UNSERVICEABLE", "UNSERVICEABLE"))
        ddlStatus.Items.Add(New ListItem("SCRAP", "SCRAP"))
    End Sub

    Protected Sub chkIncludeInactive_CheckedChanged(sender As Object, e As EventArgs) Handles chkIncludeInactive.CheckedChanged
        gvSI.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        gvSI.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        txtSearch.Text = ""
        gvSI.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub gvSI_PageIndexChanging(sender As Object, e As GridViewPageEventArgs) Handles gvSI.PageIndexChanging
        gvSI.PageIndex = e.NewPageIndex
        BindGrid()
    End Sub

    Protected Sub gvSI_Sorting(sender As Object, e As GridViewSortEventArgs) Handles gvSI.Sorting
        If SortColumn = e.SortExpression Then
            SortDir = If(SortDir = "ASC", "DESC", "ASC")
        Else
            SortColumn = e.SortExpression
            SortDir = "ASC"
        End If

        gvSI.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        hfSerializedItemId.Value = ""
        ddlPN.SelectedIndex = 0
        txtSerial.Text = ""
        ddlStatus.SelectedValue = "ACTIVE"
        txtMfgDate.Text = ""
        txtRecvDate.Text = ""
        txtNotes.Text = ""
        lblError.Visible = False
        litModalTitle.Text = "New Serialized Item"
        LoadPNSerializedOnly()
        ShowModal()
    End Sub

    Protected Sub gvSI_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvSI.RowCommand
        If e.CommandName = "EditRow" Then
            Dim id As Integer = Convert.ToInt32(e.CommandArgument)
            LoadRowForEdit(id)
            lblError.Visible = False
            litModalTitle.Text = "Edit Serialized Item"
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

        Dim pnId As Integer = 0
        Integer.TryParse(ddlPN.SelectedValue, pnId)

        Dim sn As String = txtSerial.Text.Trim().ToUpperInvariant()
        Dim status As String = ddlStatus.SelectedValue.Trim().ToUpperInvariant()
        Dim notes As String = txtNotes.Text.Trim()

        If pnId = 0 Then
            lblError.Text = "PN is required."
            lblError.Visible = True
            ShowModal()
            Exit Sub
        End If

        If sn = "" Then
            lblError.Text = "Serial Number is required."
            lblError.Visible = True
            ShowModal()
            Exit Sub
        End If

        Dim mfgObj As Object = DBNull.Value
        Dim recvObj As Object = DBNull.Value

        Dim d As DateTime
        If txtMfgDate.Text.Trim() <> "" Then
            If DateTime.TryParseExact(txtMfgDate.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then
                mfgObj = d.Date
            Else
                lblError.Text = "Manufactured Date must be YYYY-MM-DD."
                lblError.Visible = True
                ShowModal()
                Exit Sub
            End If
        End If

        If txtRecvDate.Text.Trim() <> "" Then
            If DateTime.TryParseExact(txtRecvDate.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then
                recvObj = d.Date
            Else
                lblError.Text = "Received Date must be YYYY-MM-DD."
                lblError.Visible = True
                ShowModal()
                Exit Sub
            End If
        End If

        txtSerial.Text = sn

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand("mro2.usp_SerializedItem_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure

                    Dim idObj As Object = DBNull.Value
                    If hfSerializedItemId.Value.Trim() <> "" Then idObj = Convert.ToInt32(hfSerializedItemId.Value)

                    cmd.Parameters.AddWithValue("@SerializedItemId", idObj)
                    cmd.Parameters.AddWithValue("@PartNumberId", pnId)
                    cmd.Parameters.AddWithValue("@SerialNumber", sn)
                    cmd.Parameters.AddWithValue("@ManufacturedDate", mfgObj)
                    cmd.Parameters.AddWithValue("@ReceivedDate", recvObj)
                    cmd.Parameters.AddWithValue("@StatusCode", status)
                    cmd.Parameters.AddWithValue("@Notes", If(notes = "", CType(DBNull.Value, Object), notes))

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
            Using cmd As New SqlCommand("mro2.usp_SerializedItem_List", cn)
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
        gvSI.DataSource = dt
        gvSI.DataBind()
    End Sub

   Private Sub LoadRowForEdit(ByVal id As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand("SELECT SerializedItemId, PartNumberId, SerialNumber, ManufacturedDate, ReceivedDate, StatusCode, Notes " & _
                                         "FROM mro2.SerializedItem " & _
                                         "WHERE SerializedItemId = @Id", cn)
                cmd.Parameters.AddWithValue("@Id", id)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfSerializedItemId.Value = rdr("SerializedItemId").ToString()

                        Dim pnId As String = rdr("PartNumberId").ToString()
                        If ddlPN.Items.FindByValue(pnId) IsNot Nothing Then
                            ddlPN.SelectedValue = pnId
                        Else
                            ddlPN.SelectedIndex = 0
                        End If

                        txtSerial.Text = rdr("SerialNumber").ToString()

                        Dim st As String = If(rdr("StatusCode") Is DBNull.Value, "ACTIVE", rdr("StatusCode").ToString().Trim().ToUpperInvariant())
                        If ddlStatus.Items.FindByValue(st) IsNot Nothing Then
                            ddlStatus.SelectedValue = st
                        Else
                            ddlStatus.SelectedValue = "ACTIVE"
                        End If

                        txtMfgDate.Text = If(rdr("ManufacturedDate") Is DBNull.Value, "", Convert.ToDateTime(rdr("ManufacturedDate")).ToString("yyyy-MM-dd"))
                        txtRecvDate.Text = If(rdr("ReceivedDate") Is DBNull.Value, "", Convert.ToDateTime(rdr("ReceivedDate")).ToString("yyyy-MM-dd"))
                        txtNotes.Text = If(rdr("Notes") Is DBNull.Value, "", rdr("Notes").ToString())
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub ToggleActive(ByVal id As Integer)
        Try
            Dim currentActive As Boolean = True

            Using cn As New SqlConnection(ConnStr)
                cn.Open()

                Using cmdGet As New SqlCommand("SELECT IsActive FROM mro2.SerializedItem WHERE SerializedItemId=@Id", cn)
                    cmdGet.Parameters.AddWithValue("@Id", id)
                    Dim o As Object = cmdGet.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then currentActive = Convert.ToBoolean(o)
                End Using

                Using cmd As New SqlCommand("mro2.usp_SerializedItem_SetActive", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@SerializedItemId", id)
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
            "showSIModal_" & Guid.NewGuid().ToString("N"),
            "$('#siModal').modal('show');",
            True)
    End Sub

    Private Sub HideModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "hideSIModal_" & Guid.NewGuid().ToString("N"),
            "$('#siModal').modal('hide');",
            True)
    End Sub

    Private Function FriendlyDbMessage(ByVal ex As Exception) As String
        Dim sqlEx As SqlException = TryCast(ex, SqlException)
        If sqlEx IsNot Nothing Then
            If sqlEx.Number = 2627 OrElse sqlEx.Number = 2601 Then
                Return "This Serial Number already exists for that PN."
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

    Private Function TryParseHtmlDate(ByVal s As String, ByRef d As DateTime) As Boolean
        Dim t As String = (If(s, "")).Trim()
        If t = "" Then Return False

        Return DateTime.TryParseExact( _
            t, _
            "yyyy-MM-dd", _
            CultureInfo.InvariantCulture, _
            DateTimeStyles.None, _
            d)
    End Function

End Class