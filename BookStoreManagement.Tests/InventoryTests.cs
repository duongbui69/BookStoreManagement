using FlaUI.Core.AutomationElements;
using System.Threading;
using Xunit;

namespace BookStoreManagement.Tests
{
    public class InventoryTests : TestBase
    {
        private void LoginAndNavigateToInventory()
        {
            var txtUsername = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtUsername"))?.AsTextBox();
            var txtPassword = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtPassword"))?.AsTextBox();
            var btnLogin = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("btnLogin"))?.AsButton();

            txtUsername.Text = "admin";
            txtPassword.Text = "admin123";
            btnLogin.Invoke();

            Thread.Sleep(2000);
            var mainForm = app.GetMainWindow(automation);

            var btnInventoryMenu = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnInventory"))?.AsButton();
            btnInventoryMenu?.Invoke();
            Thread.Sleep(500);

            var btnInventorySub = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnInventoryDashboard"))?.AsButton();
            btnInventorySub?.Invoke();
            Thread.Sleep(1000);
        }

        [Fact]
        public void TC_INV_01_ViewInventoryList_ShouldLoadData()
        {
            LoginAndNavigateToInventory();
            var mainForm = app.GetMainWindow(automation);

            // Assert
            var grid = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvData"));
            Assert.NotNull(grid);
        }

        [Fact]
        public void TC_INV_02_AdjustInventoryNegative_ShouldShowError()
        {
            LoginAndNavigateToInventory();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var grid = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvData"))?.AsDataGridView();
            var btnAdjust = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnAdjust"))?.AsButton();

            // Act
            grid?.Rows[0]?.Cells[0]?.Click(); // Select first row
            btnAdjust?.Invoke();
            Thread.Sleep(500);

            // Inside adjust popup
            var txtAdjustQty = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtQuantity"))?.AsTextBox();
            var btnSave = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnSave"))?.AsButton();

            if (txtAdjustQty != null) txtAdjustQty.Text = "-99999"; // Exceed current stock
            btnSave?.Invoke();
            Thread.Sleep(500);

            // Assert
            var errorProvider = mainForm.FindFirstDescendant(cf => cf.ByClassName("ErrorProvider"));
            Assert.NotNull(errorProvider);
        }
    }
}
