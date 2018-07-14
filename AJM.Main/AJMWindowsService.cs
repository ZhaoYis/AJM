using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace AJM.Main
{
    partial class AJMWindowsService : ServiceBase
    {
        private readonly JobManage _manage = null;

        public AJMWindowsService()
        {
            InitializeComponent();
            _manage = new JobManage();
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            // TODO: 在此处添加代码以启动服务。
            _manage.JobStart();
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void OnStart()
        {
            // TODO: 在此处添加代码以启动服务。
            _manage.JobStart();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        protected override void OnStop()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
            _manage.JobStop();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void OnStopService()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
            _manage.JobStop();
        }
    }
}
