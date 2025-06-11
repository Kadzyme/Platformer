using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Fan : MonoBehaviour
{
    [SerializeField] private float forceStrength = 10f;

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;

        if (rb != null)
        {
            Vector2 direction = transform.up;
            rb.AddForce(direction * forceStrength, ForceMode2D.Force);
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();

        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawCube(box.offset, box.size);

        Gizmos.matrix = oldMatrix;

        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position;
        Vector3 dir = transform.up;
        Gizmos.DrawLine(origin, origin + dir * 1f);
        Gizmos.DrawSphere(origin + dir * 1f, 0.05f);
    }
}
