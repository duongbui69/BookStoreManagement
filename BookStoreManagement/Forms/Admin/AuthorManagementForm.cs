using System;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;

namespace BookStoreManagement.Forms.Admin
{
    public class AuthorManagementForm : AdminFormBase
    {
        private readonly AuthorService _service = new AuthorService();
        private DataGridView dgv = null!;
        private TextBox txtName = null!;
        private TextBox txtDescription = null!;
        private TextBox txtSearch = null!;
        private CheckBox chkActive = null!;
        private int selectedId;

        public AuthorManagementForm()
        {
            Text = "Quản lý tác giả";
            Width = 900; Height = 600;
            InitializeComponent();
            Load += (s, e) => LoadData();
        }

        private void InitializeComponent()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 150 };
            dgv = CreateGrid();
            txtName = CreateTextBox(top, "Tên tác giả", 20, 20);
            txtDescription = CreateTextBox(top, "Mô tả", 20, 55, 520);
            chkActive = CreateCheckBox(top, "Đang hoạt động", 150, 90);
            txtSearch = CreateTextBox(top, "Tìm kiếm", 470, 20, 220);
            CreateButton(top, "Thêm", 20, 110, BtnAdd_Click);
            CreateButton(top, "Sửa", 130, 110, BtnUpdate_Click);
            CreateButton(top, "Khóa/Mở", 240, 110, BtnToggle_Click);
            CreateButton(top, "Tìm", 715, 18, BtnSearch_Click, 80);
            CreateButton(top, "Tải lại", 715, 55, (s, e) => LoadData(), 80);
            dgv.CellClick += Dgv_CellClick;
            Controls.Add(dgv); Controls.Add(top);
        }

        private void LoadData() { try { dgv.DataSource = _service.GetAll(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
        private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            selectedId = GetSelectedId(dgv);
            txtName.Text = dgv.CurrentRow!.Cells["AuthorName"].Value?.ToString();
            txtDescription.Text = dgv.CurrentRow.Cells["Description"].Value?.ToString();
            chkActive.Checked = Convert.ToBoolean(dgv.CurrentRow.Cells["IsActive"].Value);
        }
        private void BtnAdd_Click(object? sender, EventArgs e) { try { _service.Add(new Author { AuthorName = txtName.Text, Description = txtDescription.Text }); MessageBox.Show("Thêm thành công."); LoadData(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
        private void BtnUpdate_Click(object? sender, EventArgs e) { try { if (selectedId <= 0) { MessageBox.Show("Chọn tác giả."); return; } _service.Update(new Author { Id = selectedId, AuthorName = txtName.Text, Description = txtDescription.Text, IsActive = chkActive.Checked }); MessageBox.Show("Cập nhật thành công."); LoadData(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
        private void BtnToggle_Click(object? sender, EventArgs e) { try { if (selectedId <= 0) { MessageBox.Show("Chọn tác giả."); return; } _service.SetActive(selectedId, !chkActive.Checked); MessageBox.Show("Cập nhật trạng thái thành công."); LoadData(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
        private void BtnSearch_Click(object? sender, EventArgs e) { try { dgv.DataSource = _service.Search(txtSearch.Text); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
    }
}
