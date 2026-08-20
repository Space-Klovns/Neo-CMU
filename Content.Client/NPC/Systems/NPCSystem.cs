using Content.Client.NPC.HTN;

namespace Content.Client.NPC.Systems;

/// <summary>
/// Client-side NPC query helper. The older shared base no longer exposes this API,
/// so the helper remains local to the client.
/// </summary>
public sealed partial class NPCSystem : EntitySystem
{
    public bool IsNpc(EntityUid uid)
    {
        return HasComp<HTNComponent>(uid);
    }
}
