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
    public class ExportReceiptControl : UserControl, ISearchableControl
    {
        private readonly ExportReceiptService _receiptService;
        private List<ExportReceiptListViewModel> _allReceipts = new();
        private List<ExportReceiptListViewModel> _filteredReceipts = new();
        
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
        private Guna2ComboBox cbReason;
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

        public ExportReceiptControl()
        {
            _receiptService = new ExportReceiptService();
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
                Text = "Export",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            lblSubTitle = new Label
            {
                Text = "Manage export receipts and track goods leaving the inventory.",
                Font = new Font("Segoe UI", 11F),
                AutoSize = true,
                Location = new Point(0, 45)
            };

            btnExport = new Guna2Button
            {
                Text = "Export Excel",
                Size = new Size(120, 36),
                BorderRadius = 4,
                BorderThickness = 1,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.Width - 300, 20),
                Cursor = Cursors.Hand
            };
            btnAdd = new Guna2Button
            {
                Text = "+ Create Export Receipt",
                Size = new Size(140, 36),
                BorderRadius = 4,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.Width - 140, 20),
                Cursor = Cursors.Hand
            };

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

            cardTotalReceipts = CreateStatCard("TOTAL EXPORT RECEIPTS", out lblTotalReceiptsValue);
            cardTotalValue = CreateStatCard("TOTAL EXPORT VALUE", out lblTotalValueAmount);
            cardPending = CreateStatCard("PENDING ORDERS", out lblPendingValue);

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
                PlaceholderText = "Search receipt ID...",
                Size = new Size(200, 36),
                Location = new Point(12, 12),
                BorderRadius = 4
            };

            cbReason = new Guna2ComboBox
            {
                Size = new Size(160, 36),
                Location = new Point(220, 12),
                BorderRadius = 4
            };
            cbReason.Items.Add("Export reason");
            cbReason.SelectedIndex = 0;

            btnFilter = new Guna2Button
            {
                Text = "Filter",
                Size = new Size(60, 36),
                Location = new Point(390, 12),
                BorderRadius = 4,
                Cursor = Cursors.Hand
            };
            btnFilter.Click += (s, e) => ApplyFilters();

            btnRefresh = new Guna2Button
            {
                Text = "Reload",
                Size = new Size(80, 36),
                Location = new Point(460, 12),
                BorderRadius = 4,
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += (s, e) => LoadData();

            pnlFilterBar.Controls.AddRange(new Control[] { txtSearch, cbReason, btnFilter, btnRefresh });

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
            dgvReceipts.Columns.Add("ReceiptCode", "RECEIPT ID");
            dgvReceipts.Columns.Add("CustomerReason", "CUSTOMER / PURPOSE");
            dgvReceipts.Columns.Add("ExportDate", "EXPORT DATE");
            dgvReceipts.Columns.Add("UserName", "CREATOR");
            dgvReceipts.Columns.Add("Status", "STATUS");
            
            var actionCol = new DataGridViewTextBoxColumn
            {
                Name = "Action",
                HeaderText = "ACTION",
                Width = 100,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
            dgvReceipts.Columns.Add(actionCol);

            foreach (DataGridViewColumn col in dgvReceipts.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

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
                Location = new Point(0, 0)
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

        public void LoadData()
        {
            var stats = _receiptService.GetStats();
            lblTotalReceiptsValue.Text = stats.TotalReceipts.ToString("N0");
            lblTotalValueAmount.Text = stats.TotalValue.ToString("N0") + " ₫";
            lblPendingValue.Text = stats.PendingCount.ToString("N0");

            _allReceipts = _receiptService.GetAll();
            
            // Populate cbReason dynamically
            string selectedReason = cbReason.SelectedIndex > 0 ? cbReason.SelectedItem?.ToString() : null;
            cbReason.Items.Clear();
            cbReason.Items.Add("Export reason");
            var distinctReasons = _allReceipts.Select(x => x.Reason).Distinct().Where(r => !string.IsNullOrEmpty(r)).OrderBy(r => r).ToList();
            foreach (var r in distinctReasons)
            {
                cbReason.Items.Add(r);
            }
            if (selectedReason != null && cbReason.Items.Contains(selectedReason))
            {
                cbReason.SelectedItem = selectedReason;
            }
            else
            {
                cbReason.SelectedIndex = 0;
            }

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

            if (cbReason.SelectedIndex > 0)
            {
                string reason = cbReason.SelectedItem.ToString();
                filtered = filtered.Where(x => x.Reason == reason);
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
                    item.CustomerName + "\n" + item.Reason,
                    item.ExportDate.ToString("dd/MM/yyyy HH:mm"),
                    item.UserName,
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
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvReceipts.Columns["CustomerReason"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                var item = dgvReceipts.Rows[e.RowIndex].Tag as ExportReceiptListViewModel;
                if (item != null)
                {
                    var rect = e.CellBounds;
                    rect.Y += 5;
                    using (var font = new Font("Segoe UI", 10, FontStyle.Regular))
                    {
                    TextRenderer.DrawText(e.Graphics, item.CustomerName, font, rect, ThemeManager.TextPrimary, TextFormatFlags.Top | TextFormatFlags.HorizontalCenter);
                    }
                    rect.Y += 20;
                    using (var font = new Font("Segoe UI", 9, FontStyle.Regular))
                    {
                    TextRenderer.DrawText(e.Graphics, item.Reason, font, rect, ThemeManager.TextSecondary, TextFormatFlags.Top | TextFormatFlags.HorizontalCenter);
                    }
                }
                e.Handled = true;
            }

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

                    using (var font = new Font("Segoe UI Emoji", 12))
                    {
                    TextRenderer.DrawText(e.Graphics, "✏️", font, rectEdit, editColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                    }
                    using (var font = new Font("Segoe UI Emoji", 12))
                    {
                    TextRenderer.DrawText(e.Graphics, "🗑️", font, rectDelete, deleteColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                    }
                }
                
                e.Handled = true;
            }
        }

        private void DgvReceipts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvReceipts.Columns["Action"].Index)
            {
                var item = dgvReceipts.Rows[e.RowIndex].Tag as ExportReceiptListViewModel;
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

        private void EditReceipt(ExportReceiptListViewModel item)
        {
            using var editForm = new ExportReceiptEditForm(item.Id);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void DeleteReceipt(ExportReceiptListViewModel item)
        {
            var result = MessageBox.Show($"Are you sure you want to delete export receipt '{item.ReceiptCode}'?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                _receiptService.UpdateStatus(item.Id, "Cancelled");
                LoadData();
            }
        }

        public void PerformSearch(string keyword)
        {
            txtSearch.Text = keyword;
            ApplyFilters();
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
            
            ThemeManager.ApplyDataGridViewStyle(dgvReceipts);
        }
    }
}
