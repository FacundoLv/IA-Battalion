using System.Collections.Generic;
using Core.Utils;
using UnityEngine;

[RequireComponent(typeof(FlockEntity))]
public class PredatorBehavior : MonoBehaviour, IFlockBehavior
{
    public float predatorWeight;
    public LayerMask mask;
    public float range;

    public Vector3 GetDir(List<IFlockEntity> entities, IFlockEntity entity)
    {
        var obj = Physics.OverlapSphere(entity.Position, range, mask);
        var dir = Vector3.zero;
        foreach (var currEntity in obj)
        {
            var currDir = entity.Position - currEntity.transform.position;
            var distance = currDir.magnitude;
            currDir = currDir.normalized * Mathf.Clamp(range - distance, 0, range);
            dir += currDir;
        }

        return  dir.CancelIfBelowThreshold(0.1f).normalized * predatorWeight;
    }
}
