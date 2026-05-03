using AssetsTools.NET;
using AssetsTools.NET.Extra;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UABEAvalonia.Models;
using UABEAvalonia.Services;

namespace UABEAvalonia.ViewModels
{
    public partial class InfoWindowViewModel
    {
        [RelayCommand]
        private async Task AddAssetAsync()
        {
            await _dialogService.ShowAddAssetWindow(Workspace);
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            await SaveFile(false);
            ClearModified();
            Workspace.Modified = false;
        }

        [RelayCommand]
        private async Task SaveAsAsync()
        {
            await SaveFile(true);
            ClearModified();
            Workspace.Modified = false;
        }

        [RelayCommand]
        private async Task CreatePackageFileAsync()
        {
            await _dialogService.ShowModMakerDialog(Workspace);
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            var res = await _dialogService.ShowSearchDialog();
            if (res != null && res.ok)
            {
                int selectedIndex = Assets.IndexOf(SelectedAsset!);

                searchText = res.text;
                searchStart = selectedIndex != -1 ? selectedIndex : 0;
                searchDown = res.isDown;
                searchCaseSensitive = res.caseSensitive;
                searching = true;
                NextNameSearch();
            }
        }

        [RelayCommand]
        private void ContinueSearch()
        {
            NextNameSearch();
        }

        [RelayCommand]
        private async Task GoToAssetAsync()
        {
            var res = await _dialogService.ShowGoToAssetDialog(Workspace);
            if (res != null)
            {
                AssetsFileInstance targetFile = Workspace.LoadedFiles[res.FileId];
                long targetPathId = res.PathId;

                IdSearch(targetFile, targetPathId);
            }
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            HashSet<AssetClassID> usedIds = Workspace.LoadedAssets.Select(a => (AssetClassID)a.Value.ClassId).Distinct().ToHashSet();
            var result = await _dialogService.ShowFilterAssetTypeDialog(filteredOutTypeIds, usedIds);
            if (result != null)
            {
                filteredOutTypeIds = result;
                // UI filtering would normally be handled by a CollectionView, here we'll update the observable collection directly
                UpdateFilteredAssets();
            }
        }

        [RelayCommand]
        private async Task HierarchyAsync()
        {
            await _windowService.OpenGameObjectViewWindow(Workspace);
        }

        [RelayCommand]
        private async Task InfoAsync()
        {
            await OpenAssetsFileInfoWindow(AssetsFileInfoWindowStartTab.General);
        }

        [RelayCommand]
        private async Task TypeTreeAsync()
        {
            await OpenAssetsFileInfoWindow(AssetsFileInfoWindowStartTab.TypeTree);
        }

        [RelayCommand]
        private async Task DependenciesAsync()
        {
            await OpenAssetsFileInfoWindow(AssetsFileInfoWindowStartTab.Dependencies);
        }

        [RelayCommand]
        private async Task ScriptsAsync()
        {
            await OpenAssetsFileInfoWindow(AssetsFileInfoWindowStartTab.Script);
        }

        [RelayCommand]
        private async Task ViewDataAsync()
        {
            if (await FailIfNothingSelected())
                return;

            AssetInfoDataGridItem gridItem = SelectedAsset!;
            if (!await WarnIfAssetSizeLarge(gridItem))
                return;

            List<AssetContainer> selectedConts = GetSelectedAssetsReplaced();
            if (selectedConts.Count > 0)
            {
                await _windowService.OpenDataWindow(Workspace, selectedConts[0]);
            }
        }

