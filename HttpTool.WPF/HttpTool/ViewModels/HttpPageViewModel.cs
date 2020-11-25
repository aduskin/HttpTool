using AduSkin.Controls.Metro;
using GalaSoft.MvvmLight.Command;
using HttpTool.API;
using HttpTool.Model;
using HttpTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Input;

namespace HttpTool.ViewModels
{
    public class HttpPageViewModel: TabBase
    {
        #region 事件
        public HttpPageViewModel()
        {
            path_history = path_base + "history.json";

            BindDropDown();
            //注册接收广播
            //请求头
            //API.Broadcast.RegisterBroadcast<string>("RequestHeader", (Headerstr) =>
            //{
            //    Request_Headers(Headerstr);
            //});
            ////地址
            //API.Broadcast.RegisterBroadcast<string>("RequestUrl", (Url) =>
            //{
            //    RequestHistory.Clear();
            //    History his = new History();
            //    his.url = Url;
            //    RequestHistory.Add(his);
            //    isOpenSearchList = true;
            //});
        }

        /// <summary>
        /// 命令Command
        /// </summary>
        public ICommand MyCommand => new RelayCommand(() =>
        {
            Win_Setting win = new Win_Setting();
            win.Show();
        });

        /// <summary>
        /// 添加请求参数
        /// </summary>
        public ICommand AddRequestParameterCommand => new RelayCommand(() =>
        {
            RequestParameter.Add(new Sys_Code("", ""));
        });



        /// <summary>
        /// 删除当前参数
        /// </summary>
        public ICommand RemoveParameter => new RelayCommand<Sys_Code>((e) =>
        {
            RequestParameter.Remove(e);
        });

        /// <summary>
        /// 添加请求头
        /// </summary>
        public ICommand AddRequestHeaderCommand => new RelayCommand(() =>
        {
            RequestHead.Add(new Sys_Code());
        });

        /// <summary>
        /// 删除请求头
        /// </summary>
        public ICommand RemoveHeader => new RelayCommand<Sys_Code>((e) =>
        {
            RequestHead.Remove(e);
        });
        /// <summary>
        /// 搜索历史
        /// </summary>    
        public ICommand SearchTextChange => new RelayCommand(() =>
        {
            isOpenSearchList = true;
            Task.Run(() => { GetHistoryAll(); });
        });

        /// <summary>
        /// 删除请求历史
        /// </summary>    
        public ICommand RemoveRequestHistory => new RelayCommand<History>((history) =>
        {
            RequestHistory.Remove(history);
            Task.Run(() =>
            {
                ToolClass.DeleteHistory(path_history, history);
            });
        });
        /// <summary>
        /// 开始参数文本模式
        /// </summary>    
        public ICommand ChangeParameter => new RelayCommand(() =>
        {
            if (IsOpenParameterText)
            {
                RequestParameterVisibility = Visibility.Collapsed;
                RequestParameterTextVisibility = Visibility.Visible;
            }
            else
            {
                RequestParameterVisibility = Visibility.Visible;
                RequestParameterTextVisibility = Visibility.Collapsed;
            }
        });

        /// <summary>
        /// 显示文件上传
        /// </summary>    
        public ICommand OpenFileUploadList => new RelayCommand(() =>
        {
            if (!isAllowFileUpload)
                isAllowFileUpload = true;
        });

        /// <summary>
        /// 删除文件
        /// </summary>    
        public ICommand RemoveFile => new DelegateCommand(obj =>
        {
            if (obj is AFile file)
                UploadFileList.Remove(file);
        });

        private ObservableCollection<AFile> _UploadFileList;
        /// <summary>
        /// 文件列表
        /// </summary>
        public ObservableCollection<AFile> UploadFileList
        {
            get
            {
                if (_UploadFileList == null)
                {
                    _UploadFileList = new ObservableCollection<AFile>();
                }
                return _UploadFileList;
            }
            set { Set(ref _UploadFileList, value); }
        }

