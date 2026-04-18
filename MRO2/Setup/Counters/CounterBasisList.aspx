<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="CounterBasisList.aspx.vb"
    Inherits="MRO2_Setup_Counters_CounterBasisList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Bases de Comptage
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Setup/Default.aspx") %>">Setup</a>
    </li>
    <li class="breadcrumb-item active">Bases de Comptage</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional"
                     ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- Info banner --%>
            <div class="alert alert-light border-left border-primary mb-3 py-2 px-3"
                 style="border-left-width:4px!important;font-size:.85rem;">
                <i class="fas fa-info-circle text-primary mr-1"></i>
                La <strong>base de comptage</strong> d&eacute;finit le point de d&eacute;part
                &agrave; partir duquel un compteur s&apos;accumule.
                Elle d&eacute;termine aussi ce qui r&eacute;initialise le compteur
                (installation, r&eacute;vision...).
                <strong>ABSOLUTE</strong> ne se r&eacute;initialise jamais.
            </div>

            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        <i class="fas fa-history mr-1"></i>
                        Bases de Comptage
                        <small class="text-muted ml-1">
                            (<asp:Literal ID="litRowCount" runat="server" Text="0" /> entr&eacute;es)
                        </small>
                    </h3>
                    <div class="card-tools">
                        <asp:CheckBox ID="chkIncludeInactive" runat="server"
                            AutoPostBack="true" Text="Inclure inactifs" CssClass="mr-3" />
                        <asp:Button ID="btnNew" runat="server"
                            CssClass="btn btn-sm btn-success"
                            Text="+ Nouvelle base" />
                    </div>
                </div>

                <div class="card-body p-0">
                    <asp:GridView ID="gvCB" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-sm table-hover mb-0"
                        GridLines="None"
                        DataKeyNames="CounterBasisId"
                        AllowSorting="true">
                        <Columns>

                            <%-- Code --%>
                            <asp:BoundField DataField="Code" HeaderText="Code"
                                SortExpression="Code"
                                ItemStyle-CssClass="font-weight-bold" />

                            <%-- Name --%>
                            <asp:BoundField DataField="Name"
                                HeaderText="D&eacute;signation"
                                SortExpression="Name" />

                            <%-- Description truncated --%>
                            <asp:TemplateField HeaderText="Description">
                                <ItemTemplate>
                                    <small class="text-muted">
                                        <%# TruncateDesc(Eval("Description")) %>
                                    </small>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- Reset badge --%>
                            <asp:TemplateField HeaderText="R&eacute;initialisation"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# ResetBadge(Eval("Code").ToString()) %>
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
                                        CommandArgument='<%# Eval("CounterBasisId") %>'
                                        CssClass="btn btn-xs btn-outline-primary"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i> Modifier
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkToggle" runat="server"
                                        CommandName="ToggleActive"
                                        CommandArgument='<%# Eval("CounterBasisId") %>'
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
                                Aucune base de comptage trouv&eacute;e.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <asp:HiddenField ID="hfCounterBasisId" runat="server" />

            <%-- MODAL --%>
            <div class="modal fade" id="cbModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-md">
                    <div class="modal-content">

                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-history mr-1"></i>
                                <asp:Literal ID="litModalTitle" runat="server" />
                            </h5>
                            <button type="button" class="close text-white"
                                    data-dismiss="modal"><span>&times;</span></button>
                        </div>

                        <div class="modal-body">
                            <div class="form-row">
                                <%-- Code --%>
                                <div class="form-group col-md-6">
                                    <label>Code <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtCode" runat="server"
                                        CssClass="form-control form-control-sm text-uppercase"
                                        MaxLength="20"
                                        placeholder="ex: SINCE_INSTALL" />
                                    <small class="form-text text-muted">
                                        Majuscules, sans espaces.
                                    </small>
                                </div>
                                <%-- SortOrder --%>
                                <div class="form-group col-md-6">
                                    <label>Ordre affichage</label>
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
                                    MaxLength="100"
                                    placeholder="ex: Depuis l&apos;installation" />
                            </div>

                            <%-- Description --%>
                            <div class="form-group">
                                <label>
                                    Description
                                    <small class="text-muted">(affich&eacute;e en info-bulle)</small>
                                </label>
                                <asp:TextBox ID="txtDescription" runat="server"
                                    CssClass="form-control form-control-sm"
                                    TextMode="MultiLine" Rows="3" MaxLength="300"
                                    placeholder="Quand ce compteur se r&eacute;initialise-t-il ?" />
                            </div>

                            <asp:Label ID="lblError" runat="server" Visible="false"
                                CssClass="alert alert-danger d-block py-2 px-3" />
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
