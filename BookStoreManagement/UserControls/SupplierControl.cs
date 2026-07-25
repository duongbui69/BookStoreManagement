using BookStoreManagement.Helpers;
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
    public partial class SupplierControl : UserControl, ISearchableControl
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
        
        private SupplierRepository _supplierRepository;
        private int currentPage = 1;
        private int pageSize = 5;
        private int totalRecords = 0;
        private string currentSearch = "";
        
        private List<Supplier> currentSuppliers = new List<Supplier>();

        private int hoveredRow = -1;
        private int hoveredCol = -1;
        private int hoveredAction = 0; // 0: none, 1: edit, 2: delete

        public SupplierControl()
        {
            InitializeComponents();
            _supplierRepository = new SupplierRepository();
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
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100 };
            
            lblTitle = new Label 
            { 
                Text = "Supplier management", 
                Font = new Font("Segoe UI", 24F, FontStyle.Bold), 
                Location = new Point(0, 0), 
                AutoSize = true 
            };
            
            lblSubtitle = new Label 
            { 
                Text = "Manage supplier information and contacts.", 
                Font = new Font("Segoe UI", 11F), 
                Location = new Point(0, 45), 
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
            btnExport.Click += (s, e) => MessageBox.Show("Feature under development!");

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, btnAdd, btnExport });
            pnlHeader.Resize += (s, e) => 
            {
                btnAdd.Location = new Point(pnlHeader.Width - btnAdd.Width, 22);
                btnExport.Location = new Point(btnAdd.Left - btnExport.Width - 10, 22);
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
            dgvData.SetDoubleBuffered(true);
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

            Panel spacer1 = new Panel { Dock = DockStyle.Top, Height = 20, BackColor = Color.Transparent };
            
            // Add to Control
            this.Controls.Add(dgvData);
            this.Controls.Add(pnlPagination);
            this.Controls.Add(spacer1);
            this.Controls.Add(pnlHeader);
        }

        private void SetupDataGridViewColumns()
        {
            dgvData.Columns.Clear();
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIndex", HeaderText = "#", Width = 50, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCode", HeaderText = "SUPPLIER ID", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "SUPPLIER NAME", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPhone", HeaderText = "PHONE NUMBER", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAddress", HeaderText = "ADDRESS", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "STATUS", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
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
            
            ThemeManager.ApplyDataGridViewStyle(dgvData);
            
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
                var result = await _supplierRepository.GetPagedSuppliersAsync(currentSearch, currentPage, pageSize);
                currentSuppliers = (List<Supplier>)result.Items;
                totalRecords = result.TotalCount;

                if (this.IsDisposed) return;
                dgvData.Rows.Clear();
                int index = (currentPage - 1) * pageSize + 1;
                foreach (var supplier in currentSuppliers)
                {
                    string code = $"NCC{supplier.Id:D3}";
                    dgvData.Rows.Add(index++, code, supplier.SupplierName, supplier.Phone ?? "", supplier.Address ?? "", "", "");
                }

                UpdatePaginationUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePaginationUI()
        {
            int startRecord = (currentPage - 1) * pageSize + 1;
            int endRecord = Math.Min(currentPage * pageSize, totalRecords);
            
            if (totalRecords == 0)
            {
                lblPaginationInfo.Text = "No data";
            }
            else
            {
                lblPaginationInfo.Text = $"Hiển thị {startRecord} - {endRecord} của {totalRecords} nhà cung cấp";
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
                Font = new Font("Segoe UI", 11F),
                Cursor = Cursors.Hand
            };
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new SupplierForm())
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
                    bool isActive = currentSuppliers[e.RowIndex].IsActive;
                    
                    string text = isActive ? "Trading" : "Stop trading";
                    Color bgColor = isActive ? Color.FromArgb(220, 252, 231) : Color.FromArgb(254, 226, 226);
                    Color fgColor = isActive ? Color.FromArgb(22, 163, 74) : Color.FromArgb(220, 38, 38);

                    Rectangle rect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width - 110) / 2, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, 110, 24);
                    
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

                    using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
                    {
                    TextRenderer.DrawText(e.Graphics, text.ToUpper(), font, e.CellBounds, fgColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
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
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvData.Columns[e.ColumnIndex].Name == "colAction")
            {
                var supplier = currentSuppliers[e.RowIndex];
                
                if (hoveredAction == 1) // Edit
                {
                    using (var form = new SupplierForm(supplier.Id))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            LoadData();
                        }
                    }
                }
                else if (hoveredAction == 2) // Delete
                {
                    var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhà cung cấp '{supplier.SupplierName}'?", 
                        "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            // In this simple layout we assume SupplierRepository has DeleteAsync (it usually uses ExecuteNonQuery from base)
                            string sql = "DELETE FROM Suppliers WHERE Id = @Id;";
                            using var connection = BookStoreManagement.Database.DbConnectionFactory.CreateConnection();
                            int rowsAffected = await Dapper.SqlMapper.ExecuteAsync(connection, sql, new { Id = supplier.Id });
                            
                            if (rowsAffected > 0)
                            {
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Cannot delete this supplier. Data might be in use elsewhere.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error deleting: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}
