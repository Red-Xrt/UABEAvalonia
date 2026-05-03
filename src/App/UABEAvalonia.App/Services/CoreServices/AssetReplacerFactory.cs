using AssetsTools.NET;
using UABEAvalonia.Models;
using UABEAvalonia.Models;
using UABEAvalonia.Models.Workspace;
using UABEAvalonia.Services;
using UABEAvalonia.Services;
using System.IO;

namespace UABEAvalonia.Services
{
    public static class AssetReplacerFactory
    {
        public static IContentReplacer CreateAssetReplacer(AssetContainer cont, byte[] data)
        {
            return new ContentReplacerFromBuffer(data);
        }

        public static IContentReplacer CreateBundleReplacerFromMemory(string name, bool isSerialized, byte[] data)
        {
            return new ContentReplacerFromBuffer(data);
        }

        public static IContentReplacer CreateBundleReplacerFromStream(string name, bool isSerialized, Stream stream, long length)
        {
            return new ContentReplacerFromStream(stream, 0, (int)length, false);
        }

        public static IContentReplacer CreateBundleRemover(string name, bool isSerialized)
        {
            return new ContentRemover();
        }
    }
}
