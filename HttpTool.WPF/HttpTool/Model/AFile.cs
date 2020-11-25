using GalaSoft.MvvmLight;
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
            set { Set(ref _ID, value); }
        }
        private string _ZDDWID;
        /// <summary>
        /// XXX
        /// </summary>
        public string ZDDWID
        {
            get { return _ZDDWID; }
            set { Set(ref _ZDDWID, value); }
        }
        private string _FullPath;
        /// <summary>
        /// 全路径
        /// </summary>
        public string FullPath
        {
            get { return _FullPath; }
            set { Set(ref _FullPath, value); }
        }
        private string _FileName;
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get { return _FileName; }
            set { Set(ref _FileName, value); }
        }
        private string _Extension;
        /// <summary>
        /// 
        /// </summary>
        public string Extension
        {
            get { return _Extension; }
            set { Set(ref _Extension, value); }
        }


    }
}
