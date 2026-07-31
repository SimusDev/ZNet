using Godot;

namespace ArmaMovement.Components {
    [GlobalClass]
    public partial class Movement : Node
    {
        [Export] private PlayerHead _playerHead;
        [Export] private MovementController _movementController;
        [Export] private Node3D _body;

        [ExportGroup("Settings")]
        [Export] private float _turnSpeed = 3.0f;

        [Export] private float _gravityMultiplier = 1.0f;
        private float _gravity = 0.0f;
        //[Export] private float _angleThreshold = 0.1f;

        Movement()
        {
            _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity", 9.8f).As<float>();
        }

        public override void _Ready()
        {
            _movementController.StateMachine.EmplaceState<States.Movement.Idle>();
            _movementController.StateMachine.EmplaceState<States.Movement.Running>();
            _movementController.StateMachine.EmplaceState<States.Movement.Walk>();
            _movementController.StateMachine.EmplaceState<States.Movement.EasyRunning>();
            _movementController.StateMachine.EmplaceState<States.Movement.Air>();
            _movementController.StateMachine.EmplaceState<States.Movement.Crouched>();
            _movementController.StateMachine.EmplaceState<States.Movement.CrouchedWalking>();
            _movementController.StateMachine.EmplaceState<States.Movement.CrouchedRunning>();
            _movementController.StateMachine.EmplaceState<States.Movement.ProneMoving>();

            bool isAthority = IsMultiplayerAuthority();
            SetProcess(isAthority);
            SetPhysicsProcess(isAthority);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_playerHead.CharacterBody.IsOnFloor())
            {
                _GroundPhysics((float)delta);
            }
            else
            {
                _AirPhysics((float)delta);
            }

            float maxTurn = _turnSpeed * (float)delta;
            float newY = Mathf.LerpAngle(
                _body.Rotation.Y,
                _playerHead.Rotation.Y,
                maxTurn
                );
            
            _body.Rotation = new Vector3(
                _body.Rotation.X,
                newY,
                _body.Rotation.Z
                );
            

            _playerHead.CharacterBody.MoveAndSlide();
        }

        private void _AirPhysics(float delta)
        {
            _movementController.StateMachine.ChangeState<States.Movement.Air>();
            _playerHead.CharacterBody.Velocity = new Vector3()
            {
                X = _playerHead.CharacterBody.Velocity.X,
                Y = _playerHead.CharacterBody.Velocity.Y - (_gravity * _gravityMultiplier) * delta,
                Z = _playerHead.CharacterBody.Velocity.Z
            };
        }

        private void _GroundPhysics(float delta)
        {
            bool wantsToMove = _movementController.MoveInput.LengthSquared() > 0.001f;

            if (wantsToMove)
            {
                if (_movementController.WantsCrouch)
                {
                    if (_movementController.WantsSprint)
                        _movementController.StateMachine.ChangeState<States.Movement.CrouchedRunning>();
                    else
                        _movementController.StateMachine.ChangeState<States.Movement.CrouchedWalking>();
                }
                else if (_movementController.WantsProne)
                {
                    _movementController.StateMachine.ChangeState<States.Movement.ProneMoving>();
                }
                else
                {
                    if (_movementController.WantsSprint)
                        _movementController.StateMachine.ChangeState<States.Movement.Running>();
                    else if (_movementController.WantsSlowDown)
                        _movementController.StateMachine.ChangeState<States.Movement.Walk>();
                    else
                        _movementController.StateMachine.ChangeState<States.Movement.EasyRunning>();
                }
            }
            else
            {
                if (_movementController.WantsCrouch)
                {
                    _movementController.StateMachine.ChangeState<States.Movement.Crouched>();
                }
                else if (_movementController.WantsProne)
                {
                    _movementController.StateMachine.ChangeState<States.Movement.Prone>();
                }
                else
                {
                    _movementController.StateMachine.ChangeState<States.Movement.Idle>();
                }
            }

            float targetSpeed = 0.0f;
            if (_movementController.StateMachine.CurrentState is States.Movement.Base moveState)
            {
                targetSpeed = moveState.Speed;
            }

            
            Vector3 moveDirection = (_body.GlobalTransform.Basis * _movementController.MoveInput).Normalized();

            _playerHead.CharacterBody.Velocity = new(
                moveDirection.X * targetSpeed,
                _playerHead.CharacterBody.Velocity.Y,
                moveDirection.Z * targetSpeed
            );
        }

        private void _RotateBodyTowards(float delta)
        {
            
        }
    }
}