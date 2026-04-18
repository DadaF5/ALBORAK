Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Text

' ============================================================
' MRO2/Setup/Default.aspx.vb
' Setup landing page — loads row counts for all lookup tables
' and flags any that are empty (configuration warnings).
' ============================================================
Partial Class MRO2_Setup_Default
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    ' ────────────────────────────────────────────────────────
    ' PAGE LOAD
    ' ────────────────────────────────────────────────────────
    Protected Sub Page_Load(sender As Object, e As EventArgs) _
            Handles Me.Load, btnRefresh.Click
        LoadCounts()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' LOAD ALL ROW COUNTS — single query, minimal round trips
    ' ────────────────────────────────────────────────────────
    Private Sub LoadCounts()

        ' One SQL batch — all counts in a single round trip
        Dim sql As String =
            "SELECT " &
            "  (SELECT COUNT(*) FROM mro2.CounterType      WHERE IsActive=1) AS CT,  " &
            "  (SELECT COUNT(*) FROM mro2.CounterDef       WHERE IsActive=1) AS CD,  " &
            "  (SELECT COUNT(*) FROM mro2.CounterBasis     WHERE IsActive=1) AS CB,  " &
            "  (SELECT COUNT(*) FROM mro2.ComputationReference WHERE IsActive=1) AS CR, " &
            "  (SELECT COUNT(*) FROM mro2.CounterReference  WHERE IsActive=1) AS CRef, " &
            "  (SELECT COUNT(*) FROM mro2.LimitType         WHERE IsActive=1) AS LT,  " &
            "  (SELECT COUNT(*) FROM mro2.ExtensionReason   WHERE IsActive=1) AS ER,  " &
            "  (SELECT COUNT(*) FROM mro2.PartNumber        WHERE IsActive=1) AS PN,  " &
            "  (SELECT COUNT(*) FROM mro2.SerializedItem    WHERE IsActive=1) AS SN,  " &
            "  (SELECT COUNT(*) FROM mro2.TaskCounter       WHERE IsActive=1) AS TC,  " &
            "  (SELECT COUNT(*) FROM mro2.AcPositionTemplate WHERE IsActive=1) AS TPL, " &
            "  (SELECT COUNT(*) FROM mro2.AcPosition        WHERE IsActive=1) AS POS"

        Dim ct, cd, cb, cr, cref, lt, er, pn, sn, tc, tpl, pos As Integer

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand(sql, cn)
                    cn.Open()
                    Using rdr As SqlDataReader = cmd.ExecuteReader()
                        If rdr.Read() Then
                            ct   = CInt(rdr("CT"))
                            cd   = CInt(rdr("CD"))
                            cb   = CInt(rdr("CB"))
                            cr   = CInt(rdr("CR"))
                            cref = CInt(rdr("CRef"))
                            lt   = CInt(rdr("LT"))
                            er   = CInt(rdr("ER"))
                            pn   = CInt(rdr("PN"))
                            sn   = CInt(rdr("SN"))
                            tc   = CInt(rdr("TC"))
                            tpl  = CInt(rdr("TPL"))
                            pos  = CInt(rdr("POS"))
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ' If DB unreachable show dashes — don't crash the page
            SetAllDashes()
            Return
        End Try

        ' ── Bind literals ─────────────────────────────────
        litCounterTypeCount.Text  = CountBadge(ct,  5,  "types")
        litCounterDefCount.Text   = CountBadge(cd,  8,  "d&eacute;finitions")
        litCounterBasisCount.Text = CountBadge(cb,  4,  "bases")
        litCompRefCount.Text      = CountBadge(cr,  6,  "r&eacute;f&eacute;rences")
        litCounterRefCount.Text   = CountBadge(cref,8,  "r&eacute;f&eacute;rences")
        litLimitTypeCount.Text    = CountBadge(lt,  4,  "types")
        litExtReasonCount.Text    = CountBadge(er,  5,  "motifs")
        litPNCount.Text           = CountBadge(pn,  1,  "PN", False)
        litSNCount.Text           = CountBadge(sn,  1,  "SN", False)
        litTaskCounterCount.Text  = CountBadge(tc,  0,  "compteurs t&acirc;ches", False)
        litTemplateCount.Text     = CountBadge(tpl, 0,  "positions gabarit", False)
        litPositionCount.Text     = CountBadge(pos, 0,  "positions", False)

        ' ── Configuration warnings ─────────────────────────
        Dim warnings As New StringBuilder()

        ' Mandatory lookup tables — warn if below minimum seed count
        If ct   < 5  Then warnings.Append(" Types de Compteurs,")
        If cd   < 8  Then warnings.Append(" D&eacute;finitions Compteurs,")
        If cb   < 4  Then warnings.Append(" Bases de Comptage,")
        If lt   < 4  Then warnings.Append(" Types de Limites,")
        If er   < 5  Then warnings.Append(" Motifs de Prolongation,")

        ' Operational tables — warn if zero
        If pn   = 0  Then warnings.Append(" Part Numbers,")

        If warnings.Length > 0 Then
            ' Trim trailing comma
            Dim w As String = warnings.ToString().TrimEnd(","c)
            litWarnings.Text  = w & "."
            pnlWarnings.Visible = True
        Else
            pnlWarnings.Visible = False
        End If
    End Sub

    ' ────────────────────────────────────────────────────────
    ' HELPERS
    ' ────────────────────────────────────────────────────────

    ' Renders a colored count badge.
    ' minExpected: below this → orange warning badge
    ' checkMin: False for user-data tables (no minimum expected)
    Private Function CountBadge(ByVal count As Integer,
                                 ByVal minExpected As Integer,
                                 ByVal label As String,
                                 Optional ByVal checkMin As Boolean = True) As String
        Dim badgeClass As String

        If count = 0 Then
            badgeClass = "badge-secondary"
        ElseIf checkMin AndAlso count < minExpected Then
            badgeClass = "badge-warning text-dark"
        Else
            badgeClass = "badge-success"
        End If

        Return "<span class='badge " & badgeClass & "'>" &
               count.ToString() & " " & label & "</span>"
    End Function

    Private Sub SetAllDashes()
        litCounterTypeCount.Text = "<span class='text-muted'>-</span>"
        litCounterDefCount.Text = "<span class='text-muted'>-</span>"
        litCounterBasisCount.Text = "<span class='text-muted'>-</span>"
        litCompRefCount.Text = "<span class='text-muted'>-</span>"
        litCounterRefCount.Text = "<span class='text-muted'>-</span>"
        litLimitTypeCount.Text = "<span class='text-muted'>-</span>"
        litExtReasonCount.Text = "<span class='text-muted'>-</span>"
        litPNCount.Text = "<span class='text-muted'>-</span>"
        litSNCount.Text = "<span class='text-muted'>-</span>"
        litTaskCounterCount.Text = "<span class='text-muted'>-</span>"
        litTemplateCount.Text = "<span class='text-muted'>-</span>"
        litPositionCount.Text = "<span class='text-muted'>-</span>"
    End Sub

End Class
