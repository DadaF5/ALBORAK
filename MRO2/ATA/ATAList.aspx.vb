Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

Partial Class MRO2_Setup_ATA_ATAList
    Inherits System.Web.UI.Page

    Private ReadOnly ConnStr As String =
        ConfigurationManager.ConnectionStrings("2FRAConString").ConnectionString
    Private Property SortColumn As String
        Get
            Return If(TryCast(ViewState("SortColumn"), String), "ATACode")
        End Get
        Set(value As String)
            ViewState("SortColumn") = value
        End Set
    End Property

    Private Property SortDir As String
        Get
            Return If(TryCast(ViewState("SortDir"), String), "ASC")
        End Get
        Set(value As String)
            ViewState("SortDir") = value
        End Set
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            SortColumn = "ATACode"
            SortDir = "ASC"
            BindGrid()
        End If
    End Sub

    Protected Sub chkIncludeInactive_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles chkIncludeInactive.CheckedChanged
        gvATA.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub btnSearch_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSearch.Click
        gvATA.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub btnClear_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnClear.Click
        txtSearch.Text = ""
        gvATA.PageIndex = 0
        BindGrid()
    End Sub

    Protected Sub gvATA_PageIndexChanging(ByVal sender As Object, ByVal e As GridViewPageEventArgs) Handles gvATA.PageIndexChanging
        gvATA.PageIndex = e.NewPageIndex
        BindGrid()
    End Sub
    Protected Sub gvATA_Sorting(ByVal sender As Object, ByVal e As GridViewSortEventArgs) Handles gvATA.Sorting
        If SortColumn = e.SortExpression Then
            SortDir = If(SortDir = "ASC", "DESC", "ASC")
        Else
            SortColumn = e.SortExpression
            SortDir = "ASC"
        End If

        gvATA.PageIndex = 0
        BindGrid()
    End Sub
    Protected Sub btnNew_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnNew.Click
        hfATAId.Value = ""
        txtATACode.Text = ""
        txtTitle.Text = ""
        lblError.Visible = False
        litModalTitle.Text = "New ATA"

        ShowModal()
    End Sub

    Protected Sub gvATA_RowCommand(ByVal sender As Object, ByVal e As GridViewCommandEventArgs) Handles gvATA.RowCommand
        If e.CommandName = "EditRow" Then
            Dim ataId As Integer = Convert.ToInt32(e.CommandArgument)
            LoadRowForEdit(ataId)
            lblError.Visible = False
            litModalTitle.Text = "Edit ATA"
            ShowModal()
            Return
        End If

        If e.CommandName = "ToggleActive" Then
            ToggleActive(Convert.ToInt32(e.CommandArgument))
            BindGrid()
            Return
        End If
    End Sub
    Private Sub ShowToast(ByVal message As String, ByVal kind As String)
        ' kind: success | info | warning | error
        Dim k As String = (If(kind, "info")).ToLowerInvariant()
        If k <> "success" AndAlso k <> "info" AndAlso k <> "warning" AndAlso k <> "error" Then
            k = "info"
        End If

        Dim ser As New JavaScriptSerializer()
        Dim msgJs As String = ser.Serialize(If(message, "")) ' produces a safe JS string including newlines

        Dim js As String =
            "if (window.toastr) { toastr." & k & "(" & msgJs & "); }"

        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "toast_" & Guid.NewGuid().ToString("N"),
            js,
            True)
    End Sub
    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click

        lblError.Visible = False

        ' UI validation + normalization
        Dim code As String = txtATACode.Text.Trim().ToUpperInvariant()
        Dim title As String = txtTitle.Text.Trim()

        If code = "" Then
            lblError.Text = "ATA Code is required."
            lblError.Visible = True
            ShowModal()
            Exit Sub
        End If

        txtATACode.Text = code ' reflect normalization back to user

        Try
            Using cn As New SqlConnection(ConnStr)
                Using cmd As New SqlCommand("mro2.usp_ATA_Save", cn)
                    cmd.CommandType = CommandType.StoredProcedure

                    Dim ataIdObj As Object = DBNull.Value
                    If hfATAId.Value.Trim() <> "" Then ataIdObj = Convert.ToInt32(hfATAId.Value)

                    cmd.Parameters.AddWithValue("@ATAId", ataIdObj)
                    cmd.Parameters.AddWithValue("@ATACode", code)
                    cmd.Parameters.AddWithValue("@Title", If(txtTitle.Text.Trim() = "", CType(DBNull.Value, Object), txtTitle.Text.Trim()))

                    cn.Open()
                    cmd.ExecuteScalar()
                End Using
            End Using

            BindGrid()
            HideModal()
            ShowToast("Saved successfully.", "success")
        Catch ex As Exception
            lblError.Text = FriendlyDbMessage(ex)
            lblError.Visible = True
            ShowModal()
        End Try
    End Sub
    Private Function FriendlyDbMessage(ByVal ex As Exception) As String
        ' Unwrap SQL exception if present
        Dim msg As String = ex.Message

        ' Your RAISERROR messages are already friendly; keep them.
        ' But still map common duplicates if you later enforce a unique index.
        Dim sqlEx As SqlException = TryCast(ex, SqlException)
        If sqlEx IsNot Nothing Then
            If sqlEx.Number = 2627 OrElse sqlEx.Number = 2601 Then
                Return "This ATA Code already exists."
            End If
        End If

        Return Server.HtmlEncode(msg)
    End Function
    Private Sub BindGrid()
        Dim dt As New DataTable()

        Using cn As New SqlConnection(ConnStr)
            Using cmd As New SqlCommand("mro2.usp_ATA_List", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@IncludeInactive", If(chkIncludeInactive.Checked, 1, 0))
                cmd.Parameters.AddWithValue("@Search", If(txtSearch.Text.Trim() = "", CType(DBNull.Value, Object), txtSearch.Text.Trim()))
                cmd.Parameters.AddWithValue("@SortColumn", SortColumn)
                cmd.Parameters.AddWithValue("@SortDir", SortDir)

                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using
        litRowCount.Text = dt.Rows.Count.ToString()
        gvATA.DataSource = dt
        gvATA.DataBind()
    End Sub

    Private Sub LoadRowForEdit(ByVal ataId As Integer)
        Using cn As New SqlConnection(ConnStr)
            cn.Open()
            Using cmd As New SqlCommand("SELECT ATAId, ATACode, Title FROM mro2.ATA WHERE ATAId=@ATAId", cn)
                cmd.Parameters.AddWithValue("@ATAId", ataId)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        hfATAId.Value = rdr("ATAId").ToString()
                        txtATACode.Text = rdr("ATACode").ToString()
                        txtTitle.Text = If(rdr("Title") Is DBNull.Value, "", rdr("Title").ToString())
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub ToggleActive(ByVal ataId As Integer)
        Try
            Dim currentActive As Boolean = True

            Using cn As New SqlConnection(ConnStr)
                cn.Open()

                Using cmdGet As New SqlCommand("SELECT IsActive FROM mro2.ATA WHERE ATAId=@ATAId", cn)
                    cmdGet.Parameters.AddWithValue("@ATAId", ataId)
                    Dim o As Object = cmdGet.ExecuteScalar()
                    If o IsNot Nothing AndAlso o IsNot DBNull.Value Then currentActive = Convert.ToBoolean(o)
                End Using

                Using cmd As New SqlCommand("mro2.usp_ATA_SetActive", cn)
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("@ATAId", ataId)
                    cmd.Parameters.AddWithValue("@IsActive", If(currentActive, 0, 1))
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            ShowToast("Status updated.", "success")

        Catch ex As Exception
            ShowToast(FriendlyDbMessage(ex), "error")
        End Try
    End Sub

    Private Sub ShowModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "showAtaModal_" & Guid.NewGuid().ToString("N"),
            "$('#ataModal').modal('show');",
            True)
    End Sub

    Private Sub HideModal()
        ScriptManager.RegisterStartupScript(up1, up1.GetType(),
            "hideAtaModal_" & Guid.NewGuid().ToString("N"),
            "$('#ataModal').modal('hide');",
            True)
    End Sub
End Class