using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Helpers;
using BookStoreManagement.Themes;

namespace BookStoreManagement.UserControls
{
    public class PurchaseReceiptAddForm : Form
    {
        private readonly PurchaseReceiptService _receiptService;
        private readonly BookService _bookService;
        private readonly StoreService _storeService;
        private readonly SupplierService _supplierService;

        private Guna2ComboBox cbStore;
        private Guna2ComboBox cbSupplier;
        private Guna2TextBox txtNote;
        
        private Guna2ComboBox cbBook;
        private Guna2TextBox txtQuantity;
        private Guna2TextBox txtPrice;
        private Guna2Button btnAddBook;
        
        private DataGridView dgvDetails;
        private Label lblTotal;
        private Guna2Button btnSave;
        private Guna2Button btnCancel;
        
        private List<PurchaseReceiptDetail> _details = new List<PurchaseReceiptDetail>();
        private decimal _totalAmount = 0;

        public PurchaseReceiptAddForm()
        {
            _receiptService = new PurchaseReceiptService();
            _bookService = new BookService();
            _storeService = new StoreService();
            _supplierService = new SupplierService();

            InitializeComponent();
            ApplyTheme();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Thêm Phiếu Nhập Kho";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int startY = 20;

            var pnlTop = new Guna2Panel { Dock = DockStyle.Top, Height = 200, Padding = new Padding(20) };
            
            pnlTop.Controls.Add(new Label { Text = "Chọn Kho:", AutoSize = true, Location = new Point(20, startY + 10) });
            cbStore = new Guna2ComboBox { Location = new Point(120, startY), Size = new Size(250, 36) };
            pnlTop.Controls.Add(cbStore);

            pnlTop.Controls.Add(new Label { Text = "Nhà cung cấp:", AutoSize = true, Location = new Point(400, startY + 10) });
            cbSupplier = new Guna2ComboBox { Location = new Point(510, startY), Size = new Size(250, 36) };
            pnlTop.Controls.Add(cbSupplier);
            
            startY += 50;
            pnlTop.Controls.Add(new Label { Text = "Ghi chú:", AutoSize = true, Location = new Point(20, startY + 10) });
            txtNote = new Guna2TextBox { Location = new Point(120, startY), Size = new Size(640, 36) };
            pnlTop.Controls.Add(txtNote);
            
            startY += 50;
            pnlTop.Controls.Add(new Label { Text = "Sách:", AutoSize = true, Location = new Point(20, startY + 10) });
            cbBook = new Guna2ComboBox { Location = new Point(70, startY), Size = new Size(280, 36) };
            pnlTop.Controls.Add(cbBook);

            pnlTop.Controls.Add(new Label { Text = "SL:", AutoSize = true, Location = new Point(360, startY + 10) });
            txtQuantity = new Guna2TextBox { Location = new Point(390, startY), Size = new Size(80, 36), Text = "1" };
            pnlTop.Controls.Add(txtQuantity);

            pnlTop.Controls.Add(new Label { Text = "Giá nhập:", AutoSize = true, Location = new Point(480, startY + 10) });
            txtPrice = new Guna2TextBox { Location = new Point(550, startY), Size = new Size(110, 36) };
            pnlTop.Controls.Add(txtPrice);

            btnAddBook = new Guna2Button { Text = "Thêm", Location = new Point(670, startY), Size = new Size(90, 36), BorderRadius = 4, Cursor = Cursors.Hand };
            btnAddBook.Click += BtnAddBook_Click;
            pnlTop.Controls.Add(btnAddBook);
            
            cbBook.SelectedIndexChanged += (s, e) => {
                if (cbBook.SelectedValue is int bookId) {
                    var book = _bookService.GetById(bookId);
                    if (book != null) txtPrice.Text = (book.SellingPrice * 0.7m).ToString("0");
                }
            };

            this.Controls.Add(pnlTop);

            var pnlGrid = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 0) };
            dgvDetails = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            
            dgvDetails.Columns.Add("colBookId", "Mã Sách");
            dgvDetails.Columns.Add("colBookName", "Tên Sách");
            dgvDetails.Columns.Add("colQuantity", "Số lượng");
            dgvDetails.Columns.Add("colPrice", "Giá nhập");
            dgvDetails.Columns.Add("colTotal", "Thành tiền");
            
            var btnDeleteCol = new DataGridViewButtonColumn { Name = "colDelete", HeaderText = "", Text = "Xóa", UseColumnTextForButtonValue = true, Width = 60 };
            dgvDetails.Columns.Add(btnDeleteCol);
            
            dgvDetails.CellContentClick += DgvDetails_CellContentClick;
            
            pnlGrid.Controls.Add(dgvDetails);
            this.Controls.Add(pnlGrid);

            var pnlBottom = new Guna2Panel { Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(20) };
            
