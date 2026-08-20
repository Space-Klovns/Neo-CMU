using Content.Server.Hands.Systems;
using Content.Server.Interaction;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.CombatMode;

namespace Content.Server._KS14.NPC.HTN.PrimitiveTasks.Operators;

/// <summary>
/// Uses the active-hand item on the target entity.
/// </summary>
public sealed partial class InteractUsingOperator : HTNOperator
{
    [Dependency] private IEntityManager _entManager = default!;

    [DataField(required: true)] public string TargetKey = string.Empty;

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var ownerUid = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var hands = _entManager.System<HandsSystem>();
        if (!hands.TryGetActiveItem(ownerUid, out var itemUid))
            return HTNOperatorStatus.Failed;

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var targetUid, _entManager) ||
            !_entManager.TryGetComponent(targetUid, out TransformComponent? targetTransform))
        {
            return HTNOperatorStatus.Failed;
        }

        if (_entManager.TryGetComponent<CombatModeComponent>(ownerUid, out var combatMode))
            _entManager.System<SharedCombatModeSystem>().SetInCombatMode(ownerUid, false, combatMode);

        _entManager.System<InteractionSystem>().InteractUsing(ownerUid, itemUid.Value, targetUid, targetTransform.Coordinates);
        return HTNOperatorStatus.Finished;
    }
}