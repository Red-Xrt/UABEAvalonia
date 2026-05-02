using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using UABEAvalonia.ViewModels;
using System.Collections.Generic;

namespace UABEAvalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var viewModel = (MainWindowViewModel)AppServices.Provider.GetService(typeof(MainWindowViewModel))!;
            DataContext = viewModel;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            Closing += MainWindow_Closing;
            AddHandler(DragDrop.DropEvent, Drop);
        }

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            if (DataContext is MainWindowViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }

        private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                e.Cancel = true; // Prevent immediate closing

                bool shouldClose = await vm.OnClosingAsync();

                if (shouldClose)
                {
                    Closing -= MainWindow_Closing;
                    Close();
                }
            }
        }

        private void Drop(object? sender, DragEventArgs e)
        {
            // Fallback empty handle to resolve build errors for now
        }
    }
}
