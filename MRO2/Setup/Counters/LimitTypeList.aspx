<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="LimitTypeList.aspx.vb"
    Inherits="MRO2_Setup_Counters_LimitTypeList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Types de Limites
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Setup/Default.aspx") %>">Setup</a>
    </li>
    <li class="breadcrumb-item active">Types de Limites</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional"
                     ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- ══ LimitType card ══ --%>
            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        <i class="fas fa-tags mr-1"></i>
                        Types de Limites
                        <small class="text-muted ml-1">
                            (<asp:Literal ID="litRowCount" runat="server" Text="0" /> entr&eacute;es)
                        </small>
                    </h3>
                    <div class="card-tools">
                        <asp:CheckBox ID="chkIncludeInactive" runat="server"
                            AutoPostBack="true" Text="Inclure inactifs" CssClass="mr-3" />
                        <asp:Button ID="btnNew" runat="server"
                            CssClass="btn btn-sm btn-success" Text="+ Nouveau type" />
                    </div>
                </div>

                <div class="card-body p-0">
                    <asp:GridView ID="gvLT" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-sm table-hover mb-0"
                        GridLines="None"
                        DataKeyNames="LimitTypeId"
                        AllowSorting="true">
                        <Columns>

                            <%-- Badge --%>
                            <asp:TemplateField HeaderText="Code"
                                SortExpression="Code"
                                ItemStyle-CssClass="text-center"
                                HeaderStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <span class='badge badge-<%# Eval("BadgeColor") %>'
                                          style="font-size:.8rem;">
                                        <%# Eval("Code") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="Name"
                                HeaderText="D&eacute;signation"
                                SortExpression="Name" />

                            <%-- Valid ComputationReferences (from map) --%>
                            <asp:TemplateField HeaderText="R&eacute;f&eacute;rences de calcul valides">
                                <ItemTemplate>
                                    <%# RenderMapBadges(CInt(Eval("LimitTypeId"))) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="SortOrder" HeaderText="Ordre"
                                SortExpression="SortOrder"
                                ItemStyle-CssClass="text-center"
                                HeaderStyle-CssClass="text-center" />

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

                            <asp:TemplateField HeaderText="Actions"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkEdit" runat="server"
                                        CommandName="EditRow"
                                        CommandArgument='<%# Eval("LimitTypeId") %>'
                                        CssClass="btn btn-xs btn-outline-primary"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i> Modifier
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkToggle" runat="server"
                                        CommandName="ToggleActive"
                                        CommandArgument='<%# Eval("LimitTypeId") %>'
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
                                Aucun type de limite trouv&eacute;.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <asp:HiddenField ID="hfLimitTypeId" runat="server" />

            <%-- MODAL — LimitType edit (Code, Name, BadgeColor, SortOrder) --%>
            <div class="modal fade" id="ltModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-md">
                    <div class="modal-content">
                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-tags mr-1"></i>
                                <asp:Literal ID="litModalTitle" runat="server" />
                            </h5>
                            <button type="button" class="close text-white"
                                    data-dismiss="modal"><span>&times;</span></button>
                        </div>
                        <div class="modal-body">
                            <div class="form-row">
                                <div class="form-group col-md-5">
                                    <label>Code <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtCode" runat="server"
                                        CssClass="form-control form-control-sm text-uppercase"
                                        MaxLength="20"
                                        placeholder="ex: LIFE, INSPECTION" />
                                </div>
                                <div class="form-group col-md-4">
                                    <label>Couleur badge</label>
                                    <asp:DropDownList ID="ddlBadgeColor" runat="server"
                                        CssClass="form-control form-control-sm">
                                        <asp:ListItem Value="danger">
                                            danger (rouge)
                                        </asp:ListItem>
                                        <asp:ListItem Value="warning">
                                            warning (ambre)
                                        </asp:ListItem>
                                        <asp:ListItem Value="info">
                                            info (bleu)
                                        </asp:ListItem>
                                        <asp:ListItem Value="secondary">
                                            secondary (gris)
                                        </asp:ListItem>
                                        <asp:ListItem Value="success">
                                            success (vert)
                                        </asp:ListItem>
                                        <asp:ListItem Value="dark">
                                            dark (noir)
                                        </asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group col-md-3">
                                    <label>Ordre</label>
                                    <asp:TextBox ID="txtSortOrder" runat="server"
                                        CssClass="form-control form-control-sm"
                                        Text="99" MaxLength="3" />
                                </div>
                            </div>
                            <div class="form-group">
                                <label>D&eacute;signation <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtName" runat="server"
                                    CssClass="form-control form-control-sm"
                                    MaxLength="100"
                                    placeholder="ex: Life Limit, Shelf Life (calendar)" />
                            </div>
                            <div class="form-group">
                                <label>Description
                                    <small class="text-muted">(optionnel)</small>
                                </label>
                                <asp:TextBox ID="txtDescription" runat="server"
                                    CssClass="form-control form-control-sm"
                                    TextMode="MultiLine" Rows="2" MaxLength="300" />
                            </div>

                            <%-- Preview badge --%>
                            <div class="form-group mb-0">
                                <label class="small text-muted">Aper&ccedil;u</label><br />
                                <span id="badgePreview" class="badge badge-danger"
                                      style="font-size:.85rem;">LIFE</span>
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
        // Live badge preview in modal
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
