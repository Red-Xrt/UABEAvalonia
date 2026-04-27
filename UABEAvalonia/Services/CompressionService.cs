using System.IO;
using System.Threading.Tasks;
using AssetsTools.NET;

namespace UABEAvalonia.Services
{
    public class CompressionService : ICompressionService
    {
        public Task CompressBundleAsync(AssetBundleFile bundleFile, string outputPath, AssetBundleCompressionType compressionType, IAssetBundleCompressProgress progress = null!)
        {
            return Task.Run(() =>
            {
                using (FileStream fs = File.Open(outputPath, FileMode.Create))
                using (AssetsFileWriter w = new AssetsFileWriter(fs))
                {
                    bundleFile.Pack(w, compressionType, true, progress);
                }
            });
        }

        public Task DecompressBundleAsync(AssetBundleFile bundleFile, string outputPath)
        {
            return Task.Run(() =>
            {
                using (FileStream fs = File.Open(outputPath, FileMode.Create))
                {
                    bundleFile.Unpack(new AssetsFileWriter(fs));
                }
            });
        }

        public Task<AssetBundleFile> DecompressToMemoryAsync(AssetBundleFile bundleFile)
        {
            return Task.Run(() =>
            {
                MemoryStream ms = new MemoryStream();
                bundleFile.Unpack(new AssetsFileWriter(ms));
                ms.Position = 0;

                AssetBundleFile newBundle = new AssetBundleFile();
                newBundle.Read(new AssetsFileReader(ms));
                return newBundle;
            });
        }
    }
}
