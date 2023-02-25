using Nez;

namespace StratMono.Components.Character
{
    class Health : Component
    {
        public int maxHealth;
        public float currentHealth;

        public Health(int maxHealth)
        {
            this.maxHealth = maxHealth;
            currentHealth = maxHealth;
        }

        public float changeHealth(float delta)
        {
            currentHealth += delta;
            return currentHealth;
        }
    }
}
