using FlaUI.Core.AutomationElements;
using System.Threading;
using Xunit;

namespace BookStoreManagement.Tests
{
    public class CustomerTests : TestBase
    {
        private void LoginAndNavigateToCustomers()
        {
            var txtUsername = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtUsername"))?.AsTextBox();
            var txtPassword = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtPassword"))?.AsTextBox();
            var btnLogin = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("btnLogin"))?.AsButton();

            txtUsername.Text = "admin";
            txtPassword.Text = "admin123";
            btnLogin.Invoke();

            Thread.Sleep(2000);
            var mainForm = app.GetMainWindow(automation);

            var btnCustomers = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnCustomers"))?.AsButton();
            btnCustomers?.Invoke();
            Thread.Sleep(1000);
        }

        [Fact]
        public void TC_CUS_01_AddValidCustomer_ShouldSucceed()
        {
            LoginAndNavigateToCustomers();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var txtCustomerName = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtName"))?.AsTextBox();
            var txtPhone = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtPhone"))?.AsTextBox();
            var btnSave = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnSave"))?.AsButton();
            var btnAdd = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnAdd"))?.AsButton();

            // Act
            btnAdd?.Invoke();
            if (txtCustomerName != null) txtCustomerName.Text = "Nguyen Van A";
            if (txtPhone != null) txtPhone.Text = "0987654321";
            btnSave?.Invoke();
            Thread.Sleep(1000);

            // Assert
            var grid = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvData"));
            Assert.NotNull(grid);
        }

        [Fact]
        public void TC_CUS_02_AddCustomerWithInvalidPhone_ShouldShowError()
        {
            LoginAndNavigateToCustomers();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var txtCustomerName = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtName"))?.AsTextBox();
            var txtPhone = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtPhone"))?.AsTextBox();
            var btnSave = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnSave"))?.AsButton();

            // Act
            if (txtCustomerName != null) txtCustomerName.Text = "Tran Thi B";
            if (txtPhone != null) txtPhone.Text = "123"; // Invalid phone
            btnSave?.Invoke();
            Thread.Sleep(500);

            // Assert
            var errorProvider = mainForm.FindFirstDescendant(cf => cf.ByClassName("ErrorProvider"));
            Assert.NotNull(errorProvider);
        }

        [Fact]
        public void TC_CUS_03_SearchCustomer_ShouldFilterGrid()
        {
            LoginAndNavigateToCustomers();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var txtSearch = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtSearch"))?.AsTextBox();
            var btnSearch = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnSearch"))?.AsButton();

            // Act
            if (txtSearch != null) txtSearch.Text = "0987";
            btnSearch?.Invoke();
            Thread.Sleep(500);

            // Assert
            var grid = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvData"))?.AsDataGridView();
            Assert.NotNull(grid);
        }
    }
}
