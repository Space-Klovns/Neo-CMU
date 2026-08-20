using Content.Server.NPC.Queries.Considerations;

namespace Content.Server._KS14.NPC.Queries.Considerations;

/// <summary>
///     Scores entities based on their distance from
///         the given coordinates key.
/// </summary>
public sealed partial class KeyCoordinatesDistanceCon : UtilityConsideration
{
    /// <summary>
    ///     Key of the coordinates to get distance from.
    /// </summary>
    [DataField(required: true)] public string Key = "TargetCoordinates";

}
