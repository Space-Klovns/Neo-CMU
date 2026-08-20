using Content.Server.NPC.Queries.Considerations;

namespace Content.Server._KS14.NPC.Queries.Considerations;

/// <summary>
///     Assumes a field of view starting from the owner
///         and pointed in the direction of the reference coordinates.
///
///     For targets that are in this FOV (given <see cref="Angle"/>),
///         returns 1f. Otherwise, returns 0f.
/// </summary>
public sealed partial class CoordinatesInFOVCon : UtilityConsideration
{
    /// <summary>
    ///     Second set of coordinates, compared with owner coordinates, to determine
    ///         the direction of the FOV.
    /// </summary>
    [DataField("toKey", required: true)] public string ReferenceCoordinatesKey = default!;

    /// <summary>
    ///     Permitting angle, in degrees.
    /// </summary>
    [DataField(required: true)] public float Angle;

}
