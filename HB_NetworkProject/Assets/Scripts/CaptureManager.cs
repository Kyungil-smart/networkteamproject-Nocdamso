using UnityEngine;

public class CaptureManager : MonoBehaviour
{
    public void Capture(ChessPieceManager target)
    {
        if (target == null)
        {
            return;
        }

        Destroy(target.gameObject);
    }
}
