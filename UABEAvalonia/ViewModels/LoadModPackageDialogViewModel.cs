using CommunityToolkit.Mvvm.ComponentModel;
using UABEAvalonia.Services;

namespace UABEAvalonia.ViewModels
{
    public partial class LoadModPackageDialogViewModel : ViewModelBase
    {
        private readonly IDialogService? _dialogService;

        public LoadModPackageDialogViewModel() { }

        public LoadModPackageDialogViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }
    }
}
