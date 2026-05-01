using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.Collections.Generic;
using UABEAvalonia.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UABEAvalonia.Services
{
    public interface IDialogService
    {
        Task<string[]> OpenFileDialog(string title, bool allowMultiple, List<string> patterns);
        Task<string?> SaveFileDialog(string title, string suggestedFileName, List<string> patterns = null);
        Task<string[]> OpenFolderDialog(string title);
        Task<MessageBoxResult> ShowMessageBox(string title, string message, MessageBoxType type = MessageBoxType.OK);
        Task<string> ShowCustomMessageBox(string title, string message, params string[] options);
        Task<string?> AskLoadSplitFile(string fileToSplit);
        Task<string> AskForVersion(string currentVersion);
        Task<bool> AskForImportSerialized();
        Task<string> AskForRename(string currentName);

        Task<SearchDialogResult?> ShowSearchDialog();
        Task<AssetPPtr?> ShowGoToAssetDialog(AssetWorkspace workspace);
        Task<System.Collections.Generic.HashSet<AssetClassID>?> ShowFilterAssetTypeDialog(System.Collections.Generic.HashSet<AssetClassID> filteredOutTypeIds, System.Collections.Generic.HashSet<AssetClassID> usedIds);
        Task<System.Collections.Generic.Dictionary<AssetsFileInstance, AssetsFileChangeTypes>?> ShowAssetsFileInfoWindow(AssetWorkspace workspace, AssetsFileInfoWindowStartTab startTab);
        Task<string?> ShowSelectDumpWindow(bool export);
        Task<System.Collections.Generic.List<ImportBatchInfo>?> ShowImportBatchWindow(AssetWorkspace workspace, System.Collections.Generic.List<AssetContainer> selection, string dir, System.Collections.Generic.List<string> extensions);
        Task<byte[]?> ShowEditAssetWindow(AssetTypeValueField baseField);
        Task ShowPluginWindow(AssetWorkspace workspace, System.Collections.Generic.List<AssetContainer> selection, UABEAvalonia.Infrastructure.Plugins.PluginManager pluginManager);
        Task ShowAddAssetWindow(AssetWorkspace workspace);
        Task ShowModMakerDialog(AssetWorkspace workspace);
    }
}
