namespace BookStoreManagement.UserControls
{
    partial class DashboardControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BookStoreManagement.Themes.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // All components are initialized manually in DashboardControl.cs
        }
    }
}
