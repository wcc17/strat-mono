using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.States;
using StratMono.States.BattleState;

namespace stratMono.States.BattleState
{
    class CharacterAttackState : BaseBattleState
    {
        private enum AttackState : int
        {
            AttackerMoveForward,
            AttackerMoveBack,
            AttackerDone
        }

        private readonly int MoveSpeed = 9000;
        private readonly float MoveLerp = 0.15f;
       
        private readonly Entity _battleEntityCharacterAttacking; // These are the larger battle entities, not the actual entity that we got from the map
        private readonly Entity _battleEntityCharacterBeingAttacked;
        private readonly CharacterGridEntity _characterGridEntityAttacking; // These are the entities from the field
        private readonly CharacterGridEntity _characterGridEntityBeingAttacked;
        private readonly bool _attackerOnLeft;
        private int _originalX;
        private int _moveGoalX;
        private AttackState _currentAttackState = AttackState.AttackerMoveForward;
        private readonly bool _lastAttack;

        public CharacterAttackState(
            LevelScene scene, 
            string entityNameAttacking, 
            string entityNameBeingAttacked,
            CharacterGridEntity characterGridEntityAttacking,
            CharacterGridEntity characterGridEntityBeingAttacked,
            bool attackerOnLeft,
            bool lastAttack = true)
        {
            _characterGridEntityAttacking = characterGridEntityAttacking;
            _characterGridEntityBeingAttacked = characterGridEntityBeingAttacked; 
            _attackerOnLeft = attackerOnLeft;
            _lastAttack = lastAttack;

            _battleEntityCharacterAttacking = scene.FindEntity(entityNameAttacking);
            _battleEntityCharacterBeingAttacked = scene.FindEntity(entityNameBeingAttacked);

            var characterAttackingAnimator = _battleEntityCharacterBeingAttacked.GetComponent<SpriteAnimator>();
            var characterWidth = characterAttackingAnimator.Width;

            _originalX = (int)_battleEntityCharacterAttacking.Position.X;
            if (attackerOnLeft)
            {
                _moveGoalX = (int)_battleEntityCharacterBeingAttacked.Position.X - (int)characterWidth - 50;
            } else
            {
                _moveGoalX = (int)_battleEntityCharacterBeingAttacked.Position.X + (int)characterWidth + 50;
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
                case AttackState.AttackerDone:
                    if (_lastAttack)
                    {
                        return new TransitionOutState(_characterGridEntityAttacking, _characterGridEntityBeingAttacked);
                    } else
                    {
                       return new CharacterAttackState(
                           scene,
                           entityNameAttacking: _battleEntityCharacterBeingAttacked.Name,
                           entityNameBeingAttacked: _battleEntityCharacterAttacking.Name,
                           characterGridEntityAttacking: _characterGridEntityBeingAttacked,
                           characterGridEntityBeingAttacked: _characterGridEntityAttacking,
                           attackerOnLeft: false,
                           lastAttack: true);
                    }
            }

            return this;
        }

        private void handleMoveForward()
        {
            handleMove(false);

            bool attackerOnLeftMoveForwardComplete = _attackerOnLeft && _battleEntityCharacterAttacking.Position.X >= _moveGoalX;
            bool attackerOnRightMoveForwardComplete = !_attackerOnLeft && _battleEntityCharacterAttacking.Position.X <= _moveGoalX;
            if (attackerOnLeftMoveForwardComplete || attackerOnRightMoveForwardComplete)
            {
                _battleEntityCharacterAttacking.Position = new Vector2(_moveGoalX, _battleEntityCharacterAttacking.Position.Y);
                _currentAttackState = AttackState.AttackerMoveBack;
            }
        }

        private void handleMoveBack()
        {
            handleMove(true);

            bool attackerOnLeftMoveBackComplete = _attackerOnLeft && _battleEntityCharacterAttacking.Position.X <= _originalX;
            bool attackerOnRightMoveBackComplete = !_attackerOnLeft && _battleEntityCharacterAttacking.Position.X >= _originalX;
            if (attackerOnLeftMoveBackComplete || attackerOnRightMoveBackComplete)
            {
                _battleEntityCharacterAttacking.Position = new Vector2(_originalX, _battleEntityCharacterAttacking.Position.Y);
                _currentAttackState = AttackState.AttackerDone;
            }
        }

        private void handleMove(bool isMovingBack)
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
