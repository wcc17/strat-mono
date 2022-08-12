using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace StratMono.States.BattleState
{
    class InitialState : BaseState
    {
        private enum BattleStartState
        {
            Zoom, // make sure the camera zooms and is centered first
            HideNonBattleCharacters, // hide all the characters on the map that aren't in the battle
            PlaceCharacters,
        }

        private readonly float _zoomSpeed = 0.6f;
        private float opacity = 1f;
        private BattleStartState _battleStartState = BattleStartState.Zoom;

        public override void EnterState(LevelScene scene)
        {
            //TODO: this doesn't work properly yet
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
            } else
            {
                var modifier = (positionYDiff > 0) ? 
                    -spriteAnimatorBeingAttacked.Height / 2 : spriteAnimatorBeingAttacked.Height / 2;

                CenterCameraOnPosition(
                    scene,
                    new Vector2(scene.SelectedCharacter.Position.X, scene.SelectedCharacter.Position.Y + modifier));
            }
        }

        public override void ExitState(LevelScene scene)
        {
            // TODO: things to reset:
            // the zoom of the entire map needs to reset to where the user had it before the battle
            // every character entity opacity is changed 
            // the new sprites created need to be removed
            // the screen space renderer needs to be zoomed back out
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            (scene.Camera as BoundedMovingCamera).Update();

            switch (_battleStartState)
            {
                case BattleStartState.Zoom:
                    updateZoom(scene);
                    break;
                case BattleStartState.HideNonBattleCharacters:
                    hideMapCharacters(scene);
                    break;
                case BattleStartState.PlaceCharacters:
                    placeCharacters(scene);
                    break;
            }

            return this;
        }

        private void updateZoom(LevelScene scene)
        {
            scene.Camera.ZoomIn(_zoomSpeed * Time.DeltaTime);

            if (scene.Camera.RawZoom == scene.Camera.MaximumZoom)
            {
                _battleStartState = BattleStartState.HideNonBattleCharacters;
            }
        }

        private void hideMapCharacters(LevelScene scene)
        {
            opacity -= Time.DeltaTime;

            List<GridEntity> entities = scene.EntitiesOfType<GridEntity>();
            foreach(GridEntity entity in entities)
            {
                //if (entity.Name == scene.SelectedCharacter.Name || entity.Name == scene.CharacterBeingAttacked.Name)
                //{
                //    continue; 
                //}

                var animator = entity.GetComponent<SpriteAnimator>();
                animator.Color = Color.White * (opacity);
            }

            if (opacity <= 0f)
            {
                opacity = 0f;
                _battleStartState = BattleStartState.PlaceCharacters;
            }
        }

        private void placeCharacters(LevelScene scene)
        {
            //Entity battlePlayerCharacter = new Entity();
            //Entity battleCpuCharacter = new Entity();
           
            //SpriteAnimator battlePlayerCharacterAnimator = scene.CreateSpriteAnimator(scene.SelectedCharacter.SpriteName);
            //battlePlayerCharacterAnimator.RenderLayer = (int)RenderLayer.Battle;
            //SpriteAnimator battleCpuCharacterAnimator = scene.CreateSpriteAnimator(scene.CharacterBeingAttacked.SpriteName);
            //battleCpuCharacterAnimator.RenderLayer = (int)RenderLayer.Battle;

            //battlePlayerCharacter.AddComponent(battlePlayerCharacterAnimator);
            //battleCpuCharacter.AddComponent(battleCpuCharacterAnimator);

            //var renderer = scene.GetRenderer<ScreenSpaceRenderer>();
            //renderer.Camera.RawZoom = renderer.Camera.MaximumZoom;

            //battlePlayerCharacter.Position
            //    = new Vector2(
            //        renderer.Camera.Position.X + 100,
            //        (renderer.Camera.Bounds.Height / 2) - (battlePlayerCharacterAnimator.Height / 2));
            //battleCpuCharacter.Position
            //    = new Vector2(
            //        renderer.Camera.Position.X - 100 - (battleCpuCharacterAnimator.Width / 2),
            //        (renderer.Camera.Bounds.Height / 2) - (battleCpuCharacterAnimator.Height / 2));

            ////battlePlayerCharacter.Transform.Scale = new Vector2(5, 5);
            ////battleCpuCharacter.Transform.Scale = new Vector2(5, 5);

            //scene.AddEntity(battlePlayerCharacter);
            //scene.AddEntity(battleCpuCharacter);
        }
    }
}
