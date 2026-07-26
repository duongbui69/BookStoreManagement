using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public class BarcodeScanDialog : Form
    {
        private Guna2Panel pnlBackground;
        private Guna2HtmlLabel lblTitle;
        private Guna2HtmlLabel lblDescription;
        private Guna2TextBox txtBarcode;
        private Guna2Button btnConfirm;
        private Guna2Button btnCancel;

        public string ScannedBarcode { get; private set; } = string.Empty;

        public BarcodeScanDialog()
        {
            InitializeComponent();
            ApplyTheme();
            this.ActiveControl = txtBarcode;
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            pnlBackground.FillColor = ThemeManager.CardBackground;
            
            lblTitle.ForeColor = ThemeManager.ButtonFill;
            lblDescription.ForeColor = ThemeManager.TextSecondary;
            
            txtBarcode.FillColor = ThemeManager.TextBoxBackground;
            txtBarcode.ForeColor = ThemeManager.TextPrimary;
            txtBarcode.BorderColor = ThemeManager.TextBoxBorder;
            txtBarcode.FocusedState.BorderColor = ThemeManager.ButtonFill;
            
            btnConfirm.FillColor = ThemeManager.ButtonFill;
            btnConfirm.ForeColor = ThemeManager.ButtonText;
            
            btnCancel.FillColor = ThemeManager.CardBackground;
            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.BorderColor = ThemeManager.TextBoxBorder;
        }

        private void InitializeComponent()
        {
            this.pnlBackground = new Guna.UI2.WinForms.Guna2Panel();
            this.lblTitle = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblDescription = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.txtBarcode = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnConfirm = new Guna.UI2.WinForms.Guna2Button();
            this.btnCancel = new Guna.UI2.WinForms.Guna2Button();
            this.pnlBackground.SuspendLayout();
            this.SuspendLayout();
            
            // pnlBackground
            this.pnlBackground.BorderRadius = 12;
            this.pnlBackground.Controls.Add(this.btnCancel);
            this.pnlBackground.Controls.Add(this.btnConfirm);
            this.pnlBackground.Controls.Add(this.txtBarcode);
            this.pnlBackground.Controls.Add(this.lblDescription);
            this.pnlBackground.Controls.Add(this.lblTitle);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Padding = new System.Windows.Forms.Padding(24);
            this.pnlBackground.Size = new System.Drawing.Size(400, 220);
            this.pnlBackground.TabIndex = 0;
            
            // lblTitle
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Inter", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(24, 24);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(161, 27);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Quét Mã vạch";
            
            // lblDescription
            this.lblDescription.BackColor = System.Drawing.Color.Transparent;
            this.lblDescription.Font = new System.Drawing.Font("Inter", 10F);
            this.lblDescription.Location = new System.Drawing.Point(24, 60);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(273, 18);
            this.lblDescription.TabIndex = 1;
            this.lblDescription.Text = "Sử dụng máy quét hoặc nhập mã hóa đơn/sản phẩm.";
            
            // txtBarcode
            this.txtBarcode.BorderRadius = 4;
            this.txtBarcode.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtBarcode.Font = new System.Drawing.Font("Inter", 12F);
            this.txtBarcode.Location = new System.Drawing.Point(24, 94);
            this.txtBarcode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtBarcode.Name = "txtBarcode";
            this.txtBarcode.PasswordChar = '\0';
            this.txtBarcode.PlaceholderText = "Vd: INV-2023...";
            this.txtBarcode.SelectedText = "";
            this.txtBarcode.Size = new System.Drawing.Size(352, 44);
            this.txtBarcode.TabIndex = 2;
            this.txtBarcode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtBarcode_KeyDown);
            
            // btnConfirm
            this.btnConfirm.BorderRadius = 4;
            this.btnConfirm.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold);
            this.btnConfirm.Location = new System.Drawing.Point(246, 154);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(130, 42);
            this.btnConfirm.TabIndex = 3;
            this.btnConfirm.Text = "Xác nhận";
            this.btnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            
            // btnCancel
            this.btnCancel.BorderRadius = 4;
            this.btnCancel.BorderThickness = 1;
            this.btnCancel.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(110, 154);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(130, 42);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Hủy bỏ";
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            
            // BarcodeScanDialog
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 220);
            this.Controls.Add(this.pnlBackground);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "BarcodeScanDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quét Mã vạch";
            this.pnlBackground.ResumeLayout(false);
            this.pnlBackground.PerformLayout();
            this.ResumeLayout(false);
        }

        private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevent ding sound
                ConfirmScan();
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            ConfirmScan();
        }

        private void ConfirmScan()
        {
            ScannedBarcode = txtBarcode.Text.Trim();
            if (!string.IsNullOrEmpty(ScannedBarcode))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Vui lòng nhập mã vạch.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
