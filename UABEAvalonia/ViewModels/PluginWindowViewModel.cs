using CommunityToolkit.Mvvm.ComponentModel;
using UABEAvalonia.Services;

namespace UABEAvalonia.ViewModels
{
    public partial class PluginWindowViewModel : ViewModelBase
    {
        private readonly IDialogService? _dialogService;

        public PluginWindowViewModel() { }

        public PluginWindowViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }
    }
}
