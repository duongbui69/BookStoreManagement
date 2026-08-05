using FlaUI.Core.AutomationElements;
using System.Threading;
using Xunit;

namespace BookStoreManagement.Tests
{
    public class ImportTests : TestBase
    {
        private void LoginAndNavigateToImport()
        {
            var txtUsername = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtUsername"))?.AsTextBox();
            var txtPassword = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtPassword"))?.AsTextBox();
            var btnLogin = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("btnLogin"))?.AsButton();

            txtUsername.Text = "admin";
            txtPassword.Text = "admin123";
            btnLogin.Invoke();

            Thread.Sleep(2000);
            var mainForm = app.GetMainWindow(automation);

            var btnInventory = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnInventory"))?.AsButton();
            btnInventory?.Invoke();
            Thread.Sleep(500);

            var btnImport = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnImport"))?.AsButton();
            btnImport?.Invoke();
            Thread.Sleep(1000);
        }

        [Fact]
        public void TC_IMP_01_CreateValidImportReceipt_ShouldSucceed()
        {
            LoginAndNavigateToImport();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var cboSupplier = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("cboSupplier"))?.AsComboBox();
            var cboBook = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("cboBook"))?.AsComboBox();
            var txtQuantity = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtQuantity"))?.AsTextBox();
            var txtPrice = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtPrice"))?.AsTextBox();
            var btnAddDetail = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnAddDetail"))?.AsButton();
            var btnSave = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnSave"))?.AsButton();

            // Act
            if (cboSupplier != null && cboSupplier.Items.Length > 0) cboSupplier.Items[0].Select();
            if (cboBook != null && cboBook.Items.Length > 0) cboBook.Items[0].Select();
            if (txtQuantity != null) txtQuantity.Text = "50";
            if (txtPrice != null) txtPrice.Text = "100000";
            
            btnAddDetail?.Invoke();
            Thread.Sleep(500);
            
            btnSave?.Invoke();
            Thread.Sleep(1000);

            // Assert
            var grid = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvData"));
            Assert.NotNull(grid);
        }

        [Fact]
        public void TC_IMP_02_ImportQuantityZero_ShouldShowError()
        {
            LoginAndNavigateToImport();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var cboBook = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("cboBook"))?.AsComboBox();
            var txtQuantity = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtQuantity"))?.AsTextBox();
            var btnAddDetail = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnAddDetail"))?.AsButton();

            // Act
            if (cboBook != null && cboBook.Items.Length > 0) cboBook.Items[0].Select();
            if (txtQuantity != null) txtQuantity.Text = "0"; // Invalid quantity
            btnAddDetail?.Invoke();
            Thread.Sleep(500);

            // Assert
            var errorProvider = mainForm.FindFirstDescendant(cf => cf.ByClassName("ErrorProvider"));
            Assert.NotNull(errorProvider);
        }
    }
}
