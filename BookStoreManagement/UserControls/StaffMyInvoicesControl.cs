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
        private Guna2HtmlLabel lblSubtitle;
        private Guna2TextBox txtSearch;
        private Guna2Button btnFilter;

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

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24);

            // 1. Header Section
            pnlHeader = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(0, 0, 0, 16)
            };
            this.Controls.Add(pnlHeader);

            lblTitle = new Guna2HtmlLabel
            {
                Text = "Lịch sử hóa đơn",
                Font = new Font("Inter", 18F, FontStyle.Bold),
                Location = new Point(0, 10)
            };
            pnlHeader.Controls.Add(lblTitle);

            lblSubtitle = new Guna2HtmlLabel
            {
                Text = $"Ca làm việc hiện tại: Hôm nay ({DateTime.Today:dd/MM/yyyy})",
                Font = new Font("Inter", 10F),
                Location = new Point(0, 55)
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
                PlaceholderText = "Tìm theo mã hóa đơn...",
                BorderRadius = 4,
                Size = new Size(250, 40),
                Location = new Point(pnlHeader.Width - 360, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            txtSearch.KeyDown += TxtSearch_KeyDown;
            pnlHeader.Controls.Add(txtSearch);

            pnlHeader.Resize += (s, e) => {
                btnFilter.Left = pnlHeader.Width - 100;
                txtSearch.Left = pnlHeader.Width - 360;
            };

            // 2. Cards Section
            pnlCards = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                Padding = new Padding(0, 0, 0, 20)
            };
            this.Controls.Add(pnlCards);

            cardTotal = new SummaryCard("Tổng số HĐ", "0", "receipt", Color.FromArgb(43, 73, 103)); 
            cardRevenue = new SummaryCard("Doanh thu ca", "0 đ", "payments", Color.FromArgb(0, 186, 97)); 
            cardTransfer = new SummaryCard("Chuyển khoản", "0 đ", "credit_card", Color.FromArgb(115, 69, 182)); 

            pnlCards.Controls.Add(cardTotal);
            pnlCards.Controls.Add(cardRevenue);
            pnlCards.Controls.Add(cardTransfer);

            pnlCards.Resize += (s, e) =>
            {
                int cardWidth = (pnlCards.Width - 40) / 3; // 20px gap between cards
                cardTotal.Bounds = new Rectangle(0, 0, cardWidth, 80);
                cardRevenue.Bounds = new Rectangle(cardWidth + 20, 0, cardWidth, 80);
                cardTransfer.Bounds = new Rectangle(cardWidth * 2 + 40, 0, cardWidth, 80);
            };

            // 3. Grid Container
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

            dgvInvoices = new Guna2DataGridView
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

            dgvInvoices.Columns.Add("Id", "Id"); // Hidden
            dgvInvoices.Columns["Id"].Visible = false;

            dgvInvoices.Columns.Add("OrderCode", "Mã HĐ");
            dgvInvoices.Columns["OrderCode"].Width = 120;

            dgvInvoices.Columns.Add("OrderDate", "Thời gian");
            dgvInvoices.Columns["OrderDate"].Width = 150;

            dgvInvoices.Columns.Add("CustomerName", "Khách hàng");
            
            dgvInvoices.Columns.Add("TotalAmount", "Tổng tiền");
            dgvInvoices.Columns["TotalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvInvoices.Columns["TotalAmount"].Width = 120;

            dgvInvoices.Columns.Add("PaymentMethod", "Thanh toán");
            dgvInvoices.Columns["PaymentMethod"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvInvoices.Columns["PaymentMethod"].Width = 120;

            var actionCol = new DataGridViewTextBoxColumn
            {
                Name = "Thao tác",
                HeaderText = "Thao tác",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgvInvoices.Columns.Add(actionCol);

            dgvInvoices.CellPainting += DgvInvoices_CellPainting;
            dgvInvoices.CellContentClick += DgvInvoices_CellContentClick;
            dgvInvoices.Resize += DgvInvoices_Resize;
            
            pnlGridContainer.Controls.Add(dgvInvoices);
            dgvInvoices.BringToFront();

            // Fix z-order so grid is below cards and header
            pnlCards.BringToFront();
            pnlHeader.BringToFront();
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

            cardTotal.ApplyTheme();
            cardRevenue.ApplyTheme();
            cardTransfer.ApplyTheme();

            pnlGridContainer.FillColor = ThemeManager.CardBackground;
            pnlGridContainer.BorderColor = ThemeManager.TextBoxBorder;

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

            cardTotal.SetValue(totalInvoices.ToString("N0"));
            cardRevenue.SetValue(shiftRevenue.ToString("N0") + " ₫");
            cardTransfer.SetValue(bankTransfer.ToString("N0") + " ₫");
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
