<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="SNDetail.aspx.vb"
    Inherits="MRO2_Maintenance_SNDetail"
    EnableEventValidation="false"  
    ResponseEncoding="UTF-8"
    ContentType="text/html; charset=utf-8" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    D&eacute;tail S&eacute;rie
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Default.aspx") %>">MRO2</a>
    </li>
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Maintenance/FleetList.aspx") %>">Flotte</a>
    </li>
    <li class="breadcrumb-item active">D&eacute;tail SN</li>
</asp:Content>

<asp:Content ID="cHead" ContentPlaceHolderID="HeadContent" runat="server">
    
    <style>
        .progress { height: 8px; border-radius: 4px; }
        .counter-card { border-left: 4px solid #dee2e6; margin-bottom:.75rem; }
        .counter-card.status-ok       { border-left-color: #28a745; }
        .counter-card.status-alert    { border-left-color: #17a2b8; }
        .counter-card.status-due      { border-left-color: #ffc107; }
        .counter-card.status-overdue  { border-left-color: #fd7e14; }
        .counter-card.status-expired  { border-left-color: #dc3545; }
        .counter-card.status-complete { border-left-color: #6c757d; }
        .event-dot { width:10px;height:10px;border-radius:50%;
                     flex-shrink:0;margin-top:3px; }
        .ext-badge { font-size:.7rem; vertical-align:middle; }
    </style>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional"
                     ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- ═══ NOT FOUND ═══ --%>
            <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
                <div class="alert alert-warning">
                    <i class="fas fa-exclamation-triangle mr-1"></i>
                    Composant introuvable. V&eacute;rifiez l&apos;identifiant.
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlMain" runat="server" Visible="false">

                <%-- ═══ SN IDENTITY HEADER ═══ --%>
                <div class="card card-outline card-primary mb-3">
                    <div class="card-body py-2 px-3">
                        <div class="row align-items-center flex-wrap">

                            <div class="col-auto">
                                <span class="badge badge-dark mr-2"
                                      style="font-size:.95rem;padding:.35rem .6rem;">
                                    <i class="fas fa-barcode mr-1"></i>
                                    <asp:Literal ID="litSN" runat="server" />
                                </span>
                            </div>
                            <div class="col-auto">
                                <div class="font-weight-bold">
                                    <asp:Literal ID="litPN" runat="server" />
                                </div>
                                <small class="text-muted">
                                    <asp:Literal ID="litNomenclature" runat="server" />
                                </small>
                            </div>

                            <div class="col-auto border-left pl-3">
                                <small class="text-muted d-block">ATA</small>
                                <span class="font-weight-bold small">
                                    <asp:Literal ID="litATA" runat="server" Text="-" />
                                </span>
                            </div>

                            <div class="col-auto border-left pl-3">
                                <small class="text-muted d-block">Statut SN</small>
                                <asp:Literal ID="litSNStatus" runat="server" />
                            </div>

                            <div class="col-auto border-left pl-3">
                                <small class="text-muted d-block">Position</small>
                                <asp:Literal ID="litCurrentPosition" runat="server"
                                    Text="<span class='text-muted small'>Non install&eacute;</span>" />
                            </div>

                            <div class="col-auto border-left pl-3">
                                <small class="text-muted d-block">Avion</small>
                                <asp:Literal ID="litCurrentAircraft" runat="server"
                                    Text="<span class='text-muted'>-</span>" />
                            </div>

                            <div class="col-auto border-left pl-3">
                                <small class="text-muted d-block">
                                    Jours install&eacute;
                                </small>
                                <asp:Literal ID="litDaysOnWing" runat="server"
                                    Text="<span class='text-muted'>-</span>" />
                            </div>

                            <div class="col-auto ml-auto">
                                <asp:Literal ID="litOverallHealth" runat="server" />
                            </div>

                        </div>
                    </div>
                </div>

                <div class="row">

                    <%-- ═══ LEFT: TaskCounter status ═══ --%>
                    <div class="col-lg-8">

                        <div class="card card-outline card-secondary mb-3">
                            <div class="card-header py-2">
                                <h3 class="card-title">
                                    <i class="fas fa-stopwatch mr-1"></i>
                                    Compteurs de t&acirc;che
                                    <small class="text-muted ml-1">
                                        Logique OR &mdash; premier d&eacute;passement
                                        = t&acirc;che due
                                    </small>
                                </h3>
                            </div>
                            <div class="card-body pt-2 pb-1 px-3">
                                <asp:PlaceHolder ID="phCounters" runat="server" />
                            </div>
                        </div>

                    </div>

                    <%-- ═══ RIGHT: Event history ═══ --%>
                    <div class="col-lg-4">

                        <div class="card card-outline card-light">
                            <div class="card-header py-2">
                                <h3 class="card-title">
                                    <i class="fas fa-history mr-1"></i>
                                    Historique
                                    <small class="text-muted">(20 derniers)</small>
                                </h3>
                            </div>
                            <div class="card-body p-2"
                                 style="max-height:520px;overflow-y:auto;">
                                <asp:PlaceHolder ID="phHistory" runat="server" />
                            </div>
                        </div>

                    </div>
                </div>

            </asp:Panel>

            <%-- Hidden fields + trigger --%>
            <asp:HiddenField ID="hfExtTaskCounterId"    runat="server" />
            <asp:HiddenField ID="hfExtSerializedItemId" runat="server" />
            <asp:HiddenField ID="hfAction"              runat="server" />
            <asp:Button ID="btnDispatch" runat="server"
                style="display:none;" CausesValidation="false" />

            <%-- ═══ EXTENSION MODAL ═══ --%>
            <div class="modal fade" id="extModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header bg-warning">
                            <h5 class="modal-title text-dark font-weight-bold">
                                <i class="fas fa-expand-arrows-alt mr-1"></i>
                                Prolongation &mdash;
                                <asp:Literal ID="litExtCounterLabel" runat="server" />
                            </h5>
                            <button type="button" class="close"
                                    data-dismiss="modal"><span>&times;</span></button>
                        </div>
                        <div class="modal-body">

                            <%-- Current state --%>
                            <div class="alert alert-light border py-2 px-3 mb-3">
                                <div class="row">
                                    <div class="col-auto">
                                        <small class="text-muted d-block">
                                            &Eacute;ch&eacute;ance actuelle
                                        </small>
                                        <strong>
                                            <asp:Literal ID="litExtCurrentDue"
                                                runat="server" Text="-" />
                                        </strong>
                                    </div>
                                    <div class="col-auto border-left pl-3">
                                        <small class="text-muted d-block">
                                            Extension max
                                        </small>
                                        <span class="text-warning font-weight-bold">
                                            <asp:Literal ID="litExtMaxAllowed"
                                                runat="server" Text="-" />
                                        </span>
                                    </div>
                                    <div class="col-auto border-left pl-3">
                                        <small class="text-muted d-block">
                                            Nouvelle &eacute;ch&eacute;ance
                                        </small>
                                        <span class="text-success font-weight-bold"
                                              id="spanNewDue">-</span>
                                    </div>
                                </div>
                            </div>

                            <div class="form-row">
                                <div class="form-group col-md-3">
                                    <label class="font-weight-bold small">
                                        Type <span class="text-danger">*</span>
                                    </label>
                                    <asp:DropDownList ID="ddlExtType" runat="server"
                                        CssClass="form-control form-control-sm">
                                        <asp:ListItem Value="VALUE">
                                            Valeur fixe
                                        </asp:ListItem>
                                        <asp:ListItem Value="PCT">
                                            Pourcentage %
                                        </asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group col-md-3">
                                    <label class="font-weight-bold small">
                                        Valeur <span class="text-danger">*</span>
                                    </label>
                                    <div class="input-group input-group-sm">
                                        <asp:TextBox ID="txtExtValue" runat="server"
                                            CssClass="form-control form-control-sm"
                                            placeholder="ex: 50" />
                                        <div class="input-group-append">
                                            <asp:Label ID="litExtUnit" runat="server"
                                                CssClass="input-group-text"
                                                Text="-" />
                                        </div>
                                    </div>
                                </div>
                                <div class="form-group col-md-6">
                                    <label class="font-weight-bold small">
                                        Motif <span class="text-danger">*</span>
                                    </label>
                                    <asp:DropDownList ID="ddlExtReason" runat="server"
                                        CssClass="form-control form-control-sm" />
                                </div>
                            </div>

                            <div class="form-row">
                                <div class="form-group col-md-6">
                                    <label class="small">
                                        R&eacute;f&eacute;rence documentaire
                                        <asp:Literal ID="litDocRefRequired"
                                            runat="server" />
                                    </label>
                                    <asp:TextBox ID="txtExtDocRef" runat="server"
                                        CssClass="form-control form-control-sm"
                                        MaxLength="200"
                                        placeholder="ex: CMM 72-00-00, SB-2024-001" />
                                </div>
                                <div class="form-group col-md-6">
                                    <label class="small">
                                        Approbateur
                                        <asp:Literal ID="litApproverRequired"
                                            runat="server" />
                                    </label>
                                    <asp:TextBox ID="txtExtApprover" runat="server"
                                        CssClass="form-control form-control-sm"
                                        MaxLength="150"
                                        placeholder="ex: LCL ALAMI &mdash; Chef maintenance" />
                                </div>
                            </div>

                            <div class="form-group">
                                <label class="small">Notes</label>
                                <asp:TextBox ID="txtExtNotes" runat="server"
                                    CssClass="form-control form-control-sm"
                                    TextMode="MultiLine" Rows="2"
                                    MaxLength="500" />
                            </div>

                            <asp:Label ID="lblExtError" runat="server"
                                Visible="false"
                                CssClass="alert alert-danger d-block py-2 px-3" />

                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnSaveExtension" runat="server"
                                CssClass="btn btn-warning text-dark font-weight-bold"
                                Text="Confirmer la prolongation"
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
    var _hfTCId  = '<%= hfExtTaskCounterId.ClientID %>';
    var _hfSnId  = '<%= hfExtSerializedItemId.ClientID %>';
    var _hfAct   = '<%= hfAction.ClientID %>';
    var _btnDisp = '<%= btnDispatch.UniqueID %>';

    // Called from inline onclick on each counter card's Prolonger button
    function openExtModal(tcId, snId) {
        document.getElementById(_hfTCId).value = tcId;
        document.getElementById(_hfSnId).value = snId;
        document.getElementById(_hfAct).value  = 'openext';
        __doPostBack(_btnDisp, 'openext');
    }
</script>
</asp:Content>
