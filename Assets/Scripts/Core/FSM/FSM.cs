namespace Core
{
    public class FSM
    {
        public IState Current { get; private set; }

        public FSM(IState init = null)
        {
            if (init != null) SetInit(init);
        }

        public void SetInit(IState init)
        {
            Current = init;
            Current.Awake();
        }

        public void OnUpdate()
        {
            Current?.Execute();
        }

        public void Transition(IState input)
        {
            var newState = Current.GetState(input);
            if (newState == null) return;
            Current.Sleep();
            newState.Awake();
            Current = newState;
        }
    }
}