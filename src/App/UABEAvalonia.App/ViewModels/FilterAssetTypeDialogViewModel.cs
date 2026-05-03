using CommunityToolkit.Mvvm.ComponentModel;
using UABEAvalonia.Services;

namespace UABEAvalonia.ViewModels
{
    public partial class FilterAssetTypeDialogViewModel : ViewModelBase
    {
        private readonly IDialogService? _dialogService;

        public FilterAssetTypeDialogViewModel() { }

        public FilterAssetTypeDialogViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }
    }
}
