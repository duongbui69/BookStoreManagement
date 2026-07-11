using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.UserControls
{
    public partial class HRControl : UserControl
    {
        private HRRepository _repo;
        
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnPermissions;

        private FlowLayoutPanel flpCards;
        private Panel pnlFilters;
        private ComboBox cbDept;
        private ComboBox cbStatus;

        private DataGridView dgvStaff;
        private PaginationControl paginationControl;

        private int _currentPage = 1;
        private int _pageSize = 10;

        public HRControl()
        {
            _repo = new HRRepository();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += HRControl_Load;
            this.Resize += HRControl_Resize;
        }

        private void InitializeUI()
        {
            this.BackColor = ThemeManager.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(30);

            // Header
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
            lblSubTitle = new Label { Text = "👥 HUMAN RESOURCES", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.TextSecondary, AutoSize = true, Location = new Point(0, 0) };
            lblTitle = new Label { Text = "Staff Directory", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 20) };
            
            txtSearch = new TextBox { Width = 300, Font = new Font("Segoe UI", 12F), PlaceholderText = "Search employees, roles, or departments." };
            txtSearch.TextChanged += (s, e) => { _currentPage = 1; LoadData(); };

            btnAdd = new Button { Text = "+ Add New Employee", Width = 180, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnPermissions = new Button { Text = "🛡 Edit Permissions", Width = 150, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

            pnlHeader.Controls.Add(lblSubTitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(txtSearch);
            pnlHeader.Controls.Add(btnAdd);
            pnlHeader.Controls.Add(btnPermissions);
            this.Controls.Add(pnlHeader);

            // Cards
            flpCards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 130, Margin = new Padding(0, 20, 0, 20), WrapContents = false, BackColor = Color.Transparent };
            this.Controls.Add(flpCards);

            // Filters
            pnlFilters = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };
            cbDept = new ComboBox { Width = 180, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(0, 10) };
            cbDept.Items.Add("All Departments"); cbDept.SelectedIndex = 0;
            cbDept.SelectedIndexChanged += (s, e) => { _currentPage = 1; LoadData(); };

            cbStatus = new ComboBox { Width = 120, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(200, 10) };
            cbStatus.Items.AddRange(new[] { "Status: All", "ACTIVE", "ON LEAVE" }); cbStatus.SelectedIndex = 0;
            cbStatus.SelectedIndexChanged += (s, e) => { _currentPage = 1; LoadData(); };

            pnlFilters.Controls.Add(cbDept);
            pnlFilters.Controls.Add(cbStatus);
            this.Controls.Add(pnlFilters);

            // Pagination
            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadData(); };
            this.Controls.Add(paginationControl);

            // DataGridView
            dgvStaff = new DataGridView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 70 }
            };
            dgvStaff.CellPainting += DgvStaff_CellPainting;
            this.Controls.Add(dgvStaff);
            
            // Re-order controls
            dgvStaff.BringToFront();
            pnlFilters.BringToFront();
            flpCards.BringToFront();
            pnlHeader.BringToFront();

            ApplyTheme();
        }

        private void HRControl_Resize(object sender, EventArgs e)
        {
            if (pnlHeader != null)
            {
                btnAdd.Location = new Point(pnlHeader.Width - 180, 10);
                btnPermissions.Location = new Point(pnlHeader.Width - 340, 10);
                txtSearch.Location = new Point(lblTitle.Right + 50, 15);
            }
        }

        private void HRControl_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            var stats = _repo.GetStats();
            
            flpCards.Controls.Clear();
            int cardWidth = Math.Max(220, (this.Width - 160) / 3);

            flpCards.Controls.Add(CreateStatCard("Total Employees", stats.TotalEmployees.ToString(), $"+{stats.NewThisMonth} this month", cardWidth));
            flpCards.Controls.Add(CreateStatCard("Active Departments", stats.ActiveDepartments.ToString(), "", cardWidth));
            flpCards.Controls.Add(CreateLoadCard(stats.LogisticsLoad, stats.SalesLoad, stats.ITLoad, cardWidth + 100));

            var (items, totalCount) = _repo.GetPagedEmployees(_currentPage, _pageSize, cbDept.SelectedItem?.ToString(), cbStatus.SelectedItem?.ToString(), txtSearch.Text);
            dgvStaff.DataSource = items;
            
            if (dgvStaff.Columns["Email"] != null) dgvStaff.Columns["Email"].Visible = false;
            if (dgvStaff.Columns["EmployeeId"] != null) dgvStaff.Columns["EmployeeId"].Visible = false;
            if (dgvStaff.Columns["JoinDate"] != null) dgvStaff.Columns["JoinDate"].DefaultCellStyle.Format = "dd MMM yyyy";

            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
        }

        private Panel CreateStatCard(string title, string value, string badge, int width)
        {
            var pnl = new Panel { Width = width, Height = 120, BackColor = ThemeManager.CardBackground, Margin = new Padding(0, 0, 20, 0) };
            pnl.Paint += (s, e) => 
            {
                using var p = new Pen(ThemeManager.TextBoxBorder, 1);
                e.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
                
                if (!string.IsNullOrEmpty(badge))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using var b = new SolidBrush(Color.FromArgb(40, 46, 204, 113));
                    e.Graphics.FillRoundedRectangle(b, pnl.Width - 90, 15, 75, 20, 10);
                }
            };
            
            pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.TextSecondary, Location = new Point(20, 20), AutoSize = true });
            pnl.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = ThemeManager.TextPrimary, Location = new Point(15, 50), AutoSize = true });
            
            if (!string.IsNullOrEmpty(badge))
            {
                pnl.Controls.Add(new Label { Text = badge, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Color.FromArgb(46, 204, 113), Location = new Point(pnl.Width - 85, 17), AutoSize = true, BackColor = Color.Transparent });
            }
            return pnl;
        }

        private Panel CreateLoadCard(int logLoad, int salesLoad, int itLoad, int width)
        {
            var pnl = new Panel { Width = width, Height = 120, BackColor = ThemeManager.CardBackground, Margin = new Padding(0, 0, 20, 0) };
            pnl.Paint += (s, e) => 
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var p = new Pen(ThemeManager.TextBoxBorder, 1);
                e.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
                
                // Draw Progress bars
                int barWidth = (width - 60) / 3;
                DrawProgressBar(e.Graphics, "LOGISTICS", logLoad, 20, 70, barWidth, Color.FromArgb(10, 35, 55));
                DrawProgressBar(e.Graphics, "SALES", salesLoad, 20 + barWidth + 10, 70, barWidth, Color.FromArgb(128, 90, 213));
                DrawProgressBar(e.Graphics, "IT", itLoad, 20 + (barWidth + 10) * 2, 70, barWidth, Color.FromArgb(46, 204, 113));
            };
            
            pnl.Controls.Add(new Label { Text = "Department Load", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ThemeManager.TextPrimary, Location = new Point(20, 20), AutoSize = true });
            return pnl;
        }

        private void DrawProgressBar(Graphics g, string label, int percent, int x, int y, int width, Color color)
        {
            using (var b = new SolidBrush(ThemeManager.TextSecondary))
                g.DrawString(label, new Font("Segoe UI", 8F, FontStyle.Bold), b, x, y - 20);
                
            using (var b = new SolidBrush(ThemeManager.TextSecondary))
            {
                var format = new StringFormat { Alignment = StringAlignment.Far };
                g.DrawString($"{percent}%", new Font("Segoe UI", 8F, FontStyle.Bold), b, new RectangleF(x, y - 20, width, 20), format);
            }

            using (var b = new SolidBrush(ThemeManager.TextBoxBorder))
                g.FillRoundedRectangle(b, x, y, width, 6, 3);
            
            using (var b = new SolidBrush(color))
                g.FillRoundedRectangle(b, x, y, width * percent / 100f, 6, 3);
        }

        private void DgvStaff_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Custom Paint for Employee Name (Index 0)
            if (dgvStaff.Columns[e.ColumnIndex].Name == "Name")
            {
                e.PaintBackground(e.CellBounds, true);
                
                string name = e.Value?.ToString() ?? "Unknown";
                string email = dgvStaff.Rows[e.RowIndex].Cells["Email"].Value?.ToString() ?? "";
                string initials = name.Length > 0 ? name.Substring(0, 1).ToUpper() : "?";
                if (name.Contains(" ")) initials = name.Split(' ')[0].Substring(0, 1).ToUpper() + name.Split(' ')[1].Substring(0, 1).ToUpper();

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Avatar Circle
                Rectangle avatarRect = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 15, 40, 40);
                using (var brush = new SolidBrush(ThemeManager.Sidebar)) // Dark blue like the sidebar
                {
                    g.FillEllipse(brush, avatarRect);
                }
                
                using (var brush = new SolidBrush(Color.White))
                {
                    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(initials, new Font("Segoe UI", 10F, FontStyle.Bold), brush, avatarRect, format);
                }

                // Name
                using (var brush = new SolidBrush(ThemeManager.TextPrimary))
                {
                    g.DrawString(name, new Font("Segoe UI", 10F, FontStyle.Bold), brush, e.CellBounds.X + 65, e.CellBounds.Y + 15);
                }

                // Email
                using (var brush = new SolidBrush(ThemeManager.TextSecondary))
                {
                    g.DrawString(email, new Font("Segoe UI", 8.5F), brush, e.CellBounds.X + 65, e.CellBounds.Y + 38);
                }

                e.Handled = true;
            }
            // Custom Paint for Role / ID (Index 2)
            else if (dgvStaff.Columns[e.ColumnIndex].Name == "RoleName")
            {
                e.PaintBackground(e.CellBounds, true);
                string role = e.Value?.ToString() ?? "";
                string empId = dgvStaff.Rows[e.RowIndex].Cells["EmployeeId"].Value?.ToString() ?? "";

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var brush = new SolidBrush(ThemeManager.TextPrimary))
                    g.DrawString(role, new Font("Segoe UI", 9F), brush, e.CellBounds.X + 5, e.CellBounds.Y + 15);

                using (var brush = new SolidBrush(ThemeManager.TextSecondary))
                    g.DrawString(empId, new Font("Segoe UI", 8.5F), brush, e.CellBounds.X + 5, e.CellBounds.Y + 38);

                e.Handled = true;
            }
            // Custom Paint for Status (Index 5)
            else if (dgvStaff.Columns[e.ColumnIndex].Name == "Status")
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "";
                
                Color bgColor = ThemeManager.TextBoxBorder;
                Color textColor = ThemeManager.TextPrimary;

                if (status == "ACTIVE") { bgColor = Color.FromArgb(40, 46, 204, 113); textColor = Color.FromArgb(46, 204, 113); }
                else if (status == "ON LEAVE") { bgColor = Color.FromArgb(40, 128, 90, 213); textColor = Color.FromArgb(128, 90, 213); }

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                SizeF textSize = g.MeasureString(status.ToUpper(), new Font("Segoe UI", 8F, FontStyle.Bold));
                RectangleF badgeRect = new RectangleF(e.CellBounds.X + 5, e.CellBounds.Y + (e.CellBounds.Height - textSize.Height - 10) / 2, textSize.Width + 20, textSize.Height + 10);

                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillRoundedRectangle(brush, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height, 10);
                }

                using (var brush = new SolidBrush(textColor))
                {
                    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(status.ToUpper(), new Font("Segoe UI", 8F, FontStyle.Bold), brush, badgeRect, format);
                }

                e.Handled = true;
            }
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
            LoadData(); // Redraw cards and grid
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            
            btnAdd.BackColor = ThemeManager.Sidebar;
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatAppearance.BorderSize = 0;

            btnPermissions.BackColor = ThemeManager.CardBackground;
            btnPermissions.ForeColor = ThemeManager.TextPrimary;
            btnPermissions.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            txtSearch.BackColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;

            cbDept.BackColor = ThemeManager.TextBoxBackground;
            cbDept.ForeColor = ThemeManager.TextPrimary;
            cbStatus.BackColor = ThemeManager.TextBoxBackground;
            cbStatus.ForeColor = ThemeManager.TextPrimary;

            dgvStaff.BackgroundColor = ThemeManager.CardBackground;
            dgvStaff.GridColor = ThemeManager.TextBoxBorder;
            dgvStaff.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvStaff.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvStaff.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvStaff.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvStaff.EnableHeadersVisualStyles = false;
            dgvStaff.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvStaff.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvStaff.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }
}
