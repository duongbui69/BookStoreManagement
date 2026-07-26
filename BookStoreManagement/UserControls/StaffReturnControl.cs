using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookStoreManagement.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.ViewModels;
using Guna.UI2.WinForms;

namespace BookStoreManagement.UserControls
{
    public class StaffReturnControl : UserControl
    {
        private SalesOrderService _orderService;
        private ReturnReceiptService _returnService;
        private SalesOrderListViewModel? _currentOrder;
        private List<SalesOrderDetailFullViewModel> _currentOrderDetails;

        // UI Components
        private Guna2Panel pnlContent;
        
        // Search Panel
        private Guna2Panel pnlSearch;
        private Guna2HtmlLabel lblSearchTitle;
        private Guna2HtmlLabel lblSearchLabel;
        private Guna2TextBox txtSearch;
        private Guna2Button btnSearch;
        private Guna2Button btnScan;

        // Info Panel
        private Guna2Panel pnlInfo;
        private Guna2HtmlLabel lblInfoTitle;
        private Guna2HtmlLabel lblInvoiceCode;
        private Guna2HtmlLabel lblStatus;
        private Guna2Panel pnlDate;
        private Guna2HtmlLabel lblDateTitle;
        private Guna2HtmlLabel lblDateValue;
        private Guna2Panel pnlCustomer;
        private Guna2HtmlLabel lblCustomerTitle;
        private Guna2HtmlLabel lblCustomerValue;
        private Guna2Panel pnlGridHeader;
        private Guna2Panel pnlCashier;
        private Guna2HtmlLabel lblCashierTitle;
        private Guna2HtmlLabel lblCashierValue;

        // Grid Panel
        private Guna2Panel pnlGrid;
        private Guna2HtmlLabel lblGridTitle;
        private Guna2HtmlLabel lblGridSummary;
        private Guna2DataGridView dgvItems;

        // Footer Panel
        private Guna2Panel pnlFooter;
        private Guna2HtmlLabel lblTotalReturnQty;
        private Guna2HtmlLabel lblTotalRefundAmountTitle;
        private Guna2HtmlLabel lblTotalRefundAmount;
        private Guna2Button btnCancel;
        private Guna2Button btnConfirm;

        public StaffReturnControl()
        {
            _orderService = new SalesOrderService();
            _returnService = new ReturnReceiptService();
            _currentOrderDetails = new List<SalesOrderDetailFullViewModel>();

            InitializeUI();
            ApplyTheme();

            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Disposed += (s, e) => { ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged; };
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24); // container-padding

            pnlContent = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            this.Controls.Add(pnlContent);

            // 1. Footer (Dock Bottom)
            InitializeFooter();
            
            // 2. Bento Box (Search & Info) (Dock Top)
            InitializeBentoBox();

            // 3. Grid Panel (Dock Fill)
            InitializeGridPanel();
        }

        private void InitializeBentoBox()
        {
            Guna2Panel pnlBentoContainer = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 180,
                Padding = new Padding(0, 0, 0, 16) // stack-gap
            };
            pnlContent.Controls.Add(pnlBentoContainer);

            TableLayoutPanel tlpBento = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            tlpBento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tlpBento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            pnlBentoContainer.Controls.Add(tlpBento);

