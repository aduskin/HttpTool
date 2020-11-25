using HttpTool.API;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HttpTool.Model;
using System.Collections.ObjectModel;
using HttpTool.Views;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.IO.Compression;
using AduSkin.Controls.Metro;
using HttpTool.Styles.MessageBox;
using System.Security.Cryptography;
using System.Linq;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using System.Windows.Data;

namespace HttpTool.ViewModels
{
    public class MainWindowViewModel : ViewModelExampleBase, IViewModelMainWindow
    {
        public MainWindowViewModel()
        {
            //Adding items to the collection creates a tab
            ItemCollection.Add(CreateTab1());

            SelectedTab = ItemCollection.FirstOrDefault();
            ICollectionView view = CollectionViewSource.GetDefaultView(ItemCollection);

            //This sort description is what keeps the source collection sorted, based on tab number. 
            //You can also use the sort description to manually sort the tabs, based on your own criterias.
            view.SortDescriptions.Add(new SortDescription("TabNumber", ListSortDirection.Ascending));

            CanMoveTabs = true;
            ShowAddButton = true;
        }

        /// <summary>
        /// 关闭程序
        /// </summary>    
        public ICommand AduMain_ExiteCommand => new DelegateCommand(obj =>
        {
            Environment.Exit(0);
        });

        /// <summary>
        /// 设置
        /// </summary>    
        public ICommand OpenSetting => new DelegateCommand(obj =>
        {
            Win_Setting win = new Win_Setting();
            win.Show();
        });

        /// <summary>
        /// XXX
        /// </summary>    
        public ICommand ToAduSkinWeb => new DelegateCommand(obj =>
        {
            Process.Start(new ProcessStartInfo("https://github.com/Hero3821"));
        });

        //this property is to show you can lock the tabs with a binding
        private bool _canMoveTabs;
        public bool CanMoveTabs
        {
            get => _canMoveTabs;
            set
            {
                if (_canMoveTabs != value)
                {
                    Set(() => CanMoveTabs, ref _canMoveTabs, value);
                }
            }
        }
        //this property is to show you can bind the visibility of the add button
        private bool _showAddButton;
        public bool ShowAddButton
        {
            get => _showAddButton;
            set
            {
                if (_showAddButton != value)
                {
                    Set(() => ShowAddButton, ref _showAddButton, value);
                }
            }
        }
    }
}
