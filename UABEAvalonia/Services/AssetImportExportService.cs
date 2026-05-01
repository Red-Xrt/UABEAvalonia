using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.IO;

namespace UABEAvalonia.Services
{
    public class AssetImportExportService : IAssetImportExportService
    {
        public void DumpRawAsset(FileStream wfs, AssetsFileReader reader, long position, uint size)
        {
            // Note: UABEAvalonia.Logic.AssetImportExport is actually NOT static. It takes aw in constructor.
            // For now, we will just leave these as throw NotImplementedException until we migrate the class logic inside here.
            throw new System.NotImplementedException();
        }

        public void DumpTextAsset(FileStream wfs, AssetTypeValueField baseField)
        {
            throw new System.NotImplementedException();
        }

        public void DumpJsonAsset(FileStream wfs, AssetTypeValueField baseField)
        {
            throw new System.NotImplementedException();
        }
    }
}