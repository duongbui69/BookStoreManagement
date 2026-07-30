using BookStoreManagement.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookStoreManagement.Forms;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using BookStoreManagement.ViewModels;
using BookStoreManagement.Interfaces;
using Guna.UI2.WinForms;

namespace BookStoreManagement.UserControls
{
    public class RefundControl : UserControl, ISearchableControl
    {
        // ─── Repository ───────────────────────────────────────────────────────
        private readonly ReturnReceiptRepository _returnRepo;

        // ─── State ────────────────────────────────────────────────────────────
        private List<ReturnReceiptListViewModel> _allRefunds = new();
        private List<ReturnReceiptListViewModel> _filteredRefunds = new();
        private int _currentPage = 1;
        private const int PageSize = 12; // fallback — được ghi đè bởi _dynamicPageSize
        private int _dynamicPageSize = 12;
        private string _searchKeyword = "";
        private string _statusFilter = ""; // "" = all

        // ─── Layout panels ────────────────────────────────────────────────────
        private Guna2Panel pnlContent;
        private Guna2Panel pnlHeader;
        private Guna2Panel pnlFilters;
        private Guna2Panel pnlStatusBar;
        private Guna2Panel pnlKpi;
        private Panel _spacerTop;
        private Panel _spacerBot;
        private Guna2Panel pnlGridContainer;

        // ─── Header ───────────────────────────────────────────────────────────
        private Label lblTitle;
        private Label lblSubTitle;
        private Guna2Button btnAdd;

        // ─── Filters ──────────────────────────────────────────────────────────
        private Guna2TextBox txtSearch;
        private Guna2DateTimePicker dtpFrom;
        private Guna2DateTimePicker dtpTo;
        private Guna2Button btnFilter;

        // ─── Status toggle buttons ────────────────────────────────────────────
        private Guna2Button btnAll;
        private Guna2Button btnPending;
        private Guna2Button btnApproved;
        private Guna2Button btnCompleted;
        private Guna2Button btnRejected;

        // ─── KPI cards ────────────────────────────────────────────────────────
        private Label lblKpiTotalValue;
        private Label lblKpiPendingValue;
        private Label lblKpiCompletedValue;

        // ─── Grid ─────────────────────────────────────────────────────────────
        private Guna2DataGridView dgv;
        private PaginationControl pagination;

        // ─── Hover state ─────────────────────────────────────────────────────
        private int hoveredRow = -1;

        // ─── ctor ─────────────────────────────────────────────────────────────
        public RefundControl()
        {
            _returnRepo = new ReturnReceiptRepository();

            InitializeUI();
            ApplyTheme();
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
            this.Load += async (s, e) => await LoadDataAsync();
        }

