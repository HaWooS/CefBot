using System;

public static class Scheduler
{
	public static void IntervalInDays(int interval, Action task)
    {
        interval = interval * 24;
        SchedulerService.Instance.ScheduleTask(interval, task);
    }
}
