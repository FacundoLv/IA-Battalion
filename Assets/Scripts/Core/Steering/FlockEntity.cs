using System.Collections.Generic;
using UnityEngine;

public class FlockEntity : MonoBehaviour, ISteering, IFlockEntity
{
    public int FlockID { get; set; }
    public Vector3 Direction => _dir;
    public Vector3 Position => transform.position;

    public float radius;
    public LayerMask entityMask;

    private List<IFlockBehavior> _behaviors;
    private Vector3 _dir;

    private void Awake()
    {
        var behaviors = GetComponents<IFlockBehavior>();
        _behaviors = new List<IFlockBehavior>(behaviors);
    }

    public Vector3 GetDir()
    {
        var entities = new List<IFlockEntity>();
        foreach (var entity in Physics.OverlapSphere(transform.position, radius, entityMask))
        {
            if (entity.transform.position == transform.position) continue;
            var newEntity = entity.GetComponent<IFlockEntity>();
            if (newEntity != null && newEntity.FlockID == FlockID) entities.Add(newEntity);
        }

        _dir = Vector3.zero;
        foreach (var behaviour in _behaviors)
            _dir += behaviour.GetDir(entities, this);

        return _dir.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}