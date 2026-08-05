using BookStoreManagement.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using BookStoreManagement.ViewModels;
using BookStoreManagement.Models;

namespace BookStoreManagement.Forms
{
    public partial class InvoiceForm : Form
    {
        private readonly int _salesOrderId;
        private readonly SalesOrderService _orderService;
        private readonly CustomerService _customerService;

        private Panel pnlHeader;
        private Label lblTitle;
        private Button btnClose;

        private Panel pnlBody;
        
        private TableLayoutPanel tlpInfo;
        
        // Invoice Info
        private Panel pnlInvoiceInfo;
        private Label lblInvoiceTitle;
        private Label lblOrderCode;
        private Label lblOrderDate;
        private Label lblStaffName;
        private Label lblStatus;

        // Customer Info
        private Panel pnlCustomerInfo;
        private Label lblCustomerTitle;
        private Label lblCustomerName;
        private Label lblCustomerMST;
        private Label lblCustomerPhone;
        private Label lblCustomerAddress;

        private Panel pnlProductContainer;
        private Label lblProductTitle;
        private DataGridView dgvProducts;

        private Panel pnlSummary;
        private Label lblSubTotal;
        private Label lblDiscount;
        private Label lblVAT;
        private Label lblTotal;
        private Label lblTotalAmount;

        private Panel pnlFooter;
        private Button btnCloseFooter;
        private Button btnPrint;
        private Button btnExportPdf;

        public InvoiceForm(int salesOrderId)
        {
            _salesOrderId = salesOrderId;
            _orderService = new SalesOrderService();
            _customerService = new CustomerService();
            InitializeUI();
            ApplyTheme();
            this.Load += InvoiceForm_Load;
        }

        private void InitializeUI()
        {
            this.Text = "Chi tiết hóa đơn";
            this.Size = new Size(800, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = ThemeManager.TextBoxBorder;

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = ThemeManager.TextBoxBorder };
            Panel pnlInner = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Background };
            pnlMain.Controls.Add(pnlInner);

            // 1. Header
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ThemeManager.ButtonFill };
            lblTitle = new Label { Text = "CHI TIẾT HÓA ĐƠN", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.White, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
            btnClose = new Button 
            { 
                Text = "X", 
                Font = new Font("Segoe UI", 14F, FontStyle.Bold), 
                ForeColor = Color.White, 
                BackColor = Color.Transparent, 
                FlatStyle = FlatStyle.Flat, 
                Size = new Size(40, 40), 
                Cursor = Cursors.Hand, 
                Dock = DockStyle.Right 
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(231, 76, 60);
            btnClose.Click += (s, e) => this.Close();

            pnlHeader.Controls.Add(btnClose);
            pnlHeader.Controls.Add(lblTitle); // fill must be added last or sent to back
            lblTitle.SendToBack();

            // 2. Footer
            pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = ThemeManager.CardBackground };
            Panel pnlFooterTopBorder = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ThemeManager.TextBoxBorder };
            pnlFooter.Controls.Add(pnlFooterTopBorder);

            btnExportPdf = new Button { Text = "Xuất PDF", Size = new Size(120, 40), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.ButtonFill, ForeColor = Color.White, Location = new Point(650, 15) };
            btnExportPdf.FlatAppearance.BorderSize = 0;

            btnPrint = new Button { Text = "In Hóa đơn", Size = new Size(120, 40), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.Background, ForeColor = ThemeManager.ButtonFill, Location = new Point(515, 15) };
            btnPrint.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            btnCloseFooter = new Button { Text = "Đóng", Size = new Size(100, 40), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat, BackColor = ThemeManager.Background, ForeColor = ThemeManager.TextPrimary, Location = new Point(400, 15) };
            btnCloseFooter.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;
            btnCloseFooter.Click += (s, e) => this.Close();

            pnlFooter.Controls.Add(btnExportPdf);
            pnlFooter.Controls.Add(btnPrint);
            pnlFooter.Controls.Add(btnCloseFooter);

