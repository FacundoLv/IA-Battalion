using UnityEngine;

public class Evade : ISteering
{
    private readonly Transform _from;
    private readonly Rigidbody _rbTarget;
    private readonly float _timePrediction;

    public Evade(Transform from, Rigidbody rbTarget, float timePrediction)
    {
        _from = from;
        _rbTarget = rbTarget;
        _timePrediction = timePrediction;
    }

    public Vector3 GetDir()
    {
        var prediction = _rbTarget.transform.position + _rbTarget.velocity * _timePrediction;
        return (_from.position - prediction).normalized;
    }
}