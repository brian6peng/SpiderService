using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace SpiderService
{
    public class RequestProcess
    {
        static string phantomjsPath = ConfigurationManager.AppSettings["ExetPath"];
        static string jsPathRoot = ConfigurationManager.AppSettings["JsPath"];
        static int timeout = int.Parse(ConfigurationManager.AppSettings["TimeOut"]);
        static string inputPathRoot = ConfigurationManager.AppSettings["InputPath"];
        static string outputPathRoot = ConfigurationManager.AppSettings["OutputPath"];
        static EZLogger logger = EZLogger.CreateInstance();
        static Dictionary<string, AutoResetEvent> eventWaitDict = new Dictionary<string, AutoResetEvent>();
        static Dictionary<string, string> callbackDict = new Dictionary<string, string>();
        /// <summary>
        /// process request from client
        /// </summary>
        /// <param name="result"></param>
        public static void ProcessMethod(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);
            listener.BeginGetContext(new AsyncCallback(RequestProcess.ProcessMethod), listener);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            logger.Info("Process Request at: " + DateTime.Now.ToString());
            string rawUrl = request.RawUrl;
            if (!string.IsNullOrEmpty(rawUrl))
            {
                rawUrl = System.Web.HttpUtility.UrlDecode(rawUrl);
            }
            logger.Info("RawUrl: " + rawUrl);
            if (rawUrl.StartsWith("/operate"))
            {
                ProcessOperate(rawUrl, response);
            }
            else if (rawUrl.StartsWith("/callback"))
            {
                ProcessCallback(rawUrl, request);
            }
        }

        private static void ProcessOperate(string rawUrl, HttpListenerResponse response)
        {
            try
            {
                //parse rawUrl ep: operate?id=2&args=xxx
                if (rawUrl.IndexOf("&args=") == -1)
                {
                    OutPutText(response, "error: bad args");
                    return;
                }
                int argIndex = rawUrl.IndexOf("&args=");
                string idText = rawUrl.Substring(12, argIndex - 12);
                string argsText = rawUrl.Substring(argIndex + 6);
                logger.Info("idText: " + idText);
                logger.Info("argsText: " + argsText);
                int id = 0;
                if (!int.TryParse(idText, out id))
                {
                    OutPutText(response, "error: invalid operate id");
                    return;
                }
                string inputPath = Path.Combine(inputPathRoot, argsText + ".txt");
                if (!File.Exists(inputPath))
                {
                    OutPutText(response, "error: invalid input path");
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = phantomjsPath;
                startInfo.Arguments = string.Format("{0} {1}", jsPathRoot + ((SpiderOperate)id).ToString() + ".js", inputPath);
                logger.Info("FileName: " + startInfo.FileName);
                logger.Info("Arguments: " + startInfo.Arguments);
                logger.Info("File.Exists: " + File.Exists(startInfo.FileName));
                Process ps = Process.Start(startInfo);
                bool flag = ps.WaitForExit(timeout);
                if (flag)
                {
                    logger.Info("exec complate");
                    string outputPath = Path.Combine(outputPathRoot, argsText + ".txt");
                    if (!File.Exists(outputPath))
                    {
                        OutPutText(response, "error: not find result");
                        return;
                    }
                    string resultContent = File.ReadAllText(outputPath);
                    OutPutText(response, resultContent);
                    response.Close();
                    File.Delete(outputPath);
                }
                else
                {
                    OutPutText(response, "error: timeout");
                    return;
                }
            }
            catch (Exception exp)
            {
                logger.Error(exp.Message);
                logger.Error(exp.StackTrace);
            }
        }

        private static void ProcessCallback(string rawUrl, HttpListenerRequest request)
        {
            try
            {
                //parse rawUrl ep: callback?sessionId=xxx
                if (rawUrl.IndexOf("?sessionId=") != -1)
                {
                    string sessionId = rawUrl.Substring(rawUrl.IndexOf("?sessionId=") + 11);
                    logger.Info("callback sessionId: " + sessionId);
                    StreamReader reader = new StreamReader(request.InputStream);
                    string callBackText = reader.ReadToEnd();
                    logger.Info("callBackText: " + callBackText);
                    if (callbackDict.ContainsKey(sessionId))
                    {
                        callbackDict[sessionId] = callBackText;
                    }
                    else
                    {
                        callbackDict.Add(sessionId, callBackText);
                    }
                    if (eventWaitDict.ContainsKey(sessionId))
                    {
                        logger.Info("callback set the wait handle");
                        eventWaitDict[sessionId].Set();
                    }
                    else
                    {
                        logger.Info("callback has not the wait handle");
                    }
                }
            }
            catch (Exception exp)
            {
                logger.Error(exp.Message);
                logger.Error(exp.StackTrace);
            }
        }
        /// <summary>
        /// 生成临时的回话ID
        /// </summary>
        /// <returns></returns>
        private static string CreateSessionId()
        {
            DateTime dt = DateTime.Now;
            Random rd = new Random();
            return dt.Hour.ToString() + dt.Minute.ToString() + dt.Second.ToString() + rd.Next(999).ToString();
        }
        /// <summary>
        /// 输出文本
        /// </summary>
        /// <param name="response"></param>
        /// <param name="responseString"></param>
        private static void OutPutText(HttpListenerResponse response, string responseString)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            response.Close();
        }
    }
}
