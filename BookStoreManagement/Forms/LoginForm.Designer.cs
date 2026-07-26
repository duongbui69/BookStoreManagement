namespace BookStoreManagement.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private Guna.UI2.WinForms.Guna2BorderlessForm guna2BorderlessForm1;
        
        // Custom Title Bar
        private Guna.UI2.WinForms.Guna2Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitleIcon;
        private System.Windows.Forms.Label lblTitleText;
        private Guna.UI2.WinForms.Guna2ControlBox btnMinimize;
        private Guna.UI2.WinForms.Guna2ControlBox btnMaximize;
        private Guna.UI2.WinForms.Guna2ControlBox btnClose;

        // Login Card
        private Guna.UI2.WinForms.Guna2Panel loginCard;
        
        // Header Section
        private Guna.UI2.WinForms.Guna2Panel pnlHeaderIcon;
        private System.Windows.Forms.Label lblHeaderIcon;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Label lblSubtitle;

        // Form Inputs
        private System.Windows.Forms.Label lblUserLabel;
        private Guna.UI2.WinForms.Guna2TextBox txtUsername;
        private System.Windows.Forms.Label lblPassLabel;
        private Guna.UI2.WinForms.Guna2TextBox txtPassword;

        // Options
        private Guna.UI2.WinForms.Guna2CheckBox chkRemember;
        private System.Windows.Forms.Label lblForgot;

        // Action Button
        private Guna.UI2.WinForms.Guna2Button btnLogin;
        
        // Error Message
        private Guna.UI2.WinForms.Guna2HtmlLabel lblError;

        // Footer
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblCopyright;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.guna2BorderlessForm1 = new Guna.UI2.WinForms.Guna2BorderlessForm(this.components);
            
            // Title Bar
            this.pnlTitleBar = new Guna.UI2.WinForms.Guna2Panel();
            this.lblTitleIcon = new System.Windows.Forms.Label();
            this.lblTitleText = new System.Windows.Forms.Label();
            this.btnMinimize = new Guna.UI2.WinForms.Guna2ControlBox();
            this.btnMaximize = new Guna.UI2.WinForms.Guna2ControlBox();
            this.btnClose = new Guna.UI2.WinForms.Guna2ControlBox();

            // Login Card
            this.loginCard = new Guna.UI2.WinForms.Guna2Panel();
            
            // Header
            this.pnlHeaderIcon = new Guna.UI2.WinForms.Guna2Panel();
            this.lblHeaderIcon = new System.Windows.Forms.Label();
            this.lblBrand = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            
            // Inputs
            this.lblUserLabel = new System.Windows.Forms.Label();
            this.txtUsername = new Guna.UI2.WinForms.Guna2TextBox();
            this.lblPassLabel = new System.Windows.Forms.Label();
            this.txtPassword = new Guna.UI2.WinForms.Guna2TextBox();
            
            // Options
            this.chkRemember = new Guna.UI2.WinForms.Guna2CheckBox();
            this.lblForgot = new System.Windows.Forms.Label();
            
            // Actions
            this.lblError = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.btnLogin = new Guna.UI2.WinForms.Guna2Button();
            
            // Footer
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();

            this.pnlTitleBar.SuspendLayout();
            this.loginCard.SuspendLayout();
            this.pnlHeaderIcon.SuspendLayout();
            this.SuspendLayout();

            // 
            // guna2BorderlessForm1
            // 
            this.guna2BorderlessForm1.BorderRadius = 10;
            this.guna2BorderlessForm1.ContainerControl = this;
            this.guna2BorderlessForm1.DockIndicatorTransparencyValue = 0.6D;
            this.guna2BorderlessForm1.TransparentWhileDrag = true;

            // 
            // pnlTitleBar
            // 
            this.pnlTitleBar.Controls.Add(this.lblTitleIcon);
            this.pnlTitleBar.Controls.Add(this.lblTitleText);
            this.pnlTitleBar.Controls.Add(this.btnMinimize);
            this.pnlTitleBar.Controls.Add(this.btnMaximize);
            this.pnlTitleBar.Controls.Add(this.btnClose);
            this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Height = 40;
            this.pnlTitleBar.CustomBorderThickness = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.pnlTitleBar.CustomBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            
            // 
            // lblTitleIcon
            // 
            this.lblTitleIcon.AutoSize = true;
            this.lblTitleIcon.Font = new System.Drawing.Font("Segoe UI Emoji", 10.5F);
            this.lblTitleIcon.Location = new System.Drawing.Point(12, 10);
            this.lblTitleIcon.Text = "📖";
            
            // 
            // lblTitleText
            // 
            this.lblTitleText.AutoSize = true;
            this.lblTitleText.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblTitleText.Location = new System.Drawing.Point(36, 11);
            this.lblTitleText.Text = "Hệ thống Bookstore ERP";
            
            // 
            // btnMinimize
            // 
            this.btnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimize.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MinimizeBox;
            this.btnMinimize.FillColor = System.Drawing.Color.Transparent;
            this.btnMinimize.IconColor = System.Drawing.Color.Gray;
            this.btnMinimize.Location = new System.Drawing.Point(825, 0);
            this.btnMinimize.Size = new System.Drawing.Size(45, 40);

            // 
            // btnMaximize
            // 
            this.btnMaximize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMaximize.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MaximizeBox;
            this.btnMaximize.Enabled = false;
            this.btnMaximize.FillColor = System.Drawing.Color.Transparent;
            this.btnMaximize.IconColor = System.Drawing.Color.Gray;
            this.btnMaximize.Location = new System.Drawing.Point(870, 0);
            this.btnMaximize.Size = new System.Drawing.Size(45, 40);

            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.FillColor = System.Drawing.Color.Transparent;
            this.btnClose.IconColor = System.Drawing.Color.Gray;
            this.btnClose.Location = new System.Drawing.Point(915, 0);
            this.btnClose.Size = new System.Drawing.Size(45, 40);
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // 
            // loginCard
            // 
            this.loginCard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.loginCard.BackColor = System.Drawing.Color.Transparent;
            this.loginCard.BorderRadius = 12;
            this.loginCard.Controls.Add(this.pnlHeaderIcon);
            this.loginCard.Controls.Add(this.lblBrand);
            this.loginCard.Controls.Add(this.lblSubtitle);
            this.loginCard.Controls.Add(this.lblUserLabel);
            this.loginCard.Controls.Add(this.txtUsername);
            this.loginCard.Controls.Add(this.lblPassLabel);
            this.loginCard.Controls.Add(this.txtPassword);
            this.loginCard.Controls.Add(this.chkRemember);
            this.loginCard.Controls.Add(this.lblForgot);
            this.loginCard.Controls.Add(this.lblError);
            this.loginCard.Controls.Add(this.btnLogin);
            this.loginCard.Controls.Add(this.lblVersion);
            this.loginCard.Controls.Add(this.lblCopyright);
            this.loginCard.Location = new System.Drawing.Point(280, 100);
            this.loginCard.Size = new System.Drawing.Size(400, 480);
            
            // 
            // pnlHeaderIcon
            // 
            this.pnlHeaderIcon.BorderRadius = 8;
            this.pnlHeaderIcon.Controls.Add(this.lblHeaderIcon);
            this.pnlHeaderIcon.Location = new System.Drawing.Point(168, 40);
            this.pnlHeaderIcon.Size = new System.Drawing.Size(64, 64);
            
            // 
            // lblHeaderIcon
            // 
            this.lblHeaderIcon.AutoSize = true;
            this.lblHeaderIcon.Font = new System.Drawing.Font("Segoe UI Emoji", 20F);
            this.lblHeaderIcon.Location = new System.Drawing.Point(14, 12);
            this.lblHeaderIcon.Text = "📚";
            
            // 
            // lblBrand
            // 
            this.lblBrand.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblBrand.Location = new System.Drawing.Point(0, 115);
            this.lblBrand.Size = new System.Drawing.Size(400, 35);
            this.lblBrand.Text = "Bookstore ERP";
            this.lblBrand.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSubtitle.Location = new System.Drawing.Point(0, 150);
            this.lblSubtitle.Size = new System.Drawing.Size(400, 20);
            this.lblSubtitle.Text = "Đăng Nhập Hệ Thống Quản Lý";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblUserLabel
            // 
            this.lblUserLabel.AutoSize = true;
            this.lblUserLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblUserLabel.Location = new System.Drawing.Point(40, 195);
            this.lblUserLabel.Text = "Tên đăng nhập";
            
            // 
            // txtUsername
            // 
            this.txtUsername.BorderRadius = 8;
            this.txtUsername.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtUsername.Location = new System.Drawing.Point(40, 220);
            this.txtUsername.Size = new System.Drawing.Size(320, 42);
            this.txtUsername.PlaceholderText = "admin";
            
            // 
            // lblPassLabel
            // 
            this.lblPassLabel.AutoSize = true;
            this.lblPassLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblPassLabel.Location = new System.Drawing.Point(40, 275);
            this.lblPassLabel.Text = "Mật khẩu";
            
            // 
            // txtPassword
            // 
            this.txtPassword.BorderRadius = 8;
            this.txtPassword.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtPassword.Location = new System.Drawing.Point(40, 300);
            this.txtPassword.Size = new System.Drawing.Size(320, 42);
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.PlaceholderText = "••••••••";
            this.txtPassword.IconRightCursor = System.Windows.Forms.Cursors.Hand;
            this.txtPassword.IconRightClick += new System.EventHandler(this.txtPassword_IconRightClick);

            // 
            // chkRemember
            // 
            this.chkRemember.AutoSize = true;
            this.chkRemember.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkRemember.Location = new System.Drawing.Point(40, 355);
            this.chkRemember.Text = "Ghi nhớ";
            this.chkRemember.CheckedState.BorderRadius = 2;
            
            // 
            // lblForgot
            // 
            this.lblForgot.AutoSize = true;
            this.lblForgot.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblForgot.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblForgot.Location = new System.Drawing.Point(245, 357);
            this.lblForgot.Text = "Quên mật khẩu?";
            
            // 
            // lblError
            // 
            this.lblError.AutoSize = false;
            this.lblError.BackColor = System.Drawing.Color.Transparent;
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblError.ForeColor = System.Drawing.Color.Red;
            this.lblError.Location = new System.Drawing.Point(40, 380);
            this.lblError.Size = new System.Drawing.Size(320, 18);
            this.lblError.Text = "";
            this.lblError.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblError.Visible = false;

            // 
            // btnLogin
            // 
            this.btnLogin.BorderRadius = 8;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnLogin.Location = new System.Drawing.Point(40, 400);
            this.btnLogin.Size = new System.Drawing.Size(320, 45);
            this.btnLogin.Text = "LOGIN ➔";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblVersion.Location = new System.Drawing.Point(20, 455);
            this.lblVersion.Text = "v2.4.0 (Stable)";
            
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCopyright.Location = new System.Drawing.Point(245, 455);
            this.lblCopyright.Text = "© 2024 Bookstore ERP";
            
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 680);
            this.Controls.Add(this.loginCard);
            this.Controls.Add(this.pnlTitleBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bookstore ERP - Đăng nhập";
            this.pnlTitleBar.ResumeLayout(false);
            this.pnlTitleBar.PerformLayout();
            this.loginCard.ResumeLayout(false);
            this.loginCard.PerformLayout();
            this.pnlHeaderIcon.ResumeLayout(false);
            this.pnlHeaderIcon.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
