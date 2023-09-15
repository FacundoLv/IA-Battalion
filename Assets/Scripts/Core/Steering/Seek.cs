using UnityEngine;

public class Seek : ISteering
{
    private readonly Transform _target;
    private readonly Transform _from;

    public Seek(Transform from, Transform target)
    {
        _target = target;
        _from = from;
    }

    public Vector3 GetDir() => (_target.position - _from.position).normalized;
}