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
        private TableLayoutPanel tlpHeader;
        private Label lblTitle;
        private Label lblSubTitle;
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

            // 1. Header & Toolbar
            tlpHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 2,
                Height = 80,
                Margin = new Padding(0, 0, 0, gutter)
            };
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            lblTitle = new Label { Text = "Store Management", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Margin = new Padding(0) };
            lblSubTitle = new Label { Text = "Manage bookstore locations, personnel, and operational status.", Font = new Font("Segoe UI", 10F), AutoSize = true, Margin = new Padding(2, 0, 0, 0) };
            
            btnAdd = new Guna.UI2.WinForms.Guna2Button { Text = "+ Add Store", Size = new Size(160, 40), BorderRadius = 4, Font = new Font("Inter", 10, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 10, 0, 0) };
            btnAdd.Click += async (s, e) => {
                var frm = new StoreForm(null);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            };

            tlpHeader.Controls.Add(lblTitle, 0, 0);
            tlpHeader.Controls.Add(lblSubTitle, 0, 1);
            tlpHeader.Controls.Add(btnAdd, 1, 0);
            tlpHeader.SetRowSpan(btnAdd, 2);

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
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StoreCode", Name = "StoreCode", HeaderText = "STORE ID", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 120 });
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StoreName", Name = "StoreName", HeaderText = "STORE NAME", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Address", Name = "Address", HeaderText = "ADDRESS", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", Name = "Phone", HeaderText = "PHONE", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 150 });
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ManagerName", Name = "ManagerName", HeaderText = "MANAGER", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 180 });
            dgvStores.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IsActive", Name = "Status", HeaderText = "STATUS", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 120 });
            
            DataGridViewTextBoxColumn actionCol = new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "ACTION", Width = 100, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
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
            pnlContent.Controls.Add(tlpHeader);

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
        private Point _mouseLocation;

        private void DgvStores_CellMouseMove(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (_hoveredRow != e.RowIndex || _hoveredCol != e.ColumnIndex)
                {
                    _hoveredRow = e.RowIndex;
                    _hoveredCol = e.ColumnIndex;
                    _mouseLocation = e.Location;
                    dgvStores.InvalidateRow(e.RowIndex);
                }
                else if (e.ColumnIndex == dgvStores.Columns["Actions"].Index)
                {
                    _mouseLocation = e.Location;
                    dgvStores.InvalidateCell(e.ColumnIndex, e.RowIndex);
                }
            }
        }

        private void DgvStores_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredRow >= 0)
            {
                int oldRow = _hoveredRow;
                _hoveredRow = -1;
                _hoveredCol = -1;
                dgvStores.InvalidateRow(oldRow);
            }
        }

        private void DgvStores_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            bool isHoveredRow = (e.RowIndex == _hoveredRow);

            if (e.ColumnIndex == dgvStores.Columns["Status"].Index)
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
                string statusText = isActive ? "Active" : "Locked";
                
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
                e.PaintBackground(e.CellBounds, true);
                if (isHoveredRow)
                {
                    using (var hoverBrush = new SolidBrush(ThemeManager.HoverColor))
                    {
                        e.Graphics.FillRectangle(hoverBrush, e.CellBounds);
                    }
                }

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int iconSize = 32;
                int gap = 8;
                int totalWidth = (iconSize * 2) + gap;
                
                var editRect = new Rectangle(
                    e.CellBounds.X + (e.CellBounds.Width - totalWidth) / 2,
                    e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2,
                    iconSize, iconSize
                );
                
                var deleteRect = new Rectangle(
                    editRect.Right + gap,
                    editRect.Y,
                    iconSize, iconSize
                );

                bool isEditHovered = isHoveredRow && _hoveredCol == e.ColumnIndex && editRect.Contains(e.CellBounds.X + _mouseLocation.X, e.CellBounds.Y + _mouseLocation.Y);
                bool isDeleteHovered = isHoveredRow && _hoveredCol == e.ColumnIndex && deleteRect.Contains(e.CellBounds.X + _mouseLocation.X, e.CellBounds.Y + _mouseLocation.Y);

                if (isEditHovered)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(20, Color.FromArgb(0, 36, 64))))
                        g.FillRoundedRectangle(brush, editRect, 4);
                    using (var pen = new Pen(Color.FromArgb(50, Color.FromArgb(0, 36, 64))))
                        g.DrawRoundedRectangle(pen, editRect, 4);
                }
                
                if (isDeleteHovered)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(20, Color.FromArgb(186, 26, 26))))
                        g.FillRoundedRectangle(brush, deleteRect, 4);
                    using (var pen = new Pen(Color.FromArgb(50, Color.FromArgb(186, 26, 26))))
                        g.DrawRoundedRectangle(pen, deleteRect, 4);
                }

                using (var font = new Font("Segoe UI Emoji", 12F))
                {
                    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    
                    using (var brush = new SolidBrush(isEditHovered ? Color.FromArgb(0, 36, 64) : ThemeManager.TextSecondary))
                        g.DrawString("✏️", font, brush, editRect, format);
                        
                    using (var brush = new SolidBrush(isDeleteHovered ? Color.FromArgb(186, 26, 26) : ThemeManager.TextSecondary))
                        g.DrawString("🗑️", font, brush, deleteRect, format);
                }

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

            int iconSize = 32;
            int gap = 8;
            int totalWidth = (iconSize * 2) + gap;
            
            var editRect = new Rectangle(
                (dgvStores.Columns["Actions"].Width - totalWidth) / 2,
                (dgvStores.RowTemplate.Height - iconSize) / 2,
                iconSize, iconSize
            );
            
            var deleteRect = new Rectangle(
                editRect.Right + gap,
                editRect.Y,
                iconSize, iconSize
            );

            if (editRect.Contains(e.Location))
            {
                var frm = new StoreForm(store);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            }
            else if (deleteRect.Contains(e.Location))
            {
                if (MessageBox.Show($"Bạn có chắc muốn xoá cửa hàng '{store.StoreName}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    await _repository.SetActiveAsync(storeId, false);
                    await LoadDataAsync();
                }
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;
            tlpHeader.BackColor = ThemeManager.Background;

            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;

            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;

            pnlGridContainer.BackColor = ThemeManager.Background;
            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;

            dgvStores.BackgroundColor = ThemeManager.Background;
            dgvStores.GridColor = ThemeManager.TextBoxBorder;
            dgvStores.DefaultCellStyle.BackColor = ThemeManager.Background;
            dgvStores.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvStores.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvStores.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;

            dgvStores.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvStores.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvStores.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.CardBackground;
            dgvStores.ColumnHeadersHeight = 45;
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
