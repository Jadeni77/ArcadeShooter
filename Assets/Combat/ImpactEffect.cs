using UnityEngine;

namespace ArcadeShooter.Combat
{
    public sealed class ImpactEffect : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float lifetime = 0.25f;
        private float initialScale;
        private float expiresAt;

        private void OnEnable()
        {
            initialScale = transform.localScale.x;
            expiresAt = Time.time + lifetime;
        }

        private void Update()
        {
            float remaining = (expiresAt - Time.time) / lifetime;
            if (remaining <= 0f)
            {
                Destroy(gameObject);
                return;
            }
            transform.localScale = Vector3.one * (initialScale * remaining);
        }
    }
}