        private RelayCommand<object> _FileUploadCommand;
        public RelayCommand<object> FileUploadCommand
        {
            get
            {
                return _FileUploadCommand ?? (new RelayCommand<object>(HandleFileUpload));
            }

            set
            {
                _FileUploadCommand = value;
            }
        }
        private void HandleFileUpload(object param)
        {
            Array files = param as Array;

            for (int i = 0; i < files.Length; i++)
            {
                string filePath = files.GetValue(i).ToString();
                FileInfo fileInfo = new FileInfo(filePath);
                if (UploadFileList.Count(a => a.FullPath == Path.GetFullPath(filePath)) <= 0)
                {
                    this.UploadFileList.Add(new AFile()
                    {
                        ID = "1",
                        FullPath = Path.GetFullPath(filePath),
                        FileName = Path.GetFileName(filePath),
                        Extension = Path.GetExtension(filePath),
                    });
                }

            }
        }
        string path_history;
        //本地变量
        string path_base = AppDomain.CurrentDomain.BaseDirectory + "temp\\";
        //计时器
        System.Diagnostics.Stopwatch _time = new System.Diagnostics.Stopwatch();
        //线程池
        public TaskFactory _task = new TaskFactory();
        /// <summary>
        /// 跳转至
        /// </summary>    
        public ICommand ToUrlCommand => new DelegateCommand(obj =>
        {
            isOpenSearchList = false;

            if (!string.IsNullOrEmpty(ToUrlTxt))
            {
                if (!isRun)
                {
                    //if (obj is Border border)
                    //{
                    //   requestBack = border;
                    //   isWhile = true;
                    //   FloatElement(border, 0.1);
                    //}
                    isRun = true;
                    if (!ToUrlTxt.StartsWith("http"))
                    {
                        ToUrlTxt = "http://" + ToUrlTxt;
                    }
                    //请求方式，编码
                    string RequestMethod = CurrentHttpType.CodeName;
                    string RequestEnCode = CurrentCodeType.CodeName;
                    string _conmand = null;//请求参
                    if (IsOpenParameterText)
                    { _conmand = RequestParameterText; }

                    string[] _file = null;//文件地址
                    if (UploadFileList.Count > 0)
                    {
                        _file = UploadFileList.Select(a => a.FullPath).ToArray();
                    }

                    //开启任务
                    Action task = () =>
                    {
                        ProgressBarValue = 0;
                        #region 启动HTTP请求之前的初始化操作
                        Encoding _encoding = Encoding.UTF8;
                        try { _encoding = Encoding.GetEncoding(RequestEnCode); } catch { }

                        bool isget = false;
                        if (RequestMethod.ToUpper() == "GET")
                        {
                            isget = true;
                        }
                        string geturl = ToUrlTxt;
                        //get请求
                        if (isget)
                        {
                            //带参数
                            if (RequestParameter.Count > 0)
                            {
                                string param = "";
                                foreach (Sys_Code item in RequestParameter)
                                {
                                    if (!string.IsNullOrEmpty(item.CodeName) && !string.IsNullOrEmpty(item.CodeValue))
                                    {


                                        if (param.Length <= 0)
                                        {
                                            if (geturl.Contains("?"))
                                            {
                                                param += "&" + item.CodeName + "=" + item.CodeValue;
                                            }
                                            else
                                            {
                                                param = "?" + item.CodeName + "=" + item.CodeValue;
                                            }
                                        }
                                        else
                                        {
                                            param += "&" + item.CodeName + "=" + item.CodeValue;
                                        }
                                    }
                                }
                                geturl += param;
                            }
                        }
                        Uri url = null;
                        try
                        {
                            url = new Uri(geturl);
                        }
                        catch { }
                        #endregion

                        if (url != null)
                        {
                            History history = new History();
                            history.url = ToUrlTxt;
                            history.type = RequestMethod;
                            history.encode = RequestEnCode;
                            history.file = _file;
                            //开始请求，设置控件禁用属性
                            //RequestState = ProgressBarState.Wait;
                            string _scheme = url.Scheme.ToUpper();
                            WebList _web = new WebList();
                            try
                            {
                                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);

                                #region Ssl
                                if (_scheme == "HTTPS")
                                {
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                                    /// <summary>
                                    /// 验证用于身份验证的远程安全套接字层 (SSL) 证书。
                                    /// </summary>
                                    /// <param name="_sender">一个对象，它包含此验证的状态信息。</param>
                                    /// <param name="certificate">用于对远程方进行身份验证的证书。</param>
                                    /// <param name="chain">与远程证书关联的证书颁发机构链。</param>
                                    /// <param name="sslPolicyErrors">与远程证书关联的一个或多个错误。</param>
                                    /// <returns>它确定是否接受指定证书进行身份验证。</returns>
                                    ServicePointManager.ServerCertificateValidationCallback = (_sender, certificate, chain, sslPolicyErrors) =>
                                    {
                                        // Get the value.
                                        //string resultsTrue = certificate.ToString(true);
                                        //System.Diagnostics.Debug.WriteLine(resultsTrue);

                                        _web.SslErr = sslPolicyErrors;
                                        _web.SslSub = certificate.Subject;
                                        _web.SslUer = certificate.Issuer;
                                        //System.Diagnostics.Debug.WriteLine(certificate.Subject);
                                        //System.Diagnostics.Debug.WriteLine(certificate.Issuer);
                                        return true;
                                    };
                                }
                                #endregion

                                req.Host = url.Host;
                                req.Method = RequestMethod;
                                req.AllowAutoRedirect = true;

                                bool isContentType = true;
                                #region 设置请求头
                                if (RequestHead.Count > 0)
                                {
                                    List<TP> _TPs = new List<TP>();
                                    foreach (Sys_Code item in RequestHead)
                                    {
                                        if (!string.IsNullOrEmpty(item.CodeName) && !string.IsNullOrEmpty(item.CodeValue))
                                        {


                                            TP _TP = new TP();
                                            _TP.name = item.CodeName;
                                            _TP.value = item.CodeValue;
                                            _TPs.Add(_TP);

                                            string _Lower_Name = item.CodeName.ToLower();
                                            switch (_Lower_Name)
                                            {
                                                case "host":
                                                    req.Host = item.CodeValue;
                                                    break;
                                                case "accept":
                                                    req.Accept = item.CodeValue;
                                                    break;
                                                case "user-agent":
                                                    req.UserAgent = item.CodeValue;
                                                    break;
                                                case "referer":
                                                    req.Referer = item.CodeValue;
                                                    break;
                                                case "content-type":
                                                    isContentType = false;
                                                    req.ContentType = item.CodeValue;
                                                    break;
                                                case "cookie":
                                                    #region 设置COOKIE
                                                    string _cookie = item.CodeValue;
                                                    CookieContainer cookie_container = new CookieContainer();
                                                    if (_cookie.IndexOf(";") >= 0)
                                                    {
                                                        string[] arrCookie = _cookie.Split(';');
                                                        //加载Cookie
                                                        //cookie_container.SetCookies(new Uri(url), cookie);
                                                        foreach (string sCookie in arrCookie)
                                                        {
                                                            if (string.IsNullOrEmpty(sCookie))
                                                            {
                                                                continue;
                                                            }
                                                            if (sCookie.IndexOf("expires") > 0)
                                                            {
                                                                continue;
                                                            }
                                                            cookie_container.SetCookies(url, sCookie);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        cookie_container.SetCookies(url, _cookie);
                                                    }
                                                    req.CookieContainer = cookie_container;
                                                    #endregion
                                                    break;
                                                default:
                                                    SetHeaderValue(req.Headers, item.CodeName, item.CodeValue);
                                                    break;

                                            }
                                        }
                                    }
                                    history.header = _TPs;
                                }
                                #endregion

                                #region 设置POST数据 
                                if (!isget)
                                {
                                    if (_file != null)
                                    {
                                        //POST文件
                                        try
                                        {
                                            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                                            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                                            byte[] endbytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

                                            req.ContentType = "multipart/form-data; boundary=" + boundary;


                                            using (Stream reqStream = req.GetRequestStream())
                                            {
                                                //tProgress.Invoke(new Action(() =>
                                                //{
                                                //    tProgress.ShowInTaskbar = true;
                                                //}));
                                                string[] files = _file;

                                                //1.2 file 
                                                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                                                //byte[] buffer = new byte[4096];
                                                byte[] buff = new byte[1024];
                                                //int bytesRead = 0;
                                                for (int i = 0; i < files.Length; i++)
                                                {
                                                    string file = files[i];
                                                    reqStream.Write(boundarybytes, 0, boundarybytes.Length);
                                                    string contentType = MimeMapping.GetMimeMapping(file);

                                                    string header = string.Format(headerTemplate, "media", Path.GetFileName(file), contentType);//微信
                                                    byte[] headerbytes = _encoding.GetBytes(header);
                                                    reqStream.Write(headerbytes, 0, headerbytes.Length);

                                                    using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                                                    {

                                                        int contentLen = fileStream.Read(buff, 0, buff.Length);

                                                        int Value = contentLen;
                                                        //tProgress.Invoke(new Action(() =>
                                                        //{
                                                        ProgressBarMaxNum = (int)fileStream.Length;
                                                        ProgressBarValue = Value;
                                                        //}));

                                                        while (contentLen > 0)
                                                        {
                                                            reqStream.Write(buff, 0, contentLen);
                                                            contentLen = fileStream.Read(buff, 0, buff.Length);
                                                            Value += contentLen;

                                                            //tProgress.Invoke(new Action(() =>
                                                            //{
                                                            ProgressBarValue = Value;
                                                            //}));
                                                        }
                                                    }
                                                }

                                                //1.3 form end
                                                reqStream.Write(endbytes, 0, endbytes.Length);
                                            }


                                        }
                                        catch
                                        {
                                            if (isContentType)
                                            {
                                                req.ContentType = null;
                                            }
                                            req.ContentLength = 0;
                                        }
                                        //System.Diagnostics.Debug.WriteLine(contentType);
                                    }
                                    else if (!string.IsNullOrEmpty(_conmand))
                                    {
                                        history.conmandstr = _conmand;
                                        //POST参数
                                        if (isContentType)
                                        {
                                            req.ContentType = "application/x-www-form-urlencoded";
                                        }
                                        string param = _conmand;

                                        byte[] bs = _encoding.GetBytes(param);

                                        req.ContentLength = bs.Length;

                                        using (Stream reqStream = req.GetRequestStream())
                                        {
                                            reqStream.Write(bs, 0, bs.Length);
                                            reqStream.Close();
                                        }
                                    }
                                    else if (RequestParameter.Count > 0)
                                    {
                                        //POST参数（自动）
                                        if (isContentType)
                                        {
                                            req.ContentType = "application/x-www-form-urlencoded";
                                        }
                                        string param = "";
                                        List<TP> _TPs = new List<TP>();
                                        foreach (Sys_Code item in RequestParameter)
                                        {
                                            if (!string.IsNullOrEmpty(item.CodeName) && !string.IsNullOrEmpty(item.CodeValue))
                                            {
                                                TP _TP = new TP();
                                                _TP.name = item.CodeName;
                                                _TP.value = item.CodeValue;
                                                _TPs.Add(_TP);

                                                if (string.IsNullOrEmpty(param))
                                                {
                                                    param = item.CodeName + "=" + item.CodeValue;
                                                }
                                                else
                                                {
                                                    param += "&" + item.CodeName + "=" + item.CodeValue;
                                                }
                                            }
                                        }
                                        history.conmand = _TPs;

                                        byte[] bs = _encoding.GetBytes(param);

                                        req.ContentLength = bs.Length;

                                        using (Stream reqStream = req.GetRequestStream())
                                        {
                                            reqStream.Write(bs, 0, bs.Length);
                                            reqStream.Close();
                                        }
                                    }
                                    else
                                    {
                                        req.ContentLength = 0;
                                    }
                                }
                                #endregion
                                _time.Reset();
                                _time.Start();
                                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                                {
                                    //materialL.isLeft = true;
                                    _web.Html = DownSteam(_scheme, response, _web, _encoding);
                                }
                            }
                            catch (WebException err)
                            {
                                var rsp = err.Response as HttpWebResponse;
                                _web.Err = err.Message;
                                if (rsp != null)
                                {
                                    _web.Html = DownSteam(_scheme, rsp, _web, _encoding);
                                    rsp.Close();
                                }
                                else
                                {
                                    _time.Stop();
                                    _web = null;
                                }
                            }
                            if (_web != null)
                            {
                                #region 写入历史
                                history.sha1 = ToolClass.SHA1(Newtonsoft.Json.JsonConvert.SerializeObject(history), Encoding.UTF8);
                                File.AppendAllText(path_history, Newtonsoft.Json.JsonConvert.SerializeObject(history) + Environment.NewLine, Encoding.UTF8);
                                #endregion
                                double speed_Milliseconds = Math.Round(_time.Elapsed.TotalMilliseconds, 2);
                                double speed_Seconds = Math.Round(_time.Elapsed.TotalSeconds, 2);
                                string speed = speed_Seconds >= 1 ? speed_Seconds + " s" : speed_Milliseconds + " ms";
                                WinAPI.CurrentReturnWeb = _web;
                                WinAPI.Speed = speed;
                            }
                        }
                    };
                    Task[] tasks = new Task[] { _task.StartNew(task) };
                    _task.ContinueWhenAll(tasks, (action => { Ok(); }));
                }
            }

        });
        private ProgressBarState _RequestState = ProgressBarState.None;
        /// <summary>
        /// 请求状态
        /// </summary>
        public ProgressBarState RequestState
        {
            get { return _RequestState; }
            set
            {
                Set(ref _RequestState, value);
            }
        }

