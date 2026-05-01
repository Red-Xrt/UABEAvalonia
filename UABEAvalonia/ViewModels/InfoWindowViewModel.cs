using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UABEAvalonia.Models;
using UABEAvalonia.Services;

namespace UABEAvalonia.ViewModels
{
    public partial class InfoWindowViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IWindowService _windowService;

        public AssetWorkspace Workspace { get; private set; }
        public AssetsManager am => Workspace.am;

        [ObservableProperty]
        private string title = "Assets Info";

        [ObservableProperty]
        private ObservableCollection<AssetInfoDataGridItem> assets = new ObservableCollection<AssetInfoDataGridItem>();

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private string selectedName = string.Empty;

        [ObservableProperty]
        private string selectedPathId = string.Empty;

        [ObservableProperty]
        private string selectedFileId = string.Empty;

        [ObservableProperty]
        private string selectedType = string.Empty;

        [ObservableProperty]
        private AssetInfoDataGridItem? selectedAsset;

        [ObservableProperty]
        private System.Collections.IList? selectedAssets;

        private int searchStart = 0;
        private bool searchDown = false;
        private bool searchCaseSensitive = true;
        private bool searching = false;

        private HashSet<AssetClassID> filteredOutTypeIds = new HashSet<AssetClassID>();

        private UABEAvalonia.Infrastructure.Plugins.PluginManager pluginManager;

        public InfoWindowViewModel() {}

        public InfoWindowViewModel(IDialogService dialogService, IWindowService windowService)
        {
            _dialogService = dialogService;
            _windowService = windowService;
            pluginManager = new UABEAvalonia.Infrastructure.Plugins.PluginManager();
            pluginManager.LoadPluginsInDirectory(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"));
        }

        public void Init(AssetWorkspace workspace)
        {
            Workspace = workspace;
            LoadAssets();
        }

        private void LoadAssets()
        {
            if (Workspace == null) return;

            Assets.Clear();
            Workspace.GenerateAssetsFileLookup();

            foreach (AssetContainer cont in Workspace.LoadedAssets.Values)
            {
                Assets.Add(CreateGridItem(cont));
            }
        }

        private AssetInfoDataGridItem CreateGridItem(AssetContainer cont)
        {
            AssetsFileInstance thisFileInst = cont.FileInstance;

            string name;
            string type;

            string container = cont.Container;
            int fileId = Workspace.LoadedFiles.IndexOf(thisFileInst);
            long pathId = cont.PathId;
            int size = (int)cont.Size;
            string modified = "";

            AssetNameUtils.GetDisplayNameFast(Workspace, cont, true, out name, out type);

            if (name.Length > 100) name = name[..100];
            if (type.Length > 100) type = type[..100];

            return new AssetInfoDataGridItem
            {
                TypeClass = (AssetClassID)cont.ClassId,
                Name = name,
                Container = container,
                Type = type,
                TypeID = cont.ClassId,
                FileID = fileId,
                PathID = pathId,
                Size = (uint)size,
                Modified = modified,
                AssetContainer = cont
            };
        }

        partial void OnSelectedAssetChanged(AssetInfoDataGridItem? value)
        {
            if (value != null)
            {
                SelectedName = value.Name;
                SelectedPathId = value.PathID.ToString();
                SelectedFileId = value.FileID.ToString();
                SelectedType = value.Type;
            }
            else
            {
                SelectedName = string.Empty;
                SelectedPathId = string.Empty;
                SelectedFileId = string.Empty;
                SelectedType = string.Empty;
            }
        }
    }
}
