using System.Collections.Generic;
using Core.Utils;
using UnityEngine;

[RequireComponent(typeof(FlockEntity))]
public class SeparationBehavior : MonoBehaviour, IFlockBehavior
{
    public float separationWeight;
    public float range;

    public Vector3 GetDir(List<IFlockEntity> entities, IFlockEntity entity)
    {
        var dir = Vector3.zero;
        foreach (var currEntity in entities)
        {
            if (Vector3.SqrMagnitude(currEntity.Position - entity.Position) >= range * range) continue;
            var currDir = entity.Position - currEntity.Position;
            var distance = currDir.magnitude;
            currDir = currDir.normalized * (range - distance);
            dir += currDir;
        }

        return dir.CancelIfBelowThreshold(0.1f).normalized * separationWeight;
    }
}
