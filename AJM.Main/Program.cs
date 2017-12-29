using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

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

            JobManage job = new JobManage();
            job.JobStart();
        }
    }
}
