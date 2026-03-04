using UnityEngine;

public class ParticleRotationFixer : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}