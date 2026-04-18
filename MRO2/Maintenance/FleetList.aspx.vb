Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

' ============================================================
' MRO2/Maintenance/FleetList.aspx.vb
'
' Fleet overview — one row per aircraft.
' Joins tblAircraft with:
'   - mro2.AcCounter       → current FH / FC per tail
'   - mro2.vw_DueList summary → Expired / Due / Alert counts
'
' Single SP call: mro2.usp_DueList_GetSummary returns one row
' per aircraft with health counts. This is joined in-memory
' with the aircraft master data from tblAircraft.
'
' AcMainGroup filter: driven by ddlFilterGroup DDL.
' Issues-only checkbox: hides aircraft with zero problems.
' ============================================================
Partial Class MRO2_Maintenance_FleetList
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    Private Property SortColumn As String
        Get
            Dim val = TryCast(ViewState("SC"), String)
            Return If(String.IsNullOrEmpty(val), "AircraftHealth", val)
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
            SortColumn = "AircraftHealth"
            SortDir    = "ASC"
            LoadGroupFilter()
            BindGrid()
        End If
    End Sub

    ' ────────────────────────────────────────────────────────
    ' FILTER EVENTS
    ' ────────────────────────────────────────────────────────
    Protected Sub ddlFilterGroup_Changed(sender As Object, e As EventArgs) _
            Handles ddlFilterGroup.SelectedIndexChanged
        BindGrid()
    End Sub

    Protected Sub chkIssuesOnly_CheckedChanged(sender As Object, e As EventArgs) _
            Handles chkIssuesOnly.CheckedChanged
        BindGrid()
    End Sub

    Protected Sub gvFleet_Sorting(sender As Object,
            e As System.Web.UI.WebControls.GridViewSortEventArgs) _
            Handles gvFleet.Sorting
        SortDir    = If(SortColumn = e.SortExpression _
                        AndAlso SortDir = "ASC", "DESC", "ASC")
        SortColumn = e.SortExpression
        BindGrid()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' LOAD GROUP FILTER DDL
    ' ────────────────────────────────────────────────────────
    Private Sub LoadGroupFilter()
        ddlFilterGroup.Items.Clear()
        ddlFilterGroup.Items.Add(
            New System.Web.UI.WebControls.ListItem("-- Tous les groupes --", ""))

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT AcMainGroupID, AcMainGroup " &
                "FROM dbo.tblAcMainGroup " &
                "ORDER BY AcMainGroup", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlFilterGroup.Items.Add(
                            New System.Web.UI.WebControls.ListItem(
                                rdr("AcMainGroup").ToString(),
                                rdr("AcMainGroupID").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    ' ────────────────────────────────────────────────────────
    ' BIND GRID
    ' Builds a DataTable combining:
    '   - tblAircraft master data + FH/FC from AcCounter
    '   - Due list health summary from usp_DueList_GetSummary
    ' Joined in-memory on AcID.
    ' ────────────────────────────────────────────────────────
    Private Sub BindGrid()

        ' ── 1. Aircraft master + current counters ─────────
        Dim dtAc As New DataTable()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT " &
                "  a.AcID, " &
                "  a.TailNo, " &
                "  t.AcType   AS AcTypeName, " &
                "  g.AcMainGroup AS AcMainGroupName, " &
                "  g.AcMainGroupID, " &
                "  ISNULL( " &
                "    (SELECT TOP 1 " &
                "       CAST(ac.CurrentValue / 60.0 AS DECIMAL(10,1)) " &
                "     FROM mro2.AcCounter ac " &
                "     INNER JOIN mro2.CounterDef cd " &
                "       ON cd.CounterDefId = ac.CounterDefId " &
                "       AND cd.Code = 'AF_FLIGHT_MIN' " &
                "     WHERE ac.AcID = a.AcID), 0) AS FH_Display, " &
                "  ISNULL( " &
                "    (SELECT TOP 1 ac.CurrentValue " &
                "     FROM mro2.AcCounter ac " &
                "     INNER JOIN mro2.CounterDef cd " &
                "       ON cd.CounterDefId = ac.CounterDefId " &
                "       AND cd.Code = 'AF_CYCLES' " &
                "     WHERE ac.AcID = a.AcID), 0) AS FC_Display " &
                "FROM dbo.tblAircraft a " &
                "INNER JOIN dbo.tblAcType      t ON t.AcTypeId      = a.AcTypeID " &
                "INNER JOIN dbo.tblAcMainGroup g ON g.AcMainGroupID = a.AcMainGroupID " &
                "WHERE a.Active = 1 " &
                "ORDER BY g.AcMainGroup, a.TailNo", cn)
                cn.Open()
                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dtAc)
                End Using
            End Using
        End Using

        ' ── 2. Health summary from SP ─────────────────────
        Dim dtHealth As New DataTable()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                    "mro2.usp_DueList_GetSummary", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cn.Open()
                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dtHealth)
                End Using
            End Using
        End Using

        ' ── 3. Build combined DataTable ───────────────────
        Dim dt As New DataTable()
        dt.Columns.Add("AcID", GetType(Integer))
        dt.Columns.Add("TailNo", GetType(String))
        dt.Columns.Add("AcTypeName", GetType(String))
        dt.Columns.Add("AcMainGroupName", GetType(String))
        dt.Columns.Add("AcMainGroupID", GetType(Integer))
        dt.Columns.Add("FH_Display", GetType(Decimal))
        dt.Columns.Add("FC_Display", GetType(Integer))
        dt.Columns.Add("Expired", GetType(Integer))
        dt.Columns.Add("Due", GetType(Integer))
        dt.Columns.Add("Alert", GetType(Integer))
        dt.Columns.Add("AircraftHealth", GetType(String))
        ' Numeric rank for sorting (EXPIRED=0 worst first)
        dt.Columns.Add("HealthRank", GetType(Integer))

        ' Index health rows by AcID for O(1) lookup
        Dim healthIdx As New Dictionary(Of Integer, DataRow)()
        For Each hr As DataRow In dtHealth.Rows
            Dim hAcId As Integer = CInt(hr("AcID"))
            If Not healthIdx.ContainsKey(hAcId) Then
                healthIdx(hAcId) = hr
            End If
        Next

        For Each acRow As DataRow In dtAc.Rows
            Dim acId As Integer = CInt(acRow("AcID"))

            Dim expired As Integer = 0
            Dim due As Integer = 0
            Dim alert As Integer = 0
            Dim health As String = "OK"

            If healthIdx.ContainsKey(acId) Then
                Dim hr As DataRow = healthIdx(acId)
                expired = CInt(hr("Expired"))
                due = CInt(hr("Due"))
                alert = CInt(hr("Alert"))
                health = hr("AircraftHealth").ToString()
            End If

            Dim rank As Integer
            Select Case health
                Case "EXPIRED" : rank = 0
                Case "DUE" : rank = 1
                Case "ALERT" : rank = 2
                Case Else : rank = 3
            End Select

            dt.Rows.Add(
                acId,
                acRow("TailNo"),
                acRow("AcTypeName"),
                acRow("AcMainGroupName"),
                CInt(acRow("AcMainGroupID")),
                CDec(acRow("FH_Display")),
                CInt(acRow("FC_Display")),
                expired, due, alert,
                health, rank)
        Next

        ' ── 4. Apply filters ──────────────────────────────
        Dim filterParts As New List(Of String)()

        ' Group filter
        If ddlFilterGroup.SelectedValue <> "" Then
            filterParts.Add("AcMainGroupID = " & ddlFilterGroup.SelectedValue)
        End If

        ' Issues-only filter
        If chkIssuesOnly.Checked Then
            filterParts.Add("(Expired > 0 OR Due > 0 OR Alert > 0)")
        End If

        Dim filterExpr As String = If(filterParts.Count > 0,
            String.Join(" AND ", filterParts), "")

        Dim sortExpr As String = "HealthRank ASC, TailNo ASC"
        If dt.Columns.Contains(SortColumn) Then
            sortExpr = SortColumn & " " & SortDir
        End If

        Dim view As DataView = dt.DefaultView
        view.RowFilter = filterExpr
        view.Sort = sortExpr

        Dim dtFiltered As DataTable = view.ToTable()

        ' ── 5. Totals for info-boxes ──────────────────────
        Dim totalExpired As Integer = 0
        Dim totalDue As Integer = 0
        Dim totalAlert As Integer = 0

        For Each row As DataRow In dtFiltered.Rows
            totalExpired += CInt(row("Expired"))
            totalDue += CInt(row("Due"))
            totalAlert += CInt(row("Alert"))
        Next

        litTotalExpired.Text = totalExpired.ToString("N0")
        litTotalDue.Text = totalDue.ToString("N0")
        litTotalAlert.Text = totalAlert.ToString("N0")
        litTotalAc.Text = dtFiltered.Rows.Count.ToString()
        litRowCount.Text = dtFiltered.Rows.Count.ToString()

        ' ── 6. Bind grid ──────────────────────────────────
        gvFleet.DataSource = dtFiltered
        gvFleet.DataBind()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DISPLAY HELPERS
    ' ────────────────────────────────────────────────────────
    Protected Function HealthBadge(ByVal health As String) As String
        Select Case health.ToUpperInvariant()
            Case "EXPIRED"
                Return "<span class='badge badge-danger' style='font-size:.82rem;'>" &
                       "<i class='fas fa-skull-crossbones mr-1'></i>EXPIR&Eacute;</span>"
            Case "DUE"
                Return "<span class='badge badge-warning text-dark' style='font-size:.82rem;'>" &
                       "<i class='fas fa-exclamation-triangle mr-1'></i>&Agrave; FAIRE</span>"
            Case "ALERT"
                Return "<span class='badge badge-info' style='font-size:.82rem;'>" &
                       "<i class='fas fa-bell mr-1'></i>ALERTE</span>"
            Case Else
                Return "<span class='badge badge-success' style='font-size:.82rem;'>" &
                       "<i class='fas fa-check mr-1'></i>OK</span>"
        End Select
    End Function

    ' Renders a count cell — grey dash when zero, colored badge when > 0
    Protected Function CountCell(ByVal value As Object,
                                  ByVal color As String) As String
        If value Is Nothing OrElse value Is DBNull.Value Then
            Return "<span class='text-muted'>-</span>"
        End If
        Dim n As Integer = CInt(value)
        If n = 0 Then
            Return "<span class='text-muted'>-</span>"
        End If
        Dim textClass As String = If(color = "warning", " text-dark", "")
        Return "<span class='badge badge-" & color & textClass &
               "' style='font-size:.82rem;'>" & n & "</span>"
    End Function

End Class
