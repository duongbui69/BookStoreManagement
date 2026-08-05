using BookStoreManagement.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Models;
using BookStoreManagement.Themes;
using BookStoreManagement.Interfaces;
using BookStoreManagement.Forms;
using System.Collections.Generic;
using System.Linq;

namespace BookStoreManagement.UserControls
{
    public partial class CustomerControl : UserControl, ISearchableControl
    {
        private readonly CustomerService _service;

        private Panel pnlContent;
        private Guna.UI2.WinForms.Guna2Panel pnlPageHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        
        private Guna.UI2.WinForms.Guna2Panel pnlFilters;
        private Guna.UI2.WinForms.Guna2TextBox txtSearch;
        private Guna.UI2.WinForms.Guna2ComboBox cboStatus;
        private Guna.UI2.WinForms.Guna2Button btnDeleteMultiple;
        private Guna.UI2.WinForms.Guna2Button btnAdd;

        private Panel pnlGridContainer;
        private DataGridView dgvCustomers;

        private PaginationControl pagination;

        private int _currentPage = 1;
        private int _pageSize = 5;
        private string _currentSearchTerm = "";
        
        private int _hoveredRowIndex = -1;
        private int _hoveredAction = 0; // 1: Edit, 2: Delete

        public async void PerformSearch(string keyword)
        {
            _currentSearchTerm = keyword;
            _currentPage = 1;
            await LoadDataAsync();
        }

        public CustomerControl()
        {
            _service = new CustomerService();
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += CustomerControl_Load;
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);
            this.AutoScroll = false; // Disable scrolling on outer container

            pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = false };

