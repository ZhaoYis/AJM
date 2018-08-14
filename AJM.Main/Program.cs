using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Topshelf;

namespace AJM.Main
{
    class Program
    {
        static void Main(string[] args)
        {
            //ServiceBase[] servicesToRun = new ServiceBase[]
            //{
            //    new AJMWindowsService()
            //};
            //ServiceBase.Run(servicesToRun);

            //HostFactory.Run(x =>
            //{
            //    x.Service<AJMWindowsService>(s =>
            //    {
            //        s.ConstructUsing(name => new AJMWindowsService());

            //        s.WhenStarted(tc => tc.OnStart());
            //        s.WhenStopped(tc => tc.OnStopService());
            //    });
            //    x.RunAsLocalSystem();

            //    x.SetDescription("XXX服务描述");
            //    x.SetDisplayName("显示名称");
            //    x.SetServiceName("服务名称");
            //});

            JobManage job = new JobManage();
            job.JobStart();
        }
    }
}
