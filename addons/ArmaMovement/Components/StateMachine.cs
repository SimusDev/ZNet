using Godot;
using System;
using System.Collections.Generic;

namespace ArmaMovement.Components
{
    [GlobalClass]
    public partial class StateMachine : Node
    {
        private Dictionary<Type, States.Base> _states = [];

        private States.Base _currentState;
        public States.Base CurrentState => _currentState;

        public void EmplaceState<T>() where T : States.Base, new()
        {
            T state = new();
            state.OnStateAdded(this);
            _states[state.GetType()] = state;
        }

        public void RemoveState<T>() where T : States.Base
        {
            Type type = typeof(T);
            if (_states.TryGetValue(type, out var state))
            {
                state.OnStateRemoved(this);
                _states.Remove(type);
                if (_currentState == state)
                    _currentState = null;
            }
        }

        public States.Base GetState<T>() where T : States.Base
        {
            if (_states.TryGetValue(typeof(T), out States.Base state))
                return state;
            
            return null;
        }
        public bool TryGetState<T>(out States.Base state) where T : States.Base
        {
            state = GetState<T>();
            return state != null;
        }

        public void ChangeState<T>() where T : States.Base
        {
            Type type = typeof(T);
            if (!_states.TryGetValue(type, out States.Base newState))
                return;

            if (_currentState == newState)
                return;

            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        public bool CurrentStateIs<T>() where T : States.Base => _currentState is T;

        public override void _Ready()
        {
            
        }

        public override void _Process(double delta)
        {
            _currentState?.Update(delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            _currentState?.PhysicsUpdate(delta);
        }
    }
}