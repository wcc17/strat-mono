using System;
using System.Collections.Generic;
using System.Linq;
using Components.Character;
using Components.Character.Enemy;
using Components.Character.Player;
using Microsoft.Xna.Framework;
using Nez;
using Nez.BitmapFonts;
using Nez.Sprites;
using StartMono.Util;
using StratMono.Components;
using StratMono.Components.Character;
using StratMono.Components.Character.Player;
using StratMono.Entities;
using StratMono.States;
using StratMono.System;
using StratMono.Util;

namespace StratMono.Scenes
{
    public class LevelScene : Scene
    {
        private readonly Color BlueFill = new Color(99, 155, 255, 200);
        private readonly Color BlueOutline = new Color(91, 110, 225, 200);
        private readonly Color RedFill = new Color(217, 87, 99, 200);
        private readonly Color RedOutline = new Color(172, 50, 50, 200);
        private readonly Color YellowFill = new Color(244, 211, 94, 200);
        private readonly Color YellowOutline = new Color(230, 190, 100, 200);
        private readonly int OutlineWidth = 2;

        private const string TiledMapEntityName = "tiled-map";
        private const string CameraEntityName = "camera";
        private const string CursorEntityName = "cursor";

        private const string CharacterSpriteName = "player";
        private const string Npc1SpriteName = "npc1";
        private const string Npc2SpriteName = "npc2";
        private const string Npc3SpriteName = "npc3";
        private const string CursorSpriteName = "tile_cursor";

        private SpriteAtlas _spriteAtlas;

        private BaseState _state = new States.FieldState.PlayerControlDefaultState();
        //private BaseState _state = new States.FieldState.NpcControlDefaultState();

        public BitmapFont font;
        public TileCursorSystem SceneTileCursorSystem;
        public GridSystem GridSystem;
        public CharacterGridMovementInformation CharacterGridMovementInfo = null;

        public CharacterGridEntity SelectedCharacter;
        public GridTile SelectedTile = null;
        public CharacterGridEntity CharacterBeingAttacked;

        public List<CharacterGridEntity> teamEntities = new List<CharacterGridEntity>();
        public List<CharacterGridEntity> enemyEntities = new List<CharacterGridEntity>();

        public override void Initialize()
        {
            var screenSpaceRenderLayers = new int[]
            {
                (int)RenderLayer.UI,
                (int)RenderLayer.Battle,
                (int)RenderLayer.BattleLevelSeparator,
            };

            var worldRenderLayers = new int[]
            {
                (int)RenderLayer.Cursor,
                (int)RenderLayer.Character,
                (int)RenderLayer.TileHighlightOutline,
                (int)RenderLayer.TileHighlight,
                (int)RenderLayer.TileMap,
            };

            var screenSpaceRenderer = new ScreenSpaceRenderer(0, screenSpaceRenderLayers);
            var renderLayerRenderer = new RenderLayerRenderer(screenSpaceRenderLayers.Length, worldRenderLayers);
            AddRenderer(screenSpaceRenderer);
            AddRenderer(renderLayerRenderer);

            //SetDesignResolution(1920, 1080, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1920, 1080);
            Screen.IsFullscreen = true;

            _spriteAtlas = Content.LoadSpriteAtlas("Content/roots.atlas");

            createTiledMap();
            createCamera();
            createGrid();
            createCharacters_fitToInitialCameraView(numberOfTeamCharacters: 3, numberOfEnemies: 3);
            //createCharacters_randomFullMap(numberOfCharacters: 300);
            createGridCursorEntity();

            _state.EnterState(this);
        }

        public override void Update()
        {
            base.Update();
            updateInputMode();
            updateState();
        }

        public GridEntity AddToGrid(GridEntity entity, int x, int y)
        {
            AddEntity(entity);
            GridSystem.AddToGridTile(entity, x, y);
            return entity;
        }

