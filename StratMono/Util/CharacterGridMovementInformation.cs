using StratMono.System;
using System.Collections.Generic;

namespace StratMono.Util
{
    public class CharacterGridMovementInformation
    {
        public Dictionary<GridTile, GridTile> PathsFromCharacterToTilesInRange;
        public HashSet<GridTile> TilesInRangeOfCharacter;

        public CharacterGridMovementInformation(
            Dictionary<GridTile, GridTile> pathsFromCharacterToTilesInRange,
            HashSet<GridTile> tilesInRangeOfCharacter)
        {
            PathsFromCharacterToTilesInRange = pathsFromCharacterToTilesInRange;
            TilesInRangeOfCharacter = tilesInRangeOfCharacter;
        }
    }
}
