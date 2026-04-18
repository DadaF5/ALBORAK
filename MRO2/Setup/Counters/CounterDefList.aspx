<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="CounterDefList.aspx.vb"
    Inherits="MRO2_Setup_Counters_CounterDefList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    D&eacute;finitions de Compteurs
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Setup/Default.aspx") %>">Setup</a>
    </li>
    <li class="breadcrumb-item active">D&eacute;finitions de Compteurs</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional"
                     ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- Info banner --%>
            <div class="alert alert-light border-left border-primary mb-3 py-2 px-3"
                 style="border-left-width:4px!important;font-size:.85rem;">
                <i class="fas fa-info-circle text-primary mr-1"></i>
                <strong>AIRCRAFT</strong> &mdash; compteurs propag&eacute;s depuis le carnet de vol
                (FH, FC, atterrissages).&nbsp;&nbsp;
                <strong>COMPONENT</strong> &mdash; compteurs propres au composant
                (APU hrs, Engine hrs, starts) &mdash; suivis par num&eacute;ro de s&eacute;rie.
            </div>

            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        <i class="fas fa-list-ol mr-1"></i>
                        D&eacute;finitions de Compteurs
                        <small class="text-muted ml-1">
                            (<asp:Literal ID="litRowCount" runat="server" Text="0" /> entr&eacute;es)
                        </small>
                    </h3>
                    <div class="card-tools d-flex align-items-center flex-wrap">

                        <%-- Filter: CounterType --%>
                        <label class="mb-0 mr-1 text-muted small">Type&nbsp;:</label>
                        <asp:DropDownList ID="ddlFilterType" runat="server"
                            CssClass="form-control form-control-sm mr-2"
                            AutoPostBack="true" style="width:150px;"
                            OnSelectedIndexChanged="ddlFilterType_SelectedIndexChanged" />

                        <%-- Filter: AssetKind --%>
                        <label class="mb-0 mr-1 text-muted small">Asset&nbsp;:</label>
                        <asp:DropDownList ID="ddlFilterAsset" runat="server"
                            CssClass="form-control form-control-sm mr-3"
                            AutoPostBack="true" style="width:140px;"
                            OnSelectedIndexChanged="ddlFilterAsset_SelectedIndexChanged">
                            <asp:ListItem Value="">-- Tous --</asp:ListItem>
                            <asp:ListItem Value="AIRCRAFT">AIRCRAFT</asp:ListItem>
                            <asp:ListItem Value="COMPONENT">COMPONENT</asp:ListItem>
                        </asp:DropDownList>

                        <asp:CheckBox ID="chkIncludeInactive" runat="server"
                            AutoPostBack="true" Text="Inclure inactifs" CssClass="mr-3" />

                        <asp:Button ID="btnNew" runat="server"
                            CssClass="btn btn-sm btn-success"
                            Text="+ Nouveau compteur" />
                    </div>
                </div>

                <div class="card-body p-0">
                    <asp:GridView ID="gvCD" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-sm table-hover mb-0"
                        GridLines="None"
                        DataKeyNames="CounterDefId"
                        AllowSorting="true">
                        <Columns>

                            <%-- AssetKind badge --%>
                            <asp:TemplateField HeaderText="Asset"
                                SortExpression="AppliesToAssetKindCode"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# AssetBadge(Eval("AppliesToAssetKindCode").ToString()) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- CounterType --%>
                            <asp:TemplateField HeaderText="Type"
                                SortExpression="CounterTypeCode">
                                <ItemTemplate>
                                    <span class="badge badge-primary"
                                          style="font-size:.75rem;">
                                        <%# Eval("CounterTypeCode") %>
                                    </span>
                                    <small class="text-muted ml-1">
                                        <%# Eval("DisplayUnit") %>
                                    </small>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- Code --%>
                            <asp:BoundField DataField="Code" HeaderText="Code"
                                SortExpression="Code"
                                ItemStyle-CssClass="font-weight-bold" />

                            <%-- Name --%>
                            <asp:BoundField DataField="Name" HeaderText="D&eacute;signation"
                                SortExpression="Name" />

                            <%-- UnitStorage --%>
                            <asp:TemplateField HeaderText="Stockage"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# StorageBadge(Eval("UnitStorage").ToString()) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- Ordre --%>
                            <asp:BoundField DataField="SortOrder" HeaderText="Ordre"
                                SortExpression="SortOrder"
                                ItemStyle-CssClass="text-center"
                                HeaderStyle-CssClass="text-center" />

                            <%-- Statut --%>
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
                                        CommandArgument='<%# Eval("CounterDefId") %>'
                                        CssClass="btn btn-xs btn-outline-primary"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i> Modifier
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkToggle" runat="server"
                                        CommandName="ToggleActive"
                                        CommandArgument='<%# Eval("CounterDefId") %>'
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
                                Aucun compteur trouv&eacute;.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <asp:HiddenField ID="hfCounterDefId" runat="server" />

            <%-- MODAL --%>
            <div class="modal fade" id="cdModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-list-ol mr-1"></i>
                                <asp:Literal ID="litModalTitle" runat="server" />
                            </h5>
                            <button type="button" class="close text-white"
                                    data-dismiss="modal"><span>&times;</span></button>
                        </div>
                        <div class="modal-body">
                            <div class="form-row">
                                <%-- CounterType --%>
                                <div class="form-group col-md-4">
                                    <label>Type de compteur <span class="text-danger">*</span></label>
                                    <asp:DropDownList ID="ddlModalType" runat="server"
                                        CssClass="form-control form-control-sm" />
                                    <small class="form-text text-muted">
                                        D&eacute;termine le stockage (MINUTES ou COUNT).
                                    </small>
                                </div>
                                <%-- Code --%>
                                <div class="form-group col-md-4">
                                    <label>Code <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtCode" runat="server"
                                        CssClass="form-control form-control-sm text-uppercase"
                                        MaxLength="30"
                                        placeholder="ex: AF_FLIGHT_MIN" />
                                </div>
                                <%-- SortOrder --%>
                                <div class="form-group col-md-4">
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
                                    placeholder="ex: Aircraft Flight Time (minutes)" />
                            </div>

                            <%-- AppliesToAssetKindCode --%>
                            <div class="form-group">
                                <label>S&apos;applique &agrave; <span class="text-danger">*</span></label>
                                <asp:DropDownList ID="ddlAssetKind" runat="server"
                                    CssClass="form-control form-control-sm">
                                    <asp:ListItem Value="AIRCRAFT"
                                        Text="AIRCRAFT — propag&eacute; depuis le carnet de vol A/C" />
                                    <asp:ListItem Value="COMPONENT"
                                        Text="COMPONENT — propre au composant (APU, moteur...)" />
                                </asp:DropDownList>
                                <small class="form-text text-muted">
                                    <strong>AIRCRAFT</strong> : FH, FC, atterrissages — driven by airframe logbook.<br />
                                    <strong>COMPONENT</strong> : APU hrs, Engine hrs, starts — tracked individually per SN.
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