        private bool _RequestParamState = true;
        /// <summary>
        /// 请求参数是否能编辑
        /// </summary>
        public bool RequestParamState
        {
            get { return _RequestParamState; }
            set { Set(ref _RequestParamState, value); }
        }

        private bool _RequestHeaderState = true;
        /// <summary>
        /// 请求头是否能编辑
        /// </summary>
        public bool RequestHeaderState
        {
            get { return _RequestHeaderState; }
            set { Set(ref _RequestHeaderState, value); }
        }

        public void Ok()
        {
            _time.Stop();
            App.DispatcherHelper.Invoke(new Action(() =>
            {
                //if (requestBack != null)
                //{
                //   isWhile = false;
                //   FloatElement(requestBack, 1);
                //}
                Win_Response wr = new Win_Response();
                wr.Show();
                //materialL.Stop();
                //RequestState = ProgressBarState.None;

                //materialL.Visible = false;

                RequestHeaderState = true;
                //if (label3.Tag == null)
                //{
                //    RequestParamState = true;
                //    if (rich_conmand != null)
                //    {
                //        rich_conmand.Enabled = true;
                //    }
                //}
                //slideButton1.Enabled = true;
                //add_conmand.Enabled = true;
                //add_header.Enabled = true;


                //txt_url.Left = comboBox_Encode.Left + 70;
                //txt_url.Width = panel_url.Width - 12 - txt_url.Left;

                //comboBox_Type.Visible = true;
                //comboBox_Encode.Visible = true;


                //panel_url.BackColor = Color.White;
                //txt_url.BackColor = panel_url.BackColor;

                //panel_url.Enabled = true;
            }));

            isRun = false;
        }

