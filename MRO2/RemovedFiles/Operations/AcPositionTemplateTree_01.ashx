<%@ WebHandler Language="VB" Class="AcPositionTemplateTree" %>

Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Text
Imports System.Web.SessionState

' ============================================================
' MRO2/Setup/Aircraft/AcPositionTemplateTree.ashx
' Returns jsTree-compatible JSON for the position template tree.
'
' Querystring: ?AcTypeId=1
'
' jsTree node format:
' {
'   "id": "node_123",
'   "parent": "#" | "node_456",
'   "text": "MLG-L",
'   "icon": "fas fa-...",
'   "state": { "opened": true },
'   "li_attr": { "class": "..." },
'   "a_attr":  { "class": "..." },
'   "data": {
'     "nodeId": 123,
'     "level": 1,
'     "code": "MLG-L",
'     "desc": "Train Principal Gauche",
'     "ataCode": "32",
'     "pnCount": 0,
'     "qty": 1,
'     "isActive": true,
'     "parentId": null
'   }
' }
' ============================================================
Public Class AcPositionTemplateTree
    Implements IHttpHandler, IRequiresSessionState

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    Public Sub ProcessRequest(ByVal context As HttpContext) _
            Implements IHttpHandler.ProcessRequest

        context.Response.ContentType = "application/json"
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache)

        Dim acTypeId As Integer = 0
        Integer.TryParse(context.Request.QueryString("AcTypeId"), acTypeId)

        If acTypeId = 0 Then
            context.Response.Write("[]")
            Return
        End If

        Dim dt As New DataTable()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                    "mro2.usp_AcPositionTemplate_List", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@AcTypeId",        acTypeId)
                cmd.Parameters.AddWithValue("@IncludeInactive", 1)
                cn.Open()
                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using

        Dim sb As New StringBuilder()
        sb.Append("[")
        Dim first As Boolean = True

        For Each row As DataRow In dt.Rows
            If Not first Then sb.Append(",")
            first = False

            Dim nodeId    As Integer = CInt(row("AcPositionTemplateId"))
            Dim level     As Integer = CInt(row("PositionLevel"))
            Dim code      As String  = SafeStr(row("PositionCode"))
            Dim desc      As String  = SafeStr(row("Description"))
            Dim ataCode   As String  = SafeStr(row("ATACode"))
            Dim pnCount   As Integer = CInt(row("PNCount"))
            Dim qty       As Integer = CInt(row("Quantity"))
            Dim isActive  As Boolean = Convert.ToBoolean(row("IsActive"))
            Dim parentCode As String = SafeStr(row("ParentCode"))

            ' Determine jsTree parent id
            Dim parentNodeId As String = "#"   ' root
            If parentCode <> "" Then
                ' Find parent node id by code
                Dim parentRows() As DataRow = dt.Select(
                    "PositionCode='" & parentCode.Replace("'", "''") & "'")
                If parentRows.Length > 0 Then
                    parentNodeId = "n" & CInt(parentRows(0)("AcPositionTemplateId"))
                End If
            End If

            ' Icon per level
            Dim icon As String
            Select Case level
                Case 1 : icon = "fas fa-layer-group text-primary"
                Case 2 : icon = "fas fa-chevron-right text-secondary"
                Case 3 : icon = If(pnCount > 0,
                                   "fas fa-barcode text-success",
                                   "fas fa-barcode text-warning")
                Case Else : icon = "fas fa-circle"
            End Select

            ' Display text — use single quotes for HTML attrs to keep JSON valid
            Dim text As New StringBuilder()
            text.Append(JsonEscape(code))
            If desc <> "" Then
                text.Append(" <small class='text-muted'>")
                text.Append(JsonEscape(desc))
                text.Append("</small>")
            End If
            If ataCode <> "" Then
                text.Append(" <span class='badge badge-light border' ")
                text.Append("style='font-size:.68rem;'>")
                text.Append("ATA " & JsonEscape(ataCode))
                text.Append("</span>")
            End If
            If level = 3 Then
                Dim pnClass As String =
                    If(pnCount > 0, "badge-success", "badge-warning text-dark")
                text.Append(" <span class='badge " & pnClass & "' ")
                text.Append("style='font-size:.68rem;' ")
                text.Append("data-pnslot='" & nodeId & "' ")
                text.Append("data-pncode='" & JsonEscape(code) & "'>")
                text.Append(If(pnCount > 0,
                               pnCount & " PN",
                               "Aucun PN"))
                text.Append("</span>")
            End If
            If Not isActive Then
                text.Append(" <span class='badge badge-secondary' ")
                text.Append("style='font-size:.68rem;'>Inactif</span>")
            End If

            ' Node classes
            Dim liClass As String =
                If(Not isActive, "jstree-node-inactive", "")

            sb.Append("{")
            sb.Append("""id"":""n" & nodeId & """,")
            sb.Append("""parent"":""" & parentNodeId & """,")
            sb.Append("""text"":""" & text.ToString() & """,")
            sb.Append("""icon"":""" & icon & """,")
            sb.Append("""state"":{""opened"":" &
                      If(level < 3, "true", "false") & "},")
            sb.Append("""li_attr"":{""class"":""" & liClass & """},")
            sb.Append("""data"":{")
            sb.Append("""nodeId"":" & nodeId & ",")
            sb.Append("""level"":" & level & ",")
            sb.Append("""code"":""" & JsonEscape(code) & """,")
            sb.Append("""desc"":""" & JsonEscape(desc) & """,")
            sb.Append("""ataCode"":""" & JsonEscape(ataCode) & """,")
            sb.Append("""pnCount"":" & pnCount & ",")
            sb.Append("""qty"":" & qty & ",")
            sb.Append("""isActive"":" & If(isActive, "true", "false") & ",")
            sb.Append("""parentCode"":""" & JsonEscape(parentCode) & """")
            sb.Append("}")
            sb.Append("}")
        Next

        sb.Append("]")
        context.Response.Write(sb.ToString())
    End Sub

    Private Function SafeStr(ByVal o As Object) As String
        If o Is Nothing OrElse o Is DBNull.Value Then Return ""
        Return o.ToString()
    End Function

    Private Function JsonEscape(ByVal s As String) As String
        If s Is Nothing Then Return ""
        Return s.Replace("\", "\\").
                 Replace("""", "\""").
                 Replace(Chr(13), "\r").
                 Replace(Chr(10), "\n").
                 Replace(Chr(9), "\t")
    End Function

    Public ReadOnly Property IsReusable As Boolean _
            Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
