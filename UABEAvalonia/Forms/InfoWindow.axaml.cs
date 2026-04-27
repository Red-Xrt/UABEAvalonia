using Avalonia.Controls;
using System.Collections.Generic;
using UABEAvalonia.ViewModels;
using AssetsTools.NET.Extra;

namespace UABEAvalonia
{
    public partial class InfoWindow : Window
    {
        public AssetWorkspace Workspace => ((InfoWindowViewModel)DataContext).Workspace;

        public InfoWindow()
        {
            InitializeComponent();
            DataContext = AppServices.Provider.GetService(typeof(InfoWindowViewModel));
        }

        // Keep stub for backwards compatibility with parts of code not fully migrated
        public InfoWindow(AssetsManager am, List<AssetsFileInstance> assetsFiles, bool fromBundle)
        {
            InitializeComponent();
            var viewModel = (InfoWindowViewModel)AppServices.Provider.GetService(typeof(InfoWindowViewModel))!;
            viewModel.Init(new AssetWorkspace(am, fromBundle));
            DataContext = viewModel;
        }

        public async System.Threading.Tasks.Task ShowEditAssetWindow(AssetContainer cont)
        {
            // Stub to fix external calls during migration
        }

        public void SelectAsset(AssetsFileInstance targetFile, long targetPathId)
        {
            // Stub
        }
    }
}
