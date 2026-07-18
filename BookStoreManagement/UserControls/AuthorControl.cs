using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BookStoreManagement.Forms;
using BookStoreManagement.Interfaces;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.UserControls
{
    public partial class AuthorControl : UserControl, ISearchableControl
    {
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnExport;
        private Button btnAdd;
        private Panel pnlHeader;
        
        private DataGridView dgvData;
        private Panel pnlPagination;
        private Label lblPaginationInfo;
        private FlowLayoutPanel flpPagination;
        
        private AuthorRepository _authorRepository;
        private int currentPage = 1;
        private int pageSize = 5;
        private int totalRecords = 0;
        private string currentSearch = "";
        
        private List<AuthorItem> currentAuthors = new List<AuthorItem>();

        // For hover effects on Action buttons
        private int hoveredRow = -1;
        private int hoveredCol = -1;
        private int hoveredAction = 0; // 0: none, 1: edit, 2: delete

        public AuthorControl()
        {
            InitializeComponents();
            _authorRepository = new AuthorRepository();
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            
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
            this.Padding = new Padding(32);

            // Header Section
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80 };
            
            lblTitle = new Label 
            { 
                Text = "Quản lý tác giả", 
                Font = new Font("Segoe UI", 24F, FontStyle.Bold), 
                Location = new Point(0, 0), 
                AutoSize = true 
            };
            
            lblSubtitle = new Label 
            { 
                Text = "Quản lý danh sách, thông tin và tác phẩm của các tác giả.", 
                Font = new Font("Segoe UI", 13), 
                Location = new Point(0, 32), 
                AutoSize = true 
            };

            btnAdd = new Button 
            { 
                Text = "\ue145 Thêm mới", 
                Font = new Font("Material Symbols Outlined", 11, FontStyle.Regular), 
                Size = new Size(130, 40), 
                FlatStyle = FlatStyle.Flat 
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            btnExport = new Button 
            { 
                Text = "\ue2c4 Xuất Excel", 
                Font = new Font("Material Symbols Outlined", 11, FontStyle.Regular), 
                Size = new Size(130, 40), 
                FlatStyle = FlatStyle.Flat 
            };
            btnExport.Click += (s, e) => MessageBox.Show("Tính năng đang phát triển!");

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, btnAdd, btnExport });
            pnlHeader.Resize += (s, e) => 
            {
                btnAdd.Location = new Point(pnlHeader.Width - btnAdd.Width, 10);
                btnExport.Location = new Point(btnAdd.Left - btnExport.Width - 10, 10);
            };

            // DataGridView Section
            dgvData = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 48 },
                ScrollBars = ScrollBars.Vertical
            };
            dgvData.CellPainting += DgvData_CellPainting;
            dgvData.CellMouseMove += DgvData_CellMouseMove;
            dgvData.CellMouseLeave += DgvData_CellMouseLeave;
            dgvData.CellClick += DgvData_CellClick;

            // Pagination Section
            pnlPagination = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(0, 10, 0, 0) };
            
            lblPaginationInfo = new Label 
            { 
                Font = new Font("Segoe UI", 11), 
                AutoSize = true, 
                Location = new Point(0, 20) 
            };
            
            flpPagination = new FlowLayoutPanel 
            { 
                FlowDirection = FlowDirection.LeftToRight, 
                WrapContents = false, 
                AutoSize = true, 
                Anchor = AnchorStyles.Right | AnchorStyles.Top 
            };

            pnlPagination.Controls.Add(lblPaginationInfo);
            pnlPagination.Controls.Add(flpPagination);
            pnlPagination.Resize += (s, e) => 
            {
                flpPagination.Location = new Point(pnlPagination.Width - flpPagination.Width, 15);
            };

            // Add to Control
            this.Controls.Add(dgvData);
            this.Controls.Add(pnlPagination);
            this.Controls.Add(pnlHeader);
        }

        private void SetupDataGridViewColumns()
        {
            dgvData.Columns.Clear();
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIndex", HeaderText = "#", Width = 50, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCode", HeaderText = "MÃ TÁC GIẢ", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "TÊN TÁC GIẢ", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNationality", HeaderText = "QUỐC TỊCH", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBookCount", HeaderText = "SỐ LƯỢNG TP", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "TRẠNG THÁI", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAction", HeaderText = "ACTION", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });

            foreach (DataGridViewColumn col in dgvData.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.HeaderCell.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }
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
            
            dgvData.BackgroundColor = ThemeManager.CardBackground;
            dgvData.GridColor = ThemeManager.TextBoxBorder;
            dgvData.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvData.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvData.DefaultCellStyle.SelectionBackColor = ThemeManager.ButtonFill;
            dgvData.DefaultCellStyle.SelectionForeColor = ThemeManager.ButtonText;
            dgvData.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvData.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
            dgvData.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Background;
            
            lblPaginationInfo.ForeColor = ThemeManager.TextSecondary;
        }

        public async void PerformSearch(string query)
        {
            currentSearch = query;
            currentPage = 1;
            await LoadDataAsync();
        }

        public void LoadData()
        {
            _ = LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var result = await _authorRepository.GetPagedAuthorsWithStatsAsync(currentSearch, currentPage, pageSize);
                currentAuthors = (List<AuthorItem>)result.Items;
                totalRecords = result.TotalCount;

                if (this.IsDisposed) return;
                dgvData.Rows.Clear();
                int index = (currentPage - 1) * pageSize + 1;
                foreach (var author in currentAuthors)
                {
                    string code = $"TG{author.Id:D3}";
                    dgvData.Rows.Add(index++, code, author.AuthorName, author.Nationality ?? "", author.BookCount, "", "");
                }

                UpdatePaginationUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePaginationUI()
        {
            int startRecord = (currentPage - 1) * pageSize + 1;
            int endRecord = Math.Min(currentPage * pageSize, totalRecords);
            
            if (totalRecords == 0)
            {
                lblPaginationInfo.Text = "Không có dữ liệu";
            }
            else
            {
                lblPaginationInfo.Text = $"Hiển thị {startRecord} - {endRecord} của {totalRecords} tác giả";
            }

            flpPagination.Controls.Clear();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (totalPages <= 1) return;

            // Prev Button
            Button btnPrev = CreatePaginationButton("\ue5cb");
            btnPrev.Font = new Font("Material Symbols Outlined", 11);
            btnPrev.Enabled = currentPage > 1;
            btnPrev.Click += (s, e) => { currentPage--; LoadData(); };
            flpPagination.Controls.Add(btnPrev);

            // Page Buttons
            int startPage = Math.Max(1, currentPage - 1);
            int endPage = Math.Min(totalPages, currentPage + 1);

            for (int i = startPage; i <= endPage; i++)
            {
                int pageNum = i;
                Button btnPage = CreatePaginationButton(i.ToString());
                if (i == currentPage)
                {
                    btnPage.BackColor = ThemeManager.ButtonFill;
                    btnPage.ForeColor = ThemeManager.ButtonText;
                    btnPage.FlatAppearance.BorderColor = ThemeManager.ButtonFill;
                }
                else
                {
                    btnPage.Click += (s, e) => { currentPage = pageNum; LoadData(); };
                }
                flpPagination.Controls.Add(btnPage);
            }

            // Next Button
            Button btnNext = CreatePaginationButton("\ue5cc");
            btnNext.Font = new Font("Material Symbols Outlined", 11);
            btnNext.Enabled = currentPage < totalPages;
            btnNext.Click += (s, e) => { currentPage++; LoadData(); };
            flpPagination.Controls.Add(btnNext);
            
            // Re-center horizontally on resize
            flpPagination.Location = new Point(pnlPagination.Width - flpPagination.Width, 15);
        }

        private Button CreatePaginationButton(string text)
        {
            return new Button
            {
                Text = text,
                Size = new Size(32, 32),
                Margin = new Padding(2),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.CardBackground,
                ForeColor = ThemeManager.TextPrimary,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new AuthorForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void DgvData_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (dgvData.Columns[e.ColumnIndex].Name == "colStatus")
                {
                    e.PaintBackground(e.CellBounds, true);
                    bool isActive = currentAuthors[e.RowIndex].IsActive;
                    
                    string text = isActive ? "Đang hoạt động" : "Ngừng HĐ";
                    Color bgColor = isActive ? Color.FromArgb(220, 252, 231) : Color.FromArgb(254, 226, 226);
                    Color fgColor = isActive ? Color.FromArgb(22, 163, 74) : Color.FromArgb(220, 38, 38);

                    Rectangle rect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width - 100) / 2, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, 100, 24);
                    
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

                    TextRenderer.DrawText(e.Graphics, text.ToUpper(), new Font("Segoe UI", 8, FontStyle.Bold), e.CellBounds, fgColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                }
                else if (dgvData.Columns[e.ColumnIndex].Name == "colAction")
                {
                    e.PaintBackground(e.CellBounds, true);
                    
                    int iconSize = 20;
                    int spacing = 10;
                    int totalWidth = (iconSize * 2) + spacing;
                    int startX = e.CellBounds.X + (e.CellBounds.Width - totalWidth) / 2;
                    int startY = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;
                    
                    Rectangle editRect = new Rectangle(startX, startY, iconSize, iconSize);
                    Rectangle deleteRect = new Rectangle(startX + iconSize + spacing, startY, iconSize, iconSize);

                    bool isHoveredRow = hoveredRow == e.RowIndex;
                    Color editColor = (isHoveredRow && hoveredAction == 1) ? ThemeManager.ButtonFill : ThemeManager.TextSecondary;
                    Color deleteColor = (isHoveredRow && hoveredAction == 2) ? Color.Red : ThemeManager.TextSecondary;

                    if (isHoveredRow)
                    {
                        DrawIcon(e.Graphics, editRect, "\ue3c9", editColor); // Edit icon
                        DrawIcon(e.Graphics, deleteRect, "\ue872", deleteColor); // Delete icon
                    }
                    e.Handled = true;
                }
            }
        }
        
        private void DrawIcon(Graphics g, Rectangle rect, string iconCode, Color color)
        {
            using (Font iconFont = new Font("Material Symbols Outlined", 14, FontStyle.Regular))
            {
                TextRenderer.DrawText(g, iconCode, iconFont, rect, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DgvData_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                int oldHoveredRow = hoveredRow;
                int oldHoveredAction = hoveredAction;
                
                hoveredRow = e.RowIndex;
                hoveredCol = e.ColumnIndex;
                
                if (dgvData.Columns[e.ColumnIndex].Name == "colAction")
                {
                    var cellBounds = dgvData.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    int iconSize = 20;
                    int spacing = 10;
                    int totalWidth = (iconSize * 2) + spacing;
                    int startX = (cellBounds.Width - totalWidth) / 2;
                    
                    int mouseX = e.X;
                    if (mouseX >= startX && mouseX <= startX + iconSize)
                    {
                        hoveredAction = 1; // Edit
                    }
                    else if (mouseX >= startX + iconSize + spacing && mouseX <= startX + totalWidth)
                    {
                        hoveredAction = 2; // Delete
                    }
                    else
                    {
                        hoveredAction = 0;
                    }
                }
                else
                {
                    hoveredAction = 0;
                }

                if (oldHoveredRow != hoveredRow || oldHoveredAction != hoveredAction)
                {
                    dgvData.Invalidate();
                }
            }
        }

        private void DgvData_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            hoveredRow = -1;
            hoveredCol = -1;
            hoveredAction = 0;
            dgvData.Invalidate();
        }

        private async void DgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvData.Columns[e.ColumnIndex].Name == "colAction")
            {
                var author = currentAuthors[e.RowIndex];
                
                if (hoveredAction == 1) // Edit
                {
                    using (var form = new AuthorForm(author.Id))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            LoadData();
                        }
                    }
                }
                else if (hoveredAction == 2) // Delete
                {
                    var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa tác giả '{author.AuthorName}'?", 
                        "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            if (await _authorRepository.DeleteAsync(author.Id))
                            {
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Không thể xóa tác giả này. Có thể dữ liệu đang được sử dụng ở nơi khác.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}

