<%@ Page Language="VB" AutoEventWireup="false"
    MasterPageFile="~/MRO2/mro2.master"
    CodeFile="SerializedItemUninstall.aspx.vb"
    Inherits="MRO2_Operations_SerializedItemUninstall" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Uninstall Serialized Item
</asp:Content>

<asp:Content ID="cHead" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <li class="breadcrumb-item"><a href="<%= ResolveUrl("~/MRO2/") %>">MRO2</a></li>
    <li class="breadcrumb-item active">Uninstall to Workshop</li>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row">
        <div class="col-12">

            <div class="card card-warning card-outline">
                <div class="card-header">
                    <h3 class="card-title">
                        <i class="fas fa-wrench mr-1"></i>
                        Uninstall Serialized Item (to Workshop)
                    </h3>
                </div>

                <div class="card-body">

                    <asp:Label ID="lblMsg" runat="server" CssClass="text-danger" Visible="false" />

                    <div class="form-row">
                        <div class="form-group col-md-4">
                            <label>PN</label>
                            <asp:DropDownList ID="ddlPN" runat="server"
                                CssClass="form-control form-control-sm"
                                AutoPostBack="true" />
                        </div>

                        <div class="form-group col-md-4">
                            <label>Serial Number</label>
                            <asp:TextBox ID="txtSerial" runat="server"
                                CssClass="form-control form-control-sm" />
                        </div>

                        <div class="form-group col-md-4">
                            <label>Uninstall Date</label>
                            <asp:TextBox ID="txtUninstallDate" runat="server"
                                CssClass="form-control form-control-sm"
                                placeholder="YYYY-MM-DD" />
                            <small class="form-text text-muted">
                                Effective date (WO/TO). Event time is stored in UTC.
                            </small>
                        </div>
                    </div>

                    <hr />

                    <div class="form-row">
                        <div class="form-group col-md-6">
                            <label>Workshop</label>
                            <asp:DropDownList ID="ddlWorkshop" runat="server"
                                CssClass="form-control form-control-sm" />
                        </div>
                    </div>

                    <hr />

                    <div class="form-row">
                        <div class="form-group col-md-3">
                            <label>Work Order No</label>
                            <asp:TextBox ID="txtWO" runat="server" CssClass="form-control form-control-sm" />
                        </div>

                        <div class="form-group col-md-3">
                            <label>Task Card No</label>
                            <asp:TextBox ID="txtTC" runat="server" CssClass="form-control form-control-sm" />
                        </div>

                        <div class="form-group col-md-3">
                            <label>Station</label>
                            <asp:TextBox ID="txtStation" runat="server" CssClass="form-control form-control-sm" />
                        </div>

                        <div class="form-group col-md-3">
                            <label>Certifying Staff</label>
                            <asp:TextBox ID="txtCert" runat="server" CssClass="form-control form-control-sm" />
                        </div>
                    </div>

                    <div class="form-row">
                        <div class="form-group col-md-12">
                            <label>Notes</label>
                            <asp:TextBox ID="txtNotes" runat="server"
                                CssClass="form-control form-control-sm"
                                TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>

                </div>

                <div class="card-footer">
                    <asp:Button ID="btnUninstall" runat="server"
                        Text="Uninstall to Workshop"
                        CssClass="btn btn-warning btn-sm" />
                </div>

            </div>

        </div>
    </div>

</asp:Content>

<asp:Content ID="cFooterScripts" ContentPlaceHolderID="FooterScripts" runat="server">
</asp:Content>