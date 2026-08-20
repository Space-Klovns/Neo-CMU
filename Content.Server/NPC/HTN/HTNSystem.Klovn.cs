using Content.Server._KS14.NPC;

namespace Content.Server.NPC.HTN;

public sealed partial class HTNSystem
{
    public bool AttemptWork(EntityUid uid)
    {
        var ev = new AttemptNpcWorkEvent();
        RaiseLocalEvent(uid, ref ev);

        return !ev.Cancelled;
    }
}
