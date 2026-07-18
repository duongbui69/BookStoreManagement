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
        private const int PageSize = 5;

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
            Guna2Panel pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
            
            lblTitle = new Label
            {
                Text = "Nhập kho (Goods Inward)",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 20)
            };
            lblSubTitle = new Label
            {
                Text = "Inventory Management > Nhập kho",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(0, 0)
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
                Text = "+ Lập phiếu nhập",
                Size = new Size(140, 36),
                BorderRadius = 4,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.Width - 140, 20),
                Cursor = Cursors.Hand
            };

            pnlHeader.Controls.AddRange(new Control[] { lblSubTitle, lblTitle, btnExport, btnAdd });

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
                PlaceholderText = "Tìm mã phiếu...",
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
                Text = "Tải lại",
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

            // Columns
            dgvReceipts.Columns.Add("ReceiptCode", "Mã phiếu");
            dgvReceipts.Columns.Add("SupplierName", "Nhà cung cấp");
            dgvReceipts.Columns.Add("ImportDate", "Ngày nhập");
            dgvReceipts.Columns.Add("TotalAmount", "Tổng tiền (đ)");
            dgvReceipts.Columns.Add("Status", "Trạng thái");
            
            var actionCol = new DataGridViewTextBoxColumn
            {
                Name = "Action",
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
            var card = new Guna2Panel
            {
                BorderRadius = 8,
                BorderThickness = 1,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 16, 0),
                Padding = new Padding(16)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 16)
            };

            lblValue = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 45)
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            return card;
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
            lblTotalValueAmount.Text = stats.TotalValue.ToString("N0");
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

            _allReceipts = filtered.ToList();
            _currentPage = 1;
            
            pagination.UpdatePagination(_allReceipts.Count, _currentPage, PageSize);
            
            DisplayPage();
        }

        private void DisplayPage()
        {
            if (this.IsDisposed) return;
            dgvReceipts.Rows.Clear();
            var paged = _allReceipts.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();

            foreach (var item in paged)
            {
                int rowIndex = dgvReceipts.Rows.Add(
                    item.ReceiptCode,
                    item.SupplierName,
                    item.ImportDate.ToString("dd/MM/yyyy HH:mm"),
                    item.TotalAmount.ToString("N0"),
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
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvReceipts.Columns["Action"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                bool isHoveredRow = (e.RowIndex == _hoveredRowIndex);
                if (isHoveredRow)
                {
                    int iconSize = 20;
                    int padding = 10;
                    
                    int totalWidth = (iconSize * 2) + padding;
                    int startX = e.CellBounds.Left + (e.CellBounds.Width - totalWidth) / 2;
                    int startY = e.CellBounds.Top + (e.CellBounds.Height - iconSize) / 2;

                    Rectangle rectEdit = new Rectangle(startX, startY, iconSize, iconSize);
                    Rectangle rectDelete = new Rectangle(startX + iconSize + padding, startY, iconSize, iconSize);

                    bool hoverEdit = rectEdit.Contains(dgvReceipts.PointToClient(Cursor.Position));
                    bool hoverDelete = rectDelete.Contains(dgvReceipts.PointToClient(Cursor.Position));

                    Color editColor = hoverEdit ? ThemeManager.ButtonFill : ThemeManager.TextSecondary;
                    Color deleteColor = hoverDelete ? Color.FromArgb(231, 76, 60) : ThemeManager.TextSecondary;

                    TextRenderer.DrawText(e.Graphics, "✏️", new Font("Segoe UI Emoji", 12), rectEdit, editColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                    TextRenderer.DrawText(e.Graphics, "🗑️", new Font("Segoe UI Emoji", 12), rectDelete, deleteColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                
                e.Handled = true;
            }
        }

        private void DgvReceipts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvReceipts.Columns["Action"].Index)
            {
                var item = dgvReceipts.Rows[e.RowIndex].Tag as PurchaseReceiptListViewModel;
                if (item == null) return;

                int iconSize = 20;
                int padding = 10;
                int totalWidth = (iconSize * 2) + padding;
                Rectangle cellBounds = dgvReceipts.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                
                int startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2;
                int startY = cellBounds.Top + (cellBounds.Height - iconSize) / 2;

                Rectangle rectEdit = new Rectangle(startX, startY, iconSize, iconSize);
                Rectangle rectDelete = new Rectangle(startX + iconSize + padding, startY, iconSize, iconSize);

                Point mouseLoc = dgvReceipts.PointToClient(Cursor.Position);

                if (rectEdit.Contains(mouseLoc))
                {
                    EditReceipt(item);
                }
                else if (rectDelete.Contains(mouseLoc))
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

            cardTotalReceipts.BackColor = ThemeManager.CardBackground;
            cardTotalReceipts.CustomBorderColor = ThemeManager.TextBoxBorder;
            cardTotalValue.BackColor = ThemeManager.CardBackground;
            cardTotalValue.CustomBorderColor = ThemeManager.TextBoxBorder;
            cardPending.BackColor = ThemeManager.CardBackground;
            cardPending.CustomBorderColor = ThemeManager.TextBoxBorder;

            foreach (Control c in cardTotalReceipts.Controls) if (c is Label l) l.ForeColor = ThemeManager.TextPrimary;
            foreach (Control c in cardTotalValue.Controls) if (c is Label l) l.ForeColor = ThemeManager.TextPrimary;
            foreach (Control c in cardPending.Controls) if (c is Label l) l.ForeColor = ThemeManager.TextPrimary;

            pnlGridContainer.BackColor = ThemeManager.CardBackground;
            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;

            ApplyThemeToGrid();
        }

        private void ApplyThemeToGrid()
        {
            if (dgvReceipts == null) return;
            
            dgvReceipts.BackgroundColor = ThemeManager.CardBackground;
            dgvReceipts.GridColor = ThemeManager.TextBoxBorder;
            
            dgvReceipts.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvReceipts.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvReceipts.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.CardBackground;
            dgvReceipts.ColumnHeadersDefaultCellStyle.SelectionForeColor = ThemeManager.TextSecondary;
            dgvReceipts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            dgvReceipts.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvReceipts.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvReceipts.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvReceipts.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvReceipts.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        }
    }
}

