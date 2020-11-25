using GalaSoft.MvvmLight.Command;

namespace HttpTool.ViewModels
{
    public interface IViewModelPinnedTabExampleWindow
    {
        RelayCommand<TabBase> PinTabCommand { get; set; }
    }
}
