using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.IO;

namespace UABEAvalonia.Services
{
    public interface IAssetImportExportService
    {
        void DumpRawAsset(FileStream wfs, AssetsFileReader reader, long position, uint size);
        void DumpTextAsset(FileStream wfs, AssetTypeValueField baseField);
        void DumpJsonAsset(FileStream wfs, AssetTypeValueField baseField);
    }
}