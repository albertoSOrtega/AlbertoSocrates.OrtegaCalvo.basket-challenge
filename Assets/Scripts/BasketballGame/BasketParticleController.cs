using UnityEngine;

public class BasketParticleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameController gameController;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem particlePerfect;
    [SerializeField] private ParticleSystem particle2Points;
    [SerializeField] private ParticleSystem particleBonus;

    private void OnEnable()
    {
        gameController.OnPlayerScored += HandlePlayerScored;
        gameController.OnCPUScored += HandleCPUScored;
    }

    private void OnDisable()
    {
        gameController.OnPlayerScored -= HandlePlayerScored;
        gameController.OnCPUScored -= HandleCPUScored;
    }

    private void HandlePlayerScored(ShotType shotType, int points)
    {
        switch (shotType)
        {
            case ShotType.Perfect:
                PlayParticle(particlePerfect);
                break;

            case ShotType.Imperfect:
                PlayParticle(particle2Points);
                break;

            case ShotType.PerfectBackboard:
                bool hasBonus = points > 2;
                PlayParticle(hasBonus ? particleBonus : particle2Points);
                break;
        }
    }

    private void HandleCPUScored()
    {
        // always play 2 points particle when CPU scores
        PlayParticle(particle2Points);
    }

    private void PlayParticle(ParticleSystem ps)
    {
        if (ps == null) return;

        ps.gameObject.SetActive(true);  // <- reactiva si estaba desactivado
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();
    }
}