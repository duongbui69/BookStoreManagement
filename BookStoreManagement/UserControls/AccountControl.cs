using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;
using BookStoreManagement.Interfaces;
using System.Collections.Generic;

namespace BookStoreManagement.UserControls
{
    public partial class AccountControl : UserControl, ISearchableControl
    {
        private readonly UserService _userService;

        // Content Container
        private Guna2Panel pnlContent;

        // Page Header
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;

        // Toolbar
        private Guna2Panel pnlToolbar;
        private Guna2Button btnFilter;
        private Guna2Button btnAdd;

        // Grid
        private Guna2Panel pnlGridContainer;
        private Guna2DataGridView dgvAccounts;

        // Pagination
        private PaginationControl pagination;

        private int _currentPage = 1;
        private int _pageSize = 5;

        private int _hoveredRowIndex = -1;
        private int _hoveredAction = 0; // 1: Edit, 2: Delete
        private string _currentSearchTerm = "";

        public void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            LoadData();
        }

        public AccountControl()
        {
            _userService = new UserService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += AccountControl_Load;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 20;
            this.Padding = new Padding(0);

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = true };

            // 1. Page Header
            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0, 0, 0, gutter) };
            
            lblTitle = new Label { Text = "Account Management", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Manage admin and staff access permissions.", Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(2, 40) };
            
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            // 2. Toolbar
            pnlToolbar = new Guna2Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(0, 10, 0, 10) };
            
            TableLayoutPanel tlpToolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0)
            };
            tlpToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Spacer
            tlpToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            btnFilter = new Guna2Button { Text = "Lọc", Size = new Size(100, 40), BorderRadius = 4, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, BorderThickness = 1, Margin = new Padding(0, 0, 15, 0) };
            btnAdd = new Guna2Button { Text = "+ Add User", Size = new Size(130, 40), BorderRadius = 4, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0) };
            
            btnAdd.Click += BtnAdd_Click;

            tlpToolbar.Controls.Add(new Panel(), 0, 0); // Empty panel to fill space
            tlpToolbar.Controls.Add(btnFilter, 1, 0);
            tlpToolbar.Controls.Add(btnAdd, 2, 0);
            
            pnlToolbar.Controls.Add(tlpToolbar);

            // 3. Grid Container
            pnlGridContainer = new Guna2Panel { Dock = DockStyle.Fill, BorderRadius = 8, BorderThickness = 1 };
            
            dgvAccounts = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AllowUserToResizeRows = false,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle(), // Keep same as default to avoid zebra striping
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 50 },
                ColumnHeadersHeight = 45,
                Cursor = Cursors.Hand
            };

            // Set up columns
            dgvAccounts.Columns.Add("Id", "Id");
            dgvAccounts.Columns["Id"].Visible = false;
            
            dgvAccounts.Columns.Add("Username", "Tên Đăng Nhập");
            dgvAccounts.Columns.Add("FullName", "Họ Tên");
            dgvAccounts.Columns.Add("Email", "Email");
            dgvAccounts.Columns.Add("RoleName", "Vai Trò");
            dgvAccounts.Columns.Add("StoreName", "Chi Nhánh");
            dgvAccounts.Columns.Add("LastUpdate", "Lần Cập Nhật");
            dgvAccounts.Columns.Add("Status", "Trạng Thái");
            dgvAccounts.Columns.Add("Actions", "Hành Động");
            
            // Disable alternating rows colors explicitly by making it same as default
            dgvAccounts.AlternatingRowsDefaultCellStyle.BackColor = Color.Empty;

            foreach (DataGridViewColumn col in dgvAccounts.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (col.Name == "Actions")
                {
                    col.Width = 100;
                }
            }

            dgvAccounts.CellPainting += DgvAccounts_CellPainting;
            dgvAccounts.CellMouseEnter += DgvAccounts_CellMouseEnter;
            dgvAccounts.CellMouseLeave += DgvAccounts_CellMouseLeave;
            dgvAccounts.CellClick += DgvAccounts_CellClick;
            
            // 4. Pagination
            pagination = new PaginationControl { Dock = DockStyle.Bottom, Height = 50 };
            
            pagination.PageChanged += (s, args) => { _currentPage = args.NewPage; LoadData(); };

            pnlGridContainer.Controls.Add(dgvAccounts);
            pnlGridContainer.Controls.Add(pagination);

            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(pnlToolbar);
            pnlContent.Controls.Add(pnlPageHeader);

            this.Controls.Add(pnlContent);
            
            ApplyTheme();
        }

        private void AccountControl_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var result = _userService.GetPagedAccounts(_currentPage, _pageSize, _currentSearchTerm);
                if (this.IsDisposed) return;
                dgvAccounts.Rows.Clear();
                
                foreach (var item in result.Items)
                {
                    int rowIndex = dgvAccounts.Rows.Add(
                        item.Id,
                        item.Username,
                        item.FullName,
                        item.Email,
                        item.RoleName,
                        item.StoreName,
                        item.LastUpdate.ToString("dd/MM/yyyy HH:mm"),
                        item.IsActive ? "Hoạt động" : "Bị khóa",
                        ""
                    );
                    dgvAccounts.Rows[rowIndex].Tag = item;
                }
                
                pagination.UpdatePagination(result.TotalCount, _currentPage, _pageSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading accounts: " + ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var frm = new Forms.AccountForm();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void DgvAccounts_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvAccounts.Columns["Actions"].Index)
            {
                _hoveredRowIndex = e.RowIndex;
                Rectangle cellBounds = dgvAccounts.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                int mouseX = dgvAccounts.PointToClient(Cursor.Position).X - cellBounds.X;
                
                int totalWidth = 60; // 24 + 12 + 24
                int startX = (cellBounds.Width - totalWidth) / 2;
                
                if (mouseX >= startX && mouseX <= startX + 24) _hoveredAction = 1; // Edit
                else if (mouseX >= startX + 36 && mouseX <= startX + 60) _hoveredAction = 2; // Delete
                else _hoveredAction = 0;
                
                dgvAccounts.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }

        private void DgvAccounts_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvAccounts.Columns["Actions"].Index)
            {
                _hoveredRowIndex = -1;
                _hoveredAction = 0;
                dgvAccounts.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }

        private void DgvAccounts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvAccounts.Columns["Actions"].Index)
            {
                if (dgvAccounts.Rows[e.RowIndex].Tag is AccountItem item)
                {
                    if (_hoveredAction == 1) // Edit
                    {
                        var user = _userService.GetById(item.Id);
                        var frm = new Forms.AccountForm(user);
                        if (frm.ShowDialog() == DialogResult.OK) LoadData();
                    }
                    else if (_hoveredAction == 2) // Delete
                    {
                        if (MessageBox.Show($"Bạn có chắc chắn muốn vô hiệu hóa/xóa tài khoản {item.Username}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            try
                            {
                                _userService.Delete(item.Id);
                                LoadData();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error: " + ex.Message);
                            }
                        }
                    }
                }
            }
        }

        private void DgvAccounts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvAccounts.Columns["Actions"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

                int iconSize = 24;
                int spacing = 12;
                int totalWidth = (iconSize * 2) + spacing;
                int startX = e.CellBounds.X + (e.CellBounds.Width - totalWidth) / 2;
                int y = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;

                bool isHoveredRow = (e.RowIndex == _hoveredRowIndex);
                Color editColor = (isHoveredRow && _hoveredAction == 1) ? ThemeManager.ButtonFill : ThemeManager.TextSecondary;
                Color deleteColor = (isHoveredRow && _hoveredAction == 2) ? Color.FromArgb(231, 76, 60) : ThemeManager.TextSecondary;

                using (Font iconFont = new Font("Segoe UI Emoji", 12f))
                {
                    TextRenderer.DrawText(e.Graphics, "✏️", iconFont, new Rectangle(startX, y, iconSize, iconSize), editColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    TextRenderer.DrawText(e.Graphics, "🗑️", iconFont, new Rectangle(startX + iconSize + spacing, y, iconSize, iconSize), deleteColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvAccounts.Columns["Status"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                string status = e.Value?.ToString() ?? "";
                bool isActive = status == "Hoạt động";
                
                Color bgColor = isActive ? Color.FromArgb(40, 46, 204, 113) : Color.FromArgb(40, 231, 76, 60);
                Color textColor = isActive ? Color.FromArgb(39, 174, 96) : Color.FromArgb(192, 57, 43);
                Color dotColor = isActive ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);
                
                using (GraphicsPath path = new GraphicsPath())
                {
                    int h = 24;
                    int w = 90;
                    int x = e.CellBounds.X + (e.CellBounds.Width - w) / 2;
                    int y = e.CellBounds.Y + (e.CellBounds.Height - h) / 2;
                    
                    path.AddArc(x, y, h, h, 90, 180);
                    path.AddArc(x + w - h, y, h, h, 270, 180);
                    path.CloseFigure();
                    
                    using (SolidBrush bgBrush = new SolidBrush(bgColor))
                    {
                        e.Graphics.FillPath(bgBrush, path);
                    }
                    
                    using (SolidBrush dotBrush = new SolidBrush(dotColor))
                    {
                        e.Graphics.FillEllipse(dotBrush, x + 8, y + 9, 6, 6);
                    }
                    
                    using (Font f = new Font("Segoe UI", 9f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(e.Graphics, status, f, new Rectangle(x + 18, y, w - 18, h), textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                    }
                }
                
                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvAccounts.Columns["RoleName"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                string role = e.Value?.ToString() ?? "";
                bool isAdmin = role.ToLower().Contains("admin");
                
                Color bgColor = isAdmin ? ThemeManager.ButtonFill.ToArgb() == Color.FromArgb(0, 36, 64).ToArgb() ? Color.FromArgb(208, 228, 255) : Color.FromArgb(0, 36, 64) : ThemeManager.Background;
                Color textColor = isAdmin ? ThemeManager.ButtonFill.ToArgb() == Color.FromArgb(0, 36, 64).ToArgb() ? Color.FromArgb(0, 29, 53) : Color.FromArgb(208, 228, 255) : ThemeManager.TextPrimary;
                
                using (GraphicsPath path = new GraphicsPath())
                {
                    int h = 22;
                    int w = 80;
                    int x = e.CellBounds.X + (e.CellBounds.Width - w) / 2;
                    int y = e.CellBounds.Y + (e.CellBounds.Height - h) / 2;
                    
                    path.AddArc(x, y, h, h, 90, 180);
                    path.AddArc(x + w - h, y, h, h, 270, 180);
                    path.CloseFigure();
                    
                    using (SolidBrush bgBrush = new SolidBrush(bgColor))
                        e.Graphics.FillPath(bgBrush, path);
                    
                    using (Font f = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                        TextRenderer.DrawText(e.Graphics, role.ToUpper(), f, new Rectangle(x, y, w, h), textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                
                e.Handled = true;
            }
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;
            pnlPageHeader.BackColor = ThemeManager.Background;
            pnlToolbar.BackColor = ThemeManager.Background;
            
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            
            btnFilter.FillColor = ThemeManager.Background;
            btnFilter.ForeColor = ThemeManager.TextPrimary;
            btnFilter.BorderColor = ThemeManager.TextBoxBorder;
            
            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = Color.White;
            
            pnlGridContainer.FillColor = ThemeManager.CardBackground;
            pnlGridContainer.BorderColor = ThemeManager.TextBoxBorder;
            
            dgvAccounts.BackgroundColor = ThemeManager.CardBackground;
            dgvAccounts.GridColor = ThemeManager.TextBoxBorder;
            
            dgvAccounts.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.TextBoxBackground;
            dgvAccounts.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvAccounts.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.TextBoxBackground;
            dgvAccounts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            dgvAccounts.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvAccounts.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvAccounts.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvAccounts.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvAccounts.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            
            dgvAccounts.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            
            
            pagination.BackColor = ThemeManager.CardBackground;
            
            dgvAccounts.Invalidate();
        }
    }
}
