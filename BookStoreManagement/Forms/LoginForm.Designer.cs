namespace BookStoreManagement.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnExit;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblUsername = new Label();
            lblPassword = new Label();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            btnExit = new Button();

            SuspendLayout();

            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.Location = new Point(205, 35);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(244, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Book Store Login";

            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(140, 115);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(80, 15);
            lblUsername.TabIndex = 1;
            lblUsername.Text = "Tên tài khoản";

            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(250, 112);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(220, 23);
            txtUsername.TabIndex = 2;

            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(140, 160);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(57, 15);
            lblPassword.TabIndex = 3;
            lblPassword.Text = "Mật khẩu";

            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(250, 157);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(220, 23);
            txtPassword.TabIndex = 4;
            txtPassword.UseSystemPasswordChar = true;

            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(250, 215);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(95, 30);
            btnLogin.TabIndex = 5;
            btnLogin.Text = "Đăng nhập";
            btnLogin.UseVisualStyleBackColor = true;

            // 
            // btnExit
            // 
            btnExit.Location = new Point(375, 215);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(95, 30);
            btnExit.TabIndex = 6;
            btnExit.Text = "Thoát";
            btnExit.UseVisualStyleBackColor = true;

            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(620, 310);
            Controls.Add(lblTitle);
            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(btnExit);
            Name = "LoginForm";
            Text = "Đăng nhập hệ thống";

            ResumeLayout(false);
            PerformLayout();
        }
    }
}