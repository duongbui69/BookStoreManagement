using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BookStoreManagement.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using BookStoreManagement.Interfaces;

namespace BookStoreManagement.UserControls
{
    public class StoresControl : UserControl, ISearchableControl
    {
        private StoreRepository _repository;
        private int _currentPage = 1;
        private int _pageSize = 5;
        private string _currentSearchTerm = "";

        private Guna.UI2.WinForms.Guna2Panel pnlContent;
        private Guna.UI2.WinForms.Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        
        private Guna.UI2.WinForms.Guna2Panel pnlFilters;
        private Guna.UI2.WinForms.Guna2TextBox txtSearch;
        private Guna.UI2.WinForms.Guna2Button btnAdd;

        private Guna.UI2.WinForms.Guna2Panel pnlGridContainer;
        private Guna.UI2.WinForms.Guna2DataGridView dgvStores;
        private PaginationControl paginationControl;

        public StoresControl()
        {
            _repository = new StoreRepository();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 20;
            this.Padding = new Padding(0);
            this.AutoScroll = true;

            pnlContent = new Guna.UI2.WinForms.Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = true };

            // 1. Header
            pnlPageHeader = new Guna.UI2.WinForms.Guna2Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(0, 0, 0, gutter) };

            lblTitle = new Label { Text = "Quản lý Chi nhánh", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Quản lý chi nhánh, nhân sự, và trạng thái.", Font = new Font("Segoe UI", 11F), AutoSize = true, Location = new Point(2, 40) };
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            // 2. Filters Bar
            pnlFilters = new Guna.UI2.WinForms.Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0) };
            
            txtSearch = new Guna.UI2.WinForms.Guna2TextBox
            {
                PlaceholderText = "Tìm theo mã, tên chi nhánh...",
                Size = new Size(250, 36),
                Location = new Point(0, 12),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F)
            };
            txtSearch.TextChanged += (s, e) => { PerformSearch(txtSearch.Text); };

            btnAdd = new Guna.UI2.WinForms.Guna2Button { Text = "+ Thêm Chi nhánh", Size = new Size(160, 36), BorderRadius = 6, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.Click += async (s, e) => {
                var frm = new StoreForm(null);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            };

            pnlFilters.Controls.AddRange(new Control[] { txtSearch, btnAdd });
            pnlFilters.Resize += (s, e) =>
            {
                btnAdd.Location = new Point(pnlFilters.Width - 160, 12);
            };

            // 2. Grid Container
            pnlGridContainer = new Guna.UI2.WinForms.Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1, 1, 1, 1), Margin = new Padding(0, 0, 0, gutter), BorderRadius = 6 };
            
            dgvStores = new Guna.UI2.WinForms.Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 60 },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle(), // Keep same as default to avoid zebra striping
                Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default
            };
            
            // Define Columns
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Name = "Id", Visible = false });
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StoreCode", Name = "StoreCode", HeaderText = "MÃ CHI NHÁNH", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 120 });
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StoreName", Name = "StoreName", HeaderText = "TÊN CHI NHÁNH", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Address", Name = "Địa chỉ", HeaderText = "ĐỊA CHỈ", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", Name = "SĐT", HeaderText = "ĐIỆN THOẠI", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 150 });
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ManagerName", Name = "ManagerName", HeaderText = "NGƯỜI QUẢN LÝ", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 180 });
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IsActive", Name = "Trạng thái", HeaderText = "TRẠNG THÁI", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 120 });
            
            DataGridViewTextBoxColumn actionCol = new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "THAO TÁC", Width = 100, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
            dgvStores.Columns.Add(actionCol);

            dgvStores.CellPainting += DgvStores_CellPainting;
            dgvStores.CellMouseClick += DgvStores_CellMouseClick;
            dgvStores.CellMouseMove += DgvStores_CellMouseMove;
            dgvStores.CellMouseLeave += DgvStores_CellMouseLeave;
            
            pnlGridContainer.Controls.Add(dgvStores);

            // Pagination
            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += async (s, e) => { _currentPage = e.NewPage; await LoadDataAsync(); };
            pnlGridContainer.Controls.Add(paginationControl);

            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10 }); // Spacer
            pnlContent.Controls.Add(pnlFilters);
            pnlContent.Controls.Add(pnlPageHeader);

            this.Controls.Add(pnlContent);

            this.Load += StoresControl_Load;

            ApplyTheme();
        }

        private async void StoresControl_Load(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private void LoadData()
        {
            var (items, totalCount) = _repository.GetPagedStores(_currentPage, _pageSize, _currentSearchTerm);
            dgvStores.DataSource = items;
            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            var (items, totalCount) = await _repository.GetPagedStoresAsync(_currentPage, _pageSize, _currentSearchTerm);
            dgvStores.DataSource = items;
            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
        }

        public async void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            await LoadDataAsync();
        }

        // Custom painting logic
        private int _hoveredRow = -1;
        private int _hoveredCol = -1;
        private int _hoveredAction = 0; // 1=edit, 2=delete
        private Point _mouseLocation;

        private void DgvStores_CellMouseMove(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvStores.Columns[e.ColumnIndex].Name == "Actions")
            {
                int action = (e.X < dgvStores.Columns[e.ColumnIndex].Width / 2) ? 1 : 2;
                if (_hoveredRow != e.RowIndex || _hoveredCol != e.ColumnIndex || _hoveredAction != action)
                {
                    int oldRow = _hoveredRow;
                    _hoveredRow = e.RowIndex;
                    _hoveredCol = e.ColumnIndex;
                    _hoveredAction = action;
                    if (oldRow >= 0) dgvStores.InvalidateCell(e.ColumnIndex, oldRow);
                    dgvStores.InvalidateCell(e.ColumnIndex, e.RowIndex);
                }
                dgvStores.Cursor = Cursors.Hand;
            }
            else
            {
                if (_hoveredRow >= 0)
                {
                    int oldRow = _hoveredRow;
                    _hoveredRow = -1;
                    _hoveredCol = -1;
                    _hoveredAction = 0;
                    dgvStores.InvalidateRow(oldRow);
                }
                dgvStores.Cursor = Cursors.Default;
            }
        }

        private void DgvStores_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredRow >= 0)
            {
                int oldRow = _hoveredRow;
                _hoveredRow = -1;
                _hoveredCol = -1;
                _hoveredAction = 0;
                dgvStores.InvalidateRow(oldRow);
            }
            dgvStores.Cursor = Cursors.Default;
        }

        private void DgvStores_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            bool isHoveredRow = (e.RowIndex == _hoveredRow);

            if (e.ColumnIndex == dgvStores.Columns["Trạng thái"].Index)
            {
                e.PaintBackground(e.CellBounds, true);
                if (isHoveredRow)
                {
                    using (var hoverBrush = new SolidBrush(ThemeManager.HoverColor))
                    {
                        e.Graphics.FillRectangle(hoverBrush, e.CellBounds);
                    }
                }

                bool isActive = (bool)e.Value;
                string statusText = isActive ? "Đang hoạt động" : "Đã khóa";
                
                Color badgeBg = isActive ? Color.FromArgb(20, Color.FromArgb(0, 186, 97)) : Color.FromArgb(20, ThemeManager.TextSecondary);
                Color badgeText = isActive ? Color.FromArgb(0, 186, 97) : ThemeManager.TextSecondary;
                Color badgeBorder = isActive ? Color.FromArgb(50, Color.FromArgb(0, 186, 97)) : Color.FromArgb(50, ThemeManager.TextSecondary);

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var font = new Font("Segoe UI", 8F, FontStyle.Bold);
                var textSize = g.MeasureString(statusText, font);
                int badgeWidth = (int)textSize.Width + 24;
                int badgeHeight = 22;
                
                var rect = new Rectangle(
                    e.CellBounds.X + (e.CellBounds.Width - badgeWidth) / 2,
                    e.CellBounds.Y + (e.CellBounds.Height - badgeHeight) / 2,
                    badgeWidth, badgeHeight
                );

                using (var path = new GraphicsPath())
                {
                    int radius = badgeHeight;
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                    path.CloseFigure();

                    using (var brush = new SolidBrush(badgeBg))
                        g.FillPath(brush, path);
                    using (var pen = new Pen(badgeBorder))
                        g.DrawPath(pen, path);
                }

                int dotSize = 6;
                var dotRect = new Rectangle(rect.X + 8, rect.Y + (rect.Height - dotSize) / 2, dotSize, dotSize);
                using (var brush = new SolidBrush(badgeText))
                    g.FillEllipse(brush, dotRect);

                using (var brush = new SolidBrush(badgeText))
                {
                    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    var textRect = new Rectangle(rect.X + 16, rect.Y, rect.Width - 16, rect.Height);
                    g.DrawString(statusText, font, brush, textRect, format);
                }

                e.Handled = true;
            }
            else if (e.ColumnIndex == dgvStores.Columns["Actions"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

                var rect = e.CellBounds;
                var editRect = new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height);
                var delRect = new Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height);

                if (e.RowIndex == _hoveredRow)
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

                int editFontSize = (e.RowIndex == _hoveredRow && _hoveredAction == 1) ? 14 : 12;
                int delFontSize  = (e.RowIndex == _hoveredRow && _hoveredAction == 2) ? 14 : 12;

                using (var editFont = new Font("Segoe UI Emoji", editFontSize))
                    TextRenderer.DrawText(e.Graphics, "✏️", editFont, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);

                using (var delFont = new Font("Segoe UI Emoji", delFontSize))
                    TextRenderer.DrawText(e.Graphics, "🗑️", delFont, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);

                using (var pen = new Pen(Color.LightGray))
                    e.Graphics.DrawLine(pen, rect.X + rect.Width / 2, rect.Y + 8, rect.X + rect.Width / 2, rect.Bottom - 8);

                e.Handled = true;
            }
            else
            {
                e.PaintBackground(e.CellBounds, true);
                if (isHoveredRow)
                {
                    using (var hoverBrush = new SolidBrush(ThemeManager.HoverColor))
                    {
                        e.Graphics.FillRectangle(hoverBrush, e.CellBounds);
                    }
                }
                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
        }

        private async void DgvStores_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dgvStores.Columns["Actions"].Index) return;

            var storeId = (int)dgvStores.Rows[e.RowIndex].Cells["Id"].Value;
            var store = _repository.GetById(storeId);
            if (store == null) return;

            int colWidth = dgvStores.Columns["Actions"].Width;

            if (e.X < colWidth / 2)
            {
                // Edit - left half
                var frm = new StoreForm(store);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            }
            else
            {
                // Delete - right half
                if (MessageBox.Show($"Bạn có chắc muốn xoá cửa hàng '{store.StoreName}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    await _repository.SetActiveAsync(storeId, false);
                    await LoadDataAsync();
                }
            }
        }


        private void ApplyTheme()
        {
            BookStoreManagement.Themes.ThemeManager.ApplyTypography(this);
            this.BackColor = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;
            pnlPageHeader.BackColor = ThemeManager.Background;
            pnlFilters.BackColor = ThemeManager.Background;

            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            
            if (txtSearch != null)
            {
                txtSearch.FillColor = ThemeManager.TextBoxBackground;
                txtSearch.ForeColor = ThemeManager.TextPrimary;
                txtSearch.BorderColor = ThemeManager.TextBoxBorder;
            }

            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;

            pnlGridContainer.BackColor = ThemeManager.Background;
            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;

            ThemeManager.ApplyDataGridViewStyle(dgvStores);
        }
    }
}

namespace BookStoreManagement.UserControls
{
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle bounds, int cornerRadius)
        {
            if (g == null) throw new ArgumentNullException(nameof(g));
            if (brush == null) throw new ArgumentNullException(nameof(brush));

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                g.FillPath(brush, path);
            }
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle bounds, int cornerRadius)
        {
            if (g == null) throw new ArgumentNullException(nameof(g));
            if (pen == null) throw new ArgumentNullException(nameof(pen));

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                g.DrawPath(pen, path);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

