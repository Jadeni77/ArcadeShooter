using UnityEngine;

namespace ArcadeShooter.Combat
{
    // Optional explicit override for surfaces whose material name is not descriptive.
    public sealed class SurfaceFeedback : MonoBehaviour
    {
        [SerializeField] private ImpactSurface surface = ImpactSurface.Concrete;
        public ImpactSurface Surface => surface;
    }
}
