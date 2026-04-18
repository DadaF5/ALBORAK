<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="Default.aspx.vb"
    Inherits="MRO2_Setup_Default" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Configuration MRO2
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
 <%--   <li class="breadcrumb-item active">Setup</li>--%>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional">
        <ContentTemplate>

            <%-- ═══════════════════════════════════════════════
                 PAGE HEADER
            ═══════════════════════════════════════════════ --%>
            <div class="d-flex align-items-center mb-4">
                <div>
                    <h4 class="mb-0">
                        <i class="fas fa-cog text-primary mr-2"></i>
                        Configuration MRO2
                    </h4>
                    <small class="text-muted">
                        Tables de r&eacute;f&eacute;rence - &agrave; configurer avant toute op&eacute;ration de maintenance.
                    </small>
                </div>
                <div class="ml-auto">
                    <asp:Button ID="btnRefresh" runat="server"
                        CssClass="btn btn-sm btn-outline-secondary"
                        Text="&#8635; Actualiser"
                        CausesValidation="false" />
                </div>
            </div>

            <%-- ═══════════════════════════════════════════════
                 SECTION 1 — COMPTEURS
            ═══════════════════════════════════════════════ --%>
            <h6 class="text-uppercase text-muted mb-3"
                style="font-size:.72rem;letter-spacing:.08em;">
                <i class="fas fa-tachometer-alt mr-1"></i>
                Compteurs &amp; R&eacute;f&eacute;rences
            </h6>

            <div class="row">

                <%-- CounterType --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-primary h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-primary p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-tachometer-alt"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">Types de Compteurs</div>
                                    <small class="text-muted">
                                        FH, FC, APU hrs, Landings...
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litCounterTypeCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Setup/Counters/CounterTypeList.aspx") %>"
                               class="btn btn-xs btn-primary btn-block " style="color:white">
                                <span style="color: #FFFFFF"><i class="fas fa-arrow-right mr-1"></i>G&eacute;rer</span>
                            </a>
                        </div>
                    </div>
                </div>

                <%-- CounterDef --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-primary h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-primary p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-list-ol"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">
                                        D&eacute;finitions de Compteurs
                                    </div>
                                    <small class="text-muted">
                                        AF_FLIGHT_MIN, ENG_STARTS...
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litCounterDefCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Setup/Counters/CounterDefList.aspx") %>"
                               class="btn btn-xs btn-primary btn-block">
                                 <span style="color: #FFFFFF"><i class="fas fa-arrow-right mr-1"></i>G&eacute;rer</span>
                            </a>
                        </div>
                    </div>
                </div>

                <%-- CounterBasis --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-primary h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-primary p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-history"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">Bases de Comptage</div>
                                    <small class="text-muted">
                                        ABSOLUTE, SINCE_INSTALL, SINCE_OH...
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litCounterBasisCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Setup/Counters/CounterBasisList.aspx") %>"
                               class="btn btn-xs btn-primary btn-block">
                                 <span style="color: #FFFFFF"><i class="fas fa-arrow-right mr-1"></i>G&eacute;rer</span>
                            </a>
                        </div>
                    </div>
                </div>

                <%-- ComputationReference --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-info h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-info p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-calendar-alt"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">
                                        R&eacute;f&eacute;rences de Calcul
                                    </div>
                                    <small class="text-muted">
                                        SNEW, SOH, CURE_DATE... (&laquo;&nbsp;depuis quand&nbsp;&raquo;)
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litCompRefCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Setup/Counters/ComputationReferenceList.aspx") %>"
                               class="btn btn-xs btn-info btn-block">
                                 <span style="color: #FFFFFF"><i class="fas fa-arrow-right mr-1"></i>G&eacute;rer</span>
                            </a>
                        </div>
                    </div>
                </div>

                <%-- CounterReference --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-secondary h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-secondary p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-file-alt"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">
                                        R&eacute;f&eacute;rences Documentaires
                                    </div>
                                    <small class="text-muted">
                                        AMM, CMM, SB, AD... (&laquo;&nbsp;quel document&nbsp;&raquo;)
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litCounterRefCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Setup/Counters/CounterReferenceList.aspx") %>"
                               class="btn btn-xs btn-secondary btn-block">
                                  <span style="color: #FFFFFF"><i class="fas fa-arrow-right mr-1"></i>G&eacute;rer</span>
                            </a>
                        </div>
                    </div>
                </div>

                <%-- LimitType --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-danger h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-danger p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-tags"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">Types de Limites</div>
                                    <small class="text-muted">
                                        LIFE, INSPECTION, FUNCTIONAL, SHELF_LIFE
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litLimitTypeCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Setup/Counters/LimitTypeList.aspx") %>"
                               class="btn btn-xs btn-danger btn-block">
                                  <span style="color: #FFFFFF"><i class="fas fa-arrow-right mr-1"></i>G&eacute;rer</span>
                            </a>
                        </div>
                    </div>
                </div>

                <%-- ExtensionReason --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-warning h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-warning p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-clock"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">
                                        Motifs de Prolongation
                                    </div>
                                    <small class="text-muted">
                                        MFR_TOL, OPS_NEC, REG_AUTH...
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litExtReasonCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Setup/Counters/ExtensionReasonList.aspx") %>"
                               class="btn btn-xs btn-warning btn-block">
                                <i class="fas fa-arrow-right mr-1"></i>G&eacute;rer
                            </a>
                        </div>
                    </div>
                </div>

            </div>

            <%-- ═══════════════════════════════════════════════
                 SECTION 2 — COMPOSANTS
            ═══════════════════════════════════════════════ --%>
            <h6 class="text-uppercase text-muted mb-3 mt-2"
                style="font-size:.72rem;letter-spacing:.08em;">
                <i class="fas fa-barcode mr-1"></i>
                Composants &amp; Limites
            </h6>

            <div class="row">

                <%-- PartNumber --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-primary h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-primary p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-barcode"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">Part Numbers</div>
                                    <small class="text-muted">
                                        Catalogue des r&eacute;f&eacute;rences (PN)
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litPNCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Setup/Components/PartNumberList.aspx") %>"
                               class="btn btn-xs btn-primary btn-block">
                                <span style="color: #FFFFFF"><i class="fas fa-arrow-right mr-1"></i>G&eacute;rer</span>
                            </a>
                        </div>
                    </div>
                </div>

                <%-- SerializedItem --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-primary h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-primary p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-microchip"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">
                                        Articles S&eacute;rialis&eacute;s
                                    </div>
                                    <small class="text-muted">
                                        Composants suivis par num&eacute;ro de s&eacute;rie
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litSNCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Setup/Components/SerializedItemList.aspx") %>"
                               class="btn btn-xs btn-primary btn-block">
                                 <span style="color: #FFFFFF"><i class="fas fa-arrow-right mr-1"></i>G&eacute;rer</span>
                            </a>
                        </div>
                    </div>
                </div>

                <%-- PNLimit / TaskCounter --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-danger h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-danger p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-stopwatch"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">
                                        Limites PN / Compteurs t&acirc;ches
                                    </div>
                                    <small class="text-muted">
                                        G&eacute;r&eacute;es depuis la fiche PN
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litTaskCounterCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Setup/Components/PartNumberList.aspx") %>"
                               class="btn btn-xs btn-danger btn-block">                              
                                 <span style="color: #FFFFFF"><i class="fas fa-arrow-right mr-1"></i>Ouvrir via PN</span> 
                            </a>
                        </div>
                    </div>
                </div>

            </div>

            <%-- ═══════════════════════════════════════════════
                 SECTION 3 — CONFIGURATION AVIONS
            ═══════════════════════════════════════════════ --%>
            <h6 class="text-uppercase text-muted mb-3 mt-2"
                style="font-size:.72rem;letter-spacing:.08em;">
                <i class="fas fa-plane mr-1"></i>
                Configuration A&eacute;ronefs
            </h6>

            <div class="row">

                <%-- AcPositionTemplate --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-primary h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-primary p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-sitemap"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">
                                        Gabarits de Positions
                                    </div>
                                    <small class="text-muted">
                                        Arbre Zone / Syst&egrave;me / Slot par type A/C
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litTemplateCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Setup/Aircraft/AcPositionTemplateList.aspx") %>"
                               class="btn btn-xs btn-primary btn-block">
                                 <span style="color: #FFFFFF"><i class="fas fa-arrow-right mr-1"></i>G&eacute;rer</span>
                            </a>
                        </div>
                    </div>
                </div>

                <%-- AcPosition (per tail) --%>
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="card card-outline card-primary h-100">
                        <div class="card-body py-3">
                            <div class="d-flex align-items-start">
                                <div class="mr-3">
                                    <span class="badge badge-primary p-2"
                                          style="font-size:1rem;">
                                        <i class="fas fa-fighter-jet"></i>
                                    </span>
                                </div>
                                <div class="flex-grow-1">
                                    <div class="font-weight-bold">
                                        Positions par A&eacute;ronef
                                    </div>
                                    <small class="text-muted">
                                        Slots individuels par num&eacute;ro de queue
                                    </small>
                                    <div class="mt-1">
                                        <asp:Literal ID="litPositionCount"
                                            runat="server" Text="—" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer p-2 bg-light">
                            <a href="<%= ResolveUrl("~/MRO2/Maintenance/AircraftConfiguration.aspx") %>"
                               class="btn btn-xs btn-primary btn-block">
                                 <span style="color: #FFFFFF"><i class="fas fa-arrow-right mr-1"></i> Voir configuration</span>
                               
                            </a>
                        </div>
                    </div>
                </div>

            </div>

            <%-- ═══════════════════════════════════════════════
                 SETUP HEALTH INDICATOR
                 Shows which tables are empty (not yet configured)
            ═══════════════════════════════════════════════ --%>
            <asp:Panel ID="pnlWarnings" runat="server" Visible="false">
                <div class="alert alert-warning py-2 px-3 mt-2"
                     style="font-size:.85rem;">
                    <i class="fas fa-exclamation-triangle mr-1"></i>
                    <strong>Configuration incompl&egrave;te</strong> —
                    les tables suivantes sont vides ou non configur&eacute;es :
                    <asp:Literal ID="litWarnings" runat="server" />
                </div>
            </asp:Panel>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
