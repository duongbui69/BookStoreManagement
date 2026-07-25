using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    public class StaffMyShiftsControl : UserControl
    {
        private ShiftService _shiftService;
        private SalesOrderService _salesOrderService;
        private Shift? _activeShift;
        private List<ShiftViewModel> _shiftHistory;

        private Guna2Panel pnlHeader;
        private Guna2HtmlLabel lblTitle;

        // Current Shift Info
        private Guna2Panel pnlCurrentShift;
        private Guna2HtmlLabel lblCurrentShiftTitle;
        private Guna2HtmlLabel lblEmployeeInfo;
        
        private Guna2Panel pnlStartInfo;
        private Guna2HtmlLabel lblStartTime;
        private Guna2HtmlLabel lblStartDate;

        private Guna2Panel pnlRevenueInfo;
        private Guna2HtmlLabel lblRevenue;

        private Guna2Panel pnlOrdersInfo;
        private Guna2HtmlLabel lblOrders;

        private Guna2Panel pnlTimer;
        private Guna2HtmlLabel lblTimer;
        private Guna2Button btnCloseShift;
        private System.Windows.Forms.Timer _timer;

        // History Table
        private Guna2Panel pnlHistory;
        private Guna2HtmlLabel lblHistoryTitle;
        private Guna2TextBox txtSearch;
        private Guna2DataGridView dgvHistory;
        private PaginationControl paginationControl;

        private int _currentPage = 1;
        private int _pageSize = 10;
        private bool _isCalculatingPageSize = false;

        private decimal _currentRevenue = 0;
        private int _currentOrders = 0;

        public StaffMyShiftsControl()
        {
            _shiftService = new ShiftService();
            _salesOrderService = new SalesOrderService();
            _shiftHistory = new List<ShiftViewModel>();

            InitializeUI();
            ApplyTheme();

            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Disposed += (s, e) => { 
                ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged; 
                _timer?.Stop();
                _timer?.Dispose();
            };
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24);

            // Header
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80 };
            lblTitle = new Guna2HtmlLabel
            {
                Text = "My shifts",
                Font = new Font("Inter", 18F, FontStyle.Bold),
                Location = new Point(0, 10)
            };
            pnlHeader.Controls.Add(lblTitle);
            this.Controls.Add(pnlHeader);

            // Current Shift Widget
            pnlCurrentShift = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                BorderRadius = 8,
                BorderThickness = 1,
                Padding = new Padding(16)
            };
            this.Controls.Add(pnlCurrentShift);

            lblCurrentShiftTitle = new Guna2HtmlLabel { Font = new Font("Inter", 14F, FontStyle.Bold), Location = new Point(16, 16) };
            pnlCurrentShift.Controls.Add(lblCurrentShiftTitle);

            lblEmployeeInfo = new Guna2HtmlLabel { Font = new Font("Inter", 10F), Location = new Point(16, 45) };
            pnlCurrentShift.Controls.Add(lblEmployeeInfo);

            // Bento Grid Items
            pnlStartInfo = CreateBentoItem("play_circle", "Start", out lblStartTime, out lblStartDate);
            pnlRevenueInfo = CreateBentoItem("payments", "Sales", out lblRevenue, out _);
            pnlOrdersInfo = CreateBentoItem("shopping_cart", "Orders", out lblOrders, out _);

            pnlCurrentShift.Controls.Add(pnlStartInfo);
            pnlCurrentShift.Controls.Add(pnlRevenueInfo);
            pnlCurrentShift.Controls.Add(pnlOrdersInfo);

            // Timer Panel
            pnlTimer = new Guna2Panel
            {
                BorderRadius = 8,
                FillColor = Color.FromArgb(0, 36, 64) // Primary color
            };
            Guna2HtmlLabel lblTimerLabel = new Guna2HtmlLabel
            {
                Text = "Working hours",
                Font = new Font("Inter", 10F),
                ForeColor = Color.FromArgb(172, 201, 237),
                Location = new Point(16, 16)
            };
            lblTimer = new Guna2HtmlLabel
            {
                Text = "00:00:00",
                Font = new Font("Inter", 24F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 45)
            };
            btnCloseShift = new Guna2Button
            {
                Text = "End shift",
                FillColor = Color.FromArgb(186, 26, 26),
                ForeColor = Color.White,
                Font = new Font("Inter", 10F, FontStyle.Bold),
                BorderRadius = 4,
                Size = new Size(160, 40)
            };
            btnCloseShift.Click += BtnCloseShift_Click;

            pnlTimer.Controls.Add(lblTimerLabel);
            pnlTimer.Controls.Add(lblTimer);
            pnlTimer.Controls.Add(btnCloseShift);
            pnlCurrentShift.Controls.Add(pnlTimer);

            // Add spacing
            Guna2Panel spacer = new Guna2Panel { Dock = DockStyle.Top, Height = 24 };
            this.Controls.Add(spacer);

            // History Panel
            pnlHistory = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 8,
                BorderThickness = 1,
                Padding = new Padding(1)
            };
            this.Controls.Add(pnlHistory);
            
            // History Header
            Guna2Panel pnlHistoryHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(16, 10, 16, 10) };
            lblHistoryTitle = new Guna2HtmlLabel
            {
                Text = "Shift history",
                Font = new Font("Inter", 12F, FontStyle.Bold),
                Location = new Point(16, 18)
            };
            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Search shift...",
                BorderRadius = 4,
                Size = new Size(250, 36),
                Location = new Point(pnlHistoryHeader.Width - 266, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            txtSearch.KeyDown += TxtSearch_KeyDown;
            pnlHistoryHeader.Controls.Add(lblHistoryTitle);
            pnlHistoryHeader.Controls.Add(txtSearch);
            pnlHistory.Controls.Add(pnlHistoryHeader);

            pnlHistoryHeader.Resize += (s, e) => { txtSearch.Left = pnlHistoryHeader.Width - 266; };

            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; RenderHistoryPage(); };
            pnlHistory.Controls.Add(paginationControl);

            dgvHistory = new Guna2DataGridView
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
                ScrollBars = ScrollBars.None,
                ThemeStyle = {
                    HeaderStyle = { Font = new Font("Inter", 9F, FontStyle.Bold), Height = 48 },
                    RowsStyle = { Font = new Font("Inter", 10F) },
                    AlternatingRowsStyle = { Font = new Font("Inter", 10F) }
                }
            };
            dgvHistory.Columns.Add("Id", "ID");
            dgvHistory.Columns["Id"].Width = 80;
            dgvHistory.Columns.Add("Date", "Date");
            dgvHistory.Columns.Add("ShiftName", "Ca");
            dgvHistory.Columns.Add("Time", "Start - End");
            dgvHistory.Columns.Add("Revenue", "Sales (VND)");
            dgvHistory.Columns["Revenue"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvHistory.Columns.Add("Status", "Status");
            dgvHistory.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            var actionCol = new DataGridViewTextBoxColumn
            {
                Name = "Action",
                HeaderText = "Action",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgvHistory.Columns.Add(actionCol);
            
            dgvHistory.CellPainting += DgvHistory_CellPainting;
            dgvHistory.Resize += DgvHistory_Resize;
            
            pnlHistory.Controls.Add(dgvHistory);
            dgvHistory.BringToFront();

            // Set Z-Order
            pnlHistory.BringToFront();
            spacer.BringToFront();
            pnlCurrentShift.BringToFront();
            pnlHeader.BringToFront();

            _timer = new System.Windows.Forms.Timer { Interval = 1000 };
            _timer.Tick += Timer_Tick;
        }

        private Guna2Panel CreateBentoItem(string iconStr, string title, out Guna2HtmlLabel val1, out Guna2HtmlLabel val2)
        {
            var pnl = new Guna2Panel { Size = new Size(160, 80), BorderRadius = 8 };
            
            var lblTitle = new Guna2HtmlLabel { Text = title, Font = new Font("Inter", 10F, FontStyle.Bold), Location = new Point(12, 12) };
            pnl.Controls.Add(lblTitle);

            val1 = new Guna2HtmlLabel { Text = "-", Font = new Font("Inter", 14F, FontStyle.Bold), Location = new Point(12, 35) };
            pnl.Controls.Add(val1);

            val2 = new Guna2HtmlLabel { Text = "", Font = new Font("Inter", 9F), Location = new Point(12, 58) };
            pnl.Controls.Add(val2);

            return pnl;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            if (pnlCurrentShift == null) return;
            int currentWidth = pnlCurrentShift.Width;
            
            pnlTimer.Size = new Size(250, 128);
            pnlTimer.Location = new Point(currentWidth - 250 - 16, 16);
            btnCloseShift.Location = new Point(pnlTimer.Width - 160 - 16, pnlTimer.Height - 40 - 16);

            int startX = 16;
            int startY = 70;
            int gap = 16;
            int itemWidth = (pnlTimer.Left - startX - gap * 3) / 3;

            pnlStartInfo.Bounds = new Rectangle(startX, startY, itemWidth, 74);
            pnlRevenueInfo.Bounds = new Rectangle(startX + itemWidth + gap, startY, itemWidth, 74);
            pnlOrdersInfo.Bounds = new Rectangle(startX + itemWidth * 2 + gap * 2, startY, itemWidth, 74);
        }

        private void DgvHistory_Resize(object? sender, EventArgs e)
        {
            if (_isCalculatingPageSize) return;
            CalculatePageSize();
        }

        private void CalculatePageSize()
        {
            if (dgvHistory.Height == 0) return;
            _isCalculatingPageSize = true;
            
            int availableHeight = dgvHistory.Height - dgvHistory.ColumnHeadersHeight;
            int newPageSize = availableHeight / dgvHistory.RowTemplate.Height;
            if (newPageSize < 1) newPageSize = 1;

            if (_pageSize != newPageSize)
            {
                _pageSize = newPageSize;
                _currentPage = 1;
                RenderHistoryPage();
            }
            _isCalculatingPageSize = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                _activeShift = _shiftService.GetActiveShift();
                if (_activeShift == null)
                {
                    // No active shift -> Prompt to open
                    _timer.Stop();
                    var openForm = new StartShiftForm();
                    if (openForm.ShowDialog() == DialogResult.OK)
                    {
                        _activeShift = _shiftService.GetActiveShift();
                    }
                    else
                    {
                        // User cancelled opening shift -> maybe redirect to another tab
                        // For now, let it be empty, they can't do anything.
                    }
                }

                if (_activeShift != null)
                {
                    lblCurrentShiftTitle.Text = $"Current shift ({_activeShift.ShiftName})";
                    lblEmployeeInfo.Text = $"Nhân viên: {CurrentSession.FullName} (ID: EMP-{CurrentSession.UserId})";
                    lblStartTime.Text = _activeShift.StartTime.ToString("HH:mm:ss");
                    // Assuming lblStartDate is not null
                    pnlStartInfo.Controls[2].Text = _activeShift.StartTime.ToString("dd/MM/yyyy");

                    var orders = _salesOrderService.GetByDateRange(_activeShift.StartTime, DateTime.Now)
                                    .Where(x => x.StaffId == CurrentSession.UserId).ToList();
                    
                    _currentRevenue = orders.Sum(x => x.TotalAmount);
                    _currentOrders = orders.Count;

                    lblRevenue.Text = _currentRevenue.ToString("N0") + " ₫";
                    lblOrders.Text = _currentOrders.ToString();

                    _timer.Start();
                }
                else
                {
                    lblCurrentShiftTitle.Text = "No shift available";
                    lblEmployeeInfo.Text = "";
                    lblStartTime.Text = "-";
                    lblRevenue.Text = "0 VND";
                    lblOrders.Text = "0";
                    lblTimer.Text = "00:00:00";
                }

                _shiftHistory = _shiftService.GetShiftHistory();
                CalculatePageSize();
                RenderHistoryPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_activeShift != null)
            {
                var duration = DateTime.Now - _activeShift.StartTime;
                lblTimer.Text = $"{(int)duration.TotalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
            }
        }

        private void BtnCloseShift_Click(object? sender, EventArgs e)
        {
            if (_activeShift == null) return;

            var dialog = new CloseShiftDialog(_activeShift.Id, _currentRevenue, _currentOrders);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _timer.Stop();
                // Prompt to start a new shift
                var startForm = new StartShiftForm();
                if (startForm.ShowDialog() == DialogResult.OK)
                {
                    LoadData(); // Reload to get new shift
                }
                else
                {
                    // Just load data to show closed state or empty current shift
                    LoadData();
                }
            }
        }

        private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string keyword = txtSearch.Text.Trim().ToLower();
                _shiftHistory = _shiftService.GetShiftHistory();
                
                if (!string.IsNullOrEmpty(keyword))
                {
                    _shiftHistory = _shiftHistory.Where(x => 
                        x.Id.ToString().Contains(keyword) || 
                        x.ShiftName.ToLower().Contains(keyword) ||
                        x.StartTime.ToString("dd/MM/yyyy").Contains(keyword)
                    ).ToList();
                }
                _currentPage = 1;
                RenderHistoryPage();
            }
        }

        private void RenderHistoryPage()
        {
            dgvHistory.Rows.Clear();
            if (_shiftHistory == null || _shiftHistory.Count == 0)
            {
                paginationControl.UpdatePagination(0, 1, _pageSize);
                return;
            }

            int skip = (_currentPage - 1) * _pageSize;
            var pageItems = _shiftHistory.Skip(skip).Take(_pageSize).ToList();

            foreach (var item in pageItems)
            {
                string timeStr = $"{item.StartTime:HH:mm} - {(item.EndTime.HasValue ? item.EndTime.Value.ToString("HH:mm") : "Open")}";
                
                int rowIndex = dgvHistory.Rows.Add(
                    "#" + item.Id,
                    item.StartTime.ToString("dd/MM/yyyy"),
                    item.ShiftName,
                    timeStr,
                    item.Revenue.ToString("N0") + " ₫",
                    item.Status == "Closed" ? "Closed" : "Open",
                    "Details"
                );
                dgvHistory.Rows[rowIndex].Tag = item;
            }
            paginationControl.UpdatePagination(_shiftHistory.Count, _currentPage, _pageSize);
        }

        private void DgvHistory_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvHistory.Columns["Status"].Index)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "";
                if (string.IsNullOrEmpty(status)) return;

                Color bgColor = status == "Closed" ? ThemeManager.HoverColor : Color.FromArgb(40, Color.ForestGreen);
                Color textColor = status == "Closed" ? ThemeManager.TextSecondary : Color.ForestGreen;

                Rectangle badgeRect = e.CellBounds;
                badgeRect.Inflate(-25, -12); // Resize for badge

                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 8;
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
                TextRenderer.DrawText(e.Graphics, status, new Font("Inter", 8F, FontStyle.Bold), badgeRect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvHistory.Columns["Action"].Index)
            {
                e.PaintBackground(e.CellBounds, true);
                Rectangle btnRect = e.CellBounds;
                btnRect.Inflate(-15, -12);

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
                TextRenderer.DrawText(e.Graphics, "DETAILS", new Font("Inter", 8F, FontStyle.Bold), btnRect, ThemeManager.TextSecondary, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;

            pnlCurrentShift.FillColor = ThemeManager.CardBackground;
            pnlCurrentShift.BorderColor = ThemeManager.TextBoxBorder;
            lblCurrentShiftTitle.ForeColor = ThemeManager.TextPrimary;
            lblEmployeeInfo.ForeColor = ThemeManager.TextSecondary;

            pnlStartInfo.FillColor = ThemeManager.HoverColor;
            pnlStartInfo.Controls[0].ForeColor = ThemeManager.TextSecondary;
            pnlStartInfo.Controls[1].ForeColor = ThemeManager.TextPrimary;
            pnlStartInfo.Controls[2].ForeColor = ThemeManager.TextSecondary;

            pnlRevenueInfo.FillColor = Color.FromArgb(40, Color.ForestGreen);
            pnlRevenueInfo.Controls[0].ForeColor = Color.ForestGreen;
            pnlRevenueInfo.Controls[1].ForeColor = Color.ForestGreen;
            pnlRevenueInfo.Controls[2].ForeColor = Color.ForestGreen;

            pnlOrdersInfo.FillColor = Color.FromArgb(40, Color.DarkOrchid);
            pnlOrdersInfo.Controls[0].ForeColor = Color.DarkOrchid;
            pnlOrdersInfo.Controls[1].ForeColor = Color.DarkOrchid;
            pnlOrdersInfo.Controls[2].ForeColor = Color.DarkOrchid;

            pnlHistory.FillColor = ThemeManager.CardBackground;
            pnlHistory.BorderColor = ThemeManager.TextBoxBorder;
            lblHistoryTitle.ForeColor = ThemeManager.TextPrimary;
            txtSearch.FillColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;
            txtSearch.BorderColor = ThemeManager.TextBoxBorder;

            ThemeManager.ApplyDataGridViewStyle(dgvHistory);
        }
    }
}