        public void RemoveFromGrid(GridEntity entity)
        {
            GridSystem.RemoveFromGridTile(entity.Name);

            for (var i = 0; i < enemyEntities.Count; i++)
            {
                if (entity.Name == enemyEntities[i].Name)
                {
                    enemyEntities.RemoveAt(i);
                    break;
                }
            }
            for (var i = 0; i < teamEntities.Count; i++)
            {
                if (entity.Name == teamEntities[i].Name)
                {
                    teamEntities.RemoveAt(i);
                    break;
                }
            }

            entity.Destroy();
        }

        public void SetupMovementTileHighlights()
        {
            // TODO: should actually store how far a character can travel somewhere (maxMovementCost)
            // TODO: I don't think I need to save this info in CharacterGridMovementInfo. I can just get it again when I need it again
            CharacterGridMovementInfo = GetPossibleTilesToMoveTo(SelectedTile, 5);
            foreach (GridTile tile in CharacterGridMovementInfo.TilesInRangeOfCharacter)
            {
                if (tile.CharacterCanMoveThroughThisTile)
                {
                    CreateAndAddPositiveTileHighlight(tile);
                }
                else if (tile.Position == new Point((int)SelectedCharacter.Position.X, (int)SelectedCharacter.Position.Y))
                {
                    CreateAndAddActiveTileHighlight(tile);
                }
                else
                {
                    CreateAndAddNegativeTileHighlight(tile);
                }
            }
        }

        public CharacterGridMovementInformation GetPossibleTilesToMoveTo(GridTile tile, int maxMovementCost)
        {
            return GridSystem.IdentifyPossibleTilesToMoveToTile(tile, maxMovementCost);
        }

        // TODO: This should probably just be in GridSystem?
        public List<GridTile> GetImmediateTilesWithAttackableCharacters(Vector2 position, bool attackerIsEnemy)
        {
            GridTile selectedCharacterTile = GridSystem.GetNearestTileAtPosition(position);
            List<GridTile> neighbors = GridSystem.GetNeighborsOfTile(selectedCharacterTile);
            List<GridTile> tilesWithAttackableCharacters = new List<GridTile>();

            foreach (var gridTile in neighbors)
            {
                var characterEntity = GetCharacterFromSelectedTile(gridTile);

                if (characterEntity != null)
                {
                    var teamCharacterCanAttackEnemyCharacter = !attackerIsEnemy && characterEntity.GetComponent<EnemyComponent>() != null;
                    var enemyCharacterCanAttackTeamCharacter = attackerIsEnemy && characterEntity.GetComponent<EnemyComponent>() == null;
                    var canAttackCharacter = characterEntity != null && (teamCharacterCanAttackEnemyCharacter || enemyCharacterCanAttackTeamCharacter);

                    if (canAttackCharacter)
                    {
                        tilesWithAttackableCharacters.Add(gridTile);
                    }
                }
            }

            return tilesWithAttackableCharacters;
        }

        public GridEntity CreateAndAddPositiveTileHighlight(GridTile gridTile)
        {
            return CreateAndAddTileHighlight(gridTile, BlueOutline, BlueFill);
        }

        public GridEntity CreateAndAddNegativeTileHighlight(GridTile gridTile)
        {
            return CreateAndAddTileHighlight(gridTile, RedOutline, RedFill);
        }

        public GridEntity CreateAndAddActiveTileHighlight(GridTile gridTile)
        {
            return CreateAndAddTileHighlight(gridTile, YellowOutline, YellowFill);
        }

        public bool AllTeamFinishedTurn()
        {
            return AllEntitiesFinishedTurn(teamEntities);
        }

        public bool AllEnemiesFinishedTurn()
        {
            return AllEntitiesFinishedTurn(enemyEntities);
        }

        public bool AllEntitiesFinishedTurn(List<CharacterGridEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                if (entities[i].GetComponent<TurnState>().finishedTurn == false)
                {
                    return false;
                }
            }

            return true;
        }

