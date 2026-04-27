using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.IO;

namespace UABEAvalonia.Services
{
    public interface ITextDumpService
    {
        void DumpTextAsset(FileStream wfs, AssetTypeValueField baseField);
        byte[] ImportTextAsset(Stream fs, out string? exceptionMessage);
    }

    public interface IJsonDumpService
    {
        void DumpJsonAsset(FileStream wfs, AssetTypeValueField baseField);
        byte[] ImportJsonAsset(Stream fs, out string? exceptionMessage);
    }

    public interface IRawExportService
    {
        void DumpRawAsset(FileStream wfs, AssetsFileReader reader, long position, uint size);
    }
}