        // 请求头字符串复制对象
        public void Request_Headers(string _txt)
        {
            if (_txt.Contains("\n"))
            {
                RequestHead.Clear();
                string[] arr = _txt.Split('\n');
                foreach (string _item in arr)
                {
                    if (!string.IsNullOrEmpty(_item) && _item.Contains(":"))
                    {
                        string[] value = _item.Split(':');
                        Sys_Code item = new Sys_Code();
                        item.CodeName = value[0].Trim();
                        string _value = value[1].Replace("\"\"", "\"").Trim();
                        if (value.Length > 2)
                        {
                            for (int i = 2; i < value.Length; i++)
                            {
                                _value += ":" + value[i].Trim();
                            }
                        }
                        item.CodeValue = _value;
                        RequestHead.Add(item);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_txt) && _txt.Contains(":"))
                {
                    string[] value = _txt.Split(':');
                    string _name = value[0].Trim();

                    string _value = value[1].Trim();
                    if (value.Length > 2)
                    {
                        for (int i = 2; i < value.Length; i++)
                        {
                            _value += ":" + value[i].Trim();
                        }
                    }

                    Sys_Code _item = RequestHead.Where(ab => ab.CodeName == _name).FirstOrDefault();
                    if (_item == null)
                    {
                        Sys_Code item = new Sys_Code();
                        item.CodeName = _name;

                        item.CodeValue = _value;
                        RequestHead.Add(item);
                    }
                    else
                    {
                        _item.CodeValue = "";
                        _item.CodeValue = _value;
                    }
                }
            }
        }

