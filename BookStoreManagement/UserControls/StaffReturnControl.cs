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
        private Guna.UI2.WinForms.Guna2Panel pnlContent;
        
        // Search Panel
        private Guna.UI2.WinForms.Guna2Panel pnlSearch;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblSearchTitle;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblSearchLabel;
        private Guna.UI2.WinForms.Guna2TextBox txtSearch;
        private Guna.UI2.WinForms.Guna2Button btnSearch;
        private Guna.UI2.WinForms.Guna2Button btnScan;

        // Info Panel
        private Guna.UI2.WinForms.Guna2Panel pnlInfo;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblInfoTitle;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblInvoiceCode;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblStatus;
        private Guna.UI2.WinForms.Guna2Panel pnlDate;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblDateTitle;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblDateValue;
        private Guna.UI2.WinForms.Guna2Panel pnlCustomer;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblCustomerTitle;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblCustomerValue;
        private Guna.UI2.WinForms.Guna2Panel pnlGridHeader;
        private Guna.UI2.WinForms.Guna2Panel pnlCashier;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblCashierTitle;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblCashierValue;

        // Grid Panel
        private Guna.UI2.WinForms.Guna2Panel pnlGrid;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblGridTitle;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblGridSummary;
        private Guna.UI2.WinForms.Guna2DataGridView dgvItems;

        // Footer Panel
        private Guna.UI2.WinForms.Guna2Panel pnlFooter;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblTotalReturnQty;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblTotalRefundAmountTitle;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblTotalRefundAmount;
        private Guna.UI2.WinForms.Guna2Button btnConfirm;
        private Guna.UI2.WinForms.Guna2Button btnCancel;


        // Missing fields added
        private Guna.UI2.WinForms.Guna2HtmlLabel lblSubtitle;
        private Guna.UI2.WinForms.Guna2Panel pnlOrderInfo;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblInvoiceCodeTitle;
        private Guna.UI2.WinForms.Guna2Panel pnlGridContainer;
        private Guna.UI2.WinForms.Guna2Panel pnlActionFooter;

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

        private Guna2Panel pnlHeader;
        private Guna2HtmlLabel lblTitle;

        
        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(24);

            var tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            // LEFT PANEL: Search and Order Info
            var pnlLeft = new Guna2Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
            
            pnlSearch = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BorderRadius = 8,
                CustomBorderThickness = new Padding(1),
                Margin = new Padding(0, 0, 0, 16),
                Padding = new Padding(20)
            };
            
            lblTitle = new Guna2HtmlLabel { Text = "Trả hàng / Hoàn tiền", Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(20, 20) };
            lblSubtitle = new Guna2HtmlLabel { Text = "Nhập mã hóa đơn để kiểm tra", Font = new Font("Segoe UI", 10F), Location = new Point(20, 50) };
            
            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Mã hóa đơn (VD: INV-12345)...",
                BorderRadius = 6,
                Size = new Size(250, 36),
                Location = new Point(20, 75)
            };
            txtSearch.TextChanged += async (s, e) => {
                await SearchInvoiceAsync();
            };
            pnlSearch.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, txtSearch });

            pnlOrderInfo = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 8,
                CustomBorderThickness = new Padding(1),
                Padding = new Padding(20)
            };

            var lblInfoTitle = new Guna2HtmlLabel { Text = "Thông tin hóa đơn", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Location = new Point(20, 20) };
            
            lblInvoiceCodeTitle = new Guna2HtmlLabel { Text = "Mã HĐ:", Font = new Font("Segoe UI", 10F), Location = new Point(20, 60) };
            lblInvoiceCode = new Guna2HtmlLabel { Text = "---", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(100, 60) };
            
            lblDateTitle = new Guna2HtmlLabel { Text = "Ngày mua:", Font = new Font("Segoe UI", 10F), Location = new Point(20, 90) };
            lblDateValue = new Guna2HtmlLabel { Text = "---", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(100, 90) };
            
            lblCustomerTitle = new Guna2HtmlLabel { Text = "Khách hàng:", Font = new Font("Segoe UI", 10F), Location = new Point(20, 120) };
            lblCustomerValue = new Guna2HtmlLabel { Text = "---", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(100, 120) };
            
            lblCashierTitle = new Guna2HtmlLabel { Text = "Thu ngân:", Font = new Font("Segoe UI", 10F), Location = new Point(20, 150) };
            lblCashierValue = new Guna2HtmlLabel { Text = "---", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(100, 150) };
            
            lblStatus = new Guna2HtmlLabel 
            { 
                Text = "Trạng thái", 
                Font = new Font("Segoe UI", 10F, FontStyle.Bold), 
                Location = new Point(20, 190),
                Padding = new Padding(10, 5, 10, 5)
            };

            pnlOrderInfo.Controls.AddRange(new Control[] { 
                lblInfoTitle, lblInvoiceCodeTitle, lblInvoiceCode, lblDateTitle, lblDateValue, 
                lblCustomerTitle, lblCustomerValue, lblCashierTitle, lblCashierValue, lblStatus 
            });

            pnlLeft.Controls.Add(pnlOrderInfo);
            pnlLeft.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Color.Transparent });
            pnlLeft.Controls.Add(pnlSearch);

            // RIGHT PANEL: Grid and Footer
            var pnlRight = new Guna2Panel { Dock = DockStyle.Fill, Margin = new Padding(12, 0, 0, 0) };
            
            pnlGridContainer = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 8,
                CustomBorderThickness = new Padding(1),
                Padding = new Padding(1)
            };

            var pnlGridHeader = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };
            var lblGridTitle = new Guna2HtmlLabel { Text = "Chi tiết mặt hàng", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 15) };
            lblGridSummary = new Guna2HtmlLabel { Text = "0 mặt hàng (Tổng SL: 0)", Font = new Font("Segoe UI", 10F), Location = new Point(200, 17) };
            pnlGridHeader.Controls.AddRange(new Control[] { lblGridTitle, lblGridSummary });

            dgvItems = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowTemplate = { Height = 40 }
            };

            dgvItems.Columns.Add("STT", "STT");
            dgvItems.Columns["STT"].Width = 50;
            dgvItems.Columns["STT"].ReadOnly = true;

            dgvItems.Columns.Add("BookName", "Tên sách");
            dgvItems.Columns["BookName"].ReadOnly = true;
            dgvItems.Columns["BookName"].FillWeight = 200;

            dgvItems.Columns.Add("UnitPrice", "Đơn giá");
            dgvItems.Columns["UnitPrice"].ReadOnly = true;
            dgvItems.Columns["UnitPrice"].DefaultCellStyle.Format = "N0";
            
            dgvItems.Columns.Add("Qty", "SL Mua");
            dgvItems.Columns["Qty"].ReadOnly = true;
            dgvItems.Columns["Qty"].Width = 80;

            var colReturnQty = new DataGridViewTextBoxColumn
            {
                Name = "ReturnQty",
                HeaderText = "SL Trả",
                Width = 80
            };
            dgvItems.Columns.Add(colReturnQty);

            var colReason = new DataGridViewComboBoxColumn
            {
                Name = "ReturnReason",
                HeaderText = "Lý do trả",
                Width = 150
            };
            colReason.Items.AddRange("Khách đổi ý", "Hàng lỗi", "Giao sai", "Khác...");
            dgvItems.Columns.Add(colReason);

            dgvItems.Columns.Add("RefundAmount", "Hoàn tiền");
            dgvItems.Columns["RefundAmount"].ReadOnly = true;
            dgvItems.Columns["RefundAmount"].DefaultCellStyle.Format = "N0";

            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            dgvItems.CellValidating += DgvItems_CellValidating;
            dgvItems.CurrentCellDirtyStateChanged += DgvItems_CurrentCellDirtyStateChanged;

            pnlGridContainer.Controls.Add(dgvItems);
            pnlGridContainer.Controls.Add(pnlGridHeader);

            pnlActionFooter = new Guna2Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BorderRadius = 8,
                CustomBorderThickness = new Padding(1),
                Margin = new Padding(0, 16, 0, 0)
            };

            lblTotalReturnQty = new Guna2HtmlLabel { Text = "Tổng SL Hoàn: 0", Font = new Font("Segoe UI", 11F, FontStyle.Bold), Location = new Point(20, 15) };
            lblTotalRefundAmountTitle = new Guna2HtmlLabel { Text = "TỔNG HOÀN TIỀN:", Font = new Font("Segoe UI", 11F, FontStyle.Bold), Location = new Point(20, 45) };
            lblTotalRefundAmount = new Guna2HtmlLabel { Text = "0 VNĐ", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.Firebrick, Location = new Point(170, 42) };

            btnConfirm = new Guna2Button
            {
                Text = "XÁC NHẬN TRẢ",
                BorderRadius = 6,
                Size = new Size(160, 44),
                Enabled = false,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConfirm.Click += BtnConfirm_Click;

            btnCancel = new Guna2Button
            {
                Text = "HỦY BỎ",
                BorderRadius = 6,
                Size = new Size(120, 44),
                FillColor = Color.Transparent,
                BorderThickness = 1,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += BtnCancel_Click;

            pnlActionFooter.Resize += (s, e) => {
                btnConfirm.Location = new Point(pnlActionFooter.Width - btnConfirm.Width - 20, 18);
                btnCancel.Location = new Point(btnConfirm.Left - btnCancel.Width - 10, 18);
            };

            pnlActionFooter.Controls.AddRange(new Control[] { lblTotalReturnQty, lblTotalRefundAmountTitle, lblTotalRefundAmount, btnConfirm, btnCancel });

            pnlRight.Controls.Add(pnlGridContainer);
            pnlRight.Controls.Add(pnlActionFooter);

            tlpMain.Controls.Add(pnlLeft, 0, 0);
            tlpMain.Controls.Add(pnlRight, 1, 0);

            this.Controls.Add(tlpMain);
        }
private void ApplyTheme()
        {
            BookStoreManagement.Themes.ThemeManager.ApplyTypography(this);
            this.BackColor = ThemeManager.Background;
            if (lblTitle != null) lblTitle.ForeColor = ThemeManager.TextPrimary;
            if (lblSubtitle != null) lblSubtitle.ForeColor = ThemeManager.TextSecondary;
            
            if (pnlSearch != null)
            {
                if (pnlSearch != null) pnlSearch.FillColor = ThemeManager.CardBackground;
                if (pnlSearch != null) pnlSearch.BorderColor = ThemeManager.TextBoxBorder;
            }
            
            if (txtSearch != null)
            {
                if (txtSearch != null) txtSearch.FillColor = ThemeManager.TextBoxBackground;
                if (txtSearch != null) txtSearch.ForeColor = ThemeManager.TextPrimary;
                if (txtSearch != null) txtSearch.BorderColor = ThemeManager.TextBoxBorder;
                if (txtSearch != null) txtSearch.FocusedState.BorderColor = ThemeManager.ButtonFill;
            }
            
            if (pnlOrderInfo != null)
            {
                if (pnlOrderInfo != null) pnlOrderInfo.FillColor = ThemeManager.CardBackground;
                if (pnlOrderInfo != null) pnlOrderInfo.BorderColor = ThemeManager.TextBoxBorder;
            }

            if (lblInvoiceCodeTitle != null) lblInvoiceCodeTitle.ForeColor = ThemeManager.TextSecondary;
            if (lblInvoiceCode != null) lblInvoiceCode.ForeColor = ThemeManager.TextPrimary;
            
            if (lblDateTitle != null) lblDateTitle.ForeColor = ThemeManager.TextSecondary;
            if (lblDateValue != null) lblDateValue.ForeColor = ThemeManager.TextPrimary;
            
            if (lblCustomerTitle != null) lblCustomerTitle.ForeColor = ThemeManager.TextSecondary;
            if (lblCustomerValue != null) lblCustomerValue.ForeColor = ThemeManager.TextPrimary;
            
            if (lblCashierTitle != null) lblCashierTitle.ForeColor = ThemeManager.TextSecondary;
            if (lblCashierValue != null) lblCashierValue.ForeColor = ThemeManager.TextPrimary;

            if (lblStatus != null)
            {
                if (lblStatus != null) lblStatus.ForeColor = Color.White;
                if (lblStatus != null) lblStatus.BackColor = ThemeManager.HoverColor;
            }
            
            if (pnlGridContainer != null)
            {
                if (pnlGridContainer != null) pnlGridContainer.FillColor = ThemeManager.CardBackground;
                if (pnlGridContainer != null) pnlGridContainer.BorderColor = ThemeManager.TextBoxBorder;
            }

            ThemeManager.ApplyDataGridViewStyle(dgvItems);

            if (pnlActionFooter != null)
            {
                if (pnlActionFooter != null) pnlActionFooter.FillColor = ThemeManager.CardBackground;
                if (pnlActionFooter != null) pnlActionFooter.BorderColor = ThemeManager.TextBoxBorder;
            }
            
            if (lblTotalReturnQty != null) lblTotalReturnQty.ForeColor = ThemeManager.TextPrimary;
            if (lblTotalRefundAmountTitle != null) lblTotalRefundAmountTitle.ForeColor = ThemeManager.TextSecondary;
            if (lblTotalRefundAmount != null) lblTotalRefundAmount.ForeColor = ThemeManager.TextPrimary;

            if (btnConfirm != null)
            {
                if (btnConfirm != null) btnConfirm.FillColor = ThemeManager.ButtonFill;
                if (btnConfirm != null) btnConfirm.ForeColor = ThemeManager.ButtonText;
            }
            if (btnCancel != null)
            {
                if (btnCancel != null) btnCancel.FillColor = ThemeManager.Background;
                if (btnCancel != null) btnCancel.BorderColor = ThemeManager.TextBoxBorder;
                if (btnCancel != null) btnCancel.ForeColor = ThemeManager.TextPrimary;
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

                if (this.IsDisposed) return;
                
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

                if (this.IsDisposed) return;

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
                if (dgvItems.Columns.Count == 0)
                {
                    dgvItems.Columns.Add("STT", "STT");
                    dgvItems.Columns["STT"].Width = 50;
                    dgvItems.Columns["STT"].ReadOnly = true;
                    
                    dgvItems.Columns.Add("Product", "SẢN PHẨM");
                    dgvItems.Columns["Product"].FillWeight = 200;
                    dgvItems.Columns["Product"].ReadOnly = true;
                    
                    dgvItems.Columns.Add("Price", "ĐƠN GIÁ");
                    dgvItems.Columns["Price"].ReadOnly = true;
                    
                    dgvItems.Columns.Add("BuyQty", "SL MUA");
                    dgvItems.Columns["BuyQty"].ReadOnly = true;
                    
                    dgvItems.Columns.Add("ReturnQty", "SL TRẢ");
                    
                    var reasonCol = new System.Windows.Forms.DataGridViewComboBoxColumn
                    {
                        Name = "Reason",
                        HeaderText = "LÝ DO",
                        FlatStyle = System.Windows.Forms.FlatStyle.Flat
                    };
                    reasonCol.Items.AddRange("Khách đổi ý", "Hàng lỗi", "Giao sai mẫu", "Lý do khác");
                    dgvItems.Columns.Add(reasonCol);
                    
                    dgvItems.Columns.Add("Refund", "TIỀN HOÀN");
                    dgvItems.Columns["Refund"].ReadOnly = true;
                }

                dgvItems.Rows.Clear();
                int stt = 1;
                foreach (var detail in _currentOrderDetails)
                {
                    int rowIndex = dgvItems.Rows.Add(
                        stt++,
                        $"{detail.Title}\nCode: {detail.BookCode}",
                        detail.UnitPrice.ToString("N0") + " đ",
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
            
            // Adjust position dynamically to avoid overlap
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
                        ,
                            IsRestock = true
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
