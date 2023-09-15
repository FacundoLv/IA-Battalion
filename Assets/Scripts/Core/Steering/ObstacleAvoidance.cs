using UnityEngine;

public class ObstacleAvoidance : ISteering
{
    public bool Detected { get; private set; }

    private readonly Transform _from;
    private readonly float _radius;
    private readonly float _avoidWeight;
    private readonly LayerMask _mask;
    private readonly int _rayAmount;
    private readonly float _angle;

    public ObstacleAvoidance(Transform from, float radius, float avoidWeight, LayerMask mask, int rayAmount,
        float angle)
    {
        _avoidWeight = avoidWeight;
        _radius = radius;
        _mask = mask;
        _from = from;
        _rayAmount = rayAmount;
        _angle = angle;
    }

    public Vector3 GetDir()
    {
        var delta = Vector3.zero;
        var detected = false;
        for (var i = 0; i < _rayAmount; i++)
        {
            var rotation = Quaternion.AngleAxis(i * 2 * _angle / (_rayAmount - 1) - _angle, _from.up);
            var direction = _from.rotation * rotation * Vector3.forward;

            if (Physics.Raycast(_from.position, direction, _radius, _mask))
            {
                delta -= 1f / _rayAmount * direction;
                detected = true;
            }
            else
            {
                delta += 1f / _rayAmount * direction;
            }
        }

        Detected = detected;
        return delta.normalized * _avoidWeight;
    }
}