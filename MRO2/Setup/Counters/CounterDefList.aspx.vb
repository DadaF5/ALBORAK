Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

' ============================================================
' MRO2/Setup/Counters/CounterDefList.aspx.vb
' Replaces CounterList.aspx.vb
' Key additions vs v1:
'   • AppliesToAssetKindCode (AIRCRAFT | COMPONENT) filter + form field
'   • UnitStorage auto-inherited from CounterType on save (via SP)
'   • Table: mro2.CounterDef (was mro2.Counter)
'   • SP: usp_CounterDef_* (was usp_Counter_*)
' ============================================================
Partial Class MRO2_Setup_Counters_CounterDefList
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

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
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            SortColumn = "AppliesToAssetKindCode"
            SortDir    = "ASC"
            LoadFilterTypeDDL()
            LoadModalTypeDDL()
            BindGrid()
        End If
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DDL LOADERS
    ' ────────────────────────────────────────────────────────
    Private Sub LoadFilterTypeDDL()
        Dim saved As String = ddlFilterType.SelectedValue
        ddlFilterType.Items.Clear()
        ddlFilterType.Items.Add(New ListItem("-- Tous les types --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT CounterTypeId, Code FROM mro2.CounterType " &
                "WHERE IsActive=1 ORDER BY SortOrder, Code", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlFilterType.Items.Add(New ListItem(
                            rdr("Code").ToString(),
                            rdr("CounterTypeId").ToString()))
                    End While
                End Using
            End Using
        End Using
        If ddlFilterType.Items.FindByValue(saved) IsNot Nothing Then
            ddlFilterType.SelectedValue = saved
        End If
    End Sub

    Private Sub LoadModalTypeDDL()
        Dim saved As String = ddlModalType.SelectedValue
        ddlModalType.Items.Clear()
        ddlModalType.Items.Add(New ListItem("-- S&eacute;lectionner --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT CounterTypeId, Code, Name, UnitStorage " &
                "FROM mro2.CounterType WHERE IsActive=1 " &
                "ORDER BY SortOrder, Code", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim storage As String = rdr("UnitStorage").ToString()
                        ddlModalType.Items.Add(New ListItem(
                            rdr("Code").ToString() & " [" & storage & "]",
                            rdr("CounterTypeId").ToString()))
                    End While
                End Using
            End Using
        End Using
        If ddlModalType.Items.FindByValue(saved) IsNot Nothing Then
            ddlModalType.SelectedValue = saved
        End If
    End Sub

    ' ────────────────────────────────────────────────────────
    ' FILTER EVENTS
    ' ────────────────────────────────────────────────────────
    Protected Sub ddlFilterType_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles ddlFilterType.SelectedIndexChanged
        BindGrid()
    End Sub

    Protected Sub ddlFilterAsset_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles ddlFilterAsset.SelectedIndexChanged
        BindGrid()
    End Sub

    Protected Sub chkIncludeInactive_CheckedChanged(sender As Object, e As EventArgs) _
        Handles chkIncludeInactive.CheckedChanged
        BindGrid()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' GRID EVENTS
    ' ────────────────────────────────────────────────────────
    Protected Sub gvCD_Sorting(sender As Object, e As GridViewSortEventArgs) _
        Handles gvCD.Sorting
        SortDir    = If(SortColumn = e.SortExpression AndAlso SortDir = "ASC", "DESC", "ASC")
        SortColumn = e.SortExpression
        BindGrid()
    End Sub

    Protected Sub gvCD_RowCommand(sender As Object, e As GridViewCommandEventArgs) _
        Handles gvCD.RowCommand
        Select Case e.CommandName
            Case "EditRow"
                LoadForEdit(Convert.ToInt32(e.CommandArgument))
                lblError.Visible   = False
                litModalTitle.Text = "Modifier le compteur"
                LoadModalTypeDDL()
                ShowModal()
            Case "ToggleActive"
                ToggleActive(Convert.ToInt32(e.CommandArgument))
                BindGrid()
        End Select
    End Sub

    ' ────────────────────────────────────────────────────────
    ' NEW BUTTON
    ' ────────────────────────────────────────────────────────
    Protected Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        hfCounterDefId.Value         = ""
        txtCode.Text                 = ""
        txtName.Text                 = ""
        txtSortOrder.Text            = "99"
        ddlAssetKind.SelectedValue   = "AIRCRAFT"
        lblError.Visible             = False
        litModalTitle.Text           = "Nouveau compteur"
        LoadModalTypeDDL()
        ' Pre-select type from filter if active
        If ddlFilterType.SelectedValue <> "" Then
            Dim it As ListItem = ddlModalType.Items.FindByValue(ddlFilterType.SelectedValue)
            If it IsNot Nothing Then ddlModalType.SelectedValue = ddlFilterType.SelectedValue
        End If
        ' Pre-select asset kind from filter
        If ddlFilterAsset.SelectedValue <> "" Then
            ddlAssetKind.SelectedValue = ddlFilterAsset.SelectedValue
        End If
        ShowModal()
    End Sub

    ' ────────────────────────────────────────────────────────
    ' SAVE
    ' ────────────────────────────────────────────────────────
    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        lblError.Visible = False

        Dim typeIdStr As String = ddlModalType.SelectedValue.Trim()
        Dim code      As String = txtCode.Text.Trim().ToUpperInvariant()
        Dim name      As String = txtName.Text.Trim()
        Dim assetKind As String = ddlAssetKind.SelectedValue
        Dim order     As Byte   = 99
        Byte.TryParse(txtSortOrder.Text.Trim(), order)

        If typeIdStr = "" Then
            ShowError("S&eacute;lectionnez un type de compteur.")
            ShowModal() : Return
        End If
        If code = "" Then
            ShowError("Le code est obligatoire.")
            ShowModal() : Return
        End If
        If name = "" Then
            ShowError("La d&eacute;signation est obligatoire.")
            ShowModal() : Return
        End If

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand("mro2.usp_CounterDef_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    Dim idObj As Object = DBNull.Value
                    If hfCounterDefId.Value <> "" Then
                        idObj = Convert.ToInt32(hfCounterDefId.Value)
                    End If
                    cmd.Parameters.AddWithValue("@CounterDefId",           idObj)
                    cmd.Parameters.AddWithValue("@CounterTypeId",          CInt(typeIdStr))
                    cmd.Parameters.AddWithValue("@Code",                   code)
                    cmd.Parameters.AddWithValue("@Name",                   name)
                    cmd.Parameters.AddWithValue("@AppliesToAssetKindCode", assetKind)
                    cmd.Parameters.AddWithValue("@SortOrder",              order)
                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using
            BindGrid()
            HideModal()
            ShowToast("Compteur enregistr&eacute;.", "success")
        Catch ex As SqlException When ex.Number = 2627 OrElse ex.Number = 2601
            ShowError("Ce code existe d&eacute;j&agrave; pour ce type.")
            ShowModal()
        Catch ex As Exception
            ShowError(Server.HtmlEncode(ex.Message))
            ShowModal()
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DATA HELPERS
    ' ────────────────────────────────────────────────────────
    Private Sub BindGrid()
        Dim dt As New DataTable()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_CounterDef_List", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@CounterTypeId",
                    If(ddlFilterType.SelectedValue = "",
                       CType(DBNull.Value, Object),
                       CInt(ddlFilterType.SelectedValue)))
                cmd.Parameters.AddWithValue("@AppliesToAssetKindCode",
                    If(ddlFilterAsset.SelectedValue = "",
                       CType(DBNull.Value, Object),
                       ddlFilterAsset.SelectedValue))
                cmd.Parameters.AddWithValue("@IncludeInactive",
                    If(chkIncludeInactive.Checked, 1, 0))
                cn.Open()
                Using da As New SqlDataAdapter(cmd) : da.Fill(dt) : End Using
            End Using
        End Using
        If dt.Columns.Contains(SortColumn) Then
            dt.DefaultView.Sort = SortColumn & " " & SortDir
        End If
        litRowCount.Text = dt.Rows.Count.ToString()
        gvCD.DataSource  = dt
        gvCD.DataBind()
    End Sub

    Private Sub LoadForEdit(ByVal id As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT CounterDefId, CounterTypeId, Code, Name, " &
                "AppliesToAssetKindCode, SortOrder " &
                "FROM mro2.CounterDef WHERE CounterDefId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", id)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfCounterDefId.Value = rdr("CounterDefId").ToString()
                        txtCode.Text         = rdr("Code").ToString()
                        txtName.Text         = rdr("Name").ToString()
                        txtSortOrder.Text    = rdr("SortOrder").ToString()
                        ddlAssetKind.SelectedValue =
                            rdr("AppliesToAssetKindCode").ToString()
                        Dim tid As String = rdr("CounterTypeId").ToString()
                        If ddlModalType.Items.FindByValue(tid) IsNot Nothing Then
                            ddlModalType.SelectedValue = tid
                        End If
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub ToggleActive(ByVal id As Integer)
        Try
            Using cn As New SqlConnection(ConnStr)
                cn.Open()
                Dim cur As Boolean = True
                Using cmdGet As New SqlCommand(
                    "SELECT IsActive FROM mro2.CounterDef WHERE CounterDefId=@Id", cn)
                    cmdGet.Parameters.AddWithValue("@Id", id)
                    Dim o As Object = cmdGet.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                        cur = Convert.ToBoolean(o)
                    End If
                End Using
                Using cmd As New SqlCommand("mro2.usp_CounterDef_SetActive", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@CounterDefId", id)
                    cmd.Parameters.AddWithValue("@IsActive", If(cur, 0, 1))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            ShowToast("Statut mis &agrave; jour.", "success")
        Catch ex As Exception
            ShowToast(Server.HtmlEncode(ex.Message), "error")
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DISPLAY HELPERS (called from ASPX databind)
    ' ────────────────────────────────────────────────────────
    Protected Function AssetBadge(ByVal kind As String) As String
        Select Case kind.ToUpperInvariant()
            Case "AIRCRAFT"
                Return "<span class='badge badge-primary' " &
                       "style='font-size:.72rem;'>AIRCRAFT</span>"
            Case "COMPONENT"
                Return "<span class='badge badge-warning text-dark' " &
                       "style='font-size:.72rem;'>COMPONENT</span>"
            Case Else
                Return "<span class='badge badge-secondary'>" &
                       Server.HtmlEncode(kind) & "</span>"
        End Select
    End Function

    Protected Function StorageBadge(ByVal storage As String) As String
        Select Case storage.ToUpperInvariant()
            Case "MINUTES"
                Return "<span class='badge badge-info' style='font-size:.72rem;'>MIN</span>"
            Case "COUNT"
                Return "<span class='badge badge-secondary' style='font-size:.72rem;'>COUNT</span>"
            Case Else
                Return "<span class='badge badge-light'>" &
                       Server.HtmlEncode(storage) & "</span>"
        End Select
    End Function

    ' ────────────────────────────────────────────────────────
    ' UI HELPERS
    ' ────────────────────────────────────────────────────────
    Private Sub ShowModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "showCD_" & Guid.NewGuid().ToString("N"),
            "$('#cdModal').modal('show');", True)
    End Sub
    Private Sub HideModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "hideCD_" & Guid.NewGuid().ToString("N"),
            "$('#cdModal').modal('hide');", True)
    End Sub
    Private Sub ShowError(ByVal msg As String)
        lblError.Text = msg : lblError.Visible = True
    End Sub
    Private Sub ShowToast(ByVal message As String, ByVal kind As String)
        Dim ser As New JavaScriptSerializer()
        Dim js As String = "if(window.toastr){toastr." &
                           If(kind, "info").ToLowerInvariant() & "(" &
                           ser.Serialize(If(message, "")) & ");}"
        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "toast_" & Guid.NewGuid().ToString("N"), js, True)
    End Sub
End Class
