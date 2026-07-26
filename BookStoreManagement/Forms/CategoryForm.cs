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
            this.Text = CategoryModel == null ? "Thêm mới Danh mục" : "Chỉnh Sửa Danh Mục";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitle);

            int yPos = 70;
            int spacing = 60;

            // Category Name
            Label lblName = new Label { Text = "Tên Danh mục *", Location = new Point(20, yPos), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtCategoryName = new Guna2TextBox { Location = new Point(20, yPos + 20), Width = 390, Height = 36, BorderRadius = 4 };
            this.Controls.Add(lblName);
            this.Controls.Add(txtCategoryName);
            yPos += spacing;

            // Description
            Label lblDesc = new Label { Text = "Mô tả", Location = new Point(20, yPos), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtDescription = new Guna2TextBox { Location = new Point(20, yPos + 20), Width = 390, Height = 36, BorderRadius = 4 };
            this.Controls.Add(lblDesc);
            this.Controls.Add(txtDescription);
            yPos += spacing;

            // Status
            chkIsActive = new Guna2CheckBox { Text = "Trạng thái", Location = new Point(20, yPos + 10), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            this.Controls.Add(chkIsActive);
            yPos += spacing;

            // Buttons
            btnCancel = new Guna2Button { Text = "Hủy bỏ", Location = new Point(190, yPos), Width = 100, Height = 40, BorderRadius = 4, FillColor = Color.Transparent, BorderThickness = 1, ForeColor = Color.Black, Cursor = Cursors.Hand };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            
            btnSave = new Guna2Button { Text = "Lưu thay đổi", Location = new Point(310, yPos), Width = 100, Height = 40, BorderRadius = 4, Cursor = Cursors.Hand };
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
                    MessageBox.Show("Tên danh mục không được để trống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (CategoryModel == null)
                {
                    var newCategory = new Category
                    {
                        CategoryName = txtCategoryName.Text,
                        Description = txtDescription.Text,
                        IsActive = chkIsActive.Checked,
                        CreatedAt = DateTime.Now
                    };
                    _service.Add(newCategory);
                    MessageBox.Show("Thêm danh mục thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    CategoryModel.CategoryName = txtCategoryName.Text;
                    CategoryModel.Description = txtDescription.Text;
                    CategoryModel.IsActive = chkIsActive.Checked;
                    CategoryModel.UpdatedAt = DateTime.Now;
                    
                    _service.Update(CategoryModel);
                    MessageBox.Show("Cập nhật danh mục thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            bool isDark = ThemeManager.IsDarkMode;
            this.BackColor = isDark ? Color.FromArgb(30, 30, 30) : Color.White;
            this.ForeColor = isDark ? Color.White : Color.Black;

            btnSave.FillColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;

            btnCancel.BorderColor = isDark ? Color.Gray : Color.LightGray;
            btnCancel.ForeColor = isDark ? Color.White : Color.Black;
        }
    }
}
