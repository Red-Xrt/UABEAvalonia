using System.Collections.Generic;
using UABEAvalonia.Plugins;
using AssetsTools.NET.Extra;

namespace UABEAvalonia.Services
{
    public interface IPluginService
    {
        void LoadPluginsInDirectory(string path);
        List<UABEAPluginMenuInfo> GetPluginsThatSupport(AssetsManager am, List<AssetContainer> selectedAssets);
    }
}