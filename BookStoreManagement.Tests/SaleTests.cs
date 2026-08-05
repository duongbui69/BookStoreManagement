using FlaUI.Core.AutomationElements;
using System.Threading;
using Xunit;

namespace BookStoreManagement.Tests
{
    public class SaleTests : TestBase
    {
        private void LoginAndNavigateToPOS()
        {
            var txtUsername = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtUsername"))?.AsTextBox();
            var txtPassword = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtPassword"))?.AsTextBox();
            var btnLogin = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("btnLogin"))?.AsButton();

            txtUsername.Text = "staff"; // Using staff account for POS
            txtPassword.Text = "staff123";
            btnLogin.Invoke();

            Thread.Sleep(2000);
            var mainForm = app.GetMainWindow(automation);

            var btnPOS = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnPOS"))?.AsButton();
            btnPOS?.Invoke();
            Thread.Sleep(1000);
        }

        [Fact]
        public void TC_SALE_01_CreateInvoice_ShouldSucceed()
        {
            LoginAndNavigateToPOS();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var gridBooks = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvBooks"))?.AsDataGridView();
            var btnCheckout = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnCheckout"))?.AsButton();
            var txtCustomerPhone = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtCustomerPhone"))?.AsTextBox();
            var btnFindCustomer = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnFindCustomer"))?.AsButton();

            // Act
            // 1. Add book to cart by double clicking first row in book grid
            gridBooks?.Rows[0]?.Cells[0]?.DoubleClick();
            Thread.Sleep(500);

            // 2. Select customer
            if (txtCustomerPhone != null) txtCustomerPhone.Text = "0987654321";
            btnFindCustomer?.Invoke();
            Thread.Sleep(500);

            // 3. Checkout
            btnCheckout?.Invoke();
            Thread.Sleep(1000);

            // Assert
            // Expecting success popup or invoice form to appear
            var btnPrint = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnPrint"))?.AsButton();
            Assert.NotNull(btnPrint); // Print button appears on success invoice form
        }

        [Fact]
        public void TC_SALE_02_SellMoreThanInventory_ShouldShowError()
        {
            LoginAndNavigateToPOS();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var gridBooks = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvBooks"))?.AsDataGridView();
            var gridCart = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvCart"))?.AsDataGridView();

            // Act
            // Click many times to exceed inventory (assuming inventory is small for test data)
            for (int i = 0; i < 50; i++)
            {
                gridBooks?.Rows[0]?.Cells[0]?.DoubleClick();
            }
            Thread.Sleep(500);

            // Assert
            var errorPopup = mainForm.FindFirstDescendant(cf => cf.ByName("Cảnh báo"));
            Assert.NotNull(errorPopup);
        }
    }
}