        [RelayCommand]
        private async Task SceneViewAsync()
        {
            if (await FailIfNothingSelected())
                return;

            AssetInfoDataGridItem gridItem = SelectedAsset!;
            AssetContainer container = gridItem.AssetContainer;

            if (gridItem.TypeClass == AssetClassID.GameObject)
            {
                await _windowService.OpenGameObjectViewWindow(Workspace, container);
            }
            else
            {
                bool hasGameObjectParent = false;
                if (container.HasValueField)
                {
                    hasGameObjectParent = container.BaseValueField!.Children.Any(c => c.FieldName == "m_GameObject");
                }
                else
                {
                    AssetTypeTemplateField template = Workspace.GetTemplateField(container, false, true);
                    hasGameObjectParent = template.Children.Any(c => c.Name == "m_GameObject");
                }

                if (!hasGameObjectParent)
                {
                    await _dialogService.ShowMessageBox("Warning", "The asset you selected is not a scene asset.");
                    return;
                }

                AssetTypeValueField componentBf;
                if (container.HasValueField)
                {
                    componentBf = container.BaseValueField;
                }
                else
                {
                    try
                    {
                        componentBf = Workspace.GetBaseField(container);
                    }
                    catch
                    {
                        await _dialogService.ShowMessageBox("Error", "Asset failed to deserialize.");
                        return;
                    }
                }

                if (componentBf == null)
                {
                    await _dialogService.ShowMessageBox("Error", "Asset failed to deserialize.");
                    return;
                }

                AssetContainer goContainer = Workspace.GetAssetContainer(
                    container.FileInstance, componentBf["m_GameObject"], true);

                await _windowService.OpenGameObjectViewWindow(Workspace, goContainer);
            }
        }

        [RelayCommand]
        private async Task ExportRawAsync()
        {
            if (await FailIfNothingSelected())
                return;

            List<AssetContainer> selection = GetSelectedAssetsReplaced();

            if (selection.Count > 1)
                await BatchExportRaw(selection);
            else
                await SingleExportRaw(selection);
        }

        [RelayCommand]
        private async Task ExportDumpAsync()
        {
            if (await FailIfNothingSelected())
                return;

            List<AssetContainer> selection = GetSelectedAssetsReplaced();

            if (selection.Count > 1)
                await BatchExportDump(selection);
            else
                await SingleExportDump(selection);
        }

        [RelayCommand]
        private async Task ImportRawAsync()
        {
            if (await FailIfNothingSelected())
                return;

            List<AssetContainer> selection = GetSelectedAssetsReplaced();

            if (selection.Count > 1)
                await BatchImportRaw(selection);
            else
                await SingleImportRaw(selection);
        }

        [RelayCommand]
        private async Task ImportDumpAsync()
        {
            if (await FailIfNothingSelected())
                return;

            List<AssetContainer> selection = GetSelectedAssetsReplaced();

            if (selection.Count > 1)
                await BatchImportDump(selection);
            else
                await SingleImportDump(selection);
        }

        [RelayCommand]
        private async Task EditDataAsync()
        {
            if (await FailIfNothingSelected())
                return;

            AssetInfoDataGridItem gridItem = SelectedAsset!;
            if (!await WarnIfAssetSizeLarge(gridItem))
                return;

            AssetContainer? selection = GetSelectedAssetsReplaced()[0];
            if (selection != null && !selection.HasValueField)
            {
                selection = Workspace.GetAssetContainer(selection.FileInstance, 0, selection.PathId, false);
            }
            if (selection == null)
            {
                await _dialogService.ShowMessageBox("Error", "Asset failed to deserialize.");
                return;
            }

            await ShowEditAssetWindow(selection);
        }

        [RelayCommand]
        private async Task RemoveAsync()
        {
            if (await FailIfNothingSelected())
                return;

            MessageBoxResult choice = await _dialogService.ShowMessageBox(
                "Removing assets", "Removing an asset referenced by other assets can cause crashes!\nAre you sure?",
                MessageBoxType.YesNo);

            if (choice == MessageBoxResult.Yes)
            {
                List<AssetContainer> selection = GetSelectedAssetsReplaced();
                foreach (AssetContainer cont in selection)
                {
                    Workspace.AddReplacer(cont.FileInstance, new ContentRemover(), cont.PathId, cont.ClassId, cont.MonoId);
                }
            }
        }

