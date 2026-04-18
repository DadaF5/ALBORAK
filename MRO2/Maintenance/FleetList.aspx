<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="FleetList.aspx.vb"
    Inherits="MRO2_Maintenance_FleetList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Flotte — MRO2
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item">
        <a href="<%= ResolveUrl("~/MRO2/Default.aspx") %>">MRO2</a>
    </li>
    <li class="breadcrumb-item active">Flotte</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional"
                     ChildrenAsTriggers="true">
        <ContentTemplate>

            <%-- ═══ FLEET HEALTH SUMMARY STRIP ═══ --%>
            <div class="row mb-3">
                <div class="col-sm-3 col-6">
                    <div class="info-box shadow-sm">
                        <span class="info-box-icon bg-danger elevation-1">
                            <i class="fas fa-skull-crossbones"></i>
                        </span>
                        <div class="info-box-content">
                            <span class="info-box-text">Expir&eacute;s</span>
                            <span class="info-box-number">
                                <asp:Literal ID="litTotalExpired" runat="server" Text="0" />
                            </span>
                        </div>
                    </div>
                </div>
                <div class="col-sm-3 col-6">
                    <div class="info-box shadow-sm">
                        <span class="info-box-icon bg-warning elevation-1">
                            <i class="fas fa-exclamation-triangle"></i>
                        </span>
                        <div class="info-box-content">
                            <span class="info-box-text">&Agrave; faire</span>
                            <span class="info-box-number">
                                <asp:Literal ID="litTotalDue" runat="server" Text="0" />
                            </span>
                        </div>
                    </div>
                </div>
                <div class="col-sm-3 col-6">
                    <div class="info-box shadow-sm">
                        <span class="info-box-icon bg-info elevation-1">
                            <i class="fas fa-bell"></i>
                        </span>
                        <div class="info-box-content">
                            <span class="info-box-text">Alertes</span>
                            <span class="info-box-number">
                                <asp:Literal ID="litTotalAlert" runat="server" Text="0" />
                            </span>
                        </div>
                    </div>
                </div>
                <div class="col-sm-3 col-6">
                    <div class="info-box shadow-sm">
                        <span class="info-box-icon bg-success elevation-1">
                            <i class="fas fa-plane"></i>
                        </span>
                        <div class="info-box-content">
                            <span class="info-box-text">A&eacute;ronefs suivis</span>
                            <span class="info-box-number">
                                <asp:Literal ID="litTotalAc" runat="server" Text="0" />
                            </span>
                        </div>
                    </div>
                </div>
            </div>

            <%-- ═══ FLEET GRID ═══ --%>
            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        <i class="fas fa-plane mr-1"></i>
                        &Eacute;tat de la Flotte
                        <small class="text-muted ml-1">
                            (<asp:Literal ID="litRowCount" runat="server" Text="0" />
                            a&eacute;ronefs)
                        </small>
                    </h3>
                    <div class="card-tools d-flex align-items-center">
                        <%-- Filter by AcMainGroup --%>
                        <label class="mb-0 mr-2 text-muted small">
                            Groupe&nbsp;:
                        </label>
                        <asp:DropDownList ID="ddlFilterGroup" runat="server"
                            CssClass="form-control form-control-sm mr-3"
                            AutoPostBack="true"
                            style="width:180px;"
                            OnSelectedIndexChanged="ddlFilterGroup_Changed" />
                        <%-- Show only aircraft with issues --%>
                        <asp:CheckBox ID="chkIssuesOnly" runat="server"
                            AutoPostBack="true" CssClass="mr-2"
                            Text="Probl&egrave;mes uniquement" />
                    </div>
                </div>

                <div class="card-body p-0">
                    <asp:GridView ID="gvFleet" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-hover mb-0"
                        GridLines="None"
                        DataKeyNames="AcID"
                        AllowSorting="true">
                        <Columns>

                            <%-- Overall health badge --%>
                            <asp:TemplateField HeaderText="Sant&eacute;"
                                SortExpression="AircraftHealth"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# HealthBadge(Eval("AircraftHealth").ToString()) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- TailNo --%>
                                <asp:TemplateField HeaderText="Avion"
                                SortExpression="TailNo">
                                <ItemTemplate>
                                    <span class="font-weight-bold">
                                        <%# Eval("TailNo") %>
                                    </span>
                                    <br />
                                    <small class="text-muted">
                                        <%# Eval("AcTypeName") %>
                                    </small>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- AcGroup --%>
                            <asp:BoundField DataField="AcMainGroupName"
                                HeaderText="Groupe"
                                SortExpression="AcMainGroupName" />

                            <%-- FH / FC --%>
                            <asp:TemplateField HeaderText="HV / Cycles"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <span class="font-weight-bold text-primary">
                                        <%# Eval("FH_Display") %>
                                        <small class="text-muted font-weight-normal">hrs</small>
                                    </span>
                                    <br />
                                    <small class="text-muted">
                                        <%# Eval("FC_Display") %> cycles
                                    </small>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- Expired count --%>
                            <asp:TemplateField HeaderText="Expir&eacute;s"
                                SortExpression="Expired"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# CountCell(Eval("Expired"), "danger") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- Due count --%>
                            <asp:TemplateField HeaderText="&Agrave; faire"
                                SortExpression="Due"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# CountCell(Eval("Due"), "warning") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- Alert count --%>
                            <asp:TemplateField HeaderText="Alertes"
                                SortExpression="Alert"
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <%# CountCell(Eval("Alert"), "info") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- Action --%>
                            <asp:TemplateField HeaderText=""
                                HeaderStyle-CssClass="text-center"
                                ItemStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <a href='<%= ResolveUrl("~/MRO2/Maintenance/AircraftConfiguration.aspx") %>?AcID=<%# Eval("AcID") %>'
                                       class="btn btn-sm btn-primary">
                                        <i class="fas fa-sitemap mr-1"></i>
                                        Configuration
                                    </a>
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                        <HeaderStyle CssClass="bg-primary text-white" />
                        <RowStyle CssClass="align-middle" />
                        <EmptyDataTemplate>
                            <div class="text-center text-muted py-5">
                                <i class="fas fa-plane fa-2x mb-2"></i><br />
                                Aucun a&eacute;ronef trouv&eacute;.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
