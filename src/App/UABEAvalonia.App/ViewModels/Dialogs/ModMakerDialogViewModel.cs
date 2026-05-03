using CommunityToolkit.Mvvm.ComponentModel;
using UABEAvalonia.Services;

namespace UABEAvalonia.ViewModels
{
    public partial class ModMakerDialogViewModel : ViewModelBase
    {
        private readonly IDialogService? _dialogService;

        public ModMakerDialogViewModel() { }

        public ModMakerDialogViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }
    }
}
