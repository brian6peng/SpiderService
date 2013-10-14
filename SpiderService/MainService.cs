using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Net;
using System.Configuration;
using System.Threading;

namespace SpiderService
{
    public partial class MainService : ServiceBase
    {
        HttpListener httpListener = null;
        EZLogger logger = null;

        public MainService()
        {
            InitializeComponent();
            logger = EZLogger.CreateInstance();
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("Service Starting");
            if (!HttpListener.IsSupported)
            {
                logger.Fatal("操作系统不支持");
                return;
            }
            //设置线程池最大线程数量
            int MaxRequestCount = 10;
            int.TryParse(ConfigurationManager.AppSettings["MaxRequest"], out MaxRequestCount);
            ThreadPool.SetMaxThreads(MaxRequestCount, MaxRequestCount);
            //初始化HttpListener
            httpListener = new HttpListener();
            httpListener.IgnoreWriteExceptions = false;
            httpListener.Prefixes.Add(ConfigurationManager.AppSettings["HostName"]);
            httpListener.Start();
            httpListener.BeginGetContext(new AsyncCallback(RequestProcess.ProcessMethod), httpListener);
            logger.Info("Wait For Request...");
        }

        protected override void OnStop()
        {
            logger.Info("Service Stoping");
            if (httpListener.IsListening)
            {
                httpListener.Stop();
            }
            httpListener.Close();
        }
    }
}
                    