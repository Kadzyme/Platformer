using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public abstract class InteractibleObject : MonoBehaviour
{
    public bool isReadyForInteracting = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            isReadyForInteracting = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            isReadyForInteracting = false;
    }

    private void Update()
    {
        if (isReadyForInteracting && Global.interactAction.action.WasPressedThisFrame())
            Interact();
    }

    public abstract void Interact();
}