        // ═════════════════════════════════════════════════════════════════════
        // UI BUILD
        // ═════════════════════════════════════════════════════════════════════
        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 8) };
            this.Controls.Add(pnlContent);

            BuildHeader();
            BuildFilters();
            BuildStatusBar();
            BuildKpi();  // BuildKpi cũng Add spacerTop/spacerBot vào pnlContent
            BuildGrid();

            // Thêm tất cả vào pnlContent
            pnlContent.Controls.AddRange(new Control[] {
                pnlHeader, pnlFilters, pnlStatusBar, _spacerTop, pnlKpi, _spacerBot, pnlGridContainer
            });

            // Sắp xếp Z-order chuẩn bằng BringToFront() theo thứ tự từ trên xuống dưới
            pnlHeader.BringToFront();
            _spacerTop.BringToFront();
            pnlKpi.BringToFront();
            _spacerBot.BringToFront();
            pnlStatusBar.BringToFront();
            pnlFilters.BringToFront();
            pnlGridContainer.BringToFront();
        }

        private void BuildHeader()
        {
            // Giảm chiều cao xuống 58px để giải phóng không gian cho grid
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.Transparent };

            lblTitle = new Label
            {
                Text = "Quản lý Đổi/Trả",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            lblSubTitle = new Label
            {
                Text = "Quản lý phiếu trả hàng và yêu cầu hoàn tiền",
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(2, 40)
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });
        }

        private void BuildFilters()
        {
            // Filter row: [Search] [Từ ngày][DateFrom] [Đến ngày][DateTo] [Lọc]  ──────  [+ Tạo Phiếu Trả]
            pnlFilters = new Guna2Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.Transparent };

            int y = 6; // vertical center of 46px panel

            // Search
            Label lSearch = new Label { Text = "Tìm kiếm", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(0, y + 8) };
            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Mã phiếu, mã đơn, tên khách...",
                Size = new Size(210, 34),
                Location = new Point(68, y),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F)
            };
            txtSearch.TextChanged += (s, e) =>
            {
                _searchKeyword = txtSearch.Text.ToLower();
                _currentPage = 1;
                ApplyFilters();
            };

            // Date range
            Label lFrom = new Label { Text = "Từ ngày", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(294, y + 8) };
            dtpFrom = new Guna2DateTimePicker
            {
                Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                Format = DateTimePickerFormat.Short,
                Size = new Size(120, 34),
                Location = new Point(354, y),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F)
            };

            Label lTo = new Label { Text = "Đến ngày", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(488, y + 8) };
            dtpTo = new Guna2DateTimePicker
            {
                Value = DateTime.Now,
                Format = DateTimePickerFormat.Short,
                Size = new Size(120, 34),
                Location = new Point(552, y),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F)
            };

            btnFilter = new Guna2Button
            {
                Text = "Lọc",
                Size = new Size(72, 34),
                Location = new Point(686, y),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFilter.Click += (s, e) => { _currentPage = 1; ApplyFilters(); };

            // btnAdd — cùng hàng, sát phải
            btnAdd = new Guna2Button
            {
                Text = "+ Tạo Phiếu Trả",
                Size = new Size(148, 34),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdd.Click += BtnAdd_Click;
            // Căn phải động theo chiều rộng panel
            pnlFilters.Resize += (s, e) => { btnAdd.Location = new Point(pnlFilters.Width - 148, y); };

            pnlFilters.Controls.AddRange(new Control[] { lSearch, txtSearch, lFrom, dtpFrom, lTo, dtpTo, btnFilter, btnAdd });
        }

        private void BuildStatusBar()
        {
            // Dùng FlowLayoutPanel để các nút tự động có khoảng cách đều nhau
            pnlStatusBar = new Guna2Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.Transparent };

            var flp = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                AutoSize      = true,
                Location      = new Point(0, 6), // vertical center of 46px
                Padding       = new Padding(0)
            };

            btnAll       = MakeStatusBtn("Tất cả");
            btnPending   = MakeStatusBtn("⏳ Đang xử lý");
            btnApproved  = MakeStatusBtn("✅ Đã duyệt");
            btnCompleted = MakeStatusBtn("💰 Đã hoàn tiền");
            btnRejected  = MakeStatusBtn("❌ Từ chối");

            btnAll.Click       += (s, e) => SetStatus("",             btnAll);
            btnPending.Click   += (s, e) => SetStatus("Đang xử lý",   btnPending);
            btnApproved.Click  += (s, e) => SetStatus("Đã duyệt",     btnApproved);
            btnCompleted.Click += (s, e) => SetStatus("Đã hoàn tiền", btnCompleted);
            btnRejected.Click  += (s, e) => SetStatus("Từ chối",      btnRejected);

            flp.Controls.AddRange(new Control[] { btnAll, btnPending, btnApproved, btnCompleted, btnRejected });
            pnlStatusBar.Controls.Add(flp);
            SetActiveStatusBtn(btnAll);
        }

        private Guna2Button MakeStatusBtn(string text) =>
            new Guna2Button
            {
                Text            = text,
                Size            = new Size(140, 34),
                Margin          = new Padding(0, 0, 8, 0), // khoảng cách phải giữa các nút
                BorderRadius    = 6,
                Font            = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor          = Cursors.Hand,
                BorderThickness = 1
            };

        private void SetStatus(string status, Guna2Button activeBtn)
        {
            _statusFilter = status;
            _currentPage = 1;
            SetActiveStatusBtn(activeBtn);
            ApplyFilters();
        }

        private void SetActiveStatusBtn(Guna2Button active)
        {
            foreach (var btn in new[] { btnAll, btnPending, btnApproved, btnCompleted, btnRejected })
            {
                bool isActive = btn == active;
                btn.FillColor   = isActive ? ThemeManager.ButtonFill    : ThemeManager.CardBackground;
                btn.ForeColor   = isActive ? ThemeManager.ButtonText    : ThemeManager.TextPrimary;
                btn.BorderColor = isActive ? ThemeManager.ButtonFill    : ThemeManager.TextBoxBorder;
            }
        }

        private void BuildKpi()
        {
            // spacerTop: khoảng trống bên dưới StatusBar
            _spacerTop = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = Color.Transparent };
            // spacerBot: khoảng trống bên trên Grid
            _spacerBot = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = Color.Transparent };

            // Giảm KPI height xuống 78px để tiết kiệm thêm không gian
            pnlKpi = new Guna2Panel { Dock = DockStyle.Top, Height = 78, BackColor = Color.Transparent };

            var (cardTotal, valTotal) = MakeKpiCard("Tổng yêu cầu",   Color.FromArgb(52, 152, 219));
            var (cardPend,  valPend)  = MakeKpiCard("Đang xử lý",     Color.FromArgb(243, 156, 18));
            var (cardDone,  valDone)  = MakeKpiCard("Đã hoàn tiền",   Color.FromArgb(46, 204, 113));

            lblKpiTotalValue     = valTotal;
            lblKpiPendingValue   = valPend;
            lblKpiCompletedValue = valDone;

            pnlKpi.Controls.AddRange(new Control[] { cardTotal, cardPend, cardDone });
            pnlKpi.Resize += (s, e) =>
            {
                if (pnlKpi.Width <= 0) return; // Guard
                int w = (pnlKpi.Width - 40) / 3;
                int h = pnlKpi.Height;
                cardTotal.Size = new Size(w, h); cardTotal.Location = new Point(0, 0);
                cardPend.Size  = new Size(w, h); cardPend.Location  = new Point(w + 20, 0);
                cardDone.Size  = new Size(w, h); cardDone.Location  = new Point((w + 20) * 2, 0);
            };
        }

        private (Guna2Panel card, Label value) MakeKpiCard(string title, Color accent)
        {
            var card = new Guna2Panel
            {
                Size = new Size(200, 78), // khớp với pnlKpi.Height mới
                BorderRadius = 10,
                BorderThickness = 1,
                BackColor = Color.Transparent
            };
            var lblT = new Label { Text = title.ToUpper(), Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(16, 12) };
            lblT.Tag = "KpiTitle";
            var lblV = new Label { Text = "0", Font = new Font("Segoe UI", 20F, FontStyle.Bold), AutoSize = true, Location = new Point(14, 30), ForeColor = accent };
            card.Controls.AddRange(new Control[] { lblT, lblV });
            return (card, lblV);
        }

        private void BuildGrid()
        {
            pnlGridContainer = new Guna2Panel { Dock = DockStyle.Fill, BorderRadius = 8, BorderThickness = 1 };

            dgv = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 50 },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle(),
                Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default
            };

            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",         Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReturnCode", HeaderText = "MÃ TRẢ HÀNG",   Width = 120, FillWeight = 9,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "OrderCode",  HeaderText = "MÃ ĐƠN GỐC",    Width = 120, FillWeight = 9,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReturnDate", HeaderText = "NGÀY YÊU CẦU",  Width = 110, FillWeight = 9,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Customer",   HeaderText = "KHÁCH HÀNG",    FillWeight = 22, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount",     HeaderText = "SỐ TIỀN HOÀN",  Width = 130, FillWeight = 10, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight,  Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reason",     HeaderText = "LÝ DO",         FillWeight = 25, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status",     HeaderText = "TRẠNG THÁI",    Width = 130, FillWeight = 10, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Action",     HeaderText = "THAO TÁC",      Width = 90,  FillWeight = 7,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });

            dgv.CellPainting   += Dgv_CellPainting;
            dgv.CellClick      += Dgv_CellClick;
            dgv.CellMouseEnter += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == dgv.Columns["Action"].Index) { hoveredRow = e.RowIndex; dgv.InvalidateCell(e.ColumnIndex, e.RowIndex); } };
            dgv.CellMouseLeave += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == dgv.Columns["Action"].Index) { hoveredRow = -1; dgv.InvalidateCell(e.ColumnIndex, e.RowIndex); } };

            pagination = new PaginationControl { Dock = DockStyle.Bottom };
            pagination.PageChanged += (s, e) => { _currentPage = e.NewPage; UpdateGrid(); };

            pnlGridContainer.Controls.Add(dgv);
            pnlGridContainer.Controls.Add(pagination);

            // Tính PageSize động: mỗi khi grid resize, tính lại số dòng vừa đủ hiển thị
            dgv.SizeChanged += (s, e) =>
            {
                int rowH    = dgv.RowTemplate.Height;
                int headerH = dgv.ColumnHeadersHeight;
                int paginH  = pagination.Height;
                int available = dgv.Height - headerH - paginH;
                int newSize   = Math.Max(1, available / rowH);
                if (newSize != _dynamicPageSize)
                {
                    _dynamicPageSize = newSize;
                    _currentPage = 1;
                    if (_filteredRefunds?.Count > 0) UpdateGrid();
                }
            };
        }

        // ═════════════════════════════════════════════════════════════════════
        // DATA LOADING
        // ═════════════════════════════════════════════════════════════════════
        public void PerformSearch(string keyword)
        {
            _searchKeyword = keyword.ToLower();
            if (txtSearch != null && txtSearch.Text != keyword)
                txtSearch.Text = keyword;
            _currentPage = 1;
            ApplyFilters();
        }

        private async Task LoadDataAsync()
        {
            var storeId = CurrentSession.StoreId ?? 1;
            try
            {
                var result = await _returnRepo.GetAllAsync();
                _allRefunds = result.Where(r => r.StoreId == storeId).ToList();
                ApplyFilters();
                UpdateKpi();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("debug_refund.txt", ex.ToString());
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters()
        {
            var query = _allRefunds.AsQueryable();

            // Keyword
            if (!string.IsNullOrEmpty(_searchKeyword))
            {
                query = query.Where(r =>
                    (r.ReturnCode    != null && r.ReturnCode.ToLower().Contains(_searchKeyword)) ||
                    (r.OrderCode     != null && r.OrderCode.ToLower().Contains(_searchKeyword))  ||
                    (r.CustomerName  != null && r.CustomerName.ToLower().Contains(_searchKeyword)));
            }

            // Status toggle
            if (!string.IsNullOrEmpty(_statusFilter))
                query = query.Where(r => r.ReturnStatus == _statusFilter);

            // Date range
            query = query.Where(r => r.ReturnDate.Date >= dtpFrom.Value.Date && r.ReturnDate.Date <= dtpTo.Value.Date);

            _filteredRefunds = query.OrderByDescending(r => r.ReturnDate).ToList();
            _currentPage = 1;
            UpdateGrid();
        }

        private void UpdateKpi()
        {
            lblKpiTotalValue.Text     = _allRefunds.Count.ToString("N0");
            lblKpiPendingValue.Text   = _allRefunds.Count(r => r.ReturnStatus == "Đang xử lý").ToString("N0");
            lblKpiCompletedValue.Text = _allRefunds.Count(r => r.ReturnStatus == "Đã hoàn tiền").ToString("N0");
        }

        private void UpdateGrid()
        {
            var items = _filteredRefunds
                .Skip((_currentPage - 1) * _dynamicPageSize)
                .Take(_dynamicPageSize)
                .ToList();

            dgv.Rows.Clear();
            foreach (var item in items)
            {
                dgv.Rows.Add(
                    item.Id,
                    item.ReturnCode,
                    item.OrderCode,
                    item.ReturnDate.ToString("dd/MM/yyyy"),
                    item.CustomerName,
                    item.TotalRefundAmount.ToString("N0") + " đ",
                    item.Note,
                    item.ReturnStatus,
                    "" // action placeholder
                );
            }

            pagination.UpdatePagination(_filteredRefunds.Count, _currentPage, _dynamicPageSize);
        }

        // ═════════════════════════════════════════════════════════════════════
        // CELL PAINTING
        // ═════════════════════════════════════════════════════════════════════
        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgv.Columns[e.ColumnIndex].Name == "Status")
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "";

                Color bg, fg;
                if      (status == "Đang xử lý")   { bg = Color.FromArgb(35, 243, 156, 18); fg = Color.FromArgb(200, 120, 0); }
                else if (status == "Đã duyệt")      { bg = Color.FromArgb(35, 41, 128, 185); fg = Color.FromArgb(21, 101, 192); }
                else if (status == "Đã hoàn tiền")  { bg = Color.FromArgb(35, 46, 204, 113); fg = Color.FromArgb(39, 174, 96); }
                else if (status == "Từ chối")        { bg = Color.FromArgb(35, 231, 76,  60); fg = Color.FromArgb(186, 26, 26); }
                else                                 { bg = Color.FromArgb(35, 120, 120, 120); fg = Color.FromArgb(100, 100, 100); }

                bool sel = (e.State & DataGridViewElementStates.Selected) != 0;
                if (sel) fg = Color.White;

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var font = new Font("Segoe UI", 8F, FontStyle.Bold);
                SizeF sz = g.MeasureString(status, font);
                var badgeRect = new RectangleF(
                    e.CellBounds.X + (e.CellBounds.Width  - sz.Width  - 20) / 2f,
                    e.CellBounds.Y + (e.CellBounds.Height - sz.Height -  8) / 2f,
                    sz.Width + 20, sz.Height + 8);

                using (var brush = new SolidBrush(bg))
                    g.FillRoundedRectangle(brush, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height, 10);
                using (var brush = new SolidBrush(fg))
                {
                    var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(status, font, brush, badgeRect, fmt);
                }
                e.Handled = true;
            }
            else if (dgv.Columns[e.ColumnIndex].Name == "Action")
            {
                e.PaintBackground(e.CellBounds, true);

                Rectangle viewRect = new Rectangle(
                    e.CellBounds.X + e.CellBounds.Width / 2 - 26, e.CellBounds.Y + 12, 22, 22);
                Rectangle delRect = new Rectangle(
                    e.CellBounds.X + e.CellBounds.Width / 2 + 4,  e.CellBounds.Y + 12, 22, 22);

                Color viewColor = (hoveredRow == e.RowIndex && viewRect.Contains(dgv.PointToClient(Cursor.Position)))
                    ? ThemeManager.ButtonFill : ThemeManager.TextSecondary;
                Color delColor  = (hoveredRow == e.RowIndex && delRect.Contains(dgv.PointToClient(Cursor.Position)))
                    ? Color.FromArgb(186, 26, 26) : ThemeManager.TextSecondary;

                DrawIcon(e.Graphics, viewRect, "\ue8f4", viewColor);
                DrawIcon(e.Graphics, delRect,  "\ue872", delColor);
                e.Handled = true;
            }
        }

        private static void DrawIcon(Graphics g, Rectangle rect, string iconCode, Color color)
        {
            using var iconFont = new Font("Material Symbols Outlined", 13F, FontStyle.Regular, GraphicsUnit.Point);
            TextRenderer.DrawText(g, iconCode, iconFont, rect, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private async void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgv.Columns[e.ColumnIndex].Name != "Action") return;

            var cellRect = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            Rectangle viewRect = new Rectangle(cellRect.X + cellRect.Width / 2 - 26, cellRect.Y + 12, 22, 22);
            Rectangle delRect  = new Rectangle(cellRect.X + cellRect.Width / 2 + 4,  cellRect.Y + 12, 22, 22);

            Point mousePos = dgv.PointToClient(Cursor.Position);

            int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["Id"].Value);
            var rowData = _filteredRefunds.FirstOrDefault(r => r.Id == id);
            if (rowData == null) return;

            if (viewRect.Contains(mousePos))
            {
                var form = new RefundForm(rowData.Id);
                if (form.ShowDialog() == DialogResult.OK)
                    await LoadDataAsync();
            }
            else if (delRect.Contains(mousePos))
            {
                if (MessageBox.Show($"Bạn có chắc muốn xóa phiếu trả hàng {rowData.ReturnCode}?",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    await _returnRepo.DeleteAsync(rowData.Id);
                    MessageBox.Show("Đã xóa thành công!", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new RefundForm(0);
            if (form.ShowDialog() == DialogResult.OK)
                LoadDataAsync().ConfigureAwait(false);
        }

        // ═════════════════════════════════════════════════════════════════════
        // THEME
        // ═════════════════════════════════════════════════════════════════════
        private void ApplyTheme()
        {
            this.BackColor        = ThemeManager.Background;
            pnlContent.BackColor  = ThemeManager.Background;

            lblTitle.ForeColor    = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;

            btnAdd.FillColor   = ThemeManager.ButtonFill;
            btnAdd.ForeColor   = ThemeManager.ButtonText;
            btnAdd.BorderColor = ThemeManager.ButtonFill;

            // Filter bar
            foreach (Control c in pnlFilters.Controls)
            {
                if (c is Label lbl)                    lbl.ForeColor = ThemeManager.TextSecondary;
                if (c is Guna2TextBox tb)
                {
                    tb.FillColor   = ThemeManager.TextBoxBackground;
                    tb.ForeColor   = ThemeManager.TextPrimary;
                    tb.BorderColor = ThemeManager.TextBoxBorder;
                }
                if (c is Guna2DateTimePicker dtp)
                {
                    dtp.FillColor   = ThemeManager.TextBoxBackground;
                    dtp.ForeColor   = ThemeManager.TextPrimary;
                    dtp.BorderColor = ThemeManager.TextBoxBorder;
                }
            }
            btnFilter.FillColor   = ThemeManager.ButtonFill;
            btnFilter.ForeColor   = ThemeManager.ButtonText;
            btnFilter.BorderColor = ThemeManager.ButtonFill;

            // KPI cards
            foreach (Control c in pnlKpi.Controls)
            {
                if (c is Guna2Panel card)
                {
                    card.FillColor         = ThemeManager.CardBackground;
                    card.CustomBorderColor = ThemeManager.TextBoxBorder;
                    foreach (Control cc in card.Controls)
                        if (cc.Tag?.ToString() == "KpiTitle")
                            cc.ForeColor = ThemeManager.TextSecondary;
                }
            }

            // Grid container
            pnlGridContainer.FillColor         = ThemeManager.CardBackground;
            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            ThemeManager.ApplyDataGridViewStyle(dgv);

            // Active status button reset
            SetActiveStatusBtn(btnAll);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ThemeManager.ThemeChanged -= (s, e) => ApplyTheme();
            base.Dispose(disposing);
        }
    }
}
