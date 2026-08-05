using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.IO;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace BookStoreManagement.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected FlaUI.Core.Application app;
        protected UIA3Automation automation;
        protected Window mainWindow;

        public TestBase()
        {
            // The path to the built executable
            string appPath = Path.GetFullPath(@"..\..\..\..\BookStoreManagement\bin\Debug\net10.0-windows\BookStoreManagement.exe");
            if (!File.Exists(appPath))
            {
                throw new FileNotFoundException("Cannot find the application executable at " + appPath);
            }
            
            app = FlaUI.Core.Application.Launch(appPath);
            automation = new UIA3Automation();
            
            // Wait for login window
            Thread.Sleep(2000);
            mainWindow = app.GetMainWindow(automation);
            if (mainWindow == null)
            {
                Thread.Sleep(2000);
                mainWindow = app.GetMainWindow(automation);
            }
            mainWindow.WaitUntilClickable(TimeSpan.FromSeconds(10));
        }

        public void Dispose()
        {
            automation?.Dispose();
            app?.Close();
            app?.Dispose();
        }
    }
}
