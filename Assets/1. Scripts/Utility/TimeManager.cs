using System;
using System.Collections;
using UnityEngine;

public class TimeManager : StaticInstance<TimeManager>
{
    [SerializeField] private double TickRate = 64f;
    [SerializeField][HideInInspector] public double TickDelta { get; private set; }
    [SerializeField][HideInInspector] public uint Tick { get; private set; }

    [SerializeField][HideInInspector] private double nextWaitTime;

    public event Action OnTick;

    private void Start()
    {
        Tick = 0;
        TickDelta = 1 / TickRate;
        nextWaitTime = TickDelta;
        StartCoroutine(TickLoop());
    }

    public double TicksToTime(uint ticks)
    {
        return (TickDelta * ticks);
    }

    public double TimePassed(uint previousTick)
    {
        return TicksToTime(Tick - previousTick);
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

    private IEnumerator TickLoop()
    {
        double start = Time.timeAsDouble;
        yield return new WaitForSeconds((float)TickDelta);
        Tick++;
        OnTick?.Invoke();
        double end = Time.timeAsDouble;
        nextWaitTime = TickDelta - ((end - start) - TickDelta);
        Debug.LogFormat("WaitTime = {0}", nextWaitTime);
    }
}
