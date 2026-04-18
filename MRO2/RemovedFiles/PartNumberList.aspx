<%@ Page Language="VB" AutoEventWireup="false" MasterPageFile="~/MRO2/mro2.master"
    CodeFile="PartNumberList.aspx.vb" Inherits="MRO2_Setup_Components_PartNumberList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Part Numbers
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>

            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        Part Numbers
                        <small class="text-muted">
                            (<asp:Literal ID="litRowCount" runat="server" Text="0" /> rows)
                        </small>
                    </h3>

                    <div class="card-tools">
                        <div class="input-group input-group-sm" style="width: 620px; display:inline-flex;">
                            <asp:TextBox ID="txtSearch" runat="server"
                                CssClass="form-control" placeholder="Search PN, nomenclature, ATA..." />
                            <div class="input-group-append">
                                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" />
                                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" />
                            </div>
                        </div>

                        &nbsp;&nbsp;

                        <asp:CheckBox ID="chkIncludeInactive" runat="server" AutoPostBack="true"
                            Text="Include inactive" />
                        &nbsp;

                        <asp:Button ID="btnNew" runat="server" CssClass="btn btn-sm btn-success" Text="+ New" />
                    </div>
                </div>

                <div class="card-body">
                    <asp:GridView ID="gvPN" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-sm table-hover"
                        GridLines="None"
                        DataKeyNames="PartNumberId"
                        AllowPaging="true"
                        PageSize="25"
                        AllowSorting="true">
                        <Columns>
                            <asp:BoundField DataField="PN" HeaderText="PN" SortExpression="PN" />
                            <asp:BoundField DataField="Nomenclature" HeaderText="Nomenclature" SortExpression="Nomenclature" />
                            <asp:BoundField DataField="ATACode" HeaderText="ATA" SortExpression="ATACode" />

                            <asp:BoundField DataField="UOMCode" HeaderText="UOM" />

                            <asp:TemplateField HeaderText="Serialized" SortExpression="IsSerialized">
                                <ItemTemplate>
                                    <%# If(Convert.ToBoolean(Eval("IsSerialized")), "Yes", "No") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:CheckBoxField DataField="IsActive" HeaderText="Active" />

                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkEdit" runat="server"
                                        CommandName="EditRow"
                                        CommandArgument='<%# Eval("PartNumberId") %>'
                                        CssClass="btn btn-xs btn-outline-primary"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i> Edit
                                    </asp:LinkButton>

                                    <asp:LinkButton ID="lnkToggle" runat="server"
                                        CommandName="ToggleActive"
                                        CommandArgument='<%# Eval("PartNumberId") %>'
                                        CssClass='<%# If(Convert.ToBoolean(Eval("IsActive")),
                                                        "btn btn-xs btn-outline-danger ml-1",
                                                        "btn btn-xs btn-outline-success ml-1") %>'
                                        CausesValidation="false"
                                        OnClientClick="return confirm('Are you sure?');">
                                        <%# If(Convert.ToBoolean(Eval("IsActive")), "Deactivate", "Activate") %>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

            <asp:HiddenField ID="hfPartNumberId" runat="server" />

            <!-- Modal -->
            <div class="modal fade" id="pnModal" tabindex="-1" role="dialog" aria-hidden="true">
                <div class="modal-dialog modal-lg" role="document">
                    <div class="modal-content">

                        <div class="modal-header">
                            <h5 class="modal-title">
                                <asp:Literal ID="litModalTitle" runat="server" Text="Edit Part Number" />
                            </h5>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>

                        <div class="modal-body">

                            <div class="form-row">
                                <div class="form-group col-md-4">
                                    <label>PN <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtPN" runat="server"
                                        CssClass="form-control form-control-sm" MaxLength="60" />
                                </div>

                                <div class="form-group col-md-8">
                                    <label>Nomenclature</label>
                                    <asp:TextBox ID="txtNomenclature" runat="server"
                                        CssClass="form-control form-control-sm" MaxLength="200" />
                                </div>
                                <asp:Panel ID="pnlAcMainGroup" runat="server">
                                    <div class="form-group">
                                        <label>AC Main Group <span class="text-danger">*</span></label>
                                        <asp:DropDownList ID="ddlAcMainGroup" runat="server" CssClass="form-control form-control-sm" />
                                        <small class="form-text text-muted">Required for serialized part numbers (used to list workshops and positions).
                                        </small>
                                    </div>
                                </asp:Panel>
                            </div>

                            <div class="form-row">
                                <div class="form-group col-md-4">
                                    <label>ATA</label>
                                    <asp:DropDownList ID="ddlATA" runat="server" CssClass="form-control form-control-sm" />
                                </div>

                                <div class="form-group col-md-4">
                                    <label>UOM <span class="text-danger">*</span></label>
                                    <asp:DropDownList ID="ddlUOM" runat="server" CssClass="form-control form-control-sm" />
                                </div>

                                <div class="form-group col-md-4">
                                    <label>Serialized</label>
                                    <asp:DropDownList ID="ddlIsSerialized" runat="server" CssClass="form-control form-control-sm">
                                        <asp:ListItem Text="Yes" Value="1" Selected="True" />
                                        <asp:ListItem Text="No" Value="0" />
                                    </asp:DropDownList>
                                </div>
                            </div>

                            <asp:Label ID="lblError" runat="server" Visible="false"
                                CssClass="alert alert-danger py-2 px-3" />
                        </div>

                        <div class="modal-footer">
                            <asp:Button ID="btnSave" runat="server" CssClass="btn btn-success" Text="Save" />
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                        </div>

                    </div>
                </div>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>

<asp:Content ID="cFooter" ContentPlaceHolderID="FooterScripts" runat="server">
  <!-- Use your existing ATAList modal/backdrop cleanup script here OR move it into mro2.master globally -->
</asp:Content>