        #region 辅助
        /// <summary>
        /// 数据流下载
        /// </summary>
        /// <param name="response"></param>
        /// <param name="_web"></param>
        /// <param name="encoding"></param>
        string DownSteam(string _scheme, HttpWebResponse response, WebList _web, Encoding encoding)
        {
            string Disposition = response.Headers["Content-Disposition"];
            if (!string.IsNullOrEmpty(Disposition))
            {
                string filename = "";
                filename = Disposition.Substring(Disposition.IndexOf("filename=") + 9);
                if (filename.Contains(";"))
                {
                    filename = filename.Substring(0, filename.IndexOf(";"));
                    if (filename.EndsWith("\""))
                    {
                        filename = filename.Substring(1, filename.Length - 2);
                    }
                }
                _web.FileName = filename;
            }

            string header = "";
            foreach (string str in response.Headers.AllKeys)
            {
                header = header + str + ":" + response.Headers[str] + "\r\n";
            }
            string cookie = "";
            foreach (Cookie str in response.Cookies)
            {
                cookie = cookie + str.Name + "=" + str.Value + ";";
            }
            _web.StatusCode = (int)response.StatusCode;
            _web.Type = response.ContentType;

            _web.ServerHeader = string.Format("{0} {1} {2} {3} Ver:{4}.{5}", _scheme, _web.StatusCode, response.StatusCode, response.Server, response.ProtocolVersion.Major, response.ProtocolVersion.Minor);
            _web.IP = getIP(response.ResponseUri);
            _web.DNS = response.ResponseUri.DnsSafeHost;
            _web.AbsoluteUri = response.ResponseUri.AbsoluteUri;
            _web.Header = header;
            _web.Cookie = cookie;

            string _Data = String.Empty;
            #region 下载流

            using (Stream stream = response.GetResponseStream())
            {
                int int_ContentLength = (int)response.ContentLength;
                //System.Diagnostics.Debug.WriteLine(response.ContentLength);
                if (!Directory.Exists(path_base))
                {
                    Directory.CreateDirectory(path_base);
                }
                string fname = DateTime.Now.ToString("yyyyMMddHHmmss") + GenerateRandomCode(4);

                _web.Temp = path_base + fname;
                try
                {
                    FileStream file = new FileStream(_web.Temp, FileMode.Create, FileAccess.Write | FileAccess.Read);
                    int _value = 0;
                    byte[] _cache = new byte[1024];
                    int osize = stream.Read(_cache, 0, 1024);

                    bool isKnow_size = true;
                    App.DispatcherHelper.Invoke(new Action(() =>
                    {
                        if (response.ContentLength > 0)
                        {
                            //tProgress.ShowInTaskbar = true;
                            ProgressBarMaxNum = int_ContentLength;
                        }
                        else
                        {
                            isKnow_size = false;
                            //tProgress.ShowInTaskbar = true;
                            //tProgress.Style = ProgressBarStyle.Marquee;
                        }
                        ProgressBarValue = _value;
                    }));
                    while (osize > 0)
                    {
                        _value += osize;
                        if (isKnow_size)
                        {
                            App.DispatcherHelper.Invoke(new Action(() =>
                            {
                                ProgressBarValue = _value;
                            }));
                        }
                        file.Write(_cache, 0, osize);
                        osize = stream.Read(_cache, 0, 1024);
                    }

                    _time.Stop();

                    file.Seek(0, SeekOrigin.Begin);
                    string fileclass = GetFileClass(file);

                    file.Close();
                    file.Dispose();


                    _web.Extension = GetFileExtension(fileclass);

                    byte[] _byte = File.ReadAllBytes(_web.Temp);
                    if (_web.Extension == FileExtension.GZIP)
                    {
                        _web.OriginalSize = _byte.Length;
                        _web.Byte = Decompress(_byte);
                        try
                        {
                            using (MemoryStream memoryStream = new MemoryStream(_web.Byte))
                            {
                                fileclass = GetFileClass(memoryStream);
                                _web.Extension = GetFileExtension(fileclass);
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        _web.Byte = _byte;
                    }


                    _Data = encoding.GetString(_web.Byte);
                }
                catch
                {
                    //IO写入失败

                }
                //tProgress.Invoke(new Action(() =>
                //{
                //    tProgress.Style = ProgressBarStyle.Blocks;
                //    tProgress.ShowInTaskbar = false;
                //}));
            }
            #endregion
            return _Data;
        }

        /// <summary>
        /// 获取域名IP
        /// </summary>
        public string getIP(Uri _uri)
        {
            IPAddress ip = null;
            if (IPAddress.TryParse(_uri.Host, out ip))
            {
                return ip.ToString();
            }
            else
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(_uri.Host);
                IPEndPoint ipEndPoint = new IPEndPoint(hostEntry.AddressList[0], 0);
                string _ip = ipEndPoint.Address.ToString();
                if (_ip.StartsWith("::"))
                {
                    return "127.0.0.1";
                }
                else
                {
                    return _ip;
                }
            }
        }
        public string GetFileClass(Stream stream)
        {
            string fileclass = "";
            try
            {
                using (BinaryReader r = new BinaryReader(stream))
                {
                    byte buffer = r.ReadByte();
                    fileclass = buffer.ToString();
                    buffer = r.ReadByte();
                    fileclass += buffer.ToString();
                }
            }
            catch { }
            return fileclass;
        }

        ///  <summary> 
        /// 解压字符串
        ///  </summary> 
        ///  <param name="data"></param> 
        ///  <returns></returns> 
        byte[] Decompress(byte[] data)
        {
            try
            {
                var ms = new MemoryStream(data);
                var zip = new GZipStream(ms, CompressionMode.Decompress);
                var msreader = new MemoryStream();
                var buffer = new byte[0x1000];
                while (true)
                {
                    var reader = zip.Read(buffer, 0, buffer.Length);
                    if (reader <= 0)
                    {
                        break;
                    }
                    msreader.Write(buffer, 0, reader);
                }
                zip.Close();
                ms.Close();
                msreader.Position = 0;
                buffer = msreader.ToArray();
                msreader.Close();
                return buffer;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        ///生成制定位数的随机码（数字）
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GenerateRandomCode(int length)
        {
            var result = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                result.Append(r.Next(0, 10));
            }
            return result.ToString();
        }

        /// <summary>
        /// 设置对象的Header数据
        /// </summary>
        /// <param name="header">对象</param>
        /// <param name="name">键</param>
        /// <param name="value">值</param>
        public void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as System.Collections.Specialized.NameValueCollection;
                collection[name] = value;
            }
        }

