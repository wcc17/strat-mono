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
        private readonly CharacterGridEntity _characterToMove;
        private readonly float _moveSpeed;
        private readonly bool _returnedToOriginalPosition;
        private readonly bool _isPlayerCharacter;

        public CharacterMovingState(
            Stack<GridTile> pathToTake,
            CharacterGridEntity characterToMove,
            float moveSpeed,
            bool returnedToOriginalPosition = false,
            bool isPlayerCharacter = true) : base()
        {
            _pathToTake = pathToTake;
            _characterToMove = characterToMove;
            _moveSpeed = moveSpeed;
            _returnedToOriginalPosition = returnedToOriginalPosition;
            _isPlayerCharacter = isPlayerCharacter;
        }

        public override void EnterState(LevelScene scene) 
        {
            // to create the initial new stack, it pops off all of the elements. So we have to do it twice to put it back in the right order
            var stackClone = new Stack<GridTile>(new Stack<GridTile>(_pathToTake));
            _characterToMove.AddComponent(new GridEntityMoveToGoal(stackClone, _moveSpeed));
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            BaseFieldState nextState = this;

            CenterCameraOnPosition(scene, _characterToMove.Position);
            (scene.Camera as BoundedMovingCamera).Update();

            if (!_characterToMove.GetComponent<GridEntityMoveToGoal>().Enabled)
            {
                _characterToMove.RemoveComponent<GridEntityMoveToGoal>();

                // This will be set to true if character moved, then the user cancelled the action
                if (_returnedToOriginalPosition)
                {
                    // if we're on controller, put the cursor back to where we started
                    if (InputMode.CurrentInputMode == InputModeType.Controller)
                    {
                        var originalTilePosition = new Stack<GridTile>(_pathToTake).Pop().Position;
                        cursorEntity.Position = new Vector2(originalTilePosition.X, originalTilePosition.Y);
                    }

                    scene.SelectedTile = null;
                    nextState = new PlayerControlDefaultState();
                    return nextState;
                }

                if (_isPlayerCharacter)
                {
                    nextState = new PlayerCharacterSelectActionState(new Stack<GridTile>(_pathToTake));
                } else
                {
                    nextState = new NpcControlDefaultState(_characterToMove);
                }
            }

            return nextState;
        }

        public override void ExitState(LevelScene scene) { }
    }
}
