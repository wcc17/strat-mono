using Microsoft.Xna.Framework;
using StratMono.Entities;
using System;
using System.Collections.Generic;

namespace StratMono.System
{
    public class GridTile
    {
        public int Id; // NOTE: also doubles as the index - can be converted to exact x,y coordinate by converting from 1d index to 2d index
        public Point Coordinates;
        public Point Position;
        public List<GridEntity> OccupyingEntities = new List<GridEntity>();
        public int MoveCost;

        private bool _isAccessible;
        public bool IsAccessible
        {
            get
            {
                return _isAccessible;
            }

            set
            {
                _isAccessible = value;
                if (!_isAccessible)
                {
                    _characterCanMoveThroughThisTile = false;
                }
            }
        }

        private bool _characterCanMoveThroughThisTile;
        public bool CharacterCanMoveThroughThisTile
        {
            get
            {
                return _characterCanMoveThroughThisTile;
            }

            set
            {
                if (!_isAccessible)
                {
                    _characterCanMoveThroughThisTile = false;
                }

                _characterCanMoveThroughThisTile = value;
            }
        }

        public GridTile(int id, int gridX, int gridY, int x, int y)
        {
            Id = id;
            MoveCost = 1;
            IsAccessible = true;
            CharacterCanMoveThroughThisTile = true;
            Coordinates = new Point(gridX, gridY);
            Position = new Point(x, y);
        }

        public void AddToTile(GridEntity gridEntity)
        {
            OccupyingEntities.Add(gridEntity);

            if (gridEntity.GetType() == typeof(CharacterGridEntity))
            {
                CharacterCanMoveThroughThisTile = false;
            }
        }

        public void RemoveFromTile(string gridEntityName)
        {
            for (int i = OccupyingEntities.Count - 1; i >= 0; i--)
            {
                if (OccupyingEntities[i].Name.Equals(gridEntityName))
                {
                    if (OccupyingEntities[i].GetType() == typeof(CharacterGridEntity))
                    {
                        CharacterCanMoveThroughThisTile = true;
                    }

                    OccupyingEntities.RemoveAt(i);
                    break;
                }
            }
        }

        public override bool Equals(object obj)
        {
            var item = obj as GridTile;

            if (item == null)
            {
                return false;
            }

            return this.Id.Equals(item.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString() + " ID: " + Id + ", Coordinates: (" + Coordinates.X + ", " + Coordinates.Y + ")";
        }
    }
}
