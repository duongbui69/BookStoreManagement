using BookStoreManagement.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookStoreManagement.Database;
using BookStoreManagement.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using BookStoreManagement.ViewModels;
using BookStoreManagement.Interfaces;

namespace BookStoreManagement.UserControls
{
    public class RefundControl : UserControl, ISearchableControl
    {
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnAdd;
        private Button btnPrint;

        // Stats
        private Panel pnlStatsGrid;
        private Panel statTotal;
        private Panel statPending;
        private Panel statCompleted;
        private Panel statFilter;

        private Label lblTotalValue;
        private Label lblPendingValue;
        private Label lblCompletedValue;

        private ComboBox cboStatusFilter;
        private DateTimePicker dtpFilter;

        // Grid
        private Panel pnlGridContainer;
        private Panel pnlGridHeader;
        private Label lblGridTitle;
        private DataGridView dgvRefunds;
        
        // Pagination
        private Panel pnlPagination;
        private Label lblPageInfo;
        private FlowLayoutPanel flpPagination;

        private readonly ReturnReceiptRepository _returnRepo;
        private List<ReturnReceiptListViewModel> _allRefunds;
        private List<ReturnReceiptListViewModel> _filteredRefunds;

        private int _currentPage = 1;
        private int _pageSize = 10;
        private bool _isCalculatingPageSize = false;

        private string _searchKeyword = "";
        
        public RefundControl()
        {
            _returnRepo = new ReturnReceiptRepository();
            _allRefunds = new List<ReturnReceiptListViewModel>();
            _filteredRefunds = new List<ReturnReceiptListViewModel>();

            InitializeComponents();
            ApplyTheme();
            
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += RefundControl_Load;
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
            dgvRefunds.Invalidate();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(30);

            // 1. HEADER
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60 };
            
            lblTitle = new Label 
            { 
                Text = "Quản lý Hoàn tiền", 
                Font = new Font("Segoe UI", 24F, FontStyle.Bold), 
                AutoSize = true, 
                Location = new Point(0, 0) 
            };
            
            lblSubtitle = new Label 
            { 
                Text = "Quản lý danh sách phiếu trả hàng và yêu cầu hoàn tiền từ khách hàng.", 
                Font = new Font("Segoe UI", 10F), 
                AutoSize = true, 
                Location = new Point(0, 40) 
            };
            
