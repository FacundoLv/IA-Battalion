using System.Collections.Generic;
using Core.Utils;
using UnityEngine;

[RequireComponent(typeof(FlockEntity))]
public class AlineationBehavior : MonoBehaviour, IFlockBehavior
{
    public float alineationWeight;

    public Vector3 GetDir(List<IFlockEntity> entities, IFlockEntity entity)
    {
        var dir = Vector3.zero;
        foreach (var currEntity in entities)
            dir += currEntity.Direction;

        dir /= entities.Count;
        return dir.CancelIfBelowThreshold(0.1f).normalized * alineationWeight;
    }
}