            // 1. Header
            pnlPageHeader = new Guna.UI2.WinForms.Guna2Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(0, 0, 0, 20) };

            lblTitle = new Label { Text = "Quản lý Khách hàng", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            lblSubTitle = new Label { Text = "Quản lý khách hàng, điểm thưởng và trạng thái.", Font = new Font("Segoe UI", 11F), AutoSize = true, Location = new Point(2, 40) };
            pnlPageHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            // 2. Filters Bar
            pnlFilters = new Guna.UI2.WinForms.Guna2Panel { Dock = DockStyle.Top, Height = 60, Margin = new Padding(0) };
            
            txtSearch = new Guna.UI2.WinForms.Guna2TextBox
            {
                PlaceholderText = "Tìm theo tên, email, sđt...",
                Size = new Size(250, 36),
                Location = new Point(0, 12),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F)
            };
            txtSearch.TextChanged += (s, e) => { PerformSearch(txtSearch.Text); };

            cboStatus = new Guna.UI2.WinForms.Guna2ComboBox
            {
                Size = new Size(160, 36),
                Location = new Point(260, 12),
                BorderRadius = 6,
                Font = new Font("Segoe UI", 9F)
            };
            cboStatus.Items.AddRange(new string[] { "Tất cả trạng thái", "Đang hoạt động", "Ngừng hoạt động" });
            cboStatus.SelectedIndex = 0;
            cboStatus.SelectedIndexChanged += async (s, e) => { _currentPage = 1; await LoadDataAsync(); };

            btnDeleteMultiple = new Guna.UI2.WinForms.Guna2Button { Text = "Xóa mục đã chọn", Size = new Size(140, 36), BorderRadius = 6, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Visible = false };
            btnDeleteMultiple.Click += BtnDeleteMultiple_Click;

            btnAdd = new Guna.UI2.WinForms.Guna2Button { Text = "+ Thêm Khách hàng", Size = new Size(160, 36), BorderRadius = 6, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.Click += BtnAdd_Click;

            pnlFilters.Controls.AddRange(new Control[] { txtSearch, cboStatus, btnDeleteMultiple, btnAdd });
            pnlFilters.Resize += (s, e) =>
            {
                btnAdd.Location = new Point(pnlFilters.Width - 160, 12);
                btnDeleteMultiple.Location = new Point(pnlFilters.Width - 310, 12);
            };

            // 3. Grid Container
            pnlGridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1) };
            
            dgvCustomers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false, // Must be false for checkbox to work
                RowHeadersVisible = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ScrollBars = ScrollBars.Both // Allow inner scroll
            };
            dgvCustomers.SetDoubleBuffered(true);
            dgvCustomers.RowTemplate.Height = 50;
            dgvCustomers.ColumnHeadersHeight = 45;

            // Set up columns
            DataGridViewCheckBoxColumn chkCol = new DataGridViewCheckBoxColumn();
            chkCol.Name = "chkSelect";
            chkCol.HeaderText = "";
            chkCol.Width = 40;
            chkCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvCustomers.Columns.Add(chkCol);

            dgvCustomers.Columns.Add("Id", "Id");
            dgvCustomers.Columns["Id"].Visible = false;
            dgvCustomers.Columns["Id"].ReadOnly = true;

            dgvCustomers.Columns.Add("CustomerCode", "Mã Khách hàng");
            dgvCustomers.Columns.Add("FullName", "Họ và Tên");
            dgvCustomers.Columns.Add("SĐT", "SĐT");
            dgvCustomers.Columns.Add("Email", "Email");
            dgvCustomers.Columns.Add("Địa chỉ", "Địa chỉ");
            dgvCustomers.Columns.Add("Điểm", "Điểm");
            dgvCustomers.Columns.Add("Trạng thái", "Trạng thái");
            dgvCustomers.Columns.Add("Actions", "Thao tác");

            foreach (DataGridViewColumn col in dgvCustomers.Columns)
            {
                if (col.Name != "chkSelect")
                {
                    col.ReadOnly = true;
                }
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            dgvCustomers.Columns["Actions"].Width = 100;
            dgvCustomers.Columns["Actions"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            dgvCustomers.CellPainting += DgvCustomers_CellPainting;
            dgvCustomers.CellMouseEnter += DgvCustomers_CellMouseEnter;
            dgvCustomers.CellMouseLeave += DgvCustomers_CellMouseLeave;
            dgvCustomers.CellClick += DgvCustomers_CellClick;
            dgvCustomers.CurrentCellDirtyStateChanged += DgvCustomers_CurrentCellDirtyStateChanged;
            dgvCustomers.CellValueChanged += DgvCustomers_CellValueChanged;
            
            // 4. Pagination
            pagination = new PaginationControl { Dock = DockStyle.Bottom, Height = 50 };
            pagination.PageChanged += async (s, args) => { _currentPage = args.NewPage; await LoadDataAsync(); };

            // Layout assembly
            pnlGridContainer.Controls.Add(dgvCustomers);
            
            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10 }); // Spacer
            pnlContent.Controls.Add(pnlFilters);
            pnlContent.Controls.Add(pnlPageHeader);
            pnlContent.Controls.Add(pagination);
            
            pnlGridContainer.BringToFront();

            this.Controls.Add(pnlContent);
            ApplyTheme();
        }

        private async void CustomerControl_Load(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
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
            if (cboStatus != null)
            {
                cboStatus.FillColor = ThemeManager.TextBoxBackground;
                cboStatus.ForeColor = ThemeManager.TextPrimary;
                cboStatus.BorderColor = ThemeManager.TextBoxBorder;
            }

            btnAdd.FillColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;

            btnDeleteMultiple.FillColor = Color.Red;
            btnDeleteMultiple.ForeColor = Color.White;

            pnlGridContainer.BackColor = ThemeManager.TextBoxBorder;

            ThemeManager.ApplyDataGridViewStyle(dgvCustomers);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var customers = await _service.SearchAsync(_currentSearchTerm);
                
                if (cboStatus != null)
                {
                    if (cboStatus.SelectedIndex == 1) // Đang hoạt động
                        customers = customers.Where(c => c.IsActive).ToList();
                    else if (cboStatus.SelectedIndex == 2) // Ngừng hoạt động
                        customers = customers.Where(c => !c.IsActive).ToList();
                }

                int totalRecords = customers.Count;
                int totalPages = (int)Math.Ceiling(totalRecords / (double)_pageSize);
                
                if (totalPages > 0 && _currentPage > totalPages) _currentPage = totalPages;
                if (_currentPage < 1) _currentPage = 1;

                var pagedData = customers.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

                if (this.IsDisposed) return;
                dgvCustomers.Rows.Clear();
                foreach (var cus in pagedData)
                {
                    dgvCustomers.Rows.Add(
                        false,
                        cus.Id,
                        cus.CustomerCode,
                        cus.FullName,
                        cus.Phone,
                        cus.Email,
                        cus.Address,
                        cus.Points,
                        cus.IsActive ? "Đang hoạt động" : "Ngừng hoạt động",
                        ""
                    );
                }

                pagination.UpdatePagination(totalRecords, _currentPage, _pageSize);
                UpdateDeleteMultipleButtonVisibility();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            using (var form = new CustomerForm(null))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            }
        }

        private void DgvCustomers_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.IsCurrentCellDirty && dgvCustomers.CurrentCell.OwningColumn.Name == "chkSelect")
            {
                dgvCustomers.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvCustomers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvCustomers.Columns[e.ColumnIndex].Name == "chkSelect")
            {
                UpdateDeleteMultipleButtonVisibility();
            }
        }

        private void UpdateDeleteMultipleButtonVisibility()
        {
            bool hasChecked = false;
            foreach (DataGridViewRow row in dgvCustomers.Rows)
            {
                if (Convert.ToBoolean(row.Cells["chkSelect"].Value) == true)
                {
                    hasChecked = true;
                    break;
                }
            }
            btnDeleteMultiple.Visible = hasChecked;
        }

        private async void BtnDeleteMultiple_Click(object sender, EventArgs e)
        {
            var selectedIds = new List<int>();
            foreach (DataGridViewRow row in dgvCustomers.Rows)
            {
                if (Convert.ToBoolean(row.Cells["chkSelect"].Value) == true)
                {
                    selectedIds.Add(Convert.ToInt32(row.Cells["Id"].Value));
                }
            }

            if (selectedIds.Count > 0)
            {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa {selectedIds.Count} khách hàng đã chọn không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        await _service.DeleteMultipleAsync(selectedIds);
                        MessageBox.Show("Đã xóa thành công.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi xóa khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DgvCustomers_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCustomers.Columns["Actions"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                
                int iconSize = 20;
                int margin = 8;
                int totalWidth = (iconSize * 2) + margin;
                int startX = e.CellBounds.Left + (e.CellBounds.Width - totalWidth) / 2;
                int startY = e.CellBounds.Top + (e.CellBounds.Height - iconSize) / 2;

                Rectangle rectEdit = new Rectangle(startX, startY, iconSize, iconSize);
                Rectangle rectDelete = new Rectangle(startX + iconSize + margin, startY, iconSize, iconSize);

                bool isHoveringEdit = (_hoveredRowIndex == e.RowIndex && _hoveredAction == 1);
                bool isHoveringDelete = (_hoveredRowIndex == e.RowIndex && _hoveredAction == 2);

                DrawIcon(e.Graphics, rectEdit, "\uE70F", isHoveringEdit ? ThemeManager.ButtonFill : ThemeManager.TextSecondary); // Edit icon (pencil)
                DrawIcon(e.Graphics, rectDelete, "\uE74D", isHoveringDelete ? Color.Red : ThemeManager.TextSecondary); // Delete icon (trash)

                e.Handled = true;
            }
            
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCustomers.Columns["Trạng thái"].Index && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                string status = e.Value.ToString() ?? "";
                
                Color bgColor = status == "Đang hoạt động" ? Color.FromArgb(20, 34, 197, 94) : Color.FromArgb(20, 239, 68, 68);
                Color textColor = status == "Đang hoạt động" ? Color.FromArgb(34, 197, 94) : Color.FromArgb(239, 68, 68);
                
                using (GraphicsPath path = new GraphicsPath())
                {
                    int width = 120;
                    int height = 26;
                    int x = e.CellBounds.Left + (e.CellBounds.Width - width) / 2;
                    int y = e.CellBounds.Top + (e.CellBounds.Height - height) / 2;
                    int radius = 13;
                    
                    path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
                    path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
                    path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
                    path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
                    path.CloseFigure();
                    
                    using (SolidBrush brush = new SolidBrush(bgColor))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                    }
                    
                    using (var font = new Font("Segoe UI", 9F, FontStyle.Regular))
                    {
                    TextRenderer.DrawText(e.Graphics, status, font, e.CellBounds, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
                e.Handled = true;
            }
        }

        private void DrawIcon(Graphics g, Rectangle rect, string iconCode, Color color)
        {
            using (Font iconFont = new Font("Segoe MDL2 Assets", 12F, FontStyle.Regular))
            {
                TextRenderer.DrawText(g, iconCode, iconFont, rect, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DgvCustomers_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCustomers.Columns["Actions"].Index)
            {
                dgvCustomers.Cursor = Cursors.Hand;
            }
        }

        private void DgvCustomers_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCustomers.Columns["Actions"].Index)
            {
                dgvCustomers.Cursor = Cursors.Default;
                _hoveredRowIndex = -1;
                _hoveredAction = 0;
                dgvCustomers.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }

        private async void DgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCustomers.Columns["Actions"].Index)
            {
                Rectangle cellBounds = dgvCustomers.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                Point mousePos = dgvCustomers.PointToClient(Cursor.Position);
                
                int iconSize = 20;
                int margin = 8;
                int totalWidth = (iconSize * 2) + margin;
                int startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2;
                int startY = cellBounds.Top + (cellBounds.Height - iconSize) / 2;

                Rectangle rectEdit = new Rectangle(startX, startY, iconSize, iconSize);
                Rectangle rectDelete = new Rectangle(startX + iconSize + margin, startY, iconSize, iconSize);

                if (rectEdit.Contains(mousePos))
                {
                    int id = Convert.ToInt32(dgvCustomers.Rows[e.RowIndex].Cells["Id"].Value);
                    var customer = await _service.GetByIdAsync(id);
                    if (customer != null)
                    {
                        using (var form = new CustomerForm(customer))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                            {
                                await LoadDataAsync();
                            }
                        }
                    }
                }
                else if (rectDelete.Contains(mousePos))
                {
                    int id = Convert.ToInt32(dgvCustomers.Rows[e.RowIndex].Cells["Id"].Value);
                    string name = dgvCustomers.Rows[e.RowIndex].Cells["FullName"].Value?.ToString() ?? "";
                    var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng '{name}'?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            await _service.DeleteAsync(id);
                            await LoadDataAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi xóa: " + ex.Message);
                        }
                    }
                }
            }
        }
    }
}