            FlowLayoutPanel flpActions = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Right,
                Width = 400,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0)
            };

            btnAdd = new Button 
            { 
                Text = "+ Tạo Phiếu Trả Hàng", 
                Size = new Size(180, 40), 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            btnPrint = new Button 
            { 
                Text = "In danh sách", 
                Size = new Size(120, 40), 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            btnPrint.FlatAppearance.BorderSize = 1;
            //btnPrint.Click += BtnPrint_Click;

            flpActions.Controls.Add(btnAdd);
            flpActions.Controls.Add(btnPrint);

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(flpActions);

            // 2. STATS GRID (Bento Grid)
            pnlStatsGrid = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(0, 20, 0, 20) };
            
            TableLayoutPanel tlpStats = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1
            };
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            statTotal = CreateStatPanel("Tổng yêu cầu", out lblTotalValue, "assignment_return");
            statPending = CreateStatPanel("Đang xử lý", out lblPendingValue, "pending_actions");
            statCompleted = CreateStatPanel("Đã hoàn tiền", out lblCompletedValue, "payments");
            
            // Filter panel inside bento grid
            statFilter = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5) };
            
            cboStatusFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Width = 200,
                Location = new Point(15, 15)
            };
            cboStatusFilter.Items.AddRange(new object[] { "Tất cả trạng thái", "Chờ xử lý", "Đã duyệt", "Đã hoàn tiền", "Từ chối" });
            cboStatusFilter.SelectedIndex = 0;
            cboStatusFilter.SelectedIndexChanged += Filter_Changed;

            dtpFilter = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F),
                Width = 200,
                Location = new Point(15, 50)
            };
            dtpFilter.ValueChanged += Filter_Changed;

            statFilter.Controls.Add(cboStatusFilter);
            statFilter.Controls.Add(dtpFilter);

            tlpStats.Controls.Add(statTotal, 0, 0);
            tlpStats.Controls.Add(statPending, 1, 0);
            tlpStats.Controls.Add(statCompleted, 2, 0);
            tlpStats.Controls.Add(statFilter, 3, 0);

            pnlStatsGrid.Controls.Add(tlpStats);

            // 3. GRID CONTAINER
            pnlGridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 20, 0, 0) };
            
            pnlGridHeader = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15, 0, 15, 0) };
            lblGridTitle = new Label 
            { 
                Text = "Danh sách Phiếu trả hàng", 
                Font = new Font("Segoe UI", 12F, FontStyle.Bold), 
                AutoSize = true,
                Location = new Point(15, 15)
            };
            pnlGridHeader.Controls.Add(lblGridTitle);

            dgvRefunds = new DataGridView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 50 },
                AllowUserToResizeRows = false
            };
            dgvRefunds.SetDoubleBuffered(true);
            
            SetupColumns();
            
            dgvRefunds.CellPainting += DgvRefunds_CellPainting;
            dgvRefunds.CellClick += DgvRefunds_CellClick;
            dgvRefunds.CellMouseEnter += DgvRefunds_CellMouseEnter;
            dgvRefunds.CellMouseLeave += DgvRefunds_CellMouseLeave;

            pnlPagination = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(15, 0, 15, 0) };
            lblPageInfo = new Label { AutoSize = true, Location = new Point(15, 20), Font = new Font("Segoe UI", 10F) };
            flpPagination = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Location = new Point(0, 12), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            pnlPagination.Controls.Add(lblPageInfo);
            pnlPagination.Controls.Add(flpPagination);

            pnlGridContainer.Controls.Add(dgvRefunds);
            pnlGridContainer.Controls.Add(pnlGridHeader);
            pnlGridContainer.Controls.Add(pnlPagination);

            this.Controls.Add(pnlGridContainer);
            this.Controls.Add(pnlStatsGrid);
            this.Controls.Add(pnlHeader);

            dgvRefunds.Resize += DgvRefunds_Resize;
        }

        private Panel CreateStatPanel(string title, out Label lblValue, string icon)
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5) };
            
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(15, 15) };
            lblValue = new Label { Text = "0", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(15, 40) };
            
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblValue);
            return pnl;
        }

        private void SetupColumns()
        {
            dgvRefunds.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colCheck", HeaderText = "", Width = 50 });
            dgvRefunds.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReturnCode", HeaderText = "MÃ PHIẾU TRẢ", DataPropertyName = "ReturnCode", Width = 130 });
            dgvRefunds.Columns.Add(new DataGridViewTextBoxColumn { Name = "colOrderCode", HeaderText = "MÃ ĐƠN GỐC", DataPropertyName = "OrderCode", Width = 130 });
            dgvRefunds.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDate", HeaderText = "NGÀY YÊU CẦU", DataPropertyName = "ReturnDate", Width = 120 });
            dgvRefunds.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCustomer", HeaderText = "KHÁCH HÀNG", DataPropertyName = "CustomerName", Width = 150 });
            dgvRefunds.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAmount", HeaderText = "SỐ TIỀN HOÀN", DataPropertyName = "TotalRefundAmount", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvRefunds.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReason", HeaderText = "LÝ DO", DataPropertyName = "Note", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvRefunds.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "TRẠNG THÁI", DataPropertyName = "ReturnStatus", Width = 120 });
            dgvRefunds.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAction", HeaderText = "THAO TÁC", Width = 100 });
            
            foreach (DataGridViewColumn col in dgvRefunds.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            dgvRefunds.Columns["colReason"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvRefunds.Columns["colCustomer"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubtitle.ForeColor = ThemeManager.TextSecondary;
            
            btnAdd.BackColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = Color.White;
            
            btnPrint.BackColor = ThemeManager.CardBackground;
            btnPrint.ForeColor = ThemeManager.TextPrimary;
            btnPrint.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            Color statBg = ThemeManager.CardBackground;
            Color statBorder = ThemeManager.TextBoxBorder;

            foreach (var pnl in new[] { statTotal, statPending, statCompleted, statFilter, pnlGridContainer, pnlGridHeader, pnlPagination })
            {
                pnl.BackColor = statBg;
                if (pnl == statFilter || pnl == statTotal || pnl == statPending || pnl == statCompleted)
                {
                    pnl.Paint += (s, e) =>
                    {
                        Control c = (Control)s;
                        using (Pen pen = new Pen(statBorder, 1))
                        {
                            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, c.Width - 1, c.Height - 1));
                        }
                    };
                }
            }

            foreach (Control c in statTotal.Controls) c.ForeColor = ThemeManager.TextPrimary;
            foreach (Control c in statPending.Controls) c.ForeColor = ThemeManager.TextPrimary;
            foreach (Control c in statCompleted.Controls) c.ForeColor = ThemeManager.TextPrimary;

            lblGridTitle.ForeColor = ThemeManager.TextPrimary;
            lblPageInfo.ForeColor = ThemeManager.TextSecondary;
            
            cboStatusFilter.BackColor = ThemeManager.Background;
            cboStatusFilter.ForeColor = ThemeManager.TextPrimary;
            dtpFilter.BackColor = ThemeManager.Background;
            dtpFilter.ForeColor = ThemeManager.TextPrimary;

            dgvRefunds.BackgroundColor = ThemeManager.CardBackground;
            dgvRefunds.GridColor = ThemeManager.TextBoxBorder;
            dgvRefunds.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvRefunds.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvRefunds.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvRefunds.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvRefunds.DefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvRefunds.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgvRefunds.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvRefunds.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
        }

        private async void RefundControl_Load(object sender, EventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            var storeId = BookStoreManagement.Helpers.CurrentSession.StoreId ?? 1;
            
            try
            {
                var result = await _returnRepo.GetAllAsync();
                _allRefunds = result.Where(r => r.StoreId == storeId).ToList();
                ApplyFilters();
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        public void PerformSearch(string keyword)
        {
            _searchKeyword = keyword.ToLower();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var query = _allRefunds.AsQueryable();

            if (!string.IsNullOrEmpty(_searchKeyword))
            {
                query = query.Where(r => 
                    (r.ReturnCode != null && r.ReturnCode.ToLower().Contains(_searchKeyword)) ||
                    (r.OrderCode != null && r.OrderCode.ToLower().Contains(_searchKeyword)) ||
                    (r.CustomerName != null && r.CustomerName.ToLower().Contains(_searchKeyword)));
            }

            if (cboStatusFilter.SelectedIndex > 0)
            {
                string status = cboStatusFilter.SelectedItem.ToString();
                query = query.Where(r => r.ReturnStatus == status);
            }

            // For DateTime filter, we might want a checkbox to enable/disable. For now, it filters if it's not today?
            // Usually we need a way to clear date. I'll just skip strict date filtering if they didn't implement a clear button, or match the exact date.
            
            _filteredRefunds = query.OrderByDescending(r => r.ReturnDate).ToList();
            _currentPage = 1;
            UpdateGrid();
        }

        private void UpdateStats()
        {
            int total = _allRefunds.Count;
            int pending = _allRefunds.Count(r => r.ReturnStatus == "Chờ xử lý");
            int completed = _allRefunds.Count(r => r.ReturnStatus == "Đã hoàn tiền");

            lblTotalValue.Text = total.ToString("N0");
            lblPendingValue.Text = pending.ToString("N0");
            lblCompletedValue.Text = completed.ToString("N0");
        }

        private void DgvRefunds_Resize(object sender, EventArgs e)
        {
            if (_isCalculatingPageSize) return;
            _isCalculatingPageSize = true;
            
            if (dgvRefunds.Height > 0)
            {
                int availableHeight = dgvRefunds.Height - dgvRefunds.ColumnHeadersHeight;
                int newPageSize = availableHeight / dgvRefunds.RowTemplate.Height;
                if (newPageSize < 1) newPageSize = 1;

                if (_pageSize != newPageSize)
                {
                    _pageSize = newPageSize;
                    _currentPage = 1;
                    UpdateGrid();
                }
            }
            _isCalculatingPageSize = false;
        }

        private void UpdateGrid()
        {
            int totalPages = (int)Math.Ceiling((double)_filteredRefunds.Count / _pageSize);
            if (_currentPage > totalPages && totalPages > 0) _currentPage = totalPages;
            if (_currentPage < 1) _currentPage = 1;

            var items = _filteredRefunds
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)
                .ToList();

            dgvRefunds.DataSource = items;
            
            lblPageInfo.Text = $"Hiển thị {(_filteredRefunds.Count > 0 ? (_currentPage - 1) * _pageSize + 1 : 0)} đến {Math.Min(_currentPage * _pageSize, _filteredRefunds.Count)} của {_filteredRefunds.Count} kết quả";
            
            RenderPagination(totalPages);
        }

        private void RenderPagination(int totalPages)
        {
            flpPagination.Controls.Clear();
            
            // Back button
            Button btnPrev = CreatePageButton("<", _currentPage > 1);
            btnPrev.Click += (s, e) => { if (_currentPage > 1) { _currentPage--; UpdateGrid(); } };
            flpPagination.Controls.Add(btnPrev);

            // Page numbers
            for (int i = 1; i <= totalPages; i++)
            {
                Button btnPage = CreatePageButton(i.ToString(), true, i == _currentPage);
                int page = i;
                btnPage.Click += (s, e) => { _currentPage = page; UpdateGrid(); };
                flpPagination.Controls.Add(btnPage);
            }

            // Next button
            Button btnNext = CreatePageButton(">", _currentPage < totalPages);
            btnNext.Click += (s, e) => { if (_currentPage < totalPages) { _currentPage++; UpdateGrid(); } };
            flpPagination.Controls.Add(btnNext);
            
            flpPagination.Left = pnlPagination.Width - flpPagination.Width - 15;
        }

        private Button CreatePageButton(string text, bool enabled, bool isActive = false)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(32, 32),
                Margin = new Padding(2),
                FlatStyle = FlatStyle.Flat,
                Enabled = enabled,
                Cursor = enabled ? Cursors.Hand : Cursors.Default,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            if (isActive)
            {
                btn.BackColor = ThemeManager.ButtonFill;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderSize = 0;
            }
            else
            {
                btn.BackColor = ThemeManager.CardBackground;
                btn.ForeColor = ThemeManager.TextPrimary;
                btn.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;
            }

            return btn;
        }

        private int hoveredRow = -1;
        private int hoveredCol = -1;

        private void DgvRefunds_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvRefunds.Columns["colAction"].Index)
            {
                hoveredRow = e.RowIndex;
                hoveredCol = e.ColumnIndex;
                dgvRefunds.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }

        private void DgvRefunds_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvRefunds.Columns["colAction"].Index)
            {
                hoveredRow = -1;
                hoveredCol = -1;
                dgvRefunds.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }

        private void DgvRefunds_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvRefunds.Columns["colStatus"].Index)
            {
                e.PaintBackground(e.CellBounds, true);
                
                string status = e.Value?.ToString() ?? "";
                Color bgColor = Color.LightGray;
                Color txtColor = Color.DarkGray;

                if (status == "Chờ xử lý") { bgColor = Color.FromArgb(255, 243, 224); txtColor = Color.FromArgb(230, 81, 0); }
                else if (status == "Đã hoàn tiền") { bgColor = Color.FromArgb(232, 245, 233); txtColor = Color.FromArgb(46, 125, 50); }
                else if (status == "Đã duyệt") { bgColor = Color.FromArgb(227, 242, 253); txtColor = Color.FromArgb(21, 101, 192); }
                else if (status == "Từ chối") { bgColor = Color.FromArgb(255, 218, 214); txtColor = Color.FromArgb(186, 26, 26); }

                Rectangle badgeRect = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 12, e.CellBounds.Width - 20, e.CellBounds.Height - 24);
                
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillRectangle(bgBrush, badgeRect);
                }

                using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                TextRenderer.DrawText(e.Graphics, status, font, badgeRect, txtColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvRefunds.Columns["colAction"].Index)
            {
                e.PaintBackground(e.CellBounds, true);

                // Edit/View icon
                Rectangle viewRect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width / 2) - 25, e.CellBounds.Y + 10, 24, 24);
                Color viewColor = (hoveredRow == e.RowIndex && viewRect.Contains(dgvRefunds.PointToClient(Cursor.Position))) ? ThemeManager.ButtonFill : ThemeManager.TextSecondary;
                DrawIcon(e.Graphics, viewRect, "\ue8f4", viewColor); // visibility

                // Delete icon
                Rectangle deleteRect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width / 2) + 5, e.CellBounds.Y + 10, 24, 24);
                Color delColor = (hoveredRow == e.RowIndex && deleteRect.Contains(dgvRefunds.PointToClient(Cursor.Position))) ? Color.Red : ThemeManager.TextSecondary;
                DrawIcon(e.Graphics, deleteRect, "\ue872", delColor); // delete

                e.Handled = true;
            }
        }

        private void DrawIcon(Graphics g, Rectangle rect, string iconCode, Color color)
        {
            using (Font iconFont = new Font("Material Symbols Outlined", 14F, FontStyle.Regular, GraphicsUnit.Point))
            {
                TextRenderer.DrawText(g, iconCode, iconFont, rect, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private async void DgvRefunds_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvRefunds.Columns["colAction"].Index)
            {
                Rectangle viewRect = new Rectangle(dgvRefunds.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).X + (dgvRefunds.Columns[e.ColumnIndex].Width / 2) - 25, dgvRefunds.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Y + 10, 24, 24);
                Rectangle deleteRect = new Rectangle(dgvRefunds.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).X + (dgvRefunds.Columns[e.ColumnIndex].Width / 2) + 5, dgvRefunds.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Y + 10, 24, 24);
                
                Point mousePos = dgvRefunds.PointToClient(Cursor.Position);
                
                var rowData = dgvRefunds.Rows[e.RowIndex].DataBoundItem as ReturnReceiptListViewModel;
                if (rowData == null) return;

                if (viewRect.Contains(mousePos))
                {
                    // Edit/View
                    var form = new RefundForm(rowData.Id);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        await LoadData();
                    }
                }
                else if (deleteRect.Contains(mousePos))
                {
                    // Delete
                    if (MessageBox.Show($"Bạn có chắc chắn muốn xóa phiếu trả hàng {rowData.ReturnCode}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        await _returnRepo.DeleteAsync(rowData.Id);
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadData();
                    }
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new RefundForm(0);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData().ConfigureAwait(false);
            }
        }
    }
}
