using System;
using System.Threading;

public class SchedulerService
{
    private static SchedulerService _instance;
    private Timer timer;
    private SchedulerService()
    {
    }
    public static SchedulerService Instance => _instance ?? (_instance = new SchedulerService());

    public void ScheduleNewTask(int hour, int min, int interval, Action task)
    {
        DateTime now = DateTime.Now;
        DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, hour, min, 0, 0);
        if (now > firstRun)
        {
            firstRun.AddDays(1);
        }

        TimeSpan timeToGo = firstRun - now;
        if (timeToGo <= TimeSpan.Zero)
        {
            timeToGo = TimeSpan.Zero;
        }
        var timer = new Timer(x =>
        {
            task.Invoke();
        }, null, timeToGo, TimeSpan.FromHours(interval)); ;
        Console.Write("Next invoke will take a place in 1 day");


    }

}
