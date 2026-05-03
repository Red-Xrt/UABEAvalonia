using System.Threading.Tasks;
using AssetsTools.NET;

namespace UABEAvalonia.Services
{
    public interface ICompressionService
    {
        Task CompressBundleAsync(AssetBundleFile bundleFile, string outputPath, AssetBundleCompressionType compressionType, IAssetBundleCompressProgress progress = null!);
        Task DecompressBundleAsync(AssetBundleFile bundleFile, string outputPath);
        Task<AssetBundleFile> DecompressToMemoryAsync(AssetBundleFile bundleFile);
    }
}
