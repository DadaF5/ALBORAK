<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="CounterTypeList.aspx.vb"
    Inherits="MRO2_Setup_Counters_CounterTypeList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Types de Compteurs
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Setup/Default.aspx") %>">Setup</a>
    </li>
    <li class="breadcrumb-item active">Types de Compteurs</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional"
                     ChildrenAsTriggers="true">
        <ContentTemplate>

            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        <i class="fas fa-tachometer-alt mr-1"></i>
                        Types de Compteurs
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
                    <asp:GridView ID="gvCT" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-sm table-hover mb-0"
                        GridLines="None"
                        DataKeyNames="CounterTypeId"
                        AllowSorting="true">
                        <Columns>
                            <asp:BoundField DataField="Code" HeaderText="Code"
                                SortExpression="Code"
                                ItemStyle-CssClass="font-weight-bold" />
                            <asp:BoundField DataField="Name" HeaderText="D&eacute;signation"
                                SortExpression="Name" />
                            <%-- UnitStorage badge --%>
                            <asp:TemplateField HeaderText="Stockage"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# StorageBadge(Eval("UnitStorage").ToString()) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <%-- DisplayUnit --%>
                            <asp:BoundField DataField="DisplayUnit"
                                HeaderText="Unit&eacute; affich&eacute;e"
                                SortExpression="DisplayUnit"
                                ItemStyle-CssClass="text-center"
                                HeaderStyle-CssClass="text-center" />
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
                                        CommandArgument='<%# Eval("CounterTypeId") %>'
                                        CssClass="btn btn-xs btn-outline-primary"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i> Modifier
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkToggle" runat="server"
                                        CommandName="ToggleActive"
                                        CommandArgument='<%# Eval("CounterTypeId") %>'
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
                                Aucun type trouv&eacute;.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <asp:HiddenField ID="hfCounterTypeId" runat="server" />

            <%-- MODAL --%>
            <div class="modal fade" id="ctModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-md">
                    <div class="modal-content">
                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-tachometer-alt mr-1"></i>
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
                                        MaxLength="20" placeholder="ex: FLIGHT_HOURS" />
                                </div>
                                <div class="form-group col-md-4">
                                    <label>Unit&eacute; affich&eacute;e <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtDisplayUnit" runat="server"
                                        CssClass="form-control form-control-sm"
                                        MaxLength="20" placeholder="hrs, cycles, ldg..." />
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
                                    MaxLength="100" placeholder="ex: Aircraft Flight Hours" />
                            </div>
                            <div class="form-group">
                                <label>Stockage physique <span class="text-danger">*</span></label>
                                <asp:DropDownList ID="ddlUnitStorage" runat="server"
                                    CssClass="form-control form-control-sm">
                                    <asp:ListItem Value="COUNT"   Text="COUNT — entier (cycles, landings, starts)" />
                                    <asp:ListItem Value="MINUTES" Text="MINUTES — stock&eacute; en minutes entières (heures de vol)" />
                                </asp:DropDownList>
                                <small class="form-text text-muted">
                                    <strong>MINUTES</strong> uniquement pour les compteurs de temps
                                    (FH, APU hrs, engine hrs). Tout le reste : <strong>COUNT</strong>.
                                </small>
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
