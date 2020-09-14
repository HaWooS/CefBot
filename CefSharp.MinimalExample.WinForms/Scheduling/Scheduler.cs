using System;

public static class Scheduler
{
    public static void IntervalInDays(int hour, int min, int interval, Action task)
    {
        interval = interval * 24;
        SchedulerService.Instance.ScheduleNewTask(hour, min, interval, task);
    }
}
