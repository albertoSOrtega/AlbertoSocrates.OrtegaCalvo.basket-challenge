using DG.Tweening;
using UnityEngine;

public class BackboardVisualFeedbackController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer backboardRenderer;

    [Header("Textures")]
    [SerializeField] private Texture2D normalTexture;
    [SerializeField] private Texture2D bonusTexture4;
    [SerializeField] private Texture2D bonusTexture6;
    [SerializeField] private Texture2D bonusTexture8;

    [Header("Glow Configuration")]
    [SerializeField] private Color bonusGlowColor = new Color(0f, 1f, 1f); // cyan
    [SerializeField] private float glowMinIntensity = 0.1f;
    [SerializeField] private float glowMaxIntensity = 0.5f;
    [SerializeField] private float glowPulseDuration = 0.8f;

    [Header("Hit Configuration")]
    [SerializeField] private float hitIntensity = 4f;
    [SerializeField] private float hitFadeDuration = 0.5f;

    // Cached shader property IDs - avoids string lookup every frame
    private static readonly int BaseMapID = Shader.PropertyToID("_MainTex");  // Autodesk Interactive albedo
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private Material materialInstance;
    private Tween activeTween;
    private bool isBonusActive = false;
    private float currentEmissionIntensity = 0f;

    //Singleton Pattern -> allows other scripts (like BackboardCollisionController) to easily trigger visual feedback without
    //needing a direct reference to this component
    public static BackboardVisualFeedbackController instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        materialInstance = backboardRenderer.material;
        // Force enable emission on the material instance - Autodesk Interactive requires this explicitly
        materialInstance.EnableKeyword("_EMISSION");
        materialInstance.SetColor(EmissionColorID, Color.black); // starts off, no glow
    }

    public void StartBonusGlow(int points)
    {
        isBonusActive = true;
        materialInstance.EnableKeyword("_EMISSION");
        SwapToBonus(points);
        StartPulsingGlow();
    }

    // Ball hits the backboard during bonus - spike intensity then return to pulse
    public void TriggerHitFlash()
    {
        if (!isBonusActive) return;

        // Kill whatever is running - pulse or previous spike
        activeTween?.Kill();

        // Save spike in activeTween so it can always be killed
        activeTween = DOVirtual.Float(hitIntensity, glowMaxIntensity, hitFadeDuration, SetEmissionIntensity)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            if (isBonusActive) StartPulsingGlow();
        });
    }

    public void StopBonusGlow()
    {
        isBonusActive = false;
        activeTween?.Kill();
        activeTween = null;
        RestoreNormal();
    }

    private void SwapToBonus(int points)
    {
        Texture2D target;

        switch (points)
        {
            case 2: target = bonusTexture4; break;
            case 4: target = bonusTexture6; break;
            case 6: target = bonusTexture8; break;
            default: target = bonusTexture4; break;
        }

        materialInstance.SetTexture(BaseMapID, target);
    }

    private void StartPulsingGlow()
    {
        activeTween?.Kill();

        activeTween = DOVirtual.Float(glowMinIntensity, glowMaxIntensity, glowPulseDuration, SetEmissionIntensity)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void SetEmissionIntensity(float intensity)
    {
        currentEmissionIntensity = intensity; // track current value
        materialInstance.SetColor(EmissionColorID, bonusGlowColor * intensity);
    }

    private void RestoreNormal()
    {
        activeTween?.Kill();
        currentEmissionIntensity = 0f; 
        materialInstance.SetColor(EmissionColorID, Color.black);
        materialInstance.SetTexture(BaseMapID, normalTexture);
        materialInstance.DisableKeyword("_EMISSION");
        materialInstance.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
    }
}