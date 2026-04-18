<%@ Page Language="VB" AutoEventWireup="false" MasterPageFile="~/MRO2/mro2.master"
    CodeFile="PartNumberList.aspx.vb" Inherits="MRO2_Setup_Components_PartNumberList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Part Numbers
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Setup/Default.aspx") %>">Setup</a>
    </li>
    <li class="breadcrumb-item active">Part Numbers</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- ═══════════════════════════════════════════
                 CARD - PN LIST
            ═══════════════════════════════════════════ --%>
            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        <i class="fas fa-barcode mr-1"></i>
                        Part Numbers
                        <small class="text-muted">
                            (<asp:Literal ID="litRowCount" runat="server" Text="0" /> rows)
                        </small>
                    </h3>
                    <div class="card-tools">
                        <div class="input-group input-group-sm"
                             style="width:520px;display:inline-flex;">
                            <asp:TextBox ID="txtSearch" runat="server"
                                CssClass="form-control"
                                placeholder="Search PN, nomenclature, ATA..." />
                            <div class="input-group-append">
                                <asp:Button ID="btnSearch" runat="server"
                                    Text="Search" CssClass="btn btn-primary" />
                                <asp:Button ID="btnClear"  runat="server"
                                    Text="Clear"  CssClass="btn btn-secondary" />
                            </div>
                        </div>
                      <asp:DropDownList ID="ddlFilterAcMainGroup" runat="server"
                        CssClass="form-control form-control-sm d-inline-block"
                        AutoPostBack="true" style="width:220px;" />
                    &nbsp;
                    <asp:DropDownList ID="ddlMaxRows" runat="server"
                        CssClass="form-control form-control-sm d-inline-block"
                        AutoPostBack="true" style="width:120px;">
                        <asp:ListItem Text="Top 50"  Value="50" />
                        <asp:ListItem Text="Top 100" Value="100" Selected="True" />
                        <asp:ListItem Text="Top 200" Value="200" />
                    </asp:DropDownList>
                        &nbsp;&nbsp;
                        <asp:CheckBox ID="chkIncludeInactive" runat="server"
                            AutoPostBack="true" Text="Include inactive" />
                        &nbsp;
                        <asp:Button ID="btnNew" runat="server"
                            CssClass="btn btn-sm btn-success" Text="+ New" />
                    </div>
                </div>

                <div class="card-body p-0">
                    <asp:GridView ID="gvPN" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-sm table-hover mb-0"
                        GridLines="None"
                        DataKeyNames="PartNumberId"
                        AllowPaging="true"
                        PageSize="25"
                        AllowSorting="true">
                        <Columns>

                            <asp:BoundField DataField="PN"
                                HeaderText="PN" SortExpression="PN"
                                ItemStyle-CssClass="font-weight-bold" />

                            <asp:BoundField DataField="Nomenclature"
                                HeaderText="Nomenclature" SortExpression="Nomenclature" />

                            <asp:BoundField DataField="ATACode"
                                HeaderText="ATA" SortExpression="ATACode"
                                ItemStyle-CssClass="text-center"
                                HeaderStyle-CssClass="text-center" />

                            <asp:BoundField DataField="UOMCode"
                                HeaderText="UOM"
                                ItemStyle-CssClass="text-center"
                                HeaderStyle-CssClass="text-center" />

                            <asp:TemplateField HeaderText="Serialized"
                                SortExpression="IsSerialized"
                                ItemStyle-CssClass="text-center"
                                HeaderStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <i class='<%# If(Convert.ToBoolean(Eval("IsSerialized")),
                                                    "fas fa-check-circle text-success",
                                                    "fas fa-minus-circle text-secondary") %>'
                                       title='<%# If(Convert.ToBoolean(Eval("IsSerialized")),
                                                    "Serialized","Bulk") %>'></i>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- ── LIMITS BADGE COLUMN ── --%>
                            <asp:TemplateField HeaderText="Limits"
                                ItemStyle-CssClass="text-center"
                                HeaderStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# LimitBadge(CInt(Eval("PartNumberId")),
                                                   CInt(Eval("LimitCount"))) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Active"
                                ItemStyle-CssClass="text-center"
                                HeaderStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <span class='badge <%# If(Convert.ToBoolean(Eval("IsActive")),
                                                            "badge-success","badge-secondary") %>'>
                                        <%# If(Convert.ToBoolean(Eval("IsActive")),
                                                "Active","Inactive") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Actions"
                                ItemStyle-CssClass="text-center"
                                HeaderStyle-CssClass="text-center">
                                <ItemTemplate>

                                    <%-- Edit PN --%>
                                    <asp:LinkButton ID="lnkEdit" runat="server"
                                        CommandName="EditRow"
                                        CommandArgument='<%# Eval("PartNumberId") %>'
                                        CssClass="btn btn-xs btn-outline-primary"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i> Edit
                                    </asp:LinkButton>

                                    <%-- Manage Limits — only for serialized PNs --%>
                                    <asp:LinkButton ID="lnkLimits" runat="server"
                                        CommandName="ManageLimits"
                                        CommandArgument='<%# Eval("PartNumberId") %>'
                                        CssClass='<%# If(Convert.ToBoolean(Eval("IsSerialized")),
                                                        "btn btn-xs btn-outline-warning ml-1",
                                                        "btn btn-xs btn-outline-secondary ml-1") %>'
                                        CausesValidation="false"
                                        Visible='<%# Convert.ToBoolean(Eval("IsSerialized")) %>'>
                                        <i class="fas fa-clock"></i> Limits
                                    </asp:LinkButton>

                                    <%-- Toggle active --%>
                                    <asp:LinkButton ID="lnkToggle" runat="server"
                                        CommandName="ToggleActive"
                                        CommandArgument='<%# Eval("PartNumberId") %>'
                                        CssClass='<%# If(Convert.ToBoolean(Eval("IsActive")),
                                                        "btn btn-xs btn-outline-danger ml-1",
                                                        "btn btn-xs btn-outline-success ml-1") %>'
                                        CausesValidation="false"
                                        OnClientClick="return confirm('Change active status?');">
                                        <%# If(Convert.ToBoolean(Eval("IsActive")),
                                                "Deactivate","Activate") %>
                                    </asp:LinkButton>

                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                        <HeaderStyle CssClass="bg-primary text-white" />
                        <PagerStyle CssClass="bg-light" />
                        <EmptyDataTemplate>
                            <div class="text-center text-muted py-4">
                                <i class="fas fa-inbox fa-2x mb-2"></i><br />
                                No part numbers found.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <%-- Hidden fields --%>
            <asp:HiddenField ID="hfPartNumberId"  runat="server" />
            <asp:HiddenField ID="hfLimitPNId"     runat="server" />
            <asp:HiddenField ID="hfPNLimitId"     runat="server" />

            <%-- ═══════════════════════════════════════════
                 MODAL 1 — ADD / EDIT PART NUMBER
            ═══════════════════════════════════════════ --%>
            <div class="modal fade" id="pnModal" tabindex="-1"
                 role="dialog" aria-hidden="true">
                <div class="modal-dialog modal-lg" role="document">
                    <div class="modal-content">

                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-barcode mr-1"></i>
                                <asp:Literal ID="litModalTitle"
                                    runat="server" Text="New Part Number" />
                            </h5>
                            <button type="button" class="close text-white"
                                    data-dismiss="modal"><span>&times;</span></button>
                        </div>

                        <div class="modal-body">
                            <div class="form-row">
                                <div class="form-group col-md-4">
                                    <label>PN <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtPN" runat="server"
                                        CssClass="form-control form-control-sm"
                                        MaxLength="60" />
                                </div>
                                <div class="form-group col-md-8">
                                    <label>Nomenclature</label>
                                    <asp:TextBox ID="txtNomenclature" runat="server"
                                        CssClass="form-control form-control-sm"
                                        MaxLength="200" />
                                </div>
                            </div>
                            <div class="form-row">
                                <div class="form-group col-md-4">
                                    <label>ATA</label>
                                    <asp:DropDownList ID="ddlATA" runat="server"
                                        CssClass="form-control form-control-sm" />
                                </div>
                                <div class="form-group col-md-4">
                                    <label>UOM <span class="text-danger">*</span></label>
                                    <asp:DropDownList ID="ddlUOM" runat="server"
                                        CssClass="form-control form-control-sm" />
                                </div>
                                <div class="form-group col-md-4">
                                    <label>Serialized</label>
                                    <asp:DropDownList ID="ddlIsSerialized" runat="server"
                                        CssClass="form-control form-control-sm">
                                        <asp:ListItem Text="Yes" Value="1" Selected="True" />
                                        <asp:ListItem Text="No"  Value="0" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="form-group">
                                <label>AC Main Group
                                    <span class="text-danger">*</span>
                                    <small class="text-muted">(required for serialized PNs)</small>
                                </label>
                                <asp:DropDownList ID="ddlAcMainGroup" runat="server"
                                    CssClass="form-control form-control-sm" />
                            </div>

                            <asp:Label ID="lblError" runat="server" Visible="false"
                                CssClass="alert alert-danger d-block py-2 px-3" />
                        </div>

                        <div class="modal-footer">
                            <asp:Button ID="btnSave" runat="server"
                                CssClass="btn btn-success" Text="Save" />
                            <button type="button" class="btn btn-secondary"
                                    data-dismiss="modal">Cancel</button>
                        </div>
                    </div>
                </div>
            </div>

            <%-- ═══════════════════════════════════════════
                 MODAL 2 — MANAGE PN LIMITS
                 Layout:
                   Header  : PN code + nomenclature
                   Body top: existing limits grid (sortable)
                   Body mid: add/edit form (cascade CounterType→Counter)
                   Body bot: error label
                 Footer  : Close only (saves are inline)
            ═══════════════════════════════════════════ --%>
            <div class="modal fade" id="limitsModal" tabindex="-1"
                 role="dialog" aria-hidden="true" data-backdrop="static">
                <div class="modal-dialog modal-xl" role="document">
                    <div class="modal-content">

                        <%-- Header --%>
                        <div class="modal-header"
                             style="background:#1d4ed8;color:#fff;">
                            <h5 class="modal-title">
                                <i class="fas fa-clock mr-1"></i>
                                Life Limits &mdash;
                                <asp:Literal ID="litLimitPN" runat="server" />
                                <small style="opacity:.75;">
                                    <asp:Literal ID="litLimitNom" runat="server" />
                                </small>
                            </h5>
                            <button type="button" class="close"
                                    style="color:#fff;" data-dismiss="modal">
                                <span>&times;</span>
                            </button>
                        </div>

                        <div class="modal-body">

                            <%-- ── Existing limits grid ── --%>
                            <h6 class="text-uppercase text-muted mb-2"
                                style="font-size:.75rem;letter-spacing:.05em;">
                                <i class="fas fa-list mr-1"></i>Defined Limits
                            </h6>

                            <asp:GridView ID="gvLimits" runat="server"
                                AutoGenerateColumns="false"
                                CssClass="table table-sm table-bordered mb-3"
                                GridLines="None"
                                DataKeyNames="PNLimitId"
                                EmptyDataText="Aucune limite d&eacute;finie pour ce PN.">
                                <Columns>

                                    <%-- LimitType badge --%>
                                    <asp:TemplateField HeaderText="Type"
                                        ItemStyle-CssClass="text-center"
                                        HeaderStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <span class='badge badge-<%# Eval("BadgeColor") %>'
                                                  style="font-size:.73rem;">
                                                <%# Eval("LimitTypeCode") %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- CounterDef code --%>
                                    <asp:TemplateField HeaderText="Compteur"
                                        ItemStyle-CssClass="font-weight-bold">
                                        <ItemTemplate>
                                            <%# Eval("CounterDefCode") %>
                                            <small class="text-muted d-block" style="font-size:.72rem;">
                                                <%# Eval("CounterBasisCode") %>
                                            </small>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- Hard limit --%>
                                    <asp:TemplateField HeaderText="Limite"
                                        ItemStyle-CssClass="text-center"
                                        HeaderStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <strong><%# FormatHardLimit(Eval("HardLimit"),
                                                                        Eval("DisplayUnit")) %></strong>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- Alert threshold --%>
                                    <asp:TemplateField HeaderText="Alerte"
                                        ItemStyle-CssClass="text-center"
                                        HeaderStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <%# FormatAlert(Eval("HardLimit"),
                                                            Eval("AlertThresholdPct"),
                                                            Eval("DisplayUnit")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- TaskCounter count → link to detail page --%>
                                    <asp:TemplateField HeaderText="Compteurs t&acirc;che"
                                        ItemStyle-CssClass="text-center"
                                        HeaderStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <a href='<%# ResolveUrl("~/MRO2/Setup/Components/PartNumberLimitDetail.aspx") &
                                                        "?PNLimitId=" & Eval("PNLimitId") %>'
                                               class='<%# TaskCounterBadgeClass(CInt(Eval("SNCount"))) %>'
                                               style="font-size:.73rem;">
                                                <i class="fas fa-stopwatch mr-1"></i>
                                                <%# Eval("SNCount") %> compteur(s)
                                            </a>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- Active --%>
                                    <asp:TemplateField HeaderText="Statut"
                                        ItemStyle-CssClass="text-center"
                                        HeaderStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <span class='badge <%# If(Convert.ToBoolean(Eval("IsActive")),
                                                                    "badge-success","badge-secondary") %>'>
                                                <%# If(Convert.ToBoolean(Eval("IsActive")),
                                                        "Actif","Inactif") %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- Actions --%>
                                    <asp:TemplateField HeaderText=""
                                        ItemStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkEditLimit" runat="server"
                                                CommandName="EditLimit"
                                                CommandArgument='<%# Eval("PNLimitId") %>'
                                                CssClass="btn btn-xs btn-outline-primary"
                                                CausesValidation="false"
                                                title="Modifier">
                                                <i class="fas fa-edit"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="lnkToggleLimit" runat="server"
                                                CommandName="ToggleLimit"
                                                CommandArgument='<%# Eval("PNLimitId") %>'
                                                CssClass='<%# If(Convert.ToBoolean(Eval("IsActive")),
                                                                "btn btn-xs btn-outline-danger ml-1",
                                                                "btn btn-xs btn-outline-success ml-1") %>'
                                                CausesValidation="false"
                                                OnClientClick="return confirm('Confirmer ?');">
                                                <%# If(Convert.ToBoolean(Eval("IsActive")),
                                                        "<i class=""fas fa-ban""></i>",
                                                        "<i class=""fas fa-check""></i>") %>
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                </Columns>
                                <HeaderStyle CssClass="bg-light text-dark" />
                            </asp:GridView>

                            <%-- ── Divider ── --%>
                            <hr class="my-3" />

                            <%-- ── Add / Edit limit form ── --%>
                            <h6 class="text-uppercase text-muted mb-3"
                                style="font-size:.75rem;letter-spacing:.05em;">
                                <i class="fas fa-plus-circle mr-1"></i>
                                <asp:Literal ID="litFormTitle"
                                    runat="server" Text="Add New Limit" />
                            </h6>

                            <div class="form-row align-items-end">

                                <%-- LimitType --%>
                                <div class="form-group col-md-2">
                                    <label class="small font-weight-bold">
                                        Type de limite <span class="text-danger">*</span>
                                    </label>
                                    <asp:DropDownList ID="ddlLimitLimitType" runat="server"
                                        CssClass="form-control form-control-sm" />
                                </div>

                                <%-- CounterType (cascade root) --%>
                                <div class="form-group col-md-2">
                                    <label class="small font-weight-bold">
                                        Type compteur <span class="text-danger">*</span>
                                    </label>
                                    <asp:DropDownList ID="ddlLimitType" runat="server"
                                        CssClass="form-control form-control-sm"
                                        AutoPostBack="true"
                                        OnSelectedIndexChanged="ddlLimitType_SelectedIndexChanged" />
                                </div>

                                <%-- CounterDef (cascades from CounterType) --%>
                                <div class="form-group col-md-2">
                                    <label class="small font-weight-bold">
                                        Compteur <span class="text-danger">*</span>
                                    </label>
                                    <asp:DropDownList ID="ddlLimitCounter" runat="server"
                                        CssClass="form-control form-control-sm" />
                                </div>

                                <%-- Hard Limit --%>
                                <div class="form-group col-md-2">
                                    <label class="small font-weight-bold">
                                        Hard Limit <span class="text-danger">*</span>
                                    </label>
                                    <div class="input-group input-group-sm">
                                        <asp:TextBox ID="txtHardLimit" runat="server"
                                            CssClass="form-control form-control-sm"
                                            placeholder="e.g. 500" />
                                        <div class="input-group-append">
                                            <span class="input-group-text px-1"
                                                  style="font-size:.75rem;">
                                                <asp:Literal ID="litUnit"
                                                    runat="server" Text="&mdash;" />
                                            </span>
                                        </div>
                                    </div>
                                </div>

                                <%-- Alert % --%>
                                <div class="form-group col-md-2">
                                    <label class="small font-weight-bold">
                                        Alert at %
                                        <span class="text-muted">(1–99)</span>
                                    </label>
                                    <div class="input-group input-group-sm">
                                        <asp:TextBox ID="txtAlertPct" runat="server"
                                            CssClass="form-control form-control-sm"
                                            Text="90" MaxLength="2" />
                                        <div class="input-group-append">
                                            <span class="input-group-text">%</span>
                                        </div>
                                    </div>
                                    <%-- Live preview of alert value --%>
                                    <small class="text-muted" id="spanAlertCalc">
                                        &rarr; alert at
                                        <asp:Literal ID="litAlertCalc"
                                            runat="server" Text="?" />
                                    </small>
                                </div>

                                <%-- CounterBasis --%>
                                <div class="form-group col-md-2">
                                    <label class="small font-weight-bold">
                                        Base <span class="text-danger">*</span>
                                    </label>
                                    <asp:DropDownList ID="ddlLimitBasis" runat="server"
                                        CssClass="form-control form-control-sm" />
                                    <small class="form-text text-muted" style="font-size:.7rem;">
                                        Depuis quand
                                    </small>
                                </div>

                            </div>

                            <%-- Save / Cancel form buttons --%>
                            <div class="d-flex align-items-center">
                                <asp:Button ID="btnSaveLimit" runat="server"
                                    CssClass="btn btn-sm btn-success mr-2"
                                    Text="Save Limit" />
                                <asp:Button ID="btnCancelEdit" runat="server"
                                    CssClass="btn btn-sm btn-outline-secondary"
                                    Text="Cancel"
                                    CausesValidation="false" />
                                <span class="ml-3 text-muted small">
                                    One limit per counter type/code combination.
                                </span>
                            </div>

                            <asp:Label ID="lblLimitError" runat="server"
                                Visible="false"
                                CssClass="alert alert-danger d-block py-2 px-3 mt-3" />

                        </div>

                        <div class="modal-footer bg-light">
                            <small class="text-muted mr-auto">
                                <i class="fas fa-info-circle mr-1"></i>
                                Changes are saved immediately.
                                SN overrides and counter status &rarr;
                                <a href="#" class="text-primary">SN Limit Status page</a>.
                            </small>
                            <button type="button" class="btn btn-secondary"
                                    data-dismiss="modal">Close</button>
                        </div>

                    </div>
                </div>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>

<asp:Content ID="cFooter" ContentPlaceHolderID="FooterScripts" runat="server">
    <script type="text/javascript">
        // Live alert-value preview: recalculate when hard limit or pct changes
        $(document).on('input change',
            '#<%= txtHardLimit.ClientID %>, #<%= txtAlertPct.ClientID %>',
            function () {
                var lim = parseFloat($('#<%= txtHardLimit.ClientID %>').val()) || 0;
                var pct = parseFloat($('#<%= txtAlertPct.ClientID %>').val()) || 0;
                var val = (lim * pct / 100).toFixed(1);
                $('#<%= litAlertCalc.ClientID %>').text(val);
            });
    </script>
</asp:Content>
