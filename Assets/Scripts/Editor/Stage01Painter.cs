using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MarkOfAscension.Editor
{
    public static class Stage01Painter
    {
        private const string ScenePath = "Assets/Scenes/Stage01.unity";
        private const string WallBase = "Assets/Cainos/Pixel Art Top Down - Basic/Tile Palette/TP Wall/";
        private const string StoneBase = "Assets/Cainos/Pixel Art Top Down - Basic/Tile Palette/TP Stone Ground/";

        private static TileBase GroundA => T(StoneBase + "TX Tileset Stone Ground_0.asset");
        private static TileBase GroundB => T(StoneBase + "TX Tileset Stone Ground_1.asset");
        private static TileBase GroundC => T(StoneBase + "TX Tileset Stone Ground_2.asset");
        private static TileBase GroundD => T(StoneBase + "TX Tileset Stone Ground_3.asset");
        private static TileBase HazardTile => T(StoneBase + "TX Tileset Stone Ground_24.asset");
        private static TileBase DecorTile => T(StoneBase + "TX Tileset Stone Ground_40.asset");
        private static TileBase TopLeft => T(WallBase + "TX Tileset Wall_4.asset");
        private static TileBase TopMid => T(WallBase + "TX Tileset Wall_17.asset");
        private static TileBase TopRight => T(WallBase + "TX Tileset Wall_12.asset");
        private static TileBase BodyFill => T(WallBase + "TX Tileset Wall_7.asset");
        private static TileBase SideLeft => T(WallBase + "TX Tileset Wall_15.asset");
        private static TileBase SideMid => T(WallBase + "TX Tileset Wall_19.asset");
        private static TileBase BottomLeft => T(WallBase + "TX Tileset Wall_6.asset");
        private static TileBase BottomMid => T(WallBase + "TX Tileset Wall_5.asset");
        private static TileBase BottomRight => T(WallBase + "TX Tileset Wall_1.asset");

        [MenuItem("Tools/Mark of Ascension/Build Stage01 Prototype")]
        public static void BuildStage01()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var ground = FindTilemap("Ground");
            var walls = FindTilemap("Walls");
            var decor = FindTilemap("Decor");
            var hazards = FindTilemap("Hazards");

            if (ground == null || walls == null || decor == null || hazards == null)
            {
                Debug.LogError("[Stage01Painter] Missing one or more Stage01 tilemaps."
                );
                return;
            }

            Undo.RecordObject(ground, "Paint Stage01 Ground");
            Undo.RecordObject(walls, "Paint Stage01 Walls");
            Undo.RecordObject(decor, "Paint Stage01 Decor");
            Undo.RecordObject(hazards, "Paint Stage01 Hazards");

            ground.ClearAllTiles();
            walls.ClearAllTiles();
            decor.ClearAllTiles();
            hazards.ClearAllTiles();

            PaintGround(ground);
            PaintWalls(walls);
            PaintDecor(decor);
            PaintHazards(hazards);

            hazards.color = new Color(0.45f, 1f, 0.5f, 0.75f);
            var wallCollider = walls.GetComponent<TilemapCollider2D>();
            if (wallCollider != null)
            {
                wallCollider.usedByComposite = true;
            }
            walls.GetComponent<CompositeCollider2D>()?.GenerateGeometry();

            PositionMarkers();

            EditorUtility.SetDirty(ground);
            EditorUtility.SetDirty(walls);
            EditorUtility.SetDirty(decor);
            EditorUtility.SetDirty(hazards);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Stage01Painter] Stage01 prototype painted. Save the scene with Ctrl+S."
            );
        }

        private static void PaintGround(Tilemap tm)
        {
            TileBase[] pool = { GroundA, GroundA, GroundB, GroundC, GroundD };
            var rng = new System.Random(101);

            FillRoom(tm, -4, -8, 4, -4, pool, rng);
            FillRoom(tm, -6, -3, 5, 6, pool, rng);
            FillRoom(tm, -1, 7, 1, 12, pool, rng);
            FillRoom(tm, -5, 13, 4, 20, pool, rng);
        }

        private static void PaintWalls(Tilemap tm)
        {
            PaintRoomWalls(tm, -4, -8, 4, -4, false, true);
            PaintRoomWalls(tm, -6, -3, 5, 6, true, true);
            PaintRoomWalls(tm, -1, 7, 1, 12, true, true);
            PaintRoomWalls(tm, -5, 13, 4, 20, true, false);
        }

        private static void PaintDecor(Tilemap tm)
        {
            var decorCells = new[]
            {
                new Vector3Int(-3, -6, 0),
                new Vector3Int(3, -6, 0),
                new Vector3Int(-5, 4, 0),
                new Vector3Int(4, 3, 0),
                new Vector3Int(0, 10, 0),
                new Vector3Int(-4, 18, 0),
                new Vector3Int(3, 18, 0)
            };

            foreach (var cell in decorCells)
            {
                tm.SetTile(cell, DecorTile);
            }
        }

        private static void PaintHazards(Tilemap tm)
        {
            FillRect(tm, -4, 2, -3, 3, HazardTile);
            FillRect(tm, 2, 9, 3, 11, HazardTile);
        }

        private static void PositionMarkers()
        {
            SetPosition("PlayerSpawn", new Vector3(0f, -6.5f, 0f));
            SetPosition("EnemySpawn_01", new Vector3(-3f, 1f, 0f));
            SetPosition("EnemySpawn_02", new Vector3(3f, 3f, 0f));
            SetPosition("BossSpawn", new Vector3(0f, 16f, 0f));
            SetPosition("ExitPortal_Closed", new Vector3(0f, 19f, 0f));
            SetPosition("BossGatePlaceholder", new Vector3(0f, 12.5f, 0f));

            ResizeHazard("Hazard_Poison_01", new Vector2(-3.5f, 2.5f), new Vector2(2f, 2f));
            ResizeHazard("Hazard_Poison_02", new Vector2(2.5f, 10f), new Vector2(2f, 3f));
            ResizeBossGate();
        }

        private static void ResizeHazard(string name, Vector2 position, Vector2 size)
        {
            var go = GameObject.Find(name);
            if (go == null) return;
            go.transform.position = position;
            var collider = go.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
                collider.size = size;
            }
            var renderer = go.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = size;
                renderer.color = new Color(0.35f, 0.8f, 0.25f, 0.45f);
                renderer.sortingOrder = 4;
            }
        }

        private static void ResizeBossGate()
        {
            var go = GameObject.Find("BossGatePlaceholder");
            if (go == null) return;
            var collider = go.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = new Vector2(3f, 1f);
                collider.offset = Vector2.zero;
            }
        }

        private static void FillRoom(Tilemap tm, int minX, int minY, int maxX, int maxY, TileBase[] pool, System.Random rng)
        {
            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                tm.SetTile(new Vector3Int(x, y, 0), pool[rng.Next(pool.Length)]);
        }

        private static void FillRect(Tilemap tm, int minX, int minY, int maxX, int maxY, TileBase tile)
        {
            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                tm.SetTile(new Vector3Int(x, y, 0), tile);
        }

        private static void PaintRoomWalls(Tilemap tm, int minX, int minY, int maxX, int maxY, bool openBottom, bool openTop)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (!openBottom || x < -1 || x > 1)
                    tm.SetTile(new Vector3Int(x, minY, 0), x == minX ? BottomLeft : x == maxX ? BottomRight : BottomMid);

                if (!openTop || x < -1 || x > 1)
                {
                    tm.SetTile(new Vector3Int(x, maxY + 1, 0), BodyFill);
                    tm.SetTile(new Vector3Int(x, maxY, 0), x == minX ? TopLeft : x == maxX ? TopRight : TopMid);
                }
            }

            for (int y = minY + 1; y <= maxY - 1; y++)
            {
                tm.SetTile(new Vector3Int(minX, y, 0), y == maxY - 1 ? SideLeft : SideMid);
                tm.SetTile(new Vector3Int(maxX, y, 0), y == maxY - 1 ? SideLeft : SideMid);
            }
        }

        private static void SetPosition(string name, Vector3 position)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                go.transform.position = position;
            }
        }

        private static Tilemap FindTilemap(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.GetComponent<Tilemap>() : null;
        }

        private static TileBase T(string path)
        {
            return AssetDatabase.LoadAssetAtPath<TileBase>(path);
        }
    }
}
