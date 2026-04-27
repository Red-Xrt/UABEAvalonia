using CommunityToolkit.Mvvm.ComponentModel;
using UABEAvalonia.Services;

namespace UABEAvalonia.ViewModels
{
    public partial class GameObjectViewWindowViewModel : ViewModelBase
    {
        private readonly IDialogService? _dialogService;

        public GameObjectViewWindowViewModel() { }

        public GameObjectViewWindowViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }
    }
}
