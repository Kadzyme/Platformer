using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KillPlane : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.isTrigger)
            Destroy(collision.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;

        if (GetComponent<BoxCollider2D>() == null)
            return;

        var box = GetComponent<BoxCollider2D>().size;
        box *= transform.localScale;
        Vector2 center = transform.position;
        center += GetComponent<BoxCollider2D>().offset * transform.localScale;
        Gizmos.DrawWireCube(center, box);
    }
}
