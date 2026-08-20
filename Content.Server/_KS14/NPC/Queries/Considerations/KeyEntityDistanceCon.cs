using Content.Server.NPC.Queries.Considerations;

namespace Content.Server._KS14.NPC.Queries.Considerations;

/// <summary>
///     Scores entities based on their distance from
///         the given entity key.
/// </summary>
public sealed partial class KeyEntityDistanceCon : UtilityConsideration
{
    /// <summary>
    ///     Key of the entity to get distance from.
    /// </summary>
    [DataField(required: true)] public string Key = "Target";

}
