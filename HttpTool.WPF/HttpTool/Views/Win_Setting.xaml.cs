using AduSkin.Controls.Metro;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HttpTool.Views
{
    /// <summary>
    /// CompanyInfo.xaml 的交互逻辑
    /// </summary>
    public partial class Win_Setting : AduWindow
   {
        public Win_Setting()
        {
            InitializeComponent();
        }

        private void MainWin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void WinExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WinMax_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
            DropShadowEffect de = null;
            if (this.WindowState == WindowState.Normal)
            {
                de = new DropShadowEffect();
                this.BorderThickness = new Thickness(20);
                de.BlurRadius = 20;
                de.Opacity = .5;
                de.ShadowDepth = 0;
                this.Effect = de;
            }
            else
            {
                this.BorderThickness = new Thickness(5);
                this.Effect = null;
            }
        }

        private void WinMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
