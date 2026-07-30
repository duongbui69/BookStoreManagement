using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Interfaces;
using BookStoreManagement.Repositories;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;

namespace BookStoreManagement.UserControls
{
    public class InventoryLedgerControl : UserControl, ISearchableControl
    {
        // ─── Services ────────────────────────────────────────────────────────
        private readonly InventoryTransactionService _service;
        private readonly BookService _bookService;
        private readonly StoreService _storeService;

        // ─── State ───────────────────────────────────────────────────────────
        private int _currentPage = 1;
        private const int PageSize = 12; // fallback — được ghi đè bởi _dynamicPageSize
        private int _dynamicPageSize = 12;
        private string _referenceType = "";   // "" = All, "PurchaseReceipt", "SalesOrder", "ReturnReceipt"
        private int _bookId = 0;
        private int _storeId = 0;

        // ─── Layout panels ────────────────────────────────────────────────────
        private Guna2Panel pnlContent;
        private Guna2Panel pnlHeader;
        private Guna2Panel pnlFilters;
        private Guna2Panel pnlTypeBar;
        private Guna2Panel pnlKpi;
        private Panel _spacer;
        private Guna2Panel pnlGridContainer;

        // ─── Header ───────────────────────────────────────────────────────────
        private Label lblTitle;
        private Label lblSubTitle;
        private Guna2Button btnExport;

        // ─── Filters ──────────────────────────────────────────────────────────
        private Guna2DateTimePicker dtpFrom;
        private Guna2DateTimePicker dtpTo;
        private Guna2ComboBox cbBook;
        private Guna2ComboBox cbStore;
        private Guna2Button btnFilter;

        // ─── Type toggle buttons ──────────────────────────────────────────────
        private Guna2Button btnAll;
        private Guna2Button btnIn;
        private Guna2Button btnOut;
        private Guna2Button btnReturn;

        // ─── KPI cards ────────────────────────────────────────────────────────
        private Label lblKpiInValue;
        private Label lblKpiOutValue;
        private Label lblKpiBalValue;

        // ─── Grid ─────────────────────────────────────────────────────────────
        private Guna2DataGridView dgv;
        private PaginationControl pagination;

        // ─── ctor ─────────────────────────────────────────────────────────────
        public InventoryLedgerControl()
        {
            _service   = new InventoryTransactionService();
            _bookService = new BookService();
            _storeService = new StoreService();

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

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 8) };
            this.Controls.Add(pnlContent);

            BuildHeader();
            BuildFilters();
            BuildTypeBar();
            BuildKpi();
            BuildGrid();

