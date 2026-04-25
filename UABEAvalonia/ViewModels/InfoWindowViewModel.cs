using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using AssetsTools.NET.Extra;
using System.Collections.Generic;

namespace UABEAvalonia.ViewModels
{
    public partial class InfoWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string title = "Assets Info";

        [ObservableProperty]
        private ObservableCollection<AssetInfoDataGridItem> assets = new ObservableCollection<AssetInfoDataGridItem>();

        public InfoWindowViewModel()
        {
        }

        // We will move data loading logic here
    }
}
