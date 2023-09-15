using System;

public class ActionNode : INode
{
    private readonly Action _myAction;

    public ActionNode(Action myAction)
    {
        _myAction = myAction;
    }

    public void Execute() => _myAction();
}
