
using UnityEngine;

public class TimedEvent
{
    private float _interval;
    private float _chance;

    private float _lastAttemptTime;
    private int _failed = 0;
    
    public TimedEvent(float interval, float chance)
    {
        _interval = interval;
        _chance = chance;
        _lastAttemptTime = Time.time;
    }

    public bool Occured()
    {
        if (_lastAttemptTime + _interval < Time.time)
        {
            _lastAttemptTime = Time.time;
            
            if (Random.value < _chance)
            {
                _failed = 0;
                return true;
            }
            else
            {
                _failed++;
            }
        }

        return false;
    }

    public void SetChance(float chance)
    {
        _chance = chance;
    }
}
