using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class CategoryForm : Form
    {
        private CategoryService _service;
        
        private Guna2TextBox txtCategoryName;
        private Guna2TextBox txtDescription;
        private Guna2CheckBox chkIsActive;
        private Guna2Button btnSave;
        private Guna2Button btnCancel;

        public Category? CategoryModel { get; private set; }

        public CategoryForm(Category? categoryToEdit = null)
        {
            _service = new CategoryService();
            CategoryModel = categoryToEdit;
            
            InitializeComponent();
            
            if (CategoryModel != null)
            {
                BindData();
            }
            
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = CategoryModel == null ? "Thêm mới Danh mục" : "Chỉnh sửa Danh mục";
            this.Size = new Size(500, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ControlBox = false;
            this.MinimizeBox = false;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 18F, FontStyle.Bold), Location = new Point(24, 20), AutoSize = true };
            this.Controls.Add(lblTitle);

            int yPos = 70;

            // Category Name
            Label lblName = new Label { Text = "Tên danh mục (*)", Location = new Point(24, yPos), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtCategoryName = new Guna2TextBox { Location = new Point(24, yPos + 25), Width = 436, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.Add(lblName);
            this.Controls.Add(txtCategoryName);
            yPos += 75;

            // Description
            Label lblDesc = new Label { Text = "Mô tả", Location = new Point(24, yPos), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtDescription = new Guna2TextBox { Location = new Point(24, yPos + 25), Width = 436, Height = 60, Font = new Font("Segoe UI", 10F), Multiline = true, BorderRadius = 4 };
            this.Controls.Add(lblDesc);
            this.Controls.Add(txtDescription);
            yPos += 100;

            // Status
            chkIsActive = new Guna2CheckBox { Text = "Đang hoạt động", Location = new Point(24, yPos), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(chkIsActive);
            yPos += 50;

            // Buttons
            btnCancel = new Guna2Button { Text = "Hủy bỏ", Location = new Point(this.Width - 140, this.Height - 100), Width = 100, Height = 45, BorderRadius = 8, FillColor = Color.Transparent, BorderThickness = 1, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            
            btnSave = new Guna2Button { Text = "Lưu thay đổi", Location = new Point(this.Width - 280, this.Height - 100), Width = 130, Height = 45, BorderRadius = 8, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.Click += BtnSave_Click;

            this.Controls.Add(btnCancel);
            this.Controls.Add(btnSave);
        }

        private void BindData()
        {
            if (CategoryModel != null)
            {
                txtCategoryName.Text = CategoryModel.CategoryName;
                txtDescription.Text = CategoryModel.Description;
                chkIsActive.Checked = CategoryModel.IsActive;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
                {
                    MessageBox.Show("Tên danh mục không được để trống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (CategoryModel == null)
                {
                    var newCategory = new Category
                    {
                        CategoryName = txtCategoryName.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        IsActive = chkIsActive.Checked,
                        CreatedAt = DateTime.Now
                    };
                    _service.Add(newCategory);
                    MessageBox.Show("Thêm danh mục thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    CategoryModel.CategoryName = txtCategoryName.Text.Trim();
                    CategoryModel.Description = txtDescription.Text.Trim();
                    CategoryModel.IsActive = chkIsActive.Checked;
                    CategoryModel.UpdatedAt = DateTime.Now;
                    
                    _service.Update(CategoryModel);
                    MessageBox.Show("Cập nhật danh mục thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;

            foreach (Control control in this.Controls)
            {
                if (control is Label lbl)
                {
                    lbl.ForeColor = ThemeManager.TextPrimary;
                }
                else if (control is Guna2TextBox txt)
                {
                    txt.FillColor = ThemeManager.TextBoxBackground;
                    txt.ForeColor = ThemeManager.TextPrimary;
                    txt.BorderColor = ThemeManager.TextBoxBorder;
                    txt.FocusedState.BorderColor = ThemeManager.ButtonFill;
                }
                else if (control is Guna2CheckBox chk)
                {
                    chk.ForeColor = ThemeManager.TextPrimary;
                }
            }

            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.BorderColor = ThemeManager.TextBoxBorder;

            btnSave.FillColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;
        }
    }
}
