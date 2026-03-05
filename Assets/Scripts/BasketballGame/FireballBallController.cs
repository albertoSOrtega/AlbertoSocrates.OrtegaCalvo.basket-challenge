using UnityEngine;

public class FireballBallController : MonoBehaviour
{
    [Header("Fireball Child Effects")]
    [SerializeField] private GameObject cfxrSun;
    [SerializeField] private GameObject cfxrFire;

    public void ActivateEffects()
    {
        SetEffect(cfxrSun, true);
        SetEffect(cfxrFire, true);
    }

    public void DeactivateEffects()
    {
        SetEffect(cfxrSun, false);
        SetEffect(cfxrFire, false);
    }

    private void SetEffect(GameObject effect, bool active)
    {
        if (effect == null) return;

        effect.SetActive(active);

        if (active)
        {
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
            }
        }
    }
}