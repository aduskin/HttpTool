using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpTool.Model
{
    public class History
    {
        public string sha1 { get; set; }
        public string url { get; set; }
        public string type { get; set; }
        public string encode { get; set; }
        public string[] file { get; set; }
        public string conmandstr { get; set; }
        public List<TP> conmand { get; set; }
        public List<TP> header { get; set; }
    }
    public class TP
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}
