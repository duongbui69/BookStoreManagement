using BookStoreManagement.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.Models;
using BookStoreManagement.Interfaces;
using Guna.UI2.WinForms;
using System.Collections.Generic;

namespace BookStoreManagement.UserControls
{
    public partial class StockCardControl : UserControl, ISearchableControl
    {
        private readonly InventoryTransactionService _service;
        private readonly BookService _bookService;
        private readonly StoreService _storeService;

        private Guna2Panel pnlContent;
        
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        
        private Guna2Panel pnlFilterBar;
        private Guna2DateTimePicker dtpFromDate;
        private Guna2DateTimePicker dtpToDate;
        private Guna2ComboBox cbBookFilter;
        private Guna2ComboBox cbStoreFilter;
        private Guna2Button btnFilter;
        private Guna2Button btnExport;
        private Guna2Button btnPrint;

        private Guna2Panel pnlGridContainer;
        private DataGridView dgvTransactions;
        private PaginationControl paginationControl;

        private int _currentPage = 1;
        private int _pageSize = 5;
        private string _currentSearchTerm = "";

        private int _hoveredRowIndex = -1;
        private int _hoveredAction = 0;

        public StockCardControl()
        {
            _service = new InventoryTransactionService();
            _bookService = new BookService();
            _storeService = new StoreService();

            InitializeUI();
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += StockCardControl_Load;
        }

        public async void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            await LoadDataAsync();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(32, 24, 32, 24);
            
            pnlContent = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Header
            pnlPageHeader = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };

            lblTitle = new Label
            {
                Text = "Thẻ kho",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            lblSubTitle = new Label
            {
                Text = "Lịch sử giao dịch nhập / xuất kho",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(0, 40)
            };

            pnlPageHeader.Controls.Add(lblTitle);
            pnlPageHeader.Controls.Add(lblSubTitle);

            // Filter Bar
            pnlFilterBar = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.Transparent,
                BorderRadius = 8,
                BorderThickness = 1,
                CustomBorderColor = ThemeManager.TextBoxBorder,
                CustomBorderThickness = new Padding(1),
                Padding = new Padding(16)
            };
            pnlFilterBar.Margin = new Padding(0, 16, 0, 16);

