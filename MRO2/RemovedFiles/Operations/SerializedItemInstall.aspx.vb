Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Globalization

Partial Class MRO2_Operations_SerializedItemInstall
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String = _
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        txtInstallDate.Attributes("type") = "date"

        If Not IsPostBack Then
            LoadPN()
            ddlAircraft.Items.Clear()
            ddlAircraft.Items.Add(New ListItem("-- select PN first --", ""))
            ddlPosition.Items.Clear()
            ddlPosition.Items.Add(New ListItem("-- select aircraft --", ""))
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
        LoadAircraftForSelectedPN()
        ddlPosition.Items.Clear()
        ddlPosition.Items.Add(New ListItem("-- select aircraft --", ""))
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

    Private Sub LoadAircraftForSelectedPN()
        ddlAircraft.Items.Clear()
        ddlAircraft.Items.Add(New ListItem("-- select --", ""))

        Dim mgId As Integer = GetSelectedPnMainGroupId()
        If mgId = 0 Then
            ddlAircraft.Items.Clear()
            ddlAircraft.Items.Add(New ListItem("-- PN has no AcMainGroup --", ""))
            Exit Sub
        End If

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_Aircraft_List", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@AcMainGroupID", mgId)

                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim tail As String = If(rdr("TailNo") Is DBNull.Value, "", rdr("TailNo").ToString())
                        Dim intc As String = If(rdr("IntCode") Is DBNull.Value, "", rdr("IntCode").ToString())

                        Dim text As String = tail
                        If text.Trim() <> "" AndAlso intc.Trim() <> "" Then
                            text &= " / " & intc
                        ElseIf text.Trim() = "" Then
                            text = intc
                        End If

                        ddlAircraft.Items.Add(New ListItem(text, rdr("AcID").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    Protected Sub ddlAircraft_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlAircraft.SelectedIndexChanged
        lblMsg.Visible = False
        LoadPositions()
    End Sub

    Private Sub LoadPositions()
        ddlPosition.Items.Clear()
        ddlPosition.Items.Add(New ListItem("-- select --", ""))

        Dim acId As Integer = 0
        Integer.TryParse(ddlAircraft.SelectedValue, acId)
        If acId = 0 Then Exit Sub

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_InstallPosition_ListForAircraft", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@AcID", acId)
                cmd.Parameters.AddWithValue("@SerializedOnly", 1)

                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim text As String = rdr("PositionCode").ToString()
                        Dim title As String = If(rdr("Title") Is DBNull.Value, "", rdr("Title").ToString())
                        If title.Trim() <> "" Then text &= " - " & title.Trim()
                        ddlPosition.Items.Add(New ListItem(text, rdr("InstallPositionId").ToString()))
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

    Protected Sub btnInstall_Click(sender As Object, e As EventArgs) Handles btnInstall.Click
        lblMsg.Visible = False

        Dim pnId As Integer = 0
        Integer.TryParse(ddlPN.SelectedValue, pnId)

        Dim sn As String = txtSerial.Text.Trim().ToUpperInvariant()

        Dim acId As Integer = 0
        Integer.TryParse(ddlAircraft.SelectedValue, acId)

        Dim posId As Integer = 0
        Integer.TryParse(ddlPosition.SelectedValue, posId)

        If pnId = 0 Then
            ShowErr("PN is required.")
            Exit Sub
        End If

        If sn = "" Then
            ShowErr("Serial Number is required.")
            Exit Sub
        End If

        If acId = 0 Then
            ShowErr("Aircraft is required.")
            Exit Sub
        End If

        If posId = 0 Then
            ShowErr("Install Position is required.")
            Exit Sub
        End If

        Dim siId As Integer = ResolveSerializedItemId(pnId, sn)
        If siId = 0 Then
            ShowErr("Serialized item not found for that PN and Serial.")
            Exit Sub
        End If

        ' optional effective date (if blank -> NULL, proc can default)
        Dim installedOn As Object = DBNull.Value
        Dim d As DateTime
        If txtInstallDate.Text.Trim() <> "" Then
            If TryParseHtmlDate(txtInstallDate.Text, d) Then
                installedOn = d.Date
            Else
                ShowErr("Install Date is invalid.")
                Exit Sub
            End If
        End If

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand("mro2.usp_SerializedItem_Install", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@SerializedItemId", siId)
                    cmd.Parameters.AddWithValue("@AircraftId", acId)
                    cmd.Parameters.AddWithValue("@InstallPositionId", posId)
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
                    cmd.Parameters.AddWithValue("@InstalledOnDate", installedOn)

                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using

            ClearForm()
            lblMsg.CssClass = "text-success"
            lblMsg.Text = "Installed successfully."
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

    Private Sub ClearForm()
        txtSerial.Text = ""
        ddlAircraft.SelectedIndex = 0
        ddlPosition.Items.Clear()
        ddlPosition.Items.Add(New ListItem("-- select aircraft --", ""))
        txtNotes.Text = ""
        txtWO.Text = ""
        txtTC.Text = ""
        txtStation.Text = ""
        txtCert.Text = ""
        txtInstallDate.Text = ""
    End Sub

    Private Sub ShowErr(ByVal msg As String)
        lblMsg.CssClass = "text-danger"
        lblMsg.Text = Server.HtmlEncode(msg)
        lblMsg.Visible = True
    End Sub
End Class