using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UABEAvalonia.Services;
using AssetsTools.NET.Extra;
using System.Collections.Generic;
using UABEAvalonia.Logic;

namespace UABEAvalonia.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IBundleService _bundleService;

        [ObservableProperty]
        private string title = "UABEA";

        [ObservableProperty]
        private string fileName = "No file opened.";

        [ObservableProperty]
        private ObservableCollection<BundleWorkspaceItem> files = new ObservableCollection<BundleWorkspaceItem>();

        [ObservableProperty]
        private BundleWorkspaceItem? selectedFile;

        [ObservableProperty]
        private bool isWorkspaceActive;

        [ObservableProperty]
        private bool hasAssets;

        public System.Action<AssetsTools.NET.Extra.AssetsManager, List<AssetsTools.NET.Extra.AssetsFileInstance>, bool>? RequestOpenInfoWindowAction { get; set; }
        public System.Func<Task>? RequestCloseAllInfoWindowsAction { get; set; }

        public MainWindowViewModel(IDialogService dialogService, IBundleService bundleService)
        {
            _dialogService = dialogService;
            _bundleService = bundleService;
        }

        [RelayCommand]
        public async Task OpenFileAsync()
        {
            var selectedFiles = await _dialogService.OpenFileDialog("Open assets or bundle file", true, new List<string> { "*" });
            if (selectedFiles.Length == 0)
            {
                return;
            }

            await OpenFilesInternal(selectedFiles);
        }

        private async Task OpenFilesInternal(string[] files)
        {
            string selectedFile = files[0];

            DetectedFileType fileType = _bundleService.DetectFileType(selectedFile);

            if (RequestCloseAllInfoWindowsAction != null)
            {
                await RequestCloseAllInfoWindowsAction();
            }

            _bundleService.ResetWorkspace(null); // Unload all first
            _bundleService.AssetsManager.UnloadAllAssetsFiles(true);
            _bundleService.AssetsManager.UnloadAllBundleFiles();

            IsWorkspaceActive = false;
            HasAssets = false;
            FileName = "No file opened.";
            Files.Clear();

            if (fileType != DetectedFileType.Unknown)
            {
                if (selectedFile.EndsWith(".split0"))
                {
                    string? splitFilePath = await _dialogService.AskLoadSplitFile(selectedFile);
                    if (splitFilePath == null)
                        return;
                    else
                        selectedFile = splitFilePath;
                }
            }

            if (fileType == DetectedFileType.AssetsFile)
            {
                AssetsFileInstance fileInst = _bundleService.LoadAssetsFile(selectedFile);

                if (!await _bundleService.LoadOrAskTypeData(fileInst, "0.0.0")) // Simple pass for now, will be updated to ask properly
                {
                    string uVer = fileInst.file.Metadata.UnityVersion;
                    if (uVer == "0.0.0" && fileInst.parentBundle != null)
                    {
                        uVer = fileInst.parentBundle.file.Header.EngineVersion;
                    }
                    if (uVer == "0.0.0")
                    {
                        uVer = await _dialogService.AskForVersion(uVer);
                        if (uVer == string.Empty)
                        {
                            if (!fileInst.file.Metadata.TypeTreeEnabled)
                            {
                                await _dialogService.ShowMessageBox("Error", "You must enter a Unity version to load a typetree-stripped file.");
                                return;
                            }
                            else
                            {
                                uVer = "0.0.0";
                            }
                        }
                    }
                    await _bundleService.LoadOrAskTypeData(fileInst, uVer);
                }

                List<AssetsFileInstance> fileInstances = new List<AssetsFileInstance>();
                fileInstances.Add(fileInst);

                if (files.Length > 1)
                {
                    for (int i = 1; i < files.Length; i++)
                    {
                        string otherSelectedFile = files[i];
                        DetectedFileType otherFileType = _bundleService.DetectFileType(otherSelectedFile);
                        if (otherFileType == DetectedFileType.AssetsFile)
                        {
                            try
                            {
                                fileInstances.Add(_bundleService.LoadAssetsFile(otherSelectedFile));
                            }
                            catch { }
                        }
                    }
                }

                RequestOpenInfoWindowAction?.Invoke(_bundleService.AssetsManager, fileInstances, false);
            }
            else if (fileType == DetectedFileType.BundleFile)
            {
                BundleFileInstance bundleInst = _bundleService.LoadBundleFile(selectedFile);

                if (AssetBundleUtil.IsBundleDataCompressed(bundleInst.file))
                {
                    string decompSize = FileUtils.GetFormattedByteSize(_bundleService.GetBundleDataDecompressedSize(bundleInst.file));
                    string result = await _dialogService.ShowCustomMessageBox("Note", "This bundle is compressed. Decompress to file or memory?\nSize: " + decompSize, "File", "Memory", "Cancel");

                    if (result == "File")
                    {
                        string? selectedFilePath = await _dialogService.SaveFileDialog("Save as...", "", new List<string>{"*"});
                        if (selectedFilePath != null)
                        {
                            _bundleService.DecompressToFile(bundleInst, selectedFilePath);
                        }
                        else return;
                    }
                    else if (result == "Memory")
                    {
                        if (bundleInst.file.DataIsCompressed)
                        {
                            _bundleService.DecompressToMemory(bundleInst);
                        }
                    }
                    else return;
                }

                _bundleService.ResetWorkspace(bundleInst);
                FileName = bundleInst.name;
                Files.Clear();
                foreach (var item in _bundleService.Workspace.Files) Files.Add(item);
                if (Files.Count > 0) SelectedFile = Files[0];

                IsWorkspaceActive = true;
                HasAssets = Files.Count > 0;
            }
            else
            {
                await _dialogService.ShowMessageBox("Error", "This doesn't seem to be an assets file or bundle.");
            }
        }

        [RelayCommand]
        public async Task OpenInfoAsync()
        {
            if (SelectedFile == null) return;

            string name = SelectedFile.Name;
            var BundleInst = _bundleService.Workspace.BundleInst;

            if (BundleInst == null) return;

            AssetsTools.NET.AssetBundleFile bundleFile = BundleInst.file;
            System.IO.Stream assetStream = SelectedFile.Stream;

            DetectedFileType fileType = _bundleService.DetectFileType(SelectedFile.Name); // Approximation for standard logic inside stream
            assetStream.Position = 0;

            if (fileType == DetectedFileType.AssetsFile || fileType == DetectedFileType.Unknown) // Unknown fallback since we can't fully rely on extension here
            {
                // To safely implement this within ViewModel limits, we request an action
                // Real application would abstract the instantiation process directly into the BundleService.

                string assetMemPath = System.IO.Path.Combine(BundleInst.path, name);
                AssetsFileInstance fileInst = _bundleService.AssetsManager.LoadAssetsFile(assetStream, assetMemPath, true);

                if (BundleInst != null && fileInst.parentBundle == null)
                    fileInst.parentBundle = BundleInst;

                if (!await _bundleService.LoadOrAskTypeData(fileInst, "0.0.0"))
                    return;

                RequestOpenInfoWindowAction?.Invoke(_bundleService.AssetsManager, new List<AssetsFileInstance> { fileInst }, true);
            }
            else
            {
                if (SelectedFile.IsSerialized)
                {
                    await _dialogService.ShowMessageBox("Error", "This doesn't seem to be a valid assets file, although the asset is serialized.");
                }
                else
                {
                    await _dialogService.ShowMessageBox("Error", "This doesn't seem to be a valid assets file. Use Export.");
                }
            }
        }

        [RelayCommand]
        public async Task ExportAsync()
        {
            if (SelectedFile == null) return;
            string? selectedFilePath = await _dialogService.SaveFileDialog("Save as...", SelectedFile.Name, new List<string> { "*" });
            if (selectedFilePath == null) return;

            using System.IO.FileStream fileStream = System.IO.File.Open(selectedFilePath, System.IO.FileMode.Create);
            System.IO.Stream stream = SelectedFile.Stream;
            stream.Position = 0;
            stream.CopyToCompat(fileStream, stream.Length);
        }

        [RelayCommand]
        public async Task ExportAllAsync()
        {
            if (_bundleService.Workspace.BundleInst == null) return;

            string[]? selectedFolderPaths = await _dialogService.OpenFolderDialog("Select export directory");
            if (selectedFolderPaths.Length == 0) return;

            string dir = selectedFolderPaths[0];

            var BundleInst = _bundleService.Workspace.BundleInst;
            for (int i = 0; i < BundleInst.file.BlockAndDirInfo.DirectoryInfos.Count; i++)
            {
                var dirInf = BundleInst.file.BlockAndDirInfo.DirectoryInfos[i];
                string bunAssetPath = System.IO.Path.Combine(dir, dirInf.Name);

                if (dirInf.Name.Contains("\\") || dirInf.Name.Contains("/"))
                {
                    string bunAssetDir = System.IO.Path.GetDirectoryName(bunAssetPath)!;
                    if (!System.IO.Directory.Exists(bunAssetDir))
                    {
                        System.IO.Directory.CreateDirectory(bunAssetDir);
                    }
                }

                using System.IO.FileStream fileStream = System.IO.File.Open(bunAssetPath, System.IO.FileMode.Create);
                AssetsTools.NET.AssetsFileReader bundleReader = BundleInst.file.DataReader;
                bundleReader.Position = dirInf.Offset;
                bundleReader.BaseStream.CopyToCompat(fileStream, dirInf.DecompressedSize);
            }
        }

        [RelayCommand]
        public async Task ImportAsync()
        {
            if (_bundleService.Workspace.BundleInst == null) return;

            string[] selectedFilePaths = await _dialogService.OpenFileDialog("Open", false, new List<string> { "*" });
            if (selectedFilePaths.Length == 0) return;

            string file = selectedFilePaths[0];
            bool isSerialized = await _dialogService.AskForImportSerialized();

            byte[] fileBytes = System.IO.File.ReadAllBytes(file);
            string fileName = System.IO.Path.GetFileName(file);

            System.IO.MemoryStream stream = new System.IO.MemoryStream(fileBytes);
            _bundleService.AddOrReplaceFile(stream, fileName, isSerialized);

            Files.Clear();
            foreach (var item in _bundleService.Workspace.Files) Files.Add(item);
            SelectedFile = Files[Files.Count - 1];
            HasAssets = Files.Count > 0;
        }

        [RelayCommand]
        public async Task ImportAllAsync()
        {
            if (_bundleService.Workspace.BundleInst == null) return;

            string[]? selectedFolderPaths = await _dialogService.OpenFolderDialog("Select import directory");
            if (selectedFolderPaths.Length == 0) return;

            string dir = selectedFolderPaths[0];

            foreach (string filePath in System.IO.Directory.EnumerateFiles(dir, "*", System.IO.SearchOption.AllDirectories))
            {
                string relPath = System.IO.Path.GetRelativePath(dir, filePath);
                relPath = relPath.Replace("\\", "/").TrimEnd('/');

                BundleWorkspaceItem? itemToReplace = null;
                foreach (var f in _bundleService.Workspace.Files)
                {
                    if (f.Name == relPath) itemToReplace = f;
                }

                if (itemToReplace != null)
                {
                    _bundleService.AddOrReplaceFile(System.IO.File.OpenRead(filePath), itemToReplace.Name, itemToReplace.IsSerialized);
                }
                else
                {
                    DetectedFileType type = _bundleService.DetectFileType(filePath);
                    bool isSerialized = type == DetectedFileType.AssetsFile;
                    _bundleService.AddOrReplaceFile(System.IO.File.OpenRead(filePath), relPath, isSerialized);
                }
            }

            Files.Clear();
            foreach (var item in _bundleService.Workspace.Files) Files.Add(item);
            if (Files.Count > 0) SelectedFile = Files[0];
            HasAssets = Files.Count > 0;
        }

        [RelayCommand]
        public void Remove()
        {
            if (_bundleService.Workspace.BundleInst != null && SelectedFile != null)
            {
                _bundleService.RemoveFile(SelectedFile);
                Files.Remove(SelectedFile);
                if (Files.Count > 0) SelectedFile = Files[0];
                HasAssets = Files.Count > 0;
            }
        }

        [RelayCommand]
        public async Task RenameAsync()
        {
            if (_bundleService.Workspace.BundleInst == null || SelectedFile == null) return;

            string newName = await _dialogService.AskForRename(SelectedFile.Name);
            if (newName == string.Empty) return;

            _bundleService.RenameFile(SelectedFile.Name, newName);

            // Refresh UI
            var currentFile = SelectedFile;
            Files.Clear();
            foreach (var item in _bundleService.Workspace.Files) Files.Add(item);
            SelectedFile = currentFile;
        }
    }
}
