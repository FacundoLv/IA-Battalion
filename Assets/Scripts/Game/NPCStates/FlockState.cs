using Core;

internal class FlockState : BaseState
{
    private FlockEntity _flock;
    private readonly IUnit _unit;

    public FlockState(IUnit unit)
    {
        _unit = unit;
        _flock = unit.GameObject.GetComponent<FlockEntity>();
        _flock.FlockID = unit.TeamID;
    }

    public override void Awake()
    {
    }

    public override void Execute()
    {
        var flockDir = _flock.GetDir();
        _unit.Move(flockDir.normalized);
    }

    public override void Sleep()
    {
        _unit.Stop();
    }
}