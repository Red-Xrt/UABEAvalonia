using System.Collections.Generic;
using System.Threading.Tasks;
using AssetsTools.NET.Extra;
using UABEAvalonia.Models;
using UABEAvalonia.Services;

namespace UABEAvalonia.Services
{
    public interface IWindowService
    {
        Task<InfoWindow> OpenInfoWindow(AssetsManager assetsManager, List<AssetsFileInstance> assetsFiles, bool fromBundle);
        Task OpenAboutWindow();
        Task OpenLoadModPackageWindow(InstallerPackageFile emip, AssetsManager am);
        Task CloseAllInfoWindows();

        Task OpenGameObjectViewWindow(AssetWorkspace workspace, AssetContainer? selectedGo = null);
        Task OpenDataWindow(AssetWorkspace workspace, AssetContainer cont);
    }
}
