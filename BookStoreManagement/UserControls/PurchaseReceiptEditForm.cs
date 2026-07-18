using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.Models;
using Guna.UI2.WinForms;

namespace BookStoreManagement.UserControls
{
    public class PurchaseReceiptEditForm : Form
    {
        private readonly int _receiptId;
        private readonly PurchaseReceiptService _receiptService;
        private readonly BookService _bookService;
        private PurchaseReceipt? _receipt;

        private Guna2ComboBox cbStatus;
        private Guna2Button btnSave;
        private Guna2Button btnCancel;
        private DataGridView dgvDetails;

        public PurchaseReceiptEditForm(int receiptId)
        {
            _receiptId = receiptId;
            _receiptService = new PurchaseReceiptService();
            _bookService = new BookService();

            InitializeComponent();
            ApplyTheme();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Chi tiết Phiếu Nhập";
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var pnlTop = new Guna2Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(20) };
            var lblStatus = new Label { Text = "Trạng thái phiếu:", AutoSize = true, Location = new Point(20, 30) };
            cbStatus = new Guna2ComboBox
            {
                Location = new Point(150, 20),
                Size = new Size(200, 36),
                BorderRadius = 4
            };
            cbStatus.Items.AddRange(new object[] { "Đã nhập", "Chờ duyệt", "Đã hủy" });

            pnlTop.Controls.Add(lblStatus);
            pnlTop.Controls.Add(cbStatus);

            var lblDetails = new Label { Text = "Chi tiết sản phẩm:", AutoSize = true, Location = new Point(20, 80) };
            pnlTop.Controls.Add(lblDetails);

            dgvDetails = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                BorderStyle = BorderStyle.None
            };
            dgvDetails.Columns.Add("BookName", "Tên sách");
            dgvDetails.Columns.Add("Quantity", "Số lượng");
            dgvDetails.Columns.Add("ImportPrice", "Giá nhập");
            dgvDetails.Columns.Add("LineTotal", "Thành tiền");

            var pnlGrid = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            pnlGrid.Controls.Add(dgvDetails);

            var pnlBottom = new Guna2Panel { Dock = DockStyle.Bottom, Height = 70, Padding = new Padding(20) };
            btnSave = new Guna2Button
            {
                Text = "Lưu thay đổi",
                Size = new Size(120, 36),
                BorderRadius = 4,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(540, 15),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Guna2Button
            {
                Text = "Hủy bỏ",
                Size = new Size(100, 36),
                BorderRadius = 4,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(420, 15),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();

            pnlBottom.Controls.Add(btnSave);
            pnlBottom.Controls.Add(btnCancel);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlTop);
            this.Controls.Add(pnlBottom);
        }

        private void LoadData()
        {
            _receipt = _receiptService.GetById(_receiptId);
            if (_receipt == null) return;

            this.Text = $"Chi tiết Phiếu Nhập - {_receipt.ReceiptCode}";
            cbStatus.SelectedItem = _receipt.Status;

            var details = _receiptService.GetDetails(_receiptId);
            foreach (var item in details)
            {
                var book = _bookService.GetById(item.BookId);
                string bookName = book != null ? book.Title : $"Sách ID {item.BookId}";
                dgvDetails.Rows.Add(bookName, item.Quantity, item.ImportPrice.ToString("N0"), item.LineTotal.ToString("N0"));
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_receipt == null) return;
            string newStatus = cbStatus.SelectedItem?.ToString() ?? "Chờ duyệt";
            if (_receipt.Status != newStatus)
            {
                _receiptService.UpdateStatus(_receiptId, newStatus);
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            cbStatus.FillColor = ThemeManager.TextBoxBackground;
            cbStatus.ForeColor = ThemeManager.TextPrimary;
            cbStatus.BorderColor = ThemeManager.TextBoxBorder;

            btnSave.FillColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;

            btnCancel.FillColor = ThemeManager.CardBackground;
            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.BorderColor = ThemeManager.TextBoxBorder;
            btnCancel.BorderThickness = 1;

            foreach (Control c in this.Controls)
            {
                if (c is Guna2Panel p)
                {
                    foreach (Control pc in p.Controls)
                    {
                        if (pc is Label l) l.ForeColor = ThemeManager.TextPrimary;
                    }
                }
            }

            dgvDetails.BackgroundColor = ThemeManager.CardBackground;
            dgvDetails.GridColor = ThemeManager.TextBoxBorder;
            dgvDetails.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvDetails.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvDetails.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.CardBackground;
            dgvDetails.ColumnHeadersDefaultCellStyle.SelectionForeColor = ThemeManager.TextSecondary;
            dgvDetails.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvDetails.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvDetails.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvDetails.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
        }
    }
}