        public FileExtension GetFileExtension(string _ex)
        {
            switch (_ex)
            {
                case "": return FileExtension.NULL;
                case "00": return FileExtension.ICO;
                case "255216": return FileExtension.JPG;
                case "7173": return FileExtension.GIF;
                case "6677": return FileExtension.BMP;
                case "13780": return FileExtension.PNG;
                case "7790": return FileExtension.EXE_DLL_COM;
                case "8297": return FileExtension.RAR;
                case "8075": return FileExtension.ZIP;
                case "6063": return FileExtension.XML;
                case "6099": return FileExtension.XMLS;
                case "6033": return FileExtension.HTML;
                case "60105": return FileExtension.HTM;
                case "239187": return FileExtension.TXT_UTF8_ASPX;
                case "117115": return FileExtension.CS;
                case "119105": return FileExtension.JS;
                case "210187": return FileExtension.TXT;
                case "5150": return FileExtension.TXT_ANSI;
                case "255254": return FileExtension.TXT_UNICODE_SQL_RDP;
                case "254255": return FileExtension.TXT_UNICODE_BIG;
                case "64101": return FileExtension.BAT;
                case "10056": return FileExtension.BTSEED;
                case "5666": return FileExtension.PSD;
                case "3780": return FileExtension.PDF;
                case "7384": return FileExtension.CHM;
                case "70105": return FileExtension.LOG;
                case "8269": return FileExtension.REG;
                case "6395": return FileExtension.HLP;
                case "208207": return FileExtension.DOC_XLS_DOCX_XLSX;
                case "10054": return FileExtension.CA;
                case "760": return FileExtension.INK;
                case "12392": return FileExtension.RTF;
                case "55122": return FileExtension._7Z;
                case "8273": return FileExtension.WAV;
                case "9188": return FileExtension.INI;
                case "6787": return FileExtension.SWF;
                case "7368": return FileExtension.MP3;
                case "4682": return FileExtension.RMVB;
                case "31139": return FileExtension.GZIP;
                case "12334": return FileExtension.JSON;
                case "4742": return FileExtension.CSS;
                case "33102": return FileExtension.JSS;
                default: return FileExtension.NONE;
            }
        }
        #endregion


