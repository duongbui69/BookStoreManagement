namespace BookStoreManagement
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ApplicationConfiguration.Initialize();
            Application.Run(new Forms.LoginForm());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Helpers.Logger.Error(e.Exception, "Unhandled UI Exception");
            MessageBox.Show("Đã xảy ra lỗi không mong muốn. Vui lòng kiểm tra file log hoặc liên hệ quản trị viên.\n\nChi tiết: " + e.Exception.Message, 
                "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Helpers.Logger.Error(ex, "Unhandled AppDomain Exception");
                MessageBox.Show("Đã xảy ra lỗi nghiêm trọng. Ứng dụng sẽ phải đóng.\n\nChi tiết: " + ex.Message, 
                    "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}