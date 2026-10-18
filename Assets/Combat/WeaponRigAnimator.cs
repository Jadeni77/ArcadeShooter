using UnityEngine;

namespace ArcadeShooter.Combat
{
    // Optional adapter for an imported rigged rifle and hands. No gameplay state lives here.
    public sealed class WeaponRigAnimator : MonoBehaviour
    {
        [SerializeField] private AutomaticRifle rifle;
        [SerializeField] private WeaponAim aim;
        [SerializeField] private Animator animator;

        private static readonly int Fire = Animator.StringToHash("Fire");
        private static readonly int Reloading = Animator.StringToHash("Reloading");
        private static readonly int ReloadProgress = Animator.StringToHash("ReloadProgress");
        private static readonly int Aim = Animator.StringToHash("Aim");

        private void Awake()
        {
            if (rifle == null) rifle = GetComponentInParent<AutomaticRifle>();
            if (aim == null) aim = GetComponentInParent<WeaponAim>();
        }

        private void OnEnable()
        {
            if (rifle != null) rifle.Fired += OnFired;
        }

        private void OnDisable()
        {
            if (rifle != null) rifle.Fired -= OnFired;
        }

        private void Update()
        {
            if (animator == null || rifle == null) return;
            animator.SetBool(Reloading, rifle.IsReloading);
            animator.SetFloat(ReloadProgress, rifle.ReloadProgress);
            animator.SetFloat(Aim, aim != null ? aim.Amount : 0f);
        }

        private void OnFired()
        {
            if (animator != null) animator.SetTrigger(Fire);
        }
    }
}
