using BookStoreManagement.Helpers;
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
        private Guna2TextBox txtSearch;
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
            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(0, 0, 0, gutter) };
            
            lblTitle = new Label { Text = "Quản lý Tài khoản", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Quản lý quyền truy cập Admin và nhân viên.", Font = new Font("Segoe UI", 11F), AutoSize = true, Location = new Point(2, 40) };
            
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
            tlpToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // txtSearch
            tlpToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Spacer
            tlpToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // btnAdd

            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Tìm theo tên, username...",
                Size = new Size(250, 36),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 0, 15, 0)
            };
            txtSearch.TextChanged += (s, e) => { 
                _currentSearchTerm = txtSearch.Text.Trim(); 
                _currentPage = 1; 
                LoadData(); 
            };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; } };

            btnAdd = new Guna2Button { Text = "+ Thêm Người", Size = new Size(130, 36), BorderRadius = 8, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0) };
            
            btnAdd.Click += BtnAdd_Click;

            tlpToolbar.Controls.Add(txtSearch, 0, 0);
            tlpToolbar.Controls.Add(new Panel(), 1, 0); // Empty panel to fill space
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
                AlternatingRowsDefaultCellStyle = { BackColor = Color.Empty }, // Disable alternating colors
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 60 },
                ColumnHeadersHeight = 45,
                Cursor = Cursors.Hand,
                GridColor = Color.LightGray
            };
            dgvAccounts.SetDoubleBuffered(true);

            // Set up columns
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Avatar", HeaderText = "ẢNH ĐẠI DIỆN", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tên đăng nhập", HeaderText = "TÊN ĐĂNG NHẬP", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 120 });
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "HỌ VÀ TÊN", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "EMAIL", Visible = false });
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "RoleName", HeaderText = "VAI TRÒ", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 150 });
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "StoreName", HeaderText = "CHI NHÁNH", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 150 });
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "LastUpdate", HeaderText = "CẬP NHẬT", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 150 });
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Trạng thái", HeaderText = "TRẠNG THÁI", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 150 });
            dgvAccounts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "THAO TÁC", Width = 100, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            
            // Disable alternating rows colors explicitly by making it same as default
            dgvAccounts.AlternatingRowsDefaultCellStyle.BackColor = Color.Empty;

            dgvAccounts.CellPainting += DgvAccounts_CellPainting;
            dgvAccounts.CellMouseMove += DgvAccounts_CellMouseMove;
            dgvAccounts.CellMouseLeave += DgvAccounts_CellMouseLeave;
            dgvAccounts.CellMouseClick += DgvAccounts_CellMouseClick;
            
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
                        "",
                        item.Username,
                        item.FullName,
                        item.Email,
                        item.RoleName,
                        item.StoreName,
                        item.LastUpdate.ToString("dd/MM/yyyy HH:mm"),
                        item.IsActive ? "ĐANG HOẠT ĐỘNG" : "ĐÃ KHÓA",
                        ""
                    );
                    dgvAccounts.Rows[rowIndex].Tag = item;
                }
                
                pagination.UpdatePagination(result.TotalCount, _currentPage, _pageSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải tài khoản: " + ex.Message);
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

        private void DgvAccounts_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvAccounts.Columns[e.ColumnIndex].Name == "Actions")
            {
                int action = (e.X < dgvAccounts.Columns[e.ColumnIndex].Width / 2) ? 1 : 2;
                
                if (_hoveredRowIndex != e.RowIndex || _hoveredAction != action)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = e.RowIndex;
                    _hoveredAction = action;
                    
                    if (oldRow >= 0) dgvAccounts.InvalidateCell(e.ColumnIndex, oldRow);
                    dgvAccounts.InvalidateCell(e.ColumnIndex, _hoveredRowIndex);
                }
                dgvAccounts.Cursor = Cursors.Hand;
            }
            else
            {
                if (_hoveredRowIndex >= 0)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = -1;
                    _hoveredAction = 0;
                    if (e.ColumnIndex >= 0) dgvAccounts.InvalidateCell(dgvAccounts.Columns["Actions"].Index, oldRow);
                }
                dgvAccounts.Cursor = Cursors.Default;
            }
        }

        private void DgvAccounts_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredRowIndex >= 0)
            {
                int oldRow = _hoveredRowIndex;
                _hoveredRowIndex = -1;
                _hoveredAction = 0;
                dgvAccounts.InvalidateCell(dgvAccounts.Columns["Actions"].Index, oldRow);
            }
            dgvAccounts.Cursor = Cursors.Default;
        }

        private void DgvAccounts_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvAccounts.Columns[e.ColumnIndex].Name == "Actions")
            {
                if (dgvAccounts.Rows[e.RowIndex].Tag is AccountItem item)
                {
                    if (e.X < dgvAccounts.Columns[e.ColumnIndex].Width / 2) // Edit
                    {
                        var user = _userService.GetById(item.Id);
                        var frm = new Forms.AccountForm(user);
                        if (frm.ShowDialog() == DialogResult.OK) LoadData();
                    }
                    else // Delete
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
                                MessageBox.Show("Lỗi: " + ex.Message);
                            }
                        }
                    }
                }
            }
        }

        private void DgvAccounts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvAccounts.Columns[e.ColumnIndex].Name == "Actions")
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

                using (var font = new Font("Segoe UI Emoji", editFontSize)) { TextRenderer.DrawText(e.Graphics, "✏️", font, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter); }
                using (var font = new Font("Segoe UI Emoji", delFontSize)) { TextRenderer.DrawText(e.Graphics, "🗑️", font, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter); }

                using (var pen = new Pen(Color.LightGray))
                {
                    e.Graphics.DrawLine(pen, rect.X + rect.Width / 2, rect.Y + 8, rect.X + rect.Width / 2, rect.Bottom - 8);
                }

                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && dgvAccounts.Columns[e.ColumnIndex].Name == "Avatar")
            {
                e.PaintBackground(e.CellBounds, true);
                
                string name = dgvAccounts.Rows[e.RowIndex].Cells["FullName"].Value?.ToString() ?? "";
                string initials = "U";
                if (!string.IsNullOrEmpty(name))
                {
                    var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1) initials = $"{parts[0][0]}{parts[parts.Length-1][0]}".ToUpper();
                    else initials = name.Substring(0, 1).ToUpper();
                }

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int size = 32;
                Rectangle badgeRect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width - size) / 2, e.CellBounds.Y + (e.CellBounds.Height - size) / 2, size, size);

                using (var brush = new SolidBrush(Color.FromArgb(237, 220, 255))) // secondary-fixed
                {
                    g.FillEllipse(brush, badgeRect);
                }

                using (var brush = new SolidBrush(Color.FromArgb(40, 0, 86))) // on-secondary-fixed
                {
                    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
                    {
                        g.DrawString(initials, font, brush, badgeRect, format);
                    }
                }

                using (var pen = new Pen(ThemeManager.TextBoxBorder))
                {
                    g.DrawEllipse(pen, badgeRect);
                }

                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && dgvAccounts.Columns[e.ColumnIndex].Name == "FullName")
            {
                e.PaintBackground(e.CellBounds, true);

                string name = e.Value?.ToString() ?? "Unknown";

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                
                using (var brush = new SolidBrush(ThemeManager.TextPrimary))
                {
                    using (var font = new Font("Segoe UI", 9.5F, FontStyle.Bold))
                    {
                        g.DrawString(name, font, brush, e.CellBounds, format);
                    }
                }

                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && dgvAccounts.Columns[e.ColumnIndex].Name == "Trạng thái")
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "";

                Color bgColor = ThemeManager.TextBoxBorder;
                Color textColor = ThemeManager.TextPrimary;

                if (status.ToUpper() == "ĐANG HOẠT ĐỘNG") { bgColor = Color.FromArgb(40, 46, 204, 113); textColor = Color.FromArgb(46, 204, 113); }
                else if (status.ToUpper() == "ĐÃ KHÓA" || status.ToUpper() == "INACTIVE") { bgColor = Color.FromArgb(40, 231, 76, 60); textColor = Color.FromArgb(231, 76, 60); }
                else { bgColor = Color.FromArgb(40, 41, 128, 185); textColor = Color.FromArgb(41, 128, 185); }

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var statusFont = new Font("Segoe UI", 8F, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString(status, statusFont);
                    RectangleF badgeRect = new RectangleF(e.CellBounds.X + (e.CellBounds.Width - textSize.Width - 20) / 2, e.CellBounds.Y + (e.CellBounds.Height - textSize.Height - 10) / 2, textSize.Width + 20, textSize.Height + 10);

                    using (var brush = new SolidBrush(bgColor))
                    {
                        g.FillRoundedRectangle(brush, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height, 10);
                    }

                    using (var brush = new SolidBrush(textColor))
                    {
                        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString(status, statusFont, brush, badgeRect, format);
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
            
            if (txtSearch != null)
            {
                txtSearch.FillColor = ThemeManager.TextBoxBackground;
                txtSearch.ForeColor = ThemeManager.TextPrimary;
                txtSearch.BorderColor = ThemeManager.TextBoxBorder;
            }

            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;
            
            pnlGridContainer.FillColor = ThemeManager.CardBackground;
            pnlGridContainer.BorderColor = ThemeManager.TextBoxBorder;
            
            ThemeManager.ApplyDataGridViewStyle(dgvAccounts);
        }
    }
}