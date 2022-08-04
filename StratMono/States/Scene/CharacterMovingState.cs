using Microsoft.Xna.Framework;
using StratMono.Components;
using StratMono.Scenes;

namespace StratMono.States.Scene
{
    public class CharacterMovingState : BaseState
    {
        public override void EnterState(LevelScene scene) { }

        public override BaseState Update(LevelScene scene, Vector2 cursorEntityPosition)
        {
            BaseState nextState = this;

            var characterPosition = new Point((int)scene.SelectedCharacter.Position.X, (int)scene.SelectedCharacter.Position.Y);
            CenterCameraOnPosition(scene, characterPosition);

            // wait for character to finish moving, then return default state
            if (!scene.SelectedCharacter.GetComponent<GridEntityMoveToGoal>().Enabled)
            {
                scene.SelectedCharacter.RemoveComponent<GridEntityMoveToGoal>();
                scene.SelectedCharacter = null;
                scene.SelectedTile = null;
                nextState = new DefaultState();
                nextState.EnterState(scene);
            }

            return nextState;
        }
    }
}
