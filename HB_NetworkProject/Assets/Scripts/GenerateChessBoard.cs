using UnityEngine;

public class GenerateChessBoard : MonoBehaviour
{
    [Header("체스 타일")]
    public GameObject DarkTile;
    public GameObject LightTile;
    public float TileSize = 2.3f;
    public float TileScale = 1.0f;

    private void Start()
    {
        GenerateBoard();
    }

    [ContextMenu("Generate Map")]
    private void GenerateBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                GameObject spawnTile = (x + z) % 2 == 0 ? DarkTile : LightTile;

                Vector3 position = new Vector3(x * TileSize, 1, z * TileSize);

                GameObject tile = Instantiate(spawnTile, position, Quaternion.identity);

                tile.transform.localScale = new Vector3(TileScale, TileScale, TileScale);

                tile.transform.parent = this.transform;
            }
        }
    }
}
