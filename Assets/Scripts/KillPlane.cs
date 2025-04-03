using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillPlane : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(collision.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black; 
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>().size);
    }
}
