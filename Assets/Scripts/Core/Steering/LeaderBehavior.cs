using System.Collections.Generic;
using Core.Utils;
using UnityEngine;

[RequireComponent(typeof(FlockEntity))]
public class LeaderBehavior : MonoBehaviour, IFlockBehavior
{
    public float leaderWeight;
    public Transform target;
    
    [SerializeField]
    private float _stoppingDistance = .01f;

    public Vector3 GetDir(List<IFlockEntity> entities, IFlockEntity entity)
    {
        var direction = target.position - entity.Position;
        return direction.CancelIfBelowThreshold(_stoppingDistance).normalized * leaderWeight;
    }
}
