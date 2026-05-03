using AssetsTools.NET;
using UABEAvalonia.Models;
using UABEAvalonia.Models;
using UABEAvalonia.Models.Workspace;
using UABEAvalonia.Services;
using UABEAvalonia.Services;
using AssetsTools.NET.Extra;
using System.IO;

namespace UABEAvalonia.Infrastructure.FileSystem
{
    public class TextDumpService : UABEAvalonia.Services.ITextDumpService
    {
        public void DumpTextAsset(FileStream wfs, AssetTypeValueField baseField)
        {
            using (var sw = new StreamWriter(wfs))
            {
                var exporter = new UABEAvalonia.AssetImportExport();
                exporter.DumpTextAsset(sw, baseField);
            }
        }

        public byte[] ImportTextAsset(Stream fs, out string? exceptionMessage)
        {
            using (var sr = new StreamReader(fs))
            {
                var importer = new UABEAvalonia.AssetImportExport();
                return importer.ImportTextAsset(sr, out exceptionMessage)!;
            }
        }
    }

    public class JsonDumpService : UABEAvalonia.Services.IJsonDumpService
    {
        public void DumpJsonAsset(FileStream wfs, AssetTypeValueField baseField)
        {
            using (var sw = new StreamWriter(wfs))
            {
                var exporter = new UABEAvalonia.AssetImportExport();
                exporter.DumpJsonAsset(sw, baseField);
            }
        }

        public byte[] ImportJsonAsset(AssetTypeTemplateField tempField, Stream fs, out string? exceptionMessage)
        {
            using (var sr = new StreamReader(fs))
            {
                var importer = new UABEAvalonia.AssetImportExport();
                return importer.ImportJsonAsset(tempField, sr, out exceptionMessage)!;
            }
        }
    }

    public class RawExportService : UABEAvalonia.Services.IRawExportService
    {
        public void DumpRawAsset(FileStream wfs, AssetsFileReader reader, long position, uint size)
        {
            var exporter = new UABEAvalonia.AssetImportExport();
            exporter.DumpRawAsset(wfs, reader, position, size);
        }
    }
}
