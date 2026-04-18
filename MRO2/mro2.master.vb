Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Security

Partial Class MRO2_Mro2Master
    Inherits System.Web.UI.MasterPage

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    Public ReadOnly Property UserBaseId() As Integer
        Get
            Dim v As Integer = 0
            If Session("BaseId") IsNot Nothing Then Integer.TryParse(Session("BaseId").ToString(), v)
            Return v
        End Get
    End Property

    Protected Sub Page_Load(ByVal sender As Object,
                            ByVal e As System.EventArgs) Handles Me.Load

        If Not Request.IsAuthenticated Then
            Response.Redirect("~/Login.aspx", False)
            Response.End()
            Exit Sub
        End If
        ' ── 2. Session timeout guard ──
        If Session("BaseId") Is Nothing OrElse Session("UserId") Is Nothing Then
            FormsAuthentication.SignOut()
            Response.Redirect("~/Login.aspx?reason=timeout", False)
            Response.End()
            Exit Sub
        End If
        hfBaseId.Value = If(Session("BaseId") IsNot Nothing, Session("BaseId").ToString(), "0")
        hfUserId.Value = If(Session("UserId") IsNot Nothing, Session("UserId").ToString(), "")

        SetUserStrip()
        ShowFlashMessage()

        If Not IsPostBack Then
            LoadContextSwitcher()
        End If
    End Sub

    Private Sub SetUserStrip()
        Dim baseName As String = SafeStr(Session("UserBaseName"))
        If baseName = "" Then baseName = SafeStr(Session("BaseName"))
        litBaseName.Text = If(baseName <> "", Server.HtmlEncode(baseName), "Base a&eacute;rienne")

        Dim userName As String = ""
        If Context.User.Identity.IsAuthenticated Then userName = Context.User.Identity.Name

        Dim initials As String = "?"
        If userName.Length > 0 Then
            Dim parts() As String = userName.Trim().Split(" "c)
            initials = parts(0).Substring(0, 1).ToUpper()
            If parts.Length > 1 AndAlso parts(1).Length > 0 Then
                initials &= parts(1).Substring(0, 1).ToUpper()
            End If
        End If

        litUserInitials.Text = initials
    End Sub

    ' Flash messages for MRO2 module:
    '   Session("MRO2_Msg") / Session("MRO2_MsgType")
    Private Sub ShowFlashMessage()
        Dim msg As Object = Session("MRO2_Msg")
        Dim msgType As Object = Session("MRO2_MsgType")

        If msg Is Nothing OrElse msg.ToString() = "" Then
            pnlFlash.Visible = False
            Exit Sub
        End If

        Dim alertClass As String = "alert-info"
        Select Case SafeStr(msgType).ToLower()
            Case "success" : alertClass = "alert-success"
            Case "danger" : alertClass = "alert-danger"
            Case "warning" : alertClass = "alert-warning"
            Case Else : alertClass = "alert-info"
        End Select

        pnlFlash.Controls.Clear()
        pnlFlash.Controls.Add(New LiteralControl(msg.ToString()))
        pnlFlash.CssClass = "mro-flash alert " & alertClass & " alert-dismissible fade show mb-0"
        pnlFlash.Visible = True

        Session.Remove("MRO2_Msg")
        Session.Remove("MRO2_MsgType")
    End Sub

    Private Sub LoadContextSwitcher()

        Dim currentBaseId As Integer = CInt(If(Session("BaseId"), 0))
        If currentBaseId = 0 Then
            litBaseName.Text = "<strong class='text-warning'>Administration</strong>"
        Else
            litBaseName.Text = CStr(If(Session("BaseName"), ""))
        End If

        If Not HttpContext.Current.User.IsInRole("Administrators") Then Return

        Dim sql As String =
            "SELECT BaseID, RTRIM(BASE) AS BASE, RTRIM(NOM) AS NOM " &
            "FROM tblBase ORDER BY BASE"

        Using conn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(sql, conn)
                conn.Open()
                Dim dt As New DataTable()
                dt.Load(cmd.ExecuteReader())
                rptBases.DataSource = dt
                rptBases.DataBind()
            End Using
        End Using
    End Sub

    Protected Sub rptBases_ItemCommand(ByVal source As Object,
                                       ByVal e As RepeaterCommandEventArgs)
        If e.CommandName <> "SwitchBase" Then Return

        Dim targetBaseId As Integer = CInt(e.CommandArgument)
        Dim baseName As String = GetBaseName(targetBaseId)

        SwitchContext(targetBaseId, baseName)
        Response.Redirect("~/Default.aspx", False)
    End Sub

    Private Sub SwitchContext(ByVal baseId As Integer, ByVal baseName As String)
        Session("BaseId") = baseId
        Session("BaseName") = baseName
        Session("BPI_BaseId") = baseId
        Session("BPI_BaseName") = baseName
    End Sub

    Private Function GetBaseName(ByVal baseId As Integer) As String
        Dim sql As String =
            "SELECT ISNULL(RTRIM(BASE), '') FROM tblBase WHERE BaseID = @BaseID"

        Using conn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@BaseID", baseId)
                conn.Open()
                Dim result As Object = cmd.ExecuteScalar()
                Return If(result IsNot Nothing, result.ToString(), "")
            End Using
        End Using
    End Function

    Private Function SafeStr(ByVal o As Object) As String
        If o Is Nothing OrElse o Is DBNull.Value Then Return ""
        Return o.ToString().Trim()
    End Function
End Class