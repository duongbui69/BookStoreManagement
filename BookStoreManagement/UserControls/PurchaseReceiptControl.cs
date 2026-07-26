using BookStoreManagement.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;
using BookStoreManagement.Interfaces;
using Guna.UI2.WinForms;
using System.Collections.Generic;
using System.Linq;

namespace BookStoreManagement.UserControls
{
    public partial class PurchaseReceiptControl : UserControl
    {
        private readonly PurchaseReceiptService _receiptService;
        private readonly SupplierService _supplierService;
        private List<PurchaseReceiptListViewModel> _allReceipts = new();
        private List<PurchaseReceiptListViewModel> _filteredReceipts = new();
        
        // UI Components
        private Label lblTitle;
        private Label lblSubTitle;
        private Guna2Button btnExport;
        private Guna2Button btnAdd;

        // Stat Cards
        private Guna2Panel cardTotalReceipts;
        private Label lblTotalReceiptsValue;
        private Guna2Panel cardTotalValue;
        private Label lblTotalValueAmount;
        private Guna2Panel cardPending;
        private Label lblPendingValue;

        // Filters
        private Guna2Panel pnlFilterBar;
        private Guna2TextBox txtSearch;
        private Guna2ComboBox cbStatus;
        private Guna2ComboBox cbSupplier;
        private Guna2Button btnFilter;
        private Guna2Button btnRefresh;

        // Grid
        private Guna2Panel pnlGridContainer;
        private DataGridView dgvReceipts;
        private PaginationControl pagination;
        private int _currentPage = 1;
        private int PageSize = 10;
        private bool _isCalculatingPageSize = false;

        private int _hoveredRowIndex = -1;
        private int _hoveredColIndex = -1;

        public PurchaseReceiptControl()
        {
            _receiptService = new PurchaseReceiptService();
            _supplierService = new SupplierService();
            InitializeComponent();
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24);
            this.AutoScroll = true;

            // Header
            Guna2Panel pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.Transparent };
            
