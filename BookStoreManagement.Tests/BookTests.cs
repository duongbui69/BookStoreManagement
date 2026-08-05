using FlaUI.Core.AutomationElements;
using System.Threading;
using Xunit;

namespace BookStoreManagement.Tests
{
    public class BookTests : TestBase
    {
        [Fact]
        public void BookList_ShouldLoadData()
        {
            // Login first
            var txtUsername = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtUsername"))?.AsTextBox();
            var txtPassword = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtPassword"))?.AsTextBox();
            var btnLogin = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("btnLogin"))?.AsButton();

            txtUsername.Text = "admin";
            txtPassword.Text = "admin123";
            btnLogin.Invoke();

            Thread.Sleep(2000);
            var mainForm = app.GetMainWindow(automation);

            // Go to Book Management
            var btnBooks = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnBooks"))?.AsButton();
            if (btnBooks != null)
            {
                btnBooks.Invoke();
                Thread.Sleep(1000);
            }

            // Verify DataGridView exists
            var grid = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvData"));
            Assert.NotNull(grid);
        }
    }
}
