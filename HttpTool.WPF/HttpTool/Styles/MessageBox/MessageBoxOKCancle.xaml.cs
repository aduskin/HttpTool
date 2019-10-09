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
using System.Windows.Shapes;

namespace HttpTool.Styles.MessageBox
{
    /// <summary>
    /// MessageBoxOKCancle.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxOKCancle : Window
    {

        public MessageBoxOKCancle()
        {
            InitializeComponent();
        }

        public MessageBoxOKCancle(string mess, BitmapSource BackImgSour = null)
        {
            InitializeComponent();
            if (BackImgSour != null)
            {
                ImageBrush img = new ImageBrush(BackImgSour);
                img.Stretch = Stretch.UniformToFill;
                this.Background = img;
                Title.Foreground = message.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                Title.Foreground=message.Foreground =new SolidColorBrush(Colors.Black);
            }
           
            message.Text = mess;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            //如果window不是以Showdialog方式打开设置DialogResult时会引发异常
            try
            {
                this.DialogResult = true;
            }
            catch (Exception ex) { }
            this.Close();
        }
        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            //如果window不是以Showdialog方式打开设置DialogResult时会引发异常
            try
            {
                this.DialogResult = false;
            }
            catch (Exception ex) { }
            this.Close();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
