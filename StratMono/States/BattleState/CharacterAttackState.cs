using Components.Character;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using StartMono.Util;
using States.Shared;
using StratMono.Components.Character;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.States;
using StratMono.States.BattleState;
using StratMono.States.FieldState;
using StratMono.System;
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
            AttackedDeathAnimation,
            AttackDone
        }

        private readonly int MoveSpeed = 9000;
        private readonly int RotationSpeed = 500;
        private readonly float MoveLerp = 0.15f;
       
        private readonly Entity _battleEntityCharacterAttacking; // These are the larger battle entities, not the actual entity that we got from the map
        private readonly Entity _battleEntityCharacterBeingAttacked;
        private readonly CharacterGridEntity _characterGridEntityAttacking; // These are the entities from the field
        private readonly CharacterGridEntity _characterGridEntityBeingAttacked;
        private readonly bool _attackerOnLeft;
        private Vector2 originalAttackingCharacterPosition;
        private Vector2 originalAttackedCharacterPosition;
        private int _forwardAttackMoveGoalX;
        private Vector2 _deathFallMoveGoal;
        private AttackState _currentAttackState = AttackState.AttackerMoveForward;
        private bool _lastAttack;
        private BaseState _stateToReturnToAfterBattle;
        private Entity _deathAnimationRotationEntity;

        public CharacterAttackState(
            LevelScene scene, 
            string entityNameAttacking, 
            string entityNameBeingAttacked,
            CharacterGridEntity characterGridEntityAttacking,
            CharacterGridEntity characterGridEntityBeingAttacked,
            bool attackerOnLeft,
            BaseState stateToReturnToAfterBattle,
            bool lastAttack = true)
        {
            _characterGridEntityAttacking = characterGridEntityAttacking;
            _characterGridEntityBeingAttacked = characterGridEntityBeingAttacked; 
            _attackerOnLeft = attackerOnLeft;
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

            if (attackerOnLeft)
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
                case AttackState.AttackedDeathAnimation:
                    handleAttackedDeathAnimation();
                    break;
                case AttackState.AttackDone:
                    return handleAttackDone(scene);

            }

            return this;
        }

        private void handleMoveForward()
        {
            handleMoveForwardOrBack(false);

            bool attackerOnLeftMoveForwardComplete = _attackerOnLeft && _battleEntityCharacterAttacking.Position.X >= _forwardAttackMoveGoalX;
            bool attackerOnRightMoveForwardComplete = !_attackerOnLeft && _battleEntityCharacterAttacking.Position.X <= _forwardAttackMoveGoalX;
            if (attackerOnLeftMoveForwardComplete || attackerOnRightMoveForwardComplete)
            {
                _battleEntityCharacterAttacking.Position = new Vector2(_forwardAttackMoveGoalX, _battleEntityCharacterAttacking.Position.Y);
                _currentAttackState = AttackState.AttackerMoveBack;
            }
        }

        private void handleMoveBack()
        {
            handleMoveForwardOrBack(true);

            bool attackerOnLeftMoveBackComplete = _attackerOnLeft && _battleEntityCharacterAttacking.Position.X <= originalAttackingCharacterPosition.X;
            bool attackerOnRightMoveBackComplete = !_attackerOnLeft && _battleEntityCharacterAttacking.Position.X >= originalAttackingCharacterPosition.X;
            if (attackerOnLeftMoveBackComplete || attackerOnRightMoveBackComplete)
            {
                _battleEntityCharacterAttacking.Position = new Vector2(originalAttackingCharacterPosition.X, _battleEntityCharacterAttacking.Position.Y);
                _currentAttackState = AttackState.AttackAnimationDone;
            }
        }

        private void handleAttackAnimationDone(LevelScene scene)
        {
            var attackedCharacterHealth = _characterGridEntityBeingAttacked.GetComponent<Health>();
            attackedCharacterHealth.changeHealth(-1000000);

            if (attackedCharacterHealth.currentHealth < 0)
            {
                _currentAttackState = AttackState.AttackedDeathAnimation;
                this.prepareDeathAnimation(scene);
            } else
            {
                _currentAttackState = AttackState.AttackDone;
            }
        }

        private void prepareDeathAnimation(LevelScene scene)
        {
            // NOTE: can use this to debug the relationship between the parent rotationEntity and the child attacked entity
            //SpriteRenderer outline = PrimitiveShapeUtil.CreateRectangleOutlineSprite(
            //    64, 64, Color.Red, 3);
            //outline.RenderLayer = (int)RenderLayer.UI;
            //rotationEntity.AddComponent(outline);

            var spriteAnimator = _battleEntityCharacterBeingAttacked.GetComponent<SpriteAnimator>();
            float offset = (spriteAnimator.Width / spriteAnimator.Animations.Count) / 2;
            float scale = _battleEntityCharacterBeingAttacked.Scale.X;

            _deathAnimationRotationEntity = new Entity();

            // Set the parent entity's position so that when offsetting the child entity, it appears to not change its position
            // The offset is half of the child entity's width, which is the center of the child entity
            _deathAnimationRotationEntity.Position = new Vector2(
                _battleEntityCharacterBeingAttacked.Position.X + (offset * scale), 
                _battleEntityCharacterAttacking.Position.Y + (offset * scale));

            // Set the parent entity's scale to match the child and reset the child's scale so that it doesn't double
            _deathAnimationRotationEntity.Scale = new Vector2(scale);
            _battleEntityCharacterBeingAttacked.Scale = new Vector2(1f);

            scene.AddEntity(_deathAnimationRotationEntity);

            // Set the child entity's local position so that it is centered over the parent's origin (should be (0, 0) if not this next line won't work
            _battleEntityCharacterBeingAttacked.LocalPosition = new Vector2(-offset, -offset);

            // Set the child entity's transform parent to the rotationEntity so that it rotates with the parent
            _battleEntityCharacterBeingAttacked.Parent = _deathAnimationRotationEntity.Transform;

            _battleEntityCharacterBeingAttacked.GetComponent<SpriteAnimator>().Stop();
        }

        private void handleAttackedDeathAnimation()
        {

            var distanceToRotate = (Time.DeltaTime * RotationSpeed);
            var newRotation = _deathAnimationRotationEntity.RotationDegrees + distanceToRotate;
            _deathAnimationRotationEntity.RotationDegrees = MathHelper.Lerp(_deathAnimationRotationEntity.RotationDegrees, newRotation, MoveLerp);

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
                    _characterGridEntityAttacking,
                    _characterGridEntityBeingAttacked,
                    _stateToReturnToAfterBattle);
            }
            else
            {
                var nextState = new CharacterAttackState(
                   scene,
                   entityNameAttacking: _battleEntityCharacterBeingAttacked.Name,
                   entityNameBeingAttacked: _battleEntityCharacterAttacking.Name,
                   characterGridEntityAttacking: _characterGridEntityBeingAttacked,
                   characterGridEntityBeingAttacked: _characterGridEntityAttacking,
                   attackerOnLeft: false,
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
            if ((_attackerOnLeft && isMovingBack) || (!_attackerOnLeft && !isMovingBack))
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
