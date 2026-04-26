using System.Collections.Generic;
using System.Threading.Tasks;
using AssetsTools.NET.Extra;
using UABEAvalonia.Logic;

namespace UABEAvalonia.Services
{
    public interface IWindowService
    {
        Task OpenInfoWindow(AssetsManager assetsManager, List<AssetsFileInstance> assetsFiles, bool fromBundle);
        Task OpenAboutWindow();
        Task OpenLoadModPackageWindow(InstallerPackageFile emip, AssetsManager am);
        Task CloseAllInfoWindows();
    }
}
