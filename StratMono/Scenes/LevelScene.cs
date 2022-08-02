using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using StartMono.Util;
using StratMono.Components;
using StratMono.Entities;
using StratMono.System;
using StratMono.Util;

namespace StratMono.Scenes
{
    public class LevelScene : Scene
    {
        enum RenderLayer : int
        {
            Cursor = 0,
            Character = 1,
            TileHighlightOutline = 2,
            TileHighlight = 3,
            TileMap = 4,
        }

        private readonly Color BlueFill = new Color(99, 155, 255, 200);
        private readonly Color BlueOutline = new Color(91, 110, 225, 200);
        private readonly Color RedFill = new Color(217, 87, 99, 200);
        private readonly Color RedOutline = new Color(172, 50, 50, 200);

        private const string TiledMapEntityName = "tiled-map";
        private const string CameraEntityName = "camera";
        private const string CursorEntityName = "cursor";

        private const string CharacterSpriteName = "player";
        private const string CursorSpriteName = "tile_cursor";

        private const string TiledMapBoundsLayerName = "bounds";
        private const string TiledMapMoveCostLayerName = "move_cost";

        private SpriteAtlas _spriteAtlas;
        private GridSystem _gridSystem;
        private TileCursorSystem _tileCursorSystem;

        public override void Initialize()
        {
            var defaultRenderer = new DefaultRenderer();
            this.AddRenderer(defaultRenderer);

            //SetDesignResolution(1920, 1080, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1920, 1080);
            Screen.IsFullscreen = true;

            _spriteAtlas = Content.LoadSpriteAtlas("Content/roots.atlas");
            _tileCursorSystem = new TileCursorSystem();

            createTiledMap();
            createCamera();
            createGrid();
            createCharacter();
            createGridCursorEntity();
        }

        public override void Update()
        {
            base.Update();

            updateInputMode();

            var cursorEntity = FindEntity(CursorEntityName);
            _tileCursorSystem.Update(
                cursorEntity,
                Camera);
            _gridSystem.Update(
                cursorEntity,
                EntitiesOfType<GridEntity>(),
                (BoundedMovingCamera)Camera);

            var selectedCharacter = checkForSelectedCharacter();
            handleSelectedCharacter(selectedCharacter);
        }

        private void createTiledMap()
        {
            var tiledMapEntity = CreateEntity(TiledMapEntityName);
            var tiledMap = Content.LoadTiledMap("Content/assets/tiles/test_scene_map.tmx");
            var tiledMapRenderer = new TiledMapRenderer(tiledMap);

            tiledMapRenderer.RenderLayer = (int)RenderLayer.TileMap;
            tiledMapEntity.AddComponent(tiledMapRenderer);
        }

        private void createCamera()
        {
            var tiledMapEntity = FindEntity(TiledMapEntityName);
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;

            var cameraEntity = CreateEntity(CameraEntityName);
            var levelBounds = new RectangleF(Vector2.Zero, new Vector2(tiledMap.WorldWidth, tiledMap.WorldHeight));
            Camera = cameraEntity.AddComponent(new BoundedMovingCamera(levelBounds));
            //Camera.ZoomIn(5);
        }

        private void createGrid()
        {
            var tiledMapEntity = FindEntity(TiledMapEntityName);
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;
            _gridSystem = new GridSystem(
                new Point(tiledMap.TileWidth, tiledMap.TileHeight),
                new Point(tiledMap.WorldWidth, tiledMap.WorldHeight),
                tiledMapEntity.GetComponent<TiledMapRenderer>().TiledMap.GetObjectGroup("bounds"),
                tiledMapEntity.GetComponent<TiledMapRenderer>().TiledMap.GetObjectGroup("move_cost"));
        }

