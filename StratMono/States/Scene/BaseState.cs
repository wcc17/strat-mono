using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using System;
using System.Collections.Generic;

namespace StratMono.States.Scene
{
    public abstract class BaseState
    {
        public abstract BaseState Update(
            LevelScene scene,
            GridSystem gridSystem);

        public abstract void EnterState(
            LevelScene scene,
            GridSystem gridSystem);

        protected virtual bool CheckForNewSelection(
            LevelScene scene,
            GridSystem gridSystem)
        {
            List<GridEntity> entitiesOnSelectedTile = gridSystem.GetEntitiesFromSelectedTile();
            CharacterGridEntity newSelectedCharacter = null;
            if (entitiesOnSelectedTile != null)
            {
                foreach (var entity in entitiesOnSelectedTile)
                {
                    if (entity.GetType() == typeof(CharacterGridEntity))
                    {
                        newSelectedCharacter = (CharacterGridEntity)entity;
                    }
                }
            }

            if (newSelectedCharacter != scene.SelectedCharacter)
            {
                scene.SelectedCharacter = newSelectedCharacter;
                return true;
            }

            return false;
        }
    }
}
