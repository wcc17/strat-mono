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
       
        private readonly Entity _characterAttacking;
        private readonly Entity _characterBeingAttacked;
        private readonly bool _attackerOnLeft;
        private int _originalX;
        private int _moveGoalX;
        private AttackState _currentAttackState = AttackState.AttackerMoveForward;
        private readonly bool _lastAttack;

        public CharacterAttackState(
            LevelScene scene, 
            string entityNameAttacking, 
            string entityNameBeingAttacked,
            bool attackerOnLeft,
            bool lastAttack = true)
        {
            _characterAttacking = scene.FindEntity(entityNameAttacking);
            _characterBeingAttacked = scene.FindEntity(entityNameBeingAttacked);
            _attackerOnLeft = attackerOnLeft;
            _lastAttack = lastAttack;

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
                case AttackState.AttackerDone:
                    if (_lastAttack)
                    {
                        // TODO: return an exit state
                        return new TransitionOutState();
                    } else
                    {
                       return new CharacterAttackState(
                           scene,
                           entityNameAttacking: _characterBeingAttacked.Name,
                           entityNameBeingAttacked: _characterAttacking.Name,
                           attackerOnLeft: false,
                           lastAttack: true);
                    }
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
