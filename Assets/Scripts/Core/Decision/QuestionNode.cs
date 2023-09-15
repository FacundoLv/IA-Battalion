using System;

public class QuestionNode : INode
{
    private readonly Func<bool> _myQuestion;
    private readonly INode _nodeTrue;
    private readonly INode _nodeFalse;

    public QuestionNode(Func<bool> myQuestion, INode nodeTrue, INode nodeFalse)
    {
        _myQuestion = myQuestion;
        _nodeFalse = nodeFalse;
        _nodeTrue = nodeTrue;
    }

    public void Execute()
    {
        if (_myQuestion())
            _nodeTrue.Execute();
        else
            _nodeFalse.Execute();
    }
}
