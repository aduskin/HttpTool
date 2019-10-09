using HttpTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpTool.Model
{
    public class Sys_Code:ViewModelBase
    {
        public Sys_Code()
        {
        }
        public Sys_Code(string CodeName,string CodeValue)
        {
            this.CodeName = CodeName;
            this.CodeValue = CodeValue;
        }
        private string _CodeName;
        /// <summary>
        /// XXX
        /// </summary>
        public string CodeName
        {
            get { return _CodeName; }
            set
            {
                _CodeName = value;
                OnPropertyChanged(nameof(CodeName));
            }
        }
        private string _CodeValue;
        /// <summary>
        /// XXX
        /// </summary>
        public string CodeValue
        {
            get { return _CodeValue; }
            set
            {
                _CodeValue = value;
                OnPropertyChanged(nameof(CodeValue));
            }
        }
        
    }
}