            Label lblFrom = new Label { Text = "Từ ngày", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(16, 16) };
            dtpFromDate = new Guna2DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                Size = new Size(130, 36),
                Location = new Point(16, 40),
                BorderRadius = 4,
                BorderThickness = 1
            };

            Label lblTo = new Label { Text = "Đến ngày", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(162, 16) };
            dtpToDate = new Guna2DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now,
                Size = new Size(130, 36),
                Location = new Point(162, 40),
                BorderRadius = 4,
                BorderThickness = 1
            };

            Label lblBook = new Label { Text = "Sản phẩm", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(308, 16) };
            cbBookFilter = new Guna2ComboBox
            {
                Size = new Size(200, 36),
                Location = new Point(308, 40),
                BorderRadius = 4,
                BorderThickness = 1
            };

            Label lblStore = new Label { Text = "Kho", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(524, 16) };
            cbStoreFilter = new Guna2ComboBox
            {
                Size = new Size(180, 36),
                Location = new Point(524, 40),
                BorderRadius = 4,
                BorderThickness = 1
            };

            btnFilter = new Guna2Button
            {
                Text = "Lọc dữ liệu",
                Size = new Size(120, 36),
                Location = new Point(720, 40),
                BorderRadius = 4,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnFilter.Click += async (s, e) => { _currentPage = 1; await LoadDataAsync(); };

            pnlFilterBar.Controls.AddRange(new Control[] { lblFrom, dtpFromDate, lblTo, dtpToDate, lblBook, cbBookFilter, lblStore, cbStoreFilter, btnFilter });

            btnExport = new Guna2Button
            {
                Text = "Xuất Excel",
                Size = new Size(120, 36),
                BorderRadius = 4,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BorderThickness = 1,
                FillColor = Color.Transparent
            };

            btnPrint = new Guna2Button
            {
                Text = "In Thẻ Kho",
                Size = new Size(120, 36),
                BorderRadius = 4,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            // Grid Container
            pnlGridContainer = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 8,
                BorderThickness = 1,
                CustomBorderThickness = new Padding(1)
            };

            dgvTransactions = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = ThemeManager.CardBackground,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 45 },
                EnableHeadersVisualStyles = false,
                ScrollBars = ScrollBars.Both // enable horizontal and vertical scrollbars
            };
            dgvTransactions.SetDoubleBuffered(true);

            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Ngày", Width = 100, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Voucher", HeaderText = "Số chứng từ", Width = 120, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Product", HeaderText = "Tên sản phẩm", Width = 200, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Desc", HeaderText = "Diễn giải", Width = 200, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Import", HeaderText = "Nhập", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Bold) } });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Export", HeaderText = "Xuất", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Bold) } });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn { Name = "Balance", HeaderText = "Tồn", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Bold) } });
            DataGridViewTextBoxColumn actionCol = new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "Thao tác", Width = 100, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
            dgvTransactions.Columns.Add(actionCol);

            dgvTransactions.CellPainting += DgvTransactions_CellPainting;
            dgvTransactions.CellMouseClick += DgvTransactions_CellMouseClick;
            dgvTransactions.CellMouseMove += DgvTransactions_CellMouseMove;
            dgvTransactions.CellMouseLeave += DgvTransactions_CellMouseLeave;
            
            pnlGridContainer.Controls.Add(dgvTransactions);

            // Pagination
            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += async (s, e) => { _currentPage = e.NewPage; await LoadDataAsync(); };
            pnlGridContainer.Controls.Add(paginationControl);

            // Assemble layout
            pnlContent.Controls.Add(pnlGridContainer);
            
            // Container for FilterBar and Top Right Buttons
            Guna2Panel pnlFiltersHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.Transparent };
            pnlFiltersHeader.Controls.Add(pnlFilterBar);
            pnlFilterBar.Location = new Point(0,0);
            pnlFilterBar.Width = 860;
            
            pnlFiltersHeader.Controls.Add(btnExport);
            pnlFiltersHeader.Controls.Add(btnPrint);
            
            pnlContent.Controls.Add(pnlFiltersHeader);
            pnlContent.Controls.Add(pnlPageHeader);

            this.Controls.Add(pnlContent);

            this.Resize += (s, e) =>
            {
                btnExport.Location = new Point(pnlContent.Width - 256, 40);
                btnPrint.Location = new Point(pnlContent.Width - 120, 40);
            };
        }

        private async void StockCardControl_Load(object sender, EventArgs e)
        {
            await LoadFiltersAsync();
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadFiltersAsync()
        {
            var books = await _bookService.GetAllAsync();
            books.Insert(0, new BookStoreManagement.ViewModels.BookListViewModel { Id = 0, Title = "Tất cả sản phẩm" });
            cbBookFilter.DataSource = books;
            cbBookFilter.DisplayMember = "Title";
            cbBookFilter.ValueMember = "Id";

            var stores = await _storeService.GetAllAsync();
            stores.Insert(0, new Store { Id = 0, StoreName = "Tất cả các kho" });
            cbStoreFilter.DataSource = stores;
            cbStoreFilter.DisplayMember = "StoreName";
            cbStoreFilter.ValueMember = "Id";
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            DateTime fromDate = dtpFromDate.Value;
            DateTime toDate = dtpToDate.Value;
            int bookId = cbBookFilter.SelectedValue != null ? Convert.ToInt32(cbBookFilter.SelectedValue) : 0;
            int storeId = cbStoreFilter.SelectedValue != null ? Convert.ToInt32(cbStoreFilter.SelectedValue) : 0;

            var result = await _service.GetPagedTransactionsAsync(_currentPage, _pageSize, fromDate, toDate, bookId, storeId);

            if (this.IsDisposed) return;
            dgvTransactions.Rows.Clear();
            foreach (var item in result.Items)
            {
                int rowIndex = dgvTransactions.Rows.Add(
                    item.Id,
                    item.CreatedAt.ToString("dd/MM/yyyy"),
                    item.VoucherNo,
                    item.ProductName,
                    item.Description,
                    item.ImportQty > 0 ? $"+{item.ImportQty}" : "-",
                    item.ExportQty > 0 ? $"-{item.ExportQty}" : "-",
                    item.Balance,
                    ""
                );
            }

            paginationControl.UpdatePagination(result.TotalCount, _currentPage, _pageSize);
            ApplyThemeToGrid();
        }

        private void DgvTransactions_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvTransactions.Columns[e.ColumnIndex].Name == "Actions")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                var rect = e.CellBounds;
                var editRect = new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height);
                var delRect = new Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height);
                
                if (e.RowIndex == _hoveredRowIndex)
                {
                    if (_hoveredAction == 1)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(30, ThemeManager.ButtonFill)))
                            e.Graphics.FillRectangle(brush, editRect);
                    }
                    else if (_hoveredAction == 2)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(30, Color.FromArgb(231, 76, 60))))
                            e.Graphics.FillRectangle(brush, delRect);
                    }
                }

                int editFontSize = (_hoveredRowIndex == e.RowIndex && _hoveredAction == 1) ? 14 : 12;
                int delFontSize = (_hoveredRowIndex == e.RowIndex && _hoveredAction == 2) ? 14 : 12;

                using (var font = new Font("Segoe UI Emoji", editFontSize))
                {
                TextRenderer.DrawText(e.Graphics, "✏️", font, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                using (var font = new Font("Segoe UI Emoji", delFontSize))
                {
                    TextRenderer.DrawText(e.Graphics, "🗑️", font, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                
                using (var pen = new Pen(Color.LightGray))
                {
                    e.Graphics.DrawLine(pen, rect.X + rect.Width / 2, rect.Y + 8, rect.X + rect.Width / 2, rect.Bottom - 8);
                }
                
                e.Handled = true;
            }
            else if (dgvTransactions.Columns[e.ColumnIndex].Name == "Voucher")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                TextRenderer.DrawText(e.Graphics, e.FormattedValue?.ToString(), e.CellStyle.Font, e.CellBounds, ThemeManager.ButtonFill, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                e.Handled = true;
            }
            else if (dgvTransactions.Columns[e.ColumnIndex].Name == "Import")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                var valStr = e.FormattedValue?.ToString();
                Color color = valStr != "-" ? Color.Green : ThemeManager.TextPrimary;
                TextRenderer.DrawText(e.Graphics, valStr, e.CellStyle.Font, e.CellBounds, color, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                e.Handled = true;
            }
            else if (dgvTransactions.Columns[e.ColumnIndex].Name == "Export")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                var valStr = e.FormattedValue?.ToString();
                Color color = valStr != "-" ? Color.Red : ThemeManager.TextPrimary;
                TextRenderer.DrawText(e.Graphics, valStr, e.CellStyle.Font, e.CellBounds, color, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                e.Handled = true;
            }
        }

        private void DgvTransactions_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvTransactions.Columns[e.ColumnIndex].Name == "Actions")
            {
                int action = (e.X < dgvTransactions.Columns[e.ColumnIndex].Width / 2) ? 1 : 2;
                
                if (_hoveredRowIndex != e.RowIndex || _hoveredAction != action)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = e.RowIndex;
                    _hoveredAction = action;
                    
                    if (oldRow >= 0) dgvTransactions.InvalidateCell(e.ColumnIndex, oldRow);
                    dgvTransactions.InvalidateCell(e.ColumnIndex, _hoveredRowIndex);
                }
                dgvTransactions.Cursor = Cursors.Hand;
            }
            else
            {
                if (_hoveredRowIndex >= 0)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = -1;
                    _hoveredAction = 0;
                    if (e.ColumnIndex >= 0) dgvTransactions.InvalidateCell(dgvTransactions.Columns["Actions"].Index, oldRow);
                }
                dgvTransactions.Cursor = Cursors.Default;
            }
        }

        private void DgvTransactions_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredRowIndex >= 0)
            {
                int oldRow = _hoveredRowIndex;
                _hoveredRowIndex = -1;
                _hoveredAction = 0;
                dgvTransactions.InvalidateCell(dgvTransactions.Columns["Actions"].Index, oldRow);
            }
            dgvTransactions.Cursor = Cursors.Default;
        }

        private async void DgvTransactions_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvTransactions.Columns[e.ColumnIndex].Name == "Actions")
            {
                int transactionId = Convert.ToInt32(dgvTransactions.Rows[e.RowIndex].Cells["Id"].Value);
                
                if (e.X < dgvTransactions.Columns[e.ColumnIndex].Width / 2)
                {
                    // Edit
                    var trans = _service.GetById(transactionId);
                    if (trans != null)
                    {
                        // Open a simple edit note dialog
                        string newNote = Microsoft.VisualBasic.Interaction.InputBox("Chỉnh sửa ghi chú giao dịch:", "Sửa Giao Dịch", trans.Note ?? "");
                        if (!string.IsNullOrWhiteSpace(newNote))
                        {
                            if (_service.UpdateNote(transactionId, newNote))
                            {
                                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await LoadDataAsync();
                            }
                            else
                            {
                                MessageBox.Show("Cập nhật thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                else
                {
                    // Delete
                    if (MessageBox.Show("Bạn có chắc chắn muốn xóa giao dịch này? Hành động này có thể ảnh hưởng đến số dư thẻ kho.", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        if (_service.Delete(transactionId))
                        {
                            MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadDataAsync();
                        }
                        else
                        {
                            MessageBox.Show("Xóa thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            
            pnlFilterBar.BackColor = ThemeManager.CardBackground;
            pnlFilterBar.CustomBorderColor = ThemeManager.TextBoxBorder;

            foreach (Control c in pnlFilterBar.Controls)
            {
                if (c is Label lbl) lbl.ForeColor = ThemeManager.TextSecondary;
                if (c is Guna2DateTimePicker dtp)
                {
                    dtp.FillColor = ThemeManager.CardBackground;
                    dtp.ForeColor = ThemeManager.TextPrimary;
                    dtp.BorderColor = ThemeManager.TextBoxBorder;
                }
                if (c is Guna2ComboBox cb)
                {
                    cb.FillColor = ThemeManager.CardBackground;
                    cb.ForeColor = ThemeManager.TextPrimary;
                    cb.BorderColor = ThemeManager.TextBoxBorder;
                }
            }

            btnFilter.FillColor = ThemeManager.CardBackground;
            btnFilter.ForeColor = ThemeManager.TextPrimary;
            btnFilter.BorderColor = ThemeManager.TextBoxBorder;
            btnFilter.BorderThickness = 1;

            btnExport.BorderColor = ThemeManager.TextBoxBorder;
            btnExport.ForeColor = ThemeManager.TextPrimary;

            btnPrint.FillColor = ThemeManager.ButtonFill;
            btnPrint.ForeColor = ThemeManager.ButtonText;

            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            pnlGridContainer.BackColor = ThemeManager.CardBackground;

            ApplyThemeToGrid();
        }

        private void ApplyThemeToGrid()
        {
            if (dgvTransactions == null) return;
            
            dgvTransactions.BackgroundColor = ThemeManager.CardBackground;
            dgvTransactions.GridColor = ThemeManager.TextBoxBorder;
            
            dgvTransactions.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvTransactions.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvTransactions.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Background;
            dgvTransactions.ColumnHeadersDefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvTransactions.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            dgvTransactions.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvTransactions.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvTransactions.DefaultCellStyle.SelectionBackColor = ThemeManager.CardBackground;
            dgvTransactions.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvTransactions.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);

            foreach (DataGridViewColumn col in dgvTransactions.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }
    }
}