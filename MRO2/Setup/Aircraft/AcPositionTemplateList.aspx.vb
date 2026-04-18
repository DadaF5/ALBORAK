Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Web.UI.WebControls

' ============================================================
' MRO2/Setup/Aircraft/AcPositionTemplateList.aspx.vb
'
' Tree rendered client-side via jsTree.
' Server-side handles: modal data, save, PN management, copy.
' Single btnDispatch button handles all JS-initiated postbacks.
' Action stored in hfAction hidden field.
' ============================================================
Partial Class MRO2_Setup_Aircraft_AcPositionTemplateList
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString

    ' Exposed to ASPX for JS init
    Public InitialAcTypeId As String = "0"

    Private ReadOnly Property CurrentAcTypeId As Integer
        Get
            Dim id As Integer = 0
            Integer.TryParse(ddlAcType.SelectedValue, id)
            Return id
        End Get
    End Property

    ' ────────────────────────────────────────────────────────
    ' PAGE LOAD
    ' ────────────────────────────────────────────────────────
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            LoadAcTypeDDL()
            LoadATADDL()
            ' Auto-select: prefer the first AcType that already has
            ' template positions defined. Falls back to index 1 if none.
            Dim autoSelectId As String = GetFirstAcTypeWithTemplate()
            If autoSelectId <> "" AndAlso
               ddlAcType.Items.FindByValue(autoSelectId) IsNot Nothing Then
                ddlAcType.SelectedValue = autoSelectId
            ElseIf ddlAcType.Items.Count > 1 Then
                ddlAcType.SelectedIndex = 1
            End If
            UpdateStats()
        Else
            ' Always reload ATA DDL so it's available in modal handlers
            LoadATADDL()
        End If
        InitialAcTypeId = ddlAcType.SelectedValue
    End Sub

    ' ────────────────────────────────────────────────────────
    ' RENDER — register btnDispatch for event validation
    ' Required because JS calls __doPostBack with dynamic
    ' eventArgument values not known at render time.
    ' ────────────────────────────────────────────────────────
    Protected Overrides Sub Render(writer As System.Web.UI.HtmlTextWriter)
        Page.ClientScript.RegisterForEventValidation(
            btnDispatch.UniqueID, "edit")
        Page.ClientScript.RegisterForEventValidation(
            btnDispatch.UniqueID, "toggle")
        Page.ClientScript.RegisterForEventValidation(
            btnDispatch.UniqueID, "addchild")
        Page.ClientScript.RegisterForEventValidation(
            btnDispatch.UniqueID, "openpn")
        MyBase.Render(writer)
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DDL LOADERS
    ' ────────────────────────────────────────────────────────
    Private Sub LoadAcTypeDDL()
        Dim saved As String = ddlAcType.SelectedValue
        ddlAcType.Items.Clear()
        ddlAcType.Items.Add(New ListItem("-- Sélectionner --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT AcTypeId, AcType FROM dbo.tblAcType ORDER BY AcType", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlAcType.Items.Add(New ListItem(
                            rdr("AcType").ToString(),
                            rdr("AcTypeId").ToString()))
                    End While
                End Using
            End Using
        End Using
        If ddlAcType.Items.FindByValue(saved) IsNot Nothing Then
            ddlAcType.SelectedValue = saved
        End If
    End Sub

    Private Sub LoadATADDL()
        ddlNodeATA.Items.Clear()
        ddlNodeATA.Items.Add(New ListItem("-- ATA --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT ATAId, ATACode, Title FROM mro2.ATA " &
                "WHERE IsActive=1 ORDER BY ATACode", cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        ddlNodeATA.Items.Add(New ListItem(
                            rdr("ATACode").ToString() & " - " &
                            rdr("Title").ToString(),
                            rdr("ATAId").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    ' Returns the AcTypeId (as string) of the first type
    ' that has at least one template position defined.
    ' Used to auto-select a meaningful default on page load.
    Private Function GetFirstAcTypeWithTemplate() As String
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT TOP 1 AcTypeId FROM mro2.AcPositionTemplate " &
                "WHERE IsActive=1 ORDER BY AcTypeId", cn)
                cn.Open()
                Dim o As Object = cmd.ExecuteScalar()
                If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                    Return o.ToString()
                End If
                Return ""
            End Using
        End Using
    End Function

    Private Sub LoadAddPNDDL(ByVal tplId As Integer)
        ddlAddPN.Items.Clear()
        ddlAddPN.Items.Add(New ListItem("-- SELECTIONNER PN --", ""))
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT pn.PartNumberId, pn.PN, pn.Nomenclature " &
                "FROM mro2.PartNumber pn " &
                "WHERE pn.IsActive=1 AND pn.IsSerialized=1 " &
                "  AND pn.PartNumberId NOT IN ( " &
                "      SELECT pp.PartNumberId FROM mro2.AcPositionPN pp " &
                "      WHERE pp.AcPositionTemplateId=@Id AND pp.IsActive=1) " &
                "ORDER BY pn.PN", cn)
                cmd.Parameters.AddWithValue("@Id", tplId)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim nom As String = SafeStr(rdr("Nomenclature"))
                        If nom.Length > 45 Then nom = nom.Substring(0, 43) & "…"
                        ddlAddPN.Items.Add(New ListItem(
                            rdr("PN").ToString() & " - " & nom,
                            rdr("PartNumberId").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    ' ────────────────────────────────────────────────────────
    ' STATS (badge counts in toolbar)
    ' ────────────────────────────────────────────────────────
    Private Sub UpdateStats()
        If CurrentAcTypeId = 0 Then
            litTemplateCount.Text = ""
            Return
        End If
        Dim total, slots As Integer
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT COUNT(*) FROM mro2.AcPositionTemplate " &
                "WHERE AcTypeId=@Id AND IsActive=1", cn)
                cmd.Parameters.AddWithValue("@Id", CurrentAcTypeId)
                cn.Open()
                total = CInt(cmd.ExecuteScalar())
            End Using
            Using cmd As New SqlCommand(
                "SELECT COUNT(*) FROM mro2.AcPositionTemplate " &
                "WHERE AcTypeId=@Id AND PositionLevel=3 AND IsActive=1", cn)
                cmd.Parameters.AddWithValue("@Id", CurrentAcTypeId)
                slots = CInt(cmd.ExecuteScalar())
            End Using
        End Using
        litTemplateCount.Text =
            "<span class='badge badge-primary ml-1'>" & total & " positions</span>" &
            "<span class='badge badge-secondary ml-1'>" & slots & " slots</span>"
    End Sub

    ' ────────────────────────────────────────────────────────
    ' ACTYPE CHANGE
    ' ────────────────────────────────────────────────────────
    Protected Sub ddlAcType_Changed(sender As Object, e As EventArgs) _
            Handles ddlAcType.SelectedIndexChanged
        UpdateStats()
        InitialAcTypeId = ddlAcType.SelectedValue
        ' Signal JS to reload tree
        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "reloadTree", "window._needTreeReload=true;", True)
    End Sub

    ' ────────────────────────────────────────────────────────
    ' SINGLE DISPATCH BUTTON
    ' Handles all JS-initiated postbacks via hfAction value
    ' ────────────────────────────────────────────────────────
    Protected Sub btnDispatch_Click(sender As Object, e As EventArgs) _
            Handles btnDispatch.Click

        Dim action As String = hfAction.Value.Trim().ToLowerInvariant()

        Select Case action
            Case "edit"
                Dim nodeId As Integer = 0
                Integer.TryParse(hfNodeId.Value, nodeId)
                If nodeId > 0 Then
                    LoadATADDL()        ' must load before LoadNodeForEdit sets SelectedValue
                    LoadNodeForEdit(nodeId)
                    ShowModal("nodeModal")
                End If

            Case "toggle"
                Dim nodeId As Integer = 0
                Integer.TryParse(hfNodeId.Value, nodeId)
                If nodeId > 0 Then
                    ToggleNodeActive(nodeId)
                    UpdateStats()
                    ReloadTree()
                End If

            Case "addchild"
                LoadATADDL()        ' populate ATA DDL before clearing form
                ClearNodeModal()
                Dim parentId As Integer = 0
                Dim level As Integer = 1
                Dim parentCode As String = hfNodeParentCode.Value
                Integer.TryParse(hfNodeParentId.Value, parentId)
                Integer.TryParse(hfNodeLevel.Value, level)
                hfNodeParentId.Value = parentId.ToString()
                hfNodeLevel.Value = level.ToString()
                litNodeModalTitle.Text =
                    If(level = 1, "Nouvelle Zone",
                    If(level = 2, "Nouveau Syst&egrave;me sous " & parentCode,
                                  "Nouveau Slot sous " & parentCode))
                pnlSlotFields.Visible = (level = 3)
                ShowModal("nodeModal")

            Case "openpn"
                Dim tplId As Integer = 0
                Integer.TryParse(hfPNSlotTemplateId.Value, tplId)
                If tplId > 0 Then
                    ' Get slot code for modal title
                    Dim slotCode As String = GetPositionCode(tplId)
                    litPNModalSlot.Text = slotCode
                    LoadSlotPNGrid(tplId)
                    LoadAddPNDDL(tplId)
                    lblPNError.Visible = False
                    chkAddPNPrimary.Checked = True
                    ShowModal("pnModal")
                End If
        End Select

        hfAction.Value = ""  ' clear after dispatch
    End Sub

    ' ────────────────────────────────────────────────────────
    ' ADD ZONE BUTTON
    ' ────────────────────────────────────────────────────────
    Protected Sub btnAddZone_Click(sender As Object, e As EventArgs) _
            Handles btnAddZone.Click
        If CurrentAcTypeId = 0 Then Return
        ClearNodeModal()
        hfNodeLevel.Value = "1"
        hfNodeParentId.Value = "0"
        litNodeModalTitle.Text = "Nouvelle Zone"
        pnlSlotFields.Visible = False
        ShowModal("nodeModal")
    End Sub

    ' ────────────────────────────────────────────────────────
    ' SAVE NODE
    ' ────────────────────────────────────────────────────────
    Protected Sub btnNodeSave_Click(sender As Object, e As EventArgs) _
            Handles btnNodeSave.Click
        lblNodeError.Visible = False

        Dim nodeId As Integer = 0
        Dim parentId As Integer = 0
        Dim level As Integer = 1
        Integer.TryParse(hfNodeId.Value, nodeId)
        Integer.TryParse(hfNodeParentId.Value, parentId)
        Integer.TryParse(hfNodeLevel.Value, level)

        Dim code As String = txtNodeCode.Text.Trim().ToUpperInvariant()
        Dim desc As String = txtNodeDesc.Text.Trim()
        Dim sort As Integer = 100 : Integer.TryParse(txtNodeSort.Text.Trim(), sort)
        Dim qty As Integer = 1 : Integer.TryParse(txtNodeQty.Text.Trim(), qty)
        Dim ataId As Object = DBNull.Value
        If ddlNodeATA.SelectedValue <> "" Then
            ataId = CInt(ddlNodeATA.SelectedValue)
        End If

        If code = "" Then
            ShowNodeError("Le code est obligatoire.")
            Return
        End If
        If CurrentAcTypeId = 0 Then
            ShowNodeError("Sélectionnez un type d'aéronef.")
            Return
        End If

        Try
            Dim idParam As Object =
                If(nodeId > 0, CType(nodeId, Object), DBNull.Value)
            Dim parentParam As Object =
                If(parentId > 0, CType(parentId, Object), DBNull.Value)

            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand(
                        "mro2.usp_AcPositionTemplate_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@AcPositionTemplateId", idParam)
                    cmd.Parameters.AddWithValue("@AcTypeId", CurrentAcTypeId)
                    cmd.Parameters.AddWithValue("@ParentTemplatePositionId", parentParam)
                    cmd.Parameters.AddWithValue("@PositionLevel", level)
                    cmd.Parameters.AddWithValue("@PositionCode", code)
                    cmd.Parameters.AddWithValue("@Description",
                        If(desc = "", CType(DBNull.Value, Object), desc))
                    cmd.Parameters.AddWithValue("@ATAId", ataId)
                    cmd.Parameters.AddWithValue("@Quantity", qty)
                    cmd.Parameters.AddWithValue("@IsInterchangeable",
                        If(chkInterchangeable.Checked, 1, 0))
                    cmd.Parameters.AddWithValue("@SortOrder", sort)
                    cmd.Parameters.AddWithValue("@UserId",
                        If(Session("UserId") IsNot Nothing,
                           Session("UserId").ToString(), "admin"))
                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using

            UpdateStats()
            HideModal("nodeModal")
            ReloadTree()
            ShowToast("Position enregistrée.", "success")

        Catch ex As SqlException When ex.Number = 2627 OrElse ex.Number = 2601
            ShowNodeError("Ce code existe déjà pour ce type d'aéronef.")
        Catch ex As Exception
            ShowNodeError(Server.HtmlEncode(ex.Message))
        End Try
    End Sub

    ' ────────────────────────────────────────────────────────
    ' ADD PN
    ' ────────────────────────────────────────────────────────
    Protected Sub btnAddPN_Click(sender As Object, e As EventArgs) _
            Handles btnAddPN.Click
        lblPNError.Visible = False

        Dim tplId As Integer = 0
        Dim pnId As Integer = 0
        Integer.TryParse(hfPNSlotTemplateId.Value, tplId)
        Integer.TryParse(ddlAddPN.SelectedValue, pnId)

        If tplId = 0 OrElse pnId = 0 Then
            lblPNError.Text = "Sélectionnez un Part Number."
            lblPNError.Visible = True
            ShowModal("pnModal") : Return
        End If

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand(
                        "mro2.usp_AcPositionPN_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@AcPositionPNId", DBNull.Value)
                    cmd.Parameters.AddWithValue("@AcPositionTemplateId", tplId)
                    cmd.Parameters.AddWithValue("@PartNumberId", pnId)
                    cmd.Parameters.AddWithValue("@IsPrimary",
                        If(chkAddPNPrimary.Checked, 1, 0))
                    cmd.Parameters.AddWithValue("@Notes", DBNull.Value)
                    cmd.Parameters.AddWithValue("@UserId",
                        If(Session("UserId") IsNot Nothing,
                           Session("UserId").ToString(), "admin"))
                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using
            LoadSlotPNGrid(tplId)
            LoadAddPNDDL(tplId)
            ReloadTree()   ' refresh PN count badge on node
            ShowToast("PN lié.", "success")
            ShowModal("pnModal")

        Catch ex As SqlException When ex.Number = 2627 OrElse ex.Number = 2601
            lblPNError.Text = "Ce PN est déjà lié à ce slot."
            lblPNError.Visible = True
            ShowModal("pnModal")
        Catch ex As Exception
            lblPNError.Text = Server.HtmlEncode(ex.Message)
            lblPNError.Visible = True
            ShowModal("pnModal")
        End Try
    End Sub

    Protected Sub gvSlotPN_RowCommand(sender As Object,
            e As GridViewCommandEventArgs) Handles gvSlotPN.RowCommand
        If e.CommandName <> "RemovePN" Then Return
        Dim ppId As Integer = Convert.ToInt32(e.CommandArgument)
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                    "mro2.usp_AcPositionPN_SetActive", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@AcPositionPNId", ppId)
                cmd.Parameters.AddWithValue("@IsActive", 0)
                cn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using
        Dim tplId As Integer = 0
        Integer.TryParse(hfPNSlotTemplateId.Value, tplId)
        If tplId > 0 Then
            LoadSlotPNGrid(tplId)
            LoadAddPNDDL(tplId)
        End If
        ReloadTree()
        ShowToast("PN retiré.", "success")
        ShowModal("pnModal")
    End Sub

    ' ────────────────────────────────────────────────────────
    ' COPY TO TAILS
    ' ────────────────────────────────────────────────────────
    Protected Sub btnCopyToTails_Click(sender As Object, e As EventArgs) _
            Handles btnCopyToTails.Click
        If CurrentAcTypeId = 0 Then Return
        litCopyAcType.Text = ddlAcType.SelectedItem.Text
        cblTails.Items.Clear()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT AcID, TailNo FROM dbo.tblAircraft " &
                "WHERE AcTypeID=@TypeId AND IsActive=1 ORDER BY TailNo", cn)
                cmd.Parameters.AddWithValue("@TypeId", CurrentAcTypeId)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim li As New ListItem(
                            rdr("TailNo").ToString(),
                            rdr("AcID").ToString())
                        li.Selected = True
                        cblTails.Items.Add(li)
                    End While
                End Using
            End Using
        End Using
        lblCopyResult.Visible = False
        ShowModal("copyModal")
    End Sub

    Protected Sub btnCopyConfirm_Click(sender As Object, e As EventArgs) _
            Handles btnCopyConfirm.Click
        Dim userId As String =
            If(Session("UserId") IsNot Nothing,
               Session("UserId").ToString(), "admin")
        Dim copied As Integer = 0
        Dim errors As New StringBuilder()

        For Each item As ListItem In cblTails.Items
            If Not item.Selected Then Continue For
            Try
                Using cn As New SqlConnection(ConnStr)
                    Using cmd As New SqlCommand(
                            "mro2.usp_AcPosition_CopyFromTemplate", cn)
                        cmd.CommandType = CommandType.StoredProcedure
                        cmd.Parameters.AddWithValue("@AcID", CInt(item.Value))
                        cmd.Parameters.AddWithValue("@UserId", userId)
                        cn.Open()
                        cmd.ExecuteNonQuery()
                        copied += 1
                    End Using
                End Using
            Catch ex As Exception
                errors.Append(item.Text & " | ")
            End Try
        Next

        lblCopyResult.CssClass =
            If(errors.Length = 0,
               "alert alert-success d-block py-2 px-3 mt-2",
               "alert alert-warning d-block py-2 px-3 mt-2")
        lblCopyResult.Text = copied & " avion(s) mis à jour." &
            If(errors.Length > 0, " Erreurs: " & errors.ToString(), "")
        lblCopyResult.Visible = True
        ShowToast(copied & " avion(s) mis à jour.", "success")
        ShowModal("copyModal")
    End Sub

    ' ────────────────────────────────────────────────────────
    ' DATA HELPERS
    ' ────────────────────────────────────────────────────────
    Private Sub LoadSlotPNGrid(ByVal tplId As Integer)
        Dim dt As New DataTable()
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                    "mro2.usp_AcPositionPN_List", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@AcPositionTemplateId", tplId)
                cmd.Parameters.AddWithValue("@IncludeInactive", 0)
                cn.Open()
                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using
        gvSlotPN.DataSource = dt
        gvSlotPN.DataBind()
    End Sub

    Private Sub LoadNodeForEdit(ByVal id As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT AcPositionTemplateId, ParentTemplatePositionId, " &
                "PositionLevel, PositionCode, Description, ATAId, " &
                "Quantity, IsInterchangeable, SortOrder " &
                "FROM mro2.AcPositionTemplate " &
                "WHERE AcPositionTemplateId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", id)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        Dim lvl As Integer = CInt(rdr("PositionLevel"))
                        hfNodeId.Value = id.ToString()
                        hfNodeParentId.Value =
                            If(rdr("ParentTemplatePositionId") Is DBNull.Value,
                               "0", rdr("ParentTemplatePositionId").ToString())
                        hfNodeLevel.Value = lvl.ToString()
                        txtNodeCode.Text = rdr("PositionCode").ToString()
                        txtNodeDesc.Text =
                            If(rdr("Description") Is DBNull.Value, "",
                               rdr("Description").ToString())
                        txtNodeSort.Text = rdr("SortOrder").ToString()
                        txtNodeQty.Text = rdr("Quantity").ToString()
                        chkInterchangeable.Checked =
                            Convert.ToBoolean(rdr("IsInterchangeable"))
                        Dim ataVal As String =
                            If(rdr("ATAId") Is DBNull.Value, "",
                               rdr("ATAId").ToString())
                        If ddlNodeATA.Items.FindByValue(ataVal) IsNot Nothing Then
                            ddlNodeATA.SelectedValue = ataVal
                        End If
                        pnlSlotFields.Visible = (lvl = 3)
                        Dim labels() As String = {
                            "", "Modifier Zone",
                            "Modifier Système", "Modifier Slot"}
                        litNodeModalTitle.Text = labels(lvl)
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub ToggleNodeActive(ByVal id As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Dim cur As Boolean = True
            Using g As New SqlCommand(
                "SELECT IsActive FROM mro2.AcPositionTemplate " &
                "WHERE AcPositionTemplateId=@Id", cn)
                g.Parameters.AddWithValue("@Id", id)
                Dim o As Object = g.ExecuteScalar()
                If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                    cur = Convert.ToBoolean(o)
                End If
            End Using
            Using u As New SqlCommand(
                "UPDATE mro2.AcPositionTemplate SET IsActive=@v " &
                "WHERE AcPositionTemplateId=@Id", cn)
                u.Parameters.AddWithValue("@v", If(cur, 0, 1))
                u.Parameters.AddWithValue("@Id", id)
                u.ExecuteNonQuery()
            End Using
        End Using
        ShowToast("Statut mis à jour.", "success")
    End Sub

    Private Function GetPositionCode(ByVal tplId As Integer) As String
        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand(
                "SELECT PositionCode FROM mro2.AcPositionTemplate " &
                "WHERE AcPositionTemplateId=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", tplId)
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

    Private Sub ClearNodeModal()
        hfNodeId.Value = ""
        hfNodeParentId.Value = "0"
        hfNodeLevel.Value = "1"
        hfNodeParentCode.Value = ""
        txtNodeCode.Text = ""
        txtNodeDesc.Text = ""
        txtNodeSort.Text = "100"
        txtNodeQty.Text = "1"
        chkInterchangeable.Checked = False
        lblNodeError.Visible = False
    End Sub

    ' ── Signals JS to reload the tree after postback ──────
    Private Sub ReloadTree()
        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "rldTree_" & Guid.NewGuid().ToString("N"),
            "window._needTreeReload=true;", True)
    End Sub

    Private Sub ShowNodeError(ByVal msg As String)
        lblNodeError.Text = msg
        lblNodeError.Visible = True
        ShowModal("nodeModal")
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