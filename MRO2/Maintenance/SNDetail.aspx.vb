Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls

' ============================================================
' MRO2/Maintenance/SNDetail.aspx.vb
'
' URL: SNDetail.aspx?id=<SerializedItemId>  (integer only)
'
' Displays for one serialized component:
'   • Identity header (SN, PN, nomenclature, ATA, status,
'     current install position, days on wing, overall health)
'   • TaskCounter cards (badge + progress bar + extension btn)
'   • Event history timeline (last 20 events)
'   • Extension modal (VALUE or PCT, Reason + DocRef + Approver + Notes)
'
' Data loaded via usp_SNDetail_Get (5 result sets):
'   RS0 - SN identity
'   RS1 - Current installation
'   RS2 - TaskCounter states
'   RS3 - Active extensions
'   RS4 - Event history
' ============================================================
Partial Class MRO2_Maintenance_SNDetail
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    Private _snId As Integer = 0

    ' ────────────────────────────────────────────────────────
    ' PAGE LOAD
    ' ────────────────────────────────────────────────────────
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' Safe integer-only querystring - no string touches SQL
        ' Accept both ?SerializedItemId=x (from AircraftConfiguration link)
        ' and ?id=x (direct navigation / legacy)
        Dim qsId As String = Request.QueryString("SerializedItemId")
        If qsId = "" OrElse qsId Is Nothing Then
            qsId = Request.QueryString("id")
        End If
        Integer.TryParse(qsId, _snId)

        If _snId = 0 Then
            pnlNotFound.Visible = True
            pnlMain.Visible = False
            Return
        End If

        pnlNotFound.Visible = False
        pnlMain.Visible = True

        If Not IsPostBack Then
            LoadPage()
        End If
    End Sub

    Private Sub LoadPage()
        Dim ds As DataSet = LoadDetailDataSet()
        If ds Is Nothing OrElse ds.Tables.Count < 5 Then
            pnlNotFound.Visible = True
            pnlMain.Visible = False
            Return
        End If

        RenderHeader(ds.Tables(0), ds.Tables(1))

        ' Safety: verify RS3 has expected columns before rendering
        Dim tC As DataTable = ds.Tables(2)
        ' Log RS3 columns for diagnostics (remove after confirmed working)
        ' Dim colList = String.Join(", ", (From c As DataColumn In tC.Columns Select c.ColumnName).ToArray())

        RenderCounters(ds.Tables(2), ds.Tables(3))
        RenderHistory(ds.Tables(4))
        LoadExtensionDDLs()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DATA - load all 5 result sets in one call
    ' ────────────────────────────────────────────────────────
    Private Function LoadDetailDataSet() As DataSet
        Dim ds As New DataSet()
        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand(
                        "mro2.usp_SNDetail_Get", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@SerializedItemId", _snId)
                    cn.Open()
                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(ds)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ' SP not found or error - show not found
            Return Nothing
        End Try
        Return ds
    End Function

    ' ────────────────────────────────────────────────────────
    ' RENDER HEADER
    ' RS0: SN identity, RS1: current install
    ' ────────────────────────────────────────────────────────
    Private Sub RenderHeader(ByVal dtSN As DataTable,
                              ByVal dtInstall As DataTable)
        If dtSN.Rows.Count = 0 Then
            pnlNotFound.Visible = True
            pnlMain.Visible = False
            Return
        End If

        Dim r As DataRow = dtSN.Rows(0)
        litSN.Text = Server.HtmlEncode(SafeStr(r("SerialNumber")))
        litPN.Text = Server.HtmlEncode(SafeStr(r("PN")))
        litNomenclature.Text = Server.HtmlEncode(SafeStr(r("Nomenclature")))

        Dim ata As String = SafeStr(r("ATACode"))
        litATA.Text = If(ata <> "", Server.HtmlEncode(ata), "-")

        ' SN status badge
        Dim status As String = SafeStr(r("StatusCode"))
        litSNStatus.Text = SNStatusBadge(status)

        ' Current installation
        If dtInstall.Rows.Count > 0 Then
            Dim ri As DataRow = dtInstall.Rows(0)
            litCurrentPosition.Text =
                "<span class='font-weight-bold text-primary'>" &
                Server.HtmlEncode(SafeStr(ri("PositionCode"))) &
                "</span>"
            litCurrentAircraft.Text =
                "<span class='font-weight-bold'>" &
                Server.HtmlEncode(SafeStr(ri("TailNo"))) &
                "</span>"
            Dim days As Integer = 0
            Integer.TryParse(SafeStr(ri("DaysOnWing")), days)
            litDaysOnWing.Text =
                "<strong>" & days & "</strong>" &
                " <small class='text-muted'>j.</small>"
        End If

        ' Overall health is set by RenderCounters from RS3
        ' (computed from SNTaskCounterState rows, not RS1)
        litOverallHealth.Text = HealthBadgeLarge("OK") ' default until RS3 overrides
    End Sub

    ' ────────────────────────────────────────────────────────
    ' RENDER COUNTERS
    ' RS2: SNTaskCounterState rows, RS3: active extensions
    ' ────────────────────────────────────────────────────────
    Private Sub RenderCounters(ByVal dtCounters As DataTable,
                                ByVal dtExt As DataTable)
        phCounters.Controls.Clear()

        If dtCounters.Rows.Count = 0 Then
            Dim empty As New HtmlGenericControl("div")
            empty.Attributes("class") = "text-center text-muted py-4"
            empty.InnerHtml =
                "<i class='fas fa-info-circle fa-2x mb-2 d-block'></i>" &
                "Aucun compteur de t&acirc;che d&eacute;fini pour ce composant."
            phCounters.Controls.Add(empty)
            Return
        End If

        ' Compute overall worst status from all counter rows
        Dim worstRank As Integer = 0
        Dim worstSt As String = "OK"
        Dim statusCol As String = "CounterStatus"  ' default fallback
        If dtCounters.Columns.Contains("CounterStatusStored") Then
            statusCol = "CounterStatusStored"
        ElseIf dtCounters.Columns.Contains("CounterStatusCalc") Then
            statusCol = "CounterStatusCalc"
        End If
        For Each row As DataRow In dtCounters.Rows
            Dim st As String = SafeStr(row(statusCol))
            Dim rk As Integer = StatusRank(st)
            If rk > worstRank Then worstRank = rk : worstSt = st
        Next
        litOverallHealth.Text = HealthBadgeLarge(worstSt)

        ' Index active extensions by TaskCounterId
        Dim extIdx As New Dictionary(Of Integer, DataRow)()
        For Each er As DataRow In dtExt.Rows
            Dim tcId As Integer = CInt(er("TaskCounterId"))
            If Not extIdx.ContainsKey(tcId) Then
                extIdx(tcId) = er
            End If
        Next

        For Each row As DataRow In dtCounters.Rows
            phCounters.Controls.Add(
                BuildCounterCard(row, extIdx))
        Next
    End Sub

    Private Function BuildCounterCard(
            ByVal row As DataRow,
            ByVal extIdx As Dictionary(Of Integer, DataRow)) As Control

        Dim tcId As Integer = CInt(row("TaskCounterId"))
        Dim defCode As String = SafeStr(row("CounterDefCode"))
        Dim defName As String = SafeStr(row("CounterDefCode"))
        Dim basisCode As String = SafeStr(row("CounterBasisCode"))
        Dim unit As String = SafeStr(row("DisplayUnit"))

        ' Defensive column reads - handles both aliased and unaliased view output
        Dim t As DataTable = row.Table

        Dim status As String = ""
        If t.Columns.Contains("CounterStatusStored") Then
            status = SafeStr(row("CounterStatusStored"))
        ElseIf t.Columns.Contains("CounterStatus") Then
            status = SafeStr(row("CounterStatus"))
        End If

        Dim nextDue As Object = DBNull.Value
        If t.Columns.Contains("EffNextDueAt") Then
            nextDue = row("EffNextDueAt")
        ElseIf t.Columns.Contains("BaseNextDueAt") Then
            nextDue = row("BaseNextDueAt")
        ElseIf t.Columns.Contains("NextDueAt") Then
            nextDue = row("NextDueAt")
        End If

        Dim accum As Object = If(t.Columns.Contains("AccumulatedSinceLast"), row("AccumulatedSinceLast"), DBNull.Value)
        Dim lifetime As Object = If(t.Columns.Contains("LifetimeTotal"), row("LifetimeTotal"), DBNull.Value)

        Dim ceiling As Object = DBNull.Value
        If t.Columns.Contains("EffCeiling") Then
            ceiling = row("EffCeiling")
        ElseIf t.Columns.Contains("Ceiling") Then
            ceiling = row("Ceiling")
        End If

        Dim alertPct As Integer = 90
        If t.Columns.Contains("EffAlertPct") Then
            Integer.TryParse(SafeStr(row("EffAlertPct")), alertPct)
        ElseIf t.Columns.Contains("AlertThresholdPct") Then
            Integer.TryParse(SafeStr(row("AlertThresholdPct")), alertPct)
        End If

        Dim maxExtPct As Object = If(t.Columns.Contains("MaxExtensionPct"), row("MaxExtensionPct"), DBNull.Value)
        Dim maxExtVal As Object = If(t.Columns.Contains("MaxExtensionValue"), row("MaxExtensionValue"), DBNull.Value)

        ' Card container
        Dim card As New HtmlGenericControl("div")
        card.Attributes("class") =
            "card counter-card status-" & status.ToLowerInvariant() &
            " mb-2"

        Dim body As New HtmlGenericControl("div")
        body.Attributes("class") = "card-body py-2 px-3"

        Dim sb As New StringBuilder()

        ' ── Row 1: Counter label + status badge + extension btn ──
        sb.Append("<div class='d-flex align-items-center mb-1'>")

        ' Counter identity
        sb.Append("<div class='flex-grow-1'>")
        sb.Append("<span class='font-weight-bold'>" &
                  Server.HtmlEncode(defCode) & "</span>")
        If defName <> "" Then
            sb.Append(" <small class='text-muted'>" &
                      Server.HtmlEncode(defName) & "</small>")
        End If
        sb.Append(" <span class='badge badge-light border' " &
                  "style='font-size:.7rem;'>" &
                  Server.HtmlEncode(basisCode) & "</span>")
        sb.Append("</div>")

        ' Status badge
        sb.Append("<div class='ml-2'>")
        sb.Append(CounterStatusBadge(status))
        sb.Append("</div>")

        ' Extension badge if active
        If extIdx.ContainsKey(tcId) Then
            Dim er As DataRow = extIdx(tcId)
            sb.Append("<span class='badge badge-warning text-dark ml-2 ext-badge'>" &
                      "<i class='fas fa-expand-arrows-alt mr-1'></i>" &
                      "Prolongation active</span>")
        End If

        ' Prolonger button
        sb.Append("<button type='button' " &
                  "class='btn btn-xs btn-outline-warning ml-2' " &
                  "onclick='openExtModal(" & tcId & "," & _snId & ");return false;' " &
                  "title='Accorder une prolongation'>")
        sb.Append("<i class='fas fa-expand-arrows-alt mr-1'></i>" &
                  "Prolonger</button>")

        sb.Append("</div>")

        ' ── Row 2: Values ──────────────────────────────────────
        sb.Append("<div class='row mb-1' style='font-size:.82rem;'>")

        ' Accumulated
        Dim accumVal As Long = 0
        If accum IsNot DBNull.Value Then accumVal = Convert.ToInt64(accum)
        sb.Append("<div class='col-auto'>")
        sb.Append("<small class='text-muted'>Accumul&eacute;&nbsp;</small>")
        sb.Append("<strong>" & accumVal.ToString("N0") & "</strong>")
        sb.Append(" <small class='text-muted'>" &
                  Server.HtmlEncode(unit) & "</small>")
        sb.Append("</div>")

        ' NextDueAt
        If nextDue IsNot DBNull.Value Then
            Dim due As Long = Convert.ToInt64(nextDue)
            Dim remaining As Long = due - accumVal
            sb.Append("<div class='col-auto border-left pl-3'>")
            sb.Append("<small class='text-muted'>Prochaine &eacute;ch.&nbsp;</small>")
            sb.Append("<strong>" & due.ToString("N0") & "</strong>")
            sb.Append(" <small class='text-muted'>" &
                      Server.HtmlEncode(unit) & "</small>")
            If remaining >= 0 Then
                sb.Append("&nbsp;<small class='text-success'>(" &
                          remaining.ToString("N0") & " restant)</small>")
            Else
                sb.Append("&nbsp;<small class='text-danger font-weight-bold'>(" &
                          Math.Abs(remaining).ToString("N0") &
                          " d&eacute;pass&eacute;)</small>")
            End If
            sb.Append("</div>")

            ' ── Progress bar ──────────────────────────────────
            ' Progress = accumulated / nextDue (capped at 100%)
            Dim pct As Integer = 0
            If due > 0 Then
                pct = CInt(Math.Min(100,
                      Math.Round(accumVal * 100.0 / due)))
            End If
            Dim barColor As String = ProgressBarColor(status)
            sb.Append("</div>") ' close row
            sb.Append("<div class='progress mb-1'>")
            sb.Append("<div class='progress-bar " & barColor & "' " &
                      "role='progressbar' style='width:" & pct & "%;' " &
                      "aria-valuenow='" & pct & "' " &
                      "aria-valuemin='0' aria-valuemax='100'></div>")
            sb.Append("</div>")
        Else
            sb.Append("</div>") ' close row
        End If

        ' Ceiling and extension limits
        Dim limitsHtml As New StringBuilder()
        If ceiling IsNot DBNull.Value Then
            limitsHtml.Append("<small class='text-muted mr-2'>" &
                              "<i class='fas fa-skull-crossbones mr-1 text-danger'></i>" &
                              "Plafond&nbsp;" &
                              Convert.ToInt64(ceiling).ToString("N0") & " " &
                              Server.HtmlEncode(unit) & "</small>")
        End If
        If maxExtPct IsNot DBNull.Value Then
            limitsHtml.Append("<small class='text-muted mr-2'>" &
                              "<i class='fas fa-expand-arrows-alt mr-1 text-warning'></i>" &
                              "Ext.max&nbsp;" &
                              Convert.ToDecimal(maxExtPct).ToString("0.#") &
                              "%</small>")
        End If
        If maxExtVal IsNot DBNull.Value Then
            limitsHtml.Append("<small class='text-muted'>" &
                              "/ " & Convert.ToInt64(maxExtVal).ToString("N0") &
                              " " & Server.HtmlEncode(unit) & "</small>")
        End If
        If limitsHtml.Length > 0 Then
            sb.Append("<div class='mt-1'>" & limitsHtml.ToString() & "</div>")
        End If

        body.InnerHtml = sb.ToString()
        card.Controls.Add(body)
        Return card
    End Function

    ' ────────────────────────────────────────────────────────
    ' RENDER HISTORY
    ' RS4: last 20 events
    ' ────────────────────────────────────────────────────────
    Private Sub RenderHistory(ByVal dt As DataTable)
        phHistory.Controls.Clear()

        If dt.Rows.Count = 0 Then
            Dim empty As New HtmlGenericControl("div")
            empty.Attributes("class") = "text-center text-muted py-3 small"
            empty.InnerText = "Aucun &eacute;v&eacute;nement enregistr&eacute;."
            phHistory.Controls.Add(empty)
            Return
        End If

        Dim timeline As New HtmlGenericControl("div")

        For Each row As DataRow In dt.Rows
            Dim evtType As String = SafeStr(row("EventType"))
            Dim evtDate As String = ""
            If row("EventDate") IsNot DBNull.Value Then
                evtDate = Convert.ToDateTime(
                    row("EventDate")).ToString("dd/MM/yyyy")
            End If
            Dim posCode As String = SafeStr(row("PositionCode"))
            Dim tailNo As String = SafeStr(row("TailNo"))
            Dim fhSnap As String = ""
            ' vw_SNHistory uses AcFH_AtEvent (minutes) not AcFH_Minutes
            Dim fhCol As String = If(dt.Columns.Contains("AcFH_AtEvent"), "AcFH_AtEvent",
                                  If(dt.Columns.Contains("AcFH_Minutes"), "AcFH_Minutes", ""))
            If fhCol <> "" AndAlso row(fhCol) IsNot DBNull.Value Then
                Dim mins As Integer = CInt(row(fhCol))
                fhSnap = Math.Round(mins / 60.0, 1).ToString("N1") & " hrs"
            End If

            Dim item As New HtmlGenericControl("div")
            item.Attributes("class") = "d-flex mb-2"

            ' Dot
            Dim dot As New HtmlGenericControl("div")
            dot.Attributes("class") =
                "event-dot " & EventDotClass(evtType)
            item.Controls.Add(dot)

            ' Content
            Dim content As New HtmlGenericControl("div")
            content.Attributes("class") = "ml-2"
            content.Attributes("style") = "font-size:.78rem;"

            Dim html As New StringBuilder()
            html.Append("<span class='badge " &
                        EventBadgeClass(evtType) &
                        " mr-1' style='font-size:.68rem;'>" &
                        Server.HtmlEncode(evtType) & "</span>")
            html.Append("<strong class='text-dark'>" &
                        Server.HtmlEncode(evtDate) & "</strong>")
            If posCode <> "" Then
                html.Append("<br/><small class='text-muted'>" &
                            Server.HtmlEncode(posCode))
                If tailNo <> "" Then
                    html.Append(" &mdash; " & Server.HtmlEncode(tailNo))
                End If
                html.Append("</small>")
            End If
            If fhSnap <> "" Then
                html.Append("<br/><small class='text-primary'>" &
                            "<i class='fas fa-tachometer-alt mr-1'></i>" &
                            Server.HtmlEncode(fhSnap) & "</small>")
            End If

            content.InnerHtml = html.ToString()
            item.Controls.Add(content)
            timeline.Controls.Add(item)
        Next

        phHistory.Controls.Add(timeline)
    End Sub

    ' ────────────────────────────────────────────────────────
    ' EXTENSION - Open modal
    ' ────────────────────────────────────────────────────────
    Protected Sub btnDispatch_Click(sender As Object,
            e As EventArgs) Handles btnDispatch.Click

        Dim action As String = hfAction.Value.Trim().ToLowerInvariant()
        If action <> "openext" Then Return

        Dim tcId As Integer = 0
        Integer.TryParse(hfExtTaskCounterId.Value, tcId)
        If tcId = 0 OrElse _snId = 0 Then Return

        LoadExtensionModal(tcId, _snId)
        hfAction.Value = ""
        ShowModal("extModal")
    End Sub

    Private Sub LoadExtensionModal(ByVal tcId As Integer,
                                    ByVal snId As Integer)
        ' Load TaskCounter info for modal header
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT tc.TaskCounterId, " &
                "       cd.Code AS CounterDefCode, " &
                "       ct.DisplayUnit, " &
                "       tc.MaxExtensionPct, " &
                "       tc.MaxExtensionValue, " &
                "       tc.RepeatInterval, " &
                "       st.NextDueAt, " &
                "       st.AccumulatedSinceLast " &
                "FROM mro2.TaskCounter tc " &
                "INNER JOIN mro2.CounterDef  cd ON cd.CounterDefId  = tc.CounterDefId " &
                "INNER JOIN mro2.CounterType ct ON ct.CounterTypeId = cd.CounterTypeId " &
                "LEFT JOIN mro2.SNTaskCounterState st " &
                "    ON st.TaskCounterId = tc.TaskCounterId " &
                "    AND st.SerializedItemId = @SnId " &
                "WHERE tc.TaskCounterId = @TcId", cn)
                cmd.Parameters.AddWithValue("@TcId", tcId)
                cmd.Parameters.AddWithValue("@SnId", snId)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        Dim defCode As String = SafeStr(rdr("CounterDefCode"))
                        Dim unit As String = SafeStr(rdr("DisplayUnit"))

                        litExtCounterLabel.Text =
                            Server.HtmlEncode(defCode)
                        litExtUnit.Text = unit

                        ' Current due
                        Dim due As String = "-"
                        If rdr("NextDueAt") IsNot DBNull.Value Then
                            due = Convert.ToInt64(
                                rdr("NextDueAt")).ToString("N0") &
                                " " & unit
                        End If
                        litExtCurrentDue.Text = due

                        ' Max allowed extension display
                        Dim maxPct As String = ""
                        Dim maxVal As String = ""
                        If rdr("MaxExtensionPct") IsNot DBNull.Value Then
                            maxPct = Convert.ToDecimal(
                                rdr("MaxExtensionPct")).ToString("0.#") & "%"
                        End If
                        If rdr("MaxExtensionValue") IsNot DBNull.Value Then
                            maxVal = Convert.ToInt64(
                                rdr("MaxExtensionValue")).ToString("N0") &
                                " " & unit
                        End If
                        If maxPct <> "" AndAlso maxVal <> "" Then
                            litExtMaxAllowed.Text = maxPct & " / " & maxVal
                        ElseIf maxPct <> "" Then
                            litExtMaxAllowed.Text = maxPct
                        ElseIf maxVal <> "" Then
                            litExtMaxAllowed.Text = maxVal
                        Else
                            litExtMaxAllowed.Text = "Non d&eacute;finie"
                        End If
                    End If
                End Using
            End Using
        End Using

        ' Reset form fields
        ddlExtType.SelectedIndex = 0
        txtExtValue.Text = ""
        txtExtDocRef.Text = ""
        txtExtApprover.Text = ""
        txtExtNotes.Text = ""
        lblExtError.Visible = False

        ' Load/reload extension reasons
        LoadExtensionReasonDDL()
    End Sub

    Private Sub LoadExtensionDDLs()
        LoadExtensionReasonDDL()
    End Sub

    Private Sub LoadExtensionReasonDDL()
        ddlExtReason.Items.Clear()
        ddlExtReason.Items.Add(
            New ListItem("-- Motif --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT ExtensionReasonId, Code, Name, " &
                "RequiresDocRef, RequiresApprover " &
                "FROM mro2.ExtensionReason " &
                "WHERE IsActive=1 ORDER BY SortOrder", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlExtReason.Items.Add(
                            New ListItem(
                                rdr("Code").ToString() & " - " &
                                rdr("Name").ToString(),
                                rdr("ExtensionReasonId").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    ' ────────────────────────────────────────────────────────
    ' EXTENSION - Save
    ' ────────────────────────────────────────────────────────
    Protected Sub btnSaveExtension_Click(sender As Object,
            e As EventArgs) Handles btnSaveExtension.Click
        lblExtError.Visible = False

        Dim tcId As Integer = 0
        Integer.TryParse(hfExtTaskCounterId.Value, tcId)

        If tcId = 0 OrElse _snId = 0 Then
            ShowExtError("Donn&eacute;es invalides.")
            Return
        End If

        ' Validate ExtensionReason
        If ddlExtReason.SelectedValue = "" Then
            ShowExtError("S&eacute;lectionnez un motif.")
            Return
        End If
        Dim reasonId As Integer = CInt(ddlExtReason.SelectedValue)

        ' Parse extension value
        Dim extValue As Decimal = 0
        If Not Decimal.TryParse(txtExtValue.Text.Trim().Replace(",", "."),
               System.Globalization.NumberStyles.Any,
               System.Globalization.CultureInfo.InvariantCulture,
               extValue) OrElse extValue <= 0 Then
            ShowExtError("Valeur de prolongation invalide.")
            Return
        End If

        Dim extType As String = ddlExtType.SelectedValue

        ' Check RequiresDocRef / RequiresApprover
        Dim reqDocRef As Boolean = False
        Dim reqApprove As Boolean = False
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT RequiresDocRef, RequiresApprover " &
                "FROM mro2.ExtensionReason " &
                "WHERE ExtensionReasonId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", reasonId)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        reqDocRef = Convert.ToBoolean(rdr("RequiresDocRef"))
                        reqApprove = Convert.ToBoolean(rdr("RequiresApprover"))
                    End If
                End Using
            End Using
        End Using

        If reqDocRef AndAlso txtExtDocRef.Text.Trim() = "" Then
            ShowExtError("Ce motif n&eacute;cessite une r&eacute;f&eacute;rence documentaire.")
            Return
        End If
        If reqApprove AndAlso txtExtApprover.Text.Trim() = "" Then
            ShowExtError("Ce motif n&eacute;cessite un approbateur.")
            Return
        End If

        Dim userId As String =
            If(Session("UserId") IsNot Nothing,
               Session("UserId").ToString(), "admin")

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand(
                        "mro2.usp_SNTaskCounterExtension_Grant", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@SerializedItemId", _snId)
                    cmd.Parameters.AddWithValue("@TaskCounterId", tcId)
                    cmd.Parameters.AddWithValue("@ExtensionReasonId", CType(reasonId, Object))
                    cmd.Parameters.AddWithValue("@ExtensionType", extType)
                    cmd.Parameters.AddWithValue("@ExtensionValue", extValue)
                    cmd.Parameters.AddWithValue("@Justification",
                        If(txtExtNotes.Text.Trim() = "",
                           CType(DBNull.Value, Object),
                           txtExtNotes.Text.Trim()))
                    cmd.Parameters.AddWithValue("@DocReference",
                        If(txtExtDocRef.Text.Trim() = "",
                           CType(DBNull.Value, Object),
                           txtExtDocRef.Text.Trim()))
                    cmd.Parameters.AddWithValue("@ApprovedBy",
                        If(txtExtApprover.Text.Trim() = "",
                           CType(DBNull.Value, Object),
                           txtExtApprover.Text.Trim()))
                    cmd.Parameters.AddWithValue("@ApprovalDate", Date.Today)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    cn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            HideModal("extModal")
            ' Reload page data to reflect new extension
            LoadPage()
            ShowToast("Prolongation accord&eacute;e.", "success")

        Catch ex As SqlException
            ShowExtError(Server.HtmlEncode(ex.Message))
        Catch ex As Exception
            ShowExtError(Server.HtmlEncode(ex.Message))
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DISPLAY HELPERS
    ' ────────────────────────────────────────────────────────
    Private Function SNStatusBadge(ByVal status As String) As String
        Select Case status.ToUpperInvariant()
            Case "SERVICEABLE", "ACTIVE"
                Return "<span class='badge badge-success'>" &
                       Server.HtmlEncode(status) & "</span>"
            Case "UNSERVICEABLE"
                Return "<span class='badge badge-danger'>" &
                       Server.HtmlEncode(status) & "</span>"
            Case "IN_REPAIR"
                Return "<span class='badge badge-warning text-dark'>" &
                       Server.HtmlEncode(status) & "</span>"
            Case Else
                Return "<span class='badge badge-secondary'>" &
                       Server.HtmlEncode(status) & "</span>"
        End Select
    End Function

    Private Function CounterStatusBadge(ByVal status As String) As String
        Select Case status.ToUpperInvariant()
            Case "EXPIRED"
                Return "<span class='badge badge-danger'>" &
                       "<i class='fas fa-skull-crossbones mr-1'></i>" &
                       "EXPIR&Eacute;</span>"
            Case "OVERDUE"
                Return "<span class='badge badge-orange' " &
                       "style='background:#fd7e14;color:#fff;'>" &
                       "<i class='fas fa-exclamation-circle mr-1'></i>" &
                       "D&Eacute;PASS&Eacute;</span>"
            Case "DUE"
                Return "<span class='badge badge-warning text-dark'>" &
                       "<i class='fas fa-exclamation-triangle mr-1'></i>" &
                       "&Agrave; FAIRE</span>"
            Case "ALERT"
                Return "<span class='badge badge-info'>" &
                       "<i class='fas fa-bell mr-1'></i>ALERTE</span>"
            Case "COMPLETE"
                Return "<span class='badge badge-secondary'>" &
                       "<i class='fas fa-check-double mr-1'></i>" &
                       "TERMIN&Eacute;</span>"
            Case Else
                Return "<span class='badge badge-success'>" &
                       "<i class='fas fa-check mr-1'></i>OK</span>"
        End Select
    End Function

    Private Function StatusRank(ByVal s As String) As Integer
        Select Case s.ToUpperInvariant()
            Case "EXPIRED", "OVERDUE" : Return 4
            Case "DUE" : Return 3
            Case "ALERT" : Return 2
            Case Else : Return 0  ' OK / unknown
        End Select
    End Function

    Private Function HealthBadgeLarge(ByVal health As String) As String
        Select Case health.ToUpperInvariant()
            Case "EXPIRED"
                Return "<span class='badge badge-danger' " &
                       "style='font-size:.9rem;padding:.4rem .7rem;'>" &
                       "<i class='fas fa-skull-crossbones mr-1'></i>" &
                       "EXPIR&Eacute;</span>"
            Case "DUE", "OVERDUE"
                Return "<span class='badge badge-warning text-dark' " &
                       "style='font-size:.9rem;padding:.4rem .7rem;'>" &
                       "<i class='fas fa-exclamation-triangle mr-1'></i>" &
                       "&Agrave; FAIRE</span>"
            Case "ALERT"
                Return "<span class='badge badge-info' " &
                       "style='font-size:.9rem;padding:.4rem .7rem;'>" &
                       "<i class='fas fa-bell mr-1'></i>ALERTE</span>"
            Case "NO_DATA"
                Return "<span class='badge badge-light border' " &
                       "style='font-size:.9rem;padding:.4rem .7rem;'>" &
                       "Sans compteur</span>"
            Case Else
                Return "<span class='badge badge-success' " &
                       "style='font-size:.9rem;padding:.4rem .7rem;'>" &
                       "<i class='fas fa-check mr-1'></i>OK</span>"
        End Select
    End Function

    Private Function ProgressBarColor(ByVal status As String) As String
        Select Case status.ToUpperInvariant()
            Case "EXPIRED", "OVERDUE" : Return "bg-danger"
            Case "DUE" : Return "bg-warning"
            Case "ALERT" : Return "bg-info"
            Case "COMPLETE" : Return "bg-secondary"
            Case Else : Return "bg-success"
        End Select
    End Function

    Private Function EventDotClass(ByVal evtType As String) As String
        Select Case evtType.ToUpperInvariant()
            Case "INSTALL" : Return "bg-success"
            Case "REMOVE" : Return "bg-danger"
            Case "TRANSFER" : Return "bg-info"
            Case "INSPECT" : Return "bg-warning"
            Case Else : Return "bg-secondary"
        End Select
    End Function

    Private Function EventBadgeClass(ByVal evtType As String) As String
        Select Case evtType.ToUpperInvariant()
            Case "INSTALL" : Return "badge-success"
            Case "REMOVE" : Return "badge-danger"
            Case "TRANSFER" : Return "badge-info"
            Case "INSPECT" : Return "badge-warning text-dark"
            Case Else : Return "badge-secondary"
        End Select
    End Function

    ' ────────────────────────────────────────────────────────
    ' UTILITIES
    ' ────────────────────────────────────────────────────────
    Private Function SafeStr(ByVal o As Object) As String
        If o Is Nothing OrElse o Is DBNull.Value Then Return ""
        Return o.ToString()
    End Function

    Private Sub ShowExtError(ByVal msg As String)
        lblExtError.Text = msg
        lblExtError.Visible = True
        ShowModal("extModal")
    End Sub

    Private Sub ShowModal(ByVal id As String)
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "show_" & id & "_" & Guid.NewGuid().ToString("N"),
            "$('#" & id & "').modal('show');", True)
    End Sub

    Private Sub HideModal(ByVal id As String)
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "hide_" & id & "_" & Guid.NewGuid().ToString("N"),
            "$('#" & id & "').modal('hide');", True)
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