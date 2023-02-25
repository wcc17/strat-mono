using Components.Character;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using States.Shared;
using StratMono.Components.Character;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.States;
using StratMono.States.BattleState;
using StratMono.States.BattleState.Context;
using StratMono.Util;
using System;

namespace stratMono.States.BattleState
{
    class CharacterAttackState : BaseBattleState
    {
        private enum AttackState : int
        {
            AttackerMoveForward,
            AttackerMoveBack,
            AttackAnimationDone,
            AttackedHealthLoss,
            AttackedDeathAnimation,
            AttackDone
        }

        private readonly int MoveSpeed = 9000;
        private readonly int RotationSpeed = 700;
        private readonly float MoveLerp = 0.15f;
       
        private readonly Entity _battleEntityCharacterAttacking; // These are the larger battle entities, not the actual entity that we got from the map
        private readonly Entity _battleEntityCharacterBeingAttacked;
        private Vector2 originalAttackingCharacterPosition;
        private Vector2 originalAttackedCharacterPosition;
        private int _forwardAttackMoveGoalX;
        private Vector2 _deathFallMoveGoal;
        private AttackState _currentAttackState = AttackState.AttackerMoveForward;
        private bool _lastAttack;
        private bool _isCharacterBeingAttackedDead;
        private BaseState _stateToReturnToAfterBattle;

        private int _healthLossGoal;

        private Entity _deathAnimationRotationEntity;

