Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Globalization

Partial Class MRO2_Operations_SerializedItemUninstall
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String = _
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        txtUninstallDate.Attributes("type") = "date"

        If Not IsPostBack Then
            LoadPN()
            ddlWorkshop.Items.Clear()
            ddlWorkshop.Items.Add(New ListItem("-- select PN first --", ""))
        End If
    End Sub

    Private Sub LoadPN()
        ddlPN.Items.Clear()
        ddlPN.Items.Add(New ListItem("-- select --", ""))

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_PartNumber_Serialized_List", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@AcMainGroupID", CType(DBNull.Value, Object))

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

    Protected Sub ddlPN_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlPN.SelectedIndexChanged
        lblMsg.Visible = False
        LoadWorkshopsForSelectedPN()
    End Sub

    Private Function GetSelectedPnMainGroupId() As Integer
        Dim pnId As Integer = 0
        Integer.TryParse(ddlPN.SelectedValue, pnId)
        If pnId = 0 Then Return 0

        Dim sql As String = _
            "SELECT AcMainGroupID " & _
            "FROM mro2.PartNumber " & _
            "WHERE PartNumberId=@Id"

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@Id", pnId)
                cn.Open()
                Dim o As Object = cmd.ExecuteScalar()
                If o Is Nothing OrElse o Is DBNull.Value Then Return 0
                Return Convert.ToInt32(o)
            End Using
        End Using
    End Function

    Private Sub LoadWorkshopsForSelectedPN()
        ddlWorkshop.Items.Clear()
        ddlWorkshop.Items.Add(New ListItem("-- select --", ""))

        Dim mgId As Integer = GetSelectedPnMainGroupId()
        If mgId = 0 Then
            ddlWorkshop.Items.Clear()
            ddlWorkshop.Items.Add(New ListItem("-- PN has no AcMainGroup --", ""))
            Exit Sub
        End If

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_Workshop_ListForMainGroup", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@AcMainGroupID", mgId)
                cmd.Parameters.AddWithValue("@BaseId", CType(DBNull.Value, Object))

                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim code As String = rdr("Code").ToString()
                        Dim name As String = If(rdr("Name") Is DBNull.Value, "", rdr("Name").ToString())
                        Dim text As String = code
                        If name.Trim() <> "" Then text &= " - " & name.Trim()

                        ddlWorkshop.Items.Add(New ListItem(text, rdr("WorkshopId").ToString()))
                    End While
                End Using
            End Using
        End Using
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

    Protected Sub btnUninstall_Click(sender As Object, e As EventArgs) Handles btnUninstall.Click
        lblMsg.Visible = False

        Dim pnId As Integer = 0
        Integer.TryParse(ddlPN.SelectedValue, pnId)

        Dim sn As String = txtSerial.Text.Trim().ToUpperInvariant()

        Dim workshopId As Integer = 0
        Integer.TryParse(ddlWorkshop.SelectedValue, workshopId)

        If pnId = 0 Then
            ShowErr("PN is required.")
            Exit Sub
        End If

        If sn = "" Then
            ShowErr("Serial Number is required.")
            Exit Sub
        End If

        If workshopId = 0 Then
            ShowErr("Workshop is required.")
            Exit Sub
        End If

        Dim siId As Integer = ResolveSerializedItemId(pnId, sn)
        If siId = 0 Then
            ShowErr("Serialized item not found for that PN and Serial.")
            Exit Sub
        End If

        Dim workshopCode As String = ResolveWorkshopCode(workshopId)
        If workshopCode = "" Then
            ShowErr("Workshop code not found.")
            Exit Sub
        End If

        Dim uninstallOn As Object = DBNull.Value
        Dim d As DateTime
        If txtUninstallDate.Text.Trim() <> "" Then
            If TryParseHtmlDate(txtUninstallDate.Text, d) Then
                uninstallOn = d.Date
            Else
                ShowErr("Uninstall Date is invalid.")
                Exit Sub
            End If
        End If

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand("mro2.usp_SerializedItem_UninstallToWorkshop", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@SerializedItemId", siId)
                    cmd.Parameters.AddWithValue("@WorkshopCode", workshopCode)
                    cmd.Parameters.AddWithValue("@BaseId", CType(DBNull.Value, Object))
                    cmd.Parameters.AddWithValue("@PerformedBy", "DadaF5")
                    cmd.Parameters.AddWithValue("@SourceSystem", "WEB")
                    cmd.Parameters.AddWithValue("@ReasonCode", CType(DBNull.Value, Object))
                    cmd.Parameters.AddWithValue("@Notes", If(txtNotes.Text.Trim() = "", CType(DBNull.Value, Object), txtNotes.Text.Trim()))
                    cmd.Parameters.AddWithValue("@WorkOrderNo", If(txtWO.Text.Trim() = "", CType(DBNull.Value, Object), txtWO.Text.Trim()))
                    cmd.Parameters.AddWithValue("@TaskCardNo", If(txtTC.Text.Trim() = "", CType(DBNull.Value, Object), txtTC.Text.Trim()))
                    cmd.Parameters.AddWithValue("@ReleaseToServiceRef", CType(DBNull.Value, Object))
                    cmd.Parameters.AddWithValue("@Station", If(txtStation.Text.Trim() = "", CType(DBNull.Value, Object), txtStation.Text.Trim()))
                    cmd.Parameters.AddWithValue("@CertifyingStaff", If(txtCert.Text.Trim() = "", CType(DBNull.Value, Object), txtCert.Text.Trim()))
                    cmd.Parameters.AddWithValue("@DocumentRef", CType(DBNull.Value, Object))
                    cmd.Parameters.AddWithValue("@UninstalledOnDate", uninstallOn)

                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using

            ClearForm()
            lblMsg.CssClass = "text-success"
            lblMsg.Text = "Uninstalled successfully."
            lblMsg.Visible = True

        Catch ex As Exception
            ShowErr(ex.Message)
        End Try
    End Sub

    Private Function ResolveSerializedItemId(ByVal pnId As Integer, ByVal serial As String) As Integer
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_SerializedItem_GetIdByPnAndSerial", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@PartNumberId", pnId)
                cmd.Parameters.AddWithValue("@SerialNumber", serial)
                cn.Open()

                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        Dim id As Object = rdr("SerializedItemId")
                        If id IsNot Nothing AndAlso id IsNot DBNull.Value Then
                            Return Convert.ToInt32(id)
                        End If
                    End If
                End Using
            End Using
        End Using
        Return 0
    End Function

    Private Function ResolveWorkshopCode(ByVal workshopId As Integer) As String
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_Workshop_Get", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@WorkshopId", workshopId)
                cn.Open()

                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        Return rdr("Code").ToString().Trim().ToUpperInvariant()
                    End If
                End Using
            End Using
        End Using
        Return ""
    End Function

    Private Sub ClearForm()
        txtSerial.Text = ""
        ddlWorkshop.SelectedIndex = 0
        txtNotes.Text = ""
        txtWO.Text = ""
        txtTC.Text = ""
        txtStation.Text = ""
        txtCert.Text = ""
        txtUninstallDate.Text = ""
    End Sub

    Private Sub ShowErr(ByVal msg As String)
        lblMsg.CssClass = "text-danger"
        lblMsg.Text = Server.HtmlEncode(msg)
        lblMsg.Visible = True
    End Sub
End Class