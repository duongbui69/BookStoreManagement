using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.Interfaces;
using Guna.UI2.WinForms;
using BookStoreManagement.Repositories;
using BookStoreManagement.Helpers; // For InventoryTransactionDto

namespace BookStoreManagement.UserControls
{
    public partial class VoucherTypeControl : UserControl, ISearchableControl
    {
        private readonly InventoryTransactionService _service;

        private Label lblTitle;
        private Label lblSubTitle;
        private Guna2Panel pnlToolbar;
        private Guna2TextBox txtSearch;
        private Guna2Button btnExport;
        private Guna2Panel pnlGridContainer;
        private DataGridView dgvData;
        private PaginationControl pagination;
        private Guna2DateTimePicker dtpFrom;
        private Guna2DateTimePicker dtpTo;

        private int _currentPage = 1;
        private int PageSize = 8;
        private int _hoveredRowIndex = -1;

        public VoucherTypeControl()
        {
            _service = new InventoryTransactionService();
            InitializeComponent();
            ApplyTheme();
            LoadData();
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24);
            this.AutoScroll = false;

            // Header Region
            Guna2Panel pnlHeader = new Guna2Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.Transparent };
            
            lblTitle = new Label
            {
                Text = "Loại phiếu",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            lblSubTitle = new Label
            {
                Text = "Theo dõi nhật ký xuất/nhập và các thay đổi kho.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(0, 50)
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubTitle });

            // Main Card Container
            Guna2Panel pnlMainCard = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 8,
                BorderThickness = 1,
                Padding = new Padding(0)
            };

            // Toolbar
            pnlToolbar = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                CustomizableEdges = new Guna.UI2.WinForms.Suite.CustomizableEdges(true, true, false, false),
                Padding = new Padding(24, 12, 24, 12)
            };
            
            dtpFrom = new Guna2DateTimePicker
            {
                Size = new Size(150, 36),
                Location = new Point(24, 12),
                BorderRadius = 8,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(-30)
            };
            dtpFrom.ValueChanged += (s, e) => { _currentPage = 1; LoadData(); };

            dtpTo = new Guna2DateTimePicker
            {
                Size = new Size(150, 36),
                Location = new Point(190, 12),
                BorderRadius = 8,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            dtpTo.ValueChanged += (s, e) => { _currentPage = 1; LoadData(); };

            btnExport = new Guna2Button
            {
                Text = "Xuất Excel",
                Size = new Size(120, 36),
                BorderRadius = 8,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            
            // Add dynamic resize logic for toolbar buttons
            pnlToolbar.Resize += (s, e) => 
            {
                btnExport.Location = new Point(pnlToolbar.Width - btnExport.Width - 24, 12);
            };

            btnExport.Click += BtnExport_Click;

            pnlToolbar.Controls.AddRange(new Control[] { dtpFrom, dtpTo, btnExport });

            // Grid Container
            pnlGridContainer = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(1)
            };

            dgvData = new DataGridView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 45 },
                EnableHeadersVisualStyles = false,
                Cursor = Cursors.Hand,
                ScrollBars = ScrollBars.None
            };
            dgvData.SetDoubleBuffered(true);

            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Ngày", Width = 150 });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "VoucherNo", HeaderText = "Số phiếu", Width = 150 });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Sản phẩm", Width = 300 });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Diễn giải", Width = 250 });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "ImportQty", HeaderText = "Nhập", Width = 100 });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "ExportQty", HeaderText = "Xuất", Width = 100 });
            dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "Balance", HeaderText = "Tồn", Width = 100 });

            foreach (DataGridViewColumn col in dgvData.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            dgvData.Columns["ProductName"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvData.Columns["Description"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgvData.CellMouseEnter += (s, e) => { if (e.RowIndex >= 0) { _hoveredRowIndex = e.RowIndex; dgvData.InvalidateRow(e.RowIndex); } };
            dgvData.CellMouseLeave += (s, e) => { _hoveredRowIndex = -1; if (e.RowIndex >= 0) dgvData.InvalidateRow(e.RowIndex); };
            
            pnlGridContainer.Controls.Add(dgvData);

            pagination = new PaginationControl
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };
            pagination.PageChanged += (s, e) => { _currentPage = e.NewPage; LoadData(); };

            pnlMainCard.Controls.Add(pnlGridContainer);
            pnlMainCard.Controls.Add(pnlToolbar);
            pnlMainCard.Controls.Add(pagination);

            this.Controls.Add(pnlMainCard);
            this.Controls.Add(pnlHeader);
        }

        private async void LoadData()
        {
            if (this.IsDisposed) return;
            var result = await _service.GetPagedTransactionsAsync(_currentPage, PageSize, dtpFrom.Value, dtpTo.Value);
            
            dgvData.Rows.Clear();
            foreach (var item in result.Items)
            {
                int r = dgvData.Rows.Add(
                    item.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    item.VoucherNo,
                    item.ProductName,
                    item.Description,
                    item.ImportQty > 0 ? item.ImportQty.ToString() : "-",
                    item.ExportQty > 0 ? item.ExportQty.ToString() : "-",
                    item.Balance
                );
                dgvData.Rows[r].Tag = item;
            }
            pagination.UpdatePagination(result.TotalCount, _currentPage, PageSize);
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx", FileName = "LichSuGiaoDich.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var excelService = new BookStoreManagement.Services.ExcelExportService();
                        excelService.ExportDataGridView(dgvData, sfd.FileName, "Giao Dịch");
                        MessageBox.Show("Xuất file Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PerformSearch(string keyword)
        {
            // Do nothing, we removed search in favor of date filter
        }

        public void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblSubTitle.ForeColor = ThemeManager.TextSecondary;
            
            pnlToolbar.BackColor = ThemeManager.CardBackground;
            pnlToolbar.CustomBorderColor = ThemeManager.TextBoxBorder;
            pnlToolbar.Parent.BackColor = ThemeManager.CardBackground;
            if (pnlToolbar.Parent is Guna2Panel p) p.CustomBorderColor = ThemeManager.TextBoxBorder;

            btnExport.FillColor = ThemeManager.CardBackground;
            btnExport.ForeColor = ThemeManager.TextPrimary;
            btnExport.BorderColor = ThemeManager.TextBoxBorder;
            btnExport.BorderThickness = 1;
            
            dtpFrom.FillColor = ThemeManager.TextBoxBackground;
            dtpFrom.ForeColor = ThemeManager.TextPrimary;
            dtpTo.FillColor = ThemeManager.TextBoxBackground;
            dtpTo.ForeColor = ThemeManager.TextPrimary;

            ThemeManager.ApplyDataGridViewStyle(dgvData);
        }
    }
}
