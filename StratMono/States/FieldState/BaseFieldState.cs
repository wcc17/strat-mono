using Microsoft.Xna.Framework;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;

namespace StratMono.States.FieldState
{
    public abstract class BaseFieldState : BaseState
    {

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
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
    }
}
