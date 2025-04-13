using UnityEngine;

public class CheckPoint : InteractibleObject
{
    [SerializeField] private Transform newCheckpointPos;

    [SerializeField] private ParticleSystem particles;

    [SerializeField] private GameObject glow;
    private bool isActivated = false;

    private void Start()
    {
        glow.SetActive(false);
        particles.Pause();
    }

    private void FixedUpdate()
    {
        ControlEmmision();
    }

    private void ControlEmmision()
    {
        var emission = particles.emission;

        if (isReadyForInteracting)
            emission.enabled = true;
        else
            emission.enabled = false;
    }

    public override void Interact()
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
