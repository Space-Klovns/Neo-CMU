using Content.Shared._RMC14.CCVar;
using Robust.Shared.Configuration;

namespace Content.Shared._RMC14.Weapons.Ranged.Prediction;

/// <summary>
/// Global "is gun/projectile prediction enabled" toggle. Guns themselves resolve their shot
/// directly through <see cref="Content.Shared.Weapons.Ranged.Systems.SharedGunSystem"/> now
/// (see KS14's predicted-gun port), but Xeno projectiles (<see cref="Content.Shared._RMC14.Xenonids.Projectile.XenoProjectileSystem"/>)
/// still have their own separate predicted-shot correlation and consult this flag.
/// </summary>
public sealed partial class SharedGunPredictionSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _config = default!;

    public bool GunPrediction { get; private set; }

    public override void Initialize()
    {
        Subs.CVar(_config, RMCCVars.RMCGunPrediction, v => GunPrediction = v, true);
    }
}
