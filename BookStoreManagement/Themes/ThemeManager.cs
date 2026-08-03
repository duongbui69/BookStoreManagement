using System;
using System.Drawing;

namespace BookStoreManagement.Themes
{
    public static class ThemeManager
    {
        public static bool IsDarkMode { get; set; } = false;

        public static event EventHandler ThemeChanged;

        public static void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        // --- Dark Theme Colors (Bookshelf OS style) ---
        public static Color DarkBackground = Color.FromArgb(16, 17, 22);
        public static Color DarkCardBackground = Color.FromArgb(24, 25, 32);
        public static Color DarkSidebar = Color.FromArgb(10, 35, 55);
        public static Color DarkTextPrimary = Color.FromArgb(245, 245, 250);
        public static Color DarkTextSecondary = Color.FromArgb(170, 170, 175);
        public static Color DarkTextBoxBackground = Color.FromArgb(18, 19, 24);
        public static Color DarkTextBoxBorder = Color.FromArgb(40, 40, 45);
        public static Color DarkButtonFill = Color.FromArgb(180, 185, 255); // The Light Purple/Blue
        public static Color DarkButtonText = Color.Black;
        public static Color DarkHoverColor = Color.FromArgb(35, 35, 45);

        // --- Light Theme Colors ---
        public static Color LightBackground = Color.FromArgb(245, 246, 250);
        public static Color LightCardBackground = Color.White;
        public static Color LightSidebar = Color.FromArgb(220, 235, 250);
        public static Color LightTextPrimary = Color.FromArgb(20, 20, 20);
        public static Color LightTextSecondary = Color.FromArgb(100, 100, 100);
        public static Color LightTextBoxBackground = Color.FromArgb(250, 250, 253);
        public static Color LightTextBoxBorder = Color.FromArgb(220, 220, 225);
        public static Color LightButtonFill = Color.FromArgb(41, 128, 185);
        public static Color LightButtonText = Color.White;
        public static Color LightHoverColor = Color.FromArgb(235, 235, 240);

        // Current getters based on state
        public static Color Background => IsDarkMode ? DarkBackground : LightBackground;
        public static Color CardBackground => IsDarkMode ? DarkCardBackground : LightCardBackground;
        public static Color Sidebar => IsDarkMode ? DarkSidebar : LightSidebar;
        public static Color TextPrimary => IsDarkMode ? DarkTextPrimary : LightTextPrimary;
        public static Color TextSecondary => IsDarkMode ? DarkTextSecondary : LightTextSecondary;
        public static Color TextBoxBackground => IsDarkMode ? DarkTextBoxBackground : LightTextBoxBackground;
        public static Color TextBoxBorder => IsDarkMode ? DarkTextBoxBorder : LightTextBoxBorder;
        public static Color ButtonFill => IsDarkMode ? DarkButtonFill : LightButtonFill;
        public static Color ButtonText => IsDarkMode ? DarkButtonText : LightButtonText;
        public static Color HoverColor => IsDarkMode ? DarkHoverColor : LightHoverColor;

                public static Font FontTitle => new Font("Inter", 18F, FontStyle.Bold);
        public static Font FontSubtitle => new Font("Inter", 12F, FontStyle.Regular);
        public static Font FontHeader => new Font("Inter", 10F, FontStyle.Bold);
        public static Font FontBody => new Font("Inter", 10F, FontStyle.Regular);
        public static Font FontButton => new Font("Inter", 9F, FontStyle.Bold);
        public static Font FontSmall => new Font("Inter", 8.5F, FontStyle.Regular);

