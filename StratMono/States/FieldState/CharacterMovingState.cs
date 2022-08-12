using Microsoft.Xna.Framework;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using System.Collections.Generic;
using StratMono.Util;

namespace StratMono.States.FieldState
{
    public class CharacterMovingState : BaseFieldState
    {
        private readonly Stack<GridTile> _pathToTake;
        private readonly bool _returnedToOriginalPosition;

        public CharacterMovingState(
            Stack<GridTile> pathToTake,
            bool returnedToOriginalPosition = false) : base()
        {
            _pathToTake = pathToTake;
            _returnedToOriginalPosition = returnedToOriginalPosition;
        }

        public override void EnterState(LevelScene scene) 
        {
            // to create the initial new stack, it pops off all of the elements. So we have to do it twice to put it back in the right order
            var stackClone = new Stack<GridTile>(new Stack<GridTile>(_pathToTake));
            scene.SelectedCharacter.AddComponent(new GridEntityMoveToGoal(stackClone));
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            BaseFieldState nextState = this;

            CenterCameraOnPosition(scene, scene.SelectedCharacter.Position);
            (scene.Camera as BoundedMovingCamera).Update();

            if (!scene.SelectedCharacter.GetComponent<GridEntityMoveToGoal>().Enabled)
            {
                scene.SelectedCharacter.RemoveComponent<GridEntityMoveToGoal>();

                // This will be set to true if character moved, then the user cancelled the action
                if (_returnedToOriginalPosition)
                {
                    // if we're on controller, put the cursor back to where we started
                    if (InputMode.CurrentInputMode == InputModeType.Controller)
                    {
                        var originalTilePosition = new Stack<GridTile>(_pathToTake).Pop().Position;
                        cursorEntity.Position = new Vector2(originalTilePosition.X, originalTilePosition.Y);
                    }

                    scene.SelectedCharacter = null;
                    scene.SelectedTile = null;
                    nextState = new DefaultState();
                    return nextState;
                }

                nextState = new CharacterSelectActionState(new Stack<GridTile>(_pathToTake));
            }

            return nextState;
        }

        public override void ExitState(LevelScene scene) { }
    }
}
