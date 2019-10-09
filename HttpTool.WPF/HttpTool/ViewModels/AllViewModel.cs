using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using HttpTool.API;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using HttpTool.Styles.MessageBox;

namespace HttpTool.ViewModels
{
    public class AllViewModel:ViewModelBase
    {
        #region 窗口全局属性
        public string AppName = "App";
        public string _ErrMsg=null;
        public Window CurrenWindow;
        private ImageBrush _backImg;
        public ImageBrush backImg
        {
            get
            {
                if (_backImg == null)
                {
                    //return new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Background/Nor.jpg"))); 
                }
                return _backImg;
            }
            set
            {
                _backImg = value;
                OnPropertyChanged("backImg");
            }
        }
        public BitmapSource backImgSour = null;



        /// <summary>
        /// 推荐加载状态
        /// </summary>
        private bool _LoadState = false;
        public bool LoadState
        {
            get
            {
                return _LoadState;
            }
            set
            {
                _LoadState = value;
                OnPropertyChanged("LoadState");
            }
        }

        /// <summary>
        /// 系统消息
        /// </summary>
        private string _SysMsg;
        public string SysMsg
        {
            get
            {
                if (_SysMsg == null || _SysMsg.Trim() == "")
                {
                    return "无任何消息！";
                }
                return _SysMsg;
            }
            set
            {
                _SysMsg = value;
                OnPropertyChanged("SysMsg");
            }
        }
        #endregion
        
        #region 主窗口事件
        //鼠标拖动
        public ICommand AduMain_MouseLeftButtonDownCommand
        {
            get
            {
                return new DelegateCommand(obj =>
                {
                    ExCommandParameter ep = (ExCommandParameter)obj;
                    Window sender = (Window)ep.Sender;
                    sender.DragMove();
                });
            }
        }
        

        /// <summary>
        /// 退出
        /// </summary>
        public ICommand AppExit
        {
            get
            {
                return new DelegateCommand(obj =>
                {
                    //if (AduMessageBox.ShowDialog("确定退出 "+ AppName + " ？", 1, backImgSour).GetValueOrDefault())
                    //{
                        Environment.Exit(0);
                    //}
                });

            }
        }

        
        /// <summary>
        /// 超链接
        /// </summary>    
        public ICommand LinkClick => new DelegateCommand(obj =>
        {
            if(obj is Hyperlink link)
            {
                Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
            }
            
        });

        #endregion
    }
}
