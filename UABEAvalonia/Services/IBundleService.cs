using System.IO;
using System.Threading.Tasks;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace UABEAvalonia.Services
{
    public interface IBundleService
    {
        BundleWorkspace Workspace { get; }
        AssetsManager AssetsManager { get; }

        bool ChangesMade { get; set; }
        bool ChangesUnsaved { get; set; }

        void LoadClassPackage(string classDataPath);
        DetectedFileType DetectFileType(string filePath);

        Task<bool> LoadOrAskTypeData(AssetsFileInstance fileInst, string uVer);
        Task<BundleFileInstance> LoadBundleFile(string selectedFile);
        Task<AssetsFileInstance> LoadAssetsFile(string selectedFile);

        void ResetWorkspace(BundleFileInstance? bundleInst);

        void SaveBundle(BundleFileInstance bundleInst, string path);
        void SaveBundleOver(BundleFileInstance bundleInst);
        Task CompressBundle(BundleFileInstance bundleInst, string path, AssetBundleCompressionType compType, AssetsTools.NET.IAssetBundleCompressProgress progress = null);

        void DecompressToFile(BundleFileInstance bundleInst, string savePath);
        void DecompressToMemory(BundleFileInstance bundleInst);
        long GetBundleDataDecompressedSize(AssetsTools.NET.AssetBundleFile bundleFile);

        void AddOrReplaceFile(Stream stream, string fileName, bool isSerialized);
        void RenameFile(string oldName, string newName);
        void RemoveFile(BundleWorkspaceItem item);
    }
}
