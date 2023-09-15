using System.Collections.Generic;
using Core.Utils;
using UnityEngine;

[RequireComponent(typeof(FlockEntity))]
public class CohesionBehavior : MonoBehaviour, IFlockBehavior
{
    public float CohesionWeight;

    public Vector3 GetDir(List<IFlockEntity> entities, IFlockEntity entity)
    {
        var center = Vector3.zero;
        foreach (var currEntity in entities)
            center += currEntity.Position;

        center /= entities.Count;
        return (center - entity.Position).CancelIfBelowThreshold(0.1f).normalized * CohesionWeight;
    }
}
