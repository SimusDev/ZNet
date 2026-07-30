using Godot;
using System;
using System.Collections.Generic;

namespace ArmaMovement
{
    [GlobalClass]
    public partial class PlayerStateMachine : Node
    {
        private Dictionary<Type, States.BaseState> _states = new();
        private States.BaseState _currentState;

        [Export] public NodePath PlayerControllerPath;
        private PlayerController _playerController;

        public override void _Ready()
        {
            _playerController = GetNode<PlayerController>(PlayerControllerPath);

            AddState(new States.Idle());
            AddState(new States.Walking());
            AddState(new States.Running());
            AddState(new States.Crouching());
            AddState(new States.Prone());

            ChangeState<States.Idle>();
        }

        private void AddState(States.BaseState state)
        {
            state.Initialize(_playerController);
            _states[state.GetType()] = state;
        }

        public void ChangeState<T>() where T : States.BaseState
        {
            Type type = typeof(T);
            if (!_states.TryGetValue(type, out States.BaseState newState))
                return;

            if (_currentState == newState)
                return;

            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
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