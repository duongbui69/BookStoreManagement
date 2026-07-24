using BookStoreManagement.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using BookStoreManagement.Forms;
using BookStoreManagement.Interfaces;

namespace BookStoreManagement.UserControls
{
    public partial class MasterDataControl : UserControl, ISearchableControl
    {
        public enum MasterDataType
        {
            Category,
            Author,
            Publisher
        }

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private ComboBox cboDataType;
        private Button btnExport;
        private Button btnAdd;

        private Panel pnlFilter;
        private Label lblShow;
        private ComboBox cboPageSize;
        private TextBox txtSearch;

        private DataGridView dgvData;
        private Panel pnlPagination;
        private Label lblPageInfo;
        private FlowLayoutPanel flpPagination;

        private CategoryRepository _categoryRepository;
        private AuthorRepository _authorRepository;
        private PublisherRepository _publisherRepository;

        private int currentPage = 1;
        private int pageSize = 5;
        private int totalRecords = 0;
        private MasterDataType currentDataType = MasterDataType.Category;

        // Current data lists
        private List<CategoryItem> currentCategories = new List<CategoryItem>();
        private List<Author> currentAuthors = new List<Author>();
        private List<Publisher> currentPublishers = new List<Publisher>();

        public MasterDataControl(MasterDataType dataType = MasterDataType.Category)
        {
            InitializeComponents();
            _categoryRepository = new CategoryRepository();
            _authorRepository = new AuthorRepository();
            _publisherRepository = new PublisherRepository();
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            
            cboDataType.Visible = false; // Hide the combobox
            currentDataType = dataType;
            
            if (currentDataType == MasterDataType.Category)
            {
                lblTitle.Text = "Quản lý Thể loại";
                lblSubtitle.Text = "Quản lý danh sách thể loại sách";
            }
            else if (currentDataType == MasterDataType.Author)
            {
                lblTitle.Text = "Quản lý Tác giả";
                lblSubtitle.Text = "Quản lý danh sách tác giả";
            }
            else if (currentDataType == MasterDataType.Publisher)
            {
                lblTitle.Text = "Quản lý Nhà xuất bản";
                lblSubtitle.Text = "Quản lý danh sách nhà xuất bản";
            }

            SetupDataGridViewColumns();
            LoadData();
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24);

            // Header Panel
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60 };
            
            lblTitle = new Label 
            { 
                Text = "Danh sách danh mục", 
                Font = new Font("Segoe UI", 24F, FontStyle.Bold), 
                AutoSize = true, 
                Location = new Point(0, 0) 
            };
            
            lblSubtitle = new Label 
            { 
                Text = "Quản lý và phân loại dữ liệu nền", 
                Font = new Font("Segoe UI", 10), 
                AutoSize = true, 
                Location = new Point(0, 30) 
            };

