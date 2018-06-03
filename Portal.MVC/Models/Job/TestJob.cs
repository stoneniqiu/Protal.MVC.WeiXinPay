using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentScheduler;

namespace Portal.MVC.Models.Job
{
    public class TestJob:IJob
    {
        public void Execute()
        {
            Console.WriteLine("执行了");
        }
    }
}