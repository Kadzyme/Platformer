using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GroundSensor : MonoBehaviour
{
    private float currentDisableTime = 0;
    private List<Collider2D> colliders = new();

    private void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
        Disable(0.1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CanCountCollider(collision))
            return;

        colliders.Add(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!CanCountCollider(collision))
            return;

        colliders.Remove(collision);
    }

    private bool CanCountCollider(Collider2D collision)
        => !collision.isTrigger && ((1 << collision.gameObject.layer) & Global.groundLayer) != 0;

    private void Update()
    {
        currentDisableTime -= Time.deltaTime;

        if (currentDisableTime < 0)
            GetComponent<Collider2D>().enabled = true;

        for (int i = colliders.Count - 1; i >= 0; i--)
        {
            if (colliders[i] == null)
            {
                colliders.RemoveAt(i);
            }
        }
    }

    public void Disable(float time)
    {
        currentDisableTime = time;
        colliders.Clear();
        GetComponent<Collider2D>().enabled = false;
    }

    public bool State()
    {
        if (currentDisableTime > 0)
            return false;

        return colliders.Count > 0;
    }
}
