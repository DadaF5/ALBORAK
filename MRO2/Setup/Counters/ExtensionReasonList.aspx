<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="ExtensionReasonList.aspx.vb"
    Inherits="MRO2_Setup_Counters_ExtensionReasonList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Motifs de Prolongation
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Setup/Default.aspx") %>">Setup</a>
    </li>
    <li class="breadcrumb-item active">Motifs de Prolongation</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional"
                     ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- Info banner --%>
            <div class="alert alert-light border-left border-warning mb-3 py-2 px-3"
                 style="border-left-width:4px!important;font-size:.85rem;">
                <i class="fas fa-exclamation-triangle text-warning mr-1"></i>
                Les <strong>motifs de prolongation</strong> justifient toute extension
                d&apos;une &eacute;ch&eacute;ance compteur au-del&agrave; de sa limite initiale.
                Chaque motif d&eacute;finit si une <strong>r&eacute;f&eacute;rence documentaire</strong>
                et/ou un <strong>approbateur</strong> sont obligatoires avant d&apos;accorder
                la prolongation.
            </div>

            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        <i class="fas fa-clock mr-1"></i>
                        Motifs de Prolongation
                        <small class="text-muted ml-1">
                            (<asp:Literal ID="litRowCount" runat="server" Text="0" /> entr&eacute;es)
                        </small>
                    </h3>
                    <div class="card-tools">
                        <asp:CheckBox ID="chkIncludeInactive" runat="server"
                            AutoPostBack="true" Text="Inclure inactifs" CssClass="mr-3" />
                        <asp:Button ID="btnNew" runat="server"
                            CssClass="btn btn-sm btn-success"
                            Text="+ Nouveau motif" />
                    </div>
                </div>

                <div class="card-body p-0">
                    <asp:GridView ID="gvER" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-sm table-hover mb-0"
                        GridLines="None"
                        DataKeyNames="ExtensionReasonId"
                        AllowSorting="true">
                        <Columns>

                            <%-- Code badge --%>
                            <asp:TemplateField HeaderText="Code"
                                SortExpression="Code"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <span class='badge badge-<%# Eval("BadgeColor") %>'
                                          style="font-size:.78rem;">
                                        <%# Eval("Code") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- Name --%>
                            <asp:BoundField DataField="Name"
                                HeaderText="D&eacute;signation"
                                SortExpression="Name" />

                            <%-- RequiresDocRef --%>
                            <asp:TemplateField HeaderText="R&eacute;f. Doc."
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# RequiredBadge(Convert.ToBoolean(Eval("RequiresDocRef"))) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- RequiresApprover --%>
                            <asp:TemplateField HeaderText="Approbateur"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# RequiredBadge(Convert.ToBoolean(Eval("RequiresApprover"))) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- SortOrder --%>
                            <asp:BoundField DataField="SortOrder" HeaderText="Ordre"
                                SortExpression="SortOrder"
                                ItemStyle-CssClass="text-center"
                                HeaderStyle-CssClass="text-center" />

                            <%-- Active --%>
                            <asp:TemplateField HeaderText="Statut"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <span class='badge <%# If(Convert.ToBoolean(Eval("IsActive")),
                                                            "badge-success","badge-secondary") %>'>
                                        <%# If(Convert.ToBoolean(Eval("IsActive")),
                                                "Actif","Inactif") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- Actions --%>
                            <asp:TemplateField HeaderText="Actions"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkEdit" runat="server"
                                        CommandName="EditRow"
                                        CommandArgument='<%# Eval("ExtensionReasonId") %>'
                                        CssClass="btn btn-xs btn-outline-primary"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i> Modifier
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkToggle" runat="server"
                                        CommandName="ToggleActive"
                                        CommandArgument='<%# Eval("ExtensionReasonId") %>'
                                        CssClass='<%# If(Convert.ToBoolean(Eval("IsActive")),
                                                        "btn btn-xs btn-outline-danger ml-1",
                                                        "btn btn-xs btn-outline-success ml-1") %>'
                                        CausesValidation="false"
                                        OnClientClick="return confirm('Confirmer ?');">
                                        <%# If(Convert.ToBoolean(Eval("IsActive")),
                                                "D&eacute;sactiver","Activer") %>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                        <HeaderStyle CssClass="bg-primary text-white" />
                        <EmptyDataTemplate>
                            <div class="text-center text-muted py-4">
                                <i class="fas fa-inbox fa-2x mb-2"></i><br />
                                Aucun motif trouv&eacute;.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <asp:HiddenField ID="hfExtensionReasonId" runat="server" />

            <%-- MODAL --%>
            <div class="modal fade" id="erModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-md">
                    <div class="modal-content">

                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-clock mr-1"></i>
                                <asp:Literal ID="litModalTitle" runat="server" />
                            </h5>
                            <button type="button" class="close text-white"
                                    data-dismiss="modal"><span>&times;</span></button>
                        </div>

                        <div class="modal-body">
                            <div class="form-row">
                                <%-- Code --%>
                                <div class="form-group col-md-5">
                                    <label>Code <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtCode" runat="server"
                                        CssClass="form-control form-control-sm text-uppercase"
                                        MaxLength="20"
                                        placeholder="ex: MFR_TOL" />
                                </div>
                                <%-- Badge color --%>
                                <div class="form-group col-md-4">
                                    <label>Couleur badge</label>
                                    <asp:DropDownList ID="ddlBadgeColor" runat="server"
                                        CssClass="form-control form-control-sm">
                                        <asp:ListItem Value="info">info (bleu)</asp:ListItem>
                                        <asp:ListItem Value="primary">primary (indigo)</asp:ListItem>
                                        <asp:ListItem Value="warning">warning (ambre)</asp:ListItem>
                                        <asp:ListItem Value="danger">danger (rouge)</asp:ListItem>
                                        <asp:ListItem Value="secondary">secondary (gris)</asp:ListItem>
                                        <asp:ListItem Value="success">success (vert)</asp:ListItem>
                                        <asp:ListItem Value="dark">dark (noir)</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <%-- SortOrder --%>
                                <div class="form-group col-md-3">
                                    <label>Ordre</label>
                                    <asp:TextBox ID="txtSortOrder" runat="server"
                                        CssClass="form-control form-control-sm"
                                        Text="99" MaxLength="3" />
                                </div>
                            </div>

                            <%-- Name --%>
                            <div class="form-group">
                                <label>D&eacute;signation <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtName" runat="server"
                                    CssClass="form-control form-control-sm"
                                    MaxLength="150"
                                    placeholder="ex: Tol&eacute;rance Constructeur" />
                            </div>

                            <%-- Description --%>
                            <div class="form-group">
                                <label>Description
                                    <small class="text-muted">(optionnel)</small>
                                </label>
                                <asp:TextBox ID="txtDescription" runat="server"
                                    CssClass="form-control form-control-sm"
                                    TextMode="MultiLine" Rows="2" MaxLength="300" />
                            </div>

                            <%-- Mandatory field flags --%>
                            <div class="card card-body bg-light py-2 px-3 mb-2">
                                <p class="mb-2 small font-weight-bold text-muted text-uppercase">
                                    Champs obligatoires lors de la prolongation
                                </p>
                                <div class="form-check mb-1">
                                    <asp:CheckBox ID="chkRequiresDocRef" runat="server"
                                        CssClass="form-check-input" />
                                    <label class="form-check-label small"
                                           for="<%= chkRequiresDocRef.ClientID %>">
                                        <i class="fas fa-file-alt text-primary mr-1"></i>
                                        <strong>R&eacute;f&eacute;rence documentaire obligatoire</strong>
                                        <span class="text-muted">
                                            (CMM, SB, AD, Ordre d&apos;Ing&eacute;nierie)
                                        </span>
                                    </label>
                                </div>
                                <div class="form-check">
                                    <asp:CheckBox ID="chkRequiresApprover" runat="server"
                                        CssClass="form-check-input" />
                                    <label class="form-check-label small"
                                           for="<%= chkRequiresApprover.ClientID %>">
                                        <i class="fas fa-user-check text-warning mr-1"></i>
                                        <strong>Approbateur obligatoire</strong>
                                        <span class="text-muted">
                                            (nom et grade de l&apos;autorit&eacute; approbatrice)
                                        </span>
                                    </label>
                                </div>
                            </div>

                            <%-- Live badge preview --%>
                            <div class="form-group mb-0">
                                <label class="small text-muted">Aper&ccedil;u</label><br />
                                <span id="badgePreview" class="badge badge-info"
                                      style="font-size:.85rem;">MFR_TOL</span>
                            </div>

                            <asp:Label ID="lblError" runat="server" Visible="false"
                                CssClass="alert alert-danger d-block py-2 px-3 mt-3" />
                        </div>

                        <div class="modal-footer">
                            <asp:Button ID="btnSave" runat="server"
                                CssClass="btn btn-success" Text="Enregistrer" />
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
        // Live badge preview
        $(document).on('input change',
            '#<%= txtCode.ClientID %>, #<%= ddlBadgeColor.ClientID %>',
            function () {
                var code  = $('#<%= txtCode.ClientID %>').val() || 'CODE';
                var color = $('#<%= ddlBadgeColor.ClientID %>').val() || 'secondary';
                $('#badgePreview')
                    .removeClass()
                    .addClass('badge badge-' + color)
                    .text(code.toUpperCase());
            });
    </script>
</asp:Content>
