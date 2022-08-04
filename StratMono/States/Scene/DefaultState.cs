using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;

namespace StratMono.States.Scene
{
    public class DefaultState : BaseState
    {
        public override void EnterState(LevelScene scene)
        {
            scene.RemoveHighlightsFromGrid();
        }

        public override BaseState Update(LevelScene scene, Vector2 cursorEntityPosition)
        {
            BaseState nextState = this;

            scene.GridSystem.Update(scene.EntitiesOfType<GridEntity>());

            if (DidUserMakeSelection())
            {
                // default state doesn't care if selected tile or character changed
                GridTile selectedTile = scene.GridSystem.GetNearestTileAtPosition(cursorEntityPosition);
                CharacterGridEntity selectedCharacter = GetCharacterFromSelectedTile(selectedTile);

                UpdateSceneSelections(scene, selectedTile, selectedCharacter);
                CenterCameraOnPosition(scene, selectedTile.Position);

                if (selectedCharacter != null)
                {
                    nextState = new CharacterSelectedState();
                    nextState.EnterState(scene);
                }
            }
            return nextState;
        }
    }
}
