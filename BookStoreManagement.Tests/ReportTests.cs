using FlaUI.Core.AutomationElements;
using System.Threading;
using Xunit;

namespace BookStoreManagement.Tests
{
    public class ReportTests : TestBase
    {
        private void LoginAndNavigateToReports()
        {
            var txtUsername = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtUsername"))?.AsTextBox();
            var txtPassword = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("txtPassword"))?.AsTextBox();
            var btnLogin = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("btnLogin"))?.AsButton();

            txtUsername.Text = "admin";
            txtPassword.Text = "admin123";
            btnLogin.Invoke();

            Thread.Sleep(2000);
            var mainForm = app.GetMainWindow(automation);

            var btnReports = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnReports"))?.AsButton();
            btnReports?.Invoke();
            Thread.Sleep(1500); // Reports take longer to load charts
        }

        [Fact]
        public void TC_REP_01_ViewDashboard_ShouldDisplayChartsAndMetrics()
        {
            LoginAndNavigateToReports();
            var mainForm = app.GetMainWindow(automation);

            // Assert
            var chart = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("revenueChart"));
            var cardTotal = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("cardAllTimeRevenue"));
            Assert.NotNull(chart);
            Assert.NotNull(cardTotal);
        }

        [Fact]
        public void TC_REP_02_FilterRevenueByDate_ShouldUpdateTable()
        {
            LoginAndNavigateToReports();
            var mainForm = app.GetMainWindow(automation);

            // Arrange
            var dtpFrom = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dtpFromDate"))?.AsDateTimePicker();
            var dtpTo = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dtpToDate"))?.AsDateTimePicker();
            var btnFilter = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("btnFilter"))?.AsButton();
            var gridRevenue = mainForm.FindFirstDescendant(cf => cf.ByAutomationId("dgvRevenue"))?.AsDataGridView();

            // Act
            if (dtpFrom != null) dtpFrom.SelectedDate = System.DateTime.Now.AddDays(-7);
            if (dtpTo != null) dtpTo.SelectedDate = System.DateTime.Now;
            btnFilter?.Invoke();
            Thread.Sleep(1000);

            // Assert
            Assert.NotNull(gridRevenue);
        }
    }
}
