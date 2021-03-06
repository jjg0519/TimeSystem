﻿using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSystem
{
    public class TaskHelper
    {
        public static IScheduler sche = null;
        public static IEnumerable<Schedule_t> Schedules
        {
            get
            {
                TaskEntities db = new TaskEntities();
                var list = db.Schedule_t.ToList();
                list.ForEach(i =>
                {
                    i.application = db.Application_t.FirstOrDefault(p => p.Uid == i.ApplicationUid);
                });
                return list;
            }
        }

        static TaskHelper()
        {
            sche = new StdSchedulerFactory().GetScheduler();
            sche.Start();
        }

        /// <summary>
        /// 再次加载任务
        /// </summary>
        internal static void Sche()
        {
            sche.Clear();
            var list = Schedules.ToList();
          

            list.ForEach(i =>
            {
                if (!Directory.Exists(i.realLogPath))
                {
                    Directory.CreateDirectory(i.realLogPath);
                }
                if (i.Enable == 1)
                {
                    JobDataMap map = new JobDataMap();
                    map.Add("Schedule", i);
                    var uid = i.Uid.ToString();
                    IJobDetail job = JobBuilder.Create<FullTask>()
                    .UsingJobData(map)
                    .WithIdentity(uid, uid)
                    .Build();


                    if (i.Repeat == 0)
                    {
                        if(i.StartAt.HasValue)
                        sche.ScheduleJob(job, TriggerBuilder.Create().StartAt(new DateTimeOffset(i.StartAt.Value)).WithIdentity("tri_" + uid, "tri_" + uid).Build());
                        //TriggerBuilder.Create().StartAt(DateTimeOffset.);
                        
                    }
                    else
                    {
                        ICronTrigger tri = (ICronTrigger)TriggerBuilder.Create()
                        .StartNow()
                        .WithIdentity("tri_" + uid, "tri_" + uid)
                        .WithCronSchedule(i.Cron)
                        .Build();
                        sche.ScheduleJob(job, tri);

                    }



                }
            });
        }
    }
}
