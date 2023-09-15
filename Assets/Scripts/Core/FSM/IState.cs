namespace Core
{
    public interface IState
    {
        void Awake();
        void Execute();
        void Sleep();
        void AddTransition(IState state);
        void RemoveTransition(IState state);
        IState GetState(IState input);
    }
}