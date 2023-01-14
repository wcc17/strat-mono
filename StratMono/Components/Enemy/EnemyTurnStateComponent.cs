using Nez;

namespace Components.Enemy
{
    class EnemyTurnStateComponent : Component
    {
        public bool alreadyMoved = false;
        public bool alreadyAttacked = false;

        public void reset()
        {
            alreadyMoved = false;
            alreadyAttacked = false;
        }
    }
}
