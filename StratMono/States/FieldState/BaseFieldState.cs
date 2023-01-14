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
    }
}
