using UnityEngine;

public class EnableObject : MonoBehaviour
{
    [SerializeField] private GameObject obj;

    public void Enable()
        => obj.SetActive(true);

    public void Disable()
        => obj.SetActive(false);
}
