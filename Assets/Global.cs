using UnityEngine;

public class Global : MonoBehaviour
{
    public static int groundLayer;
    [SerializeField] private int groundLayerNum;

    void Awake()
    {
        groundLayer = 1 << groundLayerNum;
    }
}
