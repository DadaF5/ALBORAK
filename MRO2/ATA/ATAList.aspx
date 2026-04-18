<%@ Page Language="VB" AutoEventWireup="false" MasterPageFile="../mro2.master"
    CodeFile="ATAList.aspx.vb" Inherits="MRO2_Setup_ATA_ATAList" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    ATA Chapters
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>

            <div class="card card-outline card-primary">
                <div class="card-header">
                    <h3 class="card-title">
                        ATA Chapters
                        <small class="text-muted">
                            (<asp:Literal ID="litRowCount" runat="server" Text="0" /> rows)
                        </small>
                    </h3>

                    <div class="card-tools">

                        <div class="input-group input-group-sm" style="width: 420px; display:inline-flex;">
                            <asp:TextBox ID="txtSearch" runat="server"
                                CssClass="form-control" placeholder="Search ATA code or title..." />
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

                    <asp:GridView ID="gvATA" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-sm table-hover"
                        GridLines="None"
                        DataKeyNames="ATAId" OnRowCommand="gvATA_RowCommand"
                        AllowPaging="true"
                        PageSize="25"
                        AllowSorting="true">
                        <Columns>
                            <asp:BoundField DataField="ATACode" HeaderText="ATA Code" SortExpression="ATACode" />
                            <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                            <asp:CheckBoxField DataField="IsActive" HeaderText="Active" />

                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkEdit" runat="server"
                                        CommandName="EditRow"
                                        CommandArgument='<%# Eval("ATAId") %>'
                                        CssClass="btn btn-xs btn-outline-primary"
                                        CausesValidation="false">
                                        <i class="fas fa-edit"></i> Edit
                                    </asp:LinkButton>

                                    <asp:LinkButton ID="lnkToggle" runat="server"
                                        CommandName="ToggleActive"
                                        CommandArgument='<%# Eval("ATAId") %>'
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

                        <PagerStyle CssClass="pagination-ys" />
                    </asp:GridView>

                </div>
            </div>

            <asp:HiddenField ID="hfATAId" runat="server" />

            <%-- Modal (same as you already have) --%>
            <div class="modal fade" id="ataModal" tabindex="-1" role="dialog" aria-hidden="true">
                <div class="modal-dialog modal-lg" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">
                                <asp:Literal ID="litModalTitle" runat="server" Text="Edit ATA" />
                            </h5>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>

                        <div class="modal-body">
                            <div class="form-row">
                                <div class="form-group col-md-3">
                                    <label>ATA Code</label>
                                    <asp:TextBox ID="txtATACode" runat="server" CssClass="form-control form-control-sm" />
                                </div>
                                <div class="form-group col-md-9">
                                    <label>Title</label>
                                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control form-control-sm" />
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
  <script type="text/javascript">
      (function () {

          function cleanupModalArtifacts() {
              try {
                  // Use vanilla JS so it works even if jQuery is missing
                  var backdrops = document.querySelectorAll('.modal-backdrop');
                  for (var i = 0; i < backdrops.length; i++) {
                      backdrops[i].parentNode.removeChild(backdrops[i]);
                  }
                  document.body.classList.remove('modal-open');
                  document.body.style.paddingRight = '';
              } catch (e) { }
          }

          function wireOnceWhenJqueryReady() {
              if (!window.jQuery) return false;

              // Wire hidden event once
              if (!window.__ataModalHiddenWired) {
                  window.__ataModalHiddenWired = true;
                  jQuery(document).on('hidden.bs.modal', '#ataModal', function () {
                      cleanupModalArtifacts();
                  });
              }

              // Wire UpdatePanel endRequest once
              try {
                  if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                      var prm = Sys.WebForms.PageRequestManager.getInstance();
                      if (prm && !prm.__ataCleanupWired) {
                          prm.add_endRequest(function () {
                              // If modal is not visible, ensure no stuck overlay remains
                              if (!jQuery('#ataModal').hasClass('show')) {
                                  cleanupModalArtifacts();
                              }
                          });
                          prm.__ataCleanupWired = true;
                      }
                  }
              } catch (e) { }

              return true;
          }

          // Attempt immediately, then retry a few times (handles load order)
          if (!wireOnceWhenJqueryReady()) {
              var tries = 0;
              var t = window.setInterval(function () {
                  tries++;
                  if (wireOnceWhenJqueryReady() || tries > 40) { // ~4 seconds max
                      window.clearInterval(t);
                  }
              }, 100);
          }

          // Expose manual recovery (optional)
          window.cleanupAtaModalArtifacts = cleanupModalArtifacts;

      })();
  </script>
</asp:Content>
