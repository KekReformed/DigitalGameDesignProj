using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AveragePixels : MonoBehaviour
{

    private Tilemap tilemap;
    public Tile testTile;
    BoundsInt bounds;

    [ContextMenu("Set tiles to map tiles (will break things)")]
    public void SetMapTiles()
    {
        tilemap = GetComponent<Tilemap>();
        bounds = tilemap.cellBounds;
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                for (int z = bounds.min.z; z < bounds.max.z; z++)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, z);
                    Tile tile = tilemap.GetTile<Tile>(tilePos);

                    if (tile == null) continue;
                    if (tile.sprite == null) continue;
                    
                    string assetPath = AssetDatabase.GetAssetPath(tile.sprite.texture);

                    Sprite sprite = Sprite.Create(AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath.Split(".")[0] + "_map" + "." + assetPath.Split(".")[1]), tile.sprite.rect, new Vector2(0.5f, 0.5f), 32);
                    Tile tempTile = testTile;
                    tempTile.sprite = sprite;
                    tilemap.SetTile(tilePos, tempTile);
                }
            }
        }
    }
}
