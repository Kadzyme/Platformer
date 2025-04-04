using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private Transform newCheckpointPos;

    [SerializeField] private ParticleSystem particles;

    [SerializeField] private GameObject glow;

    private bool isReadyForUse = false;
    private bool isActivated = false;

    private void Start()
    {
        glow.SetActive(false);
        particles.Pause();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            isReadyForUse = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            isReadyForUse = false;
    }

    private void Update()
    {
        var emission = particles.emission;

        if (isReadyForUse)
        {
            emission.enabled = true;

            if (Input.GetKeyDown(KeyCode.E))
            {
                Global.SetNewCheckPointPos(newCheckpointPos.position);

                if (isActivated)
                {
                    Global.OnReplaceEvent.Invoke();
                    Global.ReviveObjects();
                }
                else
                {
                    isActivated = true;
                    glow.SetActive(true);
                }
            }
        }
        else
            emission.enabled = false;
    }
}
