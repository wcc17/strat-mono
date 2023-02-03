using Components.Character;
using Components.Character.Enemy;
using Microsoft.Xna.Framework;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using StratMono.UI;

namespace StratMono.States.FieldState
{
    public class PlayerControlDefaultState : BaseFieldState
    {
        public override void EnterState(LevelScene scene) 
        {
            var existingTurnIndicator = scene.FindEntity("turnindicator");
            if (existingTurnIndicator != null)
            {
                MenuBuilder.DestroyMenu(scene.FindEntity("turnindicator"));
            }

            var menuEntity = MenuBuilder.BuildStaticTextBox("turnindicator", "Player's turn", MenuBuilder.ScreenPosition.TopRight, Color.White, Color.Black);
            scene.AddEntity(menuEntity);
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            base.Update(scene, cursorEntity);

            if (scene.AllTeamFinishedTurn())
            {
                for (var i = 0; i < scene.teamEntities.Count; i++)
                {
                    scene.teamEntities[i].GetComponent<TurnState>().reset();
                }

                return new NpcControlDefaultState();
            }

            BaseFieldState nextState = this;
            if (DidUserMakeSelection())
            {
                // default state doesn't care if selected tile or character changed
                GridTile selectedTile = scene.GridSystem.GetNearestTileAtPosition(cursorEntity.Position);
                CharacterGridEntity selectedCharacter = scene.GetCharacterFromSelectedTile(selectedTile);

                UpdateSceneSelections(scene, selectedTile, selectedCharacter);
                CenterCameraOnPosition(scene, selectedTile.Position);

                if (selectedCharacter != null && selectedCharacter.GetComponent<EnemyComponent>() != null)
                {
                    nextState = new PlayerEnemySelectedState();
                } else if (selectedCharacter != null)
                {
                    nextState = new PlayerCharacterSelectedState();
                }
            }
            return nextState;
        }

        public override void ExitState(LevelScene scene) { }
    }
}
