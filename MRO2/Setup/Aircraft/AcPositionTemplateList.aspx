<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="AcPositionTemplateList.aspx.vb"
    Inherits="MRO2_Setup_Aircraft_AcPositionTemplateList"
    EnableEventValidation="false" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Gabarits de Positions
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Setup/Default.aspx") %>">Setup</a>
    </li>
    <li class="breadcrumb-item active">Gabarits de Positions</li>
</asp:Content>

<asp:Content ID="cHead" ContentPlaceHolderID="HeadContent" runat="server">
      <meta charset="utf-8" />
    <link rel="stylesheet"
          href="<%= ResolveUrl("~/AdminLTE-3.2.0/style.min.css") %>" />
  
    <style>
        .jstree-default .jstree-anchor {
            font-size: .84rem; height: 28px; line-height: 28px; padding-right: 8px;
        }
        #posTree { min-height: 150px; }
        .jstree-node-inactive > .jstree-anchor { opacity:.5; }
        .pn-badge  { cursor:pointer; font-size:.7rem !important; vertical-align:middle; }
        .btn-node  { vertical-align:middle; line-height:1.4 !important; }
        /* Show action buttons on row hover */
        .jstree-anchor:hover .node-actions,
        .jstree-anchor:focus .node-actions { display:inline !important; }
        .jstree-search { font-weight:bold !important; color:#1d4ed8 !important; }
    </style>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- ── Toolbar ──────────────────────────────────────── --%>
            <div class="card card-outline card-primary mb-3">
                <div class="card-body py-2 px-3">
                    <div class="row align-items-center">
                        <div class="col-auto">
                            <label class="mb-0 mr-1 small font-weight-bold">Type A/C :</label>
                        </div>
                        <div class="col-md-2">
                            <asp:DropDownList ID="ddlAcType" runat="server"
                                CssClass="form-control form-control-sm"
                                AutoPostBack="true"
                                OnSelectedIndexChanged="ddlAcType_Changed" />
                        </div>
                        <div class="col-auto">
                            <asp:Literal ID="litTemplateCount" runat="server" />
                        </div>
                        <div class="col-md-3 ml-auto">
                            <div class="input-group input-group-sm">
                                <input type="text" id="treeSearch" class="form-control" placeholder="Rechercher..." />
                                <div class="input-group-append">
                                    <button class="btn btn-outline-secondary" type="button" id="btnClearSearch">
                                        <i class="fas fa-times"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                        <div class="col-auto">
                            <button type="button" id="btnExpandAll"
                                    class="btn btn-sm btn-outline-secondary mr-1"
                                    title="Tout d&eacute;velopper">
                                <i class="fas fa-plus-square"></i>
                            </button>
                            <button type="button" id="btnCollapseAll"
                                    class="btn btn-sm btn-outline-secondary mr-2"
                                    title="Tout r&eacute;duire">
                                <i class="fas fa-minus-square"></i>
                            </button>
                            <asp:Button ID="btnAddZone" runat="server"
                                CssClass="btn btn-sm btn-success"
                                Text="+ Zone" CausesValidation="false" />
                            <asp:Button ID="btnCopyToTails" runat="server"
                                CssClass="btn btn-sm btn-outline-primary ml-1"
                                Text="&#8635; Copier" CausesValidation="false" />
                        </div>
                    </div>
                </div>
            </div>

            <%-- ══ MODALS (unchanged) ════════════════════════════ --%>
            <%-- ... your modals exactly as you already have them ... --%>

            <%-- ══ MODAL: Add/Edit node ════════════════════════════ --%>
            <div class="modal fade" id="nodeModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-md">
                    <div class="modal-content">
                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-sitemap mr-1"></i>
                                <asp:Literal ID="litNodeModalTitle" runat="server" />
                            </h5>
                            <button type="button" class="close text-white" data-dismiss="modal"><span>&times;</span></button>
                        </div>
                        <div class="modal-body">
                            <div class="form-row">
                                <div class="form-group col-md-6">
                                    <label>Code <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtNodeCode" runat="server"
                                        CssClass="form-control form-control-sm text-uppercase"
                                        MaxLength="50" placeholder="ex: MLG-L-TIRE-1" />
                                </div>
                                <div class="form-group col-md-4">
                                    <label>ATA</label>
                                    <asp:DropDownList ID="ddlNodeATA" runat="server"
                                        CssClass="form-control form-control-sm" />
                                </div>
                                <div class="form-group col-md-2">
                                    <label>Ordre</label>
                                    <asp:TextBox ID="txtNodeSort" runat="server"
                                        CssClass="form-control form-control-sm"
                                        Text="100" MaxLength="5" />
                                </div>
                            </div>
                            <div class="form-group">
                                <label>Description</label>
                                <asp:TextBox ID="txtNodeDesc" runat="server"
                                    CssClass="form-control form-control-sm"
                                    MaxLength="200" />
                            </div>
                            <asp:Panel ID="pnlSlotFields" runat="server" Visible="false">
                                <div class="form-row">
                                    <div class="form-group col-md-4">
                                        <label>Quantit&eacute;</label>
                                        <asp:TextBox ID="txtNodeQty" runat="server"
                                            CssClass="form-control form-control-sm"
                                            Text="1" MaxLength="2" />
                                    </div>
                                    <div class="form-group col-md-8 pt-4">
                                        <asp:CheckBox ID="chkInterchangeable" runat="server"
                                            Text="PN interchangeables" />
                                    </div>
                                </div>
                            </asp:Panel>
                            <asp:Label ID="lblNodeError" runat="server" Visible="false"
                                CssClass="alert alert-danger d-block py-2 px-3" />
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnNodeSave" runat="server"
                                CssClass="btn btn-success" Text="Enregistrer"
                                CausesValidation="false" />
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">Annuler</button>
                        </div>
                    </div>
                </div>
            </div>

            <%-- PN modal (unchanged) --%>
            <div class="modal fade" id="pnModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header bg-success text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-barcode mr-1"></i>
                                PN - <asp:Literal ID="litPNModalSlot" runat="server" />
                            </h5>
                            <button type="button" class="close text-white" data-dismiss="modal"><span>&times;</span></button>
                        </div>
                        <div class="modal-body">
                            <asp:GridView ID="gvSlotPN" runat="server"
                                AutoGenerateColumns="false"
                                CssClass="table table-sm mb-3"
                                GridLines="None"
                                DataKeyNames="AcPositionPNId"
                                EmptyDataText="Aucun PN li&eacute;.">
                                <Columns>
                                    <asp:TemplateField HeaderText="Type"
                                        HeaderStyle-CssClass="text-center"
                                        ItemStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <%# If(Convert.ToBoolean(Eval("IsPrimary")),
                                                "<span class='badge badge-primary'>Primaire</span>",
                                                "<span class='badge badge-light border'>Alt</span>") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="PN" HeaderText="PN"
                                        ItemStyle-CssClass="font-weight-bold" />
                                    <asp:BoundField DataField="Nomenclature"
                                        HeaderText="Nomenclature" />
                                    <asp:TemplateField HeaderText=""
                                        ItemStyle-CssClass="text-right">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkRemovePN" runat="server"
                                                CommandName="RemovePN"
                                                CommandArgument='<%# Eval("AcPositionPNId") %>'
                                                CssClass="btn btn-xs btn-outline-danger"
                                                CausesValidation="false"
                                                OnClientClick="return confirm('Retirer ?');">
                                                <i class="fas fa-times"></i>
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <HeaderStyle CssClass="bg-light" />
                            </asp:GridView>

                            <div class="card card-body bg-light py-2 px-3">
                                <div class="form-row align-items-end">
                                    <div class="form-group col-md-7 mb-0">
                                        <label class="small">Ajouter un PN</label>
                                        <asp:DropDownList ID="ddlAddPN" runat="server"
                                            CssClass="form-control form-control-sm" />
                                    </div>
                                    <div class="form-group col-md-3 mb-0">
                                        <asp:CheckBox ID="chkAddPNPrimary" runat="server"
                                            Text="Primaire" Checked="true" />
                                    </div>
                                    <div class="form-group col-md-2 mb-0">
                                        <asp:Button ID="btnAddPN" runat="server"
                                            CssClass="btn btn-sm btn-success btn-block"
                                            Text="Ajouter" CausesValidation="false" />
                                    </div>
                                </div>
                            </div>

                            <asp:Label ID="lblPNError" runat="server" Visible="false"
                                CssClass="alert alert-danger d-block py-2 px-3 mt-2" />
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">Fermer</button>
                        </div>
                    </div>
                </div>
            </div>

            <%-- Copy modal (unchanged) --%>
            <div class="modal fade" id="copyModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-copy mr-1"></i>Copier vers a&eacute;ronefs
                            </h5>
                            <button type="button" class="close text-white" data-dismiss="modal"><span>&times;</span></button>
                        </div>
                        <div class="modal-body">
                            <p><strong><asp:Literal ID="litCopyAcType" runat="server" /></strong></p>
                            <asp:CheckBoxList ID="cblTails" runat="server" CssClass="ml-2" />
                            <small class="text-muted">
                                Positions manquantes uniquement — les existantes ne sont pas &eacute;cras&eacute;es.
                            </small>
                            <asp:Label ID="lblCopyResult" runat="server" Visible="false"
                                CssClass="alert alert-success d-block py-2 px-3 mt-2" />
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnCopyConfirm" runat="server"
                                CssClass="btn btn-primary" Text="Confirmer"
                                CausesValidation="false" />
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">Annuler</button>
                        </div>
                    </div>
                </div>
            </div>

        </ContentTemplate>

        <%-- IMPORTANT: btnDispatch is outside UpdatePanel, add trigger --%>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnDispatch" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>

    <%-- Hidden fields outside UpdatePanel — JS can always find them --%>
    <asp:HiddenField ID="hfNodeId"           runat="server" />
    <asp:HiddenField ID="hfNodeParentId"     runat="server" />
    <asp:HiddenField ID="hfNodeLevel"        runat="server" />
    <asp:HiddenField ID="hfNodeParentCode"   runat="server" />
    <asp:HiddenField ID="hfPNSlotTemplateId" runat="server" />
    <asp:HiddenField ID="hfAction"           runat="server" />

    <%-- IMPORTANT: explicit OnClick makes server dispatch more reliable --%>
    <asp:Button ID="btnDispatch" runat="server"
        style="display:none;" CausesValidation="false"
        OnClick="btnDispatch_Click" />

    <%-- ── Tree OUTSIDE UpdatePanel so postbacks don't destroy jsTree ── --%>
    <div class="card">
        <div class="card-body p-2">
            <div id="posTree">
                <p class="text-center text-muted py-3" id="treeLoading">
                    <i class="fas fa-spinner fa-spin mr-1"></i>Chargement...
                </p>
            </div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="cFooter" ContentPlaceHolderID="FooterScripts" runat="server">

