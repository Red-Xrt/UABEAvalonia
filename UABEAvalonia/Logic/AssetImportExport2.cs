using AssetsTools.NET;
using System.IO;

namespace UABEAvalonia.Logic
{
    public static partial class AssetImportExport
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
