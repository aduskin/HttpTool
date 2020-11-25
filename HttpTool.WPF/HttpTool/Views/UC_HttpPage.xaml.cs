using HttpTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HttpTool.Views
{
    /// <summary>
    /// UC_HttpPage.xaml 的交互逻辑
    /// </summary>
    public partial class UC_HttpPage : UserControl
    {
        public UC_HttpPage()
        {
            InitializeComponent();
        }

        private void AduUpload_FileAduUpload(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            HttpPageViewModel vm = this.DataContext as HttpPageViewModel;
            vm.FileUploadCommand.Execute(e.NewValue);
        }
    }
}
