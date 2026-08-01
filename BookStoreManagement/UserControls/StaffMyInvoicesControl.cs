using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookStoreManagement.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.ViewModels;
using Guna.UI2.WinForms;

namespace BookStoreManagement.UserControls
{
    public class StaffMyInvoicesControl : UserControl
    {
        private SalesOrderService _salesOrderService;
        private List<SalesOrderListViewModel> _allItems;
        
        // UI Components
        private Guna2Panel pnlHeader;
        private Guna2HtmlLabel lblTitle;
        private Guna.UI2.WinForms.Guna2Panel pnlFilters;
        private Guna.UI2.WinForms.Guna2TextBox txtSearch;
        private Guna.UI2.WinForms.Guna2DateTimePicker dtpFrom;
        private Guna.UI2.WinForms.Guna2DateTimePicker dtpTo;
        private Guna.UI2.WinForms.Guna2Button btnFilter;
        private Guna2HtmlLabel lblSubtitle;

        private Guna2Panel pnlCards;
        private SummaryCard cardTotal;
        private SummaryCard cardRevenue;
        private SummaryCard cardTransfer;

        private Guna2Panel pnlGridContainer;
        private Guna2DataGridView dgvInvoices;
        private PaginationControl paginationControl;

        private int _currentPage = 1;
        private int _pageSize = 12; // Dynamic
        private bool _isCalculatingPageSize = false;

        public StaffMyInvoicesControl()
        {
            _salesOrderService = new SalesOrderService();
            _allItems = new List<SalesOrderListViewModel>();

            InitializeUI();
            ApplyTheme();

            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Disposed += (s, e) => { ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged; };
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        
        private Guna.UI2.WinForms.Guna2Button btnExport;
        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24);

            pnlHeader = new Guna.UI2.WinForms.Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Transparent
            };

