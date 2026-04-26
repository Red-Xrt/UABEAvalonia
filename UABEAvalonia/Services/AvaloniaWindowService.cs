using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetsTools.NET.Extra;
using UABEAvalonia.Logic;
using UABEAvalonia.ViewModels;
using System.Linq;

namespace UABEAvalonia.Services
{
    public class AvaloniaWindowService : IWindowService
    {
        private Window? GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }

        private IEnumerable<Window> GetWindows()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.Windows;
            }
            return Enumerable.Empty<Window>();
        }

        public async Task OpenInfoWindow(AssetsManager assetsManager, List<AssetsFileInstance> assetsFiles, bool fromBundle)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var infoWindow = new UABEAvalonia.InfoWindow(assetsManager, assetsFiles, fromBundle);
                infoWindow.Show(mainWindow); // Show modeless for info windows
            }
        }

        public async Task OpenAboutWindow()
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var aboutWindow = new UABEAvalonia.About();
                await aboutWindow.ShowDialog(mainWindow);
            }
        }

        public async Task OpenLoadModPackageWindow(InstallerPackageFile emip, AssetsManager am)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null && emip != null)
            {
                var loadModPackageWindow = new UABEAvalonia.LoadModPackageDialog(emip, am);
                await loadModPackageWindow.ShowDialog(mainWindow);
            }
        }

        public Task CloseAllInfoWindows()
        {
            foreach (var window in GetWindows().ToList())
            {
                if (window is UABEAvalonia.InfoWindow infoWindow)
                {
                    infoWindow.Close();
                }
            }
            return Task.CompletedTask;
        }
    }
}
