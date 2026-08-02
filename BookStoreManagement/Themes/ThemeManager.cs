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
        public static Color DarkTextPrimary = Color.White;
        public static Color DarkTextSecondary = Color.FromArgb(160, 160, 165);
        public static Color DarkTextBoxBackground = Color.FromArgb(18, 19, 24);
        public static Color DarkTextBoxBorder = Color.FromArgb(40, 40, 45);
        public static Color DarkButtonFill = Color.FromArgb(180, 185, 255); // The Light Purple/Blue
        public static Color DarkButtonText = Color.Black;
        public static Color DarkHoverColor = Color.FromArgb(35, 35, 45);

        // --- Light Theme Colors ---
        public static Color LightBackground = Color.FromArgb(245, 246, 250);
        public static Color LightCardBackground = Color.White;
        public static Color LightSidebar = Color.FromArgb(220, 235, 250);
        public static Color LightTextPrimary = Color.FromArgb(30, 30, 30);
        public static Color LightTextSecondary = Color.Gray;
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

        public static void ApplyDataGridViewStyle(System.Windows.Forms.DataGridView dgv)
        {
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

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Background;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Background;
            dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            
            foreach (System.Windows.Forms.DataGridViewColumn col in dgv.Columns)
            {
                col.HeaderCell.Style.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
                col.HeaderCell.Style.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            }
        }
    }
}
