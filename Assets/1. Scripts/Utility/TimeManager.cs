using System;
using System.Collections;
using UnityEngine;
using NaughtyAttributes;

public class TimeManager : StaticInstance<TimeManager>
{
    [Serializable]
    public struct GameTime
    {
        [SerializeField, MinValue(1), AllowNesting] public int day;
        [SerializeField, MinValue(0), MaxValue(23), AllowNesting] public int hour;
        [SerializeField, MinValue(0), MaxValue(59), AllowNesting] public int minute;

        public void AddMinute()
        {
            if (minute == 59)
            {
                if (hour == 23)
                {
                    day += 1;
                    hour = 0;
                }
                else
                {
                    hour += 1;
                }
                minute = 0;
            }
            else
            {
                minute += 1;
            }
        }

        public override readonly string ToString()
        {
            return String.Format("day {0} - {1:D2}:{2:D2}", day, hour, minute);
        }
    }

    [Space(10)]
    [SerializeField] public bool TrackGameTime;
    [ShowIf("TrackGameTime"), SerializeField, MinValue(1), MaxValue(60)] public int GameDayRealTimeMins = 30;
    [ShowIf("TrackGameTime"), SerializeField] private GameTime StartGameTime;
    [Space(10)]
    [SerializeField] private bool ShowInternal;
    [ShowIf("ShowInternal"), SerializeField, ReadOnly] private uint CurrentTick;
    [ShowIf("ShowInternal"), SerializeField, ReadOnly] private double TickDelta;
    [ShowIf(EConditionOperator.And, "TrackGameTime", "ShowInternal"), SerializeField, ReadOnly] private double GameMinRealTimeSecs;
    [ShowIf(EConditionOperator.And, "TrackGameTime", "ShowInternal"), SerializeField, ReadOnly] private GameTime CurrentGameTime;
    [ShowIf(EConditionOperator.And, "TrackGameTime", "ShowInternal"), SerializeField, ReadOnly] private double NextGameMinWaitTime;

    public event Action OnTick;
    public event Action OnGameMinutePassed;
    public event Action OnGameHourPassed;
    public event Action OnGameDayPassed;

    private Coroutine gameTimeCoroutine;

    public void ResetGameTime()
    {
        if (gameTimeCoroutine != null)
        {
            StopCoroutine(gameTimeCoroutine);
        }
        Init();
    }

    public uint GetCurrentTick()
    {
        return CurrentTick;
    }

    public double GetTickDelta()
    {
        return TickDelta;
    }

    public void SetGameTime(GameTime gameTime)
    {
        GameTime prevGameTime = CurrentGameTime;
        CurrentGameTime = gameTime;
        InvokeEventsOnTimeChange(prevGameTime, CurrentGameTime);
    }

    public void InvokeEventsOnTimeChange(GameTime oldTime, GameTime newTime)
    {
        // Probably best to invoke callbacks in the order minute->hour->day
        // so store these first and check+invoke at the end
        bool minutePassed = false;
        bool hourPassed = false;
        bool dayPassed = false;
        if (newTime.day > oldTime.day)
        {
            minutePassed = true;
            hourPassed = true;
            dayPassed = true;
        }
        else if (newTime.day == oldTime.day && newTime.hour > oldTime.hour)
        {
            minutePassed = true;
            hourPassed = true;
        }
        else if (newTime.day == oldTime.day && newTime.hour == oldTime.hour && newTime.minute > oldTime.minute)
        {
            minutePassed = true;
        }

        if (minutePassed)
        {
            OnGameMinutePassed?.Invoke();
            if (hourPassed)
            {
                OnGameHourPassed?.Invoke();
                if (dayPassed)
                {
                    OnGameDayPassed?.Invoke();
                }
            }
        }
    }

    public GameTime GetGameTime()
    {
        return CurrentGameTime;
    }

    public double TicksToTime(uint ticks)
    {
        return (TickDelta * ticks);
    }

    public double TimePassed(uint previousTick)
    {
        return TicksToTime(CurrentTick - previousTick);
    }

    public double TimePassed(uint currentTick, uint previousTick)
    {
        double mult;
        double ret;
        if (currentTick >= previousTick)
        {
            mult = 1f;
            ret = TicksToTime(currentTick - previousTick);
        }
        else
        {
            mult = -1f;
            ret = TicksToTime(previousTick - currentTick);
        }

        return ret * mult;
    }

    private void Init()
    {
        CurrentTick = 0;
        TickDelta = Time.fixedDeltaTime;
        if (TrackGameTime)
        {
            CurrentGameTime = StartGameTime;
            GameMinRealTimeSecs = GameDayRealTimeMins / 24d;
            NextGameMinWaitTime = GameMinRealTimeSecs;
            gameTimeCoroutine = StartCoroutine(GameTimeLoop());
        }
    }

    private void Start()
    {
        Init();
    }

    private void FixedUpdate()
    {
        // TODO:
        // Ideally this would not be tied to FixedUpdate so we could have our own
        // TickRate, and set TickDelta as 1/TickRate. However, unfortunately Unity
        // is shit for tracking time accurately so this is easiest right now.
        CurrentTick++;
        OnTick?.Invoke();
    }

    private IEnumerator GameTimeLoop()
    {
        while (true)
        {
            double start = Time.timeAsDouble;
            yield return new WaitForSeconds((float)NextGameMinWaitTime);
            GameTime prevGameTime = CurrentGameTime;
            CurrentGameTime.AddMinute();
            InvokeEventsOnTimeChange(prevGameTime, CurrentGameTime);
            double end = Time.timeAsDouble;
            NextGameMinWaitTime = GameMinRealTimeSecs - (((end - start) - NextGameMinWaitTime));
        }
    }
}
