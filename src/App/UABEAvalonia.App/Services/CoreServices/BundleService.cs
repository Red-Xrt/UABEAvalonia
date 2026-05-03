using AssetsTools.NET;
using UABEAvalonia.Models;
using UABEAvalonia.Models;
using UABEAvalonia.Models.Workspace;
using UABEAvalonia.Services;
using UABEAvalonia.Services;
using AssetsTools.NET.Extra;
using System.IO;
using System.Threading.Tasks;

namespace UABEAvalonia.Services
{
    public class BundleService : IBundleService
    {
        private readonly ICompressionService _compressionService;

        public BundleWorkspace Workspace { get; private set; }
        public AssetsManager AssetsManager => Workspace.am;

        public bool ChangesMade { get; set; }
        public bool ChangesUnsaved { get; set; }

        public BundleService(ICompressionService compressionService)
        {
            Workspace = new BundleWorkspace();
            _compressionService = compressionService;
        }

        public void LoadClassPackage(string classDataPath)
        {
            if (File.Exists(classDataPath))
            {
                AssetsManager.LoadClassPackage(classDataPath);
            }
        }

        public DetectedFileType DetectFileType(string filePath)
        {
            return FileTypeDetector.DetectFileType(filePath);
        }

        public async Task<bool> LoadOrAskTypeData(AssetsFileInstance fileInst, string uVer)
        {
            if (uVer == "0.0.0" && fileInst.parentBundle != null)
            {
                uVer = fileInst.parentBundle.file.Header.EngineVersion;
            }

            if (uVer == "0.0.0")
            {
                // Defer to dialog service in consumer
                return false;
            }

            if (AssetsManager.ClassPackage != null)
            {
                AssetsManager.LoadClassDatabaseFromPackage(uVer);
                return true;
            }
            return false;
        }

        public async Task<BundleFileInstance> LoadBundleFile(string selectedFile)
        {
            return await Task.Run(() => AssetsManager.LoadBundleFile(selectedFile, false));
        }

        public async Task<AssetsFileInstance> LoadAssetsFile(string selectedFile)
        {
            return await Task.Run(() => AssetsManager.LoadAssetsFile(selectedFile, true));
        }

        public void ResetWorkspace(BundleFileInstance? bundleInst)
        {
            Workspace.Reset(bundleInst);
        }

        public void SaveBundle(BundleFileInstance bundleInst, string path)
        {
            Workspace.ApplyChanges();
            using (FileStream fs = File.Open(path, FileMode.Create))
            using (AssetsFileWriter w = new AssetsFileWriter(fs))
            {
                bundleInst.file.Write(w);
            }
            ChangesUnsaved = false;
        }

        public void SaveBundleOver(BundleFileInstance bundleInst)
        {
            string newName = "~" + bundleInst.name;
            string dir = Path.GetDirectoryName(bundleInst.path)!;
            string filePath = Path.Combine(dir, newName);
            string origFilePath = bundleInst.path;

            SaveBundle(bundleInst, filePath);

            bundleInst.file.Reader.Close();
            File.Delete(origFilePath);
            File.Move(filePath, origFilePath);
            bundleInst.file = new AssetBundleFile();
            bundleInst.file.Read(new AssetsFileReader(File.OpenRead(origFilePath)));

            ResetWorkspace(bundleInst);
        }

        public Task CompressBundle(BundleFileInstance bundleInst, string path, AssetBundleCompressionType compType, AssetsTools.NET.IAssetBundleCompressProgress progress = null!)
        {
            return _compressionService.CompressBundleAsync(bundleInst.file, path, compType, progress);
        }

        public async void DecompressToFile(BundleFileInstance bundleInst, string savePath)
        {
            await _compressionService.DecompressBundleAsync(bundleInst.file, savePath);

            // Re-read file to replace instance
            AssetBundleFile newBundle = new AssetBundleFile();
            newBundle.Read(new AssetsFileReader(File.OpenRead(savePath)));

            bundleInst.file.Close();
            bundleInst.file = newBundle;
        }

        public async void DecompressToMemory(BundleFileInstance bundleInst)
        {
            AssetBundleFile newBundle = await _compressionService.DecompressToMemoryAsync(bundleInst.file);
            bundleInst.file.Close();
            bundleInst.file = newBundle;
        }

        public long GetBundleDataDecompressedSize(AssetBundleFile bundleFile)
        {
            long totalSize = 0;
            foreach (AssetBundleDirectoryInfo dirInf in bundleFile.BlockAndDirInfo.DirectoryInfos)
            {
                totalSize += dirInf.DecompressedSize;
            }
            return totalSize;
        }

        public void AddOrReplaceFile(Stream stream, string fileName, bool isSerialized)
        {
            Workspace.AddOrReplaceFile(stream, fileName, isSerialized);
            ChangesMade = true;
            ChangesUnsaved = true;
        }

        public void RenameFile(string oldName, string newName)
        {
            Workspace.RenameFile(oldName, newName);
            ChangesMade = true;
            ChangesUnsaved = true;
        }

        public void RemoveFile(BundleWorkspaceItem item)
        {
            item.IsRemoved = true;
            Workspace.RemovedFiles.Add(item.OriginalName);
            Workspace.Files.Remove(item);
            Workspace.FileLookup.Remove(item.Name);
            ChangesMade = true;
            ChangesUnsaved = true;
        }
    }
}
