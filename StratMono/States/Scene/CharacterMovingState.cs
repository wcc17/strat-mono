using Microsoft.Xna.Framework;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using System.Collections.Generic;
using System;

namespace StratMono.States.Scene
{
    public class CharacterMovingState : BaseState
    {
        // Both of these are in case the character moves, finishes, and then cancels the movement
        private readonly Dictionary<GridTile, GridTile> _possiblePathsFromCharacter;
        private readonly GridTile _initialTile;
        private readonly GridTile _goalTile;
        private readonly bool _returnedToOriginalPosition;

        public CharacterMovingState(
            Dictionary<GridTile, GridTile> possiblePathsFromCharacter,
            GridTile initialTile,
            GridTile goalTile,
            bool returnedToOriginalPosition = false) : base()
        {
            _possiblePathsFromCharacter = possiblePathsFromCharacter;
            _initialTile = initialTile;
            _goalTile = goalTile;
            _returnedToOriginalPosition = returnedToOriginalPosition;

            Console.WriteLine("initial tile: " + _initialTile);
            Console.WriteLine("goal tile: " + _goalTile);
        }

        public override void EnterState(LevelScene scene) 
        {
            GridTile nextTile = _goalTile;
            Stack<GridTile> pathToTake = new Stack<GridTile>();
            while (nextTile != null)
            {
                pathToTake.Push(nextTile);
                _possiblePathsFromCharacter.TryGetValue(nextTile, out nextTile);
            }

            scene.SelectedCharacter.AddComponent(new GridEntityMoveToGoal(pathToTake));
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            BaseState nextState = this;

            CenterCameraOnPosition(scene, scene.SelectedCharacter.Position);

            if (!scene.SelectedCharacter.GetComponent<GridEntityMoveToGoal>().Enabled)
            {
                scene.SelectedCharacter.RemoveComponent<GridEntityMoveToGoal>();

                // This will be set to true if character moved, then the user cancelled the action
                if (_returnedToOriginalPosition)
                {
                    scene.SelectedCharacter = null;
                    scene.SelectedTile = null;
                    nextState = new DefaultState();
                    nextState.EnterState(scene);
                    return nextState;
                }

                nextState = new CharacterFinishedMovingState(
                    _possiblePathsFromCharacter,
                    _initialTile,
                    _goalTile);
                nextState.EnterState(scene);
            }

            return nextState;
        }
    }
}
