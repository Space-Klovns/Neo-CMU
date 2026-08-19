namespace Content.Shared._KS14.Projectiles;

/// <summary>
/// Event broadcast when a projectile is shot with a non-null user.
/// Ported from Klovnstation14's traumastation-derived predicted-gun model.
/// </summary>
[ByRefEvent]
public record struct PlayerShotProjectileEvent(EntityUid Projectile, EntityUid User);
