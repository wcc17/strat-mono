using Microsoft.Xna.Framework;
using Nez;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.States;
using StratMono.States.BattleState;
using System;
using System.Collections.Generic;
using System.Text;

namespace stratMono.States.BattleState
{
    class CharacterAttackState : BaseBattleState
    {
        private enum AttackState : int
        {
            AttackerMoveForward,
            AttackerMoveBack,
        }

        private readonly int MoveGoal = 200;
        private readonly int MoveSpeed = 1200;
        private readonly float MoveLerp = 0.1f;
       
        private readonly Entity _characterAttacking;
        private readonly Entity _characterBeingAttacked;
        private readonly bool _attackerOnLeft;
        private float _distanceMoved = 0;
        private AttackState _currentAttackState = AttackState.AttackerMoveForward;

        public CharacterAttackState(
            LevelScene scene, 
            string entityNameAttacking, 
            string entityNameBeingAttacked,
            bool attackerOnLeft)
        {
            _characterAttacking = scene.FindEntity(entityNameAttacking);
            _characterBeingAttacked = scene.FindEntity(entityNameBeingAttacked);
            _attackerOnLeft = attackerOnLeft;
        }

        public override void EnterState(LevelScene scene)
        {
        }

        public override void ExitState(LevelScene scene)
        {
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            switch (_currentAttackState)
            {
                case AttackState.AttackerMoveForward:
                    handleMoveForward();
                    break;
                case AttackState.AttackerMoveBack:
                    handleMoveBack();
                    break;
                default:
                    break;
            }

            return this;
        }

        private void handleMoveForward()
        {
            bool moveLeft = (_attackerOnLeft) ? false : true;
            handleMove(!_attackerOnLeft, () =>
            {
                _distanceMoved = 0;
                _currentAttackState = AttackState.AttackerMoveBack;
            });
        }

        private void handleMoveBack()
        {
            bool moveLeft = (_attackerOnLeft) ? true : false;
            handleMove(_attackerOnLeft, () =>
            {
                // TODO temporary
                _currentAttackState += 1;
            });
        }

        private void handleMove(bool moveLeft, Action onFinished)
        {
            var distanceToMove = (Time.DeltaTime * MoveSpeed);
            var distanceDelta = new Vector2(distanceToMove, 0);
            _distanceMoved = (_attackerOnLeft) ? _distanceMoved + distanceDelta.X : _distanceMoved - distanceDelta.X;

            var newPosition = (moveLeft) 
                ? _characterAttacking.Position - distanceDelta : _characterAttacking.Position + distanceDelta;

            _characterAttacking.Position
                = Vector2.Lerp(_characterAttacking.Position, newPosition, MoveLerp);

            if (Math.Abs(_distanceMoved) >= MoveGoal)
            {
                onFinished();
            }
        }
    }
}
