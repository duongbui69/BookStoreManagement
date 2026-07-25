using BookStoreManagement.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookStoreManagement.Database;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Forms
{
    public class RefundForm : Form
    {
        private int _refundId;
        private ReturnReceiptRepository _returnRepo;
        private BookStoreManagement.Services.SalesOrderService _orderService;
        
        private Panel pnlHeader;
        private Label lblTitle;
        private Button btnClose;

        private Panel pnlContent;
        
        // Form Controls
        private TextBox txtOrderCode;
        private Button btnSearchOrder;
        private Label lblCustomerInfo;
        
        private DataGridView dgvDetails;
        
        private TextBox txtNote;
        private ComboBox cboStatus;
        private Label lblTotalRefund;

        private Button btnSave;
        
        private SalesOrder? _currentOrder;
        private List<SalesOrderDetailFullViewModel> _orderDetails = new List<SalesOrderDetailFullViewModel>();
        private List<ReturnReceiptDetailFullViewModel> _returnDetails = new List<ReturnReceiptDetailFullViewModel>();

        public RefundForm(int refundId = 0)
        {
            _refundId = refundId;
            _returnRepo = new ReturnReceiptRepository();
            _orderService = new BookStoreManagement.Services.SalesOrderService();

            InitializeComponents();
            ApplyTheme();
            
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Load += RefundForm_Load;
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void InitializeComponents()
        {
            this.Text = _refundId == 0 ? "Create Return Receipt" : "Return Receipt Details";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(2) };
            pnlMain.BackColor = ThemeManager.TextBoxBorder;
            this.Controls.Add(pnlMain);

            Panel pnlInner = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Background };
            pnlMain.Controls.Add(pnlInner);

            // Header
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ThemeManager.ButtonFill };
            lblTitle = new Label { Text = this.Text.ToUpper(), Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.White, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
            
            btnClose = new Button { Text = "X", Size = new Size(40, 40), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.White, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Location = new Point(this.Width - 45, 10);
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click += (s, e) => this.Close();
            
            pnlHeader.Controls.Add(btnClose);
            pnlHeader.Controls.Add(lblTitle);
            
            // Content
            pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            // Top Section (Order Search / Info)
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 100 };
            
            Label lblOrder = new Label { Text = "Order ID (*):", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 10) };
            txtOrderCode = new TextBox { Font = new Font("Segoe UI", 12F), Width = 200, Location = new Point(140, 5) };
            
            btnSearchOrder = new Button { Text = "Search Order", Font = new Font("Segoe UI", 10F), Size = new Size(100, 30), Location = new Point(350, 5), FlatStyle = FlatStyle.Flat };
            btnSearchOrder.Click += BtnSearchOrder_Click;

            lblCustomerInfo = new Label { Text = "Khách hàng: -", Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(0, 45) };

            Label lblStatus = new Label { Text = "Status:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Location = new Point(550, 10) };
            cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 12F), Width = 150, Location = new Point(650, 5) };
            cboStatus.Items.AddRange(new object[] { "Processing", "Approved", "Refunded", "Rejected" });
            cboStatus.SelectedIndex = 0;
            
            pnlTop.Controls.Add(lblOrder);
            pnlTop.Controls.Add(txtOrderCode);
            pnlTop.Controls.Add(btnSearchOrder);
            pnlTop.Controls.Add(lblCustomerInfo);
            pnlTop.Controls.Add(lblStatus);
            pnlTop.Controls.Add(cboStatus);
            
            // Grid
            dgvDetails = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowTemplate = { Height = 40 }
            };
            dgvDetails.SetDoubleBuffered(true);

            if (_refundId == 0)
            {
                // Add mode
                dgvDetails.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colSelect", HeaderText = "Select", Width = 50 });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle", HeaderText = "Book title", DataPropertyName = "Title", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "Unit price", DataPropertyName = "UnitPrice", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMaxQty", HeaderText = "SL Mua", DataPropertyName = "Quantity", Width = 80, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReturnQty", HeaderText = "Return Qty", Width = 80 });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReason", HeaderText = "Reason (Details)", Width = 200 });
            }
            else
            {
                // View mode
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle", HeaderText = "Book title", DataPropertyName = "Title", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "Unit price", DataPropertyName = "UnitPrice", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReturnQty", HeaderText = "Return Qty", DataPropertyName = "Quantity", Width = 80, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTotal", HeaderText = "Total amount", DataPropertyName = "RefundAmount", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReason", HeaderText = "Reason", DataPropertyName = "ReturnReason", Width = 200, ReadOnly = true });
            }

            dgvDetails.CellValueChanged += DgvDetails_CellValueChanged;
            dgvDetails.CurrentCellDirtyStateChanged += DgvDetails_CurrentCellDirtyStateChanged;

            // Bottom Section
            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 140, Padding = new Padding(0, 10, 0, 0) };
            
            Label lblNote = new Label { Text = "Receipt note:", Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(0, 10) };
            txtNote = new TextBox { Font = new Font("Segoe UI", 10F), Multiline = true, Width = 400, Height = 70, Location = new Point(0, 30) };
            
            lblTotalRefund = new Label { Text = "TOTAL REFUND: 0 ₫", Font = new Font("Segoe UI", 14F, FontStyle.Bold), AutoSize = true, Location = new Point(450, 30) };

            btnSave = new Button { Text = _refundId == 0 ? "Create Receipt" : "Update", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Size = new Size(150, 40), Location = new Point(700, 80), FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;

            pnlBottom.Controls.Add(lblNote);
            pnlBottom.Controls.Add(txtNote);
            pnlBottom.Controls.Add(lblTotalRefund);
            pnlBottom.Controls.Add(btnSave);

            pnlContent.Controls.Add(dgvDetails);
            pnlContent.Controls.Add(pnlTop);
            pnlContent.Controls.Add(pnlBottom);

            pnlInner.Controls.Add(pnlContent);
            pnlInner.Controls.Add(pnlHeader);
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;
            
            lblCustomerInfo.ForeColor = ThemeManager.TextPrimary;
            lblTotalRefund.ForeColor = ThemeManager.TextPrimary;
            
            btnSearchOrder.BackColor = ThemeManager.CardBackground;
            btnSearchOrder.ForeColor = ThemeManager.TextPrimary;
            btnSearchOrder.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            btnSave.BackColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = Color.White;
            btnSave.FlatAppearance.BorderSize = 0;

            txtOrderCode.BackColor = ThemeManager.CardBackground;
            txtOrderCode.ForeColor = ThemeManager.TextPrimary;
            txtNote.BackColor = ThemeManager.CardBackground;
            txtNote.ForeColor = ThemeManager.TextPrimary;
            cboStatus.BackColor = ThemeManager.CardBackground;
            cboStatus.ForeColor = ThemeManager.TextPrimary;

            dgvDetails.BackgroundColor = ThemeManager.CardBackground;
            dgvDetails.GridColor = ThemeManager.TextBoxBorder;
            dgvDetails.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvDetails.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvDetails.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvDetails.DefaultCellStyle.ForeColor = ThemeManager.TextSecondary;
        }

        private async void RefundForm_Load(object sender, EventArgs e)
        {
            if (_refundId > 0)
            {
                txtOrderCode.Enabled = false;
                btnSearchOrder.Visible = false;

                var receipt = await _returnRepo.GetByIdAsync(_refundId);
                if (receipt != null)
                {
                    txtOrderCode.Text = receipt.SalesOrderId?.ToString() ?? "";
                    txtNote.Text = receipt.Note;
                    cboStatus.SelectedItem = receipt.ReturnStatus ?? "Processing";
                    lblTotalRefund.Text = $"TỔNG HOÀN TIỀN: {receipt.TotalRefundAmount:N0} ₫";
                    
                    _returnDetails = await _returnRepo.GetDetailsAsync(_refundId);
                    dgvDetails.DataSource = _returnDetails;

                    if (_returnDetails.Count > 0)
                    {
                        var first = _returnDetails[0];
                        lblCustomerInfo.Text = $"Khách hàng: {first.CustomerName}";
                        txtOrderCode.Text = first.ReturnCode; // Just showing return code or order code
                    }
                }
            }
        }

        private async void BtnSearchOrder_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOrderCode.Text)) return;
            
            // To simplify, assume ordercode is actually the ID or we use OrderRepo
            if (int.TryParse(txtOrderCode.Text, out int orderId))
            {
                var details = await _orderService.GetDetailsAsync(orderId);
                if (details != null && details.Count > 0)
                {
                    _orderDetails = details;
                    dgvDetails.DataSource = null;
                    dgvDetails.DataSource = _orderDetails;
                    
                    _currentOrder = await _orderService.GetByIdAsync(orderId);
                    lblCustomerInfo.Text = _currentOrder != null ? $"Khách hàng ID: {_currentOrder.CustomerId}" : "Not found";
                }
                else
                {
                    MessageBox.Show("Order not found or empty!");
                }
            }
            else
            {
                MessageBox.Show("Please enter a numeric Order ID!");
            }
        }

        private void DgvDetails_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvDetails.IsCurrentCellDirty)
            {
                dgvDetails.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvDetails_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_refundId == 0 && e.RowIndex >= 0)
            {
                CalculateTotalRefund();
            }
        }

        private void CalculateTotalRefund()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvDetails.Rows)
            {
                bool isSelected = Convert.ToBoolean(row.Cells["colSelect"].Value);
                if (isSelected)
                {
                    var item = row.DataBoundItem as SalesOrderDetailFullViewModel;
                    if (item != null)
                    {
                        int returnQty = 0;
                        if (row.Cells["colReturnQty"].Value != null)
                        {
                            int.TryParse(row.Cells["colReturnQty"].Value.ToString(), out returnQty);
                        }

                        if (returnQty > item.Quantity)
                        {
                            returnQty = item.Quantity; // Cap it
                            row.Cells["colReturnQty"].Value = returnQty;
                        }
                        
                        total += returnQty * item.UnitPrice;
                    }
                }
            }
            lblTotalRefund.Text = $"TỔNG HOÀN TIỀN: {total:N0} ₫";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_refundId == 0)
            {
                CreateRefund();
            }
            else
            {
                UpdateRefundStatus();
            }
        }

        private void CreateRefund()
        {
            if (_currentOrder == null)
            {
                MessageBox.Show("Please find and select a valid order.");
                return;
            }

            var receipt = new ReturnReceipt
            {
                ReturnCode = "RET-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                SalesOrderId = _currentOrder.Id,
                StoreId = _currentOrder.StoreId,
                CustomerId = _currentOrder.CustomerId,
                UserId = BookStoreManagement.Helpers.CurrentSession.UserId,
                ReturnDate = DateTime.Now,
                Note = txtNote.Text,
                ReturnStatus = cboStatus.SelectedItem.ToString() ?? "Processing"
            };

            var details = new List<ReturnReceiptDetail>();
            decimal totalRefund = 0;

            foreach (DataGridViewRow row in dgvDetails.Rows)
            {
                bool isSelected = Convert.ToBoolean(row.Cells["colSelect"].Value);
                if (isSelected)
                {
                    var item = row.DataBoundItem as SalesOrderDetailFullViewModel;
                    if (item != null)
                    {
                        int returnQty = 0;
                        if (row.Cells["colReturnQty"].Value != null)
                            int.TryParse(row.Cells["colReturnQty"].Value.ToString(), out returnQty);

                        if (returnQty > 0)
                        {
                            decimal lineRefund = returnQty * item.UnitPrice;
                            totalRefund += lineRefund;
                            details.Add(new ReturnReceiptDetail
                            {
                                BookId = item.BookId,
                                Quantity = returnQty,
                                UnitPrice = item.UnitPrice,
                                RefundAmount = lineRefund,
                                ReturnReason = row.Cells["colReason"].Value?.ToString(),
                                IsRestock = true // Assuming restock is true by default
                            });
                        }
                    }
                }
            }

            if (details.Count == 0)
            {
                MessageBox.Show("Please select at least 1 product to return and enter quantity > 0.");
                return;
            }

            receipt.TotalRefundAmount = totalRefund;

            try
            {
                _returnRepo.CreateReturn(receipt, details);
                MessageBox.Show("Return receipt created successfully!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating receipt: " + ex.Message);
            }
        }

        private void UpdateRefundStatus()
        {
            try
            {
                // In a real app we'd have UpdateAsync. For now, we simulate or execute raw SQL.
                string status = cboStatus.SelectedItem.ToString() ?? "Processing";
                string note = txtNote.Text;
                
                string sql = "UPDATE ReturnReceipts SET ReturnStatus = @Status, Note = @Note WHERE Id = @Id";
                var conn = DbConnectionFactory.CreateConnection();
                conn.Open();
                var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Note", note);
                cmd.Parameters.AddWithValue("@Id", _refundId);
                cmd.ExecuteNonQuery();
                
                MessageBox.Show("Update successful!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update error: " + ex.Message);
            }
        }
    }
}