        private void createCharacter()
        {
            var tiledMapEntity = FindEntity(TiledMapEntityName);
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;

            for (var i = 0; i < 500; i++)
            {
                var characterEntity = new CharacterGridEntity();
                var spriteAnimator = createSpriteAnimator(CharacterSpriteName);
                spriteAnimator.RenderLayer = (int)RenderLayer.Character;
                characterEntity.AddComponent(spriteAnimator);
                characterEntity.AddComponent(new CharacterMovement());

                int x = Nez.Random.Range(5, tiledMap.WorldWidth / 64);
                int y = Nez.Random.Range(5, tiledMap.WorldHeight / 64);
                addToGrid(characterEntity, x, y);

            }
        }

        private void createGridCursorEntity()
        {
            var cursorEntity = new GridEntity(CursorEntityName);
            var spriteAnimator = createSpriteAnimator(CursorSpriteName);
            spriteAnimator.RenderLayer = (int)RenderLayer.Cursor;
            spriteAnimator.Play("default", SpriteAnimator.LoopMode.PingPong);
            cursorEntity.AddComponent(spriteAnimator);
            addToGrid(cursorEntity, 5, 13);
        }

        private GridEntity addToGrid(GridEntity entity, int x, int y)
        {
            AddEntity(entity);
            _gridSystem.AddToGridTile(entity, x, y);
            return entity;
        }

        private SpriteAnimator createSpriteAnimator(string spriteName)
        {
            var animationNames = _spriteAtlas.AnimationNames
                .Where(animationName => animationName.Contains(spriteName))
                .ToList();

            SpriteAnimator animator = new SpriteAnimator();
            foreach (var animationName in animationNames)
            {
                var name = animationName.Replace(spriteName + "_", "");
                animator.AddAnimation(
                    name,
                    _spriteAtlas.GetAnimation(animationName)
                );
            }

            return animator;
        }

        //TODO: needs to be updated as more controls are added
        private void updateInputMode()
        {
            if (Input.CurrentKeyboardState.GetPressedKeys().Length > 0
                || Input.MousePositionDelta.X > 0
                || Input.MousePositionDelta.Y > 0)
            {
                InputMode.CurrentInputMode = InputModeType.KeyboardMouse;
            }

            // NOTE: only supporting one gamepad for now
            if (Input.GamePads.Length > 0 && Input.GamePads[0].IsConnected())
            {
                var gamepad = Input.GamePads[0];
                if (gamepad.IsLeftStickDown()
                    || gamepad.IsLeftStickUp()
                    || gamepad.IsLeftStickRight()
                    || gamepad.IsLeftStickLeft()
                    || gamepad.IsRightStickDown()
                    || gamepad.IsRightStickUp()
                    || gamepad.IsRightStickRight()
                    || gamepad.IsRightStickLeft()
                    || gamepad.IsRightTriggerPressed()
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A))
                {
                    InputMode.CurrentInputMode = InputModeType.Controller;
                }
            }
        }

        private CharacterGridEntity checkForSelectedCharacter()
        {
            List<GridEntity> entitiesOnSelectedTile = _gridSystem.GetEntitiesFromSelectedTile();
            if (entitiesOnSelectedTile != null)
            {
                foreach (var entity in entitiesOnSelectedTile)
                {
                    if (entity.GetType() == typeof(CharacterGridEntity))
                    {
                        return (CharacterGridEntity)entity;
                    }
                    break;
                }
            }

            return null;
        }