            cboDataType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Width = 200,
                Location = new Point(0, 0) // Position will be updated in layout
            };
            cboDataType.Items.AddRange(new string[] { "Thể loại sách", "Tác giả", "Nhà xuất bản" });
            cboDataType.SelectedIndexChanged += CboDataType_SelectedIndexChanged;

            btnAdd = new Button 
            { 
                Text = "+ Thêm mới", 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                Width = 120, 
                Height = 36, 
                FlatStyle = FlatStyle.Flat 
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            btnExport = new Button 
            { 
                Text = "Xuất Excel", 
                Font = new Font("Segoe UI", 10), 
                Width = 110, 
                Height = 36, 
                FlatStyle = FlatStyle.Flat 
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(cboDataType);
            pnlHeader.Controls.Add(btnExport);
            pnlHeader.Controls.Add(btnAdd);
            pnlHeader.Resize += PnlHeader_Resize;

            this.Controls.Add(pnlHeader);

            // Data Container
            Panel pnlDataContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 20, 0, 0) };

            // Filter Panel
            pnlFilter = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(16) };
            
            lblShow = new Label { Text = "Hiển thị", AutoSize = true, Location = new Point(16, 20) };
            cboPageSize = new ComboBox 
            { 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Width = 60, 
                Location = new Point(80, 16) 
            };
            cboPageSize.Items.AddRange(new string[] { "5", "10", "20", "50" });
            cboPageSize.SelectedIndex = 0;
            cboPageSize.SelectedIndexChanged += CboPageSize_SelectedIndexChanged;

            txtSearch = new TextBox 
            { 
                Width = 250, 
                Location = new Point(0, 16),
                Font = new Font("Segoe UI", 10)
            };
            // Placeholder text logic
            txtSearch.Text = "Lọc dữ liệu...";
            txtSearch.ForeColor = Color.Gray;
            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text == "Lọc dữ liệu...") { txtSearch.Text = ""; txtSearch.ForeColor = ThemeManager.TextPrimary; } };
            txtSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Lọc dữ liệu..."; txtSearch.ForeColor = Color.Gray; } };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            pnlFilter.Controls.Add(lblShow);
            pnlFilter.Controls.Add(cboPageSize);
            pnlFilter.Controls.Add(txtSearch);
            pnlFilter.Resize += PnlFilter_Resize;

            pnlDataContainer.Controls.Add(pnlFilter);

            // DataGridView
            dgvData = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowTemplate = { Height = 45 }
            };
            dgvData.SetDoubleBuffered(true);
            dgvData.CellPainting += DgvData_CellPainting;
            dgvData.CellClick += DgvData_CellClick;
            dgvData.CellMouseMove += DgvData_CellMouseMove;
            dgvData.CellMouseLeave += DgvData_CellMouseLeave;
            dgvData.Cursor = Cursors.Hand;
            pnlDataContainer.Controls.Add(dgvData);

            // Pagination Panel
            pnlPagination = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(16) };
            lblPageInfo = new Label { AutoSize = true, Location = new Point(16, 20), Font = new Font("Segoe UI", 9) };
            flpPagination = new FlowLayoutPanel 
            { 
                AutoSize = true, 
                FlowDirection = FlowDirection.LeftToRight, 
                Location = new Point(0, 12) 
            };
            
            pnlPagination.Controls.Add(lblPageInfo);
            pnlPagination.Controls.Add(flpPagination);
            pnlPagination.Resize += PnlPagination_Resize;

            pnlDataContainer.Controls.Add(pnlPagination);
            pnlDataContainer.Controls.Add(dgvData);
            pnlDataContainer.Controls.Add(pnlFilter);

            this.Controls.Add(pnlDataContainer);
            this.Controls.Add(pnlHeader);
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubtitle.ForeColor = ThemeManager.TextSecondary;
            
            btnAdd.BackColor = ThemeManager.ButtonFill;
            btnAdd.ForeColor = ThemeManager.ButtonText;
            
            btnExport.BackColor = ThemeManager.CardBackground;
            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            cboDataType.BackColor = ThemeManager.CardBackground;
            cboDataType.ForeColor = ThemeManager.TextPrimary;

            pnlFilter.BackColor = ThemeManager.CardBackground;
            lblShow.ForeColor = ThemeManager.TextSecondary;
            cboPageSize.BackColor = ThemeManager.CardBackground;
            cboPageSize.ForeColor = ThemeManager.TextPrimary;
            
            if (txtSearch.Text != "Lọc dữ liệu...")
                txtSearch.ForeColor = ThemeManager.TextPrimary;
            txtSearch.BackColor = ThemeManager.CardBackground;

            dgvData.BackgroundColor = ThemeManager.CardBackground;
            dgvData.GridColor = ThemeManager.TextBoxBorder;
            dgvData.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvData.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvData.DefaultCellStyle.SelectionBackColor = ThemeManager.ButtonFill;
            dgvData.DefaultCellStyle.SelectionForeColor = ThemeManager.ButtonText;
            dgvData.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background; // Use background instead of Surface to avoid error
            dgvData.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvData.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Background;

            pnlPagination.BackColor = ThemeManager.CardBackground;
            lblPageInfo.ForeColor = ThemeManager.TextSecondary;

            UpdatePagination();
            dgvData.Invalidate();
        }

        private void PnlHeader_Resize(object sender, EventArgs e)
        {
            int gap = 10;
            btnAdd.Location = new Point(pnlHeader.Width - btnAdd.Width, 10);
            btnExport.Location = new Point(btnAdd.Left - gap - btnExport.Width, 10);
            cboDataType.Location = new Point(btnExport.Left - gap - 20 - cboDataType.Width, 15);
        }

        private void PnlFilter_Resize(object sender, EventArgs e)
        {
            txtSearch.Location = new Point(pnlFilter.Width - txtSearch.Width - 16, 16);
        }

        private void PnlPagination_Resize(object sender, EventArgs e)
        {
            flpPagination.Location = new Point(pnlPagination.Width - flpPagination.Width - 16, 12);
        }

        private void CboDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDataType = (MasterDataType)cboDataType.SelectedIndex;
            currentPage = 1;
            SetupDataGridViewColumns();
            LoadData();
        }

        private void SetupDataGridViewColumns()
        {
            dgvData.Columns.Clear();

            // STT Column
            dgvData.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                HeaderText = "#", 
                Name = "colSTT", 
                Width = 50, 
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } }
            });

            if (currentDataType == MasterDataType.Category)
            {
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã DM", DataPropertyName = "CategoryCode", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên danh mục", DataPropertyName = "CategoryName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số lượng sách", DataPropertyName = "BookCount", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mô tả", DataPropertyName = "Description", Width = 250, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", Name = "colStatus", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            }
            else if (currentDataType == MasterDataType.Author)
            {
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã Tác giả", DataPropertyName = "Id", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên Tác giả", DataPropertyName = "AuthorName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mô tả", DataPropertyName = "Description", Width = 300, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", Name = "colStatus", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            }
            else if (currentDataType == MasterDataType.Publisher)
            {
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã NXB", DataPropertyName = "Id", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên Nhà xuất bản", DataPropertyName = "PublisherName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số điện thoại", DataPropertyName = "Phone", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = "Email", Width = 200, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
                dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", Name = "colStatus", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } } });
            }

            dgvData.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Action",
                Name = "colAction",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = new DataGridViewColumnHeaderCell { Style = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } }
            });
        }

        private async void LoadData()
        {
            string search = txtSearch.Text == "Lọc dữ liệu..." ? "" : txtSearch.Text;

            if (currentDataType == MasterDataType.Category)
            {
                var result = await _categoryRepository.GetPagedCategoriesAsync(currentPage, pageSize, search, "");
                totalRecords = result.TotalCount;
                currentCategories = result.Items;
                dgvData.DataSource = currentCategories;
            }
            else if (currentDataType == MasterDataType.Author)
            {
                var allAuthors = await _authorRepository.GetAllAsync();
                var query = allAuthors.AsQueryable();
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(a => a.AuthorName.Contains(search, StringComparison.OrdinalIgnoreCase));
                }
                totalRecords = query.Count();
                currentAuthors = query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                dgvData.DataSource = currentAuthors;
            }
            else if (currentDataType == MasterDataType.Publisher)
            {
                var allPublishers = await _publisherRepository.GetAllAsync();
                var query = allPublishers.AsQueryable();
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.PublisherName.Contains(search, StringComparison.OrdinalIgnoreCase));
                }
                totalRecords = query.Count();
                currentPublishers = query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                dgvData.DataSource = currentPublishers;
            }

            // Fill STT
            for (int i = 0; i < dgvData.Rows.Count; i++)
            {
                dgvData.Rows[i].Cells["colSTT"].Value = (currentPage - 1) * pageSize + i + 1;
            }

            UpdatePagination();
        }

        private void CboPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(cboPageSize.SelectedItem.ToString(), out int size))
            {
                pageSize = size;
                currentPage = 1;
                LoadData();
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text != "Lọc dữ liệu...")
            {
                currentPage = 1;
                LoadData();
            }
        }

        public void PerformSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                txtSearch.Text = "Lọc dữ liệu...";
                txtSearch.ForeColor = Color.Gray;
            }
            else
            {
                txtSearch.Text = query;
                txtSearch.ForeColor = ThemeManager.TextPrimary;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new MasterDataForm(currentDataType.ToString(), null))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        // Custom drawing for Status and Action buttons
        private void DgvData_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

                if (dgvData.Columns[e.ColumnIndex].Name == "colStatus")
                {
                    bool isActive = true;
                    if (currentDataType == MasterDataType.Category && currentCategories.Count > e.RowIndex)
                        isActive = currentCategories[e.RowIndex].IsActive;
                    else if (currentDataType == MasterDataType.Author && currentAuthors.Count > e.RowIndex)
                        isActive = currentAuthors[e.RowIndex].IsActive;
                    else if (currentDataType == MasterDataType.Publisher && currentPublishers.Count > e.RowIndex)
                        isActive = currentPublishers[e.RowIndex].IsActive;

                    string text = isActive ? "Hoạt động" : "Tạm khóa";
                    Color bgColor = isActive ? Color.FromArgb(220, 252, 231) : Color.FromArgb(254, 226, 226);
                    Color fgColor = isActive ? Color.FromArgb(22, 163, 74) : Color.FromArgb(220, 38, 38);

                    // Draw pill
                    Rectangle rect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width - 80) / 2, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, 80, 24);
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        int radius = 12;
                        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        path.CloseFigure();
                        using (SolidBrush brush = new SolidBrush(bgColor))
                        {
                            e.Graphics.FillPath(brush, path);
                        }
                    }

                    using (var font = new Font("Segoe UI", 9, FontStyle.Regular))
                    {
                    TextRenderer.DrawText(e.Graphics, text, font, e.CellBounds, fgColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                    e.Handled = true;
                }
                else if (dgvData.Columns[e.ColumnIndex].Name == "colAction")
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                    
                    var rect = e.CellBounds;
                    var editRect = new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height);
                    var delRect = new Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height);
                    
                    if (e.RowIndex == hoveredRow)
                    {
                        if (hoveredAction == 1)
                        {
                            using (var brush = new SolidBrush(Color.FromArgb(30, ThemeManager.ButtonFill)))
                                e.Graphics.FillRectangle(brush, editRect);
                        }
                        else if (hoveredAction == 2)
                        {
                            using (var brush = new SolidBrush(Color.FromArgb(30, Color.FromArgb(231, 76, 60))))
                                e.Graphics.FillRectangle(brush, delRect);
                        }
                    }

                    int editFontSize = (hoveredRow == e.RowIndex && hoveredAction == 1) ? 14 : 12;
                    int delFontSize = (hoveredRow == e.RowIndex && hoveredAction == 2) ? 14 : 12;

                    using (var font = new Font("Segoe UI Emoji", editFontSize)) { TextRenderer.DrawText(e.Graphics, "✏️", font, editRect, ThemeManager.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter); }
                    using (var font = new Font("Segoe UI Emoji", delFontSize)) { TextRenderer.DrawText(e.Graphics, "🗑️", font, delRect, Color.FromArgb(231, 76, 60), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter); }
                    
                    using (var pen = new Pen(Color.LightGray))
                        e.Graphics.DrawLine(pen, rect.X + rect.Width / 2, rect.Y + 5, rect.X + rect.Width / 2, rect.Bottom - 5);

                    e.Handled = true;
                }
                else
                {
                    e.PaintContent(e.CellBounds);
                }
            }
        }

        private int hoveredRow = -1;
        private int hoveredCol = -1;
        private int hoveredAction = 0; // 0=none, 1=edit, 2=delete

        private void DgvData_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvData.Columns[e.ColumnIndex].Name == "colAction")
            {
                int action = (e.X < dgvData.Columns[e.ColumnIndex].Width / 2) ? 1 : 2;
                
                if (hoveredRow != e.RowIndex || hoveredAction != action)
                {
                    int oldRow = hoveredRow;
                    hoveredRow = e.RowIndex;
                    hoveredAction = action;
                    
                    if (oldRow >= 0) dgvData.InvalidateCell(e.ColumnIndex, oldRow);
                    dgvData.InvalidateCell(e.ColumnIndex, hoveredRow);
                }
                dgvData.Cursor = Cursors.Hand;
            }
            else
            {
                if (hoveredRow >= 0)
                {
                    int oldRow = hoveredRow;
                    hoveredRow = -1;
                    hoveredAction = 0;
                    if (e.ColumnIndex >= 0) dgvData.InvalidateCell(dgvData.Columns["colAction"].Index, oldRow);
                }
                dgvData.Cursor = Cursors.Default;
            }
        }

        private void DgvData_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (hoveredRow >= 0)
            {
                int oldRow = hoveredRow;
                hoveredRow = -1;
                hoveredAction = 0;
                dgvData.InvalidateCell(dgvData.Columns["colAction"].Index, oldRow);
            }
            dgvData.Cursor = Cursors.Default;
        }

        private async void DgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvData.Columns[e.ColumnIndex].Name == "colAction")
            {
                Rectangle cellRect = dgvData.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                Point mousePos = dgvData.PointToClient(Cursor.Position);
                Point mouseInCell = new Point(mousePos.X - cellRect.X, mousePos.Y - cellRect.Y);

                int iconSize = 20;
                int spacing = 10;
                int totalWidth = iconSize * 2 + spacing;
                int startX = (cellRect.Width - totalWidth) / 2;

                int id = 0;
                if (currentDataType == MasterDataType.Category) id = currentCategories[e.RowIndex].Id;
                else if (currentDataType == MasterDataType.Author) id = currentAuthors[e.RowIndex].Id;
                else if (currentDataType == MasterDataType.Publisher) id = currentPublishers[e.RowIndex].Id;

                if (mouseInCell.X >= startX && mouseInCell.X <= startX + iconSize)
                {
                    // Edit
                    using (var form = new MasterDataForm(currentDataType.ToString(), id))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            LoadData();
                        }
                    }
                }
                else if (mouseInCell.X >= startX + iconSize + spacing && mouseInCell.X <= startX + totalWidth)
                {
                    // Delete
                    var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa bản ghi này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        bool success = false;
                        if (currentDataType == MasterDataType.Category) success = await _categoryRepository.DeleteAsync(id);
                        else if (currentDataType == MasterDataType.Author) success = await _authorRepository.DeleteAsync(id);
                        else if (currentDataType == MasterDataType.Publisher) success = await _publisherRepository.DeleteAsync(id);

                        if (success)
                        {
                            MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Lỗi khi xóa. Bản ghi có thể đang được sử dụng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void UpdatePagination()
        {
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (totalPages == 0) totalPages = 1;
            
            int startRecord = (currentPage - 1) * pageSize + 1;
            int endRecord = Math.Min(currentPage * pageSize, totalRecords);
            
            lblPageInfo.Text = totalRecords == 0 ? "Không có dữ liệu" : $"Hiển thị {startRecord} - {endRecord} của {totalRecords} bản ghi";

            flpPagination.Controls.Clear();

            // Prev Button
            Button btnPrev = CreatePageButton("<", currentPage > 1);
            btnPrev.Click += (s, e) => { if (currentPage > 1) { currentPage--; LoadData(); } };
            flpPagination.Controls.Add(btnPrev);

            // Page Numbers
            for (int i = 1; i <= totalPages; i++)
            {
                if (i == 1 || i == totalPages || (i >= currentPage - 1 && i <= currentPage + 1))
                {
                    Button btnPage = CreatePageButton(i.ToString(), true, i == currentPage);
                    int pageNum = i;
                    btnPage.Click += (s, e) => { currentPage = pageNum; LoadData(); };
                    flpPagination.Controls.Add(btnPage);
                }
                else if (i == currentPage - 2 || i == currentPage + 2)
                {
                    Label lblDots = new Label { Text = "...", Width = 30, TextAlign = ContentAlignment.MiddleCenter, ForeColor = ThemeManager.TextSecondary };
                    flpPagination.Controls.Add(lblDots);
                }
            }

            // Next Button
            Button btnNext = CreatePageButton(">", currentPage < totalPages);
            btnNext.Click += (s, e) => { if (currentPage < totalPages) { currentPage++; LoadData(); } };
            flpPagination.Controls.Add(btnNext);
            
            PnlPagination_Resize(null, null);
        }

        private Button CreatePageButton(string text, bool enabled, bool active = false)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 32,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                Enabled = enabled,
                Cursor = enabled ? Cursors.Hand : Cursors.Default,
                Font = new Font("Segoe UI", 9),
                Margin = new Padding(2)
            };

            if (active)
            {
                btn.BackColor = ThemeManager.ButtonFill;
                btn.ForeColor = ThemeManager.ButtonText;
                btn.FlatAppearance.BorderColor = ThemeManager.ButtonFill;
            }
            else
            {
                btn.BackColor = ThemeManager.CardBackground;
                btn.ForeColor = ThemeManager.TextPrimary;
                btn.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;
            }

            return btn;
        }
    }
}