            var spacer2 = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Color.Transparent };
            var spacer3 = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.Transparent };
            
            pnlContent.Controls.AddRange(new Control[] {
                pnlHeader, pnlFilters, pnlTypeBar, _spacer, pnlKpi, pnlGridContainer, spacer2, spacer3
            });

            // Sắp xếp Z-order chuẩn bằng BringToFront() theo thứ tự từ trên xuống dưới
            pnlHeader.BringToFront();
            _spacer.BringToFront();
            pnlKpi.BringToFront();
            spacer2.BringToFront();
            pnlTypeBar.BringToFront();
            pnlFilters.BringToFront();
            spacer3.BringToFront();
            pnlGridContainer.BringToFront();
        }

        private void BuildHeader()
        {
            // Giảm chiều cao xuống 60px để cho grid thêm không gian
            pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };

            lblTitle = new Label
            {
                Text = "Sổ kho",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            lblSubTitle = new Label
            {
                Text = "Nhật ký toàn bộ biến động nhập / xuất tồn kho",
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(2, 44)
            };
            btnExport = new Guna2Button
            {
                Text = "Xuất Excel",
                Size = new Size(110, 36),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExport.Click += BtnExport_Click;

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });
        }

        private void BuildFilters()
        {
            // Giảm chiều cao xuống 50px
            pnlFilters = new Guna2Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };

            int y = 8;
            Label lFrom  = new Label { Text = "Từ ngày",  AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(0, y + 7) };
            dtpFrom = new Guna2DateTimePicker { Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), Format = DateTimePickerFormat.Short, Size = new Size(120, 34), Location = new Point(58, y), BorderRadius = 6, Font = new Font("Segoe UI", 9F) };

            Label lTo    = new Label { Text = "Đến ngày", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(192, y + 7) };
            dtpTo   = new Guna2DateTimePicker { Value = DateTime.Now, Format = DateTimePickerFormat.Short, Size = new Size(120, 34), Location = new Point(260, y), BorderRadius = 6, Font = new Font("Segoe UI", 9F) };

            Label lBook  = new Label { Text = "Sản phẩm", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(394, y + 7) };
            cbBook  = new Guna2ComboBox { Size = new Size(175, 34), Location = new Point(462, y), BorderRadius = 6, Font = new Font("Segoe UI", 9F), DropDownStyle = ComboBoxStyle.DropDownList };

            Label lStore = new Label { Text = "Kho", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(650, y + 7) };
            cbStore = new Guna2ComboBox { Size = new Size(145, 34), Location = new Point(680, y), BorderRadius = 6, Font = new Font("Segoe UI", 9F), DropDownStyle = ComboBoxStyle.DropDownList };

            btnFilter = new Guna2Button { Text = "Lọc", Size = new Size(72, 34), Location = new Point(836, y), BorderRadius = 6, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnFilter.Click += async (s, e) => { _currentPage = 1; await LoadDataAsync(); };

            pnlFilters.Controls.AddRange(new Control[] { lFrom, dtpFrom, lTo, dtpTo, lBook, cbBook, lStore, cbStore, btnFilter, btnExport });
            btnExport.Location = new Point(920, y);
            btnExport.Size     = new Size(100, 34);
        }

        private void BuildTypeBar()
        {
            // Giảm chiều cao xuống 44px
            pnlTypeBar = new Guna2Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.Transparent };

            btnAll    = MakeTypeBtn("Tất cả",     0);
            btnIn     = MakeTypeBtn("📥 Nhập mua",  118);
            btnOut    = MakeTypeBtn("📤 Xuất bán",  238);
            btnReturn = MakeTypeBtn("↩ Trả hàng",  358);

            btnAll.Click    += async (s, e) => { _referenceType = "";               _currentPage = 1; SetActiveTypeBtn(btnAll);    await LoadDataAsync(); };
            btnIn.Click     += async (s, e) => { _referenceType = "PurchaseReceipt"; _currentPage = 1; SetActiveTypeBtn(btnIn);     await LoadDataAsync(); };
            btnOut.Click    += async (s, e) => { _referenceType = "SalesOrder";      _currentPage = 1; SetActiveTypeBtn(btnOut);    await LoadDataAsync(); };
            btnReturn.Click += async (s, e) => { _referenceType = "ReturnReceipt";   _currentPage = 1; SetActiveTypeBtn(btnReturn); await LoadDataAsync(); };

            pnlTypeBar.Controls.AddRange(new Control[] { btnAll, btnIn, btnOut, btnReturn });
            SetActiveTypeBtn(btnAll);
        }

        private Guna2Button MakeTypeBtn(string text, int x) =>
            new Guna2Button
            {
                Text = text,
                Size = new Size(110, 34),
                Location = new Point(x, 7),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BorderThickness = 1
            };

        private void SetActiveTypeBtn(Guna2Button active)
        {
            foreach (var btn in new[] { btnAll, btnIn, btnOut, btnReturn })
            {
                bool isActive = btn == active;
                btn.FillColor   = isActive ? ThemeManager.ButtonFill : ThemeManager.CardBackground;
                btn.ForeColor   = isActive ? ThemeManager.ButtonText  : ThemeManager.TextPrimary;
                btn.BorderColor = isActive ? ThemeManager.ButtonFill  : ThemeManager.TextBoxBorder;
            }
        }

        private void BuildKpi()
        {
            // Giảm spacer và KPI height để giải phóng không gian cho grid
            _spacer = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.Transparent };
            pnlKpi = new Guna2Panel { Dock = DockStyle.Top, Height = 82, BackColor = Color.Transparent };

            var (cardIn,  valIn)  = MakeKpiCard("Tổng nhập kỳ", Color.FromArgb(46, 204, 113),  0);
            var (cardOut, valOut) = MakeKpiCard("Tổng xuất kỳ", Color.FromArgb(231, 76,  60),  220);
            var (cardBal, valBal) = MakeKpiCard("Tồn cuối kỳ",  Color.FromArgb(52,  152, 219), 440);

            lblKpiInValue  = valIn;
            lblKpiOutValue = valOut;
            lblKpiBalValue = valBal;

            pnlKpi.Controls.AddRange(new Control[] { cardIn, cardOut, cardBal });
            pnlKpi.Resize += (s, e) =>
            {
                if (pnlKpi.Width <= 0) return; // Guard: tránh set size âm khi chưa layout
                int w = (pnlKpi.Width - 40) / 3;
                int h = pnlKpi.Height;
                cardIn.Size  = new Size(w, h); cardIn.Location = new Point(0, 0);
                cardOut.Size = new Size(w, h); cardOut.Location = new Point(w + 20, 0);
                cardBal.Size = new Size(w, h); cardBal.Location = new Point((w + 20) * 2, 0);
            };
        }

        private (Guna2Panel card, Label value) MakeKpiCard(string title, Color accent, int x)
        {
            var card = new Guna2Panel
            {
                Size = new Size(200, 82), // khớp với pnlKpi.Height mới
                Location = new Point(x, 0),
                BorderRadius = 10,
                BorderThickness = 1,
                BackColor = Color.Transparent
            };
            var lblT = new Label { Text = title.ToUpper(), Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(16, 14) };
            lblT.Tag = "KpiTitle";
            var lblV = new Label { Text = "—", Font = new Font("Segoe UI", 22F, FontStyle.Bold), AutoSize = true, Location = new Point(14, 34), ForeColor = accent };
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

            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",          Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ngày",        HeaderText = "NGÀY",       Width = 100, FillWeight = 8,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoPhieu",     HeaderText = "SỐ PHIẾU",  Width = 130, FillWeight = 10, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Loai",        HeaderText = "LOẠI",       Width = 110, FillWeight = 9,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenSach",     HeaderText = "TÊN SÁCH",   FillWeight = 30, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "DienGiai",    HeaderText = "DIỄN GIẢI", FillWeight = 20, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nhap",        HeaderText = "NHẬP",      Width = 70,  FillWeight = 6,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Xuat",        HeaderText = "XUẤT",      Width = 70,  FillWeight = 6,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ton",         HeaderText = "TỒN",       Width = 80,  FillWeight = 7,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "RefType",     Visible = false });

            dgv.CellPainting += Dgv_CellPainting;

            pagination = new PaginationControl { Dock = DockStyle.Bottom };
            pagination.PageChanged += async (s, e) => { _currentPage = e.NewPage; await LoadDataAsync(); };

            pnlGridContainer.Controls.Add(dgv);
            pnlGridContainer.Controls.Add(pagination);

            // Tính PageSize động: mỗi khi grid resize, tính lại số dòng vừa đủ hiển thị
            System.Windows.Forms.Timer resizeTimer = new System.Windows.Forms.Timer { Interval = 150 };
            resizeTimer.Tick += async (senderTimer, eTimer) =>
            {
                resizeTimer.Stop();
                if (this.IsDisposed) return;

                int rowH    = dgv.RowTemplate.Height; // 50
                int headerH = dgv.ColumnHeadersHeight; // ~30
                int paginH  = pagination.Height;      // 40
                int available = dgv.Height - headerH - paginH;
                int newSize   = Math.Max(1, available / rowH);
                if (newSize != _dynamicPageSize)
                {
                    _dynamicPageSize = newSize;
                    _currentPage = 1;
                    await LoadDataAsync();
                }
            };

            dgv.SizeChanged += (s, e) =>
            {
                resizeTimer.Stop();
                resizeTimer.Start();
            };
        }

        // ═════════════════════════════════════════════════════════════════════
        // DATA LOADING
        // ═════════════════════════════════════════════════════════════════════
        public async void PerformSearch(string keyword) { /* Sổ kho dùng filter UI, không dùng global search */ }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            if (this.IsDisposed) return;

            // Populate combos lazily on first load
            if (cbBook.Items.Count == 0)  await LoadFiltersAsync();

            int bookId  = cbBook.SelectedValue  is int bId  && bId  > 0 ? bId  : 0;
            int storeId = cbStore.SelectedValue is int sId  && sId  > 0 ? sId  : 0;
            DateTime from = dtpFrom.Value.Date;
            DateTime to   = dtpTo.Value.Date;

            _bookId  = bookId;
            _storeId = storeId;

            // Load KPI summary
            var summary = await _service.GetLedgerSummaryAsync(from, to, bookId, storeId, _referenceType);
            if (this.IsDisposed) return;

            lblKpiInValue.Text  = summary.TotalIn.ToString("N0");
            lblKpiOutValue.Text = summary.TotalOut.ToString("N0");
            lblKpiBalValue.Text = summary.EndBalance.ToString("N0");

            // Load paged data — dùng _dynamicPageSize thay cho hằng số PageSize
            var (items, total) = await _service.GetPagedTransactionsAsync(_currentPage, _dynamicPageSize, from, to, bookId, storeId, _referenceType);
            if (this.IsDisposed) return;

            dgv.Rows.Clear();
            foreach (var item in items)
            {
                dgv.Rows.Add(
                    item.Id,
                    item.CreatedAt.ToString("dd/MM/yyyy"),
                    item.VoucherNo,
                    GetVietLabel(item.ReferenceType),   // Loại
                    item.ProductName,
                    item.Description,
                    item.ImportQty > 0 ? $"+{item.ImportQty}" : "-",
                    item.ExportQty > 0 ? $"-{item.ExportQty}" : "-",
                    item.Balance,
                    item.ReferenceType
                );
            }

            pagination.UpdatePagination(total, _currentPage, _dynamicPageSize);
        }

        private async System.Threading.Tasks.Task LoadFiltersAsync()
        {
            // Books
            var books = await _bookService.GetActiveAsync();
            cbBook.Items.Clear();
            cbBook.Items.Add(new { Id = 0, Title = "Tất cả sản phẩm" });
            foreach (var b in books) cbBook.Items.Add(new { b.Id, b.Title });
            cbBook.DisplayMember = "Title";
            cbBook.ValueMember   = "Id";
            cbBook.SelectedIndex = 0;

            // Stores
            var stores = _storeService.GetActive();
            cbStore.Items.Clear();
            cbStore.Items.Add(new { Id = 0, StoreName = "Tất cả kho" });
            foreach (var s in stores) cbStore.Items.Add(new { s.Id, s.StoreName });
            cbStore.DisplayMember = "StoreName";
            cbStore.ValueMember   = "Id";
            cbStore.SelectedIndex = 0;
        }

        private static string GetVietLabel(string refType) => refType switch
        {
            "PurchaseReceipt" => "Nhập mua",
            "SalesOrder"      => "Xuất bán",
            "ReturnReceipt"   => "Trả hàng",
            _                 => refType
        };

        // ═════════════════════════════════════════════════════════════════════
        // CELL PAINTING — badge màu cột Loại
        // ═════════════════════════════════════════════════════════════════════
        private void Dgv_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex].Name != "Loai") return;

            e.PaintBackground(e.CellBounds, true);

            string label   = e.Value?.ToString() ?? "";
            string refType = dgv.Rows[e.RowIndex].Cells["RefType"].Value?.ToString() ?? "";

            Color bg, fg;
            switch (refType)
            {
                case "PurchaseReceipt":
                    bg = Color.FromArgb(35, 46, 204, 113); fg = Color.FromArgb(46, 204, 113); break;
                case "SalesOrder":
                    bg = Color.FromArgb(35, 231, 76, 60);  fg = Color.FromArgb(231, 76, 60);  break;
                case "ReturnReceipt":
                    bg = Color.FromArgb(35, 243, 156, 18); fg = Color.FromArgb(243, 156, 18); break;
                default:
                    bg = Color.FromArgb(35, 41, 128, 185); fg = Color.FromArgb(41, 128, 185); break;
            }

            bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;
            if (isSelected) fg = Color.White;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var font = new Font("Segoe UI", 8F, FontStyle.Bold);
            SizeF sz = g.MeasureString(label, font);
            var badgeRect = new RectangleF(
                e.CellBounds.X + (e.CellBounds.Width  - sz.Width  - 20) / 2f,
                e.CellBounds.Y + (e.CellBounds.Height - sz.Height - 8)  / 2f,
                sz.Width + 20, sz.Height + 8);

            using (var brush = new SolidBrush(bg))
                g.FillRoundedRectangle(brush, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height, 10);

            using (var brush = new SolidBrush(fg))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(label, font, brush, badgeRect, fmt);
            }

            e.Handled = true;
        }

        // ═════════════════════════════════════════════════════════════════════
        // EXPORT
        // ═════════════════════════════════════════════════════════════════════
        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                using var sfd = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = "SoKho.xlsx" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var excelService = new ExcelExportService();
                    excelService.ExportDataGridView(dgv, sfd.FileName, "Sổ kho");
                    MessageBox.Show("Xuất file Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // THEME
        // ═════════════════════════════════════════════════════════════════════
        private void ApplyTheme()
        {
            this.BackColor    = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;

            lblTitle.ForeColor    = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;

            btnExport.FillColor   = ThemeManager.CardBackground;
            btnExport.ForeColor   = ThemeManager.TextPrimary;
            btnExport.BorderColor = ThemeManager.TextBoxBorder;

            // Filter labels
            foreach (Control c in pnlFilters.Controls)
            {
                if (c is Label lbl) lbl.ForeColor = ThemeManager.TextSecondary;
                if (c is Guna2ComboBox cb)
                {
                    cb.FillColor   = ThemeManager.TextBoxBackground;
                    cb.ForeColor   = ThemeManager.TextPrimary;
                    cb.BorderColor = ThemeManager.TextBoxBorder;
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

            // Grid
            pnlGridContainer.FillColor         = ThemeManager.CardBackground;
            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            ThemeManager.ApplyDataGridViewStyle(dgv);

            // Active type button
            SetActiveTypeBtn(btnAll);
        }
    }
}
