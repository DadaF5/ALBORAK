<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="ComputationReferenceList.aspx.vb"
    Inherits="MRO2_Setup_Counters_ComputationReferenceList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    R&eacute;f&eacute;rences de Calcul
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Setup/Default.aspx") %>">Setup</a>
    </li>
    <li class="breadcrumb-item active">R&eacute;f&eacute;rences de Calcul</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional"
                     ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- Distinction banner --%>
            <div class="row mb-3">
                <div class="col-md-6">
                    <div class="alert alert-info border-0 py-2 px-3 mb-0"
                         style="font-size:.84rem;">
                        <i class="fas fa-calendar-alt mr-1"></i>
                        <strong>R&eacute;f&eacute;rences de Calcul</strong> &mdash;
                        &laquo;&nbsp;depuis quand&nbsp;&raquo; on compte
                        (SNEW, SOH, date d&apos;installation...).
                        Utilis&eacute;es dans la d&eacute;finition des limites PN.
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="alert alert-secondary border-0 py-2 px-3 mb-0"
                         style="font-size:.84rem;">
                        <i class="fas fa-file-alt mr-1"></i>
                        <strong>R&eacute;f&eacute;rences Documentaires</strong> &mdash;
                        &laquo;&nbsp;quel document&nbsp;&raquo; autorise la limite
                        (AMM, CMM, SB, AD...).
                        G&eacute;r&eacute;es dans
                        <a href="<%= ResolveUrl("~/MRO2/Setup/CounterReferenceList.aspx")%>">
                            R&eacute;f&eacute;rences Documentaires</a>.
                    </div>
                </div>
            </div>

            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        <i class="fas fa-calendar-alt mr-1"></i>
                        R&eacute;f&eacute;rences de Calcul
                        <small class="text-muted ml-1">
                            (<asp:Literal ID="litRowCount" runat="server" Text="0" /> entr&eacute;es)
                        </small>
                    </h3>
                    <div class="card-tools">
                        <asp:CheckBox ID="chkIncludeInactive" runat="server"
                            AutoPostBack="true" Text="Inclure inactifs" CssClass="mr-3" />
                        <asp:Button ID="btnNew" runat="server"
                            CssClass="btn btn-sm btn-success"
                            Text="+ Nouvelle r&eacute;f&eacute;rence" />
                    </div>
                </div>

                <div class="card-body p-0">
                    <asp:GridView ID="gvCR" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-sm table-hover mb-0"
                        GridLines="None"
                        DataKeyNames="ComputationReferenceId"
                        AllowSorting="true">
                        <Columns>

                            <asp:BoundField DataField="Code" HeaderText="Code"
                                SortExpression="Code"
                                ItemStyle-CssClass="font-weight-bold" />

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
                                        CommandArgument='<%# Eval("ComputationReferenceId") %>'
                                        CssClass="btn btn-xs btn-outline-primary"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i> Modifier
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkToggle" runat="server"
                                        CommandName="ToggleActive"
                                        CommandArgument='<%# Eval("ComputationReferenceId") %>'
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
                                Aucune r&eacute;f&eacute;rence trouv&eacute;e.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <asp:HiddenField ID="hfComputationReferenceId" runat="server" />

            <%-- MODAL --%>
            <div class="modal fade" id="crModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-md">
                    <div class="modal-content">
                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-calendar-alt mr-1"></i>
                                <asp:Literal ID="litModalTitle" runat="server" />
                            </h5>
                            <button type="button" class="close text-white"
                                    data-dismiss="modal"><span>&times;</span></button>
                        </div>
                        <div class="modal-body">
                            <div class="form-row">
                                <div class="form-group col-md-6">
                                    <label>Code <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtCode" runat="server"
                                        CssClass="form-control form-control-sm text-uppercase"
                                        MaxLength="30"
                                        placeholder="ex: SINCE_NEW, CURE_DATE" />
                                </div>
                                <div class="form-group col-md-6">
                                    <label>Ordre affichage</label>
                                    <asp:TextBox ID="txtSortOrder" runat="server"
                                        CssClass="form-control form-control-sm"
                                        Text="99" MaxLength="3" />
                                </div>
                            </div>
                            <div class="form-group">
                                <label>D&eacute;signation <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtName" runat="server"
                                    CssClass="form-control form-control-sm"
                                    MaxLength="150"
                                    placeholder="ex: Since New, Manufacture Date" />
                            </div>
                            <div class="form-group">
                                <label>Description
                                    <small class="text-muted">(affich&eacute;e en info-bulle)</small>
                                </label>
                                <asp:TextBox ID="txtDescription" runat="server"
                                    CssClass="form-control form-control-sm"
                                    TextMode="MultiLine" Rows="3" MaxLength="300"
                                    placeholder="Quand ce point de r&eacute;f&eacute;rence s'applique..." />
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
