using Content.Server.NPC.Queries.Considerations;

namespace Content.Server._KS14.NPC.Queries.Considerations;

/// <summary>
///     Returns 1 if the random prob is true.
///         Otherwise, returns 0.
/// </summary>
public sealed partial class RandomValueCon : UtilityConsideration
{
    /// <summary>
    ///     Probability (in percent; 0 - 1) for score to be true.
    /// </summary>
    [DataField(required: true)] public float Probability;

}
