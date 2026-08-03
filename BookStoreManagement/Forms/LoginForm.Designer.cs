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
            components = new System.ComponentModel.Container();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges17 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges18 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges11 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges12 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges13 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges14 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges15 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges16 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            guna2BorderlessForm1 = new Guna.UI2.WinForms.Guna2BorderlessForm(components);
            pnlTitleBar = new Guna.UI2.WinForms.Guna2Panel();
            lblTitleIcon = new Label();
            lblTitleText = new Label();
            btnMinimize = new Guna.UI2.WinForms.Guna2ControlBox();
            btnMaximize = new Guna.UI2.WinForms.Guna2ControlBox();
            btnClose = new Guna.UI2.WinForms.Guna2ControlBox();
            loginCard = new Guna.UI2.WinForms.Guna2Panel();
            pnlHeaderIcon = new Guna.UI2.WinForms.Guna2Panel();
            lblHeaderIcon = new Label();
            lblBrand = new Label();
            lblSubtitle = new Label();
            lblUserLabel = new Label();
            txtUsername = new Guna.UI2.WinForms.Guna2TextBox();
            lblPassLabel = new Label();
            txtPassword = new Guna.UI2.WinForms.Guna2TextBox();
            chkRemember = new Guna.UI2.WinForms.Guna2CheckBox();
            lblForgot = new Label();
            lblError = new Guna.UI2.WinForms.Guna2HtmlLabel();
            btnLogin = new Guna.UI2.WinForms.Guna2Button();
            lblVersion = new Label();
            lblCopyright = new Label();
            pnlTitleBar.SuspendLayout();
            loginCard.SuspendLayout();
            pnlHeaderIcon.SuspendLayout();
            SuspendLayout();
            // 
            // guna2BorderlessForm1
            // 
            guna2BorderlessForm1.BorderRadius = 10;
            guna2BorderlessForm1.ContainerControl = this;
            guna2BorderlessForm1.DockIndicatorTransparencyValue = 0.6D;
            guna2BorderlessForm1.TransparentWhileDrag = true;
            // 
            // pnlTitleBar
            // 
            pnlTitleBar.Controls.Add(lblTitleIcon);
            pnlTitleBar.Controls.Add(lblTitleText);
            pnlTitleBar.Controls.Add(btnMinimize);
            pnlTitleBar.Controls.Add(btnMaximize);
            pnlTitleBar.Controls.Add(btnClose);
            pnlTitleBar.CustomBorderColor = Color.FromArgb(200, 200, 200);
            pnlTitleBar.CustomBorderThickness = new Padding(0, 0, 0, 1);
            pnlTitleBar.CustomizableEdges = customizableEdges17;
            pnlTitleBar.Dock = DockStyle.Top;
            pnlTitleBar.Location = new Point(0, 0);
            pnlTitleBar.Margin = new Padding(4, 3, 4, 3);
            pnlTitleBar.Name = "pnlTitleBar";
            pnlTitleBar.ShadowDecoration.CustomizableEdges = customizableEdges18;
            pnlTitleBar.Size = new Size(1120, 46);
            pnlTitleBar.TabIndex = 1;
            // 
            // lblTitleIcon
            // 
            lblTitleIcon.AutoSize = true;
            lblTitleIcon.Font = new Font("Segoe UI Emoji", 10.5F);
            lblTitleIcon.Location = new Point(14, 12);
            lblTitleIcon.Margin = new Padding(4, 0, 4, 0);
            lblTitleIcon.Name = "lblTitleIcon";
            lblTitleIcon.Size = new Size(28, 19);
            lblTitleIcon.TabIndex = 0;
            lblTitleIcon.Text = "📖";
            // 
            // lblTitleText
            // 
            lblTitleText.AutoSize = true;
            lblTitleText.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTitleText.Location = new Point(42, 13);
            lblTitleText.Margin = new Padding(4, 0, 4, 0);
            lblTitleText.Name = "lblTitleText";
            lblTitleText.Size = new Size(159, 17);
            lblTitleText.TabIndex = 1;
            lblTitleText.Text = "Hệ thống Bookstore ERP";
            // 
            // btnMinimize
            // 
            btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimize.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MinimizeBox;
            btnMinimize.CustomizableEdges = customizableEdges11;
            btnMinimize.FillColor = Color.Transparent;
            btnMinimize.IconColor = Color.Gray;
            btnMinimize.Location = new Point(1849, 0);
            btnMinimize.Margin = new Padding(4, 3, 4, 3);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.ShadowDecoration.CustomizableEdges = customizableEdges12;
            btnMinimize.Size = new Size(52, 46);
            btnMinimize.TabIndex = 2;
            // 
            // btnMaximize
            // 
            btnMaximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaximize.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MaximizeBox;
            btnMaximize.CustomizableEdges = customizableEdges13;
            btnMaximize.Enabled = false;
            btnMaximize.FillColor = Color.Transparent;
            btnMaximize.IconColor = Color.Gray;
            btnMaximize.Location = new Point(1902, 0);
            btnMaximize.Margin = new Padding(4, 3, 4, 3);
            btnMaximize.Name = "btnMaximize";
            btnMaximize.ShadowDecoration.CustomizableEdges = customizableEdges14;
            btnMaximize.Size = new Size(52, 46);
            btnMaximize.TabIndex = 3;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.CustomizableEdges = customizableEdges15;
            btnClose.FillColor = Color.Transparent;
            btnClose.IconColor = Color.Gray;
            btnClose.Location = new Point(1955, 0);
            btnClose.Margin = new Padding(4, 3, 4, 3);
            btnClose.Name = "btnClose";
            btnClose.ShadowDecoration.CustomizableEdges = customizableEdges16;
            btnClose.Size = new Size(52, 46);
            btnClose.TabIndex = 4;
            btnClose.Click += btnClose_Click;
            // 
            // loginCard
            // 
            loginCard.Anchor = AnchorStyles.None;
            loginCard.BackColor = Color.Transparent;
            loginCard.BorderRadius = 12;
            loginCard.Controls.Add(pnlHeaderIcon);
            loginCard.Controls.Add(lblBrand);
            loginCard.Controls.Add(lblSubtitle);
            loginCard.Controls.Add(lblUserLabel);
            loginCard.Controls.Add(txtUsername);
            loginCard.Controls.Add(lblPassLabel);
            loginCard.Controls.Add(txtPassword);
            loginCard.Controls.Add(chkRemember);
            loginCard.Controls.Add(lblForgot);
            loginCard.Controls.Add(lblError);
            loginCard.Controls.Add(btnLogin);
            loginCard.Controls.Add(lblVersion);
            loginCard.Controls.Add(lblCopyright);
            loginCard.CustomizableEdges = customizableEdges9;
            loginCard.Location = new Point(327, 115);
            loginCard.Margin = new Padding(4, 3, 4, 3);
            loginCard.Name = "loginCard";
            loginCard.ShadowDecoration.CustomizableEdges = customizableEdges10;
            loginCard.Size = new Size(467, 554);
            loginCard.TabIndex = 0;
            // 
            // pnlHeaderIcon
            // 
            pnlHeaderIcon.BorderRadius = 8;
            pnlHeaderIcon.Controls.Add(lblHeaderIcon);
            pnlHeaderIcon.CustomizableEdges = customizableEdges1;
            pnlHeaderIcon.Location = new Point(196, 46);
            pnlHeaderIcon.Margin = new Padding(4, 3, 4, 3);
            pnlHeaderIcon.Name = "pnlHeaderIcon";
            pnlHeaderIcon.ShadowDecoration.CustomizableEdges = customizableEdges2;
            pnlHeaderIcon.Size = new Size(75, 74);
            pnlHeaderIcon.TabIndex = 0;
            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.AutoSize = true;
            lblHeaderIcon.Font = new Font("Segoe UI Emoji", 20F);
            lblHeaderIcon.Location = new Point(16, 14);
            lblHeaderIcon.Margin = new Padding(4, 0, 4, 0);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new Size(49, 36);
            lblHeaderIcon.TabIndex = 0;
            lblHeaderIcon.Text = "📚";
            // 
            // lblBrand
            // 
            lblBrand.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblBrand.Location = new Point(0, 133);
            lblBrand.Margin = new Padding(4, 0, 4, 0);
            lblBrand.Name = "lblBrand";
            lblBrand.Size = new Size(467, 40);
            lblBrand.TabIndex = 1;
            lblBrand.Text = "Bookstore ERP";
            lblBrand.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.Location = new Point(0, 173);
            lblSubtitle.Margin = new Padding(4, 0, 4, 0);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(467, 23);
            lblSubtitle.TabIndex = 2;
            lblSubtitle.Text = "Đăng Nhập Hệ Thống Quản Lý";
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblUserLabel
            // 
            lblUserLabel.AutoSize = true;
            lblUserLabel.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblUserLabel.Location = new Point(47, 225);
            lblUserLabel.Margin = new Padding(4, 0, 4, 0);
            lblUserLabel.Name = "lblUserLabel";
            lblUserLabel.Size = new Size(99, 17);
            lblUserLabel.TabIndex = 3;
            lblUserLabel.Text = "Tên đăng nhập";
            // 
            // txtUsername
            // 
            txtUsername.BorderRadius = 8;
            txtUsername.Cursor = Cursors.IBeam;
            txtUsername.CustomizableEdges = customizableEdges3;
            txtUsername.DefaultText = "";
            txtUsername.Font = new Font("Segoe UI", 10.5F);
            txtUsername.Location = new Point(47, 254);
            txtUsername.Margin = new Padding(4, 3, 4, 3);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Nhập tên đăng nhập";
            txtUsername.SelectedText = "";
            txtUsername.ShadowDecoration.CustomizableEdges = customizableEdges4;
            txtUsername.Size = new Size(373, 48);
            txtUsername.TabIndex = 4;
            // 
            // lblPassLabel
            // 
            lblPassLabel.AutoSize = true;
            lblPassLabel.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblPassLabel.Location = new Point(47, 317);
            lblPassLabel.Margin = new Padding(4, 0, 4, 0);
            lblPassLabel.Name = "lblPassLabel";
            lblPassLabel.Size = new Size(66, 17);
            lblPassLabel.TabIndex = 5;
            lblPassLabel.Text = "Mật khẩu";
            // 
            // txtPassword
            // 
            txtPassword.BorderRadius = 8;
            txtPassword.Cursor = Cursors.IBeam;
            txtPassword.CustomizableEdges = customizableEdges5;
            txtPassword.DefaultText = "";
            txtPassword.Font = new Font("Segoe UI", 10.5F);
            txtPassword.IconRightCursor = Cursors.Hand;
            txtPassword.Location = new Point(47, 346);
            txtPassword.Margin = new Padding(4, 3, 4, 3);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.PlaceholderText = "Nhập mật khẩu";
            txtPassword.SelectedText = "";
            txtPassword.ShadowDecoration.CustomizableEdges = customizableEdges6;
            txtPassword.Size = new Size(373, 48);
            txtPassword.TabIndex = 6;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.IconRightClick += txtPassword_IconRightClick;
            // 
            // chkRemember
            // 
            chkRemember.AutoSize = true;
            chkRemember.CheckedState.BorderRadius = 2;
            chkRemember.CheckedState.BorderThickness = 0;
            chkRemember.Font = new Font("Segoe UI", 9.5F);
            chkRemember.Location = new Point(47, 410);
            chkRemember.Margin = new Padding(4, 3, 4, 3);
            chkRemember.Name = "chkRemember";
            chkRemember.Size = new Size(72, 21);
            chkRemember.TabIndex = 7;
            chkRemember.Text = "Ghi nhớ";
            chkRemember.UncheckedState.BorderRadius = 0;
            chkRemember.UncheckedState.BorderThickness = 0;
            // 
            // lblForgot
            // 
            lblForgot.AutoSize = true;
            lblForgot.Cursor = Cursors.Hand;
            lblForgot.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblForgot.Location = new Point(286, 412);
            lblForgot.Margin = new Padding(4, 0, 4, 0);
            lblForgot.Name = "lblForgot";
            lblForgot.Size = new Size(94, 15);
            lblForgot.TabIndex = 8;
            lblForgot.Text = "Quên mật khẩu?";
            // 
            // lblError
            // 
            lblError.AutoSize = false;
            lblError.BackColor = Color.Transparent;
            lblError.Font = new Font("Segoe UI", 9F);
            lblError.ForeColor = Color.Red;
            lblError.Location = new Point(47, 438);
            lblError.Margin = new Padding(4, 3, 4, 3);
            lblError.Name = "lblError";
            lblError.Size = new Size(373, 21);
            lblError.TabIndex = 9;
            lblError.TextAlignment = ContentAlignment.MiddleCenter;
            lblError.Visible = false;
            // 
            // btnLogin
            // 
            btnLogin.BorderRadius = 8;
            btnLogin.CustomizableEdges = customizableEdges7;
            btnLogin.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(47, 462);
            btnLogin.Margin = new Padding(4, 3, 4, 3);
            btnLogin.Name = "btnLogin";
            btnLogin.ShadowDecoration.CustomizableEdges = customizableEdges8;
            btnLogin.Size = new Size(373, 52);
            btnLogin.TabIndex = 10;
            btnLogin.Text = "ĐĂNG NHẬP ➔";
            btnLogin.Click += btnLogin_Click;
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Segoe UI", 8F);
            lblVersion.Location = new Point(23, 525);
            lblVersion.Margin = new Padding(4, 0, 4, 0);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(77, 13);
            lblVersion.TabIndex = 11;
            lblVersion.Text = "v2.4.0 (Stable)";
            // 
            // lblCopyright
            // 
            lblCopyright.AutoSize = true;
            lblCopyright.Font = new Font("Segoe UI", 8F);
            lblCopyright.Location = new Point(286, 525);
            lblCopyright.Margin = new Padding(4, 0, 4, 0);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.Size = new Size(121, 13);
            lblCopyright.TabIndex = 12;
            lblCopyright.Text = "© 2024 Bookstore ERP";
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1120, 785);
            Controls.Add(loginCard);
            Controls.Add(pnlTitleBar);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4, 3, 4, 3);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bookstore ERP - Đăng nhập";
            pnlTitleBar.ResumeLayout(false);
            pnlTitleBar.PerformLayout();
            loginCard.ResumeLayout(false);
            loginCard.PerformLayout();
            pnlHeaderIcon.ResumeLayout(false);
            pnlHeaderIcon.PerformLayout();
            ResumeLayout(false);
        }
    }
}
