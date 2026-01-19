using BlazorLayout.Enums;
using BlazorLayout.Modeles;

using Timer = System.Timers.Timer;

namespace BlazorLayout.Shared;

    public class ActivityTimerService : IDisposable
    {
    private Timer? timer;
    public bool IsRunning { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? StopTime { get; private set; }
    public TimeSpan Elapsed { get; private set; }
    private DateTimeOffset startTimestamp;

    public event Action? OnTick;
    public ActivityTypeDto ActiveActivityType { get; private set; }
    public ActivityDto? CurrentActivity { get; private set; }

    public void StartActivity(ActivityTypeDto activityType, ActivityDto activity)
    {
        ActiveActivityType = activityType;
        CurrentActivity = activity;
    }


    public void StopActivity()
    {
        ActiveActivityType = ActivityTypeDto.Unspecified;
        CurrentActivity = null;
    }
    public void Start()
    {
        IsRunning = true;
        startTimestamp = DateTimeOffset.Now;

        StartTime = TimeOnly.FromDateTime(startTimestamp.DateTime);
        Elapsed = TimeSpan.Zero;

        timer = new Timer(1000);
        timer.Elapsed += (_, _) =>
        {
            var currentTime = DateTimeOffset.Now;
            Elapsed = currentTime - startTimestamp;
            OnTick?.Invoke();
        };
        timer.Start();
    }

    public void Stop()
    {
        if (IsRunning)
        {
            timer?.Stop();

            var now = DateTimeOffset.Now;
            Elapsed = now - startTimestamp;

            StopTime = TimeOnly.FromDateTime(now.DateTime);
        }

        IsRunning = false;
    }

    public void Dispose()
    {
        timer?.Dispose();
    }

    public void ResumeFrom(TimeSpan startedAt)
    {
        IsRunning = true;

        var now = DateTimeOffset.Now;
        startTimestamp = new DateTimeOffset(
            now.Year,
            now.Month,
            now.Day,
            startedAt.Hours,
            startedAt.Minutes,
            startedAt.Seconds,
            now.Offset);

        Elapsed = DateTimeOffset.Now - startTimestamp;

        timer?.Dispose();
        timer = new Timer(1000);
        timer.Elapsed += (_, _) =>
        {
            Elapsed = DateTimeOffset.Now - startTimestamp;
            OnTick?.Invoke();
        };
        timer.Start();
    }

    public void Reset()
    {
        Stop();
        StartTime = null;
        StopTime = null;
        Elapsed = TimeSpan.Zero;
    }

}

