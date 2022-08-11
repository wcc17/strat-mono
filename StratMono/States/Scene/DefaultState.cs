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
        public override void EnterState(LevelScene scene) { }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            base.Update(scene, cursorEntity);
            
            BaseState nextState = this;

            if (DidUserMakeSelection())
            {
                // default state doesn't care if selected tile or character changed
                GridTile selectedTile = scene.GridSystem.GetNearestTileAtPosition(cursorEntity.Position);
                CharacterGridEntity selectedCharacter = scene.GetCharacterFromSelectedTile(selectedTile);

                UpdateSceneSelections(scene, selectedTile, selectedCharacter);
                CenterCameraOnPosition(scene, selectedTile.Position);

                if (selectedCharacter != null && selectedCharacter.GetComponent<EnemyComponent>() != null)
                {
                    nextState = new EnemySelectedState();
                } else if (selectedCharacter != null)
                {
                    nextState = new CharacterSelectedState();
                }
            }
            return nextState;
        }

        public override void ExitState(LevelScene scene) { }
    }
}
