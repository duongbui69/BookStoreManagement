using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.ViewModels;
using Guna.UI2.WinForms;

namespace BookStoreManagement.UserControls
{
    public class StaffInventoryControl : UserControl
    {
        private StoreBookInventoryService _inventoryService;
        private List<StoreBookInventoryViewModel> _allItems;
        
        // UI Components
        private Guna2Panel pnlHeader;
        private Guna2HtmlLabel lblTitle;
        private Guna2HtmlLabel lblSubtitle;
        private Guna2TextBox txtSearch;
        private Guna2Button btnFilter;

        private Guna2Panel pnlGridContainer;
        private Guna2DataGridView dgvInventory;
        private PaginationControl paginationControl;

        private int _currentPage = 1;
        private int _pageSize = 12; // Will be calculated dynamically
        private bool _isCalculatingPageSize = false;

        public StaffInventoryControl()
        {
            _inventoryService = new StoreBookInventoryService();
            _allItems = new List<StoreBookInventoryViewModel>();

            InitializeUI();
            ApplyTheme();

            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Disposed += (s, e) => { ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged; };
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24); // container-padding

            // Header Section
            pnlHeader = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(0, 0, 0, 16)
            };
            this.Controls.Add(pnlHeader);

            lblTitle = new Guna2HtmlLabel
            {
                Text = "Tra cứu tồn kho",
                Font = new Font("Inter", 18F, FontStyle.Bold),
                Location = new Point(0, 0)
            };
            pnlHeader.Controls.Add(lblTitle);

            lblSubtitle = new Guna2HtmlLabel
            {
                Text = "Xem tình trạng sách tại cửa hàng.",
                Font = new Font("Inter", 10F),
                Location = new Point(0, 32)
            };
            pnlHeader.Controls.Add(lblSubtitle);

            btnFilter = new Guna2Button
            {
                Text = "Lọc",
                BorderRadius = 4,
                BorderThickness = 1,
                Font = new Font("Inter", 9F, FontStyle.Bold),
                Size = new Size(100, 40),
                Location = new Point(pnlHeader.Width - 100, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            pnlHeader.Controls.Add(btnFilter);

            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Tìm theo mã, tên sách, tác giả...",
                BorderRadius = 4,
                Size = new Size(300, 40),
                Location = new Point(pnlHeader.Width - 410, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            txtSearch.KeyDown += TxtSearch_KeyDown;
            pnlHeader.Controls.Add(txtSearch);

            pnlHeader.Resize += (s, e) => {
                btnFilter.Left = pnlHeader.Width - 100;
                txtSearch.Left = pnlHeader.Width - 410;
            };

            // Grid Container
            pnlGridContainer = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 8,
                BorderThickness = 1,
                Padding = new Padding(1)
            };
            this.Controls.Add(pnlGridContainer);
            pnlGridContainer.BringToFront();

            paginationControl = new PaginationControl 
            { 
                Dock = DockStyle.Bottom 
            };
            paginationControl.PageChanged += (s, e) => { 
                _currentPage = e.NewPage; 
                RenderCurrentPage(); 
            };
            pnlGridContainer.Controls.Add(paginationControl);

            dgvInventory = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 48 },
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.None, // NO SCROLLBAR
                ThemeStyle = {
                    HeaderStyle = { Font = new Font("Inter", 9F, FontStyle.Bold), Height = 48 },
                    RowsStyle = { Font = new Font("Inter", 10F) },
                    AlternatingRowsStyle = { Font = new Font("Inter", 10F) }
                }
            };

            dgvInventory.Columns.Add("BookCode", "Mã Sách");
            dgvInventory.Columns["BookCode"].Width = 100;

            dgvInventory.Columns.Add("Title", "Tên Sách");
            dgvInventory.Columns["Title"].FillWeight = 200;

            dgvInventory.Columns.Add("Author", "Tác Giả");
            dgvInventory.Columns.Add("Category", "Thể Loại");
            dgvInventory.Columns.Add("Shelf", "Vị Trí Kệ");
            
            dgvInventory.Columns.Add("Stock", "Tồn Kho");
            dgvInventory.Columns["Stock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvInventory.Columns["Stock"].Width = 100;

            dgvInventory.Columns.Add("Status", "Trạng Thái");
            dgvInventory.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvInventory.Columns["Status"].Width = 120;

            dgvInventory.CellPainting += DgvInventory_CellPainting;
            dgvInventory.Resize += DgvInventory_Resize;
            
            pnlGridContainer.Controls.Add(dgvInventory);
            dgvInventory.BringToFront();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubtitle.ForeColor = ThemeManager.TextSecondary;
            
            txtSearch.FillColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;
            txtSearch.BorderColor = ThemeManager.TextBoxBorder;
            txtSearch.FocusedState.BorderColor = ThemeManager.ButtonFill;

            btnFilter.FillColor = ThemeManager.CardBackground;
            btnFilter.ForeColor = ThemeManager.TextSecondary;
            btnFilter.BorderColor = ThemeManager.TextBoxBorder;

            pnlGridContainer.FillColor = ThemeManager.CardBackground;
            pnlGridContainer.BorderColor = ThemeManager.TextBoxBorder;

            dgvInventory.BackgroundColor = ThemeManager.CardBackground;
            dgvInventory.GridColor = ThemeManager.TextBoxBorder;
            dgvInventory.ThemeStyle.HeaderStyle.BackColor = ThemeManager.Background;
            dgvInventory.ThemeStyle.HeaderStyle.ForeColor = ThemeManager.TextSecondary;
            dgvInventory.ThemeStyle.RowsStyle.BackColor = ThemeManager.CardBackground;
            dgvInventory.ThemeStyle.RowsStyle.ForeColor = ThemeManager.TextPrimary;
            dgvInventory.ThemeStyle.RowsStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvInventory.ThemeStyle.RowsStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvInventory.ThemeStyle.AlternatingRowsStyle.BackColor = ThemeManager.CardBackground;
            dgvInventory.ThemeStyle.AlternatingRowsStyle.ForeColor = ThemeManager.TextPrimary;
            dgvInventory.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvInventory.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvInventory.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvInventory.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
        }

