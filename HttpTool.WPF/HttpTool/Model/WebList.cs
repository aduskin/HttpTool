using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace HttpTool.Model
{
    public class WebList
    {
        public int StatusCode { set; get; }
        public FileExtension Extension { set; get; }
        public string ServerHeader { set; get; }
        //public string Host { set; get; }
        public string IP { set; get; }
        public string DNS { set; get; }
        public string AbsoluteUri { set; get; }
        public string Type { set; get; }
        public string Header { set; get; }
        public string Cookie { set; get; }
        public string Html { set; get; }
        public string Err { set; get; }
        public byte[] Byte { set; get; }
        public string Temp { set; get; }
        public int OriginalSize { set; get; }
        public string FileName { set; get; }
        public string SslSub { set; get; }
        public string SslUer { set; get; }
        public SslPolicyErrors SslErr { set; get; }
    }
}
