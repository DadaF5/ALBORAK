<%@ Page Language="VB" AutoEventWireup="false" MasterPageFile="~/MRO2/mro2.master"
    CodeFile="SerializedItemList.aspx.vb" Inherits="MRO2_Setup_Components_SerializedItemList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Serialized Items
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>

            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        Serialized Items
                        <small class="text-muted">
                            (<asp:Literal ID="litRowCount" runat="server" Text="0" /> rows)
                        </small>
                    </h3>

                    <div class="card-tools">
                        <div class="input-group input-group-sm" style="width: 680px; display:inline-flex;">
                            <asp:TextBox ID="txtSearch" runat="server"
                                CssClass="form-control" placeholder="Search Serial, PN, nomenclature, status..." />
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
                    <asp:GridView ID="gvSI" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-sm table-hover"
                        GridLines="None"
                        DataKeyNames="SerializedItemId"
                        AllowPaging="true"
                        PageSize="25"
                        AllowSorting="true">
                        <Columns>
                            <asp:BoundField DataField="SerialNumber" HeaderText="Serial" SortExpression="SerialNumber" />
                            <asp:BoundField DataField="PN" HeaderText="PN" SortExpression="PN" />
                            <asp:BoundField DataField="Nomenclature" HeaderText="Nomenclature" SortExpression="Nomenclature" />
                            <asp:BoundField DataField="StatusCode" HeaderText="Status" SortExpression="StatusCode" />

                            <asp:BoundField DataField="ManufacturedDate" HeaderText="Mfg" DataFormatString="{0:yyyy-MM-dd}"
                                HtmlEncode="false" SortExpression="ManufacturedDate" />
                            <asp:BoundField DataField="ReceivedDate" HeaderText="Received" DataFormatString="{0:yyyy-MM-dd}"
                                HtmlEncode="false" SortExpression="ReceivedDate" />

                            <asp:CheckBoxField DataField="IsActive" HeaderText="Active" />

                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkEdit" runat="server"
                                        CommandName="EditRow"
                                        CommandArgument='<%# Eval("SerializedItemId") %>'
                                        CssClass="btn btn-xs btn-outline-primary"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i> Edit
                                    </asp:LinkButton>

                                    <asp:LinkButton ID="lnkToggle" runat="server"
                                        CommandName="ToggleActive"
                                        CommandArgument='<%# Eval("SerializedItemId") %>'
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

            <asp:HiddenField ID="hfSerializedItemId" runat="server" />

            <!-- Modal -->
            <div class="modal fade" id="siModal" tabindex="-1" role="dialog" aria-hidden="true">
                <div class="modal-dialog modal-lg" role="document">
                    <div class="modal-content">

                        <div class="modal-header">
                            <h5 class="modal-title">
                                <asp:Literal ID="litModalTitle" runat="server" Text="Edit Serialized Item" />
                            </h5>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>

                        <div class="modal-body">

                            <div class="form-row">
                                <div class="form-group col-md-4">
                                    <label>PN <span class="text-danger">*</span></label>
                                    <asp:DropDownList ID="ddlPN" runat="server" CssClass="form-control form-control-sm" />
                                </div>

                                <div class="form-group col-md-4">
                                    <label>Serial Number <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtSerial" runat="server"
                                        CssClass="form-control form-control-sm" MaxLength="80" />
                                </div>

                                <div class="form-group col-md-4">
                                    <label>Status</label>
                                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control form-control-sm" />
                                </div>
                            </div>

                            <div class="form-row">
                                <div class="form-group col-md-4">
                                    <label>Manufactured Date</label>
                                    <asp:TextBox ID="txtMfgDate" runat="server"
                                        CssClass="form-control form-control-sm"
                                        TextMode="SingleLine" />
                                </div>
                                <div class="form-group col-md-4">
                                    <label>Received Date</label>
                                    <asp:TextBox ID="txtRecvDate" runat="server"
                                        CssClass="form-control form-control-sm"
                                        TextMode="SingleLine" />
                                </div>
                            </div>

                            <div class="form-group">
                                <label>Notes</label>
                                <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" Rows="2"
                                    CssClass="form-control form-control-sm" MaxLength="300" />
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
  <!-- Use the same modal/backdrop cleanup script you used in ATAList (or move it into mro2.master globally) -->
</asp:Content>