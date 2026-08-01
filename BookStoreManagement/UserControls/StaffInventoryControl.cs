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
        private Guna2Panel pnlFilters;

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

        
        private Guna2Button btnExport;
        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24);

            pnlHeader = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Transparent
            };

            lblTitle = new Guna2HtmlLabel
            {
                Text = "Tra cứu kho",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                Location = new Point(0, 0)
            };
            pnlHeader.Controls.Add(lblTitle);

            lblSubtitle = new Guna2HtmlLabel
            {
                Text = "Xem trạng thái sách tại chi nhánh.",
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, 45)
            };
            pnlHeader.Controls.Add(lblSubtitle);

            pnlFilters = new Guna2Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 70, 
                CustomBorderThickness = new Padding(1), 
                Margin = new Padding(0, 0, 0, 24), 
                BorderRadius = 8 
            };

            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Tìm theo mã, tên, tác giả...",
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F),
                Size = new Size(300, 36),
                Location = new Point(20, 16)
            };
            txtSearch.TextChanged += txtSearch_TextChanged;

            btnExport = new Guna2Button
            {
                Text = "XUẤT EXCEL",
                BorderRadius = 6,
                Size = new Size(120, 36),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExport.Click += BtnExport_Click;

            pnlFilters.Resize += (s, e) => {
                btnExport.Location = new Point(pnlFilters.Width - 140, 16);
            };

            pnlFilters.Controls.AddRange(new Control[] { txtSearch, btnExport });

            pnlGridContainer = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 8,
                CustomBorderThickness = new Padding(1)
            };

            paginationControl = new PaginationControl
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };
            paginationControl.PageChanged += async (s, e) => 
            {
                _currentPage = e.NewPage;
                RenderCurrentPage();
            };
            pnlGridContainer.Controls.Add(paginationControl);

            dgvInventory = new Guna.UI2.WinForms.Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 50 }
            };

            dgvInventory.Columns.Add("Code", "MÃ SÁCH");
            dgvInventory.Columns["Code"].Width = 100;
            dgvInventory.Columns.Add("Name", "TÊN SÁCH");
            dgvInventory.Columns["Name"].FillWeight = 200;
            dgvInventory.Columns.Add("Tác giả", "TÁC GIẢ");
            dgvInventory.Columns.Add("Category", "THỂ LOẠI");
            dgvInventory.Columns.Add("Location", "VỊ TRÍ KỆ");
            dgvInventory.Columns.Add("Tồn kho", "TỒN KHO");
            dgvInventory.Columns["Tồn kho"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvInventory.Columns["Tồn kho"].Width = 100;

            dgvInventory.Columns.Add("Trạng thái", "TRẠNG THÁI");
            dgvInventory.Columns["Trạng thái"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvInventory.Columns["Trạng thái"].Width = 120;

            dgvInventory.CellPainting += DgvInventory_CellPainting;
            dgvInventory.Resize += DgvInventory_Resize;
            
            pnlGridContainer.Controls.Add(dgvInventory);
            dgvInventory.BringToFront();

            var spacer = new Panel { Dock = DockStyle.Top, Height = 24, BackColor = Color.Transparent };

            this.Controls.Add(pnlGridContainer);
            this.Controls.Add(spacer);
            this.Controls.Add(pnlFilters);
            this.Controls.Add(pnlHeader);
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx", FileName = "TonKho_" + DateTime.Now.ToString("yyyyMMdd") })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var excelService = new BookStoreManagement.Services.ExcelExportService();
                        excelService.ExportDataGridView(dgvInventory, sfd.FileName, "Ton Kho");
                        MessageBox.Show("Xuất file Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            if (lblTitle != null) lblTitle.ForeColor = ThemeManager.TextPrimary;
            if (lblSubtitle != null) lblSubtitle.ForeColor = ThemeManager.TextSecondary;
            
            if (pnlFilters != null)
            {
                if (pnlFilters != null) pnlFilters.BackColor = ThemeManager.CardBackground;
                if (pnlFilters != null) pnlFilters.CustomBorderColor = ThemeManager.TextBoxBorder;
                if (pnlFilters != null) pnlFilters.FillColor = ThemeManager.CardBackground;
            }

            if (txtSearch != null) txtSearch.FillColor = ThemeManager.TextBoxBackground;
            if (txtSearch != null) txtSearch.ForeColor = ThemeManager.TextPrimary;
            if (txtSearch != null) txtSearch.BorderColor = ThemeManager.TextBoxBorder;
            if (txtSearch != null) txtSearch.FocusedState.BorderColor = ThemeManager.ButtonFill;


                        if (pnlGridContainer != null) pnlGridContainer.FillColor = ThemeManager.CardBackground;
            if (pnlGridContainer != null) pnlGridContainer.BorderColor = ThemeManager.TextBoxBorder;

            if (btnExport != null)
            {
                if (btnExport != null) btnExport.FillColor = ThemeManager.CardBackground;
                if (btnExport != null) btnExport.ForeColor = ThemeManager.TextPrimary;
                if (btnExport != null) btnExport.BorderColor = ThemeManager.TextBoxBorder;
                if (btnExport != null) btnExport.BorderThickness = 1;
            }

            ThemeManager.ApplyDataGridViewStyle(dgvInventory);

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

        private async void txtSearch_TextChanged(object? sender, EventArgs e)
        {
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
                    row.Cells["Tồn kho"].Style.ForeColor = Color.Firebrick;
                    row.Cells["Tồn kho"].Style.Font = new Font(dgvInventory.Font, FontStyle.Bold);
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
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvInventory.Columns["Trạng thái"].Index)
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
