Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Web.UI.HtmlControls

' ============================================================
' MRO2/Maintenance/AircraftConfiguration.aspx.vb
'
' Displays the full position tree for one aircraft (AcID from
' querystring). Tree is Zone → System → Slot, rendered as
' nested Bootstrap collapse panels server-side via PlaceHolder.
'
' Each slot row shows:
'   • Row background by SlotHealth
'     (EXPIRED=red, DUE=orange, ALERT=amber, OK=green, EMPTY=grey)
'   • PN / SN of installed component
'   • Worst counter status badge
'   • Days on wing
'   • Quick Install button (empty slots)
'   • Quick Remove button (occupied slots)
'
' Install modal: loads allowed SNs for the position's PN,
'   pre-fills aircraft counter snapshot, calls usp_RecordEvent_Install.
' Remove modal: pre-fills counter snapshot,
'   calls usp_RecordEvent_Remove.
' ============================================================
Partial Class MRO2_Maintenance_AircraftConfiguration
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    ' Exposed to ASPX for JS pre-fill
    Public AcFH_Display As String = "0"
    Public AcFC_Raw As String = "0"
    Public AcLdg_Raw As String = "0"
    Public AcTGO_Raw As String = "0"

    Private _acId As Integer = 0

    ' ────────────────────────────────────────────────────────
    ' PAGE LOAD
    ' ────────────────────────────────────────────────────────
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Integer.TryParse(Request.QueryString("AcID"), _acId)

        If _acId = 0 Then
            pnlNoAc.Visible = True
            pnlMain.Visible = False
            Return
        End If

        pnlNoAc.Visible = False
        pnlMain.Visible = True

        If Not IsPostBack Then
            LoadAircraftHeader()
            LoadCounterTotals()
            BuildTree()
        Else
            ' Dynamic controls in phTree MUST be rebuilt on every postback
            ' so ASP.NET can wire events and UpdatePanel can re-render them.
            ' Individual handlers call BuildTree() again after data changes.
            BuildTree()
        End If
    End Sub

    ' ────────────────────────────────────────────────────────
    ' AIRCRAFT HEADER
    ' ────────────────────────────────────────────────────────
    Private Sub LoadAircraftHeader()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT a.TailNo, t.AcType, g.AcMainGroup " &
                "FROM dbo.tblAircraft a " &
                "INNER JOIN dbo.tblAcType      t ON t.AcTypeId     = a.AcTypeID " &
                "INNER JOIN dbo.tblAcMainGroup g ON g.AcMainGroupID= a.AcMainGroupID " &
                "WHERE a.AcID = @Id", cn)
                cmd.Parameters.AddWithValue("@Id", _acId)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        litTailNo.Text = rdr("TailNo").ToString()
                        litAcType.Text = rdr("AcType").ToString()
                        litAcGroup.Text = rdr("AcMainGroup").ToString()
                    End If
                End Using
            End Using
        End Using
    End Sub

    ' ────────────────────────────────────────────────────────
    ' COUNTER TOTALS (header strip)
    ' ────────────────────────────────────────────────────────
    Private Sub LoadCounterTotals()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT cd.Code, ac.CurrentValue, cd.UnitStorage " &
                "FROM mro2.AcCounter ac " &
                "INNER JOIN mro2.CounterDef cd " &
                "    ON cd.CounterDefId = ac.CounterDefId " &
                "WHERE ac.AcID = @Id " &
                "  AND cd.Code IN " &
                "  ('AF_FLIGHT_MIN','AF_CYCLES','AF_LANDINGS','AF_TOUCH_AND_GO')", cn)
                cmd.Parameters.AddWithValue("@Id", _acId)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim code As String = rdr("Code").ToString()
                        Dim val As Integer = CInt(rdr("CurrentValue"))
                        Select Case code
                            Case "AF_FLIGHT_MIN"
                                ' Store display (decimal hrs) for JS pre-fill
                                Dim hrs As Decimal = Math.Round(val / 60D, 1)
                                litAcFH.Text = hrs.ToString("N1")
                                AcFH_Display = hrs.ToString("N1")
                            Case "AF_CYCLES"
                                litAcFC.Text = val.ToString("N0")
                                AcFC_Raw = val.ToString()
                            Case "AF_LANDINGS"
                                litAcLdg.Text = val.ToString("N0")
                                AcLdg_Raw = val.ToString()
                            Case "AF_TOUCH_AND_GO"
                                AcTGO_Raw = val.ToString()
                        End Select
                    End While
                End Using
            End Using
        End Using
    End Sub

    ' ────────────────────────────────────────────────────────
    ' BUILD TREE - server-side HTML generation
    ' Renders Zone → System → Slot as nested Bootstrap collapse
    ' panels. Each slot gets a color-coded row and action buttons.
    ' ────────────────────────────────────────────────────────
    Private Sub BuildTree()
        ' Clear previous controls - MUST be done every call
        ' to prevent duplicate rows on postback
        phTree.Controls.Clear()

        ' Load full configuration dataset
        Dim dt As New DataTable()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                    "mro2.usp_AircraftConfiguration_Get", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@AcID", _acId)
                cmd.Parameters.AddWithValue("@PositionFilter", DBNull.Value)
                cn.Open()
                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using

        ' Count health for header badges
        Dim cExpired, cDue, cAlert, cEmpty As Integer

        ' Collect health summary
        For Each row As DataRow In dt.Rows
            Select Case row("SlotHealth").ToString()
                Case "EXPIRED" : cExpired += 1
                Case "DUE" : cDue += 1
                Case "ALERT" : cAlert += 1
                Case "EMPTY" : cEmpty += 1
            End Select
        Next
        RenderHealthBadges(cExpired, cDue, cAlert, cEmpty)

        ' Group by Zone
        Dim zones As DataRow() = dt.Select(
            "1=1", "ZoneCode ASC, SortOrder ASC")

        Dim processedZones As New List(Of String)()
        Dim zoneIndex As Integer = 0

        For Each slotRow As DataRow In dt.Rows
            Dim zoneCode As String = SafeStr(slotRow("ZoneCode"))
            If processedZones.Contains(zoneCode) Then Continue For
            processedZones.Add(zoneCode)
            zoneIndex += 1

            ' Get all slots in this zone
            Dim zoneSlots As DataRow() = dt.Select(
                "ZoneCode = '" & zoneCode.Replace("'", "''") & "'",
                "SystemCode ASC, SortOrder ASC")

            ' Zone worst health
            Dim zoneHealth As String = ZoneWorstHealth(zoneSlots)

            ' ── Zone panel ────────────────────────────────
            Dim zonePanel As New HtmlGenericControl("div")
            zonePanel.Attributes("class") = "card card-outline " &
                HealthCardColor(zoneHealth) & " mb-2"

            ' Zone header - pure JS toggle via named function
            Dim zoneHeader As New HtmlGenericControl("div")
            zoneHeader.Attributes("class") = "card-header py-2 px-3"
            zoneHeader.Attributes("style") = "cursor:pointer;"
            ' Inline toggle - pure sibling walk, no data-* attrs needed
            ' (ASP.NET 3.5 HtmlGenericControl drops data-* attributes)
            zoneHeader.Attributes("onclick") =
                "var h=this,s=h.parentNode.childNodes,b=null;" &
                "for(var i=0;i<s.length;i++){" &
                "if(s[i].nodeType===1&&s[i]!==h&&" &
                "s[i].className&&s[i].className.indexOf('zone-collapse')!==-1)" &
                "{b=s[i];break;}}" &
                "if(!b)return false;" &
                "b.style.display=b.style.display==='none'?'':'none';" &
                "var ic=h.querySelector('.fa-chevron-down');" &
                "if(ic)ic.style.transform=b.style.display===''?'':'rotate(-90deg)';" &
                "return false;"

            Dim zoneTitle As New HtmlGenericControl("div")
            zoneTitle.Attributes("class") = "d-flex align-items-center"
            zoneTitle.InnerHtml =
                "<i class='fas fa-layer-group mr-2 text-muted'></i>" &
                "<strong>" & Server.HtmlEncode(zoneCode) & "</strong>" &
                "<small class='text-muted ml-2'>" &
                Server.HtmlEncode(SafeStr(slotRow("ZoneName"))) &
                "</small>" &
                "<span class='ml-auto'>" &
                HealthBadgeSmall(zoneHealth) &
                "<i class='fas fa-chevron-down ml-2 text-muted small'></i>" &
                "</span>"

            zoneHeader.Controls.Add(zoneTitle)
            zonePanel.Controls.Add(zoneHeader)

            ' Zone collapse body
            ' DO NOT use .ID - ASP.NET mangles it inside ContentPlaceHolder
            Dim zoneCollapse As New HtmlGenericControl("div")
            zoneCollapse.Attributes("class") = "zone-collapse"

            Dim zoneBody As New HtmlGenericControl("div")
            zoneBody.Attributes("class") = "card-body p-0"

            ' ── System grouping within zone ───────────────
            Dim processedSystems As New List(Of String)()
            Dim sysIndex As Integer = 0

            For Each slotR As DataRow In zoneSlots
                Dim sysCode As String = SafeStr(slotR("SystemCode"))
                If processedSystems.Contains(sysCode) Then Continue For
                processedSystems.Add(sysCode)
                sysIndex += 1

                Dim sysSlots As DataRow() = dt.Select(
                    "ZoneCode = '" & zoneCode.Replace("'", "''") &
                    "' AND SystemCode = '" & sysCode.Replace("'", "''") & "'",
                    "SortOrder ASC, PositionCode ASC")

                ' System header row
                Dim sysRow As New HtmlGenericControl("div")
                sysRow.Attributes("class") =
                    "px-3 py-1 border-bottom bg-light " &
                    "d-flex align-items-center"
                sysRow.Attributes("style") = "font-size:.82rem;"
                sysRow.InnerHtml =
                    "<i class='fas fa-chevron-right mr-2 text-muted'></i>" &
                    "<span class='font-weight-bold text-secondary mr-2'>" &
                    Server.HtmlEncode(sysCode) & "</span>" &
                    "<span class='text-muted'>" &
                    Server.HtmlEncode(SafeStr(slotR("SystemName"))) &
                    "</span>" &
                    "<span class='ml-auto text-muted' style='font-size:.75rem;'>" &
                    sysSlots.Length & " slot(s)</span>"
                zoneBody.Controls.Add(sysRow)

                ' ── Slot rows within system ───────────────
                For Each slot As DataRow In sysSlots
                    zoneBody.Controls.Add(BuildSlotRow(slot))
                Next
            Next

            zoneCollapse.Controls.Add(zoneBody)
            zonePanel.Controls.Add(zoneCollapse)
            phTree.Controls.Add(zonePanel)
        Next

        ' Empty state
        If processedZones.Count = 0 Then
            Dim empty As New HtmlGenericControl("div")
            empty.Attributes("class") = "alert alert-secondary"
            empty.InnerHtml =
                "<i class='fas fa-info-circle mr-1'></i>" &
                "Aucune position configur&eacute;e pour cet a&eacute;ronef. " &
                "Utilisez Setup &rarr; Gabarits pour initialiser l&apos;arbre."
            phTree.Controls.Add(empty)
        End If
    End Sub

    ' ── Build one slot row ────────────────────────────────
    Private Function BuildSlotRow(ByVal slot As DataRow) As Control
        Dim health As String = slot("SlotHealth").ToString()
        Dim isEmpty As Boolean = (slot("SlotStatus").ToString() = "EMPTY")

        Dim row As New HtmlGenericControl("div")
        row.Attributes("class") =
            "d-flex align-items-center px-4 py-2 border-bottom " &
            SlotRowClass(health)

        Dim sb As New StringBuilder()

        ' Indent marker (slot level)
        sb.Append("<span class='text-muted mr-3' style='font-size:.7rem;'>&#9492;&#9472;</span>")

        ' Position code
        sb.Append("<span class='font-weight-bold mr-3' style='min-width:160px;font-size:.85rem;'>")
        sb.Append(Server.HtmlEncode(slot("PositionCode").ToString()))
        sb.Append("</span>")

        If isEmpty Then
            ' ── EMPTY SLOT ────────────────────────────────
            sb.Append("<span class='text-muted mr-3' style='font-size:.82rem;'>")
            sb.Append("<i class='fas fa-minus-circle mr-1'></i>")
            sb.Append("Vide</span>")

            ' Allowed PN count hint
            Dim pnCount As Integer = CInt(slot("AllowedPNCount"))
            If pnCount > 0 Then
                sb.Append("<span class='badge badge-light border text-muted mr-2' ")
                sb.Append("title='" & pnCount & " PN(s) autoris&eacute;(s)'>")
                sb.Append("<i class='fas fa-barcode mr-1'></i>" & pnCount & " PN</span>")
            Else
                sb.Append("<span class='badge badge-warning mr-2' ")
                sb.Append("title='Aucun PN autoris&eacute; configur&eacute;'>")
                sb.Append("<i class='fas fa-exclamation-triangle mr-1'></i>Aucun PN</span>")
            End If

            sb.Append("<span class='ml-auto'>")

            ' Install button (only if PN configured for position)
            If pnCount > 0 Then
                Dim posId As String = slot("AcPositionId").ToString()
                Dim posCode As String = slot("PositionCode").ToString()
                sb.Append("<button type='button' ")
                sb.Append("class='btn btn-xs btn-success' ")
                sb.Append("onclick=""openInstallModal(" & posId &
                           ",'" & posCode.Replace("'", "\'") & "');return false;"">")
                sb.Append("<i class='fas fa-plus mr-1'></i>Installer</button>")
            End If

            sb.Append("</span>")
        Else
            ' ── OCCUPIED SLOT ─────────────────────────────
            ' PN / SN
            sb.Append("<span class='mr-3' style='min-width:220px;font-size:.82rem;'>")
            sb.Append("<span class='text-muted' style='font-size:.72rem;'>PN&nbsp;</span>")
            sb.Append("<strong>" & Server.HtmlEncode(slot("PN").ToString()) & "</strong>")
            sb.Append("<br/>")
            sb.Append("<span class='text-muted' style='font-size:.72rem;'>SN&nbsp;</span>")
            ' SN links to SNDetail page (integer id only - safe)
            Dim snDetailUrl As String =
                ResolveUrl("~/MRO2/Maintenance/SNDetail.aspx") &
                "?id=" & slot("SerializedItemId").ToString()
            sb.Append("<a href='" & snDetailUrl & "' class='text-dark'>")
            sb.Append(Server.HtmlEncode(slot("SerialNumber").ToString()))
            sb.Append("</a>")
            sb.Append("</span>")

            ' Nomenclature (truncated)
            Dim nomenc As String = SafeStr(slot("Nomenclature"))
            If nomenc.Length > 30 Then nomenc = nomenc.Substring(0, 28) & "…"
            sb.Append("<span class='text-muted mr-3' style='font-size:.78rem;min-width:150px;'>")
            sb.Append(Server.HtmlEncode(nomenc))
            sb.Append("</span>")

            ' Worst counter status badge
            sb.Append("<span class='mr-3'>")
            sb.Append(HealthBadgeFull(slot("WorstCounterStatus").ToString(),
                                       slot("MinRemaining"),
                                       slot("OverdueCount"),
                                       slot("AlertCount")))
            sb.Append("</span>")

            ' Days on wing
            If Not slot("DaysOnWing") Is DBNull.Value Then
                Dim days As Integer = CInt(slot("DaysOnWing"))
                sb.Append("<span class='text-muted mr-3' style='font-size:.78rem;'>")
                sb.Append("<i class='fas fa-calendar-day mr-1'></i>")
                sb.Append(days & " j.</span>")
            End If

            ' Action buttons
            sb.Append("<span class='ml-auto'>")

            ' Remove button
            Dim posId2 As String = slot("AcPositionId").ToString()
            Dim posCode2 As String = slot("PositionCode").ToString()
            Dim snId As String = slot("SerializedItemId").ToString()
            Dim snNum As String = slot("SerialNumber").ToString()

            sb.Append("<button type='button' ")
            sb.Append("class='btn btn-xs btn-outline-danger' ")
            sb.Append("onclick=""openRemoveModal(" & posId2 & "," & snId &
                       ",'" & posCode2.Replace("'", "\'") & "'" &
                       ",'" & snNum.Replace("'", "\'") & "');return false;"">")
            sb.Append("<i class='fas fa-minus mr-1'></i>D&eacute;poser</button>")

            sb.Append("</span>")
        End If

        row.InnerHtml = sb.ToString()
        Return row
    End Function

    ' ────────────────────────────────────────────────────────
    ' INSTALL BUTTON - opens modal, loads allowed SNs
    ' Called via JS onclick → __doPostBack equivalent
    ' We use a hidden LinkButton per slot - instead, we use
    ' a dedicated server PostBack via asp:Button with args
    ' stored in hidden fields set by JS, then modal shown.
    ' ────────────────────────────────────────────────────────
    Protected Sub Page_PreRender(sender As Object, e As EventArgs) _
            Handles Me.PreRender
        ' Register JS functions that call server-side postbacks
        Dim jsInstall As String =
            "function openInstallModal(posId, posCode) {" &
            "  document.getElementById('" & hfInstallPositionId.ClientID & "').value = posId;" &
            "  document.getElementById('" & litInstallPosition.ClientID & "').innerHTML = posCode;" &
            "  __doPostBack('" & btnLoadInstallModal.UniqueID & "','');" &
            "}"

        Dim jsRemove As String =
            "function openRemoveModal(posId, snId, posCode, snNum) {" &
            "  document.getElementById('" & hfRemovePositionId.ClientID & "').value = posId;" &
            "  document.getElementById('" & hfRemoveSerializedItemId.ClientID & "').value = snId;" &
            "  document.getElementById('" & litRemovePosition.ClientID & "').innerHTML = posCode;" &
            "  document.getElementById('" & litRemoveSN.ClientID & "').innerHTML = snNum;" &
            "  __doPostBack('" & btnLoadRemoveModal.UniqueID & "','');" &
            "}"

        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "acModalFns", jsInstall & jsRemove, True)
    End Sub

    Protected Sub btnLoadInstallModal_Click(sender As Object, e As EventArgs) _
            Handles btnLoadInstallModal.Click
        Dim posId As Integer = 0
        Integer.TryParse(hfInstallPositionId.Value, posId)
        If posId = 0 Then Return

        LoadInstallSNs(posId)

        ' Restore position label from hidden field (JS set it before postback
        ' but UpdatePanel re-render clears asp:Label.Text)
        Dim posCode As String = GetPositionCode(posId)
        litInstallPosition.Text = Server.HtmlEncode(posCode)

        ' Set today as default date
        txtInstallDate.Text = Date.Today.ToString("yyyy-MM-dd")
        txtInstallWO.Text = ""
        txtInstallFH.Text = ""
        txtInstallFC.Text = ""
        txtInstallLdg.Text = ""
        txtInstallTGO.Text = ""
        lblInstallError.Visible = False

        ShowModal("installModal")
    End Sub

    Protected Sub btnLoadRemoveModal_Click(sender As Object, e As EventArgs) _
            Handles btnLoadRemoveModal.Click
        ' Restore position + SN labels from hidden fields
        Dim posId As Integer = 0
        Integer.TryParse(hfRemovePositionId.Value, posId)
        If posId > 0 Then
            litRemovePosition.Text = Server.HtmlEncode(GetPositionCode(posId))
        End If
        ' SN label restored from hidden field value via JS data-attr
        ' hfRemoveSerializedItemId holds the snId - get SN number for display
        Dim snId As Integer = 0
        Integer.TryParse(hfRemoveSerializedItemId.Value, snId)
        If snId > 0 Then
            litRemoveSN.Text = Server.HtmlEncode(GetSerialNumber(snId))
        End If

        txtRemoveDate.Text = Date.Today.ToString("yyyy-MM-dd")
        txtRemoveWO.Text = ""
        txtRemoveRemarks.Text = ""
        txtRemoveFH.Text = ""
        txtRemoveFC.Text = ""
        txtRemoveLdg.Text = ""
        lblRemoveError.Visible = False
        ShowModal("removeModal")
    End Sub

    Private Sub LoadInstallSNs(ByVal positionId As Integer)
        ddlInstallSN.Items.Clear()
        ddlInstallSN.Items.Add(New System.Web.UI.WebControls.ListItem(
            "-- Sélectionner SN --", ""))

        ' Get template position ID for this tail position
        Dim templateId As Integer = 0
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT AcPositionTemplateId FROM mro2.AcPosition " &
                "WHERE AcPositionId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", positionId)
                cn.Open()
                Dim o As Object = cmd.ExecuteScalar()
                If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                    templateId = CInt(o)
                End If
            End Using
        End Using

        If templateId = 0 Then Return

        ' Load SNs for the allowed PNs at this position
        ' Filter: IsActive=1, StatusCode in (ACTIVE, SERVICEABLE)
        ' Exclude SNs currently installed anywhere
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT si.SerializedItemId, pn.PN, si.SerialNumber, " &
                "       pn.Nomenclature, pp.IsPrimary " &
                "FROM mro2.AcPositionPN pp " &
                "INNER JOIN mro2.PartNumber     pn " &
                "    ON pn.PartNumberId = pp.PartNumberId " &
                "INNER JOIN mro2.SerializedItem si " &
                "    ON si.PartNumberId = pn.PartNumberId " &
                "WHERE pp.AcPositionTemplateId = @TplId " &
                "  AND pp.IsActive = 1 " &
                "  AND si.IsActive = 1 " &
                "  AND si.StatusCode IN ('ACTIVE','SERVICEABLE') " &
                "  AND si.SerializedItemId NOT IN ( " &
                "      SELECT SerializedItemId FROM mro2.vw_CurrentInstallation) " &
                "ORDER BY pp.IsPrimary DESC, pn.PN, si.SerialNumber", cn)
                cmd.Parameters.AddWithValue("@TplId", templateId)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim primary As String =
                            If(Convert.ToBoolean(rdr("IsPrimary")), "", " [Alt]")
                        ddlInstallSN.Items.Add(
                            New System.Web.UI.WebControls.ListItem(
                                rdr("PN").ToString() & " / " &
                                rdr("SerialNumber").ToString() & primary,
                                rdr("SerializedItemId").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    ' ────────────────────────────────────────────────────────
    ' SAVE - INSTALL
    ' ────────────────────────────────────────────────────────
    Protected Sub btnInstallSave_Click(sender As Object, e As EventArgs) _
            Handles btnInstallSave.Click
        lblInstallError.Visible = False

        Dim posId As Integer = 0
        Dim snId As Integer = 0
        Integer.TryParse(hfInstallPositionId.Value, posId)
        Integer.TryParse(ddlInstallSN.SelectedValue, snId)

        If posId = 0 OrElse snId = 0 Then
            ShowInstallError("S&eacute;lectionnez un num&eacute;ro de s&eacute;rie.")
            Return
        End If

        Dim eventDate As Date = Date.Today
        If txtInstallDate.Text.Trim() = "" OrElse
           Not Date.TryParse(txtInstallDate.Text.Trim(),
               System.Globalization.CultureInfo.InvariantCulture,
               System.Globalization.DateTimeStyles.None,
               eventDate) Then
            ShowInstallError("Date invalide. Format attendu : YYYY-MM-DD")
            Return
        End If

        ' Parse FH (entered as decimal hours → convert to minutes)
        Dim fhMinutes As Object = DBNull.Value
        Dim fhDec As Decimal = 0
        If txtInstallFH.Text.Trim() <> "" Then
            If Decimal.TryParse(txtInstallFH.Text.Trim().Replace(",", "."),
                                System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture,
                                fhDec) Then
                fhMinutes = CInt(Math.Round(fhDec * 60))
            End If
        End If

        Dim fcVal As Object = DBNull.Value
        Dim ldgVal As Object = DBNull.Value
        Dim tgoVal As Object = DBNull.Value
        Dim tmp As Integer = 0
        If Integer.TryParse(txtInstallFC.Text.Trim(), tmp) Then fcVal = tmp : tmp = 0
        If Integer.TryParse(txtInstallLdg.Text.Trim(), tmp) Then ldgVal = tmp : tmp = 0
        If Integer.TryParse(txtInstallTGO.Text.Trim(), tmp) Then tgoVal = tmp

        Dim userId As String = If(Session("UserId") IsNot Nothing, Session("UserId").ToString(), "system")
        If String.IsNullOrEmpty(userId) Then userId = "system"

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand("mro2.usp_RecordEvent_Install", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@SerializedItemId", snId)
                    cmd.Parameters.AddWithValue("@AcID", _acId)
                    cmd.Parameters.AddWithValue("@AcPositionId", posId)
                    cmd.Parameters.AddWithValue("@EventDate", eventDate)
                    cmd.Parameters.AddWithValue("@EventTime", DBNull.Value)
                    cmd.Parameters.AddWithValue("@WorkOrderRef",
                        If(txtInstallWO.Text.Trim() = "",
                           CType(DBNull.Value, Object),
                           txtInstallWO.Text.Trim()))
                    cmd.Parameters.AddWithValue("@Remarks", DBNull.Value)
                    cmd.Parameters.AddWithValue("@PerformedByUserId", userId)
                    cmd.Parameters.AddWithValue("@AuthorisedByUserId", DBNull.Value)
                    cmd.Parameters.AddWithValue("@AcFH_Minutes", fhMinutes)
                    cmd.Parameters.AddWithValue("@AcFC", fcVal)
                    cmd.Parameters.AddWithValue("@AcLandings", ldgVal)
                    cmd.Parameters.AddWithValue("@AcTGO", tgoVal)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    cn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            HideModal("installModal")
            LoadCounterTotals()
            BuildTree()
            ShowToast("Composant install&eacute; avec succ&egrave;s.", "success")

        Catch ex As SqlException
            ShowInstallError(FriendlyDbMessage(ex))
        Catch ex As Exception
            ShowInstallError(Server.HtmlEncode(ex.Message))
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' SAVE - REMOVE
    ' ────────────────────────────────────────────────────────
    Protected Sub btnRemoveSave_Click(sender As Object, e As EventArgs) _
            Handles btnRemoveSave.Click
        lblRemoveError.Visible = False

        Dim posId As Integer = 0
        Dim snId As Integer = 0
        Integer.TryParse(hfRemovePositionId.Value, posId)
        Integer.TryParse(hfRemoveSerializedItemId.Value, snId)

        If posId = 0 OrElse snId = 0 Then
            ShowRemoveError("Donn&eacute;es de position invalides.")
            Return
        End If

        Dim eventDate As Date = Date.Today
        If txtRemoveDate.Text.Trim() = "" OrElse
           Not Date.TryParse(txtRemoveDate.Text.Trim(),
               System.Globalization.CultureInfo.InvariantCulture,
               System.Globalization.DateTimeStyles.None,
               eventDate) Then
            ShowRemoveError("Date invalide. Format attendu : YYYY-MM-DD")
            Return
        End If

        Dim fhMinutes As Object = DBNull.Value
        Dim fhDec As Decimal = 0
        If txtRemoveFH.Text.Trim() <> "" Then
            If Decimal.TryParse(txtRemoveFH.Text.Trim().Replace(",", "."),
                                System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture,
                                fhDec) Then
                fhMinutes = CInt(Math.Round(fhDec * 60))
            End If
        End If

        Dim fcVal As Object = DBNull.Value
        Dim ldgVal As Object = DBNull.Value
        Dim tmp As Integer = 0
        If Integer.TryParse(txtRemoveFC.Text.Trim(), tmp) Then fcVal = tmp : tmp = 0
        If Integer.TryParse(txtRemoveLdg.Text.Trim(), tmp) Then ldgVal = tmp

        Dim userId As String = If(Session("UserId") IsNot Nothing, Session("UserId").ToString(), "system")
        If String.IsNullOrEmpty(userId) Then userId = "system"

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand("mro2.usp_RecordEvent_Remove", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@SerializedItemId", snId)
                    cmd.Parameters.AddWithValue("@AcID", _acId)
                    cmd.Parameters.AddWithValue("@AcPositionId", posId)
                    cmd.Parameters.AddWithValue("@EventDate", eventDate)
                    cmd.Parameters.AddWithValue("@EventTime", DBNull.Value)
                    cmd.Parameters.AddWithValue("@WorkOrderRef",
                        If(txtRemoveWO.Text.Trim() = "",
                           CType(DBNull.Value, Object),
                           txtRemoveWO.Text.Trim()))
                    cmd.Parameters.AddWithValue("@Remarks",
                        If(txtRemoveRemarks.Text.Trim() = "",
                           CType(DBNull.Value, Object),
                           txtRemoveRemarks.Text.Trim()))
                    cmd.Parameters.AddWithValue("@PerformedByUserId", userId)
                    cmd.Parameters.AddWithValue("@AuthorisedByUserId", DBNull.Value)
                    cmd.Parameters.AddWithValue("@AcFH_Minutes", fhMinutes)
                    cmd.Parameters.AddWithValue("@AcFC", fcVal)
                    cmd.Parameters.AddWithValue("@AcLandings", ldgVal)
                    cmd.Parameters.AddWithValue("@AcTGO", DBNull.Value)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    cn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            HideModal("removeModal")
            LoadCounterTotals()
            BuildTree()
            ShowToast("Composant d&eacute;pos&eacute; avec succ&egrave;s.", "success")

        Catch ex As SqlException
            ShowRemoveError(FriendlyDbMessage(ex))
        Catch ex As Exception
            ShowRemoveError(Server.HtmlEncode(ex.Message))
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DISPLAY HELPERS
    ' ────────────────────────────────────────────────────────
    Private Sub RenderHealthBadges(expired As Integer, due As Integer,
                                    alert As Integer, empty As Integer)
        Dim sb As New StringBuilder()
        If expired > 0 Then
            sb.Append("<span class='badge badge-danger mr-1'>" &
                      "<i class='fas fa-skull-crossbones mr-1'></i>" &
                      expired & " EXPIR&Eacute;</span>")
        End If
        If due > 0 Then
            sb.Append("<span class='badge badge-warning text-dark mr-1'>" &
                      "<i class='fas fa-exclamation-triangle mr-1'></i>" &
                      due & " &Agrave; FAIRE</span>")
        End If
        If alert > 0 Then
            sb.Append("<span class='badge badge-info mr-1'>" &
                      "<i class='fas fa-bell mr-1'></i>" &
                      alert & " ALERTE</span>")
        End If
        If empty > 0 Then
            sb.Append("<span class='badge badge-secondary mr-1'>" &
                      empty & " VIDE</span>")
        End If
        If sb.Length = 0 Then
            sb.Append("<span class='badge badge-success'>" &
                      "<i class='fas fa-check mr-1'></i>OK</span>")
        End If
        litHealthBadges.Text = sb.ToString()
    End Sub

    Private Function SlotRowClass(ByVal health As String) As String
        Select Case health
            Case "EXPIRED" : Return "table-danger"
            Case "DUE" : Return "table-warning"
            Case "ALERT" : Return "bg-light-yellow"
            Case "EMPTY" : Return "bg-light"
            Case Else : Return ""   ' OK - no background
        End Select
    End Function

    Private Function HealthCardColor(ByVal health As String) As String
        Select Case health
            Case "EXPIRED" : Return "card-danger"
            Case "DUE" : Return "card-warning"
            Case "ALERT" : Return "card-info"
            Case Else : Return "card-primary"
        End Select
    End Function

    Private Function HealthBadgeSmall(ByVal health As String) As String
        Select Case health
            Case "EXPIRED"
                Return "<span class='badge badge-danger'>EXPIR&Eacute;</span>"
            Case "DUE"
                Return "<span class='badge badge-warning text-dark'>&Agrave; FAIRE</span>"
            Case "ALERT"
                Return "<span class='badge badge-info'>ALERTE</span>"
            Case "EMPTY"
                Return "<span class='badge badge-secondary'>VIDE</span>"
            Case Else
                Return "<span class='badge badge-success'>OK</span>"
        End Select
    End Function

    Private Function HealthBadgeFull(ByVal status As String,
                                      ByVal minRemaining As Object,
                                      ByVal overdueCount As Object,
                                      ByVal alertCount As Object) As String
        Dim sb As New StringBuilder()

        Select Case status
            Case "EXPIRED"
                sb.Append("<span class='badge badge-danger'>")
                sb.Append("<i class='fas fa-skull-crossbones mr-1'></i>EXPIR&Eacute;</span>")
            Case "DUE"
                sb.Append("<span class='badge badge-warning text-dark'>")
                sb.Append("<i class='fas fa-exclamation-triangle mr-1'></i>&Agrave; FAIRE</span>")
                If overdueCount IsNot DBNull.Value AndAlso CInt(overdueCount) > 1 Then
                    sb.Append(" <small class='text-muted'>(" &
                              overdueCount.ToString() & ")</small>")
                End If
            Case "ALERT"
                Dim trem As String = ""
                If minRemaining IsNot DBNull.Value Then
                    trem = " " & minRemaining.ToString()
                End If
                sb.Append("<span class='badge badge-info'>")
                sb.Append("<i class='fas fa-bell mr-1'></i>ALERTE" & trem & "</span>")
            Case "NO_DATA"
                sb.Append("<span class='badge badge-light border text-muted'>")
                sb.Append("Sans compteur</span>")
            Case Else  ' OK
                sb.Append("<span class='badge badge-success'>")
                sb.Append("<i class='fas fa-check mr-1'></i>OK</span>")
        End Select

        Return sb.ToString()
    End Function

    Private Function ZoneWorstHealth(ByVal rows As DataRow()) As String
        ' EMPTY does not override OK - a zone with one installed OK slot
        ' and one empty slot shows OK, not VIDE.
        ' VIDE only shows when ALL slots in the zone are empty.
        Dim worst As Integer = -1   ' -1 = no slots seen yet
        Dim allEmpty As Boolean = True

        For Each r As DataRow In rows
            Dim h As String = r("SlotHealth").ToString()
            If h <> "EMPTY" Then allEmpty = False
            Dim rank As Integer = 0
            Select Case h
                Case "EXPIRED" : rank = 4
                Case "DUE" : rank = 3
                Case "ALERT" : rank = 2
                Case "OK" : rank = 1
                Case "EMPTY" : rank = 0   ' EMPTY lowest - never overrides OK
                Case Else : rank = 0
            End Select
            If rank > worst Then worst = rank
        Next

        If allEmpty Then Return "EMPTY"

        Select Case worst
            Case 4 : Return "EXPIRED"
            Case 3 : Return "DUE"
            Case 2 : Return "ALERT"
            Case Else : Return "OK"
        End Select
    End Function

    ' ────────────────────────────────────────────────────────
    ' UTILITY HELPERS
    ' ────────────────────────────────────────────────────────
    Private Function GetPositionCode(ByVal posId As Integer) As String
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT PositionCode FROM mro2.AcPosition " &
                "WHERE AcPositionId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", posId)
                cn.Open()
                Dim o As Object = cmd.ExecuteScalar()
                Return If(o IsNot Nothing AndAlso o IsNot DBNull.Value,
                          o.ToString(), "")
            End Using
        End Using
    End Function

    Private Function GetSerialNumber(ByVal snId As Integer) As String
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT SerialNumber FROM mro2.SerializedItem " &
                "WHERE SerializedItemId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", snId)
                cn.Open()
                Dim o As Object = cmd.ExecuteScalar()
                Return If(o IsNot Nothing AndAlso o IsNot DBNull.Value,
                          o.ToString(), "")
            End Using
        End Using
    End Function

    Private Function SafeStr(ByVal o As Object) As String
        If o Is Nothing OrElse o Is DBNull.Value Then Return ""
        Return o.ToString()
    End Function

    Private Function FriendlyDbMessage(ByVal ex As SqlException) As String
        If ex.Number = 2627 OrElse ex.Number = 2601 Then
            Return "Ce composant est d&eacute;j&agrave; install&eacute; &agrave; cet emplacement."
        End If
        Return Server.HtmlEncode(ex.Message)
    End Function

    Private Sub ShowInstallError(ByVal msg As String)
        lblInstallError.Text = msg
        lblInstallError.Visible = True
        ShowModal("installModal")
    End Sub

    Private Sub ShowRemoveError(ByVal msg As String)
        lblRemoveError.Text = msg
        lblRemoveError.Visible = True
        ShowModal("removeModal")
    End Sub

    Private Sub ShowModal(ByVal modalId As String)
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "show_" & modalId & "_" & Guid.NewGuid().ToString("N"),
            "$('#" & modalId & "').modal('show');", True)
    End Sub

    Private Sub HideModal(ByVal modalId As String)
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "hide_" & modalId & "_" & Guid.NewGuid().ToString("N"),
            "$('#" & modalId & "').modal('hide');", True)
    End Sub

    Private Sub ShowToast(ByVal message As String, ByVal kind As String)
        Dim ser As New JavaScriptSerializer()
        Dim js As String = "if(window.toastr){toastr." &
                           kind.ToLowerInvariant() & "(" &
                           ser.Serialize(message) & ");}"
        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "toast_" & Guid.NewGuid().ToString("N"), js, True)
    End Sub

End Class