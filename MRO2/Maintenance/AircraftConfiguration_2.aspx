<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="AircraftConfiguration_2.aspx.vb"
    Inherits="MRO2_Maintenance_AircraftConfiguration" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Configuration A&eacute;ronef
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Default.aspx") %>">MRO2</a>
    </li>
    <li class="breadcrumb-item active">Configuration A&eacute;ronef</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional"
                     ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- ═══ NO AIRCRAFT SELECTED ═══ --%>
            <asp:Panel ID="pnlNoAc" runat="server" Visible="false">
                <div class="alert alert-warning">
                    <i class="fas fa-exclamation-triangle mr-1"></i>
                    Aucun a&eacute;ronef s&eacute;lectionn&eacute;.
                    Veuillez naviguer depuis la liste de la flotte.
                </div>
            </asp:Panel>

            <%-- ═══ MAIN CONTENT ═══ --%>
            <asp:Panel ID="pnlMain" runat="server" Visible="false">

                <%-- ── AIRCRAFT HEADER STRIP ───────────────────────────────── --%>
                <div class="card card-outline card-primary mb-3">
                    <div class="card-body py-2 px-3">
                        <div class="row align-items-center">

                            <%-- Tail info --%>
                            <div class="col-auto">
                                <span class="badge badge-primary"
                                      style="font-size:1.1rem;padding:.4rem .7rem;">
                                    <i class="fas fa-fighter-jet mr-1"></i>
                                    <asp:Literal ID="litTailNo" runat="server" />
                                </span>
                            </div>
                            <div class="col-auto">
                                <div class="font-weight-bold">
                                    <asp:Literal ID="litAcType" runat="server" />
                                </div>
                                <small class="text-muted">
                                    <asp:Literal ID="litAcGroup" runat="server" />
                                </small>
                            </div>

                            <%-- Divider --%>
                            <div class="col-auto border-left ml-2 pl-3">
                                <small class="text-muted d-block">Heures de vol</small>
                                <span class="font-weight-bold text-primary">
                                    <asp:Literal ID="litAcFH" runat="server" Text="—" />
                                    <small class="text-muted font-weight-normal">hrs</small>
                                </span>
                            </div>
                            <div class="col-auto border-left pl-3">
                                <small class="text-muted d-block">Cycles</small>
                                <span class="font-weight-bold text-primary">
                                    <asp:Literal ID="litAcFC" runat="server" Text="—" />
                                </span>
                            </div>
                            <div class="col-auto border-left pl-3">
                                <small class="text-muted d-block">Atterrissages</small>
                                <span class="font-weight-bold text-primary">
                                    <asp:Literal ID="litAcLdg" runat="server" Text="—" />
                                </span>
                            </div>

                            <%-- Health summary badges --%>
                            <div class="col-auto border-left pl-3 ml-auto">
                                <small class="text-muted d-block mb-1">Sant&eacute; flotte</small>
                                <asp:Literal ID="litHealthBadges" runat="server" />
                            </div>

                            <%-- Collapse all / Expand all --%>
                            <div class="col-auto">
                                <button type="button" class="btn btn-sm btn-outline-secondary mr-1"
                                        onclick="document.querySelectorAll('.zone-collapse').forEach(function(el){el.style.display='';});return false;">
                                    <i class="fas fa-expand-alt"></i>
                                </button>
                                <button type="button" class="btn btn-sm btn-outline-secondary"
                                        onclick="document.querySelectorAll('.zone-collapse').forEach(function(el){el.style.display='none';});return false;">
                                    <i class="fas fa-compress-alt"></i>
                                </button>
                            </div>

                        </div>
                    </div>
                </div>

                <%-- ── TREE VIEW ────────────────────────────────────────────── --%>
                <%-- Rendered server-side as nested panels --%>
                <asp:PlaceHolder ID="phTree" runat="server" />

            </asp:Panel>

            <%-- Hidden trigger buttons — fired from JS to open modals --%>
            <asp:Button ID="btnLoadInstallModal" runat="server"
                style="display:none;" CausesValidation="false" />
            <asp:Button ID="btnLoadRemoveModal" runat="server"
                style="display:none;" CausesValidation="false" />

            <%-- ═══════════════════════════════════════════════════════
                 INSTALL MODAL
            ═══════════════════════════════════════════════════════ --%>
            <div class="modal fade" id="installModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header bg-success text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-plus-circle mr-1"></i>
                                Installer un composant
                            </h5>
                            <button type="button" class="close text-white"
                                    data-dismiss="modal"><span>&times;</span></button>
                        </div>
                        <div class="modal-body">

                            <%-- Position info (read-only) --%>
                            <div class="alert alert-light border py-2 px-3 mb-3">
                                <i class="fas fa-map-marker-alt text-primary mr-1"></i>
                                <strong>Position :</strong>
                                <asp:Label ID="litInstallPosition" runat="server" />
                            </div>

                            <div class="form-row">
                                <%-- SN picker --%>
                                <div class="form-group col-md-6">
                                    <label>
                                        Num&eacute;ro de s&eacute;rie
                                        <span class="text-danger">*</span>
                                    </label>
                                    <asp:DropDownList ID="ddlInstallSN" runat="server"
                                        CssClass="form-control form-control-sm" />
                                    <small class="form-text text-muted">
                                        Seuls les SN du PN autoris&eacute; pour ce slot sont affich&eacute;s.
                                    </small>
                                </div>
                                <%-- Date --%>
                                <div class="form-group col-md-3">
                                    <label>Date <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtInstallDate" runat="server"
                                        CssClass="form-control form-control-sm"
                                        MaxLength="10"
                                        placeholder="YYYY-MM-DD" />
                                </div>
                                <%-- Work order --%>
                                <div class="form-group col-md-3">
                                    <label>N&deg; OT</label>
                                    <asp:TextBox ID="txtInstallWO" runat="server"
                                        CssClass="form-control form-control-sm"
                                        MaxLength="100"
                                        placeholder="WO-2024-xxx" />
                                </div>
                            </div>

                            <%-- Aircraft counter snapshot --%>
                            <div class="card card-body bg-light py-2 px-3 mb-2">
                                <p class="mb-2 small font-weight-bold text-muted text-uppercase">
                                    Compteurs avion au moment de l&apos;installation
                                </p>
                                <div class="form-row">
                                    <div class="form-group col-md-3 mb-1">
                                        <label class="small mb-0">HV (heures)</label>
                                        <asp:TextBox ID="txtInstallFH" runat="server"
                                            CssClass="form-control form-control-sm"
                                            placeholder="ex: 4280.5" />
                                    </div>
                                    <div class="form-group col-md-3 mb-1">
                                        <label class="small mb-0">Cycles</label>
                                        <asp:TextBox ID="txtInstallFC" runat="server"
                                            CssClass="form-control form-control-sm"
                                            placeholder="ex: 3210" />
                                    </div>
                                    <div class="form-group col-md-3 mb-1">
                                        <label class="small mb-0">Atterrissages</label>
                                        <asp:TextBox ID="txtInstallLdg" runat="server"
                                            CssClass="form-control form-control-sm"
                                            placeholder="ex: 3190" />
                                    </div>
                                    <div class="form-group col-md-3 mb-1">
                                        <label class="small mb-0">TGO</label>
                                        <asp:TextBox ID="txtInstallTGO" runat="server"
                                            CssClass="form-control form-control-sm"
                                            placeholder="ex: 20" />
                                    </div>
                                </div>
                                <small class="text-muted" style="font-size:.75rem;">
                                    <i class="fas fa-info-circle mr-1"></i>
                                    Les valeurs sont pr&eacute;remplies depuis les compteurs
                                    courants de l&apos;avion. Corrigez si n&eacute;cessaire.
                                </small>
                            </div>

                            <asp:HiddenField ID="hfInstallPositionId" runat="server" />
                            <asp:Label ID="lblInstallError" runat="server"
                                Visible="false"
                                CssClass="alert alert-danger d-block py-2 px-3" />
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnInstallSave" runat="server"
                                CssClass="btn btn-success"
                                Text="Confirmer l&apos;installation"
                                CausesValidation="false" />
                            <button type="button" class="btn btn-secondary"
                                    data-dismiss="modal">Annuler</button>
                        </div>
                    </div>
                </div>
            </div>

            <%-- ═══════════════════════════════════════════════════════
                 REMOVE MODAL
            ═══════════════════════════════════════════════════════ --%>
            <div class="modal fade" id="removeModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-md">
                    <div class="modal-content">
                        <div class="modal-header bg-danger text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-minus-circle mr-1"></i>
                                D&eacute;poser le composant
                            </h5>
                            <button type="button" class="close text-white"
                                    data-dismiss="modal"><span>&times;</span></button>
                        </div>
                        <div class="modal-body">

                            <div class="alert alert-light border py-2 px-3 mb-3">
                                <i class="fas fa-map-marker-alt text-danger mr-1"></i>
                                <strong>Position :</strong>
                                <asp:Label ID="litRemovePosition" runat="server" />
                                <br />
                                <i class="fas fa-barcode text-muted mr-1"></i>
                                <strong>SN :</strong>
                                <asp:Label ID="litRemoveSN" runat="server" />
                            </div>

                            <div class="form-row">
                                <div class="form-group col-md-4">
                                    <label>Date <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtRemoveDate" runat="server"
                                        CssClass="form-control form-control-sm"
                                        MaxLength="10"
                                        placeholder="YYYY-MM-DD" />
                                </div>
                                <div class="form-group col-md-8">
                                    <label>N&deg; OT</label>
                                    <asp:TextBox ID="txtRemoveWO" runat="server"
                                        CssClass="form-control form-control-sm"
                                        MaxLength="100" placeholder="WO-2024-xxx" />
                                </div>
                            </div>

                            <%-- Counter snapshot at removal --%>
                            <div class="card card-body bg-light py-2 px-3 mb-2">
                                <p class="mb-2 small font-weight-bold text-muted text-uppercase">
                                    Compteurs avion au moment de la d&eacute;pose
                                </p>
                                <div class="form-row">
                                    <div class="form-group col-md-4 mb-1">
                                        <label class="small mb-0">HV (heures)</label>
                                        <asp:TextBox ID="txtRemoveFH" runat="server"
                                            CssClass="form-control form-control-sm"
                                            placeholder="ex: 4350.0" />
                                    </div>
                                    <div class="form-group col-md-4 mb-1">
                                        <label class="small mb-0">Cycles</label>
                                        <asp:TextBox ID="txtRemoveFC" runat="server"
                                            CssClass="form-control form-control-sm"
                                            placeholder="ex: 3280" />
                                    </div>
                                    <div class="form-group col-md-4 mb-1">
                                        <label class="small mb-0">Atterrissages</label>
                                        <asp:TextBox ID="txtRemoveLdg" runat="server"
                                            CssClass="form-control form-control-sm"
                                            placeholder="ex: 3260" />
                                    </div>
                                </div>
                            </div>

                            <div class="form-group">
                                <label>Remarques</label>
                                <asp:TextBox ID="txtRemoveRemarks" runat="server"
                                    CssClass="form-control form-control-sm"
                                    TextMode="MultiLine" Rows="2" MaxLength="500" />
                            </div>

                            <asp:HiddenField ID="hfRemovePositionId"    runat="server" />
                            <asp:HiddenField ID="hfRemoveSerializedItemId" runat="server" />
                            <asp:Label ID="lblRemoveError" runat="server"
                                Visible="false"
                                CssClass="alert alert-danger d-block py-2 px-3" />
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnRemoveSave" runat="server"
                                CssClass="btn btn-danger"
                                Text="Confirmer la d&eacute;pose"
                                CausesValidation="false" />
                            <button type="button" class="btn btn-secondary"
                                    data-dismiss="modal">Annuler</button>
                        </div>
                    </div>
                </div>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>

