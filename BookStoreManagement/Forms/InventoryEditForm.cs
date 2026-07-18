using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Themes;
using BookStoreManagement.Services;
using BookStoreManagement.Models;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class InventoryEditForm : Form
    {
        private readonly InventoryService _inventoryService;
        private readonly BookService _bookService;
        private int _bookId;
        private string _warehouse;
        private bool _isEditMode;

        private Guna2Panel pnlHeader;
        private Label lblTitle;
        private Guna2Button btnClose;
        private Guna2Panel pnlContent;
        
        private Label lblBook;
        private Guna2ComboBox cbBook;
        
        private Label lblWarehouse;
        private Guna2ComboBox cbWarehouse;
        
        private Label lblCurrentStock;
        private Guna2TextBox txtCurrentStock;
        
        private Label lblMinStock;
        private Guna2TextBox txtMinStock;
        
        private Guna2Button btnSave;
        private Guna2Button btnCancel;

        public InventoryEditForm(int bookId = 0, string warehouse = "", int currentStock = 0, int minStock = 0)
        {
            _inventoryService = new InventoryService();
            _bookService = new BookService();
            _bookId = bookId;
            _warehouse = warehouse;
            _isEditMode = bookId > 0 && !string.IsNullOrEmpty(warehouse);
            
            InitializeComponent();
            LoadBooks();
            ApplyTheme();
            
            if (_isEditMode)
            {
                cbBook.SelectedValue = _bookId;
                cbBook.Enabled = false;
                cbWarehouse.SelectedItem = _warehouse;
                cbWarehouse.Enabled = false;
                txtCurrentStock.Text = currentStock.ToString();
                txtMinStock.Text = minStock.ToString();
                lblTitle.Text = "Cập nhật tồn kho";
            }
            else
            {
                lblTitle.Text = "Thêm tồn kho mới";
            }
            
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.FormClosed += (s, e) => ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý tồn kho";
            this.Size = new Size(500, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = ThemeManager.Background;
            
            // Header
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, FillColor = ThemeManager.ButtonFill };
            lblTitle = new Label { Text = "Quản lý tồn kho", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20), BackColor = Color.Transparent };
            btnClose = new Guna2Button { Text = "✕", Size = new Size(40, 40), Location = new Point(450, 10), FillColor = Color.Transparent, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClose.Click += (s, e) => this.Close();
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, btnClose });
            
            // Content
            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(30), FillColor = ThemeManager.Background };
            
            lblBook = new Label { Text = "Sản phẩm", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(30, 20), ForeColor = ThemeManager.TextPrimary };
            cbBook = new Guna2ComboBox { Size = new Size(440, 36), Location = new Point(30, 45), BorderRadius = 4, Font = new Font("Segoe UI", 10) };
            
            lblWarehouse = new Label { Text = "Kho hàng", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(30, 95), ForeColor = ThemeManager.TextPrimary };
            cbWarehouse = new Guna2ComboBox { Size = new Size(440, 36), Location = new Point(30, 120), BorderRadius = 4, Font = new Font("Segoe UI", 10) };
            cbWarehouse.Items.AddRange(new object[] { "Kho Tổng (Hà Nội)", "Kho Chi Nhánh (HCM)", "Kho Miền Trung" });
            cbWarehouse.SelectedIndex = 0;
            
            lblCurrentStock = new Label { Text = "Tồn hiện tại", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(30, 170), ForeColor = ThemeManager.TextPrimary };
            txtCurrentStock = new Guna2TextBox { Size = new Size(440, 36), Location = new Point(30, 195), BorderRadius = 4, Font = new Font("Segoe UI", 10) };
            
            lblMinStock = new Label { Text = "Tồn tối thiểu", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(30, 245), ForeColor = ThemeManager.TextPrimary };
            txtMinStock = new Guna2TextBox { Size = new Size(440, 36), Location = new Point(30, 270), BorderRadius = 4, Font = new Font("Segoe UI", 10) };
            
            btnCancel = new Guna2Button { Text = "Hủy bỏ", Size = new Size(120, 40), Location = new Point(220, 340), BorderRadius = 4, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, BorderThickness = 1 };
            btnCancel.Click += (s, e) => this.Close();
            
            btnSave = new Guna2Button { Text = "Lưu thông tin", Size = new Size(140, 40), Location = new Point(350, 340), BorderRadius = 4, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.Click += BtnSave_Click;
            
            pnlContent.Controls.AddRange(new Control[] { lblBook, cbBook, lblWarehouse, cbWarehouse, lblCurrentStock, txtCurrentStock, lblMinStock, txtMinStock, btnCancel, btnSave });
            
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
            
            // Add Border to Form
            Guna2BorderlessForm borderlessForm = new Guna2BorderlessForm { ContainerControl = this, BorderRadius = 8 };
        }
        
        private void LoadBooks()
        {
            var books = _bookService.GetAll();
            cbBook.DataSource = books;
            cbBook.DisplayMember = "Title";
            cbBook.ValueMember = "Id";
        }
        
        private void ThemeManager_ThemeChanged(object? sender, EventArgs e) => ApplyTheme();

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlContent.FillColor = ThemeManager.Background;
            pnlHeader.FillColor = ThemeManager.ButtonFill;
            lblTitle.ForeColor = Color.White;
            
            lblBook.ForeColor = ThemeManager.TextPrimary;
            lblWarehouse.ForeColor = ThemeManager.TextPrimary;
            lblCurrentStock.ForeColor = ThemeManager.TextPrimary;
            lblMinStock.ForeColor = ThemeManager.TextPrimary;
            
            cbBook.FillColor = ThemeManager.TextBoxBackground;
            cbBook.ForeColor = ThemeManager.TextPrimary;
            cbBook.BorderColor = ThemeManager.TextBoxBorder;
            
            cbWarehouse.FillColor = ThemeManager.TextBoxBackground;
            cbWarehouse.ForeColor = ThemeManager.TextPrimary;
            cbWarehouse.BorderColor = ThemeManager.TextBoxBorder;
            
            txtCurrentStock.FillColor = ThemeManager.TextBoxBackground;
            txtCurrentStock.ForeColor = ThemeManager.TextPrimary;
            txtCurrentStock.BorderColor = ThemeManager.TextBoxBorder;
            
            txtMinStock.FillColor = ThemeManager.TextBoxBackground;
            txtMinStock.ForeColor = ThemeManager.TextPrimary;
            txtMinStock.BorderColor = ThemeManager.TextBoxBorder;
            
            btnCancel.FillColor = Color.Transparent;
            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.BorderColor = ThemeManager.TextBoxBorder;
            
            btnSave.FillColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = Color.White;
            btnSave.BorderColor = ThemeManager.ButtonFill;
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
                
                MessageBox.Show("Đã lưu thông tin tồn kho thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(ThemeManager.TextBoxBorder, 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
    }
}