        [RelayCommand]
        private async Task PluginAsync()
        {
            if (await FailIfNothingSelected())
                return;

            List<AssetContainer> conts = GetSelectedAssetsReplaced();
            await _dialogService.ShowPluginWindow(Workspace, conts, pluginManager);
        }

        [RelayCommand]
        private async Task CloseAsync()
        {
            if (Workspace.Modified)
            {
                MessageBoxResult choice = await _dialogService.ShowMessageBox(
                    "Changes made", "You've modified this file. Would you like to save?",
                    MessageBoxType.YesNo);
                if (choice == MessageBoxResult.Yes)
                {
                    await SaveFile(false);
                }
            }

            CloseFile();
        }

        // --- Helper methods for commands ---

        private async Task<bool> FailIfNothingSelected()
        {
            if (SelectedAssets == null || SelectedAssets.Count == 0)
            {
                await _dialogService.ShowMessageBox("Note", "No item selected.");
                return true;
            }
            return false;
        }

        private async Task<bool> WarnIfAssetSizeLarge(AssetInfoDataGridItem gridItem)
        {
            if (gridItem.Size > 500000)
            {
                var result = await _dialogService.ShowCustomMessageBox(
                    "Warning", "The asset you are about to open is very big and may take a lot of time and memory.",
                    "Continue anyway", "Cancel");

                if (result == "Cancel")
                    return false;
            }

            return true;
        }

        private List<AssetContainer> GetSelectedAssetsReplaced()
        {
            var exts = new List<AssetContainer>();
            if (SelectedAssets != null)
            {
                foreach (AssetInfoDataGridItem gridItem in SelectedAssets)
                {
                    exts.Add(gridItem.AssetContainer);
                }
            }
            return exts;
        }

        private void ClearModified()
        {
            foreach (AssetInfoDataGridItem gridItem in Assets)
            {
                if (gridItem.Modified != "")
                {
                    gridItem.Modified = "";
                }
            }
        }

        private void CloseFile()
        {
            am.UnloadAllAssetsFiles(true);
            // Window close will be handled by calling component or event
        }

