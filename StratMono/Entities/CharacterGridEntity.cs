namespace StratMono.Entities
{
    public class CharacterGridEntity : GridEntity
    {
        public string SpriteName;

        public CharacterGridEntity() : base() { }

        public CharacterGridEntity(string name) : base(name) { }
    }
}