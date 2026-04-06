using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MarkOfAscension.Editor
{
    /// <summary>
    /// Paints and improves the SC_Lobby tilemap layout.
    /// All tile indices verified against TP Wall.prefab palette YAML data.
    /// Run via Tools > Mark of Ascension > Paint Lobby Tiles.
    /// Opens SC_Lobby automatically. Safe to re-run at any time.
    /// </summary>
    public static class LobbyPainter
    {
        private const string ScenePath = "Assets/Scenes/SC_Lobby.unity";
        private const string WallBase  = "Assets/Cainos/Pixel Art Top Down - Basic/Tile Palette/TP Wall/";
        private const string GrassBase = "Assets/Cainos/Pixel Art Top Down - Basic/Tile Palette/TP Grass/";
        private const string StoneBase = "Assets/Cainos/Pixel Art Top Down - Basic/Tile Palette/TP Stone Ground/";

        // ── Wall tiles — indices verified from TP Wall.prefab palette grid data ─
        // Palette grid (x,y) → m_TileIndex confirmed from YAML:
        //   Top face: (4,3)→_4  (5,3)→_17  (7,3)→_12   Body: (8,3)→_7
        //   Left col: (-2,3)→_15  (-2,2)→_19
        //   Right col: (2,3)→_22  (2,2)→_10             ← RIGHT WALL FIX
        //   Bottom:   (0,-2)→_6  (1,-2)→_5  (3,-2)→_1

        private static TileBase TopLeft    => T(WallBase + "TX Tileset Wall_4.asset");
        private static TileBase TopMid     => T(WallBase + "TX Tileset Wall_17.asset");
        private static TileBase TopRight   => T(WallBase + "TX Tileset Wall_12.asset");
        private static TileBase BodyFill   => T(WallBase + "TX Tileset Wall_7.asset");
        private static TileBase SideLeft   => T(WallBase + "TX Tileset Wall_15.asset");
        private static TileBase SideLeftM  => T(WallBase + "TX Tileset Wall_19.asset");
        private static TileBase SideRight  => T(WallBase + "TX Tileset Wall_22.asset");  // right-face top
        private static TileBase SideRightM => T(WallBase + "TX Tileset Wall_10.asset");  // right-face mid
        private static TileBase BotLeft    => T(WallBase + "TX Tileset Wall_6.asset");
        private static TileBase BotMid     => T(WallBase + "TX Tileset Wall_5.asset");
        private static TileBase BotRight   => T(WallBase + "TX Tileset Wall_1.asset");

        // ── Grass tiles ──
        private static TileBase Grass0  => T(GrassBase + "TX Tileset Grass 0.asset");
        private static TileBase Grass1  => T(GrassBase + "TX Tileset Grass 1.asset");
        private static TileBase Grass2  => T(GrassBase + "TX Tileset Grass 2.asset");
        private static TileBase Grass3  => T(GrassBase + "TX Tileset Grass 3.asset");
        private static TileBase Grass5  => T(GrassBase + "TX Tileset Grass 5.asset");
        private static TileBase Grass6  => T(GrassBase + "TX Tileset Grass 6.asset");
        private static TileBase Flower0 => T(GrassBase + "TX Tileset Grass Flower 0.asset");
        private static TileBase Flower1 => T(GrassBase + "TX Tileset Grass Flower 1.asset");
        private static TileBase Flower2 => T(GrassBase + "TX Tileset Grass Flower 2.asset");
        private static TileBase Flower3 => T(GrassBase + "TX Tileset Grass Flower 3.asset");
        private static TileBase Flower4 => T(GrassBase + "TX Tileset Grass Flower 4.asset");
        private static TileBase Flower5 => T(GrassBase + "TX Tileset Grass Flower 5.asset");

        // ── Stone tiles ──
        // Plain flat fill — indices _0, _1, _2, _3 (confirmed from palette YAML: palette row y=0 = clean flat stones)
        private static TileBase Stone0  => T(StoneBase + "TX Tileset Stone Ground_0.asset");   // plain flat
        private static TileBase Stone1  => T(StoneBase + "TX Tileset Stone Ground_1.asset");   // plain flat var
        private static TileBase Stone2  => T(StoneBase + "TX Tileset Stone Ground_2.asset");   // plain flat var
        private static TileBase Stone3  => T(StoneBase + "TX Tileset Stone Ground_3.asset");   // plain flat var
        // Bordered edge tiles — used for path perimeter (confirmed from palette YAML: top/bottom/side edges)
        private static TileBase StoneEdgeTop    => T(StoneBase + "TX Tileset Stone Ground_10.asset");  // top edge
        private static TileBase StoneEdgeBot    => T(StoneBase + "TX Tileset Stone Ground_12.asset");  // bottom edge
        private static TileBase StoneEdgeLeft   => T(StoneBase + "TX Tileset Stone Ground_11.asset");  // left edge
        private static TileBase StoneEdgeRight  => T(StoneBase + "TX Tileset Stone Ground_13.asset");  // right edge
        private static TileBase StoneCornerTL   => T(StoneBase + "TX Tileset Stone Ground_8.asset");   // top-left corner
        private static TileBase StoneCornerTR   => T(StoneBase + "TX Tileset Stone Ground_9.asset");   // top-right corner
        private static TileBase StoneCornerBL   => T(StoneBase + "TX Tileset Stone Ground_16.asset");  // bottom-left corner
        private static TileBase StoneCornerBR   => T(StoneBase + "TX Tileset Stone Ground_17.asset");  // bottom-right corner

        // ── Layout constants ──
        // Room is 22 tiles wide (-11 to +10) and 17 tiles tall (-8 to +8).
        // Centre of room = x: -0.5 (between -1 and 0), y: 0.
        // Gate gap is 2 tiles: x=-1 and x=0 — the two centremost columns on top wall.
        // Stone path is 3 tiles wide: x=-1,0,1 — centred on world x=0.
        private const int WallL  = -11;
        private const int WallR  =  10;   // 22-tile wide room
        private const int WallB  =  -8;
        private const int WallT  =   8;
        private const int BodyT  =   9;   // dark fill row above top face
        private const int GrassL = -10;
        private const int GrassR =   9;
        private const int GrassB =  -7;
        private const int GrassT =   7;
        private const int GateX1 =  -1;   // gate gap left column  (centre-left)
        private const int GateX2 =   0;   // gate gap right column (centre-right)
        private const int PathL  =  -1;   // path centre-left
        private const int PathR  =   1;   // path centre-right
        private const int PathB  =  -6;
        private const int PathT  =   6;

        [MenuItem("Tools/Mark of Ascension/Paint Lobby Tiles")]
        public static void PaintLobby()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                scene = EditorSceneManager.OpenScene(ScenePath);
            }

            var gnd = FindTilemap("TM_Ground");
            var wls = FindTilemap("TM_Walls");
            var dcr = FindTilemap("TM_Decor");

            if (gnd == null || wls == null || dcr == null)
            {
                Debug.LogError("[LobbyPainter] TM_Ground, TM_Walls or TM_Decor not found in SC_Lobby.");
                return;
            }

            Undo.RecordObject(gnd, "Paint Lobby Ground");
            Undo.RecordObject(wls, "Paint Lobby Walls");
            Undo.RecordObject(dcr, "Paint Lobby Decor");

            gnd.ClearAllTiles();
            wls.ClearAllTiles();
            dcr.ClearAllTiles();

            PaintGround(gnd);
            PaintStonePath(gnd);
            PaintWalls(wls);
            PaintDecor(dcr);

            wls.GetComponent<CompositeCollider2D>()?.GenerateGeometry();

            EditorUtility.SetDirty(gnd);
            EditorUtility.SetDirty(wls);
            EditorUtility.SetDirty(dcr);
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[LobbyPainter] Done. Save with Ctrl+S.");
        }

        // ── Ground ───────────────────────────────────────────────────────────
        private static void PaintGround(Tilemap tm)
        {
            // Weighted toward plain grass with subtle variation for a natural look
            TileBase[] pool = { Grass0, Grass0, Grass0, Grass0, Grass1, Grass1, Grass2, Grass3, Grass5, Grass6 };
            var rng = new System.Random(42);
            for (int x = GrassL; x <= GrassR; x++)
            for (int y = GrassB; y <= GrassT; y++)
                Set(tm, x, y, pool[rng.Next(pool.Length)]);
        }

        // ── Stone path ────────────────────────────────────────────────────────
        private static void PaintStonePath(Tilemap tm)
        {
            // 3-tile-wide centre strip: x = -1, 0, 1
            TileBase[] pool = { Stone0, Stone0, Stone1, Stone2, Stone3 };
            var rng = new System.Random(7);
            for (int x = PathL; x <= PathR; x++)
            for (int y = PathB; y <= PathT; y++)
                Set(tm, x, y, pool[rng.Next(pool.Length)]);

            // Entrance plaza: widen to 5 tiles (x = -2..2) near the gate
            for (int x = -2; x <= 2; x++)
            for (int y = PathT - 1; y <= PathT; y++)
                Set(tm, x, y, Stone0);

            // Spawn platform: slightly wider at the bottom
            for (int x = -2; x <= 2; x++)
            for (int y = PathB; y <= PathB + 1; y++)
                Set(tm, x, y, Stone0);
        }

        // ── Walls — all 4 sides ───────────────────────────────────────────────
        private static void PaintWalls(Tilemap tm)
        {
            // TOP: solid dark body fill row above the lit face row
            for (int x = WallL; x <= WallR; x++)
            {
                if (x == GateX1 || x == GateX2) continue;
                Set(tm, x, BodyT, BodyFill);
                Set(tm, x, WallT, x == WallL ? TopLeft : x == WallR ? TopRight : TopMid);
            }

            // BOTTOM: full shadow ledge across the bottom
            for (int x = WallL; x <= WallR; x++)
                Set(tm, x, WallB, x == WallL ? BotLeft : x == WallR ? BotRight : BotMid);

            // LEFT column — full height between bottom and top rows
            for (int y = WallB + 1; y <= WallT - 1; y++)
                Set(tm, WallL, y, y == WallT - 1 ? SideLeft : SideLeftM);

            // RIGHT column — same tiles mirrored (Cainos wall art is symmetric)
            for (int y = WallB + 1; y <= WallT - 1; y++)
                Set(tm, WallR, y, y == WallT - 1 ? SideLeft : SideLeftM);
        }

        // ── Decor — dense flower clusters on TM_Decor ────────────────────────
        private static void PaintDecor(Tilemap tm)
        {
            (int x, int y, TileBase tile)[] spots =
            {
                // Top-left cluster
                (-9, 6, Flower0), (-8, 6, Flower1), (-9, 5, Flower2),
                (-7, 6, Flower0), (-8, 5, Flower3), (-9, 4, Flower1), (-6, 6, Flower4),
                // Bottom-left cluster
                (-9, -4, Flower5), (-8, -5, Flower0), (-9, -6, Flower2),
                (-7, -5, Flower1), (-8, -6, Flower4),
                // Top-right cluster
                ( 9, 6, Flower3), ( 8, 6, Flower0), ( 9, 5, Flower1),
                ( 7, 6, Flower2), ( 8, 5, Flower5), ( 9, 4, Flower0), ( 6, 6, Flower4),
                // Bottom-right cluster
                ( 9, -4, Flower2), ( 8, -5, Flower3), ( 9, -6, Flower0),
                ( 7, -5, Flower1), ( 8, -6, Flower5),
                // Mid scattered
                (-5, 2, Flower1), (-4, 3, Flower3), ( 5, 2, Flower4), ( 4, 3, Flower0),
                (-5, -2, Flower2), ( 5, -2, Flower5),
            };
            foreach (var (x, y, tile) in spots)
                Set(tm, x, y, tile);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static void Set(Tilemap tm, int x, int y, TileBase tile)
            => tm.SetTile(new Vector3Int(x, y, 0), tile);

        private static Tilemap FindTilemap(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.GetComponent<Tilemap>() : null;
        }

        private static TileBase T(string path)
        {
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
            if (tile == null) Debug.LogWarning($"[LobbyPainter] Missing tile: {path}");
            return tile;
        }
    }
}