<asp:Content ID="cFooter" ContentPlaceHolderID="FooterScripts" runat="server">
<script type="text/javascript">
    // Pre-fill counter fields — wrapped in waitForJQuery
    // because mro2.master loads jQuery AFTER FooterScripts
    var _acFH  = '<%= AcFH_Display %>';
    var _acFC  = '<%= AcFC_Raw %>';
    var _acLdg = '<%= AcLdg_Raw %>';
    var _acTGO = '<%= AcTGO_Raw %>';

    function prefillInstallCounters() {
        var fhField = document.getElementById('<%= txtInstallFH.ClientID %>');
        var fcField = document.getElementById('<%= txtInstallFC.ClientID %>');
        var ldgField= document.getElementById('<%= txtInstallLdg.ClientID %>');
        var tgoField= document.getElementById('<%= txtInstallTGO.ClientID %>');
        if (fhField  && fhField.value  === '') fhField.value  = _acFH;
        if (fcField  && fcField.value  === '') fcField.value  = _acFC;
        if (ldgField && ldgField.value === '') ldgField.value = _acLdg;
        if (tgoField && tgoField.value === '') tgoField.value = _acTGO;
    }

    function prefillRemoveCounters() {
        var fhField = document.getElementById('<%= txtRemoveFH.ClientID %>');
        var fcField = document.getElementById('<%= txtRemoveFC.ClientID %>');
        var ldgField= document.getElementById('<%= txtRemoveLdg.ClientID %>');
        if (fhField  && fhField.value  === '') fhField.value  = _acFH;
        if (fcField  && fcField.value  === '') fcField.value  = _acFC;
        if (ldgField && ldgField.value === '') ldgField.value = _acLdg;
    }

    // Today's date in YYYY-MM-DD format for pre-fill
    function todayStr() {
        var d = new Date();
        var mm = ('0' + (d.getMonth()+1)).slice(-2);
        var dd = ('0' + d.getDate()).slice(-2);
        return d.getFullYear() + '-' + mm + '-' + dd;
    }

    // Wire modal events after jQuery is available
    (function waitForJQuery() {
        if (typeof jQuery === 'undefined') {
            setTimeout(waitForJQuery, 30);
            return;
        }
        $(document).on('show.bs.modal', '#installModal', function () {
            // Pre-fill date if empty
            var df = document.getElementById('<%= txtInstallDate.ClientID %>');
            if (df && df.value === '') df.value = todayStr();
            prefillInstallCounters();
        });
        $(document).on('show.bs.modal', '#removeModal', function () {
            var df = document.getElementById('<%= txtRemoveDate.ClientID %>');
            if (df && df.value === '') df.value = todayStr();
            prefillRemoveCounters();
        });
    })();
</script>
</asp:Content>
