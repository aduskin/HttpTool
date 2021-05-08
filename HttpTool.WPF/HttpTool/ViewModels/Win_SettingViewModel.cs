using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using HttpTool.API;
using HttpTool.Styles.MessageBox;

namespace HttpTool.ViewModels
{
    public class Win_SettingViewModel : ViewModelBase
    {
        string tempPath = System.Environment.CurrentDirectory + "\\temp";
        DirectoryInfo TheFolder = null;
        FileInfo[] Files = null;

        #region 事件
        public ICommand AduMain_LoadCommand
        {
            get
            {
                return new DelegateCommand(obj =>
                {
                    BindDropdown();
                    TheFolder = new DirectoryInfo(tempPath);
                    if (TheFolder.Exists)
                    {
                        Files = TheFolder.GetFiles();
                        BarMaxValue = Files.Length;
                        CurrentValue = "剩余 " + BarMaxValue + " 个文件可清理";
                    }
                });
            }
        }

        /// <summary>
        /// 清空
        /// </summary>    
        public ICommand ClearFile => new DelegateCommand(obj =>
        {
            TheFolder = new DirectoryInfo(tempPath);
            Files = null;
            Files = TheFolder.GetFiles();
            BarMaxValue = Files.Length;
            for (int i = 0; i < Files.Length; i++)
            {
                Files[i].Delete();
                CurrentValue = "剩余" + (BarMaxValue - i) + "个文件可清理";
            }
            CurrentValue = "无文件可清理";
        });

        #endregion

        #region 页面属性
        /// <summary>
        /// 总文件数
        /// </summary>
        private int _BarMaxValue = 0;
        public int BarMaxValue
        {
            get
            {
                return _BarMaxValue;
            }
            set { Set(ref _BarMaxValue, value); }
        }
        /// <summary>
        /// 当前进度
        /// </summary>
        private string _CurrentValue;
        public string CurrentValue
        {
            get
            {
                return _CurrentValue;
            }
            set { Set(ref _CurrentValue, value); }
        }
        #endregion

        #region 获得数据
        public void BindDropdown()
        {


        }

        #endregion
    }
}
