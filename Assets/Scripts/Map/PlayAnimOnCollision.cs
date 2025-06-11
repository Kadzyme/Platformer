using UnityEngine;

public class PlayAnimOnCollision : MonoBehaviour
{
    [SerializeField] private string animName = "Attack";

    [SerializeField] private float cooldown = 2f;
    private float currentCooldown = 2f;

    private Animator animator;

    private void Start()
        => animator = GetComponent<Animator>();

    private void FixedUpdate()
        => currentCooldown -= Time.fixedDeltaTime;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentCooldown < 0)
        {
            animator.SetTrigger(animName);
            currentCooldown = cooldown;
        }
    }
}
