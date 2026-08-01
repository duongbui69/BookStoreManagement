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
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class RefundForm : Form
    {
        private int _refundId;
        private ReturnReceiptRepository _returnRepo;
        private BookStoreManagement.Services.SalesOrderService _orderService;
        
        
        
        

        private Guna2Panel pnlContent;
        
        // Form Controls
        private Guna2TextBox txtOrderCode;
        private Guna2Button btnSearchOrder;
        private Label lblCustomerInfo;
        
        private Guna2DataGridView dgvDetails;
        
        private Guna2TextBox txtNote;
        private Guna2ComboBox cboStatus;
        private Label lblTotalRefund;

        private Guna2Button btnSave;
        private Guna2Button btnCancel;
        
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
            this.Text = _refundId == 0 ? "Tạo Phiếu Trả Hàng" : "Chi tiết Phiếu Trả Hàng";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ControlBox = false;

            Guna2Panel pnlMain = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(2) };
            pnlMain.BackColor = ThemeManager.TextBoxBorder;
            this.Controls.Add(pnlMain);

            Guna2Panel pnlInner = new Guna2Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Background };
            pnlMain.Controls.Add(pnlInner);

            // Header
            
            
            
            // Content
            pnlContent = new Guna2Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            // Top Section (Order Search / Info)
            Guna2Panel pnlTop = new Guna2Panel { Dock = DockStyle.Top, Height = 100 };
            
            Label lblOrder = new Label { Text = "Mã đơn hàng (*):", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 10) };
            txtOrderCode = new Guna2TextBox { Font = new Font("Segoe UI", 10F), Width = 200, Height = 36, Location = new Point(140, 5), BorderRadius = 4 };
            
            btnSearchOrder = new Guna2Button { Text = "Tìm Đơn hàng", Font = new Font("Segoe UI", 9F, FontStyle.Bold), Size = new Size(120, 36), Location = new Point(350, 5), BorderRadius = 4 };
            btnSearchOrder.Click += BtnSearchOrder_Click;

            lblCustomerInfo = new Label { Text = "Khách hàng: -", Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(0, 45) };

            Label lblStatus = new Label { Text = "Trạng thái:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Location = new Point(550, 10) };
            cboStatus = new Guna2ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F), Width = 150, Height = 36, Location = new Point(650, 5), BorderRadius = 4 };
            cboStatus.Items.AddRange(new object[] { "Đang xử lý", "Đã duyệt", "Đã hoàn tiền", "Từ chối" });
            cboStatus.SelectedIndex = 0;
            
            pnlTop.Controls.Add(lblOrder);
            pnlTop.Controls.Add(txtOrderCode);
            pnlTop.Controls.Add(btnSearchOrder);
            pnlTop.Controls.Add(lblCustomerInfo);
            pnlTop.Controls.Add(lblStatus);
            pnlTop.Controls.Add(cboStatus);
            
            // Grid
            dgvDetails = new Guna2DataGridView
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
                dgvDetails.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colSelect", HeaderText = "Chọn", Width = 50 });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle", HeaderText = "Tên sách", DataPropertyName = "Title", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "Đơn giá", DataPropertyName = "UnitPrice", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMaxQty", HeaderText = "SL Mua", DataPropertyName = "Quantity", Width = 80, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReturnQty", HeaderText = "Số lượng trả", Width = 80 });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReason", HeaderText = "Lý do (Chi tiết)", Width = 200 });
            }
            else
            {
                // View mode
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle", HeaderText = "Tên sách", DataPropertyName = "Title", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "Đơn giá", DataPropertyName = "UnitPrice", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReturnQty", HeaderText = "Số lượng trả", DataPropertyName = "Quantity", Width = 80, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTotal", HeaderText = "Tổng tiền", DataPropertyName = "RefundAmount", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }, ReadOnly = true });
                dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReason", HeaderText = "Lý do", DataPropertyName = "ReturnReason", Width = 200, ReadOnly = true });
            }

            dgvDetails.CellValueChanged += DgvDetails_CellValueChanged;
            dgvDetails.CurrentCellDirtyStateChanged += DgvDetails_CurrentCellDirtyStateChanged;

            // Bottom Section
            Guna2Panel pnlBottom = new Guna2Panel { Dock = DockStyle.Bottom, Height = 140, Padding = new Padding(0, 10, 0, 0) };
            
            Label lblNote = new Label { Text = "Ghi chú phiếu:", Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(0, 10) };
            txtNote = new Guna.UI2.WinForms.Guna2TextBox { Font = new Font("Segoe UI", 10F), Multiline = true, Width = 400, Height = 70, Location = new Point(0, 30), BorderRadius = 4 };
            
            lblTotalRefund = new Label { Text = "TỔNG HOÀN TIỀN: 0 ₫", Font = new Font("Segoe UI", 14F, FontStyle.Bold), AutoSize = true, Location = new Point(450, 30) };

            btnSave = new Guna2Button { Text = _refundId == 0 ? "Tạo Phiếu" : "Cập Nhật", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Size = new Size(150, 40), Location = new Point(700, 80), BorderRadius = 6 };
            btnSave.Click += BtnSave_Click;

            pnlBottom.Controls.Add(lblNote);
            pnlBottom.Controls.Add(txtNote);
            pnlBottom.Controls.Add(lblTotalRefund);
            pnlBottom.Controls.Add(btnSave);

            btnCancel = new Guna2Button { Text = "Hủy bỏ", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Size = new Size(110, 40), Location = new Point(570, 80), BorderRadius = 6, FillColor = Color.Transparent, BorderThickness = 1, ForeColor = ThemeManager.TextPrimary, Cursor = Cursors.Hand };
            btnCancel.Click += (s, e) => this.Close();
            
            pnlBottom.Controls.Add(btnCancel);


            pnlContent.Controls.Add(dgvDetails);
            pnlContent.Controls.Add(pnlTop);
            pnlContent.Controls.Add(pnlBottom);

            pnlInner.Controls.Add(pnlContent);
            
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlContent.BackColor = ThemeManager.Background;
            
            
            foreach (Control c in pnlContent.Controls)
            {
                if (c is Panel pnl)
                {
                    foreach (Control child in pnl.Controls)
                    {
                        if (child is Label lbl) lbl.ForeColor = ThemeManager.TextPrimary;
                    }
                }
            }

            
            btnSearchOrder.FillColor = ThemeManager.CardBackground;
            btnSearchOrder.ForeColor = ThemeManager.TextPrimary;
            btnSearchOrder.BorderColor = ThemeManager.TextBoxBorder;
            btnSearchOrder.BorderThickness = 1;

            btnSave.FillColor = ThemeManager.ButtonFill;
            if (btnCancel != null) { btnCancel.ForeColor = ThemeManager.TextPrimary; btnCancel.BorderColor = ThemeManager.TextBoxBorder; }
            btnSave.ForeColor = Color.White;
            

            txtOrderCode.FillColor = ThemeManager.CardBackground;
            txtOrderCode.ForeColor = ThemeManager.TextPrimary;
            txtNote.FillColor = ThemeManager.CardBackground;
            txtNote.ForeColor = ThemeManager.TextPrimary;
            cboStatus.FillColor = ThemeManager.CardBackground;
            cboStatus.ForeColor = ThemeManager.TextPrimary;

            ThemeManager.ApplyDataGridViewStyle(dgvDetails);
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
                    cboStatus.SelectedItem = receipt.ReturnStatus ?? "Đang xử lý";
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
                    lblCustomerInfo.Text = _currentOrder != null ? $"Khách hàng ID: {_currentOrder.CustomerId}" : "Không tìm thấy";
                }
                else
                {
                    MessageBox.Show("Không tìm thấy đơn hàng hoặc đơn hàng trống!");
                }
            }
            else
            {
                MessageBox.Show("Vui lòng nhập Mã đơn hàng dạng số!");
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
                MessageBox.Show("Vui lòng tìm và chọn đơn hàng hợp lệ.");
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
                ReturnStatus = cboStatus.SelectedItem.ToString() ?? "Đang xử lý"
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
                MessageBox.Show("Vui lòng chọn ít nhất 1 sản phẩm để trả và nhập số lượng > 0.");
                return;
            }

            receipt.TotalRefundAmount = totalRefund;

            try
            {
                _returnRepo.CreateReturn(receipt, details);
                MessageBox.Show("Tạo phiếu trả thành công!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo phiếu: " + ex.Message);
            }
        }

        private void UpdateRefundStatus()
        {
            try
            {
                // In a real app we'd have UpdateAsync. For now, we simulate or execute raw SQL.
                string status = cboStatus.SelectedItem.ToString() ?? "Đang xử lý";
                string note = txtNote.Text;
                
                string sql = "UPDATE ReturnReceipts SET ReturnStatus = @Status, Note = @Note WHERE Id = @Id";
                var conn = DbConnectionFactory.CreateConnection();
                conn.Open();
                var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Note", note);
                cmd.Parameters.AddWithValue("@Id", _refundId);
                cmd.ExecuteNonQuery();
                
                MessageBox.Show("Cập nhật thành công!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message);
            }
        }
    }
}
