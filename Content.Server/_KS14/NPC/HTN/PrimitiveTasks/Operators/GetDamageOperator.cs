using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Damage;

namespace Content.Server._KS14.NPC.HTN.PrimitiveTasks.Operators;

/// <summary>
///     Sets the value of the target key to the health of the NPC.
/// </summary>
public sealed partial class GetDamageOperator : HTNOperator
{
    [Dependency] private IEntityManager _entityManager = default!;

    [DataField(required: true)] public string Key = "Damage";

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard, CancellationToken _) => (true, new Dictionary<string, object>()
        {
            {Key, (float)_entityManager.GetComponent<DamageableComponent>(blackboard.GetValue<EntityUid>(NPCBlackboard.Owner)).TotalDamage}
        });
}
