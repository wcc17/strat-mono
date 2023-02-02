using Nez;

namespace StratMono.Entities
{
    public class CharacterGridEntity : GridEntity
    {
        public string SpriteName;

        public Entity rotationEntity;

        public CharacterGridEntity() : base() { }

        public CharacterGridEntity(string name) : base(name) { }
    }
}