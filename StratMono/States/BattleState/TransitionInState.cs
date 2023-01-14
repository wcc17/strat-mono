using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using StartMono.Util;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace StratMono.States.BattleState
{
    class TransitionInState : BaseBattleState
    {
        private enum BattleStartState : int
        {
            Zoom, // make sure the camera zooms and is centered first
            PlaceCharacters,
            BattleReady
        }

        private readonly float _zoomSpeed = 1.4f;
        private float _screenFadeOpacity = 0f;
        private float _characterFadeOpacity = 1f;
        private BattleStartState _battleStartState = BattleStartState.Zoom;

        public override void EnterState(LevelScene scene)
        {
            setupScreenOverlay(scene);
        }

        public override void ExitState(LevelScene scene)
        {
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            switch (_battleStartState)
            {
                case BattleStartState.Zoom:
                    updateZoom(scene);
                    break;
                case BattleStartState.PlaceCharacters:
                    placeCharacters(scene);
                    break;
                case BattleStartState.BattleReady:
                    return new DefaultState();
            }

            return this;
        }

        private void setupScreenOverlay(LevelScene scene)
        {
            Entity screenOverlayEntity = new Entity(ScreenOverlayEntityName);
            SpriteRenderer screenOverlay = PrimitiveShapeUtil.CreateRectangleSprite(Screen.Width, Screen.Height, Color.Black);
            screenOverlay.RenderLayer = (int)RenderLayer.BattleLevelSeparator;
            screenOverlayEntity.AddComponent(screenOverlay);
            scene.AddEntity(screenOverlayEntity);
        }

        private void centerCamera(LevelScene scene)
        {
            var positionXDiff = scene.SelectedCharacter.Position.X - scene.CharacterBeingAttacked.Position.X;
            var positionYDiff = scene.SelectedCharacter.Position.Y - scene.CharacterBeingAttacked.Position.Y;

            var spriteAnimatorBeingAttacked = scene.CharacterBeingAttacked.GetComponent<SpriteAnimator>();
            if (positionXDiff > 0)
            {
                var modifier = (positionXDiff > 0) ?
                    -spriteAnimatorBeingAttacked.Width / 2 : spriteAnimatorBeingAttacked.Width / 2;

                CenterCameraOnPosition(
                    scene,
                    new Vector2(scene.SelectedCharacter.Position.X + modifier, scene.SelectedCharacter.Position.Y));
            }
            else
            {
                var modifier = (positionYDiff > 0) ?
                    -spriteAnimatorBeingAttacked.Height / 2 : spriteAnimatorBeingAttacked.Height / 2;

                CenterCameraOnPosition(
                    scene,
                    new Vector2(scene.SelectedCharacter.Position.X, scene.SelectedCharacter.Position.Y + modifier));
            }

        }

        private void updateZoom(LevelScene scene)
        {
            centerCamera(scene); // Just keep setting moveGoal for camera so that the bounds correction doesnt mess it up
            (scene.Camera as BoundedMovingCamera).Update();

            handleZoom(scene);
            handleHalfFade(scene);
            hideMapCharacters(scene);

            if (scene.Camera.RawZoom == scene.Camera.MaximumZoom 
                && _screenFadeOpacity == 0.5f
                && _characterFadeOpacity == 0f) 
            {
                _battleStartState = BattleStartState.PlaceCharacters;
            }
        }

        private void handleZoom(LevelScene scene)
        {
            scene.Camera.ZoomIn(_zoomSpeed * Time.DeltaTime);
        }

        private void handleHalfFade(LevelScene scene)
        {
            _screenFadeOpacity = (_screenFadeOpacity < 0.5f) ? _screenFadeOpacity += Time.DeltaTime : 0.5f;

            var overlayEntity = scene.FindEntity(ScreenOverlayEntityName);
            var animator = overlayEntity.GetComponent<SpriteRenderer>();
            animator.Color = Color.Black * _screenFadeOpacity;
        }

        private void hideMapCharacters(LevelScene scene)
        {
            _characterFadeOpacity -= Time.DeltaTime;

            List<GridEntity> entities = scene.EntitiesOfType<GridEntity>();
            foreach(GridEntity entity in entities)
            {
                var animator = entity.GetComponent<SpriteAnimator>();
                animator.Color = Color.White * (_characterFadeOpacity);
            }

            if (_characterFadeOpacity <= 0f)
            {
                _characterFadeOpacity = 0f;
            }
        }

        private void placeCharacters(LevelScene scene)
        {
            Entity battlePlayerCharacter = new Entity(BattlePlayerEntityName);
            SpriteAnimator battlePlayerCharacterAnimator = scene.CreateSpriteAnimator(scene.SelectedCharacter.SpriteName);
            battlePlayerCharacterAnimator.Play("walk_right", SpriteAnimator.LoopMode.PingPong);
            battlePlayerCharacterAnimator.RenderLayer = (int)RenderLayer.Battle;
            battlePlayerCharacter.AddComponent(battlePlayerCharacterAnimator);

            var screenSpaceRenderer = scene.GetRenderer<ScreenSpaceRenderer>();
            var screenSpaceCamera = screenSpaceRenderer.Camera;

            battlePlayerCharacter.SetScale(4);
            battlePlayerCharacter.Position = new Vector2(
                screenSpaceCamera.Bounds.X + (screenSpaceCamera.Bounds.Width / 7),
                screenSpaceCamera.Bounds.Y + (screenSpaceCamera.Bounds.Height / 2) - (battlePlayerCharacterAnimator.Height / 2));

            
            Entity battleNpcCharacter = new Entity(BattleNpcEntityName);
            SpriteAnimator battleNpcCharacterAnimator = scene.CreateSpriteAnimator(scene.CharacterBeingAttacked.SpriteName);
            battleNpcCharacterAnimator.Play("walk_left", SpriteAnimator.LoopMode.PingPong);
            battleNpcCharacterAnimator.RenderLayer = (int)RenderLayer.Battle;
            battleNpcCharacter.AddComponent(battleNpcCharacterAnimator);

            battleNpcCharacter.SetScale(4);
            battleNpcCharacter.Position = new Vector2(
                screenSpaceCamera.Bounds.Width - (screenSpaceCamera.Bounds.Width / 7) - battleNpcCharacterAnimator.Width,
                screenSpaceCamera.Bounds.Y + (screenSpaceCamera.Bounds.Height / 2) - (battleNpcCharacterAnimator.Height / 2));

            scene.AddEntity(battlePlayerCharacter);
            scene.AddEntity(battleNpcCharacter);

            _battleStartState = BattleStartState.BattleReady;
        }
    }
}