        /// <summary>
        /// XXX
        /// </summary>    
        public ICommand ToAduSkinWeb => new DelegateCommand(obj =>
        {
            Process.Start(new ProcessStartInfo("https://github.com/Hero3821"));
        });

        #endregion

        #region 属性
        public bool isRun = false;

        private string _ToUrlTxt;
        /// <summary>
        /// 访问地址
        /// </summary>
        public string ToUrlTxt
        {
            get { return _ToUrlTxt; }
            set { Set(ref _ToUrlTxt, value); }
        }
        private bool _IsOpenParameterText;
        /// <summary>
        /// 是否打开参数文本
        /// </summary>
        public bool IsOpenParameterText
        {
            get { return _IsOpenParameterText; }
            set { Set(ref _IsOpenParameterText, value); }
        }

        private bool _isAllowFileUpload = false;
        /// <summary>
        /// 是否显示文件列表
        /// </summary>
        public bool isAllowFileUpload
        {
            get { return _isAllowFileUpload; }
            set { Set(ref _isAllowFileUpload, value); }
        }




        private Visibility _RequestParameterVisibility = Visibility.Visible;
        /// <summary>
        /// 请求参数列表是否启用
        /// </summary>
        public Visibility RequestParameterVisibility
        {
            get { return _RequestParameterVisibility; }
            set { Set(ref _RequestParameterVisibility, value); }
        }

        private Visibility _RequestParameterTextVisibility = Visibility.Collapsed;
        /// <summary>
        /// 请求字符串参数是否启用
        /// </summary>
        public Visibility RequestParameterTextVisibility
        {
            get { return _RequestParameterTextVisibility; }
            set { Set(ref _RequestParameterTextVisibility, value); }
        }

        private bool _isOpenSearchList = false;
        /// <summary>
        /// 是否显示搜索历史或推荐
        /// </summary>
        public bool isOpenSearchList
        {
            get { return _isOpenSearchList; }
            set { Set(ref _isOpenSearchList, value); }
        }

        private int _ProgressBarMaxNum = 0;
        /// <summary>
        /// 进度条最大值
        /// </summary>
        public int ProgressBarMaxNum
        {
            get { return _ProgressBarMaxNum; }
            set { Set(ref _ProgressBarMaxNum, value); }
        }

        private int _ProgressBarValue = 0;
        /// <summary>
        /// 进度条当前值
        /// </summary>
        public int ProgressBarValue
        {
            get { return _ProgressBarValue; }
            set { Set(ref _ProgressBarValue, value); }
        }

        #endregion

        #region 临时数据
        /// <summary>
        /// 绑定下拉框
        /// </summary>
        public void BindDropDown()
        {
            //请求编码类型
            CodeTypeList = new ObservableCollection<Sys_Code>
            {
                new Sys_Code{CodeValue="1",CodeName="utf-8"}
                , new Sys_Code{CodeValue="2",CodeName="utf-7"}
                , new Sys_Code{CodeValue="2",CodeName="utf-32"}
                , new Sys_Code{CodeValue="2",CodeName="ascii"}
                , new Sys_Code{CodeValue="2",CodeName="unicode"}
            };
            CurrentCodeType = CodeTypeList[0];
            //请求类型
            HttpTypList = new ObservableCollection<Sys_Code>
            {
                new Sys_Code{CodeValue="1",CodeName="get"}
                , new Sys_Code{CodeValue="2",CodeName="post"}
                , new Sys_Code{CodeValue="2",CodeName="put"}
                , new Sys_Code{CodeValue="2",CodeName="delete"}
            };
            CurrentHttpType = HttpTypList[0];

            //请求参数默认两个空
            RequestParameter = new ObservableCollection<Sys_Code>
            {
                new Sys_Code("","")
                , new Sys_Code("","")
            };
            //请求头默认两个空
            RequestHead = new ObservableCollection<Sys_Code>
            {
                new Sys_Code("","")
                , new Sys_Code("","")
            };
        }
        /// <summary>
        /// 请求历史
        /// </summary>
        public void GetHistoryAll()
        {
            //请求历史
            App.DispatcherHelper.Invoke(() =>
            {
                RequestHistory.Clear();
            });
            List<History> list = ToolClass.GetHistoryAll(path_history);
            foreach (var item in list)
            {
                if (item.url.IndexOf(ToUrlTxt) >= 0 || (ToUrlTxt == null || ToUrlTxt.Trim().Length == 0))
                {
                    App.DispatcherHelper.Invoke(() =>
                    {
                        RequestHistory.Add(item);
                    });
                }
            }
        }

