using TMPro;
using UnityEngine;

namespace ArcadeShooter.UI
{
    public sealed class WeaponHud : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private UnityEngine.UI.Image hitMarker;
        [SerializeField, Min(0f)] private float hitMarkerDuration = 0.12f;
        private float hitMarkerUntil;
        private Color normalHitColor;

        private void Awake()
        {
            if (hitMarker != null) normalHitColor = hitMarker.color;
        }

        public void SetAmmo(int magazine, int reserve, bool reloading)
        {
            if (ammoText != null)
                ammoText.text = reloading ? $"RELOADING  {magazine} / {reserve}" : $"{magazine} / {reserve}";
        }

        public void ConfirmHit(bool killed = false)
        {
            if (hitMarker == null)
                return;
            hitMarker.color = killed ? new Color(1f, 0.72f, 0.2f, normalHitColor.a) : normalHitColor;
            hitMarker.enabled = true;
            hitMarkerUntil = Time.time + hitMarkerDuration;
        }

        private void Update()
        {
            if (hitMarker != null && hitMarker.enabled && Time.time >= hitMarkerUntil)
                hitMarker.enabled = false;
        }
    }
}
