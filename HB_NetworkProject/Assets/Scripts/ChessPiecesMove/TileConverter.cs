using UnityEngine;

public class TileConverter : MonoBehaviour
{
    public static TileConverter Instance;

    public float TileSize = 2.3f;

    private void Awake()
    {
        Instance = this;
    }

    // 격자 좌표에 TileSize를 곱해서 기물이 이동할 수 있도록
    public Vector3 GridToWorld(int x, int z, float y = 1f)
    {
        return new Vector3(x * TileSize, y, z * TileSize);
    }

    // 
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x / TileSize), Mathf.RoundToInt(worldPos.z / TileSize));
    }
}
