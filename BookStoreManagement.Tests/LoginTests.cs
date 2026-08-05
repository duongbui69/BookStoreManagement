using FlaUI.Core.AutomationElements;
using System.Threading;
using Xunit;

namespace BookStoreManagement.Tests
{
    public class LoginTests : TestBase
    {
        [Fact]
        public void Login_WithValidAdminCredentials_ShouldOpenMainForm()
        {
            // Arrange
            var txtUsername = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtUsername"))?.AsTextBox();
            var txtPassword = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtPassword"))?.AsTextBox();
            var btnLogin = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("btnLogin"))?.AsButton();

            Assert.NotNull(txtUsername);
            Assert.NotNull(txtPassword);
            Assert.NotNull(btnLogin);

            // Act
            txtUsername.Text = "admin";
            Thread.Sleep(1000);
            txtPassword.Text = "123456";
            Thread.Sleep(1000);
            btnLogin.Click();

            // Assert
            // Wait for main window to appear
            Thread.Sleep(3000); 
            var newMainWindow = app.GetAllTopLevelWindows(automation).FirstOrDefault(w => w.Name.Contains("Nhà Sách")) ?? app.GetMainWindow(automation);
            Assert.Contains("Nhà Sách", newMainWindow.Name);
        }

        [Fact]
        public void Login_WithInvalidCredentials_ShouldShowError()
        {
            // Arrange
            var txtUsername = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtUsername"))?.AsTextBox();
            var txtPassword = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtPassword"))?.AsTextBox();
            var btnLogin = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("btnLogin"))?.AsButton();

            // Act
            txtUsername.Text = "admin";
            Thread.Sleep(1000);
            txtPassword.Text = "wrongpassword";
            Thread.Sleep(1000);
            btnLogin.Click();

            // Assert
            Thread.Sleep(1000);
            var errorLabel = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("lblError"))?.AsLabel();
            Assert.NotNull(errorLabel);
            Assert.False(string.IsNullOrEmpty(errorLabel.Name)); // In FlaUI, Text maps to Name for labels
        }
    }
}
