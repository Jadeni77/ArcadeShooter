using UnityEngine;

namespace ArcadeShooter.Enemies
{
    // Only changes presentation; damage and attack timing remain in their own components.
    [RequireComponent(typeof(EnemyHealth), typeof(MeleeEnemy))]
    public sealed class EnemyFeedback : MonoBehaviour
    {
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Color damageColor = new Color(1f, 0.2f, 0.12f);
        [SerializeField] private Color attackColor = new Color(1f, 0.7f, 0.12f);
        [SerializeField] private Color deadColor = new Color(0.24f, 0.23f, 0.25f);
        [SerializeField, Min(0f)] private float flashDuration = 0.18f;

        private EnemyHealth health;
        private MeleeEnemy enemy;
        private MaterialPropertyBlock block;
        private float flashUntil;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            enemy = GetComponent<MeleeEnemy>();
            if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<Renderer>();
            block = new MaterialPropertyBlock();
            health.Damaged += FlashDamage;
            health.Died += ShowDeath;
            enemy.Attacked += FlashAttack;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Damaged -= FlashDamage;
                health.Died -= ShowDeath;
            }
            if (enemy != null) enemy.Attacked -= FlashAttack;
        }

        private void Update()
        {
            if (flashUntil > 0f && Time.time >= flashUntil && !health.IsDead)
            {
                flashUntil = 0f;
                bodyRenderer?.SetPropertyBlock(null);
            }
        }

        private void FlashDamage() => Flash(damageColor);
        private void FlashAttack() => Flash(attackColor);

        private void Flash(Color color)
        {
            if (health.IsDead || bodyRenderer == null) return;
            block.SetColor("_BaseColor", color);
            bodyRenderer.SetPropertyBlock(block);
            flashUntil = Time.time + flashDuration;
        }

        private void ShowDeath()
        {
            if (bodyRenderer == null) return;
            block.SetColor("_BaseColor", deadColor);
            bodyRenderer.SetPropertyBlock(block);
            flashUntil = 0f;
        }
    }
}
