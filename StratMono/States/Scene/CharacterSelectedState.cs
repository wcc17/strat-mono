using StratMono.Scenes;
using StratMono.System;
using System;

namespace StratMono.States.Scene
{
    public class CharacterSelectedState : BaseState
    {
        public override BaseState HandleSelectedCharacter(
            LevelScene scene, 
            GridSystem gridSystem)
        {
            return this;
        }
    }
}
