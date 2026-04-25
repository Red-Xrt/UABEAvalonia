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
        BundleFileInstance LoadBundleFile(string selectedFile);
        AssetsFileInstance LoadAssetsFile(string selectedFile);

        void ResetWorkspace(BundleFileInstance? bundleInst);

        void SaveBundle(BundleFileInstance bundleInst, string path);
        void SaveBundleOver(BundleFileInstance bundleInst);
        Task CompressBundle(BundleFileInstance bundleInst, string path, AssetBundleCompressionType compType);

        void DecompressToFile(BundleFileInstance bundleInst, string savePath);
        void DecompressToMemory(BundleFileInstance bundleInst);
        long GetBundleDataDecompressedSize(AssetsTools.NET.AssetBundleFile bundleFile);

        void AddOrReplaceFile(Stream stream, string fileName, bool isSerialized);
        void RenameFile(string oldName, string newName);
        void RemoveFile(BundleWorkspaceItem item);
    }
}
