using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.Collections.Generic;
using UABEAvalonia.Plugins;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using System.Linq;

namespace UABEAvalonia.Services
{
    public class AvaloniaDialogService : IDialogService
    {
        private Window? GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }

        public async Task<string[]> OpenFileDialog(string title, bool allowMultiple, List<string> patterns)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return new string[0];

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = allowMultiple,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Files") { Patterns = patterns }
                }
            };

            var result = await mainWindow.StorageProvider.OpenFilePickerAsync(options);
            return result.Select(x => x.Path.LocalPath).ToArray();
        }

        public async Task<string?> SaveFileDialog(string title, string suggestedFileName, List<string> patterns = null)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return null;

            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = suggestedFileName
            };

            if (patterns != null && patterns.Count > 0)
            {
                options.FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Files") { Patterns = patterns }
                };
            }

            var result = await mainWindow.StorageProvider.SaveFilePickerAsync(options);
            return result?.Path.LocalPath;
        }

        public async Task<string[]> OpenFolderDialog(string title)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return new string[0];

            var options = new FolderPickerOpenOptions
            {
                Title = title
            };

            var result = await mainWindow.StorageProvider.OpenFolderPickerAsync(options);
            return result.Select(x => x.Path.LocalPath).ToArray();
        }

        public async Task<MessageBoxResult> ShowMessageBox(string title, string message, MessageBoxType type = MessageBoxType.OK)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return MessageBoxResult.OK;

            return await MessageBoxUtil.ShowDialog(mainWindow, title, message, type);
        }

        public async Task<string> ShowCustomMessageBox(string title, string message, params string[] options)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return string.Empty;

            return await MessageBoxUtil.ShowDialogCustom(mainWindow, title, message, options);
        }

        public async Task<string?> AskLoadSplitFile(string fileToSplit)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return fileToSplit;

            MessageBoxResult splitRes = await MessageBoxUtil.ShowDialog(mainWindow,
                "Split file detected", "This file ends with .split0. Create merged file?\n",
                MessageBoxType.YesNoCancel);

            if (splitRes == MessageBoxResult.Yes)
            {
                var selectedFilePath = await SaveFileDialog("Select location for merged file", System.IO.Path.GetFileName(fileToSplit[..^".split0".Length]));

                if (selectedFilePath == null)
                    return null;

                using (System.IO.FileStream mergeFile = System.IO.File.Open(selectedFilePath, System.IO.FileMode.Create))
                {
                    int idx = 0;
                    string thisSplitFileNoNum = fileToSplit.Substring(0, fileToSplit.Length - 1);
                    string thisSplitFileNum = fileToSplit;
                    while (System.IO.File.Exists(thisSplitFileNum))
                    {
                        using (System.IO.FileStream thisSplitFile = System.IO.File.OpenRead(thisSplitFileNum))
                        {
                            thisSplitFile.CopyTo(mergeFile);
                        }

                        idx++;
                        thisSplitFileNum = $"{thisSplitFileNoNum}{idx}";
                    };
                }
                return selectedFilePath;
            }
            else if (splitRes == MessageBoxResult.No)
            {
                return fileToSplit;
            }
            else //if (splitRes == MessageBoxResult.Cancel)
            {
                return null;
            }
        }

        public async Task<string> AskForVersion(string currentVersion)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return "0.0.0";
            VersionWindow window = new VersionWindow(currentVersion);
            return await window.ShowDialog<string>(mainWindow);
        }

        public async Task<string> AskForRename(string currentName)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return string.Empty;
            RenameWindow window = new RenameWindow(currentName);
            return await window.ShowDialog<string>(mainWindow);
        }

        public async Task<bool> AskForImportSerialized()
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return false;
            ImportSerializedDialog dialog = new ImportSerializedDialog();
            return await dialog.ShowDialog<bool>(mainWindow);
        }

        public async Task<SearchDialogResult?> ShowSearchDialog()
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return null;
            SearchDialog dialog = new SearchDialog();
            return await dialog.ShowDialog<SearchDialogResult?>(mainWindow);
        }

        public async Task<AssetPPtr?> ShowGoToAssetDialog(AssetWorkspace workspace)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return null;
            GoToAssetDialog dialog = new GoToAssetDialog(workspace);
            return await dialog.ShowDialog<AssetPPtr?>(mainWindow);
        }

        public async Task<HashSet<AssetClassID>?> ShowFilterAssetTypeDialog(HashSet<AssetClassID> filteredOutTypeIds, HashSet<AssetClassID> usedIds)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return null;
            FilterAssetTypeDialog dialog = new FilterAssetTypeDialog(filteredOutTypeIds, usedIds);
            return await dialog.ShowDialog<HashSet<AssetClassID>?>(mainWindow);
        }

        public async Task<Dictionary<AssetsFileInstance, AssetsFileChangeTypes>?> ShowAssetsFileInfoWindow(AssetWorkspace workspace, AssetsFileInfoWindowStartTab startTab)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return null;
            AssetsFileInfoWindow dialog = new AssetsFileInfoWindow(workspace, startTab);
            return await dialog.ShowDialog<Dictionary<AssetsFileInstance, AssetsFileChangeTypes>?>(mainWindow);
        }

        public async Task<string?> ShowSelectDumpWindow(bool export)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return null;
            SelectDumpWindow dialog = new SelectDumpWindow(export);
            return await dialog.ShowDialog<string?>(mainWindow);
        }

        public async Task<List<ImportBatchInfo>?> ShowImportBatchWindow(AssetWorkspace workspace, List<AssetContainer> selection, string dir, List<string> extensions)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return null;
            ImportBatch dialog = new ImportBatch(workspace, selection, dir, extensions);
            return await dialog.ShowDialog<List<ImportBatchInfo>?>(mainWindow);
        }

        public async Task<byte[]?> ShowEditAssetWindow(AssetTypeValueField baseField)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return null;
            EditDataWindow dialog = new EditDataWindow(baseField);
            return await dialog.ShowDialog<byte[]?>(mainWindow);
        }

        public async Task ShowPluginWindow(AssetWorkspace workspace, List<AssetContainer> selection, UABEAvalonia.Infrastructure.Plugins.PluginManager pluginManager)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return;
            PluginWindow dialog = new PluginWindow(mainWindow, workspace, selection, pluginManager);
            await dialog.ShowDialog(mainWindow);
        }

        public async Task ShowAddAssetWindow(AssetWorkspace workspace)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return;
            AddAssetWindow dialog = new AddAssetWindow(workspace);
            await dialog.ShowDialog(mainWindow);
        }

        public async Task ShowModMakerDialog(AssetWorkspace workspace)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return;
            ModMakerDialog dialog = new ModMakerDialog(workspace);
            await dialog.ShowDialog(mainWindow);
        }
    }
}