            // 3. Body
            pnlBody = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(24) };
            
            // Info TableLayout
            tlpInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 1,
                Height = 160,
                Margin = new Padding(0, 0, 0, 24)
            };
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            pnlInvoiceInfo = CreateInfoBox("THÔNG TIN HÓA ĐƠN", out lblInvoiceTitle);
            pnlInvoiceInfo.Margin = new Padding(0, 0, 12, 0);
            lblOrderCode = AddInfoRow(pnlInvoiceInfo, "Mã hóa đơn:", "...", 40, true);
            lblOrderDate = AddInfoRow(pnlInvoiceInfo, "Ngày lập:", "...", 65);
            lblStaffName = AddInfoRow(pnlInvoiceInfo, "Nhân viên bán:", "...", 90);
            lblStatus = AddInfoRow(pnlInvoiceInfo, "Trạng thái:", "...", 115, true, ThemeManager.ButtonFill);

            pnlCustomerInfo = CreateInfoBox("THÔNG TIN KHÁCH HÀNG", out lblCustomerTitle);
            pnlCustomerInfo.Margin = new Padding(12, 0, 0, 0);
            lblCustomerName = AddInfoRow(pnlCustomerInfo, "Tên KH:", "...", 40, true);
            lblCustomerMST = AddInfoRow(pnlCustomerInfo, "MST/CCCD:", "...", 65);
            lblCustomerPhone = AddInfoRow(pnlCustomerInfo, "Điện thoại:", "...", 90);
            lblCustomerAddress = AddInfoRow(pnlCustomerInfo, "Địa chỉ:", "...", 115);

            tlpInfo.Controls.Add(pnlInvoiceInfo, 0, 0);
            tlpInfo.Controls.Add(pnlCustomerInfo, 1, 0);

            // Product Grid
            pnlProductContainer = new Panel { Dock = DockStyle.Top, Height = 250, Margin = new Padding(0, 24, 0, 24) };
            
            lblProductTitle = new Label { Text = "CHI TIẾT SẢN PHẨM", Dock = DockStyle.Top, Height = 40, Font = new Font("Segoe UI", 10F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, BackColor = ThemeManager.CardBackground, ForeColor = ThemeManager.ButtonFill };
            Panel pnlGridBorder = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = ThemeManager.TextBoxBorder };
            pnlProductContainer.Controls.Add(pnlGridBorder);
            pnlGridBorder.Controls.Add(lblProductTitle); // Will be pushed down. Wait, Dock = Fill. Better: Top for label, Fill for Grid.
            
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false,
                BackgroundColor = ThemeManager.Background,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = ThemeManager.TextBoxBorder
            };
            dgvProducts.SetDoubleBuffered(true);
            dgvProducts.RowTemplate.Height = 40;
            dgvProducts.ColumnHeadersHeight = 40;

            dgvProducts.Columns.Add("STT", "STT");
            dgvProducts.Columns.Add("Tiêu đề", "Tên sách / Sản phẩm");
            dgvProducts.Columns.Add("UnitPrice", "Đơn giá");
            dgvProducts.Columns.Add("Số lượng", "SL");
            dgvProducts.Columns.Add("LineTotal", "Tổng tiền");

            dgvProducts.Columns["STT"].Width = 50;
            dgvProducts.Columns["Số lượng"].Width = 60;
            dgvProducts.Columns["UnitPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns["LineTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns["STT"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvProducts.Columns["Số lượng"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            foreach (DataGridViewColumn col in dgvProducts.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            Panel pnlGridHost = new Panel { Dock = DockStyle.Fill };
            pnlGridHost.Controls.Add(dgvProducts);
            pnlGridBorder.Controls.Add(pnlGridHost);
            lblProductTitle.Dock = DockStyle.Top;
            pnlGridBorder.Controls.Add(lblProductTitle); // ensure label is top

            // Summary
            pnlSummary = new Panel { Dock = DockStyle.Top, Height = 140 };
            Panel pnlSummaryBox = new Panel { Width = 300, Height = 140, BorderStyle = BorderStyle.FixedSingle };
            // Manual right alignment
            pnlSummary.Resize += (s, e) => pnlSummaryBox.Location = new Point(pnlSummary.Width - pnlSummaryBox.Width, 0);

            lblSubTotal = AddSummaryRow(pnlSummaryBox, "Tổng tiền hàng:", "0 ₫", 10);
            lblDiscount = AddSummaryRow(pnlSummaryBox, "Chiết khấu:", "0 ₫", 35);
            lblVAT = AddSummaryRow(pnlSummaryBox, "VAT (8%):", "0 ₫", 60);
            
            Panel pnlLine = new Panel { Width = 280, Height = 1, BackColor = ThemeManager.TextBoxBorder, Location = new Point(10, 90) };
            pnlSummaryBox.Controls.Add(pnlLine);

            Label lblTotalText = new Label { Text = "TỔNG CỘNG:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 105) };
            lblTotalAmount = new Label { Text = "0 ₫", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = ThemeManager.ButtonFill, AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Width = 150, Location = new Point(140, 100) };
            
            pnlSummaryBox.Controls.Add(lblTotalText);
            pnlSummaryBox.Controls.Add(lblTotalAmount);
            pnlSummary.Controls.Add(pnlSummaryBox);

            pnlBody.Controls.Add(pnlSummary);
            pnlBody.Controls.Add(pnlProductContainer);
            pnlBody.Controls.Add(tlpInfo);

            pnlInner.Controls.Add(pnlBody);
            pnlInner.Controls.Add(pnlFooter);
            pnlInner.Controls.Add(pnlHeader);

            this.Controls.Add(pnlMain);
        }

        private Panel CreateInfoBox(string title, out Label lblTitle)
        {
            Panel p = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, BackColor = ThemeManager.Background };
            lblTitle = new Label { Text = title, Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = ThemeManager.ButtonFill, TextAlign = ContentAlignment.MiddleCenter };
            Panel line = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ThemeManager.TextBoxBorder };
            p.Controls.Add(line);
            p.Controls.Add(lblTitle);
            return p;
        }

        private Label AddInfoRow(Panel parent, string label, string value, int yPos, bool isBold = false, Color? valueColor = null)
        {
            Label lblKey = new Label { Text = label, Font = new Font("Segoe UI", 9.5F), ForeColor = ThemeManager.TextSecondary, AutoSize = true, Location = new Point(15, yPos) };
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 9.5F, isBold ? FontStyle.Bold : FontStyle.Regular), ForeColor = valueColor ?? ThemeManager.TextPrimary, AutoSize = false, Width = 200, TextAlign = ContentAlignment.MiddleRight, Location = new Point(140, yPos) };
            parent.Controls.Add(lblKey);
            parent.Controls.Add(lblValue);
            // Resize parent handle
            parent.Resize += (s, e) => { lblValue.Width = parent.Width - 155; lblValue.Location = new Point(140, yPos); };
            return lblValue;
        }

        private Label AddSummaryRow(Panel parent, string label, string value, int yPos)
        {
            Label lblKey = new Label { Text = label, Font = new Font("Segoe UI", 9.5F), ForeColor = ThemeManager.TextSecondary, AutoSize = true, Location = new Point(10, yPos) };
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 9.5F), ForeColor = ThemeManager.TextPrimary, AutoSize = false, Width = 150, TextAlign = ContentAlignment.MiddleRight, Location = new Point(140, yPos) };
            parent.Controls.Add(lblKey);
            parent.Controls.Add(lblValue);
            return lblValue;
        }

        private void ApplyTheme()
        {
            // Apply theme colors to custom panels
            pnlInvoiceInfo.BackColor = ThemeManager.CardBackground;
            pnlCustomerInfo.BackColor = ThemeManager.CardBackground;
            
            lblInvoiceTitle.ForeColor = ThemeManager.ButtonFill;
            lblCustomerTitle.ForeColor = ThemeManager.ButtonFill;

            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Background;
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvProducts.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Background;

            dgvProducts.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            dgvProducts.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            dgvProducts.DefaultCellStyle.SelectionBackColor = ThemeManager.CardBackground;
            dgvProducts.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.CardBackground;
            
            pnlSummary.Controls[0].BackColor = ThemeManager.CardBackground; // pnlSummaryBox
        }

        private async void InvoiceForm_Load(object? sender, EventArgs e)
        {
            try
            {
                var details = await _orderService.GetDetailsAsync(_salesOrderId);
                if (details == null || details.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy chi tiết hóa đơn.");
                    this.Close();
                    return;
                }

                var first = details.First();

                lblOrderCode.Text = first.OrderCode;
                lblOrderDate.Text = first.OrderDate.ToString("dd/MM/yyyy HH:mm");
                lblStaffName.Text = first.StaffName;
                lblStatus.Text = first.OrderStatus;
                
                // Colors for status
                if (first.OrderStatus.ToLower().Contains("hoàn thành") || first.OrderStatus.ToLower().Contains("đã thanh toán")) lblStatus.ForeColor = Color.FromArgb(34, 197, 94);
                else if (first.OrderStatus.ToLower().Contains("hủy") || first.OrderStatus.ToLower().Contains("chưa thanh toán")) lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
                else lblStatus.ForeColor = Color.FromArgb(245, 158, 11);

                if (first.CustomerId.HasValue && first.CustomerId.Value > 0)
                {
                    var customer = await _customerService.GetByIdAsync(first.CustomerId.Value);
                    if (customer != null)
                    {
                        lblCustomerName.Text = customer.FullName;
                        lblCustomerMST.Text = string.IsNullOrEmpty(customer.IdentityNumber) ? "-" : customer.IdentityNumber;
                        lblCustomerPhone.Text = string.IsNullOrEmpty(customer.Phone) ? "-" : customer.Phone;
                        lblCustomerAddress.Text = string.IsNullOrEmpty(customer.Address) ? "-" : customer.Address;
                    }
                    else
                    {
                        lblCustomerName.Text = first.CustomerName;
                        lblCustomerMST.Text = "-";
                        lblCustomerPhone.Text = "-";
                        lblCustomerAddress.Text = "-";
                    }
                }
                else
                {
                    lblCustomerName.Text = "Khách vãng lai";
                    lblCustomerMST.Text = "-";
                    lblCustomerPhone.Text = "-";
                    lblCustomerAddress.Text = "-";
                }

                decimal sumTotal = first.TotalAmount;
                decimal subTotal = sumTotal / 1.08m;
                decimal vat = sumTotal - subTotal;
                decimal totalDiscount = details.Sum(d => d.DiscountAmount); // Approximate

                lblSubTotal.Text = subTotal.ToString("N0") + " ₫";
                lblDiscount.Text = totalDiscount.ToString("N0") + " ₫";
                lblVAT.Text = vat.ToString("N0") + " ₫";
                lblTotalAmount.Text = sumTotal.ToString("N0") + " ₫";

                dgvProducts.Rows.Clear();
                int stt = 1;
                foreach (var d in details)
                {
                    dgvProducts.Rows.Add(
                        stt++,
                        d.Title,
                        d.UnitPrice.ToString("N0") + " ₫",
                        d.Quantity,
                        d.LineTotal.ToString("N0") + " ₫"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết: " + ex.Message);
            }
        }
    }
}
