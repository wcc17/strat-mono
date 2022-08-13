using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
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

        private readonly int MoveSpeed = 9000;
        private readonly float MoveLerp = 0.1f;
       
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
            bool shouldMoveLeft = _attackerOnLeft ? false : true;
            handleMove(shouldMoveLeft);

            if (_attackerOnLeft)
            {
                if (!shouldMoveLeft && _characterAttacking.Position.X >= _moveGoalX)
                {
                    _characterAttacking.Position = new Vector2(_moveGoalX, _characterAttacking.Position.Y);
                    _currentAttackState = AttackState.AttackerMoveBack;
                }
            } else
            {
                if (shouldMoveLeft && _characterAttacking.Position.X <= _moveGoalX)
                {
                    _characterAttacking.Position = new Vector2(_moveGoalX, _characterAttacking.Position.Y);
                    _currentAttackState = AttackState.AttackerMoveBack;
                }
            }
        }

        private void handleMoveBack()
        {
            bool shouldMoveLeft = _attackerOnLeft ? true : false;
            handleMove(shouldMoveLeft);

            if (_attackerOnLeft)
            {
                if (shouldMoveLeft && _characterAttacking.Position.X <= _originalX)
                {
                    _characterAttacking.Position = new Vector2(_originalX, _characterAttacking.Position.Y);
                    _currentAttackState += 1; //TODO: temporary
                }
            }
            else
            {
                if (!shouldMoveLeft && _characterAttacking.Position.X >= _originalX)
                {
                    _characterAttacking.Position = new Vector2(_originalX, _characterAttacking.Position.Y);
                    _currentAttackState += 1; //TODO: temporary
                }
            }
        }

        private void handleMove(bool moveLeft)
        {
            var distanceToMove = (Time.DeltaTime * MoveSpeed);
            var distanceDelta = new Vector2(distanceToMove, 0);

            var newPosition = (moveLeft) 
                ? _characterAttacking.Position - distanceDelta : _characterAttacking.Position + distanceDelta;

            _characterAttacking.Position
                = Vector2.Lerp(_characterAttacking.Position, newPosition, MoveLerp);
        }
    }
}
