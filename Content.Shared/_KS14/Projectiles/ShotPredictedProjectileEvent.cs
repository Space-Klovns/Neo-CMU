using Robust.Shared.Serialization;

namespace Content.Shared._KS14.Projectiles;

/// <summary>
/// Event sent to the client that shot a predicted projectile.
/// Used to hide the server-spawned one, since the shooter's client already has its own
/// locally-predicted copy of the same shot.
/// </summary>
[Serializable, NetSerializable]
public sealed class ShotPredictedProjectileEvent(NetEntity projectile) : EntityEventArgs
{
    public NetEntity Projectile = projectile;
}
