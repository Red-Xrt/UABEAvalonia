using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace UABEAvalonia.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string title = "UABEA";

        [ObservableProperty]
        private string fileName = "No file opened.";

        [ObservableProperty]
        private ObservableCollection<BundleWorkspaceItem> files = new ObservableCollection<BundleWorkspaceItem>();

        [ObservableProperty]
        private BundleWorkspaceItem? selectedFile;

        // Add more properties and commands here later when refactoring the logic

        public MainWindowViewModel()
        {
        }
    }
}
