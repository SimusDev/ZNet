using Godot;
using System;

namespace ArmaMovement {
    [GlobalClass]
    public partial class PlayerMovement : Node
    {
        [ExportGroup("Speeds")]
        [Export] public float WalkSpeed = 3.0f;
        [Export] public float RunSpeed = 6.0f;
        [Export] public float CrouchSpeed = 1.5f;
        [Export] public float ProneSpeed = 0.8f;
        [Export] public float AimWalkSpeed = 2.0f;

        [ExportGroup("Acceleration")]
        [Export] public float Acceleration = 10.0f;
        [Export] public float Deceleration = 10.0f;
        [Export] public float AirControl = 0.3f;

        [ExportGroup("Rotation")]
        [Export] public float TurnSpeed = 180.0f; // градусов в секунду

        [ExportGroup("Physics")]
        [Export] public float Gravity = -9.8f;

        private CharacterBody3D _characterBody;
        private Node3D _bodyMesh;
        private float _currentSpeed = 0.0f;
        private Vector3 _moveDirection = Vector3.Zero;

        public void Initialize(CharacterBody3D characterBody, Node3D bodyMesh)
        {
            _characterBody = characterBody;
            _bodyMesh = bodyMesh;
        }

        public void UpdateMovement(
            float delta,
            Vector2 moveInput,
            bool isSprinting,
            bool isAiming,
            bool isCrouching,
            bool isProne,
            bool isGrounded,
            Vector3 cameraForward,
            Vector3 cameraRight)
        {
            // Определение целевой скорости
            float targetSpeed = 0f;
            if (isProne)
                targetSpeed = ProneSpeed;
            else if (isCrouching)
                targetSpeed = CrouchSpeed;
            else if (isAiming)
                targetSpeed = AimWalkSpeed;
            else if (isSprinting)
                targetSpeed = RunSpeed;
            else
                targetSpeed = WalkSpeed;

            if (moveInput.LengthSquared() < 0.001f)
                targetSpeed = 0f;

            // Направление движения относительно камеры
            Vector3 moveDirection = (cameraForward * moveInput.Y + cameraRight * moveInput.X).Normalized();
            if (moveDirection.LengthSquared() > 0.001f)
            {
                _moveDirection = moveDirection;
            }

            // Плавное изменение скорости
            if (isGrounded)
            {
                float accel = targetSpeed > _currentSpeed ? Acceleration : Deceleration;
                _currentSpeed = Mathf.MoveToward(_currentSpeed, targetSpeed, accel * delta);
            }
            else
            {
                float accel = targetSpeed > _currentSpeed ? Acceleration * AirControl : Deceleration * AirControl;
                _currentSpeed = Mathf.MoveToward(_currentSpeed, targetSpeed, accel * delta);
            }

            // Поворот тела
            if (_currentSpeed > 0.1f && _moveDirection.LengthSquared() > 0.001f)
            {
                RotateBodyTowards(delta, _moveDirection);
            }
            else
            {
                RotateBodyTowards(delta, cameraForward);
            }

            // Применение скорости
            Vector3 velocity = _moveDirection * _currentSpeed;
            if (!isGrounded)
            {
                velocity.Y = _characterBody.Velocity.Y + Gravity * delta;
            }
            else
            {
                velocity.Y = _characterBody.Velocity.Y;
            }
            _characterBody.Velocity = velocity;
        }

        private void RotateBodyTowards(float delta, Vector3 targetDirection)
        {
            if (targetDirection.LengthSquared() < 0.001f)
                return;

            float currentAngle = _bodyMesh.Rotation.Y;
            float targetAngle = Mathf.Atan2(targetDirection.X, targetDirection.Z);
            float angleDiff = Mathf.LerpAngle(currentAngle, targetAngle, 10.0f * delta);
            float maxTurn = TurnSpeed * delta * Mathf.DegToRad(1.0f);
            float turn = Mathf.Clamp(angleDiff, -maxTurn, maxTurn);
            _bodyMesh.RotateY(turn);
        }
    }
}