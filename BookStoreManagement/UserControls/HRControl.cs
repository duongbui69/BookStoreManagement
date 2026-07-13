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

        // Page Header
        private Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;

        // Bento Metric Cards
        private Guna2Panel pnlMetrics;
        private Guna2Panel cardEmployees;
        private Guna2Panel cardBranches;
        private Guna2Panel cardPerformance;
        private Guna2Panel cardLeaveRequests;

        // Filters Bar
        private Guna2Panel pnlFilters;
        private Guna2ComboBox cbBranch;
        private Guna2ComboBox cbRole;
        private Guna2Button btnPayroll;
        private Guna2Button btnAdd;

        // Grid
        private Guna2Panel pnlGridContainer;
        private Guna2DataGridView dgvEmployees;

        // Pagination
        private PaginationControl paginationControl;

        private int _currentPage = 1;
        private int _pageSize = 5;
        private string _currentBranch = "All Departments";
        private string _currentRole = "Status: All";
        private string _currentSearchTerm = "";

        public void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            LoadData();
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

            // 1. Page Header
            pnlPageHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0, 0, 0, gutter) };
            lblTitle = new Label { Text = "Employee Directory", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "View and manage all staff members across the organization.", Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(2, 40) };
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            // 2. Metric Cards
            pnlMetrics = new Guna2Panel { Dock = DockStyle.Top, Height = 100, Margin = new Padding(0, 0, 0, gutter) };
            cardEmployees = CreateMetricCard("Total Employees", "badge", "+0 this month");
            cardBranches = CreateMetricCard("Active Branches", "storefront", "");
            cardPerformance = CreateMetricCard("Avg Performance", "trending_up", "94%");
            cardLeaveRequests = CreateMetricCard("Pending Leaves", "event_busy", "0");
            
            pnlMetrics.Controls.AddRange(new Control[] { cardEmployees, cardBranches, cardPerformance, cardLeaveRequests });
            pnlMetrics.Resize += (s, e) => 
            {
                int cardWidth = (pnlMetrics.Width - (gutter * 3)) / 4;
                if (cardWidth > 0)
                {
                    cardEmployees.Width = cardWidth; cardEmployees.Left = 0;
                    cardBranches.Width = cardWidth; cardBranches.Left = cardWidth + gutter;
                    cardPerformance.Width = cardWidth; cardPerformance.Left = (cardWidth + gutter) * 2;
                    cardLeaveRequests.Width = cardWidth; cardLeaveRequests.Left = (cardWidth + gutter) * 3;
                }
            };

            // 3. Filters Bar
            pnlFilters = new Guna2Panel { Dock = DockStyle.Top, Height = 70, CustomBorderThickness = new Padding(1, 1, 1, 0), Margin = new Padding(0), BorderRadius = 6 };
            pnlFilters.CustomizableEdges.BottomLeft = false;
            pnlFilters.CustomizableEdges.BottomRight = false;

            cbBranch = new Guna2ComboBox { Size = new Size(180, 36), Location = new Point(20, 17), BorderRadius = 4, Font = new Font("Segoe UI", 9F) };
            cbBranch.Items.AddRange(new object[] { "All Departments", "Logistics", "IT", "Sales" });
            cbBranch.SelectedIndex = 0;
            cbBranch.SelectedIndexChanged += (s, e) => { _currentBranch = cbBranch.SelectedItem.ToString(); _currentPage = 1; LoadData(); };

            cbRole = new Guna2ComboBox { Size = new Size(180, 36), Location = new Point(220, 17), BorderRadius = 4, Font = new Font("Segoe UI", 9F) };
            cbRole.Items.AddRange(new object[] { "Status: All", "ACTIVE", "INACTIVE" });
            cbRole.SelectedIndex = 0;
            cbRole.SelectedIndexChanged += (s, e) => { _currentRole = cbRole.SelectedItem.ToString(); _currentPage = 1; LoadData(); };

            btnPayroll = new Guna2Button { Text = "Payroll", Size = new Size(120, 36), BorderRadius = 4, BorderThickness = 1, FillColor = Color.Transparent, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnPayroll.Click += (s, e) => MessageBox.Show("Payroll feature is under development.", "Info");

            btnAdd = new Guna2Button { Text = "+ Add Employee", Size = new Size(150, 36), BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnAdd.Click += (s, e) => MessageBox.Show("Add Employee form is under development.", "Info");

            pnlFilters.Controls.AddRange(new Control[] { cbBranch, cbRole, btnPayroll, btnAdd });
            pnlFilters.Resize += (s, e) =>
            {
                btnPayroll.Location = new Point(pnlFilters.Width - 300, 17);
                btnAdd.Location = new Point(pnlFilters.Width - 170, 17);
            };

            // 4. Grid Container
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
                Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default
            };
            
            // Define Columns
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Name = "Id", Visible = false });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", Name = "Name", HeaderText = "EMPLOYEE", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoleName", HeaderText = "ROLE", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 120 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Department", HeaderText = "BRANCH/DEPT", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 120 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", Name = "Status", HeaderText = "STATUS", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, Width = 100 });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", Name = "Email", Visible = false });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EmployeeId", Name = "EmployeeId", Visible = false });
            dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "JoinDate", HeaderText = "SCORE (JOIN DATE)", HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "MMM dd, yyyy" }, Width = 120 });
            
            DataGridViewTextBoxColumn actionCol = new DataGridViewTextBoxColumn { Name = "Actions", HeaderText = "ACTIONS", Width = 80, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } };
            dgvEmployees.Columns.Add(actionCol);

            dgvEmployees.CellPainting += DgvEmployees_CellPainting;
            dgvEmployees.CellMouseClick += DgvEmployees_CellMouseClick;
            
            pnlGridContainer.Controls.Add(dgvEmployees);

            // Pagination
            paginationControl = new PaginationControl { Dock = DockStyle.Bottom };
            paginationControl.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadData(); };
            pnlGridContainer.Controls.Add(paginationControl);

            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(pnlFilters);
            pnlContent.Controls.Add(pnlMetrics);
            pnlContent.Controls.Add(pnlPageHeader);

            this.Controls.Add(pnlContent);

            ApplyTheme();
        }

        private Guna2Panel CreateMetricCard(string title, string iconText, string value)
        {
            var card = new Guna2Panel { Height = 100, BorderRadius = 6, BorderThickness = 1 };
            
            var lblVal = new Label { Name = "ValueLabel", Text = value, Font = new Font("Segoe UI", 20F, FontStyle.Bold), AutoSize = true, Location = new Point(15, 45) };
            var lblTitle = new Label { Name = "TitleLabel", Text = title, Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(18, 15) };
            
            var lblIcon = new Label { Name = "IconLabel", Text = iconText, Font = new Font("Segoe UI", 12F), AutoSize = true, Location = new Point(card.Width - 40, 15), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            
            card.Controls.AddRange(new Control[] { lblVal, lblTitle, lblIcon });
            return card;
        }

        private void HRControl_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            var stats = _service.GetStats();
            
            cardEmployees.Controls["ValueLabel"].Text = stats.TotalEmployees.ToString();
            cardBranches.Controls["ValueLabel"].Text = stats.ActiveDepartments.ToString();
            cardPerformance.Controls["ValueLabel"].Text = "94%";
            cardLeaveRequests.Controls["ValueLabel"].Text = stats.NewThisMonth.ToString();

            var (items, totalCount) = _service.GetPagedEmployees(_currentPage, _pageSize, _currentBranch, _currentRole, _currentSearchTerm);
            dgvEmployees.DataSource = items;
            paginationControl.UpdatePagination(totalCount, _currentPage, _pageSize);
        }

        private void DgvEmployees_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvEmployees.Columns[e.ColumnIndex].Name == "Actions")
            {
                int empId = Convert.ToInt32(dgvEmployees.Rows[e.RowIndex].Cells["Id"].Value);
                var cellRect = dgvEmployees.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                
                if (e.X < cellRect.Width / 2)
                {
                    // Edit
                    MessageBox.Show("Edit Employee form is under development.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Delete
                    if (MessageBox.Show("Are you sure you want to delete this employee?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            _service.DeleteUser(empId);
                            MessageBox.Show("Employee deleted successfully!");
                            LoadData();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                
                TextRenderer.DrawText(e.Graphics, "✏️", e.CellStyle.Font, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                TextRenderer.DrawText(e.Graphics, "🗑️", e.CellStyle.Font, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                
                e.Handled = true;
            }
            // Custom Paint for Employee Name
            else if (dgvEmployees.Columns[e.ColumnIndex].Name == "Name")
            {
                e.PaintBackground(e.CellBounds, true);

                string name = e.Value?.ToString() ?? "Unknown";
                string email = dgvEmployees.Rows[e.RowIndex].Cells["EmployeeId"].Value?.ToString() ?? "";

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                
                using (var brush = new SolidBrush(ThemeManager.TextPrimary))
                {
                    g.DrawString(name, new Font("Segoe UI", 9.5F, FontStyle.Bold), brush, new RectangleF(e.CellBounds.X, e.CellBounds.Y + 12, e.CellBounds.Width, e.CellBounds.Height), format);
                }

                using (var brush = new SolidBrush(ThemeManager.TextSecondary))
                {
                    g.DrawString(email, new Font("Segoe UI", 8.5F), brush, new RectangleF(e.CellBounds.X, e.CellBounds.Y + 32, e.CellBounds.Width, e.CellBounds.Height), format);
                }

                e.Handled = true;
            }
            else if (dgvEmployees.Columns[e.ColumnIndex].Name == "Status")
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "";

                Color bgColor = ThemeManager.TextBoxBorder;
                Color textColor = ThemeManager.TextPrimary;

                if (status.ToUpper() == "ACTIVE") { bgColor = Color.FromArgb(40, 46, 204, 113); textColor = Color.FromArgb(46, 204, 113); }
                else if (status.ToUpper() == "INACTIVE" || status.ToUpper() == "TERMINATED") { bgColor = Color.FromArgb(40, 231, 76, 60); textColor = Color.FromArgb(231, 76, 60); }
                else { bgColor = Color.FromArgb(40, 41, 128, 185); textColor = Color.FromArgb(41, 128, 185); }

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                SizeF textSize = g.MeasureString(status, new Font("Segoe UI", 8F, FontStyle.Bold));
                RectangleF badgeRect = new RectangleF(e.CellBounds.X + (e.CellBounds.Width - textSize.Width - 20) / 2, e.CellBounds.Y + (e.CellBounds.Height - textSize.Height - 10) / 2, textSize.Width + 20, textSize.Height + 10);

                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillRoundedRectangle(brush, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height, 10);
                }

                using (var brush = new SolidBrush(textColor))
                {
                    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(status, new Font("Segoe UI", 8F, FontStyle.Bold), brush, badgeRect, format);
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

            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;

            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;

            btnPayroll.FillColor = ThemeManager.CardBackground;
            btnPayroll.ForeColor = ThemeManager.TextPrimary;
            btnPayroll.BorderColor = ThemeManager.TextBoxBorder;

            // Metrics Cards
            var cards = new[] { cardEmployees, cardBranches, cardPerformance, cardLeaveRequests };
            foreach (var card in cards)
            {
                card.FillColor = ThemeManager.CardBackground;
                card.CustomBorderColor = ThemeManager.TextBoxBorder;
                if (card.Controls["TitleLabel"] is Label lTitle) lTitle.ForeColor = ThemeManager.TextSecondary;
                if (card.Controls["ValueLabel"] is Label lVal) lVal.ForeColor = ThemeManager.TextPrimary;
            }

            // Filters
            pnlFilters.BackColor = ThemeManager.Background;
            pnlFilters.CustomBorderColor = ThemeManager.TextBoxBorder;
            pnlFilters.FillColor = ThemeManager.CardBackground;

            cbBranch.FillColor = ThemeManager.TextBoxBackground;
            cbBranch.ForeColor = ThemeManager.TextPrimary;
            cbBranch.BorderColor = ThemeManager.TextBoxBorder;

            cbRole.FillColor = ThemeManager.TextBoxBackground;
            cbRole.ForeColor = ThemeManager.TextPrimary;
            cbRole.BorderColor = ThemeManager.TextBoxBorder;

            // Grid
            pnlGridContainer.CustomBorderColor = ThemeManager.TextBoxBorder;
            dgvEmployees.BackgroundColor = ThemeManager.CardBackground;
            dgvEmployees.GridColor = ThemeManager.TextBoxBorder;
            dgvEmployees.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvEmployees.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvEmployees.DefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvEmployees.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            
            dgvEmployees.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvEmployees.AlternatingRowsDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvEmployees.AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeManager.HoverColor;
            dgvEmployees.AlternatingRowsDefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;

            dgvEmployees.EnableHeadersVisualStyles = false;
            dgvEmployees.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvEmployees.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvEmployees.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }
}
