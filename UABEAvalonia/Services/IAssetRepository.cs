using System.Collections.Generic;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace UABEAvalonia.Services
{
    public interface IAssetReadRepository
    {
        IReadOnlyList<AssetsFileInstance> ReadOnlyLoadedFiles { get; }
        IReadOnlyDictionary<AssetPPtr, AssetContainer> ReadOnlyLoadedAssets { get; }

        AssetContainer? GetAssetContainer(AssetsFileInstance file, int fileId, long pathId, bool onlyExisting = false);
        AssetTypeValueField? GetBaseField(AssetContainer cont);
    }

    public interface IAssetWriteRepository
    {
        void AddOrReplaceAsset(AssetContainer cont, AssetTypeValueField baseField);
        void AddOrReplaceAsset(AssetContainer cont, IContentReplacer replacer, System.IO.Stream previewStream);
        void RemoveAsset(AssetContainer cont);
    }
}
