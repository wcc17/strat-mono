using Nez;

namespace StratMono.Components.Character
{
    class Health : Component
    {
        public int maxHealth;
        public int currentHealth;

        public Health(int maxHealth)
        {
            this.maxHealth = maxHealth;
            currentHealth = maxHealth;
        }

        public int changeHealth(int delta)
        {
            currentHealth += delta;
            return currentHealth;
        }
    }
}
