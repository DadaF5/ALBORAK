Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

' ============================================================
' MRO2/Setup/Components/PartNumberLimitDetail.aspx.vb
'
' Manages TaskCounter rows for one PNLimit.
' URL: ?PNLimitId=x
'
' One PNLimit can have multiple TaskCounter rows (FH, FC,
' Calendar...) - OR logic: first line to reach threshold
' triggers the task.
'
' Fields per TaskCounter:
'   CounterDefId, CounterBasisId,
'   FirstThreshold (INT minutes or count),
'   RepeatInterval (NULL = one-time task),
'   Ceiling (NULL = no lifetime cap),
'   AlertThresholdPct (1-99),
'   MaxExtensionPct (NULL = no % extension),
'   MaxExtensionValue (NULL = no value extension)
' ============================================================
Partial Class MRO2_Setup_Components_PartNumberLimitDetail
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    Private _pnLimitId As Integer = 0

    ' ────────────────────────────────────────────────────────
    ' PAGE LOAD
    ' ────────────────────────────────────────────────────────
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Integer.TryParse(Request.QueryString("PNLimitId"), _pnLimitId)

        If _pnLimitId = 0 Then
            pnlNotFound.Visible = True
            pnlMain.Visible     = False
            Return
        End If

        pnlNotFound.Visible = False
        pnlMain.Visible     = True

        If Not IsPostBack Then
            LoadHeader()
            LoadCounterTypeDDL()
            LoadCounterBasisDDL()
            ResetForm()
            BindGrid()
        End If
    End Sub

    ' ────────────────────────────────────────────────────────
    ' HEADER - PN info + limit summary
    ' ────────────────────────────────────────────────────────
    Private Sub LoadHeader()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT pn.PN, " &
                "       ISNULL(pn.Nomenclature,'') AS Nomenclature, " &
                "       ISNULL(ata.ATACode,'')     AS ATACode, " &
                "       lt.Code  AS LimitTypeCode, " &
                "       lt.BadgeColor, " &
                "       pl.HardLimit, " &
                "       pl.AlertThresholdPct " &
                "FROM mro2.PNLimit pl " &
                "INNER JOIN mro2.PartNumber pn " &
                "    ON pn.PartNumberId = pl.PartNumberId " &
                "LEFT  JOIN mro2.ATA      ata " &
                "    ON ata.ATAId = pn.ATAId " &
                "LEFT  JOIN mro2.LimitType lt " &
                "    ON lt.LimitTypeId = pl.LimitTypeId " &
                "WHERE pl.PNLimitId = @Id", cn)
                cmd.Parameters.AddWithValue("@Id", _pnLimitId)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        litPN.Text           = Server.HtmlEncode(rdr("PN").ToString())
                        litNomenclature.Text = Server.HtmlEncode(
                                                   rdr("Nomenclature").ToString())
                        Dim ataCode As String = rdr("ATACode").ToString()
                        litATA.Text = If(ataCode <> "",
                            "ATA " & Server.HtmlEncode(ataCode), "")

                        Dim ltCode  As String = SafeStr(rdr("LimitTypeCode"))
                        Dim ltColor As String = SafeStr(rdr("BadgeColor"))
                        If ltCode <> "" Then
                            litLimitTypeBadge.Text =
                                "<span class='badge badge-" &
                                Server.HtmlEncode(ltColor) & "' " &
                                "style='font-size:.82rem;'>" &
                                Server.HtmlEncode(ltCode) & "</span>"
                        Else
                            litLimitTypeBadge.Text =
                                "<span class='text-muted'>-</span>"
                        End If

                        litHardLimit.Text =
                           If(rdr("HardLimit") Is DBNull.Value, "-",
                              Convert.ToDecimal(rdr("HardLimit")).ToString("N0"))
                        litAlertPct.Text =
                            If(rdr("AlertThresholdPct") Is DBNull.Value, "-",
                               rdr("AlertThresholdPct").ToString() & "%")
                    Else
                        pnlNotFound.Visible = True
                        pnlMain.Visible     = False
                    End If
                End Using
            End Using
        End Using
    End Sub

    ' ────────────────────────────────────────────────────────
    ' BIND GRID
    ' ────────────────────────────────────────────────────────
    Private Sub BindGrid()
        Dim dt As New DataTable()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT tc.TaskCounterId, " &
                "       cd.Code     AS CounterDefCode, " &
                "       cd.Name     AS CounterDefName, " &
                "       ct.DisplayUnit, " &
                "       ct.UnitStorage, " &
                "       cb.Code     AS CounterBasisCode, " &
                "       tc.FirstThreshold, " &
                "       tc.RepeatInterval, " &
                "       tc.Ceiling, " &
                "       tc.AlertThresholdPct, " &
                "       tc.MaxExtensionPct, " &
                "       tc.MaxExtensionValue, " &
                "       tc.IsActive " &
                "FROM mro2.TaskCounter tc " &
                "INNER JOIN mro2.CounterDef  cd " &
                "    ON cd.CounterDefId  = tc.CounterDefId " &
                "INNER JOIN mro2.CounterType ct " &
                "    ON ct.CounterTypeId = cd.CounterTypeId " &
                "INNER JOIN mro2.CounterBasis cb " &
                "    ON cb.CounterBasisId = tc.CounterBasisId " &
                "WHERE tc.PNLimitId = @Id " &
                "ORDER BY tc.TaskCounterId", cn)
                cmd.Parameters.AddWithValue("@Id", _pnLimitId)
                cn.Open()
                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using
        litTCCount.Text    = dt.Rows.Count.ToString()
        gvTC.DataSource    = dt
        gvTC.DataBind()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' GRID ROW COMMANDS
    ' ────────────────────────────────────────────────────────
    Protected Sub gvTC_RowCommand(sender As Object,
            e As GridViewCommandEventArgs) Handles gvTC.RowCommand

        Dim tcId As Integer = Convert.ToInt32(e.CommandArgument)

        Select Case e.CommandName
            Case "EditTC"
                LoadTCForEdit(tcId)
                litFormTitle.Text = "Modifier le compteur"
                lblError.Visible  = False
                BindGrid()

            Case "ToggleTC"
                ToggleTCActive(tcId)
                BindGrid()
        End Select
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DDL LOADERS
    ' ────────────────────────────────────────────────────────
    Private Sub LoadCounterTypeDDL()
        Dim saved As String = ddlCounterType.SelectedValue
        ddlCounterType.Items.Clear()
        ddlCounterType.Items.Add(
            New System.Web.UI.WebControls.ListItem("-- Type --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT CounterTypeId, Code, DisplayUnit " &
                "FROM mro2.CounterType WHERE IsActive=1 " &
                "ORDER BY SortOrder, Code", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlCounterType.Items.Add(
                            New System.Web.UI.WebControls.ListItem(
                                rdr("Code").ToString() & " [" &
                                rdr("DisplayUnit").ToString() & "]",
                                rdr("CounterTypeId").ToString()))
                    End While
                End Using
            End Using
        End Using
        If ddlCounterType.Items.FindByValue(saved) IsNot Nothing Then
            ddlCounterType.SelectedValue = saved
        End If
        LoadCounterDefDDL(ddlCounterType.SelectedValue)
    End Sub

    Private Sub LoadCounterDefDDL(ByVal typeIdStr As String)
        ddlCounterDef.Items.Clear()
        ddlCounterDef.Items.Add(
            New System.Web.UI.WebControls.ListItem("-- Compteur --", ""))
        If typeIdStr = "" Then Return
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT CounterDefId, Code, Name, UnitStorage " &
                "FROM mro2.CounterDef " &
                "WHERE CounterTypeId=@TypeId AND IsActive=1 " &
                "ORDER BY SortOrder, Code", cn)
                cmd.Parameters.AddWithValue("@TypeId", CInt(typeIdStr))
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlCounterDef.Items.Add(
                            New System.Web.UI.WebControls.ListItem(
                                rdr("Code").ToString() & " - " &
                                rdr("Name").ToString(),
                                rdr("CounterDefId").ToString()))
                    End While
                End Using
            End Using
        End Using
        UpdateUnitLiteral()
    End Sub

    Private Sub LoadCounterBasisDDL()
        ddlCounterBasis.Items.Clear()
        ddlCounterBasis.Items.Add(
            New System.Web.UI.WebControls.ListItem("-- Base --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT CounterBasisId, Code, Name " &
                "FROM mro2.CounterBasis WHERE IsActive=1 " &
                "ORDER BY SortOrder", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlCounterBasis.Items.Add(
                            New System.Web.UI.WebControls.ListItem(
                                rdr("Code").ToString() & " - " &
                                rdr("Name").ToString(),
                                rdr("CounterBasisId").ToString()))
                    End While
                End Using
            End Using
        End Using
        ' Default SINCE_NEW
        For Each item As System.Web.UI.WebControls.ListItem In ddlCounterBasis.Items
            If item.Text.StartsWith("SINCE_NEW") Then
                ddlCounterBasis.SelectedValue = item.Value
                Exit For
            End If
        Next
    End Sub

    Private Sub UpdateUnitLiteral()
        Dim unit As String = "-"
        If ddlCounterType.SelectedValue <> "" Then
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand(
                    "SELECT DisplayUnit FROM mro2.CounterType " &
                    "WHERE CounterTypeId=@Id", cn)
                    cmd.Parameters.AddWithValue("@Id",
                        CInt(ddlCounterType.SelectedValue))
                    cn.Open()
                    Dim o As Object = cmd.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                        unit = o.ToString()
                    End If
                End Using
            End Using
        End If
        litUnit.Text  = unit
        litUnit2.Text = unit
        litUnit3.Text = unit
        litUnit4.Text = unit
    End Sub

    ' ────────────────────────────────────────────────────────
    ' CASCADE: CounterType → CounterDef
    ' ────────────────────────────────────────────────────────
    Protected Sub ddlCounterType_Changed(sender As Object,
            e As EventArgs) Handles ddlCounterType.SelectedIndexChanged
        LoadCounterDefDDL(ddlCounterType.SelectedValue)
        ' Rebind grid so modal stays populated
        BindGrid()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' SAVE TASK COUNTER
    ' ────────────────────────────────────────────────────────
    Protected Sub btnSaveTC_Click(sender As Object,
            e As EventArgs) Handles btnSaveTC.Click
        lblError.Visible = False

        ' Validate required fields
        If ddlCounterType.SelectedValue = "" Then
            ShowError("S&eacute;lectionnez un type de compteur.")
            Return
        End If
        If ddlCounterDef.SelectedValue = "" Then
            ShowError("S&eacute;lectionnez un compteur.")
            Return
        End If
        If ddlCounterBasis.SelectedValue = "" Then
            ShowError("S&eacute;lectionnez une base de comptage.")
            Return
        End If

        ' Parse FirstThreshold - required
        Dim firstThr As Integer = 0
        If Not Integer.TryParse(txtFirstThreshold.Text.Trim(), firstThr) _
           OrElse firstThr <= 0 Then
            ShowError("1&egrave;re &eacute;ch&eacute;ance : entrez un entier positif.")
            Return
        End If

        ' Parse RepeatInterval - optional (empty = one-time)
        Dim repeatObj As Object = DBNull.Value
        If txtRepeatInterval.Text.Trim() <> "" Then
            Dim ri As Integer = 0
            If Not Integer.TryParse(txtRepeatInterval.Text.Trim(), ri) _
               OrElse ri <= 0 Then
                ShowError("Intervalle : entrez un entier positif ou laissez vide.")
                Return
            End If
            repeatObj = ri
        End If

        ' Parse Ceiling - optional
        Dim ceilObj As Object = DBNull.Value
        If txtCeiling.Text.Trim() <> "" Then
            Dim ceil As Integer = 0
            If Not Integer.TryParse(txtCeiling.Text.Trim(), ceil) _
               OrElse ceil <= 0 Then
                ShowError("Plafond vie : entrez un entier positif ou laissez vide.")
                Return
            End If
            ceilObj = ceil
        End If

        ' Parse AlertThresholdPct - required 1-99
        Dim alertPct As Byte = 90
        If Not Byte.TryParse(txtAlertPct.Text.Trim(), alertPct) _
           OrElse alertPct < 1 OrElse alertPct > 99 Then
            ShowError("Alerte % : entrez une valeur entre 1 et 99.")
            Return
        End If

        ' Parse MaxExtensionPct - optional
        Dim maxExtPctObj As Object = DBNull.Value
        If txtMaxExtPct.Text.Trim() <> "" Then
            Dim p As Decimal = 0
            If Not Decimal.TryParse(txtMaxExtPct.Text.Trim(),
                   System.Globalization.NumberStyles.Any,
                   System.Globalization.CultureInfo.InvariantCulture, p) _
               OrElse p <= 0 Then
                ShowError("Extension % : entrez un nombre positif ou laissez vide.")
                Return
            End If
            maxExtPctObj = p
        End If

        ' Parse MaxExtensionValue - optional
        Dim maxExtValObj As Object = DBNull.Value
        If txtMaxExtValue.Text.Trim() <> "" Then
            Dim v As Integer = 0
            If Not Integer.TryParse(txtMaxExtValue.Text.Trim(), v) _
               OrElse v <= 0 Then
                ShowError("Extension valeur : entrez un entier positif ou laissez vide.")
                Return
            End If
            maxExtValObj = v
        End If

        ' Duplicate check - same CounterDefId + CounterBasisId on this PNLimit
        Dim tcId As Integer = 0
        Integer.TryParse(hfTaskCounterId.Value, tcId)
        Dim defId   As Integer = CInt(ddlCounterDef.SelectedValue)
        Dim basisId As Integer = CInt(ddlCounterBasis.SelectedValue)

        If IsDuplicate(defId, basisId, tcId) Then
            ShowError("Ce compteur avec cette base est d&eacute;j&agrave; " &
                      "d&eacute;fini pour cette limite. Modifiez la ligne existante.")
            Return
        End If

        Dim userId As String =
            If(Session("UserId") IsNot Nothing,
               Session("UserId").ToString(), "admin")

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand(
                        "mro2.usp_TaskCounter_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure

                    Dim idObj As Object =
                        If(tcId > 0, CType(tcId, Object), DBNull.Value)

                    cmd.Parameters.AddWithValue("@TaskCounterId",    idObj)
                    cmd.Parameters.AddWithValue("@PNLimitId",        _pnLimitId)
                    cmd.Parameters.AddWithValue("@CounterDefId",     defId)
                    cmd.Parameters.AddWithValue("@CounterBasisId",   basisId)
                    cmd.Parameters.AddWithValue("@FirstThreshold",   firstThr)
                    cmd.Parameters.AddWithValue("@RepeatInterval",   repeatObj)
                    cmd.Parameters.AddWithValue("@Ceiling",          ceilObj)
                    cmd.Parameters.AddWithValue("@AlertThresholdPct", alertPct)
                    cmd.Parameters.AddWithValue("@MaxExtensionPct",  maxExtPctObj)
                    cmd.Parameters.AddWithValue("@MaxExtensionValue", maxExtValObj)
                    cmd.Parameters.AddWithValue("@DisplayLabel",     DBNull.Value)
                    cmd.Parameters.AddWithValue("@Notes",            DBNull.Value)
                    cmd.Parameters.AddWithValue("@UserId",           userId)
                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using

            ResetForm()
            BindGrid()
            ShowToast("Compteur enregistr&eacute;.", "success")

        Catch ex As SqlException When ex.Number = 2627 OrElse ex.Number = 2601
            ShowError("Ce compteur existe d&eacute;j&agrave; pour cette limite.")
        Catch ex As Exception
            ShowError(Server.HtmlEncode(ex.Message))
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' CANCEL EDIT
    ' ────────────────────────────────────────────────────────
    Protected Sub btnCancelEdit_Click(sender As Object,
            e As EventArgs) Handles btnCancelEdit.Click
        ResetForm()
        lblError.Visible  = False
        litFormTitle.Text = "Ajouter un compteur"
        BindGrid()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' LOAD FOR EDIT
    ' ────────────────────────────────────────────────────────
    Private Sub LoadTCForEdit(ByVal tcId As Integer)
        LoadCounterTypeDDL()   ' repopulate before setting values
        LoadCounterBasisDDL()

        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT tc.TaskCounterId, tc.CounterDefId, " &
                "       cd.CounterTypeId, tc.CounterBasisId, " &
                "       tc.FirstThreshold, tc.RepeatInterval, " &
                "       tc.Ceiling, tc.AlertThresholdPct, " &
                "       tc.MaxExtensionPct, tc.MaxExtensionValue " &
                "FROM mro2.TaskCounter tc " &
                "INNER JOIN mro2.CounterDef cd " &
                "    ON cd.CounterDefId = tc.CounterDefId " &
                "WHERE tc.TaskCounterId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", tcId)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfTaskCounterId.Value = tcId.ToString()

                        ' Set CounterType → cascade CounterDef
                        Dim typeId As String = rdr("CounterTypeId").ToString()
                        If ddlCounterType.Items.FindByValue(typeId) IsNot Nothing Then
                            ddlCounterType.SelectedValue = typeId
                        End If
                        LoadCounterDefDDL(typeId)

                        Dim defId As String = rdr("CounterDefId").ToString()
                        If ddlCounterDef.Items.FindByValue(defId) IsNot Nothing Then
                            ddlCounterDef.SelectedValue = defId
                        End If

                        ' CounterBasis
                        Dim basisId As String = rdr("CounterBasisId").ToString()
                        If ddlCounterBasis.Items.FindByValue(basisId) IsNot Nothing Then
                            ddlCounterBasis.SelectedValue = basisId
                        End If

                        txtFirstThreshold.Text =
                            rdr("FirstThreshold").ToString()
                        txtRepeatInterval.Text =
                            If(rdr("RepeatInterval") Is DBNull.Value, "",
                               rdr("RepeatInterval").ToString())
                        txtCeiling.Text =
                            If(rdr("Ceiling") Is DBNull.Value, "",
                               rdr("Ceiling").ToString())
                        txtAlertPct.Text =
                            rdr("AlertThresholdPct").ToString()
                        txtMaxExtPct.Text =
                            If(rdr("MaxExtensionPct") Is DBNull.Value, "",
                               Convert.ToDecimal(
                                   rdr("MaxExtensionPct")).ToString("0.#"))
                        txtMaxExtValue.Text =
                            If(rdr("MaxExtensionValue") Is DBNull.Value, "",
                               rdr("MaxExtensionValue").ToString())

                        litFormTitle.Text = "Modifier le compteur"
                        UpdateUnitLiteral()
                    End If
                End Using
            End Using
        End Using
    End Sub

    ' ────────────────────────────────────────────────────────
    ' TOGGLE ACTIVE
    ' ────────────────────────────────────────────────────────
    Private Sub ToggleTCActive(ByVal tcId As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Dim cur As Boolean = True
            Using g As New SqlCommand(
                "SELECT IsActive FROM mro2.TaskCounter " &
                "WHERE TaskCounterId=@Id", cn)
                g.Parameters.AddWithValue("@Id", tcId)
                Dim o As Object = g.ExecuteScalar()
                If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                    cur = Convert.ToBoolean(o)
                End If
            End Using
            Using u As New SqlCommand(
                "UPDATE mro2.TaskCounter SET IsActive=@v " &
                "WHERE TaskCounterId=@Id", cn)
                u.Parameters.AddWithValue("@v",  If(cur, 0, 1))
                u.Parameters.AddWithValue("@Id", tcId)
                u.ExecuteNonQuery()
            End Using
        End Using
        ShowToast("Statut mis &agrave; jour.", "success")
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DUPLICATE CHECK
    ' ────────────────────────────────────────────────────────
    Private Function IsDuplicate(ByVal counterDefId As Integer,
                                  ByVal counterBasisId As Integer,
                                  ByVal excludeId As Integer) As Boolean
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT COUNT(*) FROM mro2.TaskCounter " &
                "WHERE PNLimitId=@LimitId " &
                "  AND CounterDefId=@DefId " &
                "  AND CounterBasisId=@BasisId " &
                "  AND IsActive=1 " &
                "  AND TaskCounterId <> @ExId", cn)
                cmd.Parameters.AddWithValue("@LimitId", _pnLimitId)
                cmd.Parameters.AddWithValue("@DefId",   counterDefId)
                cmd.Parameters.AddWithValue("@BasisId", counterBasisId)
                cmd.Parameters.AddWithValue("@ExId",    excludeId)
                Return CInt(cmd.ExecuteScalar()) > 0
            End Using
        End Using
    End Function

    ' ────────────────────────────────────────────────────────
    ' RESET FORM
    ' ────────────────────────────────────────────────────────
    Private Sub ResetForm()
        hfTaskCounterId.Value     = ""
        ddlCounterType.SelectedIndex = 0
        LoadCounterDefDDL("")
        LoadCounterBasisDDL()
        txtFirstThreshold.Text    = ""
        txtRepeatInterval.Text    = ""
        txtCeiling.Text           = ""
        txtAlertPct.Text          = "90"
        txtMaxExtPct.Text         = ""
        txtMaxExtValue.Text       = ""
        litUnit.Text = "-"
        litUnit2.Text = "-"
        litUnit3.Text = "-"
        litUnit4.Text = "-"
        litFormTitle.Text         = "Ajouter un compteur"
        lblError.Visible          = False
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DISPLAY HELPERS
    ' ────────────────────────────────────────────────────────
    Protected Function FormatValue(ByVal value As Object,
                                    ByVal unit As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then Return "-"
        Dim v As Long = Convert.ToInt64(value)
        Dim u As String = If(unit Is Nothing OrElse unit Is DBNull.Value,
                             "", unit.ToString())
        Return v.ToString("N0") &
               " <small class='text-muted'>" &
               Server.HtmlEncode(u) & "</small>"
    End Function

    Protected Function FormatExtension(ByVal pct As Object,
                                        ByVal val As Object,
                                        ByVal unit As Object) As String
        Dim parts As New System.Text.StringBuilder()
        If pct IsNot Nothing AndAlso pct IsNot DBNull.Value Then
            parts.Append(Convert.ToDecimal(pct).ToString("0.#") & "%")
        End If
        If val IsNot Nothing AndAlso val IsNot DBNull.Value Then
            If parts.Length > 0 Then parts.Append(" / ")
            Dim u As String = If(unit Is Nothing OrElse unit Is DBNull.Value,
                                 "", unit.ToString())
            parts.Append(Convert.ToInt64(val).ToString("N0") & " " & u)
        End If
        If parts.Length = 0 Then
            Return "<span class='text-muted'>-</span>"
        End If
        Return "<span class='text-info'>" & parts.ToString() & "</span>"
    End Function

    ' ────────────────────────────────────────────────────────
    ' UI HELPERS
    ' ────────────────────────────────────────────────────────
    Private Function SafeStr(ByVal o As Object) As String
        If o Is Nothing OrElse o Is DBNull.Value Then Return ""
        Return o.ToString()
    End Function

    Private Sub ShowError(ByVal msg As String)
        lblError.Text    = msg
        lblError.Visible = True
    End Sub

    Private Sub ShowToast(ByVal message As String, ByVal kind As String)
        Dim ser As New JavaScriptSerializer()
        Dim js  As String = "if(window.toastr){toastr." &
                            kind.ToLowerInvariant() & "(" &
                            ser.Serialize(message) & ");}"
        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "toast_" & Guid.NewGuid().ToString("N"), js, True)
    End Sub

End Class
