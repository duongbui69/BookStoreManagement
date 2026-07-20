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
using System.Linq;

namespace BookStoreManagement.UserControls
{
    public class VoucherTypeControl : UserControl, ISearchableControl
    {
        private readonly VoucherTypeService _service;
        private List<VoucherType> _allVouchers = new();

        // Headers
        private Label lblTitle;
        private Label lblSubTitle;
        
        // Toolbar
        private Guna2Panel pnlToolbar;
        private Guna2TextBox txtSearch;
        private Guna2Button btnExport;
        private Guna2Button btnAdd;

        // Grid
        private Guna2Panel pnlGridContainer;
        private DataGridView dgvData;
        private PaginationControl pagination;
        private int _currentPage = 1;
        private const int PageSize = 5;

        private int _hoveredRowIndex = -1;

        public VoucherTypeControl()
        {
            _service = new VoucherTypeService();
            InitializeComponent();
            ApplyTheme();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24);
            this.AutoScroll = true;

            // Header Region
            Guna2Panel pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.Transparent };
            
            lblTitle = new Label
            {
                Text = "Loại phiếu",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            lblSubTitle = new Label
            {
                Text = "Admin manages document types for accounting/inventory.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(0, 35)
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            // Main Card Container
            Guna2Panel pnlMainCard = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 8,
                BorderThickness = 1,
                Padding = new Padding(0)
            };

            // Toolbar
            pnlToolbar = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                CustomizableEdges = new Guna.UI2.WinForms.Suite.CustomizableEdges(true, true, false, false),
                Padding = new Padding(24, 12, 24, 12)
            };
            
            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Tìm loại phiếu...",
                Size = new Size(250, 36),
                Location = new Point(24, 12),
                BorderRadius = 8
            };
            txtSearch.TextChanged += (s, e) => ApplyFilters();

            btnExport = new Guna2Button
            {
                Text = "Xuất Excel",
                Size = new Size(120, 36),
                BorderRadius = 8,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            
            btnAdd = new Guna2Button
            {
                Text = "+ Thêm mới",
                Size = new Size(120, 36),
                BorderRadius = 8,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnAdd.Click += (s, e) => ShowEditForm(null);

            // Add dynamic resize logic for toolbar buttons
            pnlToolbar.Resize += (s, e) => 
            {
                btnAdd.Location = new Point(pnlToolbar.Width - btnAdd.Width - 24, 12);
                btnExport.Location = new Point(btnAdd.Left - btnExport.Width - 10, 12);
            };

            pnlToolbar.Controls.AddRange(new Control[] { txtSearch, btnExport, btnAdd });

            // Grid Container
            pnlGridContainer = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(1)
            };

            dgvData = new DataGridView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 45 },
                EnableHeadersVisualStyles = false,
                Cursor = Cursors.Hand
            };
            dgvData.SetDoubleBuffered(true);

            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã loại", Width = 100 });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên loại phiếu", Width = 200 });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Mô tả", Width = 300 });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "GroupType", HeaderText = "Nhóm", Width = 120 });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng thái", Width = 120 });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "Action", HeaderText = "Thao tác", Width = 100, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });

            foreach (DataGridViewColumn col in dgvData.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            dgvData.Columns["Name"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvData.Columns["Description"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgvData.CellMouseEnter += (s, e) => { if (e.RowIndex >= 0) { _hoveredRowIndex = e.RowIndex; dgvData.InvalidateRow(e.RowIndex); } };
            dgvData.CellMouseLeave += (s, e) => { _hoveredRowIndex = -1; if (e.RowIndex >= 0) dgvData.InvalidateRow(e.RowIndex); };
            dgvData.CellPainting += DgvData_CellPainting;
            dgvData.CellClick += DgvData_CellClick;

            pnlGridContainer.Controls.Add(dgvData);

            pagination = new PaginationControl
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };
            pagination.PageChanged += (s, e) => { _currentPage = e.NewPage; DisplayPage(); };

            pnlMainCard.Controls.Add(pnlGridContainer);
            pnlMainCard.Controls.Add(pnlToolbar);
            pnlMainCard.Controls.Add(pagination);

            this.Controls.Add(pnlMainCard);
            this.Controls.Add(pnlHeader);
        }

        private void LoadData()
        {
            _allVouchers = _service.GetAll();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allVouchers.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string kw = txtSearch.Text.ToLower();
                filtered = filtered.Where(x => x.Code.ToLower().Contains(kw) || x.Name.ToLower().Contains(kw));
            }
            _allVouchers = filtered.ToList();
            _currentPage = 1;
            pagination.UpdatePagination(_allVouchers.Count, _currentPage, PageSize);
            DisplayPage();
        }

        private void DisplayPage()
        {
            if (this.IsDisposed) return;
            dgvData.Rows.Clear();
            var paged = _allVouchers.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();

            foreach (var item in paged)
            {
                int r = dgvData.Rows.Add(
                    item.Code,
                    item.Name,
                    item.Description,
                    item.GroupType,
                    item.Status,
                    ""
                );
                dgvData.Rows[r].Tag = item;
            }
        }

        private void DgvData_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvData.Columns["Action"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                if (e.RowIndex == _hoveredRowIndex)
                {
                    int iconSize = 20;
                    int padding = 10;
                    int totalWidth = (iconSize * 2) + padding;
                    int startX = e.CellBounds.Left + (e.CellBounds.Width - totalWidth) / 2;
                    int startY = e.CellBounds.Top + (e.CellBounds.Height - iconSize) / 2;

                    Rectangle rectEdit = new Rectangle(startX, startY, iconSize, iconSize);
                    Rectangle rectDelete = new Rectangle(startX + iconSize + padding, startY, iconSize, iconSize);

                    Point mouseLoc = dgvData.PointToClient(Cursor.Position);
                    Color editColor = rectEdit.Contains(mouseLoc) ? ThemeManager.ButtonFill : ThemeManager.TextSecondary;
                    Color deleteColor = rectDelete.Contains(mouseLoc) ? Color.FromArgb(231, 76, 60) : ThemeManager.TextSecondary;

                    using (var font = new Font("Segoe UI Emoji", 12))
                    {
                    TextRenderer.DrawText(e.Graphics, "✏️", font, rectEdit, editColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                    }
                    using (var font = new Font("Segoe UI Emoji", 12))
                    {
                    TextRenderer.DrawText(e.Graphics, "🗑️", font, rectDelete, deleteColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                    }
                }
                e.Handled = true;
            }
            // Tags styling
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvData.Columns["GroupType"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                string text = e.Value?.ToString() ?? "";
                Color tagBg = text == "Nhập" ? Color.FromArgb(208, 228, 255) : (text == "Xuất" ? Color.FromArgb(237, 220, 255) : Color.FromArgb(231, 232, 234));
                Color tagText = text == "Nhập" ? Color.FromArgb(0, 29, 53) : (text == "Xuất" ? Color.FromArgb(40, 0, 86) : Color.FromArgb(25, 28, 30));

                DrawPillTag(e.Graphics, e.CellBounds, text, tagBg, tagText);
                e.Handled = true;
            }
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvData.Columns["Status"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                string text = e.Value?.ToString() ?? "";
                Color tagBg = text == "Hoạt động" ? Color.FromArgb(107, 254, 156) : Color.FromArgb(217, 218, 220);
                Color tagText = text == "Hoạt động" ? Color.FromArgb(0, 33, 12) : Color.FromArgb(67, 71, 77);

                DrawPillTag(e.Graphics, e.CellBounds, text, tagBg, tagText);
                e.Handled = true;
            }
        }

        private void DrawPillTag(Graphics g, Rectangle bounds, string text, Color bg, Color fg)
        {
            SizeF size = g.MeasureString(text, new Font("Segoe UI", 9, FontStyle.Bold));
            int width = (int)size.Width + 16;
            int height = 24;
            int x = bounds.Left + (bounds.Width - width) / 2;
            int y = bounds.Top + (bounds.Height - height) / 2;

            using var brush = new SolidBrush(bg);
            g.FillRectangle(brush, x, y, width, height); // Simplified for WinForms without complex GraphicsPath
            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
            {
            TextRenderer.DrawText(g, text, font, new Rectangle(x, y, width, height), fg, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            }
        }

        private void DgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvData.Columns["Action"].Index)
            {
                var item = dgvData.Rows[e.RowIndex].Tag as VoucherType;
                if (item == null) return;

                int iconSize = 20, padding = 10;
                int totalWidth = (iconSize * 2) + padding;
                Rectangle cellBounds = dgvData.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                
                int startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2;
                int startY = cellBounds.Top + (cellBounds.Height - iconSize) / 2;

                Rectangle rectEdit = new Rectangle(startX, startY, iconSize, iconSize);
                Rectangle rectDelete = new Rectangle(startX + iconSize + padding, startY, iconSize, iconSize);

                Point mouseLoc = dgvData.PointToClient(Cursor.Position);

                if (rectEdit.Contains(mouseLoc)) ShowEditForm(item);
                else if (rectDelete.Contains(mouseLoc)) DeleteItem(item);
            }
        }

        private void ShowEditForm(VoucherType? vt)
        {
            using var frm = new VoucherTypeEditForm(vt);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void DeleteItem(VoucherType vt)
        {
            var res = MessageBox.Show($"Xác nhận xóa loại phiếu '{vt.Name}'?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res == DialogResult.Yes)
            {
                _service.Delete(vt.Id);
                LoadData();
            }
        }

        public void PerformSearch(string keyword)
        {
            txtSearch.Text = keyword;
            ApplyFilters();
        }

        public void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            
            pnlToolbar.BackColor = ThemeManager.CardBackground;
            pnlToolbar.CustomBorderColor = ThemeManager.TextBoxBorder;
            pnlToolbar.Parent.BackColor = ThemeManager.CardBackground;
            if (pnlToolbar.Parent is Guna2Panel p) p.CustomBorderColor = ThemeManager.TextBoxBorder;

            txtSearch.FillColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;
            txtSearch.BorderColor = ThemeManager.TextBoxBorder;

            btnExport.FillColor = ThemeManager.CardBackground;
            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.BorderColor = ThemeManager.TextBoxBorder;
            btnExport.BorderThickness = 1;

            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;

            dgvData.BackgroundColor = ThemeManager.CardBackground;
            dgvData.GridColor = ThemeManager.TextBoxBorder;
            dgvData.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvData.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvData.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvData.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvData.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvData.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
        }
    }
}
