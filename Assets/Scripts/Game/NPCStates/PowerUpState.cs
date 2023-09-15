using Core;
using UnityEngine;

public class PowerUpState : BaseState
{
    private readonly BossUnit _unit;
    private int _duration = 2;

    public PowerUpState(BossUnit unit)
    {
        _unit = unit;
    }

    public override void Awake()
    {
        var boost = new BoostData(_duration, 2f);
        _unit.BoostDamage(boost);
        _unit.Stop();
    }

    public override void Execute()
    {
    }

    public override void Sleep()
    {
        var newDuration = _duration + 1;
        while (!IsPrime(newDuration)) newDuration++;
        _duration = newDuration;
    }

    private static bool IsPrime(int number)
    {
        for (var i = 2; i <= number / 2; i++)
        {
            if (number % i == 0) return false;
        }
        return true;
    }
}
