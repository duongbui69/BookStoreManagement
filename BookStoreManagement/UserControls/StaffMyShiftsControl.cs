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
            this.Padding = new Padding(32);

            // Header
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60 };
            lblTitle = new Guna2HtmlLabel
            {
                Text = "Ca làm việc của tôi",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                Location = new Point(0, 0)
            };
            pnlHeader.Controls.Add(lblTitle);
            this.Controls.Add(pnlHeader);

            // Current Shift Widget
            pnlCurrentShift = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 190,
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
            pnlStartInfo = CreateBentoItem("play_circle", "Bắt đầu", out lblStartTime, out lblStartDate);
            pnlRevenueInfo = CreateBentoItem("payments", "Doanh số", out lblRevenue, out _);
            pnlOrdersInfo = CreateBentoItem("shopping_cart", "Đơn hàng", out lblOrders, out _);

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
                Text = "Giờ làm việc",
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
                Text = "Kết thúc ca làm việc",
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
                Text = "Lịch sử ca",
                Font = new Font("Inter", 12F, FontStyle.Bold),
                Location = new Point(16, 18)
            };
            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Tìm ca...",
                BorderRadius = 4,
                Size = new Size(250, 36),
                Location = new Point(pnlHistoryHeader.Width - 266, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            txtSearch.TextChanged += (s, e) => {
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
            };            pnlHistoryHeader.Controls.Add(lblHistoryTitle);
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
            dgvHistory.Columns.Add("Ngày", "Ngày");
            dgvHistory.Columns.Add("ShiftName", "Ca");
            dgvHistory.Columns.Add("Thời gian", "Bắt đầu - Kết thúc");
            dgvHistory.Columns.Add("Revenue", "Doanh số (đ)");
            dgvHistory.Columns["Revenue"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvHistory.Columns.Add("Trạng thái", "Trạng thái");
            dgvHistory.Columns["Trạng thái"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            var actionCol = new DataGridViewTextBoxColumn
            {
                Name = "Thao tác",
                HeaderText = "Thao tác",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgvHistory.Columns.Add(actionCol);
            
            dgvHistory.CellPainting += DgvHistory_CellPainting;
            dgvHistory.CellMouseClick += DgvHistory_CellMouseClick;
            dgvHistory.Resize += DgvHistory_Resize;
            
            pnlHistory.Controls.Add(dgvHistory);
            dgvHistory.BringToFront();

            // Set Z-Order
            pnlHeader.SendToBack();
            pnlCurrentShift.SendToBack();
            spacer.SendToBack();
            pnlHistory.BringToFront();

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
            
            pnlTimer.Size = new Size(260, 150);
            pnlTimer.Location = new Point(currentWidth - 260 - 24, 16);
            
            lblTimer.Location = new Point(16, 42);
            btnCloseShift.Size = new Size(228, 44);
            btnCloseShift.Location = new Point(16, 92);

            int startX = 16;
            int startY = 86;
            int gap = 24;
            int itemWidth = Math.Min(280, (pnlTimer.Left - startX - gap * 2 - 24) / 3);
            if (itemWidth < 100) itemWidth = 100;

            pnlStartInfo.Bounds = new Rectangle(startX, startY, itemWidth, 84);
            pnlRevenueInfo.Bounds = new Rectangle(startX + itemWidth + gap, startY, itemWidth, 84);
            pnlOrdersInfo.Bounds = new Rectangle(startX + itemWidth * 2 + gap * 2, startY, itemWidth, 84);
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
                    // No active shift -> Prompt to open AFTER load is complete
                    _timer.Stop();
                    this.BeginInvoke(new Action(() => {
                        if (this.IsDisposed) return;
                        var openForm = new StartShiftForm();
                        openForm.TopMost = true; // Ensure it doesn't hide behind main window!
                        var parentForm = this.FindForm();
                        if (openForm.ShowDialog(parentForm) == DialogResult.OK)
                        {
                            if (!this.IsDisposed) LoadData(); // Reload everything to update UI properly
                        }
                    }));
                }
                else
                {
                    lblCurrentShiftTitle.Text = $"Ca hiện tại ({_activeShift.ShiftName})";
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

                _shiftHistory = _shiftService.GetShiftHistory();
                CalculatePageSize();
                RenderHistoryPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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



        private void RenderHistoryPage()
        {
            try
            {
                if (dgvHistory.Columns.Count == 0)
                {
                    dgvHistory.Columns.Add("Id", "ID");
                    dgvHistory.Columns["Id"].Width = 80;
                    dgvHistory.Columns.Add("Ngày", "Ngày");
                    dgvHistory.Columns.Add("ShiftName", "Ca");
                    dgvHistory.Columns.Add("Thời gian", "Bắt đầu - Kết thúc");
                    dgvHistory.Columns.Add("Revenue", "Doanh số (đ)");
                    dgvHistory.Columns["Revenue"].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                    dgvHistory.Columns.Add("Trạng thái", "Trạng thái");
                    dgvHistory.Columns["Trạng thái"].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
                    
                    var actionCol = new System.Windows.Forms.DataGridViewTextBoxColumn
                    {
                        Name = "Thao tác",
                        HeaderText = "Thao tác",
                        Width = 100,
                        DefaultCellStyle = { Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter }
                    };
                    dgvHistory.Columns.Add(actionCol);
                }

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
                    string timeStr = $"{item.StartTime:HH:mm} - {(item.EndTime.HasValue ? item.EndTime.Value.ToString("HH:mm") : "Đang mở")}";
                    
                    decimal displayRevenue = item.Revenue;
                    if (_activeShift != null && item.Id == _activeShift.Id) {
                        displayRevenue = _currentRevenue;
                    }

                    int rowIndex = dgvHistory.Rows.Add(
                        "#" + item.Id,
                        item.StartTime.ToString("dd/MM/yyyy"),
                        item.ShiftName,
                        timeStr,
                        displayRevenue.ToString("N0") + " đ",
                        (item.Status == "Closed" || item.Status == "Đã đóng") ? "Đã kết thúc" : "Đang mở",
                        "Chi tiết"
                    );
                    dgvHistory.Rows[rowIndex].Tag = item;
                }
                paginationControl.UpdatePagination(_shiftHistory.Count, _currentPage, _pageSize);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Lỗi StaffMyShiftsControl", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void DgvHistory_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvHistory.Columns["Trạng thái"].Index)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "";
                if (string.IsNullOrEmpty(status)) return;

                Color bgColor = status == "Đã kết thúc" ? ThemeManager.HoverColor : Color.FromArgb(40, Color.ForestGreen);
                Color textColor = status == "Đã kết thúc" ? ThemeManager.TextSecondary : Color.ForestGreen;

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
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvHistory.Columns["Thao tác"].Index)
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
                TextRenderer.DrawText(e.Graphics, "CHI TIẾT", new Font("Inter", 8F, FontStyle.Bold), btnRect, ThemeManager.TextSecondary, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }
        }

        private void DgvHistory_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvHistory.Columns[e.ColumnIndex].Name == "Thao tác")
            {
                var shiftVm = dgvHistory.Rows[e.RowIndex].Tag as ShiftViewModel;
                if (shiftVm != null)
                {
                    var detailsForm = new ShiftDetailsForm(shiftVm.Id);
                    detailsForm.ShowDialog(this.FindForm());
                }
            }
        }

        private void ApplyTheme()
        {
            BookStoreManagement.Themes.ThemeManager.ApplyTypography(this);
            this.BackColor = ThemeManager.Background;
            if (lblTitle != null) lblTitle.ForeColor = ThemeManager.TextPrimary;

            if (pnlCurrentShift != null) pnlCurrentShift.FillColor = ThemeManager.CardBackground;
            if (pnlCurrentShift != null) pnlCurrentShift.BorderColor = ThemeManager.TextBoxBorder;
            if (lblCurrentShiftTitle != null) lblCurrentShiftTitle.ForeColor = ThemeManager.TextPrimary;
            if (lblEmployeeInfo != null) lblEmployeeInfo.ForeColor = ThemeManager.TextSecondary;

            if (pnlStartInfo != null) pnlStartInfo.FillColor = ThemeManager.HoverColor;
            if (pnlStartInfo != null) pnlStartInfo.Controls[0].ForeColor = ThemeManager.TextSecondary;
            if (pnlStartInfo != null) pnlStartInfo.Controls[1].ForeColor = ThemeManager.TextPrimary;
            if (pnlStartInfo != null) pnlStartInfo.Controls[2].ForeColor = ThemeManager.TextSecondary;

            if (pnlRevenueInfo != null) pnlRevenueInfo.FillColor = Color.FromArgb(40, Color.ForestGreen);
            if (pnlRevenueInfo != null) pnlRevenueInfo.Controls[0].ForeColor = Color.ForestGreen;
            if (pnlRevenueInfo != null) pnlRevenueInfo.Controls[1].ForeColor = Color.ForestGreen;
            if (pnlRevenueInfo != null) pnlRevenueInfo.Controls[2].ForeColor = Color.ForestGreen;

            if (pnlOrdersInfo != null) pnlOrdersInfo.FillColor = Color.FromArgb(40, Color.DarkOrchid);
            if (pnlOrdersInfo != null) pnlOrdersInfo.Controls[0].ForeColor = Color.DarkOrchid;
            if (pnlOrdersInfo != null) pnlOrdersInfo.Controls[1].ForeColor = Color.DarkOrchid;
            if (pnlOrdersInfo != null) pnlOrdersInfo.Controls[2].ForeColor = Color.DarkOrchid;

            if (pnlHistory != null) pnlHistory.FillColor = ThemeManager.CardBackground;
            if (pnlHistory != null) pnlHistory.BorderColor = ThemeManager.TextBoxBorder;
            if (lblHistoryTitle != null) lblHistoryTitle.ForeColor = ThemeManager.TextPrimary;
            if (txtSearch != null) txtSearch.FillColor = ThemeManager.TextBoxBackground;
            if (txtSearch != null) txtSearch.ForeColor = ThemeManager.TextPrimary;
            if (txtSearch != null) txtSearch.BorderColor = ThemeManager.TextBoxBorder;

            ThemeManager.ApplyDataGridViewStyle(dgvHistory);
        }
    }
}
