<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="CounterReferenceList.aspx.vb"
    Inherits="MRO2_Setup_Counters_CounterReferenceList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    R&eacute;f&eacute;rences Compteurs
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item"><a href="<%= ResolveUrl("~/MRO2/Setup/Default.aspx") %>">Setup</a></li>
    <li class="breadcrumb-item active">R&eacute;f&eacute;rences</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- Info banner: why this table exists --%>
            <div class="alert alert-light border-left border-primary mb-3 py-2 px-3"
                 style="border-left-width:4px!important;">
                <i class="fas fa-info-circle text-primary mr-1"></i>
                Les r&eacute;f&eacute;rences sont <strong>globales</strong> - Elles s&apos;appliquent
                &agrave; tous les types de compteurs. Elles d&eacute;finissent l&apos;&eacute;v&eacute;nement
                ou le document qui &eacute;tablit ou r&eacute;initialise une limite
                (ex&nbsp;: SNEW, SOH, AMM 05-10-10).
            </div>

            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        <i class="fas fa-book mr-1"></i>
                        R&eacute;f&eacute;rences Compteurs
                        <small class="text-muted ml-1">
                            (<asp:Literal ID="litRowCount" runat="server" Text="0" /> entr&eacute;es)
                        </small>
                    </h3>
                    <div class="card-tools d-flex align-items-center">
                        <%-- Filter by category --%>
                        <label class="mb-0 mr-2 text-muted" style="font-size:.82rem;">
                            Cat&eacute;gorie&nbsp;:
                        </label>
                        <asp:DropDownList ID="ddlFilterCategory" runat="server"
                            CssClass="form-control form-control-sm mr-3"
                            AutoPostBack="true"
                            style="width:140px;"
                            OnSelectedIndexChanged="ddlFilterCategory_SelectedIndexChanged">
                            <asp:ListItem Value="">-- Toutes --</asp:ListItem>
                            <asp:ListItem Value="EVENT">&Eacute;v&eacute;nement</asp:ListItem>
                            <asp:ListItem Value="DOCUMENT">Document</asp:ListItem>
                        </asp:DropDownList>

                        <asp:CheckBox ID="chkIncludeInactive" runat="server" AutoPostBack="true"
                            Text="Inclure inactifs" CssClass="mr-3" />

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
                        DataKeyNames="CounterReferenceId"
                        AllowSorting="true">
                        <Columns>
                            <%-- Category badge --%>
                            <asp:TemplateField HeaderText="Cat&eacute;gorie"
                                SortExpression="RefCategory"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# CategoryBadge(Eval("RefCategory").ToString()) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <%-- Code --%>
                            <asp:BoundField DataField="Code" HeaderText="Code"
                                SortExpression="Code"
                                ItemStyle-CssClass="font-weight-bold" />
                            <%-- Name --%>
                            <asp:BoundField DataField="Name" HeaderText="D&eacute;signation"
                                SortExpression="Name" />
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
                                        <%# If(Convert.ToBoolean(Eval("IsActive")), "Actif", "Inactif") %>
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
                                        CommandArgument='<%# Eval("CounterReferenceId") %>'
                                        CssClass="btn btn-xs btn-outline-primary"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i> Modifier
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkToggle" runat="server"
                                        CommandName="ToggleActive"
                                        CommandArgument='<%# Eval("CounterReferenceId") %>'
                                        CssClass='<%# If(Convert.ToBoolean(Eval("IsActive")),
                                                        "btn btn-xs btn-outline-danger ml-1",
                                                        "btn btn-xs btn-outline-success ml-1") %>'
                                        CausesValidation="false"
                                        OnClientClick="return confirm('Confirmer le changement de statut ?');">
                                        <%# If(Convert.ToBoolean(Eval("IsActive")), "D&eacute;sactiver", "Activer") %>
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

            <asp:HiddenField ID="hfCounterReferenceId" runat="server" />

            <%-- ═══ MODAL ═══ --%>
            <div class="modal fade" id="crModal" tabindex="-1" role="dialog" aria-hidden="true">
                <div class="modal-dialog modal-md" role="document">
                    <div class="modal-content">

                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-book mr-1"></i>
                                <asp:Literal ID="litModalTitle" runat="server" Text="Nouvelle r&eacute;f&eacute;rence" />
                            </h5>
                            <button type="button" class="close text-white" data-dismiss="modal">
                                <span>&times;</span>
                            </button>
                        </div>

                        <div class="modal-body">
                            <div class="form-row">
                                <%-- Code --%>
                                <div class="form-group col-md-5">
                                    <label>Code <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtCode" runat="server"
                                        CssClass="form-control form-control-sm text-uppercase"
                                        MaxLength="30"
                                        placeholder="ex: SNEW, SOH, AMM" />
                                </div>
                                <%-- Category --%>
                                <div class="form-group col-md-4">
                                    <label>Cat&eacute;gorie <span class="text-danger">*</span></label>
                                    <asp:DropDownList ID="ddlModalCategory" runat="server"
                                        CssClass="form-control form-control-sm">
                                        <asp:ListItem Value="EVENT">&Eacute;v&eacute;nement (SNEW, SOH...)</asp:ListItem>
                                        <asp:ListItem Value="DOCUMENT">Document (AMM, CMM, SB...)</asp:ListItem>
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
                                    placeholder="ex: Since New, Aircraft Maintenance Manual" />
                            </div>

                            <asp:Label ID="lblError" runat="server" Visible="false"
                                CssClass="alert alert-danger d-block py-2 px-3" />
                        </div>

                        <div class="modal-footer">
                            <asp:Button ID="btnSave" runat="server"
                                CssClass="btn btn-success" Text="Enregistrer" />
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">
                                Annuler
                            </button>
                        </div>

                    </div>
                </div>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
