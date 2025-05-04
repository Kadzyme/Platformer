using UnityEngine;

public class ParticlesOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject particles;

    private void OnDestroy()
    {
        var obj = Instantiate(particles, transform.position, Quaternion.identity);
        Destroy(obj, 10f);
    }
}
