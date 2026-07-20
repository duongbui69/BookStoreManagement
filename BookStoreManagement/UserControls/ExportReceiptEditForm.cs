using BookStoreManagement.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;
using System.Linq;

namespace BookStoreManagement.UserControls
{
    public class ExportReceiptEditForm : Form
    {
        private readonly int _receiptId;
        private readonly ExportReceiptService _receiptService;
        private readonly BookService _bookService;
        private ExportReceipt _currentReceipt;

        private Guna2Panel pnlHeader;
        private Label lblTitle;
        private Guna2Button btnClose;

        private Guna2Panel pnlContent;
        
        // Info Section
        private Label lblReceiptCode;
        private Label lblReason;
        private Label lblCustomer;
        private Label lblDate;
        
        // Edit Status
        private Label lblStatus;
        private Guna2ComboBox cbStatus;

        // Details Grid
        private DataGridView dgvDetails;

        // Footer
        private Guna2Panel pnlFooter;
        private Guna2Button btnCancel;
        private Guna2Button btnSave;

        public ExportReceiptEditForm(int receiptId)
        {
            _receiptId = receiptId;
            _receiptService = new ExportReceiptService();
            _bookService = new BookService();
            InitializeComponent();
            ApplyTheme();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            
            // Header
            pnlHeader = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BorderThickness = 1,
                BorderRadius = 8,
                CustomizableEdges = new Guna.UI2.WinForms.Suite.CustomizableEdges(true, true, false, false)
            };
            
            lblTitle = new Label
            {
                Text = "Chi tiết Phiếu xuất",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 18)
            };
            
            btnClose = new Guna2Button
            {
                Text = "X",
                Size = new Size(40, 40),
                Location = new Point(this.Width - 50, 10),
                FillColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, btnClose });

            // Content
            pnlContent = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24)
            };

            lblReceiptCode = new Label { Location = new Point(24, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            lblCustomer = new Label { Location = new Point(24, 50), AutoSize = true, Font = new Font("Segoe UI", 10) };
            lblReason = new Label { Location = new Point(400, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            lblDate = new Label { Location = new Point(400, 50), AutoSize = true, Font = new Font("Segoe UI", 10) };

            lblStatus = new Label { Text = "Trạng thái:", Location = new Point(24, 90), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            cbStatus = new Guna2ComboBox
            {
                Location = new Point(120, 85),
                Size = new Size(200, 36),
                BorderRadius = 4
            };
            cbStatus.Items.AddRange(new string[] { "Chờ duyệt", "Đang giao", "Hoàn thành", "Đã hủy" });

            dgvDetails = new DataGridView
            {
                Location = new Point(24, 140),
                Size = new Size(752, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 40 }
            };
            dgvDetails.SetDoubleBuffered(true);
            dgvDetails.Columns.Add("BookName", "SẢN PHẨM");
            dgvDetails.Columns.Add("Quantity", "SỐ LƯỢNG");
            dgvDetails.Columns.Add("Price", "ĐƠN GIÁ");
            dgvDetails.Columns.Add("LineTotal", "THÀNH TIỀN");

            pnlContent.Controls.AddRange(new Control[] { lblReceiptCode, lblCustomer, lblReason, lblDate, lblStatus, cbStatus, dgvDetails });

            // Footer
            pnlFooter = new Guna2Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BorderThickness = 1,
                BorderRadius = 8,
                CustomizableEdges = new Guna.UI2.WinForms.Suite.CustomizableEdges(false, false, true, true)
            };

            btnCancel = new Guna2Button
            {
                Text = "Hủy bỏ",
                Size = new Size(100, 40),
                Location = new Point(this.Width - 250, 15),
                BorderRadius = 4,
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            btnSave = new Guna2Button
            {
                Text = "Lưu thay đổi",
                Size = new Size(120, 40),
                Location = new Point(this.Width - 140, 15),
                BorderRadius = 4,
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            pnlFooter.Controls.AddRange(new Control[] { btnCancel, btnSave });

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);
        }

        private void LoadData()
        {
            _currentReceipt = _receiptService.GetById(_receiptId);
            if (_currentReceipt == null) return;

            lblReceiptCode.Text = $"Mã phiếu: {_currentReceipt.ReceiptCode}";
            lblCustomer.Text = $"Khách hàng: {_currentReceipt.CustomerName}";
            lblReason.Text = $"Lý do xuất: {_currentReceipt.Reason}";
            lblDate.Text = $"Ngày xuất: {_currentReceipt.ExportDate.ToString("dd/MM/yyyy HH:mm")}";

            cbStatus.SelectedItem = _currentReceipt.Status;

            var details = _receiptService.GetDetails(_receiptId);
            if (this.IsDisposed) return;
            dgvDetails.Rows.Clear();
            foreach (var d in details)
            {
                var book = _bookService.GetById(d.BookId);
                string bookName = book != null ? book.Title : $"Book #{d.BookId}";
                dgvDetails.Rows.Add(
                    bookName,
                    d.Quantity,
                    d.Price.ToString("N0") + " đ",
                    d.LineTotal.ToString("N0") + " đ"
                );
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string newStatus = cbStatus.SelectedItem?.ToString() ?? "Hoàn thành";
            _receiptService.UpdateStatus(_receiptId, newStatus);
            this.DialogResult = DialogResult.OK;
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;

            pnlHeader.BackColor = ThemeManager.CardBackground;
            pnlHeader.CustomBorderColor = ThemeManager.TextBoxBorder;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            btnClose.ForeColor = ThemeManager.TextSecondary;

            pnlContent.BackColor = ThemeManager.CardBackground;
            lblReceiptCode.ForeColor = ThemeManager.TextPrimary;
            lblCustomer.ForeColor = ThemeManager.TextPrimary;
            lblReason.ForeColor = ThemeManager.TextPrimary;
            lblDate.ForeColor = ThemeManager.TextPrimary;
            lblStatus.ForeColor = ThemeManager.TextPrimary;

            cbStatus.FillColor = ThemeManager.TextBoxBackground;
            cbStatus.ForeColor = ThemeManager.TextPrimary;
            cbStatus.BorderColor = ThemeManager.TextBoxBorder;

            dgvDetails.BackgroundColor = ThemeManager.CardBackground;
            dgvDetails.GridColor = ThemeManager.TextBoxBorder;
            dgvDetails.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvDetails.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvDetails.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvDetails.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;

            pnlFooter.BackColor = ThemeManager.CardBackground;
            pnlFooter.CustomBorderColor = ThemeManager.TextBoxBorder;

            btnCancel.FillColor = ThemeManager.CardBackground;
            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.BorderThickness = 1;
            btnCancel.BorderColor = ThemeManager.TextBoxBorder;

            btnSave.FillColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;
        }
    }
}