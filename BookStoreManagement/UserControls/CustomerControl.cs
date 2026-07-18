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
        private TableLayoutPanel tlpHeader;
        private Label lblTitle;
        private Label lblSubTitle;
        private Button btnDeleteMultiple;
        private Button btnAdd;

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

            // 1. Header & Toolbar
            tlpHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                RowCount = 2,
                Height = 80,
                Margin = new Padding(0, 0, 0, 20)
            };
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            lblTitle = new Label { Text = "Quản lý Khách hàng", Font = new Font("Segoe UI", 24F, FontStyle.Bold), AutoSize = true, Margin = new Padding(0) };
            lblSubTitle = new Label { Text = "Quản lý thông tin, điểm thưởng và trạng thái khách hàng.", Font = new Font("Segoe UI", 10F), AutoSize = true, Margin = new Padding(2, 0, 0, 0) };
            
            btnDeleteMultiple = new Button { Text = "Xóa đã chọn", Size = new Size(130, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 10, 15, 0), Visible = false };
            btnDeleteMultiple.Click += BtnDeleteMultiple_Click;

            btnAdd = new Button { Text = "+ Thêm Khách hàng", Size = new Size(160, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 10, 0, 0) };
            btnAdd.Click += BtnAdd_Click;

            tlpHeader.Controls.Add(lblTitle, 0, 0);
            tlpHeader.Controls.Add(lblSubTitle, 0, 1);
            
            tlpHeader.Controls.Add(btnDeleteMultiple, 1, 0);
            tlpHeader.SetRowSpan(btnDeleteMultiple, 2);
            tlpHeader.Controls.Add(btnAdd, 2, 0);
            tlpHeader.SetRowSpan(btnAdd, 2);

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

            dgvCustomers.Columns.Add("CustomerCode", "Mã KH");
            dgvCustomers.Columns.Add("FullName", "Họ và Tên");
            dgvCustomers.Columns.Add("Phone", "SĐT");
            dgvCustomers.Columns.Add("Email", "Email");
            dgvCustomers.Columns.Add("Address", "Địa chỉ");
            dgvCustomers.Columns.Add("Points", "Điểm");
            dgvCustomers.Columns.Add("Status", "Trạng thái");
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

            pnlGridContainer.Controls.Add(dgvCustomers);
            
            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(tlpHeader);
            pnlContent.Controls.Add(pagination);

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
            this.BackColor = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            
            btnAdd.BackColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatAppearance.BorderSize = 0;

            btnDeleteMultiple.BackColor = Color.Red;
            btnDeleteMultiple.ForeColor = Color.White;
            btnDeleteMultiple.FlatAppearance.BorderSize = 0;

            pnlGridContainer.BackColor = ThemeManager.TextBoxBorder;

            dgvCustomers.BackgroundColor = ThemeManager.CardBackground;
            dgvCustomers.GridColor = ThemeManager.TextBoxBorder;
            
            dgvCustomers.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvCustomers.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvCustomers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvCustomers.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.CardBackground;

            dgvCustomers.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvCustomers.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvCustomers.DefaultCellStyle.SelectionBackColor = ThemeManager.CardBackground; // No selection color highlight
            dgvCustomers.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvCustomers.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            
            // Disable alternating colors explicitly to ensure 1 single background color
            dgvCustomers.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvCustomers.AlternatingRowsDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var customers = await _service.SearchAsync(_currentSearchTerm);
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
                        MessageBox.Show("Xóa thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvCustomers.Columns["Status"].Index && e.Value != null)
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
                    
                    TextRenderer.DrawText(e.Graphics, status, new Font("Segoe UI", 9F, FontStyle.Regular), e.CellBounds, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
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
                    string name = dgvCustomers.Rows[e.RowIndex].Cells["FullName"].Value.ToString();
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
