using Nez;

namespace Components
{
    class TurnStateComponent : Component
    {
        public bool alreadyMoved = false;
        public bool alreadyAttacked = false;
        public bool finishedTurn = false;

        public void reset()
        {
            alreadyMoved = false;
            alreadyAttacked = false;
            finishedTurn = false;
        }
    }
}