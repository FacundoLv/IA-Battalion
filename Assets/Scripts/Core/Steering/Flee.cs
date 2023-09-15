using UnityEngine;

public class Flee : ISteering
{
    private readonly Transform _target;
    private readonly Transform _from;

    public Flee(Transform from, Transform target)
    {
        _target = target;
        _from = from;
    }

    public Vector3 GetDir() => (_from.position - _target.position).normalized;
}