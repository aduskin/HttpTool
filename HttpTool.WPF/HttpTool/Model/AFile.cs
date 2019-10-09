using HttpTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpTool.Model
{
    public class AFile: ViewModelBase
    {
        private string _ID;
        /// <summary>
        /// XXX
        /// </summary>
        public string ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
                OnPropertyChanged(nameof(ID));
            }
        }
        private string _ZDDWID;
        /// <summary>
        /// XXX
        /// </summary>
        public string ZDDWID
        {
            get { return _ZDDWID; }
            set
            {
                _ZDDWID = value;
                OnPropertyChanged(nameof(ZDDWID));
            }
        }
        private string _FullPath;
        /// <summary>
        /// 全路径
        /// </summary>
        public string FullPath
        {
            get { return _FullPath; }
            set
            {
                _FullPath = value;
                OnPropertyChanged(nameof(FullPath));
            }
        }
        private string _FileName;
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get { return _FileName; }
            set
            {
                _FileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }
        private string _Extension;
        /// <summary>
        /// 
        /// </summary>
        public string Extension
        {
            get { return _Extension; }
            set
            {
                _Extension = value;
                OnPropertyChanged(nameof(Extension));
            }
        }


    }
}