            lblTitle = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                Text = "Hóa đơn của tôi",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                Location = new Point(0, 0)
            };
            pnlHeader.Controls.Add(lblTitle);

            lblSubtitle = new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                Text = "Lịch sử bán hàng trong ca làm việc.",
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, 45)
            };
            pnlHeader.Controls.Add(lblSubtitle);

            pnlFilters = new Guna.UI2.WinForms.Guna2Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 70, 
                CustomBorderThickness = new Padding(1), 
                Margin = new Padding(0, 0, 0, 24), 
                BorderRadius = 8 
            };

            txtSearch = new Guna.UI2.WinForms.Guna2TextBox
            {
                PlaceholderText = "Mã hóa đơn, SĐT khách...",
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F),
                Size = new Size(250, 36),
                Location = new Point(20, 16)
            };
            txtSearch.TextChanged += async (s, e) => {
                _currentPage = 1;
                await LoadDataAsync();
            };

            dtpFrom = new Guna.UI2.WinForms.Guna2DateTimePicker
            {
                Size = new Size(140, 36),
                Location = new Point(280, 16),
                BorderRadius = 6,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9F)
            };
            dtpFrom.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            dtpTo = new Guna.UI2.WinForms.Guna2DateTimePicker
            {
                Size = new Size(140, 36),
                Location = new Point(430, 16),
                BorderRadius = 6,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9F)
            };

            btnFilter = new Guna.UI2.WinForms.Guna2Button
            {
                Text = "LỌC",
                BorderRadius = 6,
                Size = new Size(100, 36),
                Location = new Point(580, 16),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFilter.Click += async (s, e) => await LoadDataAsync();
            
            btnExport = new Guna.UI2.WinForms.Guna2Button
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

            pnlFilters.Controls.AddRange(new Control[] { txtSearch, dtpFrom, dtpTo, btnFilter, btnExport });

            pnlGridContainer = new Guna.UI2.WinForms.Guna2Panel
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

            dgvInvoices = new Guna.UI2.WinForms.Guna2DataGridView
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

            dgvInvoices.Columns.Add("Code", "MÃ HÓA ĐƠN");
            dgvInvoices.Columns.Add("Date", "NGÀY BÁN");
            dgvInvoices.Columns.Add("Customer", "KHÁCH HÀNG");
            dgvInvoices.Columns.Add("Amount", "TỔNG TIỀN");
            dgvInvoices.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvInvoices.Columns["Amount"].DefaultCellStyle.Format = "N0";

            var actionCol = new DataGridViewTextBoxColumn
            {
                Name = "Thao tác",
                HeaderText = "THAO TÁC",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgvInvoices.Columns.Add(actionCol);

            dgvInvoices.CellPainting += DgvInvoices_CellPainting;
            
            dgvInvoices.Resize += DgvInvoices_Resize;
            
            pnlGridContainer.Controls.Add(dgvInvoices);
            dgvInvoices.BringToFront();

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
                using (var sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx", FileName = "HoaDon_" + DateTime.Now.ToString("yyyyMMdd") })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var excelService = new BookStoreManagement.Services.ExcelExportService();
                        excelService.ExportDataGridView(dgvInvoices, sfd.FileName, "Hoa Don");
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

            if (dtpFrom != null) {
                dtpFrom.FillColor = ThemeManager.TextBoxBackground;
                dtpFrom.ForeColor = ThemeManager.TextPrimary;
            }
            if (dtpTo != null) {
                dtpTo.FillColor = ThemeManager.TextBoxBackground;
                dtpTo.ForeColor = ThemeManager.TextPrimary;
            }
            if (txtSearch != null) txtSearch.FocusedState.BorderColor = ThemeManager.ButtonFill;

            if (btnFilter != null) btnFilter.FillColor = ThemeManager.CardBackground;
            if (btnFilter != null) btnFilter.ForeColor = ThemeManager.TextSecondary;
            if (btnFilter != null) btnFilter.BorderColor = ThemeManager.TextBoxBorder;


                        if (pnlGridContainer != null) pnlGridContainer.FillColor = ThemeManager.CardBackground;
            if (pnlGridContainer != null) pnlGridContainer.BorderColor = ThemeManager.TextBoxBorder;
            
            if (btnExport != null)
            {
                if (btnExport != null) btnExport.FillColor = ThemeManager.CardBackground;
                if (btnExport != null) btnExport.ForeColor = ThemeManager.TextPrimary;
                if (btnExport != null) btnExport.BorderColor = ThemeManager.TextBoxBorder;
                if (btnExport != null) btnExport.BorderThickness = 1;
            }

            ThemeManager.ApplyDataGridViewStyle(dgvInvoices);

        }

        private void DgvInvoices_Resize(object? sender, EventArgs e)
        {
            if (_isCalculatingPageSize) return;
            CalculatePageSize();
        }

        private void CalculatePageSize()
        {
            if (dgvInvoices.Height == 0) return;

            _isCalculatingPageSize = true;
            
            int availableHeight = dgvInvoices.Height - dgvInvoices.ColumnHeadersHeight;
            int newPageSize = availableHeight / dgvInvoices.RowTemplate.Height;
            
            if (newPageSize < 1) newPageSize = 1;

            if (_pageSize != newPageSize)
            {
                _pageSize = newPageSize;
                _currentPage = 1;
                RenderCurrentPage();
            }

            _isCalculatingPageSize = false;
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadDataAsync();
        }



        private async Task LoadDataAsync()
        {
            try
            {
                // Load only today's data for current staff by default (shift)
                DateTime today = DateTime.Today;
                DateTime endOfToday = today.AddDays(1).AddTicks(-1);
                
                _allItems = await _salesOrderService.GetByDateRangeAsync(today, endOfToday, CurrentSession.StoreId);

                // Apply search filter if any
                string keyword = txtSearch.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(keyword))
                {
                    _allItems = _allItems.Where(x => x.OrderCode.ToLower().Contains(keyword) || 
                                                     (x.CustomerName?.ToLower().Contains(keyword) == true)).ToList();
                }

                UpdateSummaryCards();

                _currentPage = 1;
                CalculatePageSize();
                RenderCurrentPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSummaryCards()
        {
            int totalInvoices = _allItems.Count;
            decimal shiftRevenue = _allItems.Sum(x => x.TotalAmount);
            decimal bankTransfer = _allItems.Where(x => x.PaymentMethod == AppConstants.PaymentMethods.Banking).Sum(x => x.TotalAmount);

            if (cardTotal != null) cardTotal.SetValue(totalInvoices.ToString("N0"));
            if (cardRevenue != null) cardRevenue.SetValue(shiftRevenue.ToString("N0") + " ₫");
            if (cardTransfer != null) cardTransfer.SetValue(bankTransfer.ToString("N0") + " ₫");
        }

        private void RenderCurrentPage()
        {
            dgvInvoices.Rows.Clear();
            if (_allItems == null || _allItems.Count == 0)
            {
                paginationControl.UpdatePagination(0, 1, _pageSize);
                return;
            }

            int skip = (_currentPage - 1) * _pageSize;
            var pageItems = _allItems.Skip(skip).Take(_pageSize).ToList();

            foreach (var item in pageItems)
            {
                string timeStr = $"{item.OrderDate:HH:mm} - {item.OrderDate:dd/MM}";
                string paymentStr = MapPaymentMethod(item.PaymentMethod);

                int rowIndex = dgvInvoices.Rows.Add(
                    item.Id,
                    item.OrderCode,
                    timeStr,
                    item.CustomerName ?? "Khách lẻ",
                    item.TotalAmount.ToString("N0") + " ₫",
                    paymentStr,
                    "Chi tiết"
                );

                var row = dgvInvoices.Rows[rowIndex];
                row.Tag = item;
                
                row.Cells["OrderCode"].Style.Font = new Font(dgvInvoices.Font, FontStyle.Bold);
                row.Cells["TotalAmount"].Style.Font = new Font(dgvInvoices.Font, FontStyle.Bold);
            }

            paginationControl.UpdatePagination(_allItems.Count, _currentPage, _pageSize);
        }

        private string MapPaymentMethod(string pm)
        {
            switch (pm)
            {
                case AppConstants.PaymentMethods.Cash: return "Tiền mặt";
                case AppConstants.PaymentMethods.Banking: return "Chuyển khoản";
                case AppConstants.PaymentMethods.Card: return "Quẹt thẻ";
                default: return pm;
            }
        }

        private void DgvInvoices_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["PaymentMethod"].Index)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                string method = e.Value?.ToString() ?? "";
                if (string.IsNullOrEmpty(method)) return;

                Color bgColor, textColor;

                if (method == "Tiền mặt")
                {
                    bgColor = Color.FromArgb(40, Color.ForestGreen);
                    textColor = Color.ForestGreen;
                }
                else if (method == "Chuyển khoản")
                {
                    bgColor = Color.FromArgb(40, Color.DarkOrchid);
                    textColor = Color.DarkOrchid;
                }
                else
                {
                    bgColor = Color.FromArgb(40, Color.DarkOrange);
                    textColor = Color.DarkOrange;
                }

                Rectangle badgeRect = e.CellBounds;
                badgeRect.Inflate(-15, -10); // Shrink bounds for badge size
                
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 10;
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

                TextRenderer.DrawText(
                    e.Graphics,
                    method,
                    new Font("Inter", 9F, FontStyle.Bold),
                    badgeRect,
                    textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["Thao tác"].Index)
            {
                e.PaintBackground(e.CellBounds, true);
                
                Rectangle btnRect = e.CellBounds;
                btnRect.Inflate(-10, -10);

                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 4;
                    path.AddArc(btnRect.X, btnRect.Y, radius * 2, radius * 2, 180, 90);
                    path.AddArc(btnRect.Right - radius * 2, btnRect.Y, radius * 2, radius * 2, 270, 90);
                    path.AddArc(btnRect.Right - radius * 2, btnRect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                    path.AddArc(btnRect.X, btnRect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                    path.CloseFigure();

                    using (var brush = new SolidBrush(ThemeManager.CardBackground))
                    using (var pen = new Pen(ThemeManager.TextBoxBorder))
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                        e.Graphics.DrawPath(pen, path);
                    }
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    "CHI TIẾT",
                    new Font("Inter", 8F, FontStyle.Bold),
                    btnRect,
                    ThemeManager.TextSecondary,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );

                e.Handled = true;
            }
        }

        private void DgvInvoices_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvInvoices.Columns["Thao tác"].Index)
            {
                int orderId = Convert.ToInt32(dgvInvoices.Rows[e.RowIndex].Cells["Id"].Value);
                using (var form = new InvoiceForm(orderId))
                {
                    form.ShowDialog();
                }
            }
        }
    }

    // A small helper class for Summary Cards
    internal class SummaryCard : Guna2Panel
    {
        private Guna2HtmlLabel lblTitle;
        private Guna2HtmlLabel lblValue;
        private Guna2Panel iconPanel;

        public SummaryCard(string title, string initialValue, string iconName, Color iconColor)
        {
            this.BorderRadius = 8;
            this.BorderThickness = 1;
            this.Padding = new Padding(16);

            iconPanel = new Guna2Panel
            {
                Size = new Size(48, 48),
                Location = new Point(16, 16),
                BorderRadius = 24,
                FillColor = Color.FromArgb(30, iconColor)
            };
            this.Controls.Add(iconPanel);

            lblTitle = new Guna2HtmlLabel
            {
                Text = title,
                Font = new Font("Inter", 10F),
                Location = new Point(80, 16)
            };
            this.Controls.Add(lblTitle);

            lblValue = new Guna2HtmlLabel
            {
                Text = initialValue,
                Font = new Font("Inter", 16F, FontStyle.Bold),
                Location = new Point(80, 36)
            };
            this.Controls.Add(lblValue);
        }

        public void SetValue(string value)
        {
            lblValue.Text = value;
        }

        public void ApplyTheme()
        {
            this.FillColor = ThemeManager.CardBackground;
            this.BorderColor = ThemeManager.TextBoxBorder;
            lblTitle.ForeColor = ThemeManager.TextSecondary;
            lblValue.ForeColor = ThemeManager.TextPrimary;
        }
    }
}
