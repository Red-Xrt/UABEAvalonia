using System.ComponentModel;
using AssetsTools.NET;
using UABEAvalonia.Models;
using UABEAvalonia.Models;
using UABEAvalonia.Models.Workspace;
using UABEAvalonia.Services;
using UABEAvalonia.Services;
using AssetsTools.NET.Extra;

namespace UABEAvalonia.Models
{
    public class AssetInfoDataGridItem : INotifyPropertyChanged
    {
        public AssetClassID TypeClass { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Container { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int TypeID { get; set; }
        public int FileID { get; set; }
        public long PathID { get; set; }
        public uint Size { get; set; }
        public string Modified { get; set; } = string.Empty;

        public UABEAvalonia.AssetContainer AssetContainer { get; set; } = null!;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Update(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
