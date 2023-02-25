using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.States.BattleState.Context;
using StratMono.Util;

namespace StratMono.States.BattleState
{
    class FieldCleanupState : BaseBattleState
    {
        private readonly float _rotationSpeed = 600f;
        private readonly float _rotationLerp = 0.15f;

        private CharacterGridEntity _characterToCleanup;
        private BaseState _stateToReturnTo;
        private Entity _rotationEntity;

        private float _characterFadeOpacity = 1f;

        public FieldCleanupState(
            BattleContext battleContext,
            CharacterGridEntity characterToCleanup, 
            BaseState stateToReturnTo) : base(battleContext)
        {
            ShouldShowBattleStats = false;

            _characterToCleanup = characterToCleanup;
            _stateToReturnTo = stateToReturnTo;

            _rotationEntity = RotationEntityUtil.CreateRotationEntity(_characterToCleanup);
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

            bool isFadeDone = handleFade();
            bool isRotateDone = handleRotate();
            
            if (isFadeDone && isRotateDone)
            {
                RotationEntityUtil.ResetRotationEntity(_characterToCleanup);

                _characterToCleanup.RemoveAllComponents();
                _characterToCleanup.DetachFromScene();
                _characterToCleanup.Destroy();
                scene.RemoveFromGrid(_characterToCleanup);

                return _stateToReturnTo;
            }

            return this;
        }

        private bool handleFade()
        {
            if (_characterFadeOpacity > 0f)
            {
                _characterFadeOpacity -= Time.DeltaTime;
                var renderer = _characterToCleanup.GetComponent<SpriteRenderer>();
                renderer.Color = renderer.Color * _characterFadeOpacity;
                return false;
            }

            _characterFadeOpacity = 0.0f;
            return true;
        }

        private bool handleRotate()
        {
            if (_rotationEntity.RotationDegrees < 90)
            {
                var distanceToRotate = (Time.DeltaTime * _rotationSpeed);
                var newRotation = _rotationEntity.RotationDegrees + distanceToRotate;

                _rotationEntity.RotationDegrees = MathHelper.Lerp(
                    _rotationEntity.RotationDegrees,
                    newRotation,
                    _rotationLerp);

                return false;
            }

            _rotationEntity.RotationDegrees = 90;
            return true;
        }
    }
}
