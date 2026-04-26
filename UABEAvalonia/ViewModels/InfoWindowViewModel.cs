using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using AssetsTools.NET.Extra;
using System.Collections.Generic;
using UABEAvalonia.Services;

namespace UABEAvalonia.ViewModels
{
    public partial class InfoWindowViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IWindowService _windowService;

        [ObservableProperty]
        private string title = "Assets Info";

        [ObservableProperty]
        private ObservableCollection<AssetInfoDataGridItem> assets = new ObservableCollection<AssetInfoDataGridItem>();

        public InfoWindowViewModel(IDialogService dialogService, IWindowService windowService)
        {
            _dialogService = dialogService;
            _windowService = windowService;
        }

        // We will move data loading logic here
    }
}
