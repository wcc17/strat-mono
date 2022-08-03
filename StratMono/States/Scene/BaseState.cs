using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using System;
using System.Collections.Generic;

namespace StratMono.States.Scene
{
    public abstract class BaseState
    {
        public virtual BaseState CheckForSelectedCharacter(
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
                    break;
                }
            }

            if (scene.SelectedCharacter == newSelectedCharacter)
            {
                return this;
            }
            else
            {
                scene.SelectedCharacter = newSelectedCharacter;
                return new SelectionChangeState();
            }
        }

        public abstract BaseState HandleSelectedCharacter(
            LevelScene scene, 
            GridSystem gridSystem);
    }
}
