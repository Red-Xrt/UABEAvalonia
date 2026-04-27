using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.IO;

namespace UABEAvalonia.Services
{
    public class TextDumpService : ITextDumpService
    {
        public void DumpTextAsset(FileStream wfs, AssetTypeValueField baseField)
        {
            throw new System.NotImplementedException();
        }

        public byte[] ImportTextAsset(Stream fs, out string? exceptionMessage)
        {
            exceptionMessage = null;
            return null; // Stub
        }
    }

    public class JsonDumpService : IJsonDumpService
    {
        public void DumpJsonAsset(FileStream wfs, AssetTypeValueField baseField)
        {
            throw new System.NotImplementedException();
        }

        public byte[] ImportJsonAsset(Stream fs, out string? exceptionMessage)
        {
            exceptionMessage = null;
            return null; // Stub
        }
    }

    public class RawExportService : IRawExportService
    {
        public void DumpRawAsset(FileStream wfs, AssetsFileReader reader, long position, uint size)
        {
            throw new System.NotImplementedException();
        }
    }
}