        bool handledCharacterSelect = false; // TODO: do better
        private void handleSelectedCharacter(CharacterGridEntity character)
        {
            if (character == null)
            {
                return;
            }

            if (handledCharacterSelect)
            {
                return;
            }

            // TODO: get all possible squares that we could travel to
            // TODO: should actually store how far a character can travel somewhere (maxMovementCost)
            var tilesInRange = _gridSystem.IdentifyPossibleTilesToMoveToFromSelectedTile(5);
            foreach(GridTile tile in tilesInRange)
            {
                GridEntity tileHighlight = new GridTileHighlight("highlight" + tile.Id);
                SpriteRenderer outline;
                SpriteRenderer shape;
                if (tile.CharacterCanMoveThroughThisTile)
                {
                    outline = PrimitiveShapeUtil.CreateRectangleOutlineSprite(64, 64, BlueOutline, 2);
                    shape = PrimitiveShapeUtil.CreateRectangleSprite(64, 64, BlueFill);
                } else
                {
                    outline = PrimitiveShapeUtil.CreateRectangleOutlineSprite(64, 64, RedOutline, 2);
                    shape = PrimitiveShapeUtil.CreateRectangleSprite(64, 64, RedFill);
                }

                shape.RenderLayer = (int)RenderLayer.TileHighlight;
                outline.RenderLayer = (int)RenderLayer.TileHighlightOutline;
                tileHighlight.AddComponent(outline);
                tileHighlight.AddComponent(shape);

                Point tileCoordinates = _gridSystem.GetTileCoordinates(tile);
                addToGrid(tileHighlight, tileCoordinates.X, tileCoordinates.Y);
            }

            handledCharacterSelect = true;
        }
    }
}

//static identifyTilesForPossibleCharacterMovement(startTile, grid, gridWidthInTiles, gridHeightInTiles)
//{
//    let maxCost = 5; //TODO: will depend on the character that is being moved
//    let cameFrom = new Map(); //tileId > tileId map. what tile we came from and where we went
//    let costSoFar = new Map(); //tileId > cost to get to that tile
//    let inaccessibleTileIdsInRange = [];
//    let frontier = new PriorityQueue((a, b) =>
//    {
//        return a.cost - b.cost;
//    });

//    frontier.enq({ tile: startTile, cost: 0 });
//    cameFrom.set(startTile.id, undefined);
//    costSoFar.set(startTile.id, 0);

//    while (!frontier.isEmpty())
//    {
//        let current = frontier.deq().tile;

//        let costToGetHere = costSoFar.get(current.id);
//        if (costToGetHere < maxCost)
//        {
//            let neighbors = GridUtil.getNeighborsOfTile(current, grid, gridWidthInTiles, gridHeightInTiles);

//            for (let i = 0; i < neighbors.length; i++)
//            {
//                let neighbor = neighbors[i];

//                if (neighbor.characterCanMoveThroughTile())
//                {
//                    let newCost = costToGetHere + neighbor.cost;

//                    if (!costSoFar.has(neighbor.id) || newCost < costSoFar.get(neighbor.id))
//                    {
//                        costSoFar.set(neighbor.id, newCost);
//                        cameFrom.set(neighbor.id, current);
//                        frontier.enq({ tile: neighbor, cost: newCost });
//                     }
//                } else
//               {
//                  if (!inaccessibleTileIdsInRange.includes(neighbor.id))
//                  {
//                       inaccessibleTileIdsInRange.push(neighbor.id);
//                  }
//               }
//            }
//        }
//    }

//    return [cameFrom, costSoFar, inaccessibleTileIdsInRange];
//}

//getTilesWithinRangeOfSelectedCharacter(selectedTile) {
//    let[pathsMap, costsMap, inaccessibleTileIdsInRange] = GridPathfindingUtil.identifyTilesForPossibleCharacterMovement(selectedTile, this._grid, this._gridTileWidth, this._gridTileHeight);
//    let pathsToTileIdsWithinRangeOfSelected = pathsMap;
//    let tileIdsWithinRangeOfSelected = MapUtil.getKeyArrayFromMap(costsMap);
//    tileIdsWithinRangeOfSelected.push.apply(tileIdsWithinRangeOfSelected, inaccessibleTileIdsInRange);

//    let tilesWithinRange = [];
//    for (let i = 0; i < tileIdsWithinRangeOfSelected.length; i++)
//    {
//        tilesWithinRange.push(this.getTileAtIndex(tileIdsWithinRangeOfSelected[i]));
//    }

//    return [tilesWithinRange, pathsToTileIdsWithinRangeOfSelected];
//}
