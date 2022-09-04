using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.States;
using StratMono.States.BattleState;
using StratMono.Util;
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

        private readonly int MoveSpeed = 9000;
        private readonly float MoveLerp = 0.15f;
       
        private readonly Entity _characterAttacking;
        private readonly Entity _characterBeingAttacked;
        private readonly bool _attackerOnLeft;
        private int _originalX;
        private int _moveGoalX;
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

            var characterAttackingAnimator = _characterAttacking.GetComponent<SpriteAnimator>();
            var characterWidth = characterAttackingAnimator.Width;

            _originalX = (int)_characterAttacking.Position.X;
            if (attackerOnLeft)
            {
                _moveGoalX = (int)_characterBeingAttacked.Position.X - (int)characterWidth - 50;
            } else
            {
                _moveGoalX = (int)_characterBeingAttacked.Position.X + (int)characterWidth + 50;
            }
        }

        public override void EnterState(LevelScene scene)
        {
            var menuEntity = scene.FindEntity(ActionMenuEntityName);
            menuEntity.Enabled = false;
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
            handleMove(false);

            bool attackerOnLeftMoveForwardComplete = _attackerOnLeft && _characterAttacking.Position.X >= _moveGoalX;
            bool attackerOnRightMoveForwardComplete = !_attackerOnLeft && _characterAttacking.Position.X <= _moveGoalX;
            if (attackerOnLeftMoveForwardComplete || attackerOnRightMoveForwardComplete)
            {
                _characterAttacking.Position = new Vector2(_moveGoalX, _characterAttacking.Position.Y);
                _currentAttackState = AttackState.AttackerMoveBack;
            }
        }

        private void handleMoveBack()
        {
            handleMove(true);

            bool attackerOnLeftMoveBackComplete = _attackerOnLeft && _characterAttacking.Position.X <= _originalX;
            bool attackerOnRightMoveBackComplete = !_attackerOnLeft && _characterAttacking.Position.X >= _originalX;
            if (attackerOnLeftMoveBackComplete || attackerOnRightMoveBackComplete)
            {
                _characterAttacking.Position = new Vector2(_originalX, _characterAttacking.Position.Y);
                _currentAttackState += 1; //TODO: temporary
            }
        }

        private void handleMove(bool isMovingBack)
        {
            var distanceToMove = (Time.DeltaTime * MoveSpeed);
            var distanceDelta = new Vector2(distanceToMove, 0);

            Vector2 newPosition;
            if ((_attackerOnLeft && isMovingBack) || (!_attackerOnLeft && !isMovingBack))
            {
                newPosition = _characterAttacking.Position - distanceDelta;
            } else
            {
                newPosition = _characterAttacking.Position + distanceDelta;
            }

            _characterAttacking.Position
                = Vector2.Lerp(_characterAttacking.Position, newPosition, MoveLerp);
        }
    }
}