        public static void ApplyTypography(System.Windows.Forms.Control parent)
        {
            if (parent == null) return;

            foreach (System.Windows.Forms.Control c in parent.Controls)
            {
                if (c is System.Windows.Forms.Label lbl)
                {
                    if (lbl.Name.StartsWith("lblTitle") || (lbl.Tag != null && lbl.Tag.ToString() == "Title"))
                    {
                        lbl.Font = FontTitle;
                    }
                    else if (lbl.Name.StartsWith("lblSubTitle") || (lbl.Tag != null && lbl.Tag.ToString() == "Subtitle"))
                    {
                        lbl.Font = FontSubtitle;
                    }
                    else
                    {
                        if (lbl.Font.Size >= 14) lbl.Font = FontTitle;
                        else if (lbl.Font.Size >= 11) lbl.Font = FontSubtitle;
                        else if (lbl.Font.Bold) lbl.Font = FontHeader;
                        else lbl.Font = FontBody;
                    }
                }
                else if (c is Guna.UI2.WinForms.Guna2Button btn)
                {
                    btn.Font = FontButton;
                }
                else if (c is System.Windows.Forms.Button sysBtn)
                {
                    sysBtn.Font = FontButton;
                }
                else if (c is Guna.UI2.WinForms.Guna2TextBox tb)
                {
                    tb.Font = FontBody;
                }
                else if (c is Guna.UI2.WinForms.Guna2ComboBox cb)
                {
                    cb.Font = FontBody;
                }
                
                if (c.HasChildren)
                {
                    ApplyTypography(c);
                }
            }
        }

        public static void ApplyDataGridViewStyle(System.Windows.Forms.DataGridView dgv)
        {
            try {
                if (dgv == null) return;
                
                dgv.EnableHeadersVisualStyles = false;
                dgv.RowHeadersVisible = false;
                dgv.AllowUserToAddRows = false;
                dgv.AllowUserToResizeRows = false;
                dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                dgv.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
                
                if (dgv.RowTemplate.Height < 50) dgv.RowTemplate.Height = 50;

                dgv.BackgroundColor = CardBackground;
                dgv.GridColor = TextBoxBorder;
                
                dgv.DefaultCellStyle.BackColor = CardBackground;
                dgv.DefaultCellStyle.ForeColor = TextPrimary;
                dgv.DefaultCellStyle.SelectionBackColor = ButtonFill; 
                dgv.DefaultCellStyle.SelectionForeColor = ButtonText;

                // Save columns if it's a Guna2DataGridView to prevent them from being cleared
                System.Windows.Forms.DataGridViewColumn[] savedCols = null;
                if (dgv is Guna.UI2.WinForms.Guna2DataGridView && dgv.Columns.Count > 0)
                {
                    savedCols = new System.Windows.Forms.DataGridViewColumn[dgv.Columns.Count];
                    dgv.Columns.CopyTo(savedCols, 0);
                }

                if (dgv is Guna.UI2.WinForms.Guna2DataGridView gunaDgv)
                {
                    gunaDgv.ThemeStyle.AlternatingRowsStyle.BackColor = CardBackground;
                    gunaDgv.ThemeStyle.AlternatingRowsStyle.ForeColor = TextPrimary;
                    gunaDgv.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = ButtonFill;
                    gunaDgv.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = ButtonText;
                    
                    gunaDgv.ThemeStyle.BackColor = CardBackground;
                    gunaDgv.ThemeStyle.GridColor = TextBoxBorder;
                    
                    gunaDgv.ThemeStyle.HeaderStyle.BackColor = Background;
                    gunaDgv.ThemeStyle.HeaderStyle.ForeColor = TextSecondary;
                    
                    gunaDgv.ThemeStyle.RowsStyle.BackColor = CardBackground;
                    gunaDgv.ThemeStyle.RowsStyle.ForeColor = TextPrimary;
                    gunaDgv.ThemeStyle.RowsStyle.SelectionBackColor = ButtonFill;
                    gunaDgv.ThemeStyle.RowsStyle.SelectionForeColor = ButtonText;
                }

                // Restore columns if they were cleared
                if (savedCols != null && dgv.Columns.Count == 0)
                {
                    dgv.Columns.AddRange(savedCols);
                }

                dgv.ColumnHeadersDefaultCellStyle.BackColor = Background;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Background;
                dgv.ColumnHeadersDefaultCellStyle.Font = FontHeader;
                dgv.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
                
                foreach (System.Windows.Forms.DataGridViewColumn col in dgv.Columns)
                {
                    col.HeaderCell.Style.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
                    col.HeaderCell.Style.Font = FontHeader;
                }
            } catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show("Theme Error: " + ex.Message);
            }
        }
    }
}