            lblTitle = new Label
            {
                Text = "Nhập hàng",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            lblSubTitle = new Label
            {
                Text = "Quản lý phiếu nhập và hàng hóa nhập.",
                Font = new Font("Segoe UI", 11F),
                AutoSize = true,
                Location = new Point(0, 45)
            };

            btnExport = new Guna2Button
            {
                Text = "Xuất Excel",
                Size = new Size(120, 36),
                BorderRadius = 4,
                BorderThickness = 1,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.Width - 300, 20),
                Cursor = Cursors.Hand
            };
            btnAdd = new Guna2Button
            {
                Text = "+ Tạo Phiếu Nhập",
                Size = new Size(140, 36),
                BorderRadius = 4,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.Width - 140, 20),
                Cursor = Cursors.Hand
            };

            btnAdd.Click += BtnAdd_Click;
            btnExport.Click += BtnExport_Click;

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle, btnExport, btnAdd });

            // Stat Cards Container
            TableLayoutPanel pnlStats = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 120,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 16, 0, 16),
                Padding = new Padding(0, 16, 0, 16)
            };
            pnlStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pnlStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pnlStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            cardTotalReceipts = CreateStatCard("TỔNG PHIẾU NHẬP", out lblTotalReceiptsValue);
            cardTotalValue = CreateStatCard("TỔNG GIÁ TRỊ (Đã nhập)", out lblTotalValueAmount);
            cardPending = CreateStatCard("CHỜ DUYỆT", out lblPendingValue);

            pnlStats.Controls.Add(cardTotalReceipts, 0, 0);
            pnlStats.Controls.Add(cardTotalValue, 1, 0);
            pnlStats.Controls.Add(cardPending, 2, 0);

            // Filter Bar
            pnlFilterBar = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BorderRadius = 8,
                BorderThickness = 1,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 16)
            };

            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Tìm theo mã phiếu...",
                Size = new Size(200, 36),
                Location = new Point(12, 12),
                BorderRadius = 4
            };

            cbStatus = new Guna2ComboBox
            {
                Size = new Size(160, 36),
                Location = new Point(220, 12),
                BorderRadius = 4
            };
            cbStatus.Items.AddRange(new object[] { "Tất cả Trạng thái", "Đã nhập", "Chờ duyệt", "Đã hủy" });
            cbStatus.SelectedIndex = 0;

            cbSupplier = new Guna2ComboBox
            {
                Size = new Size(180, 36),
                Location = new Point(390, 12),
                BorderRadius = 4
            };

            btnFilter = new Guna2Button
            {
                Text = "Lọc",
                Size = new Size(60, 36),
                Location = new Point(580, 12),
                BorderRadius = 4,
                Cursor = Cursors.Hand
            };
            btnFilter.Click += (s, e) => ApplyFilters();

            btnRefresh = new Guna2Button
            {
                Text = "Làm mới",
                Size = new Size(80, 36),
                Location = new Point(650, 12),
                BorderRadius = 4,
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += (s, e) => LoadData();

            pnlFilterBar.Controls.AddRange(new Control[] { txtSearch, cbStatus, cbSupplier, btnFilter, btnRefresh });

            // Grid Container
            pnlGridContainer = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 8,
                BorderThickness = 1,
                Padding = new Padding(1)
            };

            dgvReceipts = new DataGridView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 45 },
                EnableHeadersVisualStyles = false,
                Cursor = Cursors.Hand
            };
            dgvReceipts.SetDoubleBuffered(true);

            // Columns
            dgvReceipts.Columns.Add("ReceiptCode", "Mã phiếu");
            dgvReceipts.Columns.Add("SupplierName", "Nhà cung cấp");
            dgvReceipts.Columns.Add("ImportDate", "Ngày nhập");
            dgvReceipts.Columns.Add("TotalAmount", "Tổng tiền (đ)");
            dgvReceipts.Columns.Add("Trạng thái", "Trạng thái");
            
            var actionCol = new DataGridViewTextBoxColumn
            {
                Name = "Thao tác",
                HeaderText = "Thao tác",
                Width = 100,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
            dgvReceipts.Columns.Add(actionCol);

            foreach (DataGridViewColumn col in dgvReceipts.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (col.Name != "SupplierName") col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            dgvReceipts.Columns["TotalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvReceipts.CellMouseEnter += DgvReceipts_CellMouseEnter;
            dgvReceipts.CellMouseLeave += DgvReceipts_CellMouseLeave;
            dgvReceipts.CellPainting += DgvReceipts_CellPainting;
            dgvReceipts.CellClick += DgvReceipts_CellClick;

            pagination = new PaginationControl
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };
            pagination.PageChanged += (s, e) => { _currentPage = e.NewPage; DisplayPage(); };

            dgvReceipts.Resize += DgvReceipts_Resize;

            pnlGridContainer.Controls.Add(dgvReceipts);

            this.Controls.Add(pnlGridContainer);
            this.Controls.Add(pagination);
            this.Controls.Add(pnlFilterBar);
            this.Controls.Add(pnlStats);
            this.Controls.Add(pnlHeader);

            LoadFilterData();
            LoadData();
        }

        private Guna2Panel CreateStatCard(string title, out Label lblValue)
        {
            var pnl = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1), Margin = new Padding(0,0,20,0), BorderRadius = 12 };
            
            // Icon
            Color color = title.Contains("PHIẾU") ? Color.FromArgb(41, 128, 185) : 
                          (title.Contains("GIÁ TRỊ") ? Color.FromArgb(0, 186, 97) : Color.FromArgb(243, 156, 18));
            string iconText = title.Contains("PHIẾU") ? "🏷️" : (title.Contains("GIÁ TRỊ") ? "💰" : "⏳");
            
            var pnlIcon = new Guna2Panel { Size = new Size(48, 48), Location = new Point(20, 26), BorderRadius = 24, FillColor = Color.FromArgb(30, color) };
            Label lblIcon = new Label { Text = iconText, Font = new Font("Segoe UI Emoji", 16F), AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            pnlIcon.Controls.Add(lblIcon);
            
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Location = new Point(80, 26) };
            lblTitle.Tag = "CardTitle";
            lblValue = new Label { Text = "0", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(78, 45), ForeColor = color };
            lblValue.Tag = "CardValue";

            pnl.Controls.Add(pnlIcon);
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);
            return pnl;
        }

        private void LoadFilterData()
        {
            var suppliers = _supplierService.GetAll();
            suppliers.Insert(0, new Supplier { Id = 0, SupplierName = "Tất cả Nhà cung cấp" });
            cbSupplier.DataSource = suppliers;
            cbSupplier.DisplayMember = "SupplierName";
            cbSupplier.ValueMember = "Id";
        }

        public void LoadData()
        {
            var stats = _receiptService.GetStats();
            lblTotalReceiptsValue.Text = stats.TotalReceipts.ToString("N0");
            lblTotalValueAmount.Text = stats.TotalValue.ToString("N0") + " ₫";
            lblPendingValue.Text = stats.PendingCount.ToString("N0");

            _allReceipts = _receiptService.GetAll();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allReceipts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string kw = txtSearch.Text.ToLower();
                filtered = filtered.Where(x => x.ReceiptCode.ToLower().Contains(kw));
            }

            if (cbStatus.SelectedIndex > 0)
            {
                string status = cbStatus.SelectedItem.ToString();
                filtered = filtered.Where(x => x.Status == status);
            }

            if (cbSupplier.SelectedValue is int supId && supId > 0)
            {
                filtered = filtered.Where(x => x.SupplierId == supId);
            }

            _filteredReceipts = filtered.ToList();
            _currentPage = 1;
            
            CalculatePageSize();
        }

        private void DgvReceipts_Resize(object sender, EventArgs e)
        {
            if (_isCalculatingPageSize) return;
            CalculatePageSize();
        }

        private void CalculatePageSize()
        {
            if (dgvReceipts.Height == 0) return;
            _isCalculatingPageSize = true;

            int availableHeight = dgvReceipts.Height - dgvReceipts.ColumnHeadersHeight;
            int newPageSize = availableHeight / dgvReceipts.RowTemplate.Height;
            if (newPageSize < 1) newPageSize = 1;

            if (PageSize != newPageSize)
            {
                PageSize = newPageSize;
                _currentPage = 1;
            }

            pagination.UpdatePagination(_filteredReceipts.Count, _currentPage, PageSize);
            DisplayPage();

            _isCalculatingPageSize = false;
        }

        private void DisplayPage()
        {
            if (this.IsDisposed) return;
            dgvReceipts.Rows.Clear();
            var paged = _filteredReceipts.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();

            foreach (var item in paged)
            {
                int rowIndex = dgvReceipts.Rows.Add(
                    item.ReceiptCode,
                    item.SupplierName,
                    item.ImportDate.ToString("dd/MM/yyyy HH:mm"),
                    item.TotalAmount.ToString("N0") + " ₫",
                    item.Status,
                    ""
                );
                dgvReceipts.Rows[rowIndex].Tag = item;
            }
        }

        private void DgvReceipts_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _hoveredRowIndex = e.RowIndex;
                _hoveredColIndex = e.ColumnIndex;
                dgvReceipts.InvalidateRow(e.RowIndex);
            }
        }

        private void DgvReceipts_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _hoveredRowIndex = -1;
                _hoveredColIndex = -1;
                dgvReceipts.InvalidateRow(e.RowIndex);
            }
        }

        private void DgvReceipts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvReceipts.Columns["Thao tác"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                var rect = e.CellBounds;
                var editRect = new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height);
                var delRect = new Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height);
                
                bool hoverEdit = false;
                bool hoverDelete = false;

                if (e.RowIndex == _hoveredRowIndex)
                {
                    Point mouseLoc = dgvReceipts.PointToClient(Cursor.Position);
                    hoverEdit = editRect.Contains(mouseLoc);
                    hoverDelete = delRect.Contains(mouseLoc);

                    if (hoverEdit)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(30, ThemeManager.ButtonFill)))
                            e.Graphics.FillRectangle(brush, editRect);
                    }
                    else if (hoverDelete)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(30, Color.FromArgb(231, 76, 60))))
                            e.Graphics.FillRectangle(brush, delRect);
                    }
                }

                int editFontSize = hoverEdit ? 16 : 14;
                int delFontSize = hoverDelete ? 16 : 14;

                using (var font = new Font("Segoe UI Emoji", editFontSize))
                {
                    TextRenderer.DrawText(e.Graphics, "✏️", font, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                using (var font = new Font("Segoe UI Emoji", delFontSize))
                {
                    TextRenderer.DrawText(e.Graphics, "🗑️", font, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                
                e.Handled = true;
            }
        }

        private void DgvReceipts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvReceipts.Columns["Thao tác"].Index)
            {
                var item = dgvReceipts.Rows[e.RowIndex].Tag as PurchaseReceiptListViewModel;
                if (item == null) return;

                Rectangle cellBounds = dgvReceipts.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                var editRect = new Rectangle(cellBounds.X, cellBounds.Y, cellBounds.Width / 2, cellBounds.Height);
                var delRect = new Rectangle(cellBounds.X + cellBounds.Width / 2, cellBounds.Y, cellBounds.Width / 2, cellBounds.Height);

                Point mouseLoc = dgvReceipts.PointToClient(Cursor.Position);

                if (editRect.Contains(mouseLoc))
                {
                    EditReceipt(item);
                }
                else if (delRect.Contains(mouseLoc))
                {
                    DeleteReceipt(item);
                }
            }
        }

        private void EditReceipt(PurchaseReceiptListViewModel item)
        {
            using var editForm = new PurchaseReceiptEditForm(item.Id);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var addForm = new PurchaseReceiptAddForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx", FileName = "DanhSachPhieuNhap.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var excelService = new ExcelExportService();
                        excelService.ExportDataGridView(dgvReceipts, sfd.FileName, "Phiếu Nhập");
                        MessageBox.Show("Xuất file Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteReceipt(PurchaseReceiptListViewModel item)
        {
            var result = MessageBox.Show($"Bạn có chắc muốn xóa phiếu nhập '{item.ReceiptCode}' không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                // Soft delete or actual delete here.
                // Assuming we can update status to "Đã hủy" instead of actual delete to preserve history
                _receiptService.UpdateStatus(item.Id, "Đã hủy");
                LoadData();
            }
        }

        public void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            
            pnlFilterBar.BackColor = ThemeManager.CardBackground;
            pnlFilterBar.CustomBorderColor = ThemeManager.TextBoxBorder;

            foreach (Control c in pnlFilterBar.Controls)
            {
                if (c is Guna2TextBox txt)
                {
                    txt.FillColor = ThemeManager.TextBoxBackground;
                    txt.ForeColor = ThemeManager.TextPrimary;
                    txt.BorderColor = ThemeManager.TextBoxBorder;
                }
                if (c is Guna2ComboBox cb)
                {
                    cb.FillColor = ThemeManager.TextBoxBackground;
                    cb.ForeColor = ThemeManager.TextPrimary;
                    cb.BorderColor = ThemeManager.TextBoxBorder;
                }
            }

            btnFilter.FillColor = ThemeManager.ButtonFill;
            btnFilter.ForeColor = ThemeManager.ButtonText;
            
            btnRefresh.FillColor = ThemeManager.CardBackground;
            btnRefresh.ForeColor = ThemeManager.TextPrimary;
            btnRefresh.BorderColor = ThemeManager.TextBoxBorder;
            btnRefresh.BorderThickness = 1;

            btnExport.FillColor = ThemeManager.CardBackground;
            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.BorderColor = ThemeManager.TextBoxBorder;

            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;

            ApplyThemeToCard(cardTotalReceipts);
            ApplyThemeToCard(cardTotalValue);
            ApplyThemeToCard(cardPending);

            pnlGridContainer.BackColor = ThemeManager.CardBackground;
            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;

            ApplyThemeToGrid();
        }

        private void ApplyThemeToCard(Guna2Panel card)
        {
            if (card == null) return;
            card.BackColor = ThemeManager.CardBackground;
            card.CustomBorderColor = ThemeManager.TextBoxBorder;
            card.FillColor = ThemeManager.CardBackground;
            foreach (Control c in card.Controls)
            {
                if (c.Tag?.ToString() == "CardTitle") c.ForeColor = ThemeManager.TextSecondary;
            }
        }

        private void ApplyThemeToGrid()
        {
            if (dgvReceipts == null) return;
            
            ThemeManager.ApplyDataGridViewStyle(dgvReceipts);
        }
    }
}
