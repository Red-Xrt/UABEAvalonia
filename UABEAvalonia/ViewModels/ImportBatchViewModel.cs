using CommunityToolkit.Mvvm.ComponentModel;
using UABEAvalonia.Services;

namespace UABEAvalonia.ViewModels
{
    public partial class ImportBatchViewModel : ViewModelBase
    {
        private readonly IDialogService? _dialogService;

        public ImportBatchViewModel() { }

        public ImportBatchViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }
    }
}