        public CharacterGridEntity GetNextEnemy()
        {
            for (var i = 0; i < enemyEntities.Count; i++)
            {
                if (!enemyEntities[i].GetComponent<TurnState>().finishedTurn) {
                    return enemyEntities[i];
                }
            }

            return null;
        }

        public void RemoveHighlightsFromGrid()
        {
            List<GridTileHighlight> highlights = EntitiesOfType<GridTileHighlight>();
            foreach (GridTileHighlight highlight in highlights)
            {
                RemoveFromGrid(highlight);
            }
        }

        public CharacterGridEntity GetCharacterFromSelectedTile(GridTile selectedTile)
        {
            List<GridEntity> entitiesOnSelectedTile = selectedTile.OccupyingEntities;
            if (entitiesOnSelectedTile != null)
            {
                foreach (var entity in entitiesOnSelectedTile)
                {
                    if (entity.GetType() == typeof(CharacterGridEntity))
                    {
                        return (CharacterGridEntity)entity;
                    }
                }
            }

            return null;
        }

        public SpriteAnimator CreateSpriteAnimator(string spriteName)
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

        private void updateState()
        {
            var previousState = _state;
            _state = _state.Update(this, (GridEntity)FindEntity(CursorEntityName));
            if (previousState != _state)
            {
                Console.WriteLine("Previous State: " + previousState.ToString());
                Console.WriteLine("New State: " + _state.ToString());
                Console.WriteLine();

                previousState.ExitState(this);
                _state.EnterState(this);
            }
        }

        private GridEntity CreateAndAddTileHighlight(GridTile gridTile, Color outlineColor, Color fillColor)
        {
            GridEntity tileHighlight = new GridTileHighlight("highlight" + gridTile.Id);

            SpriteRenderer outline = PrimitiveShapeUtil.CreateRectangleOutlineSprite(
                64, 64, outlineColor, OutlineWidth);
            SpriteRenderer shape = PrimitiveShapeUtil.CreateRectangleSprite(
                64, 64, fillColor);

            shape.RenderLayer = (int)RenderLayer.TileHighlight;
            outline.RenderLayer = (int)RenderLayer.TileHighlightOutline;
            tileHighlight.AddComponent(outline);
            tileHighlight.AddComponent(shape);

            AddToGrid(tileHighlight, gridTile.Coordinates.X, gridTile.Coordinates.Y);

            return tileHighlight;
        }

        private void createTiledMap()
        {
            var tiledMapEntity = CreateEntity(TiledMapEntityName);
            var tiledMap = Content.LoadTiledMap("Content/assets/tiles/test_scene_map_2.tmx");
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
        }

        private void createGrid()
        {
            var tiledMapEntity = FindEntity(TiledMapEntityName);
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;
            GridSystem = new GridSystem(
                new Point(tiledMap.TileWidth, tiledMap.TileHeight),
                new Point(tiledMap.WorldWidth, tiledMap.WorldHeight),
                tiledMapEntity.GetComponent<TiledMapRenderer>().TiledMap.GetObjectGroup("bounds"),
                tiledMapEntity.GetComponent<TiledMapRenderer>().TiledMap.GetObjectGroup("move_cost"));
        }

        private void createCharacters_randomFullMap(int numberOfCharacters)
        {
            var tiledMapEntity = FindEntity(TiledMapEntityName);
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;

            for (var i = 0; i < numberOfCharacters; i++)
            {
                CharacterGridEntity characterEntity;
                int npc = Nez.Random.Range(0, 4);
                switch (npc)
                {
                    case 0:
                        characterEntity = createEnemyCharacter(Npc1SpriteName);
                        break;
                    case 1:
                        characterEntity = createEnemyCharacter(Npc2SpriteName);
                        break;
                    case 2:
                        characterEntity = createEnemyCharacter(Npc3SpriteName);
                        break;
                    default:
                        characterEntity = createTeamCharacter(CharacterSpriteName);
                        break;
                }

                placeCharacterOnRandomTile(5, tiledMap.WorldWidth / 64 - 5, 5, tiledMap.WorldWidth / 64 - 5, characterEntity);
            }
        }

