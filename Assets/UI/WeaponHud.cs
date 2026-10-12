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

        public void SetAmmo(int magazine, int reserve, bool reloading)
        {
            if (ammoText != null)
                ammoText.text = reloading ? $"RELOADING  {magazine} / {reserve}" : $"{magazine} / {reserve}";
        }

        public void ConfirmHit()
        {
            if (hitMarker == null)
                return;
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