<script type="text/javascript">
    (function waitForJQuery() {
        if (typeof jQuery === 'undefined') {
            setTimeout(waitForJQuery, 30);
            return;
        }

        function start() {
            // Ensure $ is always jQuery inside pageInit (noConflict safe)
            (function ($) { pageInit($); })(jQuery);
        }

        if (typeof jQuery.jstree === 'undefined') {
            var scr = document.createElement('script');
            scr.src = '<%= ResolveUrl("~/AdminLTE-3.2.0/jstree.min.js") %>';
        scr.onload = start;
        document.head.appendChild(scr);
    } else {
        start();
    }
})();

function pageInit($) {
    // ── Resolved server IDs ───────────────────────────────
    var _hfNodeId = '<%= hfNodeId.ClientID %>';
    var _hfParentId = '<%= hfNodeParentId.ClientID %>';
    var _hfLevel = '<%= hfNodeLevel.ClientID %>';
    var _hfParentCode = '<%= hfNodeParentCode.ClientID %>';
    var _hfPNSlot = '<%= hfPNSlotTemplateId.ClientID %>';
    var _hfAction = '<%= hfAction.ClientID %>';
    var _btnDispatch = '<%= btnDispatch.UniqueID %>';
    var _treeUrl = '<%= ResolveUrl("~/MRO2/Setup/Aircraft/AcPositionTemplateTree.ashx") %>';
    var _acTypeId = parseInt('<%= InitialAcTypeId %>', 10) || 0;

    // ── Build tree ────────────────────────────────────────
    function buildTree(acTypeId) {
        if (!acTypeId) { return; }
        _acTypeId = acTypeId;

        $('#treeLoading').show();

        if ($.jstree.reference('#posTree')) {
            $('#posTree').jstree('destroy');
        }

        $('#posTree').jstree({
            core: {
                data: {
                    url: _treeUrl + '?AcTypeId=' + acTypeId,
                    dataType: 'json'
                },
                themes: { dots: true, icons: true },
                force_text: false
            },
            plugins: ['search', 'wholerow'],
            search: {
                show_only_matches: true,
                show_only_matches_children: true
            }
        })
        .on('load_node_failed.jstree', function () {
            $('#treeLoading')
                .removeClass('text-muted')
                .addClass('text-danger')
                .text('Impossible de charger l arborescence.');
        })
        .on('ready.jstree', function () {
            $('#treeLoading').hide();
        });
    }

    // ── Dispatch action to server ─────────────────────────
    function dispatch(action, nodeId, parentId, level, parentCode, pnSlot) {
        document.getElementById(_hfAction).value = action;
        document.getElementById(_hfNodeId).value = nodeId || '';
        document.getElementById(_hfParentId).value = parentId || '';
        document.getElementById(_hfLevel).value = level || '';
        document.getElementById(_hfParentCode).value = parentCode || '';
        document.getElementById(_hfPNSlot).value = pnSlot || '';
        __doPostBack(_btnDispatch, action);
    }

    // ── Node action handlers (use pointerdown so jsTree doesn't swallow clicks) ──
    $(document).off('pointerdown.acpt', '.btn-edit-node')
               .on('pointerdown.acpt', '.btn-edit-node', function (e) {
                   e.preventDefault(); e.stopPropagation();
                   dispatch('edit', $(this).data('id'), '', '', '', '');
               });

    $(document).off('pointerdown.acpt', '.btn-toggle-node')
               .on('pointerdown.acpt', '.btn-toggle-node', function (e) {
                   e.preventDefault(); e.stopPropagation();
                   if (!confirm('Confirmer le changement de statut ?')) return;
                   dispatch('toggle', $(this).data('id'), '', '', '', '');
               });

    $(document).off('pointerdown.acpt', '.btn-add-child')
               .on('pointerdown.acpt', '.btn-add-child', function (e) {
                   e.preventDefault(); e.stopPropagation();
                   dispatch('addchild', '',
                       $(this).data('parentid'),
                       $(this).data('level'),
                       $(this).data('parentcode'),
                       '');
               });

    $(document).off('pointerdown.acpt', '.pn-badge')
               .on('pointerdown.acpt', '.pn-badge', function (e) {
                   e.preventDefault(); e.stopPropagation();
                   dispatch('openpn', '', '', '', '', $(this).data('pnslot'));
               });

    // ── Expand / Collapse + search ────────────────────────
    function bindTreeUiHandlers() {
        var _st;

        $(document).off('click.acpt', '#btnExpandAll')
                   .on('click.acpt', '#btnExpandAll', function () {
                       $('#posTree').jstree('open_all');
                   });

        $(document).off('click.acpt', '#btnCollapseAll')
                   .on('click.acpt', '#btnCollapseAll', function () {
                       $('#posTree').jstree('close_all');
                   });

        $(document).off('keyup.acpt', '#treeSearch')
                   .on('keyup.acpt', '#treeSearch', function () {
                       clearTimeout(_st);
                       var q = $(this).val();
                       _st = setTimeout(function () {
                           $('#posTree').jstree('search', q);
                       }, 300);
                   });

        $(document).off('click.acpt', '#btnClearSearch')
                   .on('click.acpt', '#btnClearSearch', function () {
                       $('#treeSearch').val('');
                       $('#posTree').jstree('clear_search');
                   });
    }

    // ── After UpdatePanel postback: reload tree if needed ─
    if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        var _ddlAcTypeId = '<%= ddlAcType.ClientID %>';

        prm.add_endRequest(function () {
            bindTreeUiHandlers();

            if (window._needTreeReload) {
                window._needTreeReload = false;
                var ddl = document.getElementById(_ddlAcTypeId);
                var newTypeId = ddl ? parseInt(ddl.value, 10) : 0;
                if (newTypeId) buildTree(newTypeId);
            }
        });
    }

    // ── Init ──────────────────────────────────────────────
    $(function () {
        bindTreeUiHandlers();
        if (_acTypeId) buildTree(_acTypeId);
    });
}
</script>

</asp:Content>