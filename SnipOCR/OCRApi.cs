using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Controls;

namespace SnipOCR
{
    class OCRApi
    {
        
        // 百度云中开通对应服务应用的 API Key 建议开通应用的时候多选服务

        private static String clientId = MyXml.GetXmlValue("API_Key");
        // 百度云中开通对应服务应用的 Secret Key
        private static String clientSecret = MyXml.GetXmlValue("Secret_Key");
        
        public static string NoEnterString = " ";                 // 定义不换行文本
        public static string EnterString = " ";                   // 定义换行文本

        /// <summary>
        /// 更新API Key   Secret Key
        /// </summary>
        public static void Update()
        {
            clientId = MyXml.GetXmlValue("API_Key");
            clientSecret = MyXml.GetXmlValue("Secret_Key");
        }

        /// <summary>
        /// 获得accessToken
        /// </summary>
        /// <returns>AccessToken</returns>
        public static String getAccessToken()
        {
            String authHost = "https://aip.baidubce.com/oauth/2.0/token";
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            List<KeyValuePair<String, String>> paraList = new List<KeyValuePair<string, string>>();
            paraList.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            paraList.Add(new KeyValuePair<string, string>("client_id", clientId));
            paraList.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
            try
            {
                System.Net.Http.HttpResponseMessage response = client.PostAsync(authHost, new System.Net.Http.FormUrlEncodedContent(paraList)).Result;
                String result = response.Content.ReadAsStringAsync().Result;
                String[] SpiltStr1 = result.Split(',');
                String[] SpiltStr2 = SpiltStr1[3].Split(':');
                String[] SpiltStr3 = SpiltStr2[1].Split('"');

                return SpiltStr3[1];
            }
            catch
            {
                System.Windows.MessageBox.Show("出错了，请检查网络之后重试");
                return null;
            }

        }

        /// <summary>
        /// 上传图片，获得识别内容
        /// </summary>
        /// <returns></returns>
        public static string GetWords(string path)
        {
            string token = getAccessToken();
            string host = "https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic?access_token=" + token;
            Encoding encoding = Encoding.Default;
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(host);
            request.Method = "post";
            request.KeepAlive = true;
            // 图片的base64编码
            string base64 = getFileBase64(path);
            String str = "image=" + HttpUtility.UrlEncode(base64);
            byte[] buffer = encoding.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string result = reader.ReadToEnd();
            return result;
        }
        /// <summary>
        /// 直接上传base64的字符串，获得内容
        /// </summary>
        /// <param name="base64"></param>
        /// <param name="a">标识符（无明确含义）</param>
        /// <returns></returns>
        public static string GetWords(string base64, bool a)
        {
            string token = getAccessToken();
            string host = "https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic?access_token=" + token;
            Encoding encoding = Encoding.Default;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.KeepAlive = true;
            String str = "image=" + HttpUtility.UrlEncode(base64);
            byte[] buffer = encoding.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string result = reader.ReadToEnd();
            return result;
        }
        /// <summary>
        /// 转换为用 Base64 数字编码的字符串表示形式
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <returns>字符串</returns>
        public static String getFileBase64(String fileName)
        {
            FileStream filestream = new FileStream(fileName, FileMode.Open);
            byte[] arr = new byte[filestream.Length];
            filestream.Read(arr, 0, (int)filestream.Length);
            string baser64 = Convert.ToBase64String(arr);
            filestream.Close();
            return baser64;
        }
        /// <summary>
        /// 转换为用 Base64 数字编码的字符串表示形式（重载）
        /// </summary>
        /// <param name="bitmap">图像文件</param>
        /// <returns>字符串</returns>
        public static String getFileBase64(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())

            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
                String str = Convert.ToBase64String(data);
                return str;
            }
        }
        /// <summary>
        /// 将字符串转成JSON后提取需要的信息
        /// </summary>
        /// <param name="text">初始文本</param>
        /// <param name="textBlock1">字数</param>
        /// <param name="textBlock2">显示的文字</param>
        /// <returns>所需信息</returns>
        public static String Str2Json(string text,TextBlock textBlock,TextBox textBox)
        {
            JObject jo = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(text);
            string str = "";
            for (int i = 0; i < jo["words_result"].Count(); i++)
            {
                str += jo["words_result"][i]["words"].ToString() + '\n';
            }
            EnterString = str;

            str = "";
            for (int i = 0; i < jo["words_result"].Count(); i++)
            {
                str += jo["words_result"][i]["words"].ToString() + ' ';
            }
            NoEnterString = str;
            textBox.Text = str;
            textBlock.Text = str.Length.ToString();
            return str;
        }
        /// <summary>
        /// 清除数据
        /// </summary>
        public static void Clear()
        {
            EnterString = null;
            NoEnterString = null;
        }

        /// <summary>
        /// 获取手写笔迹的识别结果
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string getHandWriting(string path)
        {
            string token = getAccessToken();
            string host = "https://aip.baidubce.com/rest/2.0/ocr/v1/handwriting?access_token=" + token;
            Encoding encoding = Encoding.Default;
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(host);
            request.Method = "post";
            request.KeepAlive = true;
            // 图片的base64编码
            string base64 = getFileBase64(path);
            String str = "image=" + HttpUtility.UrlEncode(base64);
            byte[] buffer = encoding.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string result = reader.ReadToEnd();

            return result;
        }

        
    }
}
