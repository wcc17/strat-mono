using System;
using StratMono.Scenes;
using StratMono.System;

namespace StratMono.States.Scene
{
    public class DefaultState : BaseState
    {
        public override BaseState Update(
            LevelScene scene,
            GridSystem gridSystem)
        {
            BaseState nextState = this;

            var selectionChanged = CheckForNewSelection(scene, gridSystem);
            if (selectionChanged && scene.SelectedCharacter != null)
            { 
                // NOTE: assuming that not null is a selected character until something else could be selected
                nextState = new CharacterSelectedState();
                nextState.EnterState(scene, gridSystem);
            }

            return nextState;
        }

        public override void EnterState(
            LevelScene scene, 
            GridSystem gridSystem) { }
    }
}