            lblTotal = new Label { Text = "Tổng tiền: 0 đ", AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 25), ForeColor = Color.Red };
            pnlBottom.Controls.Add(lblTotal);

            btnCancel = new Guna2Button { Text = "Hủy bỏ", Size = new Size(100, 40), Location = new Point(540, 20), BorderRadius = 4, Cursor = Cursors.Hand };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            pnlBottom.Controls.Add(btnCancel);

            btnSave = new Guna2Button { Text = "Lưu Phiếu", Size = new Size(100, 40), Location = new Point(660, 20), BorderRadius = 4, Cursor = Cursors.Hand };
            btnSave.Click += BtnSave_Click;
            pnlBottom.Controls.Add(btnSave);

            this.Controls.Add(pnlBottom);
            
            pnlBottom.BringToFront();
            pnlGrid.BringToFront();
            pnlTop.BringToFront();
        }

        private void LoadData()
        {
            var stores = _storeService.GetActive();
            cbStore.DataSource = stores;
            cbStore.DisplayMember = "StoreName";
            cbStore.ValueMember = "Id";

            var suppliers = _supplierService.GetActive();
            cbSupplier.DataSource = suppliers;
            cbSupplier.DisplayMember = "SupplierName";
            cbSupplier.ValueMember = "Id";

            var books = _bookService.GetActive();
            cbBook.DataSource = books;
            cbBook.DisplayMember = "Title";
            cbBook.ValueMember = "Id";
        }

        private void BtnAddBook_Click(object? sender, EventArgs e)
        {
            if (cbBook.SelectedValue == null) return;
            
            int bookId = (int)cbBook.SelectedValue;
            string bookName = cbBook.Text;
            
            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Số lượng không hợp lệ!");
                return;
            }
            
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Giá không hợp lệ!");
                return;
            }
            
            var existing = _details.FirstOrDefault(d => d.BookId == bookId);
            if (existing != null)
            {
                existing.Quantity += quantity;
                existing.ImportPrice = price;
                existing.LineTotal = existing.Quantity * existing.ImportPrice;
            }
            else
            {
                _details.Add(new PurchaseReceiptDetail
                {
                    BookId = bookId,
                    Quantity = quantity,
                    ImportPrice = price,
                    SellingPrice = price * 1.3m, // Default markup 30%
                    LineTotal = quantity * price
                });
            }
            
            RefreshGrid();
        }

        private void DgvDetails_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvDetails.Columns[e.ColumnIndex].Name == "colDelete")
            {
                int bookId = Convert.ToInt32(dgvDetails.Rows[e.RowIndex].Cells["colBookId"].Value);
                _details.RemoveAll(d => d.BookId == bookId);
                RefreshGrid();
            }
        }

        private void RefreshGrid()
        {
            dgvDetails.Rows.Clear();
            _totalAmount = 0;
            
            foreach (var item in _details)
            {
                var book = _bookService.GetById(item.BookId);
                dgvDetails.Rows.Add(item.BookId, book?.Title ?? "", item.Quantity, item.ImportPrice.ToString("N0"), item.LineTotal.ToString("N0"));
                _totalAmount += item.LineTotal;
            }
            
            lblTotal.Text = $"Tổng tiền: {_totalAmount:N0} đ";
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cbStore.SelectedValue == null) { MessageBox.Show("Vui lòng chọn Kho!"); return; }
            if (cbSupplier.SelectedValue == null) { MessageBox.Show("Vui lòng chọn Nhà cung cấp!"); return; }
            if (_details.Count == 0) { MessageBox.Show("Vui lòng thêm ít nhất một sản phẩm!"); return; }
            
            try
            {
                btnSave.Enabled = false;
                
                var receipt = new PurchaseReceipt
                {
                    ReceiptCode = "PR" + DateTime.Now.ToString("yyMMddHHmmss"),
                    StoreId = (int)cbStore.SelectedValue,
                    SupplierId = (int)cbSupplier.SelectedValue,
                    UserId = CurrentSession.UserId > 0 ? CurrentSession.UserId : 1,
                    Note = txtNote.Text.Trim(),
                    Status = "Đã nhập"
                };
                
                await _receiptService.CreateReceiptAsync(receipt.StoreId, receipt.SupplierId, receipt.Note, _details);
                
                MessageBox.Show("Thêm phiếu nhập thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSave.Enabled = true;
            }
        }

        private void ApplyTheme()
        {
            BookStoreManagement.Themes.ThemeManager.ApplyTypography(this);
            this.BackColor = ThemeManager.Background;
            dgvDetails.BackgroundColor = ThemeManager.CardBackground;
            dgvDetails.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvDetails.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvDetails.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.TextBoxBackground;
            dgvDetails.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvDetails.EnableHeadersVisualStyles = false;

            cbStore.FillColor = ThemeManager.TextBoxBackground; cbStore.ForeColor = ThemeManager.TextPrimary; cbStore.BorderColor = ThemeManager.TextBoxBorder;
            cbSupplier.FillColor = ThemeManager.TextBoxBackground; cbSupplier.ForeColor = ThemeManager.TextPrimary; cbSupplier.BorderColor = ThemeManager.TextBoxBorder;
            cbBook.FillColor = ThemeManager.TextBoxBackground; cbBook.ForeColor = ThemeManager.TextPrimary; cbBook.BorderColor = ThemeManager.TextBoxBorder;
            txtNote.FillColor = ThemeManager.TextBoxBackground; txtNote.ForeColor = ThemeManager.TextPrimary; txtNote.BorderColor = ThemeManager.TextBoxBorder;
            txtQuantity.FillColor = ThemeManager.TextBoxBackground; txtQuantity.ForeColor = ThemeManager.TextPrimary; txtQuantity.BorderColor = ThemeManager.TextBoxBorder;
            txtPrice.FillColor = ThemeManager.TextBoxBackground; txtPrice.ForeColor = ThemeManager.TextPrimary; txtPrice.BorderColor = ThemeManager.TextBoxBorder;
            
            btnAddBook.FillColor = ThemeManager.ButtonFill; btnAddBook.ForeColor = Color.White;
            btnSave.FillColor = ThemeManager.ButtonFill; btnSave.ForeColor = Color.White;
            btnCancel.FillColor = ThemeManager.CardBackground; btnCancel.ForeColor = ThemeManager.TextPrimary; btnCancel.BorderColor = ThemeManager.TextBoxBorder; btnCancel.BorderThickness = 1;
        }
    }
}
