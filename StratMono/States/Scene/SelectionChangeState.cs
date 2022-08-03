using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using System;
using System.Collections.Generic;

namespace StratMono.States.Scene
{
    public class SelectionChangeState : BaseState
    {
        public override BaseState CheckForSelectedCharacter(
            LevelScene scene, 
            GridSystem gridSystem)
        {
            return this;
        }

        public override BaseState HandleSelectedCharacter(
            LevelScene scene, 
            GridSystem gridSystem)
        {
            List<GridTileHighlight> highlights = scene.EntitiesOfType<GridTileHighlight>();
            foreach (GridTileHighlight highlight in highlights)
            {
                scene.RemoveFromGrid(highlight);
            }

            return new DefaultState();
        }
    }
}
