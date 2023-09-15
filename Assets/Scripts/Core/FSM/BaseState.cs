using System;
using System.Collections.Generic;

namespace Core
{
    public abstract class BaseState : IState
    {
        private readonly Dictionary<Type, IState> _dic = new Dictionary<Type, IState>();
    
        public abstract void Awake();

        public abstract void Execute();

        public abstract void Sleep();

        public void AddTransition(IState state)
        {
            var key = state.GetType();
            if (!_dic.ContainsKey(key))
                _dic.Add(key, state);
        }

        public void RemoveTransition(IState state)
        {
            var key = state.GetType();
            if (_dic.ContainsKey(key))
                _dic.Remove(key);
        }

        public IState GetState(IState input)
        {
            var key = input.GetType();
            return _dic.ContainsKey(key) ? _dic[key] : null;
        }
    }
}