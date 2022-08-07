using Microsoft.Xna.Framework;
using Nez;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using System;
using System.Collections.Generic;

namespace StratMono.States.Scene
{
    public abstract class BaseState
    {
        public abstract void EnterState(LevelScene scene);

        public virtual BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            (scene.Camera as BoundedMovingCamera).Update();

            scene.SceneTileCursorSystem.Update(
                cursorEntity,
                scene.Camera);

            scene.GridSystem.Update(scene.EntitiesOfType<GridEntity>());

            return this;
        }

        protected virtual void UpdateSceneSelections(
            LevelScene scene, 
            GridTile selectedTile, 
            CharacterGridEntity selectedCharacter)
        {
            scene.SelectedTile = selectedTile;
            scene.SelectedCharacter = selectedCharacter;
        }

        protected virtual void CenterCameraOnPosition(LevelScene scene, Vector2 position)
        {
            var point = new Point((int)position.X, (int)position.Y);
            CenterCameraOnPosition(scene, point);
        }

        protected virtual void CenterCameraOnPosition(LevelScene scene, Point position)
        {
            // move the camera so that the selected tile is in the middle of the screen
            ((BoundedMovingCamera)scene.Camera).MoveGoal = new Vector2(
                position.X + (scene.GridSystem.GridTileWidth / 2),
                position.Y + (scene.GridSystem.GridTileHeight / 2));
        }

        protected virtual bool DidUserMakeSelection()
        {
            if (Input.LeftMouseButtonPressed
                || Input.GamePads[0].IsRightTriggerPressed()
                || Input.GamePads[0].IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                return true;
            }

            return false;
        }

        protected CharacterGridEntity GetCharacterFromSelectedTile(GridTile selectedTile)
        {
            List<GridEntity> entitiesOnSelectedTile = selectedTile.OccupyingEntities;
            if (entitiesOnSelectedTile != null)
            {
                foreach (var entity in entitiesOnSelectedTile)
                {
                    if (entity.GetType() == typeof(CharacterGridEntity))
                    {
                        return (CharacterGridEntity)entity;
                    }
                }
            }

            return null;
        }
    }
}
