using UnityEngine;
public class GenerateChessBoard : MonoBehaviour
{
    [Header("체스 타일")]
    public GameObject DarkTile;
    public GameObject LightTile;
    public float TileScale = 1.15f;

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
                // 좌표의 합이 짝수면 어두운 타일, 홀수면 밝은 타일 생성
                GameObject spawnTile = (x + z) % 2 == 0 ? DarkTile : LightTile;
                
                Vector3 position = TileConverter.Instance.GridToWorld(x, z);

                GameObject tile = Instantiate(spawnTile, position, Quaternion.identity);

                // 생성된 타일 하이어라키에서 정리
                tile.transform.localScale = new Vector3(TileScale, TileScale, TileScale);
                tile.transform.parent = this.transform;
            }
        }
    }
}
