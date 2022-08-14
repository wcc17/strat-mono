namespace StratMono.States.BattleState
{
    public abstract class BaseBattleState : BaseState
    {
        public readonly string BattlePlayerEntityName = "BattlePlayerCharacter";
        public readonly string BattleNpcEntityName = "BattleNpcCharacter";
        public readonly string ScreenOverlayEntityName = "screenoverlay";
        public readonly string ActionMenuEntityName = "battleactionmenu";
    }
}
