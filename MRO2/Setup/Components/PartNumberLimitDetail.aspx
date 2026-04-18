<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="PartNumberLimitDetail.aspx.vb"
    Inherits="MRO2_Setup_Components_PartNumberLimitDetail" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    D&eacute;tail Limite PN
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Setup/Default.aspx") %>">Setup</a>
    </li>
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Setup/Components/PartNumberList.aspx") %>">
            Part Numbers
        </a>
    </li>
    <li class="breadcrumb-item active">D&eacute;tail Limite</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional"
                     ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- ═══ NOT FOUND ═══ --%>
            <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
                <div class="alert alert-warning">
                    <i class="fas fa-exclamation-triangle mr-1"></i>
                    Limite introuvable. Veuillez revenir &agrave; la liste des PN.
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlMain" runat="server" Visible="false">

                <%-- ═══ PN + LIMIT HEADER ═══ --%>
                <div class="card card-outline card-primary mb-3">
                    <div class="card-body py-2 px-3">
                        <div class="row align-items-center">

                            <%-- PN identity --%>
                            <div class="col-auto">
                                <span class="badge badge-primary"
                                      style="font-size:1rem;padding:.35rem .6rem;">
                                    <i class="fas fa-barcode mr-1"></i>
                                    <asp:Literal ID="litPN" runat="server" />
                                </span>
                            </div>
                            <div class="col-auto">
                                <div class="font-weight-bold">
                                    <asp:Literal ID="litNomenclature" runat="server" />
                                </div>
                                <small class="text-muted">
                                    <asp:Literal ID="litATA" runat="server" />
                                </small>
                            </div>

                            <%-- Divider --%>
                            <div class="col-auto border-left ml-2 pl-3">
                                <small class="text-muted d-block">Type de limite</small>
                                <asp:Literal ID="litLimitTypeBadge" runat="server" />
                            </div>
                            <div class="col-auto border-left pl-3">
                                <small class="text-muted d-block">Limite maxi</small>
                                <span class="font-weight-bold text-primary">
                                    <asp:Literal ID="litHardLimit" runat="server" />
                                </span>
                            </div>
                            <div class="col-auto border-left pl-3">
                                <small class="text-muted d-block">Seuil alerte</small>
                                <span class="font-weight-bold text-warning">
                                    <asp:Literal ID="litAlertPct" runat="server" />
                                </span>
                            </div>

                            <%-- OR logic explanation --%>
                            <div class="col-auto ml-auto">
                                <span class="badge badge-info">
                                    <i class="fas fa-info-circle mr-1"></i>
                                    Logique OR entre compteurs
                                </span>
                            </div>

                        </div>
                    </div>
                </div>

                <%-- ═══ EXISTING TASK COUNTERS GRID ═══ --%>
                <div class="card card-outline card-secondary mb-3">
                    <div class="card-header py-2">
                        <h3 class="card-title">
                            <i class="fas fa-stopwatch mr-1"></i>
                            Compteurs de t&acirc;che
                            <span class="badge badge-secondary ml-1">
                                <asp:Literal ID="litTCCount" runat="server" Text="0" />
                            </span>
                        </h3>
                        <div class="card-tools">
                            <small class="text-muted">
                                Premier d&eacute;passement = t&acirc;che &agrave; r&eacute;aliser
                            </small>
                        </div>
                    </div>
                    <div class="card-body p-0">
                        <asp:GridView ID="gvTC" runat="server"
                            AutoGenerateColumns="false"
                            CssClass="table table-sm table-hover mb-0"
                            GridLines="None"
                            DataKeyNames="TaskCounterId"
                            EmptyDataText="Aucun compteur d&eacute;fini. Ajoutez-en un ci-dessous.">
                            <Columns>

                                <%-- CounterDef --%>
                                <asp:TemplateField HeaderText="Compteur"
                                    ItemStyle-CssClass="font-weight-bold">
                                    <ItemTemplate>
                                        <i class="fas fa-tachometer-alt mr-1 text-primary"></i>
                                        <%# Eval("CounterDefCode") %>
                                        <small class="text-muted">
                                            <%# Eval("CounterDefName") %>
                                        </small>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <%-- CounterBasis --%>
                                <asp:TemplateField HeaderText="Base"
                                    ItemStyle-CssClass="text-center"
                                    HeaderStyle-CssClass="text-center">
                                    <ItemTemplate>
                                        <span class="badge badge-light border"
                                              style="font-size:.75rem;">
                                            <%# Eval("CounterBasisCode") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <%-- FirstThreshold --%>
                                <asp:TemplateField HeaderText="1&egrave;re &eacute;ch&eacute;ance"
                                    ItemStyle-CssClass="text-center"
                                    HeaderStyle-CssClass="text-center">
                                    <ItemTemplate>
                                        <strong>
                                            <%# FormatValue(Eval("FirstThreshold"),
                                                            Eval("DisplayUnit")) %>
                                        </strong>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <%-- RepeatInterval --%>
                                <asp:TemplateField HeaderText="Intervalle"
                                    ItemStyle-CssClass="text-center"
                                    HeaderStyle-CssClass="text-center">
                                    <ItemTemplate>
                                        <%# If(Eval("RepeatInterval") Is DBNull.Value,
                                            "<span class='text-muted'>Unique</span>",
                                            "<span>" & FormatValue(Eval("RepeatInterval"),
                                                Eval("DisplayUnit")) & "</span>") %>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <%-- Ceiling --%>
                                <asp:TemplateField HeaderText="Plafond vie"
                                    ItemStyle-CssClass="text-center"
                                    HeaderStyle-CssClass="text-center">
                                    <ItemTemplate>
                                        <%# If(Eval("Ceiling") Is DBNull.Value,
                                            "<span class='text-muted'>-</span>",
                                            "<span class='text-danger font-weight-bold'>" &
                                            FormatValue(Eval("Ceiling"),
                                                Eval("DisplayUnit")) & "</span>")%>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <%-- AlertThresholdPct --%>
                                <asp:TemplateField HeaderText="Alerte"
                                    ItemStyle-CssClass="text-center"
                                    HeaderStyle-CssClass="text-center">
                                    <ItemTemplate>
                                        <span class="text-warning font-weight-bold">
                                            <%# Eval("AlertThresholdPct") %>%
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <%-- MaxExtension --%>
                                <asp:TemplateField HeaderText="Extension max"
                                    ItemStyle-CssClass="text-center"
                                    HeaderStyle-CssClass="text-center">
                                    <ItemTemplate>
                                        <%# FormatExtension(Eval("MaxExtensionPct"),
                                                            Eval("MaxExtensionValue"),
                                                            Eval("DisplayUnit")) %>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <%-- Active --%>
                                <asp:TemplateField HeaderText="Statut"
                                    ItemStyle-CssClass="text-center"
                                    HeaderStyle-CssClass="text-center">
                                    <ItemTemplate>
                                        <span class='badge <%# If(Convert.ToBoolean(
                                            Eval("IsActive")),
                                            "badge-success","badge-secondary") %>'>
                                            <%# If(Convert.ToBoolean(Eval("IsActive")),
                                                "Actif","Inactif") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <%-- Actions --%>
                                <asp:TemplateField HeaderText=""
                                    ItemStyle-CssClass="text-right">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkEdit" runat="server"
                                            CommandName="EditTC"
                                            CommandArgument='<%# Eval("TaskCounterId") %>'
                                            CssClass="btn btn-xs btn-outline-primary mr-1"
                                            CausesValidation="false"
                                            title="Modifier">
                                            <i class="fas fa-edit"></i>
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="lnkToggle" runat="server"
                                            CommandName="ToggleTC"
                                            CommandArgument='<%# Eval("TaskCounterId") %>'
                                            CssClass='<%# If(Convert.ToBoolean(Eval("IsActive")),
                                                "btn btn-xs btn-outline-danger",
                                                "btn btn-xs btn-outline-success") %>'
                                            CausesValidation="false"
                                            OnClientClick="return confirm('Confirmer ?');"
                                            title='<%# If(Convert.ToBoolean(Eval("IsActive")),
                                                "Désactiver","Activer") %>'>
                                            <%# If(Convert.ToBoolean(Eval("IsActive")),
                                                "<i class='fas fa-ban'></i>",
                                                "<i class='fas fa-check'></i>") %>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>

                            </Columns>
                            <HeaderStyle CssClass="bg-light text-dark" />
                            <RowStyle CssClass="align-middle" />
                        </asp:GridView>
                    </div>
                </div>

                <%-- ═══ ADD / EDIT FORM ═══ --%>
                <div class="card card-outline card-success">
                    <div class="card-header py-2">
                        <h3 class="card-title">
                            <i class="fas fa-plus-circle mr-1"></i>
                            <asp:Literal ID="litFormTitle" runat="server"
                                Text="Ajouter un compteur" />
                        </h3>
                    </div>
                    <div class="card-body">

                        <div class="form-row">

                            <%-- CounterType → CounterDef cascade --%>
                            <div class="form-group col-md-2">
                                <label class="small font-weight-bold">
                                    Type <span class="text-danger">*</span>
                                </label>
                                <asp:DropDownList ID="ddlCounterType" runat="server"
                                    CssClass="form-control form-control-sm"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlCounterType_Changed" />
                            </div>
                            <div class="form-group col-md-2">
                                <label class="small font-weight-bold">
                                    Compteur <span class="text-danger">*</span>
                                </label>
                                <asp:DropDownList ID="ddlCounterDef" runat="server"
                                    CssClass="form-control form-control-sm" />
                            </div>

                            <%-- CounterBasis --%>
                            <div class="form-group col-md-2">
                                <label class="small font-weight-bold">
                                    Base <span class="text-danger">*</span>
                                </label>
                                <asp:DropDownList ID="ddlCounterBasis" runat="server"
                                    CssClass="form-control form-control-sm" />
                            </div>

                        </div>

                        <div class="form-row">

                            <%-- FirstThreshold --%>
                            <div class="form-group col-md-2">
                                <label class="small font-weight-bold">
                                    1&egrave;re &eacute;ch&eacute;ance
                                    <span class="text-danger">*</span>
                                </label>
                                <div class="input-group input-group-sm">
                                    <asp:TextBox ID="txtFirstThreshold" runat="server"
                                        CssClass="form-control form-control-sm"
                                        placeholder="ex: 600" />
                                    <div class="input-group-append">
                                        <span class="input-group-text px-1"
                                              style="font-size:.75rem;">
                                            <asp:Literal ID="litUnit"
                                                runat="server" Text="-" />
                                        </span>
                                    </div>
                                </div>
                                <small class="form-text text-muted" style="font-size:.7rem;">
                                    Premi&egrave;re &eacute;ch&eacute;ance depuis base
                                </small>
                            </div>

                            <%-- RepeatInterval --%>
                            <div class="form-group col-md-2">
                                <label class="small font-weight-bold">Intervalle</label>
                                <div class="input-group input-group-sm">
                                    <asp:TextBox ID="txtRepeatInterval" runat="server"
                                        CssClass="form-control form-control-sm"
                                        placeholder="vide = unique" />
                                    <div class="input-group-append">
                                        <span class="input-group-text px-1"
                                              style="font-size:.75rem;">
                                            <asp:Literal ID="litUnit2"
                                                runat="server" Text="-" />
                                        </span>
                                    </div>
                                </div>
                                <small class="form-text text-muted" style="font-size:.7rem;">
                                    Vide = t&acirc;che unique (non r&eacute;p&eacute;titive)
                                </small>
                            </div>

                            <%-- Ceiling --%>
                            <div class="form-group col-md-2">
                                <label class="small font-weight-bold">Plafond vie</label>
                                <div class="input-group input-group-sm">
                                    <asp:TextBox ID="txtCeiling" runat="server"
                                        CssClass="form-control form-control-sm"
                                        placeholder="vide = illimit&eacute;" />
                                    <div class="input-group-append">
                                        <span class="input-group-text px-1"
                                              style="font-size:.75rem;">
                                            <asp:Literal ID="litUnit3"
                                                runat="server" Text="-" />
                                        </span>
                                    </div>
                                </div>
                                <small class="form-text text-muted" style="font-size:.7rem;">
                                    Limite vie - remplac&eacute; apr&egrave;s cette valeur
                                </small>
                            </div>

                            <%-- AlertThresholdPct --%>
                            <div class="form-group col-md-2">
                                <label class="small font-weight-bold">
                                    Alerte &agrave; %
                                    <span class="text-muted">(1-99)</span>
                                </label>
                                <div class="input-group input-group-sm">
                                    <asp:TextBox ID="txtAlertPct" runat="server"
                                        CssClass="form-control form-control-sm"
                                        Text="90" MaxLength="2" />
                                    <div class="input-group-append">
                                        <span class="input-group-text">%</span>
                                    </div>
                                </div>
                            </div>

                        </div>

                        <%-- Extension fields --%>
                        <div class="form-row">
                            <div class="col-12 mb-2">
                                <small class="text-muted text-uppercase font-weight-bold"
                                       style="font-size:.7rem;letter-spacing:.05em;">
                                    <i class="fas fa-expand-arrows-alt mr-1"></i>
                                    Param&egrave;tres d&apos;extension (optionnel)
                                </small>
                            </div>

                            <%-- MaxExtensionPct --%>
                            <div class="form-group col-md-2">
                                <label class="small">Extension max %</label>
                                <div class="input-group input-group-sm">
                                    <asp:TextBox ID="txtMaxExtPct" runat="server"
                                        CssClass="form-control form-control-sm"
                                        placeholder="ex: 10" />
                                    <div class="input-group-append">
                                        <span class="input-group-text">%</span>
                                    </div>
                                </div>
                                <small class="form-text text-muted" style="font-size:.7rem;">
                                    % de l&apos;intervalle courant
                                </small>
                            </div>

                            <%-- MaxExtensionValue --%>
                            <div class="form-group col-md-2">
                                <label class="small">Extension max valeur</label>
                                <div class="input-group input-group-sm">
                                    <asp:TextBox ID="txtMaxExtValue" runat="server"
                                        CssClass="form-control form-control-sm"
                                        placeholder="ex: 50" />
                                    <div class="input-group-append">
                                        <span class="input-group-text px-1"
                                              style="font-size:.75rem;">
                                            <asp:Literal ID="litUnit4"
                                                runat="server" Text="-" />
                                        </span>
                                    </div>
                                </div>
                                <small class="form-text text-muted" style="font-size:.7rem;">
                                    Valeur fixe - min(%, valeur) appliqu&eacute;
                                </small>
                            </div>

                        </div>

                        <%-- Form actions --%>
                        <div class="d-flex align-items-center mt-1">
                            <asp:Button ID="btnSaveTC" runat="server"
                                CssClass="btn btn-sm btn-success mr-2"
                                Text="Enregistrer" CausesValidation="false" />
                            <asp:Button ID="btnCancelEdit" runat="server"
                                CssClass="btn btn-sm btn-outline-secondary"
                                Text="Annuler" CausesValidation="false" />
                            <span class="ml-3 text-muted small">
                                Un compteur par type/base. Logique OR : premier
                                d&eacute;passement = t&acirc;che due.
                            </span>
                        </div>

                        <asp:Label ID="lblError" runat="server"
                            Visible="false"
                            CssClass="alert alert-danger d-block py-2 px-3 mt-3" />

                        <asp:HiddenField ID="hfTaskCounterId" runat="server" />

                    </div>
                </div>

            </asp:Panel>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>

<asp:Content ID="cFooter" ContentPlaceHolderID="FooterScripts" runat="server">
<script type="text/javascript">
    // Live unit preview: show alert value as user types
    (function waitForJQ() {
        if (typeof jQuery === 'undefined') { setTimeout(waitForJQ, 30); return; }
        $(function() {
            function updatePreview() {
                var thresh = parseFloat($('#<%= txtFirstThreshold.ClientID %>').val()) || 0;
                var pct    = parseFloat($('#<%= txtAlertPct.ClientID %>').val()) || 0;
                var alert  = (thresh * pct / 100).toFixed(0);
                $('#alertPreview').text(alert > 0 ? '→ alerte à ' + alert : '');
            }
            $('#<%= txtFirstThreshold.ClientID %>, #<%= txtAlertPct.ClientID %>')
                .on('input change', updatePreview);
        });
    })();
</script>
</asp:Content>
