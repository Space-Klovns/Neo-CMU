using Content.Server.NPC.Queries.Considerations;

namespace Content.Server._KS14.NPC.Queries.Considerations;

/// <summary>
///     Returns 1 if the target key coordinates (if any) are in LOS of origin key coordinates (if any).
///         Otherwise, returns 0.
/// </summary>
public sealed partial class CoordinatesInLOSCon : UtilityConsideration
{
    /// <summary>
    ///     Coordinates that must be visible by the target for this to be valid.
    /// </summary>
    [DataField(required: true)] public string ToKey;
    [DataField(required: true)] public float Radius;

}
