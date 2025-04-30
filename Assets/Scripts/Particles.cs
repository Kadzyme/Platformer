using UnityEngine;

public class Particles : MonoBehaviour
{
    private ParticleSystem currentParticleSystem;

    private void Awake()
    {
        currentParticleSystem = GetComponent<ParticleSystem>(); 
    }

    public void Play()
    {
        var pEmission = currentParticleSystem.emission;
        pEmission.enabled = true;
    }

    public void Stop()
    {
        var pEmission = currentParticleSystem.emission;
        pEmission.enabled = false;
    }
}
