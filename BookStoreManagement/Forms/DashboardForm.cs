using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Services;
using BookStoreManagement.Forms.Admin;

namespace BookStoreManagement.Forms
{
    public class DashboardForm : Form
    {
        private readonly AuthService _authService = new AuthService();

        private Panel pnlMenu = new Panel();
        private Panel pnlContent = new Panel();
        private Label lblTitle = new Label();
        private Label lblWelcome = new Label();
        private Label lblRole = new Label();

        private Button btnStores = new Button();
        private Button btnUsers = new Button();
        private Button btnBooks = new Button();
        private Button btnCategories = new Button();
        private Button btnAuthors = new Button();
        private Button btnPublishers = new Button();
        private Button btnSuppliers = new Button();
        private Button btnInventory = new Button();
        private Button btnSalesOrders = new Button();
        private Button btnPurchaseReceipts = new Button();
        private Button btnReturnReceipts = new Button();
        private Button btnRevenue = new Button();
        private Button btnLogout = new Button();

        public DashboardForm()
        {
            InitializeComponent();
            Load += DashboardForm_Load;
            FormClosed += DashboardForm_FormClosed;
        }

        private void InitializeComponent()
        {
            Text = "BookStoreManagement - Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);

            pnlMenu.Dock = DockStyle.Left;
            pnlMenu.Width = 230;
            pnlMenu.BackColor = Color.FromArgb(34, 40, 49);

            pnlContent.Dock = DockStyle.Fill;
            pnlContent.BackColor = Color.WhiteSmoke;

            lblTitle.Text = "BOOK STORE";
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Height = 60;
            lblTitle.Dock = DockStyle.Top;

            pnlMenu.Controls.Add(lblTitle);

            AddMenuButton(btnStores, "Cửa hàng", 70, (s, e) => OpenChild(new StoreManagementForm()));
            AddMenuButton(btnUsers, "Tài khoản / Nhân viên", 115, (s, e) => OpenChild(new UserManagementForm()));
            AddMenuButton(btnBooks, "Sách", 160, (s, e) => OpenChild(new BookManagementForm()));
            AddMenuButton(btnCategories, "Danh mục", 205, (s, e) => OpenChild(new CategoryManagementForm()));
            AddMenuButton(btnAuthors, "Tác giả", 250, (s, e) => OpenChild(new AuthorManagementForm()));
            AddMenuButton(btnPublishers, "Nhà xuất bản", 295, (s, e) => OpenChild(new PublisherManagementForm()));
            AddMenuButton(btnSuppliers, "Nhà cung cấp", 340, (s, e) => OpenChild(new SupplierManagementForm()));
            AddMenuButton(btnInventory, "Tồn kho", 385, (s, e) => OpenChild(new InventoryManagementForm()));
            AddMenuButton(btnSalesOrders, "Hóa đơn bán", 430, (s, e) => OpenChild(new SalesOrderManagementForm()));
            AddMenuButton(btnPurchaseReceipts, "Phiếu nhập", 475, (s, e) => OpenChild(new PurchaseReceiptManagementForm()));
            AddMenuButton(btnReturnReceipts, "Trả hàng", 520, (s, e) => OpenChild(new ReturnReceiptManagementForm()));
            AddMenuButton(btnRevenue, "Doanh thu", 565, (s, e) => OpenChild(new RevenueReportForm()));

            btnLogout.Text = "Đăng xuất";
            btnLogout.Width = 190;
            btnLogout.Height = 36;
            btnLogout.Left = 20;
            btnLogout.Top = 625;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.ForeColor = Color.White;
            btnLogout.BackColor = Color.FromArgb(214, 48, 49);
            btnLogout.Click += BtnLogout_Click;
            pnlMenu.Controls.Add(btnLogout);

            lblWelcome.AutoSize = true;
            lblWelcome.Left = 40;
            lblWelcome.Top = 35;
            lblWelcome.Font = new Font("Segoe UI", 18, FontStyle.Bold);

            lblRole.AutoSize = true;
            lblRole.Left = 42;
            lblRole.Top = 75;
            lblRole.Font = new Font("Segoe UI", 11, FontStyle.Regular);

            pnlContent.Controls.Add(lblWelcome);
            pnlContent.Controls.Add(lblRole);

            Controls.Add(pnlContent);
            Controls.Add(pnlMenu);
        }

        private void AddMenuButton(Button button, string text, int top, EventHandler click)
        {
            button.Text = text;
            button.Left = 20;
            button.Top = top;
            button.Width = 190;
            button.Height = 36;
            button.FlatStyle = FlatStyle.Flat;
            button.ForeColor = Color.White;
            button.BackColor = Color.FromArgb(57, 62, 70);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Click += click;
            pnlMenu.Controls.Add(button);
        }

        private void DashboardForm_Load(object? sender, EventArgs e)
        {
            if (!CurrentSession.IsLoggedIn)
            {
                MessageBox.Show("Bạn chưa đăng nhập.");
                Close();
                return;
            }

            lblWelcome.Text = "Xin chào, " + CurrentSession.FullName;
            lblRole.Text = "Quyền: " + CurrentSession.RoleName + " | Cửa hàng: " + (CurrentSession.StoreName ?? "Tất cả");
            SetupMenuByRole();
        }

        private void SetupMenuByRole()
        {
            if (CurrentSession.IsAdmin)
            {
                btnStores.Visible = true;
                btnUsers.Visible = true;
                btnBooks.Visible = true;
                btnCategories.Visible = true;
                btnAuthors.Visible = true;
                btnPublishers.Visible = true;
                btnSuppliers.Visible = true;
                btnInventory.Visible = true;
                btnSalesOrders.Visible = true;
                btnPurchaseReceipts.Visible = true;
                btnReturnReceipts.Visible = true;
                btnRevenue.Visible = true;
                return;
            }

            if (CurrentSession.IsStaff)
            {
                btnStores.Visible = false;
                btnUsers.Visible = false;
                btnCategories.Visible = false;
                btnAuthors.Visible = false;
                btnPublishers.Visible = false;
                btnSuppliers.Visible = false;
                btnPurchaseReceipts.Visible = false;
                btnRevenue.Visible = false;

                btnBooks.Visible = true;
                btnInventory.Visible = true;
                btnSalesOrders.Visible = true;
                btnReturnReceipts.Visible = true;
                return;
            }

            MessageBox.Show("Tài khoản chưa có quyền hợp lệ.");
            Application.Exit();
        }

        private void OpenChild(Form form)
        {
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog(this);
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            _authService.Logout();
            Hide();
            new LoginForm().Show();
        }

        private void DashboardForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (CurrentSession.IsLoggedIn)
            {
                Application.Exit();
            }
        }
    }
}
