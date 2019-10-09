
using HttpTool.API;
using HttpTool.Model;
using HttpTool.Styles.MessageBox;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HttpTool.ViewModels
{
    public class Win_ResponseViewModel : AllViewModel
    {
        #region 事件

        /// <summary>
        /// 加载事件
        /// </summary>    
        public ICommand AduMain_LoadCommand => new DelegateCommand(obj =>
        {
            HtmlText = "Html";
            if (WinAPI.CurrentReturnWeb != null && !string.IsNullOrEmpty(WinAPI.Speed))
            {
                CurrentReturnWeb = WinAPI.CurrentReturnWeb;
                Speed = WinAPI.Speed;

                StateColor = Color.FromArgb(1, 0, 255, 0);
                //状态色
                if (CurrentReturnWeb.StatusCode >= 200 && CurrentReturnWeb.StatusCode < 300)
                {
                    StateColor = Color.FromArgb(0, 255, 128);//绿
                }
                else if (CurrentReturnWeb.StatusCode >= 300 && CurrentReturnWeb.StatusCode < 400)
                {
                    StateColor = Color.FromArgb(233, 240, 29);//黄
                }
                else
                {
                    StateColor = Color.FromArgb(240, 27, 45);//红
                }
                StateBrush = new SolidBrush(StateColor);
                if (CurrentReturnWeb.SslSub != null || CurrentReturnWeb.SslUer != null)
                {
                    string _Ssl = "未知";

                    if (CurrentReturnWeb.SslSub.StartsWith("O="))
                    {
                        string _O = CurrentReturnWeb.SslSub.Substring(2).Trim();
                        if (_O.StartsWith("\""))
                        {
                            _O = _O.Substring(1);
                            _Ssl = _O.Substring(0, _O.IndexOf("\""));
                        }
                        else
                        {
                            _Ssl = _O.Substring(0, _O.IndexOf(","));
                        }
                    }
                    if (CurrentReturnWeb.SslSub.Contains(" O="))
                    {
                        string _O = CurrentReturnWeb.SslSub.Substring(CurrentReturnWeb.SslSub.IndexOf(" O=") + 3).Trim();
                        if (_O.StartsWith("\""))
                        {
                            _O = _O.Substring(1);
                            _Ssl = _O.Substring(0, _O.IndexOf("\""));
                        }
                        else
                        {
                            _Ssl = _O.Substring(0, _O.IndexOf(","));
                        }
                    }
                    else
                    {
                        _Ssl = CurrentReturnWeb.SslSub.Substring(CurrentReturnWeb.SslSub.IndexOf("CN=") + 3).Trim();
                        if (_Ssl.Contains(", "))
                        {
                            _Ssl = _Ssl.Substring(0, _Ssl.IndexOf(", "));
                        }
                    }
                    if (_Ssl.StartsWith("*."))
                    {
                        _Ssl = _Ssl.Substring(2);
                    }

                    switch (CurrentReturnWeb.SslErr)
                    {
                        case System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable:
                            _Ssl += " 证书不可用";
                            //证书不可用
                            break;
                        case System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch:
                            _Ssl += " 证书名称不匹配";
                            //证书名称不匹配
                            break;
                        case System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors:
                            _Ssl += " 异常证书";
                            //异常证书（空证书）
                            break;
                        default: break;
                    }
                    SSL = _Ssl;
                }

                if (CurrentReturnWeb.Byte != null)
                {
                    if (CurrentReturnWeb.OriginalSize > 0)
                    {
                        ReturnTitle = $"耗时{Speed} | 数据包大小{ToolClass.CountSize(CurrentReturnWeb.Byte.Length)} | 原始数据包大小{ToolClass.CountSize(CurrentReturnWeb.OriginalSize)}";
                        //this.Text = string.Format("耗时 {0} | 数据包大小 {2} | 原始数据包大小{1}", speed, CountSize(CurrentReturnWeb.Byte.Length), CountSize(CurrentReturnWeb.OriginalSize));
                    }
                    else
                    {
                        ReturnTitle = $"耗时 {Speed} | 数据包大小 {ToolClass.CountSize(CurrentReturnWeb.Byte.Length)}";
                    }
                }
                else
                {
                    ReturnTitle = $"耗时 {Speed} s";
                }
                //其他信息
                Server = CurrentReturnWeb.ServerHeader;
                IP = CurrentReturnWeb.IP;
                DNS = CurrentReturnWeb.DNS;
                AUrl = CurrentReturnWeb.AbsoluteUri;
                Header = CurrentReturnWeb.Header;
                Cookie = CurrentReturnWeb.Cookie;
                //请求内容
                if (CurrentReturnWeb.Extension == FileExtension.JPG || CurrentReturnWeb.Extension == FileExtension.GIF || CurrentReturnWeb.Extension == FileExtension.PNG || CurrentReturnWeb.Extension == FileExtension.ICO || CurrentReturnWeb.Extension == FileExtension.BMP)
                {
                    using (MemoryStream ms = new MemoryStream(CurrentReturnWeb.Byte))
                    {
                        //Image img = new Image();
                        //RequestContent=Image.FromStream(ms);
                        //PictureBox pictureBox = new PictureBox();
                        //pictureBox.Dock = DockStyle.Fill;
                        //this.Controls.Add(pictureBox);
                        //pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                        //pictureBox.Image = Image.FromStream(ms);
                        //pictureBox.Dock = DockStyle.Fill;
                        //TextPanel.Controls.Add(pictureBox);
                    }
                    //link_html.Text = "Img";
                }
                else if (CurrentReturnWeb.Extension == FileExtension.MP3 || CurrentReturnWeb.Extension == FileExtension.WAV)
                {
                   HtmlText = "Music";
                }
                else if (CurrentReturnWeb.Extension == FileExtension.EXE_DLL_COM)
                {
                   HtmlText = "Exe_Dll";
                }
                else if (CurrentReturnWeb.Extension == FileExtension.ZIP)
                {
                   HtmlText = "Zip";
                }
                else if (CurrentReturnWeb.Extension == FileExtension._7Z)
                {
                   HtmlText = "7z";
                }
                else if (CurrentReturnWeb.Extension == FileExtension.GZIP)
                {
                   HtmlText = "GZip";
                }
                else if (CurrentReturnWeb.Extension == FileExtension.PDF)
                {
                   HtmlText = "Pdf";
                }
                else if (CurrentReturnWeb.Extension == FileExtension.CHM)
                {
                   HtmlText = "Chm";
                }
                else { Init_Txt(); }
            }
        });

        //Scintilla TextArea;
        void Init_Txt()
        {
            if (!string.IsNullOrEmpty(CurrentReturnWeb.Html))
            {

                RequestContent = CurrentReturnWeb.Html;
            }
        }
        /// <summary>
        /// 打开浏览器
        /// </summary>    
        public ICommand OpenHtmlToBrowser => new DelegateCommand(obj =>
        {
            AduMessageBox.ShowAutoClose("从浏览器中打开");
            if (CurrentReturnWeb.Extension == FileExtension.JPG || CurrentReturnWeb.Extension == FileExtension.GIF || CurrentReturnWeb.Extension == FileExtension.PNG || CurrentReturnWeb.Extension == FileExtension.ICO || CurrentReturnWeb.Extension == FileExtension.BMP)
            {
                using (MemoryStream ms = new MemoryStream(CurrentReturnWeb.Byte))
                {
                    //try { Clipboard.SetImage(Image.FromStream(ms)); } catch { }
                }
            }
            else if (CurrentReturnWeb.Extension == FileExtension.MP3 || CurrentReturnWeb.Extension == FileExtension.WAV)
            {
                //try { Clipboard.SetAudio(CurrentReturnWeb.Byte); } catch { }
            }
            else if (CurrentReturnWeb.Extension == FileExtension.EXE_DLL_COM || CurrentReturnWeb.Extension == FileExtension.ZIP || CurrentReturnWeb.Extension == FileExtension._7Z || CurrentReturnWeb.Extension == FileExtension.GZIP || CurrentReturnWeb.Extension == FileExtension.PDF || CurrentReturnWeb.Extension == FileExtension.CHM)
            {
                string _FileName = CurrentReturnWeb.FileName;

                if (_FileName == null)
                {
                    _FileName = Path.GetFileName(GetName(CurrentReturnWeb.AbsoluteUri));
                }
                string _Extension = Path.GetExtension(_FileName);
                //using (SaveFileDialog _save = new SaveFileDialog())
                //{
                //    _save.FileName = _FileName;
                //    _save.AddExtension = false;
                //    _save.DefaultExt = _Extension;
                //    if (_save.ShowDialog() == DialogResult.OK)
                //    {
                //        File.WriteAllBytes(_save.FileName, CurrentReturnWeb.Byte);
                //    }
                //}
            }
            else
            {
                try
                {
                    if (HtmlText == "Html")
                    {
                        //string temp = Environment.GetEnvironmentVariable("TEMP");
                        //string path = temp + "\\" + Guid.NewGuid().ToString() + ".html";
                        //File.WriteAllBytes(path, CurrentReturnWeb.Byte);
                        //System.Diagnostics.Process.Start(path);
                    }
                    else
                    {
                        //Clipboard.SetText(CurrentReturnWeb.Html);
                    }
                }
                catch { }
            }
        });
        string GetName(string str)
        {
            if (str.EndsWith("?") || str.EndsWith("/") || str.EndsWith("#") || str.EndsWith("&") || str.EndsWith("."))
            {
                return str.Substring(0, str.Length - 1);
            }
            else if (str.Contains("?"))
            {
                return str.Substring(0, str.IndexOf("?"));
            }
            else if (str.Contains("#"))
            {
                return str.Substring(0, str.IndexOf("#"));
            }
            else { return str; }
        }


        /// <summary>
        /// 将请求头添加值请求列表
        /// </summary>    
        public ICommand ToHeader => new DelegateCommand(obj =>
        {
            Broadcast.PushBroadcast("RequestHeader", Header);
        });

        /// <summary>
        /// 将地址返回到请求栏地址
        /// </summary>    
        public ICommand ToUrl => new DelegateCommand(obj =>
        {
            Broadcast.PushBroadcast("RequestUrl", AUrl);
        });

        #endregion

        #region 属性
        private Color _StateColor;//= Color.FromArgb(1,0,256,0));
        /// <summary>
        /// 状态色
        /// </summary>
        public Color StateColor
        {
            get { return _StateColor; }
            set
            {
                _StateColor = value;
                OnPropertyChanged(nameof(StateColor));
            }
        }
        private Brush _StateBrush=new SolidBrush(Color.White);
        /// <summary>
        /// 状态色
        /// </summary>
        public Brush StateBrush
        {
            get { return _StateBrush; }
            set
            {
                _StateBrush = value;
                OnPropertyChanged(nameof(StateBrush));
            }
        }

        private string _ReturnTitle;
        /// <summary>
        /// 返回结果标题
        /// </summary>
        public string ReturnTitle
        {
            get { return _ReturnTitle; }
            set
            {
                _ReturnTitle = value;
                OnPropertyChanged(nameof(ReturnTitle));
            }
        }

        private string _HtmlText;
        /// <summary>
        /// 返回结果标题
        /// </summary>
        public string HtmlText
        {
            get { return _HtmlText; }
            set
            {
                _HtmlText = value;
                OnPropertyChanged(nameof(HtmlText));
            }
        }
        private string _Server;
        /// <summary>
        /// 返回结果标题
        /// </summary>
        public string Server
        {
            get { return _Server; }
            set
            {
                _Server = value;
                OnPropertyChanged(nameof(Server));
            }
        }
        private string _IP;
        /// <summary>
        /// 返回结果标题
        /// </summary>
        public string IP
        {
            get { return _IP; }
            set
            {
                _IP = value;
                OnPropertyChanged(nameof(IP));
            }
        }
        private string _DNS;
        /// <summary>
        /// 返回结果标题
        /// </summary>
        public string DNS
        {
            get { return _DNS; }
            set
            {
                _DNS = value;
                OnPropertyChanged(nameof(DNS));
            }
        }
        private string _AUrl;
        /// <summary>
        /// 返回结果标题
        /// </summary>
        public string AUrl
        {
            get { return _AUrl; }
            set
            {
                _AUrl = value;
                OnPropertyChanged(nameof(AUrl));
            }
        }
        private string _Header;
        /// <summary>
        /// 返回结果标题
        /// </summary>
        public string Header
        {
            get { return _Header; }
            set
            {
                _Header = value;
                OnPropertyChanged(nameof(Header));
            }
        }
        private string _Cookie;
        /// <summary>
        /// 返回结果标题
        /// </summary>
        public string Cookie
        {
            get { return _Cookie; }
            set
            {
                _Cookie = value;
                OnPropertyChanged(nameof(Cookie));
            }
        }

        private string _SSL;
        /// <summary>
        /// 返回结果标题
        /// </summary>
        public string SSL
        {
            get { return _SSL; }
            set
            {
                _SSL = value;
                OnPropertyChanged(nameof(SSL));
            }
        }

        private string _RequestContent;
        /// <summary>
        /// 请求内容
        /// </summary>
        public string RequestContent
        {
            get { return _RequestContent; }
            set
            {
                _RequestContent = value;
                OnPropertyChanged(nameof(RequestContent));
            }
        }

        #endregion

        #region 临时数据
        private WebList _CurrentReturnWeb;
        /// <summary>
        /// 当前请求结果
        /// </summary>
        public WebList CurrentReturnWeb
        {
            get { return _CurrentReturnWeb; }
            set
            {
                _CurrentReturnWeb = value;
                OnPropertyChanged(nameof(CurrentReturnWeb));
            }
        }
        private string _Speed;
        /// <summary>
        /// 速度
        /// </summary>
        public string Speed
        {
            get { return _Speed; }
            set
            {
                _Speed = value;
                OnPropertyChanged(nameof(Speed));
            }
        }
        #endregion
    }
   
}
