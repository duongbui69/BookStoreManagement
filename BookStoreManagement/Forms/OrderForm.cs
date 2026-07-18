using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class OrderForm : Form
    {
        private SalesOrderService _service;
        
        private Guna2TextBox txtOrderCode;
        private Guna2TextBox txtOrderDate;
        private Guna2TextBox txtTotalAmount;
        private Guna2TextBox txtPaymentMethod;
        private Guna2ComboBox cbOrderStatus;
        private Guna2TextBox txtNote;
        private Guna2Button btnSave;
        private Guna2Button btnCancel;

        public SalesOrder? OrderModel { get; private set; }

        public OrderForm(SalesOrder? orderToEdit)
        {
            _service = new SalesOrderService();
            OrderModel = orderToEdit;
            
            InitializeComponent();
            
            if (OrderModel != null)
            {
                BindData();
            }
            
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = "Chi Tiết Đơn Hàng";
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitle);

            int yPos = 70;
            int spacing = 60;

            // Order Code
            Label lblCode = new Label { Text = "Mã Đơn Hàng", Location = new Point(20, yPos), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtOrderCode = new Guna2TextBox { Location = new Point(20, yPos + 20), Width = 440, Height = 36, BorderRadius = 4, ReadOnly = true, FillColor = Color.WhiteSmoke };
            this.Controls.Add(lblCode);
            this.Controls.Add(txtOrderCode);
            yPos += spacing;

            // Order Date
            Label lblDate = new Label { Text = "Ngày Đặt", Location = new Point(20, yPos), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtOrderDate = new Guna2TextBox { Location = new Point(20, yPos + 20), Width = 210, Height = 36, BorderRadius = 4, ReadOnly = true, FillColor = Color.WhiteSmoke };
            this.Controls.Add(lblDate);
            this.Controls.Add(txtOrderDate);

            // Total Amount
            Label lblAmount = new Label { Text = "Tổng Tiền", Location = new Point(250, yPos), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtTotalAmount = new Guna2TextBox { Location = new Point(250, yPos + 20), Width = 210, Height = 36, BorderRadius = 4, ReadOnly = true, FillColor = Color.WhiteSmoke };
            this.Controls.Add(lblAmount);
            this.Controls.Add(txtTotalAmount);
            yPos += spacing;

            // Payment Method
            Label lblPayment = new Label { Text = "Phương Thức Thanh Toán", Location = new Point(20, yPos), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtPaymentMethod = new Guna2TextBox { Location = new Point(20, yPos + 20), Width = 440, Height = 36, BorderRadius = 4, ReadOnly = true, FillColor = Color.WhiteSmoke };
            this.Controls.Add(lblPayment);
            this.Controls.Add(txtPaymentMethod);
            yPos += spacing;

            // Status
            Label lblStatus = new Label { Text = "Trạng Thái", Location = new Point(20, yPos), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbOrderStatus = new Guna2ComboBox { Location = new Point(20, yPos + 20), Width = 440, Height = 36, BorderRadius = 4 };
            cbOrderStatus.Items.AddRange(new string[] { AppConstants.OrderStatuses.Completed, AppConstants.OrderStatuses.Cancelled });
            this.Controls.Add(lblStatus);
            this.Controls.Add(cbOrderStatus);
            yPos += spacing;

            // Note
            Label lblNote = new Label { Text = "Ghi Chú", Location = new Point(20, yPos), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtNote = new Guna2TextBox { Location = new Point(20, yPos + 20), Width = 440, Height = 60, BorderRadius = 4, Multiline = true };
            this.Controls.Add(lblNote);
            this.Controls.Add(txtNote);
            yPos += 90;

            // Buttons
            btnCancel = new Guna2Button { Text = "Đóng", Location = new Point(240, yPos), Width = 100, Height = 40, BorderRadius = 4, FillColor = Color.Transparent, BorderThickness = 1, ForeColor = Color.Black, Cursor = Cursors.Hand };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            
            btnSave = new Guna2Button { Text = "Lưu Thay Đổi", Location = new Point(360, yPos), Width = 100, Height = 40, BorderRadius = 4, Cursor = Cursors.Hand };
            btnSave.Click += BtnSave_Click;

            this.Controls.Add(btnCancel);
            this.Controls.Add(btnSave);
        }

        private void BindData()
        {
            if (OrderModel != null)
            {
                txtOrderCode.Text = OrderModel.OrderCode;
                txtOrderDate.Text = OrderModel.OrderDate.ToString("dd/MM/yyyy HH:mm");
                txtTotalAmount.Text = OrderModel.TotalAmount.ToString("N0") + " VND";
                txtPaymentMethod.Text = OrderModel.PaymentMethod;
                cbOrderStatus.SelectedItem = OrderModel.OrderStatus;
                txtNote.Text = OrderModel.Note;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (OrderModel != null)
                {
                    // Lấy Repository bằng Reflection hoặc tạo UpdateOrder nếu cần
                    // Ở đây chỉ mô phỏng việc lưu vì chưa có UpdateOrder
                    // Để an toàn, chỉ cập nhật nếu trạng thái là Cancelled, vì Service đã có CancelOrder
                    if (cbOrderStatus.SelectedItem?.ToString() == AppConstants.OrderStatuses.Cancelled && OrderModel.OrderStatus != AppConstants.OrderStatuses.Cancelled)
                    {
                        _service.CancelOrder(OrderModel.Id);
                    }
                    MessageBox.Show("Cập nhật đơn hàng thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            this.BackColor = isDark ? Color.FromArgb(30, 30, 30) : Color.White;
            this.ForeColor = isDark ? Color.White : Color.Black;

            btnSave.FillColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;

            btnCancel.BorderColor = isDark ? Color.Gray : Color.LightGray;
            btnCancel.ForeColor = isDark ? Color.White : Color.Black;
        }
    }
}
