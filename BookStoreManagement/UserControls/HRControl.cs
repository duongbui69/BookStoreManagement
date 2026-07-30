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
    public partial class HRControl : UserControl, ISearchableControl
    {
        private readonly HRService _service;

        // Content Container
        private Guna2Panel pnlContent;

        // Header & Toolbar
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        private Guna2Button btnAdd;

        // Filters Bar
        private Guna2Panel pnlFilters;
        private Guna2TextBox txtSearch;
        private Guna2ComboBox cbBranch;
        private Guna2ComboBox cbRole;
        private Guna2Button btnPayroll;

        // Grid
        private Guna2Panel pnlGridContainer;
        private Guna2DataGridView dgvEmployees;

        // Pagination
        private PaginationControl paginationControl;

        private int _currentPage = 1;
        private int _pageSize = 5;

        private int _hoveredRowIndex = -1;
        private int _hoveredAction = 0;
        private string _currentBranch = "All Departments";
        private string _currentRole = "Status: All";
        private string _currentSearchTerm = "";

        public async void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            await LoadDataAsync();
        }

        public HRControl()
        {
            _service = new HRService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += HRControl_Load;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            int gutter = 20;
            this.Padding = new Padding(0);
            this.AutoScroll = true;

            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(gutter), AutoScroll = true };

            // 1. Header & Toolbar
            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(0, 0, 0, gutter) };

            lblTitle = new Label { Text = "Quản lý Nhân viên", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Quản lý nhân viên, vai trò, và chi nhánh.", Font = new Font("Segoe UI", 11F), AutoSize = true, Location = new Point(2, 40) };
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            // 2. Filters Bar
            pnlFilters = new Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0) };
            
            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Tìm theo mã, tên...",
                Size = new Size(220, 36),
                Location = new Point(0, 12),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F)
            };
            txtSearch.TextChanged += async (s, e) => { _currentSearchTerm = txtSearch.Text; _currentPage = 1; await LoadDataAsync(); };

            cbBranch = new Guna2ComboBox { Size = new Size(160, 36), Location = new Point(230, 12), BorderRadius = 6, Font = new Font("Segoe UI", 9F) };
            cbBranch.Items.AddRange(new object[] { "All Departments", "Logistics", "IT", "Doanh số" });
            cbBranch.SelectedIndex = 0;
            cbBranch.SelectedIndexChanged += async (s, e) => { _currentBranch = cbBranch.SelectedItem.ToString(); _currentPage = 1; await LoadDataAsync(); };

            cbRole = new Guna2ComboBox { Size = new Size(160, 36), Location = new Point(400, 12), BorderRadius = 6, Font = new Font("Segoe UI", 9F) };
            cbRole.Items.AddRange(new object[] { "Tất cả trạng thái", "ACTIVE", "INACTIVE", "ON LEAVE" });
            cbRole.SelectedIndex = 0;
            cbRole.SelectedIndexChanged += async (s, e) =>
            {
                string selected = cbRole.SelectedItem?.ToString() ?? "";
                _currentRole = (selected == "Tất cả trạng thái") ? "Status: All" : selected;
                _currentPage = 1;
                await LoadDataAsync();
            };

            btnPayroll = new Guna2Button { Text = "Tính lương", Size = new Size(100, 36), BorderRadius = 6, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnPayroll.Click += (s, e) => MessageBox.Show("Tính năng tính lương đang phát triển.", "Thông tin");
            
            btnAdd = new Guna2Button { Text = "+ Thêm Nhân viên", Size = new Size(150, 36), BorderRadius = 6, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.Click += async (s, e) => {
                var frm = new Forms.EmployeeForm(null);
                if (frm.ShowDialog() == DialogResult.OK) await LoadDataAsync();
            };

            pnlFilters.Controls.AddRange(new Control[] { txtSearch, cbBranch, cbRole, btnPayroll, btnAdd });
            pnlFilters.Resize += (s, e) =>
            {
                btnAdd.Location = new Point(pnlFilters.Width - 150, 12);
                btnPayroll.Location = new Point(pnlFilters.Width - 260, 12);
            };

            // 3. Grid Container
            pnlGridContainer = new Guna2Panel { Dock = DockStyle.Fill, CustomBorderThickness = new Padding(1, 0, 1, 1), Margin = new Padding(0, 0, 0, gutter), BorderRadius = 6 };
            pnlGridContainer.CustomizableEdges.TopLeft = false;
            pnlGridContainer.CustomizableEdges.TopRight = false;
            
            dgvEmployees = new Guna2DataGridView
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
            dgvEmployees.SetDoubleBuffered(true);
            
            // Define Columns
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Name = "Id", Visible = false });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { Name = "Avatar", HeaderText = "ẢNH ĐẠI DIỆN", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EmployeeId", Name = "EmployeeId", HeaderText = "MÀ NHÂN VIÊN", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 120 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", Name = "Tên", HeaderText = "HỌ VÀ TÊN", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoleName", HeaderText = "VỊ TRÍ", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 150 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Department", HeaderText = "CHI NHÁNH", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 150 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", Name = "Trạng thái", HeaderText = "TRẠNG THÁI", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 100 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", Name = "Email", Visible = false });
            
            DataGridViewTextBoxColumn actionCol = new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "THAO TÁC", Width = 100, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
            dgvEmployees.Columns.Add(actionCol);

            dgvEmployees.CellPainting += DgvEmployees_CellPainting;
            dgvEmployees.CellMouseClick += DgvEmployees_CellMouseClick;
            dgvEmployees.CellMouseMove += DgvEmployees_CellMouseMove;
            dgvEmployees.CellMouseLeave += DgvEmployees_CellMouseLeave;
            
            pnlGridContainer.Controls.Add(dgvEmployees);

            // Pagination
            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += async (s, e) => { _currentPage = e.NewPage; await LoadDataAsync(); };
            pnlGridContainer.Controls.Add(paginationControl);

            // Assemble layout
            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10 }); // Spacer
            pnlContent.Controls.Add(pnlFilters);
            pnlContent.Controls.Add(pnlPageHeader);
            
            this.Controls.Add(pnlContent);

            ApplyTheme();
        }



        private async void HRControl_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            var (items, totalCount) = await _service.GetPagedEmployeesAsync(_currentPage, _pageSize, _currentBranch, _currentRole, _currentSearchTerm);
            dgvEmployees.DataSource = items;
            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
        }

        private void LoadData()
        {
            var (items, totalCount) = _service.GetPagedEmployees(_currentPage, _pageSize, _currentBranch, _currentRole, _currentSearchTerm);
            dgvEmployees.DataSource = items;
            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
        }

        private void DgvEmployees_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvEmployees.Columns[e.ColumnIndex].Name == "Actions")
            {
                int action = (e.X < dgvEmployees.Columns[e.ColumnIndex].Width / 2) ? 1 : 2;
                
                if (_hoveredRowIndex != e.RowIndex || _hoveredAction != action)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = e.RowIndex;
                    _hoveredAction = action;
                    
                    if (oldRow >= 0) dgvEmployees.InvalidateCell(e.ColumnIndex, oldRow);
                    dgvEmployees.InvalidateCell(e.ColumnIndex, _hoveredRowIndex);
                }
                dgvEmployees.Cursor = Cursors.Hand;
            }
            else
            {
                if (_hoveredRowIndex >= 0)
                {
                    int oldRow = _hoveredRowIndex;
                    _hoveredRowIndex = -1;
                    _hoveredAction = 0;
                    if (e.ColumnIndex >= 0) dgvEmployees.InvalidateCell(dgvEmployees.Columns["Actions"].Index, oldRow);
                }
                dgvEmployees.Cursor = Cursors.Default;
            }
        }

        private void DgvEmployees_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredRowIndex >= 0)
            {
                int oldRow = _hoveredRowIndex;
                _hoveredRowIndex = -1;
                _hoveredAction = 0;
                dgvEmployees.InvalidateCell(dgvEmployees.Columns["Actions"].Index, oldRow);
            }
            dgvEmployees.Cursor = Cursors.Default;
        }

        private async void DgvEmployees_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvEmployees.Columns[e.ColumnIndex].Name == "Actions")
            {
                int empId = Convert.ToInt32(dgvEmployees.Rows[e.RowIndex].Cells["Id"].Value);
                
                if (e.X < dgvEmployees.Columns[e.ColumnIndex].Width / 2)
                {
                    // Edit
                    var employee = _service.GetById(empId);
                    if (employee != null)
                    {
                        var frm = new Forms.EmployeeForm(employee);
                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadDataAsync();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy nhân viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Delete
                    if (MessageBox.Show("Bạn có chắc chắn muốn xóa nhân viên này không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            _service.DeleteUser(empId);
                            MessageBox.Show("Xóa nhân viên thành công!");
                            await LoadDataAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void DgvEmployees_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Custom Paint for Actions
            if (dgvEmployees.Columns[e.ColumnIndex].Name == "Actions")
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

                using (var editFont = new Font("Segoe UI Emoji", editFontSize))
                {
                    TextRenderer.DrawText(e.Graphics, "✏️", editFont, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                
                using (var delFont = new Font("Segoe UI Emoji", delFontSize))
                {
                    TextRenderer.DrawText(e.Graphics, "🗑️", delFont, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                
                using (var pen = new Pen(Color.LightGray))
                {
                    e.Graphics.DrawLine(pen, rect.X + rect.Width / 2, rect.Y + 8, rect.X + rect.Width / 2, rect.Bottom - 8);
                }
                
                e.Handled = true;
            }
            // Custom Paint for Avatar
            else if (dgvEmployees.Columns[e.ColumnIndex].Name == "Avatar")
            {
                e.PaintBackground(e.CellBounds, true);
                
                string name = dgvEmployees.Rows[e.RowIndex].Cells["Tên"].Value?.ToString() ?? "";
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
            // Custom Paint for Employee Name
            else if (dgvEmployees.Columns[e.ColumnIndex].Name == "Tên")
            {
                e.PaintBackground(e.CellBounds, true);

                string name = e.Value?.ToString() ?? "Unknown";
                string email = dgvEmployees.Rows[e.RowIndex].Cells["EmployeeId"].Value?.ToString() ?? "";

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
            else if (dgvEmployees.Columns[e.ColumnIndex].Name == "Trạng thái")
            {
                e.PaintBackground(e.CellBounds, true);
                string statusRaw = e.Value?.ToString() ?? "";

                // Translate DB status to Vietnamese display text
                string statusDisplay;
                Color bgColor, textColor;
                switch (statusRaw.ToUpper())
                {
                    case "ACTIVE":
                        statusDisplay = "Đang hoạt động";
                        bgColor = Color.FromArgb(40, 46, 204, 113);
                        textColor = Color.FromArgb(46, 204, 113);
                        break;
                    case "INACTIVE":
                    case "TERMINATED":
                        statusDisplay = statusRaw.ToUpper() == "TERMINATED" ? "Đã nghỉ" : "Ngừng hoạt động";
                        bgColor = Color.FromArgb(40, 231, 76, 60);
                        textColor = Color.FromArgb(231, 76, 60);
                        break;
                    case "ON LEAVE":
                        statusDisplay = "Nghỉ phép";
                        bgColor = Color.FromArgb(40, 243, 156, 18);
                        textColor = Color.FromArgb(243, 156, 18);
                        break;
                    default:
                        statusDisplay = statusRaw;
                        bgColor = Color.FromArgb(40, 41, 128, 185);
                        textColor = Color.FromArgb(41, 128, 185);
                        break;
                }

                // When row is selected, override badge text to white so it stays readable
                bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;
                if (isSelected) textColor = Color.White;

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var statusFont = new Font("Segoe UI", 8F, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString(statusDisplay, statusFont);
                    RectangleF badgeRect = new RectangleF(
                        e.CellBounds.X + (e.CellBounds.Width - textSize.Width - 20) / 2,
                        e.CellBounds.Y + (e.CellBounds.Height - textSize.Height - 10) / 2,
                        textSize.Width + 20, textSize.Height + 10);

                    using (var brush = new SolidBrush(bgColor))
                    {
                        g.FillRoundedRectangle(brush, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height, 10);
                    }

                    using (var brush = new SolidBrush(textColor))
                    {
                        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString(statusDisplay, statusFont, brush, badgeRect, format);
                    }
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

            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;

            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;

            btnPayroll.FillColor = ThemeManager.CardBackground;
            btnPayroll.ForeColor = ThemeManager.TextPrimary;
            btnPayroll.BorderColor = ThemeManager.TextBoxBorder;

            // Filters
            pnlFilters.BackColor = ThemeManager.Background;
            pnlFilters.CustomBorderColor = ThemeManager.TextBoxBorder;
            pnlFilters.FillColor = ThemeManager.Background;
            
            if (txtSearch != null)
            {
                txtSearch.FillColor = ThemeManager.TextBoxBackground;
                txtSearch.ForeColor = ThemeManager.TextPrimary;
                txtSearch.BorderColor = ThemeManager.TextBoxBorder;
            }

            cbBranch.FillColor = ThemeManager.TextBoxBackground;
            cbBranch.ForeColor = ThemeManager.TextPrimary;
            cbBranch.BorderColor = ThemeManager.TextBoxBorder;

            cbRole.FillColor = ThemeManager.TextBoxBackground;
            cbRole.ForeColor = ThemeManager.TextPrimary;
            cbRole.BorderColor = ThemeManager.TextBoxBorder;
            
            btnPayroll.FillColor = ThemeManager.CardBackground;
            btnPayroll.ForeColor = ThemeManager.TextPrimary;
            btnPayroll.BorderColor = ThemeManager.TextBoxBorder;
            
            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;

            // Grid
            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            ThemeManager.ApplyDataGridViewStyle(dgvEmployees);
        }
    
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BookStoreManagement.Themes.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            }
            base.Dispose(disposing);
        }
}
}
