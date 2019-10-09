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
using HttpTool.API;
using System.Threading;
namespace HttpTool.Styles.MessageBox
{
    /// <summary>
    /// MessageBoxOK.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBox_AutoShutdown : Window
    {
        /// <summary>
        /// 弹框内容
        /// </summary>
        /// <param name="Msg">消息内容</param>
        /// <param name="timeout">消失时间</param>
        /// <param name="left">距离左边</param>
        /// <param name="top">距离上边</param>
        public MessageBox_AutoShutdown(string Msg, int timeout = 3, int left = 100, int top = 100)
        {
            this.Left = left;
            this.Top = top;
            InitializeComponent();
            
               
                message.Text = Msg;
                Task.Factory.StartNew(() =>
                {
                    RUN(timeout);
                });
        }
        public void RUN(int timeout)
        {
            Thread.Sleep(timeout*1000);
            App.DispatcherHelper.Invoke(new Action(() =>
            {
                this.Close();
            }));
        }
        //public MessageBox_AutoShutdown(string mess, BitmapSource BackImgSour = null)
        //{
        //    InitializeComponent();
        //    if (BackImgSour != null)
        //    {
        //        ImageBrush img = new ImageBrush(BackImgSour);
        //        img.Stretch = Stretch.UniformToFill;
        //        this.Background = img;
        //        Title.Foreground = message.Foreground = new SolidColorBrush(Colors.White);
        //    }
        //    else
        //    {
        //        Title.Foreground = message.Foreground = new SolidColorBrush(Colors.Black);
        //    }
        //    message.Text = mess;
        //}
        private void Button_Click(object sender, RoutedEventArgs e)
        {
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