        private void DgvInventory_Resize(object? sender, EventArgs e)
        {
            if (_isCalculatingPageSize) return;

            CalculatePageSize();
        }

        private void CalculatePageSize()
        {
            if (dgvInventory.Height == 0) return;

            _isCalculatingPageSize = true;
            
            int availableHeight = dgvInventory.Height - dgvInventory.ColumnHeadersHeight;
            int newPageSize = availableHeight / dgvInventory.RowTemplate.Height;
            
            if (newPageSize < 1) newPageSize = 1;

            if (_pageSize != newPageSize)
            {
                _pageSize = newPageSize;
                _currentPage = 1; // Reset to page 1 on resize
                RenderCurrentPage();
            }

            _isCalculatingPageSize = false;
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadDataAsync();
        }

        private async void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                if (!CurrentSession.StoreId.HasValue) return;

                string keyword = txtSearch.Text.Trim();
                _allItems = await _inventoryService.SearchAsync(keyword, CurrentSession.StoreId.Value);

                _currentPage = 1;
                CalculatePageSize(); // Ensure page size is correct before rendering
                RenderCurrentPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderCurrentPage()
        {
            dgvInventory.Rows.Clear();
            if (_allItems == null || _allItems.Count == 0)
            {
                paginationControl.UpdatePagination(0, 1, _pageSize);
                return;
            }

            int skip = (_currentPage - 1) * _pageSize;
            var pageItems = _allItems.Skip(skip).Take(_pageSize).ToList();

            foreach (var item in pageItems)
            {
                int rowIndex = dgvInventory.Rows.Add(
                    item.BookCode,
                    item.Title,
                    item.AuthorName ?? "Unknown",
                    item.CategoryName,
                    item.ShelfLocation ?? "Chưa xếp kệ",
                    item.Quantity,
                    GetStatusText(item.Quantity, item.MinStock)
                );

                var row = dgvInventory.Rows[rowIndex];
                row.Tag = item; // Store item for custom painting
                
                if (item.Quantity == 0)
                {
                    row.Cells["Stock"].Style.ForeColor = Color.Firebrick;
                    row.Cells["Stock"].Style.Font = new Font(dgvInventory.Font, FontStyle.Bold);
                }
            }

            paginationControl.UpdatePagination(_allItems.Count, _currentPage, _pageSize);
        }



        private string GetStatusText(int qty, int minStock)
        {
            if (qty <= 0) return "Hết Hàng";
            if (qty <= minStock) return "Sắp Hết";
            return "Còn Hàng";
        }

        private void DgvInventory_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvInventory.Columns["Status"].Index)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                string status = e.Value?.ToString() ?? "";
                if (string.IsNullOrEmpty(status)) return;

                Color bgColor, textColor;

                if (status == "Hết Hàng")
                {
                    bgColor = Color.FromArgb(40, Color.Firebrick);
                    textColor = Color.Firebrick;
                }
                else if (status == "Sắp Hết")
                {
                    bgColor = Color.FromArgb(40, Color.DarkOrange);
                    textColor = Color.DarkOrange;
                }
                else // Còn Hàng
                {
                    bgColor = Color.FromArgb(40, Color.ForestGreen);
                    textColor = Color.ForestGreen;
                }

                // Draw badge background
                Rectangle badgeRect = e.CellBounds;
                badgeRect.Inflate(-10, -10); // Shrink bounds for badge size
                
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 4;
                    path.AddArc(badgeRect.X, badgeRect.Y, radius * 2, radius * 2, 180, 90);
                    path.AddArc(badgeRect.Right - radius * 2, badgeRect.Y, radius * 2, radius * 2, 270, 90);
                    path.AddArc(badgeRect.Right - radius * 2, badgeRect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                    path.AddArc(badgeRect.X, badgeRect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                    path.CloseFigure();

                    using (var brush = new SolidBrush(bgColor))
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                    }
                }

                // Draw text
                TextRenderer.DrawText(
                    e.Graphics,
                    status,
                    new Font("Inter", 9F, FontStyle.Bold),
                    badgeRect,
                    textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }
        }
    }
}
