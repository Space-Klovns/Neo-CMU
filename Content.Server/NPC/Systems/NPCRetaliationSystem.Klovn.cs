using Content.Server.NPC.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Throwing;

namespace Content.Server.NPC.Systems;

// KS14 Additions

public sealed partial class NPCRetaliationSystem : EntitySystem
{
    private void InitialiseKlovn()
    {
        // This should all warrant retaliation but it didn't, so now it does
        SubscribeLocalEvent<NPCRetaliationComponent, ThrowHitByEvent>(OnThrownHit);
        SubscribeLocalEvent<NPCRetaliationComponent, ContactInteractionEvent>(KsOnContact); // KS14: ANK: contact should warrant retaliation
    }

    private void OnThrownHit(Entity<NPCRetaliationComponent> ent, ref ThrowHitByEvent args)
    {
        if (args.Component.Thrower is { } thrower)
            TryRetaliate(ent, thrower);
    }

    private void KsOnContact(Entity<NPCRetaliationComponent> ent, ref ContactInteractionEvent args)
    {
        TryRetaliate(ent, args.Other, tryWarn: true);
    }

    private void RetaliateOnThrowerIfPossible(Entity<NPCRetaliationComponent> entity, EntityUid originUid, bool tryWarn = false)
    {
        // super hardcode god
        if (TryComp<ThrownItemComponent>(originUid, out var thrownItemComponent) &&
            thrownItemComponent.Thrower is { } throwerUid)
            TryRetaliate(entity, throwerUid, tryWarn: tryWarn);
        else
            TryRetaliate(entity, originUid, tryWarn: tryWarn);
    }

}