            // --- Search Panel ---
            pnlSearch = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 12,
                BorderThickness = 1,
                Padding = new Padding(20),
                Margin = new Padding(0, 0, 8, 0)
            };
            tlpBento.Controls.Add(pnlSearch, 0, 0);

            lblSearchTitle = new Guna2HtmlLabel
            {
                Text = "Tìm Hóa đơn",
                Font = new Font("Inter", 14F, FontStyle.Bold),
                Location = new Point(20, 20)
            };
            pnlSearch.Controls.Add(lblSearchTitle);

            lblSearchLabel = new Guna2HtmlLabel
            {
                Text = "Mã Hóa đơn / SĐT",
                Font = new Font("Inter", 9F, FontStyle.Bold),
                Location = new Point(20, 55)
            };
            pnlSearch.Controls.Add(lblSearchLabel);

            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Vd: INV-2023...",
                BorderRadius = 4,
                Location = new Point(20, 75),
                Size = new Size(pnlSearch.Width - 40, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            txtSearch.KeyDown += TxtSearch_KeyDown;
            pnlSearch.Controls.Add(txtSearch);

            btnSearch = new Guna2Button
            {
                Text = "Tìm kiếm",
                BorderRadius = 4,
                Font = new Font("Inter", 9F, FontStyle.Bold),
                Location = new Point(20, 118),
                Size = new Size(pnlSearch.Width - 76, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnSearch.Click += BtnSearch_Click;
            pnlSearch.Controls.Add(btnSearch);

            btnScan = new Guna2Button
            {
                Text = "Q",
                BorderRadius = 4,
                Location = new Point(pnlSearch.Width - 48, 118),
                Size = new Size(36, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnScan.Click += BtnScan_Click;
            pnlSearch.Controls.Add(btnScan);

            // --- Info Panel ---
            pnlInfo = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 12,
                BorderThickness = 1,
                Padding = new Padding(20),
                Margin = new Padding(8, 0, 0, 0)
            };
            tlpBento.Controls.Add(pnlInfo, 1, 0);

            lblInfoTitle = new Guna2HtmlLabel
            {
                Text = "Thông tin hóa đơn",
                Font = new Font("Inter", 14F, FontStyle.Bold),
                Location = new Point(20, 20)
            };
            pnlInfo.Controls.Add(lblInfoTitle);

            lblInvoiceCode = new Guna2HtmlLabel
            {
                Text = "---",
                Font = new Font("Inter", 11F),
                Location = new Point(20, 48)
            };
            pnlInfo.Controls.Add(lblInvoiceCode);

            lblStatus = new Guna2HtmlLabel
            {
                Text = "Trạng thái",
                Font = new Font("Inter", 9F, FontStyle.Bold),
                Location = new Point(pnlInfo.Width - 100, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = false,
                Size = new Size(80, 24),
                TextAlignment = ContentAlignment.MiddleCenter
            };
            pnlInfo.Controls.Add(lblStatus);

            TableLayoutPanel tlpInfoDetails = new TableLayoutPanel
            {
                Location = new Point(20, 85),
                Size = new Size(pnlInfo.Width - 40, 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 3,
                RowCount = 1
            };
            tlpInfoDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpInfoDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpInfoDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnlInfo.Controls.Add(tlpInfoDetails);

            pnlDate = CreateInfoBox("Ngày mua", out lblDateTitle, out lblDateValue);
            pnlCustomer = CreateInfoBox("Khách hàng", out lblCustomerTitle, out lblCustomerValue);
            pnlCashier = CreateInfoBox("Thu ngân", out lblCashierTitle, out lblCashierValue);

            pnlDate.Margin = new Padding(0, 0, 8, 0);
            pnlCustomer.Margin = new Padding(4, 0, 4, 0);
            pnlCashier.Margin = new Padding(8, 0, 0, 0);

            tlpInfoDetails.Controls.Add(pnlDate, 0, 0);
            tlpInfoDetails.Controls.Add(pnlCustomer, 1, 0);
            tlpInfoDetails.Controls.Add(pnlCashier, 2, 0);
            
            pnlSearch.Resize += (s, e) => {
                txtSearch.Width = pnlSearch.Width - 40;
                btnSearch.Width = pnlSearch.Width - 68;
                btnScan.Left = pnlSearch.Width - 56;
            };
            pnlInfo.Resize += (s, e) => {
                lblStatus.Left = pnlInfo.Width - 120;
                tlpInfoDetails.Width = pnlInfo.Width - 40;
            };
        }

        private Guna2Panel CreateInfoBox(string title, out Guna2HtmlLabel lblTitle, out Guna2HtmlLabel lblValue)
        {
            Guna2Panel pnl = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 4,
                Padding = new Padding(12)
            };
            
            lblTitle = new Guna2HtmlLabel
            {
                Text = title,
                Font = new Font("Inter", 9F, FontStyle.Bold),
                Location = new Point(12, 8)
            };
            pnl.Controls.Add(lblTitle);

            lblValue = new Guna2HtmlLabel
            {
                Text = "---",
                Font = new Font("Inter", 10F),
                Location = new Point(12, 30),
                AutoSize = false,
                Size = new Size(150, 20)
            };
            pnl.Controls.Add(lblValue);
            
            var localLblValue = lblValue;
            pnl.Resize += (s, e) => {
                localLblValue.Width = pnl.Width - 24;
            };

            return pnl;
        }

        private void InitializeGridPanel()
        {
            pnlGrid = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 12,
                BorderThickness = 1,
                Padding = new Padding(1),
                Margin = new Padding(0, 0, 0, 16) // gap above footer
            };
            pnlContent.Controls.Add(pnlGrid);
            pnlGrid.BringToFront(); // To be above bento box in Z-order for fill

            pnlGridHeader = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                CustomBorderThickness = new Padding(0, 0, 0, 1)
            };
            pnlGrid.Controls.Add(pnlGridHeader);

            lblGridTitle = new Guna2HtmlLabel
            {
                Text = "Danh sách Sản phẩm",
                Font = new Font("Inter", 12F, FontStyle.Bold),
                Location = new Point(16, 12)
            };
            pnlGridHeader.Controls.Add(lblGridTitle);

            lblGridSummary = new Guna2HtmlLabel
            {
                Text = "0 mặt hàng (Tổng SL: 0)",
                Font = new Font("Inter", 9F, FontStyle.Bold),
                Location = new Point(pnlGridHeader.Width - 200, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = false,
                Size = new Size(180, 20),
                TextAlignment = ContentAlignment.MiddleRight
            };
            pnlGridHeader.Controls.Add(lblGridSummary);

            pnlGridHeader.Resize += (s, e) => {
                lblGridSummary.Left = pnlGridHeader.Width - 200;
            };

            dgvItems = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                RowTemplate = { Height = 48 },
                BorderStyle = BorderStyle.None,
                ThemeStyle = {
                    HeaderStyle = { Font = new Font("Inter", 9F, FontStyle.Bold), Height = 40 },
                    RowsStyle = { Font = new Font("Inter", 10F) },
                    AlternatingRowsStyle = { Font = new Font("Inter", 10F) }
                }
            };
            
            // Define Columns
            dgvItems.Columns.Add("STT", "STT");
            dgvItems.Columns["STT"].Width = 50;
            dgvItems.Columns["STT"].ReadOnly = true;

            dgvItems.Columns.Add("Sản phẩm", "Sản phẩm");
            dgvItems.Columns["Sản phẩm"].ReadOnly = true;

            dgvItems.Columns.Add("UnitPrice", "Đơn giá");
            dgvItems.Columns["UnitPrice"].Width = 100;
            dgvItems.Columns["UnitPrice"].ReadOnly = true;
            dgvItems.Columns["UnitPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvItems.Columns.Add("PurchasedQty", "SL Mua");
            dgvItems.Columns["PurchasedQty"].Width = 80;
            dgvItems.Columns["PurchasedQty"].ReadOnly = true;
            dgvItems.Columns["PurchasedQty"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvItems.Columns.Add("ReturnQty", "Số lượng trả");
            dgvItems.Columns["ReturnQty"].Width = 80;
            dgvItems.Columns["ReturnQty"].ReadOnly = false;
            dgvItems.Columns["ReturnQty"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewComboBoxColumn colReason = new DataGridViewComboBoxColumn
            {
                Name = "ReturnReason",
                HeaderText = "Lý do trả",
                Width = 150,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            colReason.Items.AddRange("Sách lỗi/Rách trang", "Sai mặt hàng", "Khách đổi ý", "Khác...");
            dgvItems.Columns.Add(colReason);

            dgvItems.Columns.Add("RefundAmount", "Thành tiền hoàn");
            dgvItems.Columns["RefundAmount"].Width = 120;
            dgvItems.Columns["RefundAmount"].ReadOnly = true;
            dgvItems.Columns["RefundAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["RefundAmount"].DefaultCellStyle.Font = new Font("Inter", 10F, FontStyle.Bold);

            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            dgvItems.CurrentCellDirtyStateChanged += DgvItems_CurrentCellDirtyStateChanged;
            dgvItems.CellValidating += DgvItems_CellValidating;

            pnlGrid.Controls.Add(dgvItems);
            dgvItems.BringToFront();
        }

        private void InitializeFooter()
        {
            pnlFooter = new Guna2Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BorderRadius = 12,
                BorderThickness = 1,
                Padding = new Padding(20)
            };
            pnlContent.Controls.Add(pnlFooter);

            lblTotalReturnQty = new Guna2HtmlLabel
            {
                Text = "Tổng SL Trả: 0",
                Font = new Font("Inter", 10F, FontStyle.Bold),
                Location = new Point(20, 16)
            };
            pnlFooter.Controls.Add(lblTotalReturnQty);

            lblTotalRefundAmountTitle = new Guna2HtmlLabel
            {
                Text = "Tổng tiền hoàn: ",
                Font = new Font("Inter", 12F),
                Location = new Point(20, 40)
            };
            pnlFooter.Controls.Add(lblTotalRefundAmountTitle);

            lblTotalRefundAmount = new Guna2HtmlLabel
            {
                Text = "0 đ",
                Font = new Font("Inter", 18F, FontStyle.Bold),
                Location = new Point(lblTotalRefundAmountTitle.Right + 5, 34)
            };
            pnlFooter.Controls.Add(lblTotalRefundAmount);

            btnConfirm = new Guna2Button
            {
                Text = "Xác nhận Trả hàng & Hoàn tiền",
                BorderRadius = 4,
                Font = new Font("Inter", 10F, FontStyle.Bold),
                Size = new Size(220, 44),
                Location = new Point(pnlFooter.Width - 240, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnConfirm.Click += BtnConfirm_Click;
            pnlFooter.Controls.Add(btnConfirm);

            btnCancel = new Guna2Button
            {
                Text = "Hủy bỏ",
                BorderRadius = 4,
                BorderThickness = 1,
                Font = new Font("Inter", 10F, FontStyle.Bold),
                Size = new Size(120, 44),
                Location = new Point(pnlFooter.Width - 370, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCancel.Click += BtnCancel_Click;
            pnlFooter.Controls.Add(btnCancel);

            pnlFooter.Resize += (s, e) => {
                btnConfirm.Left = pnlFooter.Width - 240;
                btnCancel.Left = pnlFooter.Width - 370;
            };
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            
            pnlSearch.FillColor = ThemeManager.CardBackground;
            pnlSearch.BorderColor = ThemeManager.TextBoxBorder;
            lblSearchTitle.ForeColor = ThemeManager.TextPrimary;
            lblSearchLabel.ForeColor = ThemeManager.TextSecondary;
            
            txtSearch.FillColor = ThemeManager.TextBoxBackground;
            txtSearch.ForeColor = ThemeManager.TextPrimary;
            txtSearch.BorderColor = ThemeManager.TextBoxBorder;
            txtSearch.FocusedState.BorderColor = ThemeManager.ButtonFill;
            
            btnSearch.FillColor = ThemeManager.ButtonFill;
            btnSearch.ForeColor = ThemeManager.ButtonText;
            
            btnScan.FillColor = ThemeManager.CardBackground;
            btnScan.BorderColor = ThemeManager.TextBoxBorder;
            btnScan.BorderThickness = 1;
            btnScan.ForeColor = ThemeManager.TextPrimary;

            pnlInfo.FillColor = ThemeManager.CardBackground;
            pnlInfo.BorderColor = ThemeManager.TextBoxBorder;
            lblInfoTitle.ForeColor = ThemeManager.TextPrimary;
            lblInvoiceCode.ForeColor = ThemeManager.TextSecondary;
            lblStatus.ForeColor = Color.White;
            lblStatus.BackColor = ThemeManager.HoverColor;
            
            Color boxBg = ThemeManager.Background;
            pnlDate.FillColor = boxBg;
            lblDateTitle.ForeColor = ThemeManager.TextSecondary;
            lblDateValue.ForeColor = ThemeManager.TextPrimary;
            pnlCustomer.FillColor = boxBg;
            lblCustomerTitle.ForeColor = ThemeManager.TextSecondary;
            lblCustomerValue.ForeColor = ThemeManager.TextPrimary;
            pnlCashier.FillColor = boxBg;
            lblCashierTitle.ForeColor = ThemeManager.TextSecondary;
            lblCashierValue.ForeColor = ThemeManager.TextPrimary;

            pnlGrid.FillColor = ThemeManager.CardBackground;
            pnlGrid.BorderColor = ThemeManager.TextBoxBorder;
            pnlGridHeader.BackColor = ThemeManager.CardBackground;
            pnlGridHeader.CustomBorderColor = ThemeManager.TextBoxBorder;
            lblGridTitle.ForeColor = ThemeManager.TextPrimary;
            lblGridSummary.ForeColor = ThemeManager.TextSecondary;

            ThemeManager.ApplyDataGridViewStyle(dgvItems);

            pnlFooter.FillColor = ThemeManager.CardBackground;
            pnlFooter.BorderColor = ThemeManager.TextBoxBorder;
            lblTotalReturnQty.ForeColor = ThemeManager.TextSecondary;
            lblTotalRefundAmountTitle.ForeColor = ThemeManager.TextPrimary;
            lblTotalRefundAmount.ForeColor = Color.Firebrick;
            
            btnConfirm.FillColor = Color.Firebrick;
            btnConfirm.ForeColor = Color.White;
            
            btnCancel.FillColor = ThemeManager.CardBackground;
            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.BorderColor = ThemeManager.TextBoxBorder;
        }

        private async void BtnSearch_Click(object? sender, EventArgs e)
        {
            await SearchInvoiceAsync();
        }

        private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SearchInvoiceAsync();
            }
        }

        private void BtnScan_Click(object? sender, EventArgs e)
        {
            using (var dlg = new BarcodeScanDialog())
            {
                if (dlg.ShowDialog(this.ParentForm) == DialogResult.OK)
                {
                    txtSearch.Text = dlg.ScannedBarcode;
                    SearchInvoiceAsync();
                }
            }
        }

        private async Task SearchInvoiceAsync()
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword)) return;

            try
            {
                var orders = await _orderService.SearchAsync(keyword);
                if (orders == null || orders.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy hóa đơn.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearData();
                    return;
                }

                var orderMatch = orders.FirstOrDefault(o => o.OrderCode.Equals(keyword, StringComparison.OrdinalIgnoreCase)) ?? orders.First();

                await LoadOrderDetailsAsync(orderMatch);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadOrderDetailsAsync(SalesOrderListViewModel order)
        {
            try
            {
                _currentOrder = order;
                _currentOrderDetails = await _orderService.GetDetailsAsync(order.Id);

                if (_currentOrder == null || _currentOrderDetails == null)
                {
                    ClearData();
                    return;
                }

                // Update Info Panel
                lblInvoiceCode.Text = _currentOrder.OrderCode;
                lblDateValue.Text = _currentOrder.OrderDate.ToString("dd/MM/yyyy HH:mm");
                lblCustomerValue.Text = string.IsNullOrEmpty(_currentOrder.CustomerName) ? "Khách lẻ" : _currentOrder.CustomerName;
                lblCashierValue.Text = _currentOrder.StaffName;
                
                // Status mapping
                switch (_currentOrder.OrderStatus)
                {
                    case "Hoàn thành":
                        lblStatus.Text = "Hoàn thành";
                        lblStatus.ForeColor = Color.White;
                        lblStatus.BackColor = Color.ForestGreen;
                        break;
                    case "Đã hủy":
                        lblStatus.Text = "Đã hủy";
                        lblStatus.ForeColor = Color.White;
                        lblStatus.BackColor = Color.Firebrick;
                        break;
                    default:
                        lblStatus.Text = _currentOrder.OrderStatus;
                        break;
                }

                // Populate Grid
                dgvItems.Rows.Clear();
                int stt = 1;
                foreach (var detail in _currentOrderDetails)
                {
                    int rowIndex = dgvItems.Rows.Add(
                        stt++,
                        $"{detail.Title}\nCode: {detail.BookCode}",
                        detail.UnitPrice.ToString("N0") + " ₫",
                        detail.Quantity,
                        0, // Default return qty is 0
                        "Khách đổi ý", // Default reason
                        "0" // Refund amount
                    );
                    
                    dgvItems.Rows[rowIndex].Tag = detail; // Store detail obj in Tag
                    if (detail.Quantity == 0) 
                    {
                        dgvItems.Rows[rowIndex].ReadOnly = true;
                        dgvItems.Rows[rowIndex].DefaultCellStyle.BackColor = ThemeManager.HoverColor;
                    }
                }

                UpdateGridSummary();
                UpdateFooterTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearData()
        {
            _currentOrder = null;
            _currentOrderDetails.Clear();

            lblInvoiceCode.Text = "---";
            lblDateValue.Text = "---";
            lblCustomerValue.Text = "---";
            lblCashierValue.Text = "---";
            lblStatus.Text = "Trạng thái";
            lblStatus.BackColor = ThemeManager.HoverColor;
            lblStatus.ForeColor = ThemeManager.TextSecondary;

            dgvItems.Rows.Clear();
            UpdateGridSummary();
            UpdateFooterTotals();
        }

        private void UpdateGridSummary()
        {
            int totalItems = _currentOrderDetails.Count;
            int totalQty = _currentOrderDetails.Sum(d => d.Quantity);
            lblGridSummary.Text = $"{totalItems} mặt hàng (Tổng SL: {totalQty})";
        }

        // --- Grid Events ---
        private void DgvItems_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgvItems.IsCurrentCellDirty)
            {
                dgvItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvItems_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == dgvItems.Columns["ReturnQty"].Index)
            {
                if (!int.TryParse(e.FormattedValue?.ToString(), out int returnQty) || returnQty < 0)
                {
                    e.Cancel = true;
                    MessageBox.Show("Số lượng trả phải là số dương hợp lệ.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var detail = dgvItems.Rows[e.RowIndex].Tag as SalesOrderDetailFullViewModel;
                if (detail != null && returnQty > detail.Quantity)
                {
                    e.Cancel = true;
                    MessageBox.Show($"Số lượng trả không được vượt quá số lượng mua ({detail.Quantity}).", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void DgvItems_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvItems.Columns["ReturnQty"].Index)
            {
                var row = dgvItems.Rows[e.RowIndex];
                var detail = row.Tag as SalesOrderDetailFullViewModel;
                
                if (detail != null && int.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out int returnQty))
                {
                    decimal refundAmt = returnQty * detail.UnitPrice;
                    row.Cells["RefundAmount"].Value = refundAmt.ToString("N0") + " ₫";
                    
                    if (returnQty > 0)
                        row.Cells["RefundAmount"].Style.ForeColor = Color.Firebrick;
                    else
                        row.Cells["RefundAmount"].Style.ForeColor = ThemeManager.TextPrimary;
                        
                    UpdateFooterTotals();
                }
            }
        }

        private void UpdateFooterTotals()
        {
            int totalReturnQty = 0;
            decimal totalRefundAmt = 0;

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (int.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out int rQty))
                {
                    totalReturnQty += rQty;
                    var detail = row.Tag as SalesOrderDetailFullViewModel;
                    if (detail != null)
                    {
                        totalRefundAmt += rQty * detail.UnitPrice;
                    }
                }
            }

            lblTotalReturnQty.Text = $"Tổng SL Hoàn: {totalReturnQty}";
            lblTotalRefundAmount.Text = $"{totalRefundAmt:N0} VNĐ";
            lblTotalRefundAmount.Left = lblTotalRefundAmountTitle.Right + 5;
            
            btnConfirm.Enabled = totalReturnQty > 0;
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            ClearData();
            txtSearch.Clear();
            txtSearch.Focus();
        }

        private async void BtnConfirm_Click(object? sender, EventArgs e)
        {
            if (_currentOrder == null) return;

            var returnDetails = new List<ReturnReceiptDetail>();
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (int.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out int rQty) && rQty > 0)
                {
                    var orderDetail = row.Tag as SalesOrderDetailFullViewModel;
                    if (orderDetail != null)
                    {
                        returnDetails.Add(new ReturnReceiptDetail
                        {
                            BookId = orderDetail.BookId,
                            Quantity = rQty,
                            UnitPrice = orderDetail.UnitPrice,
                            ReturnReason = row.Cells["ReturnReason"].Value?.ToString() ?? "Khác..."
                        });
                    }
                }
            }

            if (returnDetails.Count == 0)
            {
                MessageBox.Show("Vui lòng nhập số lượng trả cho ít nhất một mặt hàng.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show($"Xác nhận trả hàng và hoàn số tiền {lblTotalRefundAmount.Text}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult != DialogResult.Yes) return;

            try
            {
                if (!CurrentSession.StoreId.HasValue)
                {
                    throw new Exception("Nhân viên chưa được gán cửa hàng.");
                }

                int returnId = await _returnService.CreateReturnAsync(
                    _currentOrder.Id, 
                    CurrentSession.StoreId.Value, 
                    _currentOrder.CustomerId, 
                    "Khách trả hàng trực tiếp", 
                    returnDetails
                );

                MessageBox.Show($"Trả hàng thành công! Mã phiếu trả: #{returnId}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearData();
                txtSearch.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi khi lưu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