        public CharacterAttackState(
            LevelScene scene, 
            string entityNameAttacking, 
            string entityNameBeingAttacked,
            BattleContext battleContext,
            BaseState stateToReturnToAfterBattle,
            bool lastAttack = true) : base(battleContext)
        {
            _lastAttack = lastAttack;
            _stateToReturnToAfterBattle = stateToReturnToAfterBattle;

            _battleEntityCharacterAttacking = scene.FindEntity(entityNameAttacking);
            _battleEntityCharacterBeingAttacked = scene.FindEntity(entityNameBeingAttacked);

            var characterAttackingAnimator = _battleEntityCharacterBeingAttacked.GetComponent<SpriteAnimator>();
            var characterWidth = characterAttackingAnimator.Width;

            originalAttackingCharacterPosition = new Vector2(
                _battleEntityCharacterAttacking.Position.X,
                _battleEntityCharacterAttacking.Position.Y);
            originalAttackedCharacterPosition = new Vector2(
                _battleEntityCharacterBeingAttacked.Position.X,
                _battleEntityCharacterBeingAttacked.Position.Y);

            _deathFallMoveGoal = new Vector2(
                originalAttackedCharacterPosition.X + 100f,
                originalAttackedCharacterPosition.Y + 200f);

            if (CurrentBattleContext.AttackerOnLeft)
            {
                _forwardAttackMoveGoalX = (int)_battleEntityCharacterBeingAttacked.Position.X - (int)characterWidth - 50;
            } else
            {
                _forwardAttackMoveGoalX = (int)_battleEntityCharacterBeingAttacked.Position.X + (int)characterWidth + 50;
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
            base.Update(scene, cursorEntity);

            switch (_currentAttackState)
            {
                case AttackState.AttackerMoveForward:
                    handleMoveForward();
                    break;
                case AttackState.AttackerMoveBack:
                    handleMoveBack();
                    break;
                case AttackState.AttackAnimationDone:
                    handleAttackAnimationDone(scene);
                    break;
                case AttackState.AttackedHealthLoss:
                    handleAttackedHealthLoss(scene);
                    break;
                case AttackState.AttackedDeathAnimation:
                    handleAttackedDeathAnimation(scene);
                    break;
                case AttackState.AttackDone:
                    return handleAttackDone(scene);

            }

            return this;
        }

        private void handleMoveForward()
        {
            handleMoveForwardOrBack(false);

            bool attackerOnLeftMoveForwardComplete = CurrentBattleContext.AttackerOnLeft && _battleEntityCharacterAttacking.Position.X >= _forwardAttackMoveGoalX;
            bool attackerOnRightMoveForwardComplete = !CurrentBattleContext.AttackerOnLeft && _battleEntityCharacterAttacking.Position.X <= _forwardAttackMoveGoalX;
            if (attackerOnLeftMoveForwardComplete || attackerOnRightMoveForwardComplete)
            {
                _battleEntityCharacterAttacking.Position = new Vector2(_forwardAttackMoveGoalX, _battleEntityCharacterAttacking.Position.Y);
                _currentAttackState = AttackState.AttackerMoveBack;
            }
        }

        private void handleMoveBack()
        {
            handleMoveForwardOrBack(true);

            bool attackerOnLeftMoveBackComplete = CurrentBattleContext.AttackerOnLeft && _battleEntityCharacterAttacking.Position.X <= originalAttackingCharacterPosition.X;
            bool attackerOnRightMoveBackComplete = !CurrentBattleContext.AttackerOnLeft && _battleEntityCharacterAttacking.Position.X >= originalAttackingCharacterPosition.X;
            if (attackerOnLeftMoveBackComplete || attackerOnRightMoveBackComplete)
            {
                _battleEntityCharacterAttacking.Position = new Vector2(originalAttackingCharacterPosition.X, _battleEntityCharacterAttacking.Position.Y);
                _currentAttackState = AttackState.AttackAnimationDone;
            }
        }

        private void handleAttackAnimationDone(LevelScene scene)
        {
            var attackedCharacterHealth = CurrentBattleContext.CharacterGridEntityBeingAttacked.GetComponent<Health>();
            _healthLossGoal = (int)attackedCharacterHealth.currentHealth - 50; // TODO: hardcoded damage amount
            _currentAttackState = AttackState.AttackedHealthLoss;
        }

        private void handleAttackedHealthLoss(LevelScene scene)
        {
            var attackedCharacterHealth = CurrentBattleContext.CharacterGridEntityBeingAttacked.GetComponent<Health>();

            var healthToRemove = -(Time.DeltaTime * 50);
            attackedCharacterHealth.changeHealth(healthToRemove);

            if (attackedCharacterHealth.currentHealth <= _healthLossGoal)
            {
                attackedCharacterHealth.currentHealth = _healthLossGoal;

                if (attackedCharacterHealth.currentHealth < 1)
                {
                    _currentAttackState = AttackState.AttackedDeathAnimation;
                    this.prepareDeathAnimation(scene);
                }
                else
                {
                    _currentAttackState = AttackState.AttackDone;
                }
            }

        }

        private void prepareDeathAnimation(LevelScene scene)
        {
            var spriteAnimator = _battleEntityCharacterBeingAttacked.GetComponent<SpriteAnimator>();
            spriteAnimator.Stop();
            _deathAnimationRotationEntity = RotationEntityUtil.CreateRotationEntity(_battleEntityCharacterBeingAttacked);
        }

        private void handleAttackedDeathAnimation(LevelScene scene)
        {
            var distanceToRotate = (Time.DeltaTime * RotationSpeed);
            var newRotation = _deathAnimationRotationEntity.RotationDegrees + distanceToRotate;
            _deathAnimationRotationEntity.RotationDegrees = MathHelper.Lerp(
                _deathAnimationRotationEntity.RotationDegrees, 
                newRotation, 
                MoveLerp);

            var distanceToMove = (Time.DeltaTime * RotationSpeed);
            var distanceDelta = new Vector2(distanceToMove, distanceToMove);
            var newPosition = _deathAnimationRotationEntity.Position + distanceDelta;
            _deathAnimationRotationEntity.Position
                = Vector2.Lerp(_deathAnimationRotationEntity.Position, newPosition, MoveLerp);

            bool doneRotating = _deathAnimationRotationEntity.RotationDegrees >= 90;
            bool doneMovingRight = _deathAnimationRotationEntity.Position.X >= (_deathFallMoveGoal.X);
            bool doneMovingDown = _deathAnimationRotationEntity.Position.Y >= (_deathFallMoveGoal.Y);

            if (doneRotating)
            {
                _deathAnimationRotationEntity.RotationDegrees = 90;
            }
            if (doneMovingDown)
            {
                _deathAnimationRotationEntity.Position = new Vector2(_deathAnimationRotationEntity.Position.X, _deathFallMoveGoal.Y);
            }
            if (doneMovingRight)
            {
                _deathAnimationRotationEntity.Position = new Vector2(_deathFallMoveGoal.X, _deathAnimationRotationEntity.Position.Y);
            }

            bool deathAnimationFinished = doneRotating && doneMovingDown && doneMovingRight;
            if (deathAnimationFinished) 
            {
                RotationEntityUtil.ResetRotationEntity(_battleEntityCharacterBeingAttacked);

                _isCharacterBeingAttackedDead = true;
                _currentAttackState = AttackState.AttackDone;
                _lastAttack = true;
                return;
            }
        }

        private BaseState handleAttackDone(LevelScene scene)
        {

            if (_lastAttack)
            {
                return new TransitionOutState(
                    CurrentBattleContext,
                    _stateToReturnToAfterBattle,
                    _isCharacterBeingAttackedDead);
            }
            else
            {
                var nextState = new CharacterAttackState(
                   scene,
                   entityNameAttacking: _battleEntityCharacterBeingAttacked.Name,
                   entityNameBeingAttacked: _battleEntityCharacterAttacking.Name,
                   new BattleContext(CurrentBattleContext.CharacterGridEntityBeingAttacked, CurrentBattleContext.CharacterGridEntityAttacking, false),
                   stateToReturnToAfterBattle: _stateToReturnToAfterBattle,
                   lastAttack: true
                );
                return new DelayState(nextState, 1.0f);
            }
        }

        private void handleMoveForwardOrBack(bool isMovingBack)
        {
            var distanceToMove = (Time.DeltaTime * MoveSpeed);
            var distanceDelta = new Vector2(distanceToMove, 0);

            Vector2 newPosition;
            if ((CurrentBattleContext.AttackerOnLeft && isMovingBack) || (!CurrentBattleContext.AttackerOnLeft && !isMovingBack))
            {
                newPosition = _battleEntityCharacterAttacking.Position - distanceDelta;
            }
            else
            {
                newPosition = _battleEntityCharacterAttacking.Position + distanceDelta;
            }

            _battleEntityCharacterAttacking.Position
                = Vector2.Lerp(_battleEntityCharacterAttacking.Position, newPosition, MoveLerp);
        }
    }
}
