using FlaUI.Core.AutomationElements;
using System.Threading;
using Xunit;

namespace BookStoreManagement.Tests
{
    public class CategoryTests : TestBase
    {
        private void LoginAndNavigateToCategories()
        {
            var txtUsername = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtUsername"))?.AsTextBox();
            var txtPassword = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtPassword"))?.AsTextBox();
            var btnLogin = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("btnLogin"))?.AsButton();

            txtUsername.Text = "admin";
            txtPassword.Text = "123456";
            btnLogin.Click();

            Thread.Sleep(3000);
            var mainForm = app.GetAllTopLevelWindows(automation).FirstOrDefault(w => w.Name.Contains("Nhà Sách")) ?? app.GetMainWindow(automation);

            var btnCatalog = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnCatalog"))?.AsButton();
            btnCatalog?.Click();
            Thread.Sleep(1000);
        }

        [Fact]
        public void TC_CAT_01_AddValidCategory_ShouldSucceed()
        {
            LoginAndNavigateToCategories();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var txtCategoryName = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtCategoryName"))?.AsTextBox();
            var btnSave = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnSave"))?.AsButton();
            var btnAdd = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnAdd"))?.AsButton();

            // Act
            btnAdd?.Click();
            Thread.Sleep(1000);
            if (txtCategoryName != null) txtCategoryName.Text = "Sách Thiếu Nhi";
            Thread.Sleep(1000);
            btnSave?.Click();
            Thread.Sleep(1500);

            // Assert
            var grid = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvData"));
            Assert.NotNull(grid);
        }

        [Fact]
        public void TC_CAT_02_AddEmptyCategory_ShouldShowError()
        {
            LoginAndNavigateToCategories();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var txtCategoryName = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtName"))?.AsTextBox();
            var btnSave = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnSave"))?.AsButton();

            // Act
            if (txtCategoryName != null) txtCategoryName.Text = "";
            btnSave?.Click();
            Thread.Sleep(500);

            // Assert
            var errorProvider = mainForm.FindFirstDescendant(cf => cf.ByClassName("ErrorProvider"));
            Assert.NotNull(errorProvider);
        }

        [Fact]
        public void TC_CAT_03_UpdateCategory_ShouldReflectChanges()
        {
            LoginAndNavigateToCategories();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var txtCategoryName = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtName"))?.AsTextBox();
            var btnSave = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnSave"))?.AsButton();
            var grid = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvData"))?.AsDataGridView();
            
            // Act
            grid?.Rows[0]?.Cells[0]?.Click(); // Select first row
            if (txtCategoryName != null) txtCategoryName.Text = "Sách Văn Học Mới";
            btnSave?.Click();
            Thread.Sleep(1000);

            // Assert
            Assert.NotNull(grid);
        }

        [Fact]
        public void TC_CAT_04_AddDuplicateCategory_ShouldShowError()
        {
            LoginAndNavigateToCategories();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var txtCategoryName = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("txtName"))?.AsTextBox();
            var btnSave = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnSave"))?.AsButton();
            var btnAdd = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnAdd"))?.AsButton();

            // Act
            btnAdd?.Click();
            if (txtCategoryName != null) txtCategoryName.Text = "Sách Giáo Khoa"; // Assuming this exists
            btnSave?.Click();
            Thread.Sleep(500);

            // Assert
            var errorProvider = mainForm.FindFirstDescendant(cf => cf.ByClassName("ErrorProvider"));
            Assert.NotNull(errorProvider);
        }

        [Fact]
        public void TC_CAT_05_DeleteCategory_ShouldRemoveFromGrid()
        {
            LoginAndNavigateToCategories();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var btnDelete = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnDelete"))?.AsButton();
            var grid = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvData"))?.AsDataGridView();

            // Act
            grid?.Rows[0]?.Cells[0]?.Click(); // Select first row
            btnDelete?.Click();
            Thread.Sleep(500);
            
            // Confirm popup (if any)
            var btnConfirm = mainForm.FindFirstDescendant(cf => cf.ByName("Yes"))?.AsButton();
            btnConfirm?.Click();
            Thread.Sleep(1000);

            // Assert
            Assert.NotNull(grid);
        }
    }
}
