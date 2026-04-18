Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

' ============================================================
' MRO2/Setup/Components/PartNumberList.aspx.vb
' Two modals:
'   pnModal     - add / edit a Part Number
'   limitsModal - manage PNLimit rows for a selected PN
'                 cascade: CounterType → Counter DDL
'                 grid shows: type, code, hard limit, alert%,
'                             reset trigger, reference, SN count
' ============================================================
Partial Class MRO2_Setup_Components_PartNumberList
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    ' ── ViewState sort (PN grid) ────────────────────────────
    'Private Property SortColumn As String
    '    Get : Return If(TryCast(ViewState("SortColumn"), String), "PN") : End Get
    '    Set(v As String) : ViewState("SortColumn") = v : End Set
    'End Property
    'Private Property SortDir As String
    '    Get : Return If(TryCast(ViewState("SortDir"), String), "ASC") : End Get
    '    Set(v As String) : ViewState("SortDir") = v : End Set
    'End Property
    Private Property SortColumn As String
        Get
            Dim val = TryCast(ViewState("SortCol"), String)
            Return If(String.IsNullOrEmpty(val), "AppliesToAssetKindCode", val)
        End Get
        Set(value As String)
            ViewState("SortCol") = value
        End Set
    End Property

    Private Property SortDir As String
        Get
            Dim val = TryCast(ViewState("SortDir"), String)
            Return If(String.IsNullOrEmpty(val), "ASC", val)
        End Get
        Set(value As String)
            ViewState("SortDir") = value
        End Set
    End Property
    ' ────────────────────────────────────────────────────────
    ' PAGE LOAD
    ' ────────────────────────────────────────────────────────
    Protected Sub Page_Load(ByVal sender As Object,
                            ByVal e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            SortColumn = "PN"
            SortDir    = "ASC"
            LoadATA()
            LoadUOM()
            LoadAcMainGroup()
            LoadLimitTypeDDL()
            LoadAcMainGroupFilter()
            BindGrid()
        End If
    End Sub

    ' ────────────────────────────────────────────────────────
    ' PN GRID - EVENTS
    ' ────────────────────────────────────────────────────────
    Protected Sub chkIncludeInactive_CheckedChanged(
            sender As Object, e As EventArgs) _
            Handles chkIncludeInactive.CheckedChanged
        BindGrid()
    End Sub

    Protected Sub btnSearch_Click(sender As Object, e As EventArgs) _
            Handles btnSearch.Click
        gvPN.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs) _
            Handles btnClear.Click
        txtSearch.Text = ""
        gvPN.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub gvPN_PageIndexChanging(sender As Object,
            e As GridViewPageEventArgs) Handles gvPN.PageIndexChanging
        gvPN.PageIndex = e.NewPageIndex
        BindGrid()
    End Sub

    Protected Sub gvPN_Sorting(sender As Object,
            e As GridViewSortEventArgs) Handles gvPN.Sorting
        SortDir    = If(SortColumn = e.SortExpression _
                        AndAlso SortDir = "ASC", "DESC", "ASC")
        SortColumn = e.SortExpression
        gvPN.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub gvPN_RowCommand(sender As Object,
            e As GridViewCommandEventArgs) Handles gvPN.RowCommand

        Select Case e.CommandName

            Case "EditRow"
                Dim id As Integer = Convert.ToInt32(e.CommandArgument)
                LoadPNForEdit(id)
                lblError.Visible   = False
                litModalTitle.Text = "Edit Part Number"
                ShowModal("pnModal")

            Case "ToggleActive"
                TogglePNActive(Convert.ToInt32(e.CommandArgument))
                BindGrid()

            Case "ManageLimits"
                ' Open the limits modal for this PN
                Dim id As Integer = Convert.ToInt32(e.CommandArgument)
                OpenLimitsModal(id)

        End Select
    End Sub

    ' ────────────────────────────────────────────────────────
    ' NEW PN BUTTON
    ' ────────────────────────────────────────────────────────
    Protected Sub btnNew_Click(sender As Object,
            e As EventArgs) Handles btnNew.Click
        hfPartNumberId.Value  = ""
        txtPN.Text            = ""
        txtNomenclature.Text  = ""
        ddlATA.SelectedIndex  = 0
        ddlIsSerialized.SelectedValue = "1"
        SelectUomByCode("EA")
        ddlAcMainGroup.SelectedIndex = 0
        lblError.Visible      = False
        litModalTitle.Text    = "New Part Number"
        ShowModal("pnModal")
    End Sub

    ' ────────────────────────────────────────────────────────
    ' SAVE PART NUMBER
    ' ────────────────────────────────────────────────────────
    Protected Sub btnSave_Click(sender As Object,
            e As EventArgs) Handles btnSave.Click
        lblError.Visible = False

        Dim pn    As String  = txtPN.Text.Trim().ToUpperInvariant()
        Dim nom   As String  = txtNomenclature.Text.Trim()
        Dim isSer As Boolean = (ddlIsSerialized.SelectedValue = "1")
        Dim uomId As Integer = 0
        Integer.TryParse(ddlUOM.SelectedValue, uomId)
        Dim acMg  As Integer = 0
        Integer.TryParse(ddlAcMainGroup.SelectedValue, acMg)

        Dim ataObj As Object = DBNull.Value
        If ddlATA.SelectedValue <> "" Then
            ataObj = Convert.ToInt32(ddlATA.SelectedValue)
        End If

        If pn = "" Then
            ShowPNError("PN is required.")
            ShowModal("pnModal") : Return
        End If
        If uomId = 0 Then
            ShowPNError("UOM is required.")
            ShowModal("pnModal") : Return
        End If
        If isSer AndAlso acMg = 0 Then
            ShowPNError("AC Main Group is required for serialized PNs.")
            ShowModal("pnModal") : Return
        End If

        txtPN.Text = pn

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand("mro2.usp_PartNumber_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure

                    Dim idObj As Object = DBNull.Value
                    If hfPartNumberId.Value <> "" Then
                        idObj = Convert.ToInt32(hfPartNumberId.Value)
                    End If

                    cmd.Parameters.AddWithValue("@PartNumberId",  idObj)
                    cmd.Parameters.AddWithValue("@PN",            pn)
                    cmd.Parameters.AddWithValue("@Nomenclature",
                        If(nom = "", CType(DBNull.Value, Object), nom))
                    cmd.Parameters.AddWithValue("@ATAId",         ataObj)
                    cmd.Parameters.AddWithValue("@IsSerialized",  If(isSer, 1, 0))
                    cmd.Parameters.AddWithValue("@UnitOfMeasureId", uomId)
                    cmd.Parameters.AddWithValue("@AcMainGroupID",
                        If(acMg = 0, CType(DBNull.Value, Object), acMg))

                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using

            BindGrid()
            HideModal("pnModal")
            ShowToast("Part Number saved.", "success")

        Catch ex As SqlException When ex.Number = 2627 OrElse ex.Number = 2601
            ShowPNError("This PN already exists.")
            ShowModal("pnModal")
        Catch ex As Exception
            ShowPNError(Server.HtmlEncode(ex.Message))
            ShowModal("pnModal")
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' LIMITS MODAL - OPEN
    ' ────────────────────────────────────────────────────────
    Private Sub OpenLimitsModal(ByVal pnId As Integer)
        hfLimitPNId.Value = pnId.ToString()
        hfPNLimitId.Value = ""                      ' clear any edit state

        ' Load PN header info for modal title
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT PN, ISNULL(Nomenclature,'') AS Nomenclature " &
                "FROM mro2.PartNumber WHERE PartNumberId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", pnId)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        litLimitPN.Text  = Server.HtmlEncode(rdr("PN").ToString())
                        litLimitNom.Text = " - " &
                            Server.HtmlEncode(rdr("Nomenclature").ToString())
                    End If
                End Using
            End Using
        End Using

        ' Load DDLs for add form (always fresh)
        LoadLimitLimitTypeDDL()
        LoadLimitTypeDDL()    ' also calls LoadCounterDDL("")
        LoadLimitBasisDDL()
        ' Reset form to Add mode
        ResetLimitForm()
        ' Bind the limits grid for this PN
        BindLimitsGrid(pnId)
        ShowModal("limitsModal")
    End Sub

    ' ────────────────────────────────────────────────────────
    ' LIMITS GRID - BIND
    ' ────────────────────────────────────────────────────────
    Private Sub BindLimitsGrid(ByVal pnId As Integer)
        Dim dt As New DataTable()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT pl.PNLimitId, pl.PartNumberId, " &
                "       lt.Code  AS LimitTypeCode, " &
                "       lt.BadgeColor, " &
                "       cd.Code  AS CounterDefCode, " &
                "       cd.Name  AS CounterDefName, " &
                "       ct.DisplayUnit, " &
                "       cb.Code  AS CounterBasisCode, " &
                "       pl.HardLimit, pl.AlertThresholdPct, " &
                "       pl.IsActive, pl.IsPhased, " &
                "       ISNULL(tc.TCCount,0) AS SNCount " &
                "FROM mro2.PNLimit pl " &
                "LEFT JOIN mro2.LimitType  lt ON lt.LimitTypeId  = pl.LimitTypeId " &
                "LEFT JOIN mro2.TaskCounter tc_sub ON tc_sub.PNLimitId = pl.PNLimitId " &
                "LEFT JOIN (" &
                "    SELECT PNLimitId, COUNT(*) AS TCCount " &
                "    FROM mro2.TaskCounter WHERE IsActive=1 " &
                "    GROUP BY PNLimitId) tc ON tc.PNLimitId = pl.PNLimitId " &
                "LEFT JOIN mro2.TaskCounter tc2 ON tc2.PNLimitId = pl.PNLimitId AND tc2.IsActive=1 " &
                "LEFT JOIN mro2.CounterDef  cd ON cd.CounterDefId = tc2.CounterDefId " &
                "LEFT JOIN mro2.CounterType ct ON ct.CounterTypeId = cd.CounterTypeId " &
                "LEFT JOIN mro2.CounterBasis cb ON cb.CounterBasisId = tc2.CounterBasisId " &
                "WHERE pl.PartNumberId=@PnId " &
                "GROUP BY pl.PNLimitId, pl.PartNumberId, lt.Code, lt.BadgeColor, " &
                "         cd.Code, cd.Name, ct.DisplayUnit, cb.Code, " &
                "         pl.HardLimit, pl.AlertThresholdPct, pl.IsActive, " &
                "         pl.IsPhased, tc.TCCount " &
                "ORDER BY pl.PNLimitId", cn)
                cmd.Parameters.AddWithValue("@PnId", pnId)
                cn.Open()
                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using
        gvLimits.DataSource = dt
        gvLimits.DataBind()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' LIMITS GRID - ROW COMMANDS
    ' ────────────────────────────────────────────────────────
    Protected Sub gvLimits_RowCommand(sender As Object,
            e As GridViewCommandEventArgs) Handles gvLimits.RowCommand

        Dim pnId As Integer = Convert.ToInt32(hfLimitPNId.Value)

        Select Case e.CommandName

            Case "EditLimit"
                Dim limitId As Integer = Convert.ToInt32(e.CommandArgument)
                LoadLimitForEdit(limitId)
                litFormTitle.Text = "Edit Limit"
                lblLimitError.Visible = False
                BindLimitsGrid(pnId)
                ShowModal("limitsModal")

            Case "ToggleLimit"
                Dim limitId As Integer = Convert.ToInt32(e.CommandArgument)
                ToggleLimitActive(limitId)
                BindLimitsGrid(pnId)
                ShowModal("limitsModal")

        End Select
    End Sub

    ' ────────────────────────────────────────────────────────
    ' LIMITS FORM - COUNTER TYPE CASCADE
    ' ────────────────────────────────────────────────────────
    Protected Sub ddlLimitType_SelectedIndexChanged(
            sender As Object, e As EventArgs) _
            Handles ddlLimitType.SelectedIndexChanged

        ' Cascade: reload Counter DDL filtered by selected CounterType
        LoadCounterDDL(ddlLimitType.SelectedValue)
        ' Update unit literal
        UpdateUnitLiteral()
        ' Keep modal open
        Dim pnId As Integer = Convert.ToInt32(hfLimitPNId.Value)
        BindLimitsGrid(pnId)
        ShowModal("limitsModal")
    End Sub

    ' ────────────────────────────────────────────────────────
    ' LIMITS FORM - SAVE
    ' ────────────────────────────────────────────────────────
    Protected Sub btnSaveLimit_Click(sender As Object,
            e As EventArgs) Handles btnSaveLimit.Click

        lblLimitError.Visible = False
        Dim pnId As Integer = Convert.ToInt32(hfLimitPNId.Value)

        ' Validate inputs
        Dim counterIdStr As String = ddlLimitCounter.SelectedValue.Trim()
        Dim hardLimitStr As String = txtHardLimit.Text.Trim()
        Dim alertPctStr  As String = txtAlertPct.Text.Trim()

        If ddlLimitType.SelectedValue = "" Then
            ShowLimitError("S&eacute;lectionnez un type de compteur.")
            BindLimitsGrid(pnId) : ShowModal("limitsModal") : Return
        End If
        If counterIdStr = "" Then
            ShowLimitError("S&eacute;lectionnez un compteur.")
            BindLimitsGrid(pnId) : ShowModal("limitsModal") : Return
        End If
        If ddlLimitBasis.SelectedValue = "" Then
            ShowLimitError("S&eacute;lectionnez une base de comptage.")
            BindLimitsGrid(pnId) : ShowModal("limitsModal") : Return
        End If

        Dim hardLimit As Decimal = 0
        If Not Decimal.TryParse(hardLimitStr,
               System.Globalization.NumberStyles.Any,
               System.Globalization.CultureInfo.InvariantCulture,
               hardLimit) OrElse hardLimit <= 0 Then
            ShowLimitError("Hard Limit must be a positive number.")
            BindLimitsGrid(pnId) : ShowModal("limitsModal") : Return
        End If

        Dim alertPct As Byte = 90
        If Not Byte.TryParse(alertPctStr, alertPct) _
           OrElse alertPct < 1 OrElse alertPct > 99 Then
            ShowLimitError("Alert % must be between 1 and 99.")
            BindLimitsGrid(pnId) : ShowModal("limitsModal") : Return
        End If

        ' Check for duplicate counter on this PN (block it)
        Dim currentLimitId As Integer = 0
        Integer.TryParse(hfPNLimitId.Value, currentLimitId)

        If IsDuplicateCounter(pnId, CInt(counterIdStr), currentLimitId) Then
            ShowLimitError("This counter already has a limit defined " &
                           "for this PN. Edit the existing row instead.")
            BindLimitsGrid(pnId) : ShowModal("limitsModal") : Return
        End If


        Try
            Using cn As New SqlConnection(ConnStr)
                ' Step 1: Save PNLimit row
                Dim newLimitId As Integer = currentLimitId
                Using cmd As New SqlCommand("mro2.usp_PNLimit_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    Dim idObj As Object = DBNull.Value
                    If currentLimitId > 0 Then idObj = currentLimitId

                    Dim limitTypeObj As Object = DBNull.Value
                    If ddlLimitLimitType.SelectedValue <> "" Then
                        limitTypeObj = CInt(ddlLimitLimitType.SelectedValue)
                    End If

                    cmd.Parameters.AddWithValue("@PNLimitId",         idObj)
                    cmd.Parameters.AddWithValue("@PartNumberId",       pnId)
                    cmd.Parameters.AddWithValue("@LimitTypeId",        limitTypeObj)
                    cmd.Parameters.AddWithValue("@HardLimit",          hardLimit)
                    cmd.Parameters.AddWithValue("@AlertThresholdPct",  alertPct)
                    cmd.Parameters.AddWithValue("@CounterReferenceId", DBNull.Value)
                    cmd.Parameters.AddWithValue("@Notes",              DBNull.Value)
                    cmd.Parameters.AddWithValue("@UserId",
                        If(Session("UserId") IsNot Nothing,
                           Session("UserId").ToString(), "admin"))
                    cn.Open()
                    Dim result As Object = cmd.ExecuteScalar()
                    If currentLimitId = 0 AndAlso result IsNot Nothing Then
                        newLimitId = CInt(result)
                    End If
                End Using

                ' Step 2: Save TaskCounter row (one per limit = the CounterDef line)
                ' Only create if this is a new PNLimit (not an edit)
                If currentLimitId = 0 AndAlso newLimitId > 0 Then
                    Using cmd2 As New SqlCommand("mro2.usp_TaskCounter_Save", cn)
                        cmd2.CommandType = CommandType.StoredProcedure
                        cmd2.Parameters.AddWithValue("@TaskCounterId",    DBNull.Value)
                        cmd2.Parameters.AddWithValue("@PNLimitId",        newLimitId)
                        cmd2.Parameters.AddWithValue("@CounterDefId",     CInt(counterIdStr))
                        cmd2.Parameters.AddWithValue("@CounterBasisId",   CInt(ddlLimitBasis.SelectedValue))
                        ' HardLimit stored in storage units
                        ' CounterDef UnitStorage determines if minutes or count
                        Dim storageVal As Integer = CInt(Math.Round(hardLimit))
                        cmd2.Parameters.AddWithValue("@FirstThreshold",   storageVal)
                        cmd2.Parameters.AddWithValue("@RepeatInterval",   storageVal)
                        cmd2.Parameters.AddWithValue("@Ceiling",          storageVal)
                        cmd2.Parameters.AddWithValue("@AlertThresholdPct", alertPct)
                        cmd2.Parameters.AddWithValue("@DisplayLabel",     DBNull.Value)
                        cmd2.Parameters.AddWithValue("@Notes",            DBNull.Value)
                        cmd2.Parameters.AddWithValue("@UserId",
                            If(Session("UserId") IsNot Nothing,
                               Session("UserId").ToString(), "admin"))
                        cmd2.ExecuteScalar()
                    End Using
                End If
            End Using

            ' Refresh PN grid (limit count badge changes)
            BindGrid()
            ' Refresh limits grid inside modal
            BindLimitsGrid(pnId)
            ' Reset form to Add mode
            ResetLimitForm()
            litFormTitle.Text = "Add New Limit"
            ShowToast("Limit saved.", "success")
            ShowModal("limitsModal")

        Catch ex As SqlException When ex.Number = 2627 OrElse ex.Number = 2601
            ShowLimitError("This counter already has a limit on this PN.")
            BindLimitsGrid(pnId) : ShowModal("limitsModal")
        Catch ex As Exception
            ShowLimitError(Server.HtmlEncode(ex.Message))
            BindLimitsGrid(pnId) : ShowModal("limitsModal")
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' LIMITS FORM - CANCEL EDIT (back to Add mode)
    ' ────────────────────────────────────────────────────────
    Protected Sub btnCancelEdit_Click(sender As Object,
            e As EventArgs) Handles btnCancelEdit.Click
        Dim pnId As Integer = Convert.ToInt32(hfLimitPNId.Value)
        ResetLimitForm()
        litFormTitle.Text     = "Add New Limit"
        lblLimitError.Visible = False
        BindLimitsGrid(pnId)
        ShowModal("limitsModal")
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DATA HELPERS
    ' ────────────────────────────────────────────────────────
    Private Sub BindGrid()
        Dim ds As New DataSet()

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_PartNumber_List", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@IncludeInactive", If(chkIncludeInactive.Checked, 1, 0))
                cmd.Parameters.AddWithValue("@Search", If(txtSearch.Text.Trim() = "", CType(DBNull.Value, Object), txtSearch.Text.Trim()))
                cmd.Parameters.AddWithValue("@SortColumn", SortColumn)
                cmd.Parameters.AddWithValue("@SortDir", SortDir)

                ' NEW:
                cmd.Parameters.AddWithValue("@AcMainGroupId", If(ddlFilterAcMainGroup.SelectedValue = "", CType(DBNull.Value, Object), CInt(ddlFilterAcMainGroup.SelectedValue)))
                cmd.Parameters.AddWithValue("@PageNumber", gvPN.PageIndex + 1)  ' GridView PageIndex is 0-based
                cmd.Parameters.AddWithValue("@PageSize", gvPN.PageSize)

                Using da As New SqlDataAdapter(cmd)
                    da.Fill(ds)
                End Using
            End Using
        End Using

        Dim dt As DataTable = ds.Tables(0)

        Dim totalRows As Integer = 0
        If ds.Tables.Count > 1 AndAlso ds.Tables(1).Rows.Count > 0 Then
            totalRows = Convert.ToInt32(ds.Tables(1).Rows(0)("TotalRows"))
        End If

        litRowCount.Text = totalRows.ToString()

        gvPN.DataSource = dt
        gvPN.DataBind()
    End Sub

    Private Sub LoadPNForEdit(ByVal id As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT PartNumberId, PN, Nomenclature, ATAId, " &
                "IsSerialized, UnitOfMeasureId, AcMainGroupID " &
                "FROM mro2.PartNumber WHERE PartNumberId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", id)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfPartNumberId.Value          = rdr("PartNumberId").ToString()
                        txtPN.Text                    = rdr("PN").ToString()
                        txtNomenclature.Text          = SafeStr(rdr("Nomenclature"))
                        ddlIsSerialized.SelectedValue =
                            If(Convert.ToBoolean(rdr("IsSerialized")), "1", "0")

                        Dim ataVal As String = SafeStr(rdr("ATAId"))
                        If ddlATA.Items.FindByValue(ataVal) IsNot Nothing Then
                            ddlATA.SelectedValue = ataVal
                        Else
                            ddlATA.SelectedIndex = 0
                        End If

                        Dim uomVal As String = SafeStr(rdr("UnitOfMeasureId"))
                        If ddlUOM.Items.FindByValue(uomVal) IsNot Nothing Then
                            ddlUOM.SelectedValue = uomVal
                        End If

                        Dim mgVal As String = SafeStr(rdr("AcMainGroupID"))
                        If ddlAcMainGroup.Items.FindByValue(mgVal) IsNot Nothing Then
                            ddlAcMainGroup.SelectedValue = mgVal
                        Else
                            ddlAcMainGroup.SelectedIndex = 0
                        End If
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub TogglePNActive(ByVal id As Integer)
        Try
            Using cn As New SqlConnection(ConnStr)
                cn.Open()
                Dim cur As Boolean = True
                Using cmdGet As New SqlCommand(
                    "SELECT IsActive FROM mro2.PartNumber WHERE PartNumberId=@Id", cn)
                    cmdGet.Parameters.AddWithValue("@Id", id)
                    Dim o As Object = cmdGet.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                        cur = Convert.ToBoolean(o)
                    End If
                End Using
                Using cmd As New SqlCommand("mro2.usp_PartNumber_SetActive", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@PartNumberId", id)
                    cmd.Parameters.AddWithValue("@IsActive", If(cur, 0, 1))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            ShowToast("Status updated.", "success")
        Catch ex As Exception
            ShowToast(Server.HtmlEncode(ex.Message), "error")
        End Try
    End Sub

    Private Sub LoadLimitForEdit(ByVal limitId As Integer)
        ' Load PNLimit + first TaskCounter row for edit form
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT pl.PNLimitId, pl.LimitTypeId, " &
                "       pl.HardLimit, pl.AlertThresholdPct, " &
                "       tc.TaskCounterId, tc.CounterDefId, " &
                "       cd.CounterTypeId, tc.CounterBasisId " &
                "FROM mro2.PNLimit pl " &
                "LEFT JOIN mro2.TaskCounter tc " &
                "    ON tc.PNLimitId=pl.PNLimitId AND tc.IsActive=1 " &
                "LEFT JOIN mro2.CounterDef cd " &
                "    ON cd.CounterDefId=tc.CounterDefId " &
                "WHERE pl.PNLimitId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", limitId)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfPNLimitId.Value = limitId.ToString()

                        ' LimitType
                        Dim ltId As String =
                            If(rdr("LimitTypeId") Is DBNull.Value, "",
                               rdr("LimitTypeId").ToString())
                        If ddlLimitLimitType.Items.FindByValue(ltId) IsNot Nothing Then
                            ddlLimitLimitType.SelectedValue = ltId
                        End If

                        ' CounterType → CounterDef cascade
                        Dim typeId As String =
                            If(rdr("CounterTypeId") Is DBNull.Value, "",
                               rdr("CounterTypeId").ToString())
                        If ddlLimitType.Items.FindByValue(typeId) IsNot Nothing Then
                            ddlLimitType.SelectedValue = typeId
                        End If
                        LoadCounterDDL(typeId)

                        Dim defId As String =
                            If(rdr("CounterDefId") Is DBNull.Value, "",
                               rdr("CounterDefId").ToString())
                        If ddlLimitCounter.Items.FindByValue(defId) IsNot Nothing Then
                            ddlLimitCounter.SelectedValue = defId
                        End If

                        ' CounterBasis
                        Dim basisId As String =
                            If(rdr("CounterBasisId") Is DBNull.Value, "",
                               rdr("CounterBasisId").ToString())
                        If ddlLimitBasis.Items.FindByValue(basisId) IsNot Nothing Then
                            ddlLimitBasis.SelectedValue = basisId
                        End If

                        UpdateUnitLiteral()
                        txtHardLimit.Text = Convert.ToDecimal(
                            rdr("HardLimit")).ToString("0.#")
                        txtAlertPct.Text  = rdr("AlertThresholdPct").ToString()
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub ToggleLimitActive(ByVal limitId As Integer)
        Try
            Using cn As New SqlConnection(ConnStr)
                cn.Open()
                Dim cur As Boolean = True
                Using cmdGet As New SqlCommand(
                    "SELECT IsActive FROM mro2.PNLimit WHERE PNLimitId=@Id", cn)
                    cmdGet.Parameters.AddWithValue("@Id", limitId)
                    Dim o As Object = cmdGet.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                        cur = Convert.ToBoolean(o)
                    End If
                End Using
                Using cmd As New SqlCommand("mro2.usp_PNLimit_SetActive", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@PNLimitId", limitId)
                    cmd.Parameters.AddWithValue("@IsActive",  If(cur, 0, 1))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            ShowToast("Limit status updated.", "success")
        Catch ex As Exception
            ShowToast(Server.HtmlEncode(ex.Message), "error")
        End Try
    End Sub

    Private Function IsDuplicateCounter(ByVal pnId As Integer,
                                         ByVal counterDefId As Integer,
                                         ByVal excludeLimitId As Integer) As Boolean
        ' Block if this PN already has a TaskCounter with the same CounterDefId
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT COUNT(*) FROM mro2.TaskCounter tc " &
                "INNER JOIN mro2.PNLimit pl ON pl.PNLimitId = tc.PNLimitId " &
                "WHERE pl.PartNumberId = @PnId " &
                "  AND tc.CounterDefId = @CId " &
                "  AND tc.IsActive = 1 " &
                "  AND pl.PNLimitId <> @ExId", cn)
                cmd.Parameters.AddWithValue("@PnId", pnId)
                cmd.Parameters.AddWithValue("@CId",  counterDefId)
                cmd.Parameters.AddWithValue("@ExId", excludeLimitId)
                Return CInt(cmd.ExecuteScalar()) > 0
            End Using
        End Using
    End Function

    ' ────────────────────────────────────────────────────────
    ' DDL LOADERS
    ' ────────────────────────────────────────────────────────
    Private Sub LoadATA()
        ddlATA.Items.Clear()
        ddlATA.Items.Add(New ListItem("(none)", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_GetAtaList", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlATA.Items.Add(New ListItem(
                            rdr("ATACode").ToString(),
                            rdr("ATAId").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub LoadUOM()
        ddlUOM.Items.Clear()
        ddlUOM.Items.Add(New ListItem("-- select --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT UnitOfMeasureId, Code, Name " &
                "FROM mro2.UnitOfMeasure WHERE IsActive=1 ORDER BY Code", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlUOM.Items.Add(New ListItem(
                            rdr("Code").ToString() & " (" &
                            rdr("Name").ToString() & ")",
                            rdr("UnitOfMeasureId").ToString()))
                    End While
                End Using
            End Using
        End Using
        SelectUomByCode("EA")
    End Sub

    Private Sub LoadAcMainGroup()
        ddlAcMainGroup.Items.Clear()
        ddlAcMainGroup.Items.Add(New ListItem("-- select --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT AcMainGroupID, AcMainGroup " &
                "FROM dbo.tblAcMainGroup WHERE Active=1 ORDER BY AcMainGroup", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlAcMainGroup.Items.Add(New ListItem(
                            rdr("AcMainGroup").ToString(),
                            rdr("AcMainGroupID").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    ' ── Loads LimitType DDL (LIFE/INSPECTION/FUNCTIONAL/SHELF_LIFE) ──────────
    Private Sub LoadLimitLimitTypeDDL()
        ddlLimitLimitType.Items.Clear()
        ddlLimitLimitType.Items.Add(New ListItem("-- Type de limite --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT LimitTypeId, Code, Name, BadgeColor " &
                "FROM mro2.LimitType WHERE IsActive=1 ORDER BY SortOrder", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlLimitLimitType.Items.Add(New ListItem(
                            rdr("Code").ToString() & " - " &
                            rdr("Name").ToString(),
                            rdr("LimitTypeId").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    ' ── Loads CounterType DDL (cascade root: FH/FC/APU/Calendar...) ──────────
    Private Sub LoadLimitTypeDDL()
        ddlLimitType.Items.Clear()
        ddlLimitType.Items.Add(New ListItem("-- Type compteur --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT CounterTypeId, Code, DisplayUnit " &
                "FROM mro2.CounterType WHERE IsActive=1 " &
                "ORDER BY SortOrder, Code", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlLimitType.Items.Add(New ListItem(
                            rdr("Code").ToString() & " [" &
                            rdr("DisplayUnit").ToString() & "]",
                            rdr("CounterTypeId").ToString()))
                    End While
                End Using
            End Using
        End Using
        LoadCounterDDL("")
    End Sub

    ' ── Cascades CounterType → CounterDef ─────────────────────────────────────
    Private Sub LoadCounterDDL(ByVal typeIdStr As String)
        ddlLimitCounter.Items.Clear()
        ddlLimitCounter.Items.Add(New ListItem("-- Compteur --", ""))
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
                        ddlLimitCounter.Items.Add(New ListItem(
                            rdr("Code").ToString() & " - " &
                            rdr("Name").ToString(),
                            rdr("CounterDefId").ToString()))
                    End While
                End Using
            End Using
        End Using
        ' Update unit literal
        UpdateUnitLiteral()
    End Sub

    ' ── CounterBasis DDL (SINCE_NEW/SINCE_INSTALL/SINCE_OH/ABSOLUTE) ─────────
    Private Sub LoadLimitBasisDDL()
        ddlLimitBasis.Items.Clear()
        ddlLimitBasis.Items.Add(New ListItem("-- Base --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT CounterBasisId, Code, Name " &
                "FROM mro2.CounterBasis WHERE IsActive=1 ORDER BY SortOrder", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlLimitBasis.Items.Add(New ListItem(
                            rdr("Code").ToString() & " - " &
                            rdr("Name").ToString(),
                            rdr("CounterBasisId").ToString()))
                    End While
                End Using
            End Using
        End Using
        ' Default: SINCE_NEW (index 1 after blank)
        If ddlLimitBasis.Items.Count > 1 Then
            Dim snew As ListItem = Nothing
            For Each item As ListItem In ddlLimitBasis.Items
                If item.Text.StartsWith("SINCE_NEW") Then snew = item
            Next
            If snew IsNot Nothing Then ddlLimitBasis.SelectedValue = snew.Value
        End If
    End Sub
    Private Sub LoadAcMainGroupFilter()
        ddlFilterAcMainGroup.Items.Clear()
        ddlFilterAcMainGroup.Items.Add(New ListItem("All AC Main Groups", ""))

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT AcMainGroupID, AcMainGroup " &
                "FROM dbo.tblAcMainGroup WHERE Active=1 ORDER BY AcMainGroup", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlFilterAcMainGroup.Items.Add(New ListItem(
                            rdr("AcMainGroup").ToString(),
                            rdr("AcMainGroupID").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub
    Protected Sub ddlFilterAcMainGroup_SelectedIndexChanged(sender As Object, e As EventArgs) _
    Handles ddlFilterAcMainGroup.SelectedIndexChanged
        gvPN.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub ddlMaxRows_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles ddlMaxRows.SelectedIndexChanged
        gvPN.PageIndex = 0
        BindGrid()
    End Sub
    ' ── Updates unit literal from selected CounterType ────────────────────────
    Private Sub UpdateUnitLiteral()
        If ddlLimitType.SelectedValue = "" Then
            litUnit.Text = "-" : Return
        End If
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT DisplayUnit FROM mro2.CounterType " &
                "WHERE CounterTypeId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", CInt(ddlLimitType.SelectedValue))
                Dim o As Object = cmd.ExecuteScalar()
                litUnit.Text = If(o IsNot Nothing, o.ToString(), "-")
            End Using
        End Using
    End Sub

    Private Sub ResetLimitForm()
        hfPNLimitId.Value               = ""
        ddlLimitLimitType.SelectedIndex = 0
        ddlLimitType.SelectedIndex      = 0
        LoadCounterDDL("")
        LoadLimitBasisDDL()   ' resets to SINCE_NEW default
        txtHardLimit.Text               = ""
        txtAlertPct.Text                = "90"
        litUnit.Text = "-"
        litAlertCalc.Text               = "?"
        litFormTitle.Text               = "Ajouter une limite"
        lblLimitError.Visible           = False
    End Sub

    Private Sub SelectUomByCode(ByVal code As String)
        Dim target As String = code.Trim().ToUpperInvariant()
        For Each it As ListItem In ddlUOM.Items
            If it.Text IsNot Nothing AndAlso
               (it.Text.Trim().ToUpperInvariant().StartsWith(target & " ") _
                OrElse it.Text.Trim().ToUpperInvariant() = target) Then
                ddlUOM.SelectedValue = it.Value
                Exit Sub
            End If
        Next
    End Sub

    ' ────────────────────────────────────────────────────────
    ' PROTECTED DISPLAY HELPERS  (called from ASPX databind)
    ' ────────────────────────────────────────────────────────

    ' Returns colored badge showing limit count; 0 = grey, >0 = warning
    Protected Function LimitBadge(ByVal pnId As Integer,
                                   ByVal count As Integer) As String
        If count = 0 Then
            Return "<span class='badge badge-secondary'>0 limits</span>"
        End If
        Return "<span class='badge badge-warning text-dark' " &
               "title='Click Limits button to manage'>" &
               count.ToString() & " limit" &
               If(count = 1, "", "s") & "</span>"
    End Function

    ' Format HardLimit for display (no IsDecimal - use DisplayUnit from CounterType)
    Protected Function FormatHardLimit(ByVal hardLimit As Object,
                                        ByVal unit As Object) As String
        If hardLimit Is Nothing OrElse hardLimit Is DBNull.Value Then Return "-"
        Dim v As Decimal = Convert.ToDecimal(hardLimit)
        Dim u As String  = If(unit Is Nothing OrElse unit Is DBNull.Value,
                              "", unit.ToString())
        Return v.ToString("N0") & " <small class='text-muted'>" &
               Server.HtmlEncode(u) & "</small>"
    End Function

    Protected Function FormatAlert(ByVal hardLimit As Object,
                                    ByVal pct As Object,
                                    ByVal unit As Object) As String
        If hardLimit Is Nothing OrElse hardLimit Is DBNull.Value Then Return "-"
        Dim lim   As Decimal = Convert.ToDecimal(hardLimit)
        Dim p     As Decimal = Convert.ToDecimal(pct)
        Dim atVal As Decimal = lim * p / 100D
        Dim u     As String  = If(unit Is Nothing OrElse unit Is DBNull.Value,
                                  "", unit.ToString())
        Return "<span class='text-warning font-weight-bold'>" &
               atVal.ToString("N0") & "</span> " &
               "<small class='text-muted'>(" & p.ToString("0") & "% - " &
               Server.HtmlEncode(u) & ")</small>"
    End Function

    Protected Function TaskCounterBadgeClass(ByVal count As Integer) As String
        If count = 0 Then
            Return "badge badge-warning text-dark"
        End If
        Return "badge badge-primary"
    End Function

    ' ────────────────────────────────────────────────────────
    ' UI HELPERS
    ' ────────────────────────────────────────────────────────
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

    Private Sub ShowPNError(ByVal msg As String)
        lblError.Text    = msg
        lblError.Visible = True
    End Sub

    Private Sub ShowLimitError(ByVal msg As String)
        lblLimitError.Text    = msg
        lblLimitError.Visible = True
    End Sub

    Private Sub ShowToast(ByVal message As String, ByVal kind As String)
        Dim k   As String = If(kind, "info").ToLowerInvariant()
        Dim ser As New JavaScriptSerializer()
        Dim js  As String = "if(window.toastr){toastr." & k & "(" &
                            ser.Serialize(If(message, "")) & ");}"
        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "toast_" & Guid.NewGuid().ToString("N"), js, True)
    End Sub

    Private Function SafeStr(ByVal o As Object) As String
        If o Is Nothing OrElse o Is DBNull.Value Then Return ""
        Return o.ToString().Trim()
    End Function

End Class
