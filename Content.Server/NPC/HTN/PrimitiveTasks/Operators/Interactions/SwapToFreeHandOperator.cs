using System.Threading;
using System.Threading.Tasks;
using Content.Server.Hands.Systems;
using Content.Shared.Inventory.VirtualItem;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Interactions;


/// <summary>
/// Swaps to any free hand.
/// </summary>
public sealed partial class SwapToFreeHandOperator : HTNOperator
{
    [Dependency] private IEntityManager _entManager = default!;

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard, CancellationToken cancelToken)
    {
        if (!blackboard.TryGetValue<List<string>>(NPCBlackboard.FreeHands, out var hands, _entManager))
        {
            return (false, null);
        }

        foreach (var hand in hands)
        {
            return (true, new Dictionary<string, object>()
            {
                {
                    NPCBlackboard.ActiveHand, hand
                },
                {
                    NPCBlackboard.ActiveHandFree, true
                },
            });
        }

        return (false, null);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        // Select the same hand recorded during planning so later preconditions see
        // the real active hand, not merely the planned one.
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        if (!blackboard.TryGetValue<string>(NPCBlackboard.ActiveHand, out var hand, _entManager))
            return HTNOperatorStatus.Failed;

        var handSystem = _entManager.System<HandsSystem>();
        // KS reloads deliberately select the hand occupied by a wield virtual item.
        // The following WaitTickOperator lets that queued virtual item clear before
        // the inventory magazine is equipped into this hand.
        if (handSystem.TryGetHeldItem(owner, hand, out var held) &&
            !_entManager.HasComponent<VirtualItemComponent>(held))
        {
            return HTNOperatorStatus.Failed;
        }

        return handSystem.TrySetActiveHand(owner, hand)
            ? HTNOperatorStatus.Finished
            : HTNOperatorStatus.Failed;
    }
}
