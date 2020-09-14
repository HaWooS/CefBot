using System;

public class SchedulerService
{
    private static SchedulerService Instance;
    private Timer timer;
    private SchedulerService()
    {
    }
    public static SchedulerService Instance => Instance ?? (Instance = new SchedulerService());

    public void ScheduleNewTask(double interval, Action task)
    {
        DateTime now = DateTime.Now;
        DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0);
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
         }, TimeSpan.FromHours(interval));

            

    }

}
