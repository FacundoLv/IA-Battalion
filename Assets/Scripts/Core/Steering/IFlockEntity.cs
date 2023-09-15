using UnityEngine;

public interface IFlockEntity
{
    int FlockID { get; set; }
    Vector3 Direction { get; }
    Vector3 Position { get; }
}