        private void createCharacters_fitToInitialCameraView(int numberOfTeamCharacters, int numberOfEnemies)
        {
            var tiledMapEntity = FindEntity(TiledMapEntityName);
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;

            for (var i = 0; i < numberOfTeamCharacters; i++)
            {
                var characterEntity = createTeamCharacter(CharacterSpriteName);
                placeCharacterOnRandomTile(5, 25, 5, 15, characterEntity);
            }

            for (var i = 0; i < numberOfEnemies; i++)
            {

                int npc = Nez.Random.Range(0, 3);
                string spriteName = Npc1SpriteName;
                switch (npc)
                {
                    case 0:
                        spriteName = Npc1SpriteName;
                        break;
                    case 1:
                        spriteName = Npc2SpriteName;
                        break;
                    case 2:
                        spriteName = Npc3SpriteName;
                        break;
                }

                CharacterGridEntity characterEntity = createEnemyCharacter(spriteName);
                placeCharacterOnRandomTile(5, 25, 5, 15, characterEntity);
            }
        }

        private CharacterGridEntity createTeamCharacter(string spriteName)
        {
            CharacterGridEntity characterEntity = createCharacter(spriteName);
            characterEntity.AddComponent(new PlayerTeamComponent());
            characterEntity.AddComponent(new PlayerCharacterTurnState());
            characterEntity.AddComponent(new Health(100));
            this.teamEntities.Add(characterEntity);
            return characterEntity;
        }

        private CharacterGridEntity createEnemyCharacter(string spriteName)
        {
            CharacterGridEntity characterEntity = createCharacter(spriteName);
            characterEntity.AddComponent(new EnemyComponent());
            characterEntity.AddComponent(new EnemyTurnState());
            characterEntity.AddComponent(new Health(100));
            enemyEntities.Add(characterEntity);
            return characterEntity;
        }

        private CharacterGridEntity createCharacter(string spriteName)
        {
            SpriteAnimator spriteAnimator;
            CharacterGridEntity characterEntity = new CharacterGridEntity();

            spriteAnimator = CreateSpriteAnimator(spriteName);
            characterEntity.SpriteName = spriteName;

            spriteAnimator.RenderLayer = (int)RenderLayer.Character;
            characterEntity.AddComponent(spriteAnimator);
            characterEntity.AddComponent(new CharacterAnimatedMovement());

            return characterEntity;
        }

        private void placeCharacterOnRandomTile(int xMin, int xMax, int yMin, int yMax, GridEntity characterEntity)
        {
            int x = Nez.Random.Range(xMin, xMax);
            int y = Nez.Random.Range(yMin, yMax);
            bool tileInaccessible = true;
            while (tileInaccessible)
            {
                x = Nez.Random.Range(xMin, xMax);
                y = Nez.Random.Range(yMin, yMax);
                tileInaccessible = !GridSystem.GetGridTileFromCoords(new Point(x, y)).IsAccessible
                    || GetCharacterFromSelectedTile(GridSystem.GetGridTileFromCoords(new Point(x, y))) != null;
            }

            AddToGrid(characterEntity, x, y);
        }

        private void createGridCursorEntity()
        {
            SceneTileCursorSystem = new TileCursorSystem();
            var cursorEntity = new GridEntity(CursorEntityName);
            var spriteAnimator = CreateSpriteAnimator(CursorSpriteName);
            spriteAnimator.RenderLayer = (int)RenderLayer.Cursor;
            spriteAnimator.Play("default", SpriteAnimator.LoopMode.PingPong);
            cursorEntity.AddComponent(spriteAnimator);
            AddToGrid(cursorEntity, 5, 13);
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
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.B)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.RightShoulder) 
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.LeftShoulder)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadLeft)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadRight)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadUp)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadDown))
                {
                    InputMode.CurrentInputMode = InputModeType.Controller;
                }
            }
        }
    }
}