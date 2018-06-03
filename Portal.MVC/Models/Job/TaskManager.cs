using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentScheduler;

namespace Portal.MVC.Models.Job
{
    public class TaskManager : Registry
    {
        public TaskManager()
        {
             Schedule<CheckQuestionTimeJob>().ToRunEvery(1).Days();
            //Schedule<CheckQuestionTimeJob>().ToRunEvery(2).Hours();
        }

    }
}