        /// <summary>
        /// 编码格式
        /// </summary>
        private ObservableCollection<Sys_Code> _CodeTypeList;
        public ObservableCollection<Sys_Code> CodeTypeList
        {
            get
            {
                if (_CodeTypeList == null)
                {
                    _CodeTypeList = new ObservableCollection<Sys_Code>();
                }
                return _CodeTypeList;
            }
            set { Set(ref _CodeTypeList, value); }
        }
        /// <summary>
        /// 选中编码格式
        /// </summary>
        private Sys_Code _CurrentCodeType;
        public Sys_Code CurrentCodeType
        {
            get
            {
                return _CurrentCodeType;
            }
            set { Set(ref _CurrentCodeType, value); }
        }
        /// <summary>
        /// 请求方式
        /// </summary>
        private ObservableCollection<Sys_Code> _HttpTypList;
        public ObservableCollection<Sys_Code> HttpTypList
        {
            get
            {
                if (_HttpTypList == null)
                {
                    _HttpTypList = new ObservableCollection<Sys_Code>();
                }
                return _HttpTypList;
            }
            set { Set(ref _HttpTypList, value); }
        }
        /// <summary>
        /// 当前请求方式
        /// </summary>
        private Sys_Code _CurrentHttpType;
        public Sys_Code CurrentHttpType
        {
            get
            {
                return _CurrentHttpType;
            }
            set { Set(ref _CurrentHttpType, value); }
        }
        /// <summary>
        /// 参数列表
        /// </summary>
        private ObservableCollection<Sys_Code> _RequestParameter;
        public ObservableCollection<Sys_Code> RequestParameter
        {
            get
            {
                if (_RequestParameter == null)
                {
                    _RequestParameter = new ObservableCollection<Sys_Code>();
                }
                return _RequestParameter;
            }
            set { Set(ref _RequestParameter, value); }
        }
        /// <summary>
        /// 当前参数
        /// </summary>
        private Sys_Code _CurrentRequestParameter;
        public Sys_Code CurrentRequestParameter
        {
            get
            {
                return _CurrentRequestParameter;
            }
            set { Set(ref _CurrentRequestParameter, value); }
        }
        private string _RequestParameterText;
        /// <summary>
        /// 请求字符串参数
        /// </summary>
        public string RequestParameterText
        {
            get { return _RequestParameterText; }
            set { Set(ref _RequestParameterText, value); }
        }

        /// <summary>
        /// 请求头列表
        /// </summary>
        private ObservableCollection<Sys_Code> _RequestHead;
        public ObservableCollection<Sys_Code> RequestHead
        {
            get
            {
                if (_RequestHead == null)
                {
                    _RequestHead = new ObservableCollection<Sys_Code>();
                }
                return _RequestHead;
            }
            set { Set(ref _RequestHead, value); }
        }
        /// <summary>
        /// 请求历史
        /// </summary>
        private ObservableCollection<History> _RequestHistory;
        public ObservableCollection<History> RequestHistory
        {
            get
            {
                if (_RequestHistory == null)
                {
                    _RequestHistory = new ObservableCollection<History>();
                }
                return _RequestHistory;
            }
            set { Set(ref _RequestHistory, value); }
        }

        /// <summary>
        /// 当前选中的历史
        /// </summary>
        private History _CurrentRequestHistory;
        public History CurrentRequestHistory
        {
            get
            {
                return _CurrentRequestHistory;
            }
            set { Set(ref _CurrentRequestHistory, value);
                if (value != null)
                    ToUrlTxt = value.url;
            }
        }
        /// <summary>
        /// 当前参数
        /// </summary>
        private Sys_Code _CurrentHeadValue;
        public Sys_Code CurrentHeadValue
        {
            get
            {
                return _CurrentHeadValue;
            }
            set { Set(ref _CurrentHeadValue, value); }
        }
        #endregion
    }
}
