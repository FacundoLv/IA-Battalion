using System.Collections.Generic;
using UnityEngine;

public class Roulette
{
    public string Execute(Dictionary<string, int> actions)
    {
        var totalWeight = 0;
        foreach (var item in actions)
            totalWeight += item.Value;

        var random = Random.Range(0, totalWeight);
        foreach (var item in actions)
        {
            random -= item.Value;
            if (random < 0) return item.Key;
        }

        return default;
    }
}
