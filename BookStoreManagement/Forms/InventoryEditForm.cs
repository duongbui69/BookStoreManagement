using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class InventoryEditForm : Form
    {
        private InventoryService _inventoryService;
        private BookService _bookService;
        
        private int? _bookId;
        private string? _warehouse;
        
        private Label lblTitle;
        private Guna2ComboBox cbBook, cbWarehouse;
        private Guna2TextBox txtCurrentStock, txtMinStock;
        private Guna2Button btnSave, btnCancel;

        public InventoryEditForm(int? bookId = null, string? warehouse = null, int currentStock = 0, int minStock = 0)
        {
            _inventoryService = new InventoryService();
            _bookService = new BookService();
            _bookId = bookId;
            _warehouse = warehouse;
            
            InitializeComponent();
            LoadBooks();
            ApplyTheme();
            
            if (_bookId.HasValue)
            {
                cbBook.SelectedValue = _bookId.Value;
                cbBook.Enabled = false;
                cbWarehouse.SelectedItem = _warehouse;
                cbWarehouse.Enabled = false;
                txtCurrentStock.Text = currentStock.ToString();
                txtMinStock.Text = minStock.ToString();
                lblTitle.Text = "Cập nhật Kho";
                this.Text = "Cập nhật Kho";
            }
            else
            {
                lblTitle.Text = "Thêm mới Kho";
                this.Text = "Thêm mới Kho";
            }
            
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label { Location = new Point(24, 20), AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold) };
            this.Controls.Add(lblTitle);

            int startY = 70;

            Label lblBook = new Label { Text = "Sản phẩm (*)", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbBook = new Guna2ComboBox { Location = new Point(24, startY + 25), Width = 436, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lblBook, cbBook });
            startY += 75;

            Label lblWarehouse = new Label { Text = "Kho hàng (*)", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbWarehouse = new Guna2ComboBox { Location = new Point(24, startY + 25), Width = 436, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            cbWarehouse.Items.AddRange(new object[] { "Kho Tổng (Hà Nội)", "Kho Chi Nhánh (HCM)", "Kho Miền Trung" });
            cbWarehouse.SelectedIndex = 0;
            this.Controls.AddRange(new Control[] { lblWarehouse, cbWarehouse });
            startY += 75;

            txtCurrentStock = CreateInput("Tồn kho hiện tại", ref startY);
            txtMinStock = CreateInput("Tồn kho tối thiểu", ref startY);

            btnCancel = new Guna2Button
            {
                Text = "Hủy bỏ",
                Size = new Size(100, 45),
                Location = new Point(this.Width - 140, this.Height - 100),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FillColor = Color.Transparent,
                BorderThickness = 1
            };
            btnCancel.Click += (s, e) => this.Close();

            btnSave = new Guna2Button
            {
                Text = "Lưu thay đổi",
                Size = new Size(130, 45),
                Location = new Point(this.Width - 280, this.Height - 100),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private Guna2TextBox CreateInput(string label, ref int y)
        {
            Label lbl = new Label { Text = label, Location = new Point(24, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            Guna2TextBox txt = new Guna2TextBox { Location = new Point(24, y + 25), Width = 436, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lbl, txt });
            y += 75;
            return txt;
        }

        private void LoadBooks()
        {
            var books = _bookService.GetAll();
            cbBook.DataSource = books;
            cbBook.DisplayMember = "Title"; // Assuming Title is the property name, adjust if necessary
            cbBook.ValueMember = "Id";
        }
        
        private void ThemeManager_ThemeChanged(object? sender, EventArgs e) => ApplyTheme();

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            lblTitle.ForeColor = ThemeManager.TextPrimary;

            foreach (Control control in this.Controls)
            {
                if (control is Label lbl && lbl != lblTitle)
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
                else if (control is Guna2ComboBox cb)
                {
                    cb.FillColor = ThemeManager.TextBoxBackground;
                    cb.ForeColor = ThemeManager.TextPrimary;
                    cb.BorderColor = ThemeManager.TextBoxBorder;
                }
            }

            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.BorderColor = ThemeManager.TextBoxBorder;

            btnSave.FillColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;
        }
        
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cbBook.SelectedValue == null) throw new Exception("Vui lòng chọn sản phẩm.");
                int bookId = (int)cbBook.SelectedValue;
                string warehouse = cbWarehouse.SelectedItem?.ToString() ?? "";
                if (!int.TryParse(txtCurrentStock.Text, out int currentStock) || currentStock < 0) throw new Exception("Tồn hiện tại phải là số hợp lệ (>= 0).");
                if (!int.TryParse(txtMinStock.Text, out int minStock) || minStock < 0) throw new Exception("Tồn tối thiểu phải là số hợp lệ (>= 0).");
                
                _inventoryService.UpdateStock(bookId, warehouse, currentStock, minStock);
                
                MessageBox.Show("Đã lưu thông tin kho thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
