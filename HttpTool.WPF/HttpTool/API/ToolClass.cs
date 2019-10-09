using HttpTool.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HttpTool.API
{
   public static class ToolClass
    {
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="Size"></param>
        /// <returns></returns>
        public static string CountSize(double Size)
        {
            string houzui = "B";
            double FactSize = Size;
            if (FactSize >= 1024)
            {
                houzui = "K";
                FactSize = (FactSize / 1024.00);
            }
            if (FactSize >= 1024)
            {
                houzui = "M";
                FactSize = (FactSize / 1024.00);
            }
            if (FactSize >= 1024)
            {
                houzui = "G";
                FactSize = (FactSize / 1024.00);
            }
            if (FactSize >= 1024)
            {
                houzui = "T";
                FactSize = (FactSize / 1024.00);
            }
            return $"{Math.Round(FactSize, 2)} {houzui}";
        }

        #region 历史
        public static string SHA1(string content, Encoding encode)
        {
            try
            {
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] bytes_in = encode.GetBytes(content);
                byte[] bytes_out = sha1.ComputeHash(bytes_in);
                sha1.Dispose();
                string result = BitConverter.ToString(bytes_out);
                result = result.Replace("-", "");
                return result;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获得所有请求历史
        /// </summary>
        /// <returns></returns>
        public static void GetHistoryAll(string path_history,Action<History> callback)
        {
            if (File.Exists(path_history))
            {
                string[] arr = File.ReadAllLines(path_history, Encoding.UTF8);
                
                foreach (string item in arr)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        try
                        {
                            callback(Newtonsoft.Json.JsonConvert.DeserializeObject<History>(item));
                        }
                        catch { }
                    }
                }
            }
        }

        public static List<History> GetHistoryAll(string path_history)
        {
            List<History> histories = new List<History>();
            if (File.Exists(path_history))
            {
                string[] arr = File.ReadAllLines(path_history, Encoding.UTF8);
                foreach (string item in arr)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        try
                        {
                            histories.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<History>(item));
                        }
                        catch { }
                    }
                }
            }
            return histories;
        }
        /// <summary>
        /// 清除历史
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        public static bool DeleteHistory(string path_history, History history)
        {
            return DeleteHistory(path_history,history.sha1);
        }
        /// <summary>
        /// 根据Sha清除历史
        /// </summary>
        /// <param name="path_history"></param>
        /// <param name="sha1"></param>
        /// <returns></returns>
        public static bool DeleteHistory(string path_history,string sha1)
        {
            if (File.Exists(path_history))
            {
                List<string> histories = new List<string>();
                bool isD = false;
                string[] arr = File.ReadAllLines(path_history, Encoding.UTF8);
                foreach (string item in arr)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        History json = Newtonsoft.Json.JsonConvert.DeserializeObject<History>(item);
                        if (json.sha1 != sha1)
                        {
                            histories.Add(item);
                        }
                        else
                        {
                            isD = true;
                            break;
                        }
                    }
                }
                if (isD)
                {
                    File.WriteAllLines(path_history, histories.ToArray(), Encoding.UTF8);
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