        private async Task SaveFile(bool saveAs)
        {
            var fileToReplacer = new Dictionary<AssetsFileInstance, List<IContentReplacer>>();
            var changedFiles = Workspace.GetChangedFiles();

            foreach (var newAsset in Workspace.NewAssets)
            {
                var assetId = newAsset.Key;
                IContentReplacer replacer = newAsset.Value;
                int fileId = assetId.FileId;

                if (Workspace.LoadedFiles.Count > fileId)
                {
                    var file = Workspace.LoadedFiles[fileId];
                    if (file != null)
                    {
                        if (!fileToReplacer.ContainsKey(file))
                            fileToReplacer[file] = new List<IContentReplacer>();

                        fileToReplacer[file].Add(replacer);
                    }
                }
            }

            if (Workspace.fromBundle)
            {
                // Logic for Bundle
                List<Tuple<AssetsFileInstance, byte[]>> changedAssetsDatas = new List<Tuple<AssetsFileInstance, byte[]>>();

                foreach (var file in changedFiles)
                {
                    List<IContentReplacer> replacers = fileToReplacer.ContainsKey(file) ? fileToReplacer[file] : new List<IContentReplacer>(0);

                    try
                    {
                        using (MemoryStream ms = new MemoryStream())
                        using (AssetsFileWriter w = new AssetsFileWriter(ms))
                        {
                            file.file.Write(w);
                            changedAssetsDatas.Add(new Tuple<AssetsFileInstance, byte[]>(file, ms.ToArray()));
                        }
                    }
                    catch (Exception ex)
                    {
                        await _dialogService.ShowMessageBox("Write exception", "There was a problem while writing the file:\n" + ex.ToString());
                    }
                }

                await _dialogService.ShowMessageBox("Success", "File saved. To complete changes, exit this window and File->Save in bundle window.");
            }
            else
            {
                List<int> changedFileIds = new List<int>();

                foreach (var file in changedFiles)
                {
                    List<IContentReplacer> replacers = fileToReplacer.ContainsKey(file) ? fileToReplacer[file] : new List<IContentReplacer>(0);

                    string? filePath;

                    if (saveAs)
                    {
                        while (true)
                        {
                            filePath = await _dialogService.SaveFileDialog("Save as...", file.name);

                            if (filePath == null)
                                return;

                            if (Path.GetFullPath(filePath) == Path.GetFullPath(file.path))
                            {
                                await _dialogService.ShowMessageBox("File in use", "You already have this file open. To overwrite, use Save instead of Save as.");
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        string newName = "~" + file.name;
                        string dir = Path.GetDirectoryName(file.path)!;
                        filePath = Path.Combine(dir, newName);
                    }

                    try
                    {
                        using (FileStream fs = File.Open(filePath, FileMode.Create))
                        using (AssetsFileWriter w = new AssetsFileWriter(fs))
                        {
                            file.file.Write(w);
                        }

                        if (!saveAs)
                        {
                            string origFilePath = file.path;

                            file.file.Reader.Close();
                            File.Delete(file.path);
                            File.Move(filePath, origFilePath);
                            file.file = new AssetsFile();
                            file.file.Read(new AssetsFileReader(File.OpenRead(origFilePath)));
                            file.file.GenerateQuickLookup();
                        }

                        changedFileIds.Add(Workspace.LoadedFiles.IndexOf(file));
                    }
                    catch (Exception ex)
                    {
                        await _dialogService.ShowMessageBox("Write exception", "There was a problem while writing the file:\n" + ex.ToString());
                    }
                }

                if (!saveAs)
                {
                    foreach (AssetInfoDataGridItem item in Assets)
                    {
                        int fileId = item.FileID;
                        if (changedFileIds.Contains(fileId))
                        {
                            item.AssetContainer.SetNewFile(Workspace.LoadedFiles[fileId]);
                        }
                    }
                }
            }
        }

        private async void NextNameSearch()
        {
            bool foundResult = false;
            if (searching)
            {
                if (searchDown)
                {
                    for (int i = searchStart; i < Assets.Count; i++)
                    {
                        string name = Assets[i].Name;
                        if (SearchUtils.WildcardMatches(name, searchText, searchCaseSensitive))
                        {
                            SelectedAsset = Assets[i];
                            searchStart = i + 1;
                            foundResult = true;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = searchStart; i >= 0; i--)
                    {
                        string name = Assets[i].Name;
                        if (SearchUtils.WildcardMatches(name, searchText, searchCaseSensitive))
                        {
                            SelectedAsset = Assets[i];
                            searchStart = i - 1;
                            foundResult = true;
                            break;
                        }
                    }
                }
            }

            if (!foundResult)
            {
                await _dialogService.ShowMessageBox("Search end", "Can't find any assets that match.");

                searchText = "";
                searchStart = 0;
                searchDown = false;
                searching = false;
            }
        }

        private async void IdSearch(AssetsFileInstance targetFile, long targetPathId)
        {
            bool foundResult = false;

            for (int i = 0; i < Assets.Count; i++)
            {
                AssetContainer cont = Assets[i].AssetContainer;
                if (cont.FileInstance == targetFile && cont.PathId == targetPathId)
                {
                    SelectedAsset = Assets[i];
                    foundResult = true;
                    break;
                }
            }

            if (!foundResult)
            {
                await _dialogService.ShowMessageBox("Search end", "Can't find any assets that match.");
            }
        }

        private async Task OpenAssetsFileInfoWindow(AssetsFileInfoWindowStartTab startTab)
        {
            var changedFiles = await _dialogService.ShowAssetsFileInfoWindow(Workspace, startTab);
            if (changedFiles != null && changedFiles.Count > 0)
            {
                Workspace.Modified = true;
                foreach ((AssetsFileInstance changedFile, AssetsFileChangeTypes newFlags) in changedFiles)
                {
                    Workspace.SetOtherAssetChangeFlag(changedFile, newFlags);
                }
            }
        }

        public async Task<bool> ShowEditAssetWindow(AssetContainer cont)
        {
            AssetTypeValueField baseField = cont.BaseValueField;
            if (baseField == null)
            {
                await _dialogService.ShowMessageBox("Error", "Something went wrong deserializing this asset.");
                return false;
            }

            byte[]? data = await _dialogService.ShowEditAssetWindow(baseField);
            if (data == null)
            {
                return false;
            }

            IContentReplacer replacer = AssetImportExport.CreateAssetReplacer(cont, data);
            Workspace.AddReplacer(cont.FileInstance, replacer, cont.PathId, cont.ClassId, cont.MonoId, new MemoryStream(data));
            return true;
        }

        private void UpdateFilteredAssets(string? searchTextOverride = null)
        {
            LoadAssets();

            var textToSearch = searchTextOverride != null ? searchTextOverride.ToLower() : SearchText?.ToLower() ?? string.Empty;

            var filteredAssets = Assets.ToList();

            if (filteredOutTypeIds.Count > 0)
            {
                filteredAssets = filteredAssets.Where(a => !filteredOutTypeIds.Contains(a.TypeClass)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(textToSearch))
            {
                filteredAssets = filteredAssets.Where(a =>
                    (a.Name != null && a.Name.ToLower().Contains(textToSearch)) ||
                    (a.Type != null && a.Type.ToLower().Contains(textToSearch))
                ).ToList();
            }

            if (filteredAssets.Count != Assets.Count)
            {
                Assets.Clear();
                foreach (var a in filteredAssets) Assets.Add(a);
            }
        }

        private async Task BatchExportRaw(List<AssetContainer> selection)
        {
            var selectedFolderPaths = await _dialogService.OpenFolderDialog("Select export directory");
            if (selectedFolderPaths.Length == 0) return;

            string dir = selectedFolderPaths[0];

            foreach (AssetContainer selectedCont in selection)
            {
                AssetsFileInstance selectedInst = selectedCont.FileInstance;

                AssetNameUtils.GetDisplayNameFast(Workspace, selectedCont, false, out string assetName, out string _);
                assetName = PathUtils.ReplaceInvalidPathChars(assetName);
                string file = Path.Combine(dir, $"{assetName}-{Path.GetFileName(selectedInst.path)}-{selectedCont.PathId}.dat");

                using (FileStream fs = File.Open(file, FileMode.Create))
                {
                    AssetImportExport dumper = new AssetImportExport();
                    dumper.DumpRawAsset(fs, selectedCont.FileReader, selectedCont.FilePosition, selectedCont.Size);
                }
            }
        }

        private async Task SingleExportRaw(List<AssetContainer> selection)
        {
            AssetContainer selectedCont = selection[0];
            AssetsFileInstance selectedInst = selectedCont.FileInstance;

            AssetNameUtils.GetDisplayNameFast(Workspace, selectedCont, false, out string assetName, out string _);
            assetName = PathUtils.ReplaceInvalidPathChars(assetName);

            string? selectedFilePath = await _dialogService.SaveFileDialog("Save as...", $"{assetName}-{Path.GetFileName(selectedInst.path)}-{selectedCont.PathId}", new List<string> { "*.dat", "*.*" });
            if (selectedFilePath == null) return;

            using (FileStream fs = File.Open(selectedFilePath, FileMode.Create))
            {
                AssetImportExport dumper = new AssetImportExport();
                dumper.DumpRawAsset(fs, selectedCont.FileReader, selectedCont.FilePosition, selectedCont.Size);
            }
        }

        private async Task BatchExportDump(List<AssetContainer> selection)
        {
            var selectedFolderPaths = await _dialogService.OpenFolderDialog("Select export directory");
            if (selectedFolderPaths.Length == 0) return;

            string dir = selectedFolderPaths[0];

            string? extension = await _dialogService.ShowSelectDumpWindow(true);
            if (extension == null) return;

            foreach (AssetContainer selectedCont in selection)
            {
                AssetNameUtils.GetDisplayNameFast(Workspace, selectedCont, false, out string assetName, out string _);
                assetName = PathUtils.ReplaceInvalidPathChars(assetName);
                string file = Path.Combine(dir, $"{assetName}-{Path.GetFileName(selectedCont.FileInstance.path)}-{selectedCont.PathId}.{extension}");

                using (FileStream fs = File.Open(file, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    AssetTypeValueField? baseField = Workspace.GetBaseField(selectedCont);

                    if (baseField == null)
                    {
                        sw.WriteLine("Asset failed to deserialize.");
                        continue;
                    }

                    AssetImportExport dumper = new AssetImportExport();
                    if (extension == "json")
                        dumper.DumpJsonAsset(sw, baseField);
                    else
                        dumper.DumpTextAsset(sw, baseField);
                }
            }
        }

        private async Task SingleExportDump(List<AssetContainer> selection)
        {
            AssetContainer selectedCont = selection[0];
            AssetsFileInstance selectedInst = selectedCont.FileInstance;

            AssetNameUtils.GetDisplayNameFast(Workspace, selectedCont, false, out string assetName, out string _);
            assetName = PathUtils.ReplaceInvalidPathChars(assetName);

            string? selectedFilePath = await _dialogService.SaveFileDialog("Save as...", $"{assetName}-{Path.GetFileName(selectedInst.path)}-{selectedCont.PathId}", new List<string> { "*.txt", "*.json" });
            if (selectedFilePath == null) return;

            using (FileStream fs = File.Open(selectedFilePath, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                AssetTypeValueField? baseField = Workspace.GetBaseField(selectedCont);

                if (baseField == null)
                {
                    sw.WriteLine("Asset failed to deserialize.");
                    return;
                }

                AssetImportExport dumper = new AssetImportExport();

                if (selectedFilePath.EndsWith(".json"))
                    dumper.DumpJsonAsset(sw, baseField);
                else
                    dumper.DumpTextAsset(sw, baseField);
            }
        }

        private async Task BatchImportRaw(List<AssetContainer> selection)
        {
            var selectedFolderPaths = await _dialogService.OpenFolderDialog("Select import directory");
            if (selectedFolderPaths.Length == 0) return;

            string dir = selectedFolderPaths[0];
            List<string> extensions = new List<string>() { "dat" };

            var batchInfos = await _dialogService.ShowImportBatchWindow(Workspace, selection, dir, extensions);
            if (batchInfos != null)
            {
                foreach (var batchInfo in batchInfos)
                {
                    string selectedFilePath = batchInfo.importFile;
                    AssetContainer selectedCont = batchInfo.cont;
                    AssetsFileInstance selectedInst = selectedCont.FileInstance;

                    using (FileStream fs = File.OpenRead(selectedFilePath))
                    {
                        AssetImportExport importer = new AssetImportExport();
                        byte[] bytes = importer.ImportRawAsset(fs);

                        IContentReplacer replacer = AssetImportExport.CreateAssetReplacer(selectedCont, bytes);
                        Workspace.AddReplacer(selectedInst, replacer, selectedCont.PathId, selectedCont.ClassId, selectedCont.MonoId, new MemoryStream(bytes));
                    }
                }
            }
        }

        private async Task SingleImportRaw(List<AssetContainer> selection)
        {
            AssetContainer selectedCont = selection[0];
            AssetsFileInstance selectedInst = selectedCont.FileInstance;

            var selectedFilePaths = await _dialogService.OpenFileDialog("Open", false, new List<string> { "*.dat" });
            if (selectedFilePaths.Length == 0) return;

            string file = selectedFilePaths[0];

            using (FileStream fs = File.OpenRead(file))
            {
                AssetImportExport importer = new AssetImportExport();
                byte[] bytes = importer.ImportRawAsset(fs);

                IContentReplacer replacer = AssetImportExport.CreateAssetReplacer(selectedCont, bytes);
                Workspace.AddReplacer(selectedInst, replacer, selectedCont.PathId, selectedCont.ClassId, selectedCont.MonoId, new MemoryStream(bytes));
            }
        }

        private async Task BatchImportDump(List<AssetContainer> selection)
        {
            var selectedFolderPaths = await _dialogService.OpenFolderDialog("Select import directory");
            if (selectedFolderPaths.Length == 0) return;

            string dir = selectedFolderPaths[0];

            string? extension = await _dialogService.ShowSelectDumpWindow(false);
            if (extension == null) return;

            List<string> extensions = extension == "any" ? SelectDumpWindow.ALL_EXTENSIONS : new List<string>() { extension };

            var batchInfos = await _dialogService.ShowImportBatchWindow(Workspace, selection, dir, extensions);
            if (batchInfos != null)
            {
                foreach (var batchInfo in batchInfos)
                {
                    string selectedFilePath = batchInfo.importFile;
                    AssetContainer selectedCont = batchInfo.cont;
                    AssetsFileInstance selectedInst = selectedCont.FileInstance;

                    using (FileStream fs = File.OpenRead(selectedFilePath))
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        AssetImportExport importer = new AssetImportExport();
                        byte[]? bytes;
                        string? exceptionMessage;

                        if (selectedFilePath.EndsWith(".json"))
                        {
                            AssetTypeTemplateField tempField = Workspace.GetTemplateField(selectedCont);
                            bytes = importer.ImportJsonAsset(tempField, sr, out exceptionMessage);
                        }
                        else
                        {
                            bytes = importer.ImportTextAsset(sr, out exceptionMessage);
                        }

                        if (bytes == null)
                        {
                            await _dialogService.ShowMessageBox("Parse error", "Something went wrong when reading the dump file:\n" + exceptionMessage);
                            return;
                        }

                        IContentReplacer replacer = AssetImportExport.CreateAssetReplacer(selectedCont, bytes);
                        Workspace.AddReplacer(selectedInst, replacer, selectedCont.PathId, selectedCont.ClassId, selectedCont.MonoId, new MemoryStream(bytes));
                    }
                }
            }
        }

        private async Task SingleImportDump(List<AssetContainer> selection)
        {
            AssetContainer selectedCont = selection[0];
            AssetsFileInstance selectedInst = selectedCont.FileInstance;

            var selectedFilePaths = await _dialogService.OpenFileDialog("Open", false, new List<string> { "*.txt", "*.json" });
            if (selectedFilePaths.Length == 0) return;

            string file = selectedFilePaths[0];

            using (FileStream fs = File.OpenRead(file))
            using (StreamReader sr = new StreamReader(fs))
            {
                AssetImportExport importer = new AssetImportExport();
                byte[]? bytes = null;
                string? exceptionMessage = null;

                if (file.EndsWith(".json"))
                {
                    AssetTypeTemplateField tempField = Workspace.GetTemplateField(selectedCont);
                    bytes = importer.ImportJsonAsset(tempField, sr, out exceptionMessage);
                }
                else
                {
                    bytes = importer.ImportTextAsset(sr, out exceptionMessage);
                }

                if (bytes == null)
                {
                    await _dialogService.ShowMessageBox("Parse error", "Something went wrong when reading the dump file:\n" + exceptionMessage);
                    return;
                }

                IContentReplacer replacer = AssetImportExport.CreateAssetReplacer(selectedCont, bytes);
                Workspace.AddReplacer(selectedInst, replacer, selectedCont.PathId, selectedCont.ClassId, selectedCont.MonoId, new MemoryStream(bytes));
            }
        }
    }
}
