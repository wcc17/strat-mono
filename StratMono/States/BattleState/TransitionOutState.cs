using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using System.Collections.Generic;

namespace StratMono.States.BattleState
{
    class TransitionOutState : BaseBattleState
    {
        private enum BattleEndState : int
        {
            RemoveCharacters,
            Zoom,
            ReadyToExit
        }

        private readonly float _zoomSpeed = 3.4f;
        private float _screenFadeOpacity = 0.5f;
        private float _characterFadeOpacity = 0f;
        private BattleEndState _battleEndState = BattleEndState.RemoveCharacters;
        private CharacterGridEntity _attackingCharacter;
        private CharacterGridEntity _characterBeingAttacked;
        private BaseState _stateToReturnTo;
        private bool _isCharacterBeingAttackedDead;

        public TransitionOutState(
            CharacterGridEntity attackingCharacter, 
            CharacterGridEntity characterBeingAttacked,
            BaseState stateToReturnTo,
            bool isCharacterBeingAttackedDead)
        {
            _attackingCharacter = attackingCharacter;
            _characterBeingAttacked = characterBeingAttacked;
            _stateToReturnTo = stateToReturnTo;
            _isCharacterBeingAttackedDead = isCharacterBeingAttackedDead;
        }

        public override void EnterState(LevelScene scene)
        {
        }

        public override void ExitState(LevelScene scene)
        {
            // TODO: can we take the camera zoom back to where the user had it rather than the default zoom?
            teardownScreenOverlay(scene);

            // Tried to avoid directly referring to the scene properties, but unavoidable here for now
            scene.CharacterBeingAttacked = null;
            scene.SelectedCharacter = null;
            scene.SelectedTile = null;
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            switch (_battleEndState)
            {
                case BattleEndState.RemoveCharacters:
                    removeCharacters(scene);
                    break;
                case BattleEndState.Zoom:
                    updateZoom(scene);
                    break;
                case BattleEndState.ReadyToExit:
                    if (_isCharacterBeingAttackedDead)
                    {
                        return new FieldCleanupState(_characterBeingAttacked, _stateToReturnTo);
                    }
                    return _stateToReturnTo;
            }

            return this;
        }

        private void teardownScreenOverlay(LevelScene scene)
        {
            var screenOverlayEntity = scene.FindEntity(ScreenOverlayEntityName);
            screenOverlayEntity.RemoveAllComponents();
            screenOverlayEntity.DetachFromScene();
            screenOverlayEntity.Destroy();
        }

        private void removeCharacters(LevelScene scene)
        {
            Entity battlePlayerCharacter = scene.FindEntity(BattlePlayerEntityName);
            battlePlayerCharacter.RemoveAllComponents();
            battlePlayerCharacter.DetachFromScene();
            battlePlayerCharacter.Destroy();

            Entity battleNpcCharacter = scene.FindEntity(BattleNpcEntityName);
            battleNpcCharacter.RemoveAllComponents();
            battleNpcCharacter.DetachFromScene();
            battleNpcCharacter.Destroy();

            _battleEndState = BattleEndState.Zoom;
        }

        private void updateZoom(LevelScene scene)
        {
            centerCamera(scene); // Just keep setting moveGoal for camera so that the bounds correction doesnt mess it up
            (scene.Camera as BoundedMovingCamera).Update();

            handleZoom(scene);
            handleHalfFade(scene);
            showMapCharacters(scene);

            if (scene.Camera.RawZoom == 1
                && _screenFadeOpacity == 0f
                && _characterFadeOpacity == 1f)
            {
                _battleEndState = BattleEndState.ReadyToExit;
            }
        }

        private void centerCamera(LevelScene scene)
        {
            var positionXDiff = _attackingCharacter.Position.X - _characterBeingAttacked.Position.X;
            var positionYDiff = _attackingCharacter.Position.Y - _characterBeingAttacked.Position.Y;

            var spriteAnimatorBeingAttacked = _characterBeingAttacked.GetComponent<SpriteAnimator>();
            if (positionXDiff > 0)
            {
                var modifier = (positionXDiff > 0) ?
                    -spriteAnimatorBeingAttacked.Width / 2 : spriteAnimatorBeingAttacked.Width / 2;

                CenterCameraOnPosition(
                    scene,
                    new Vector2(_attackingCharacter.Position.X + modifier, _attackingCharacter.Position.Y));
            }
            else
            {
                var modifier = (positionYDiff > 0) ?
                    -spriteAnimatorBeingAttacked.Height / 2 : spriteAnimatorBeingAttacked.Height / 2;

                CenterCameraOnPosition(
                    scene,
                    new Vector2(_attackingCharacter.Position.X, _attackingCharacter.Position.Y + modifier));
            }

        }

        private void handleZoom(LevelScene scene)
        {
            if (scene.Camera.RawZoom > 1)
            {
                scene.Camera.ZoomOut(_zoomSpeed * Time.DeltaTime);
                return;
            }

            scene.Camera.RawZoom = 1;
        }

        private void handleHalfFade(LevelScene scene)
        {
            _screenFadeOpacity = (_screenFadeOpacity > 0f) ? _screenFadeOpacity -= Time.DeltaTime : 0.0f;

            var overlayEntity = scene.FindEntity(ScreenOverlayEntityName);
            var animator = overlayEntity.GetComponent<SpriteRenderer>();
            animator.Color = Color.Black * _screenFadeOpacity;
        }

        private void showMapCharacters(LevelScene scene)
        {
            _characterFadeOpacity += Time.DeltaTime;

            List<GridEntity> entities = scene.EntitiesOfType<GridEntity>();
            foreach (GridEntity entity in entities)
            {
                var animator = entity.GetComponent<SpriteAnimator>();
                animator.Color = Color.White * (_characterFadeOpacity);
            }

            if (_characterFadeOpacity >= 1f)
            {
                _characterFadeOpacity = 1f;
            }
        }
    }
}
