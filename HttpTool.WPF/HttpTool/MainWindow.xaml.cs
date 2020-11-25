using AduSkin.Controls.Metro;
using ChromeTabs;
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

namespace HttpTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : AduWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }
        /// <summary>
        /// 双击放大
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //long ticks = DateTime.Now.Ticks / 1000;
            //if (ticks - 2000 > lastTitleLeftDownTime)
            //{
            //    lastTitleLeftDownTime = ticks;
            //}
            //else
            //{
            //    lastTitleLeftDownTime = 0;
            //    btnMax_Click(null, null);
            //}
        }
        private void WrapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void TabControl_TabDraggedOutsideBonds(object sender, TabDragEventArgs e)
        {
            //TabBase draggedTab = e.Tab as TabBase;
            //if (TryDragTabToWindow(e.CursorPosition, draggedTab))
            //{
            //    //Set Handled to true to tell the tab control that we have dragged the tab to a window, and the tab should be closed.
            //    e.Handled = true;
            //}
        }
    }
}
