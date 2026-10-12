using UnityEngine;

namespace ArcadeShooter.Combat
{
    public interface IDamageable
    {
        // Returns true only when health was actually reduced.
        bool ApplyDamage(float amount, Vector3 hitPoint);
    }
}
