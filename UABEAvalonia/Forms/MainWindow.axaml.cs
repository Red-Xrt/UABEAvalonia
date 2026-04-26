using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using UABEAvalonia.ViewModels;

namespace UABEAvalonia
{
    public partial class MainWindow : Window
    {
        // Keep a reference if any legacy external plugins attempt to reach in, otherwise ViewModel holds the state
        public BundleWorkspace Workspace { get; private set; }

        public MainWindow()
        {
            var viewModel = (MainWindowViewModel)AppServices.Provider.GetService(typeof(MainWindowViewModel))!;
            DataContext = viewModel;

            Workspace = ((UABEAvalonia.Services.IBundleService)AppServices.Provider.GetService(typeof(UABEAvalonia.Services.IBundleService))!).Workspace;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            Closing += MainWindow_Closing;
        }

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            string classDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "classdata.tpk");

            var bundleService = (UABEAvalonia.Services.IBundleService)AppServices.Provider.GetService(typeof(UABEAvalonia.Services.IBundleService))!;
            var dialogService = (UABEAvalonia.Services.IDialogService)AppServices.Provider.GetService(typeof(UABEAvalonia.Services.IDialogService))!;

            if (File.Exists(classDataPath))
            {
                bundleService.LoadClassPackage(classDataPath);
            }
            else
            {
                await dialogService.ShowMessageBox("Error", "Missing classdata.tpk by exe.\nPlease make sure it exists.");
                Close();
                Environment.Exit(1);
            }
        }

        private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            var bundleService = (UABEAvalonia.Services.IBundleService)AppServices.Provider.GetService(typeof(UABEAvalonia.Services.IBundleService))!;
            var dialogService = (UABEAvalonia.Services.IDialogService)AppServices.Provider.GetService(typeof(UABEAvalonia.Services.IDialogService))!;
            var viewModel = (MainWindowViewModel)DataContext!;

            if (bundleService.ChangesUnsaved)
            {
                e.Cancel = true;

                var result = await dialogService.ShowMessageBox("Changes made", "You've modified this file. Would you like to save?", UABEAvalonia.MessageBoxType.YesNoCancel);

                if (result == UABEAvalonia.MessageBoxResult.Cancel)
                {
                    return; // Abort close
                }

                if (result == UABEAvalonia.MessageBoxResult.Yes)
                {
                    if (viewModel.SaveCommand != null && viewModel.SaveCommand.CanExecute(null))
                    {
                        await viewModel.SaveCommand.ExecuteAsync(null);
                    }
                }

                Closing -= MainWindow_Closing;
                Close();
            }
        }
    }
}
