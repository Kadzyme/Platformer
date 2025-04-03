using UnityEngine;

public class Global : MonoBehaviour
{
    public static int groundLayer;
    [SerializeField] private int groundLayerNum;

    public static Transform currentPlayer;
    [SerializeField] private Transform startPlayer;

    void Awake()
    {
        groundLayer = 1 << groundLayerNum;
        currentPlayer = startPlayer;
    }
}
