using Unity.Netcode.Components;

namespace ArcadeShooter.Networking
{
    // The CharacterController moves immediately on its owner; NGO interpolates copies.
    public sealed class OwnerNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
