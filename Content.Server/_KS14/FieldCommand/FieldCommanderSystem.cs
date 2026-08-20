// SPDX-License-Identifier: MPL-2.0

using System.Numerics;
using System.Linq;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared._KS14.FieldCommand;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Server._KS14.FieldCommand;

/// <summary>Authoritative half of the predicted Field Commander command system.</summary>
public sealed partial class FieldCommanderSystem : SharedFieldCommanderSystem
{
    [Dependency] private NPCSystem _npc = default!;
    [Dependency] private HTNSystem _htn = default!;
    [Dependency] private SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<FieldCommanderSelectRectangleEvent>(OnSelectRectangle);
    }

    private void OnSelectRectangle(FieldCommanderSelectRectangleEvent args, EntitySessionEventArgs session)
    {
        var commander = GetEntity(args.Commander);
        if (session.SenderSession.AttachedEntity != commander ||
            !TryComp<FieldCommanderComponent>(commander, out var component))
            return;

        SelectUnits((commander, component), GetCoordinates(args.Start), GetCoordinates(args.End));

        var selectionAction = GetEntity(args.SelectionAction);
        if (TryComp<ActionComponent>(selectionAction, out var action))
            _actions.SetToggled((selectionAction, action), false);
    }

    protected override void IssueMoveOrder(Entity<FieldCommanderComponent> commander, EntityCoordinates target)
    {
        var count = commander.Comp.SelectedUnits.Count;
        var columns = Math.Max(1, (int) Math.Ceiling(Math.Sqrt(count)));
        var index = 0;
        foreach (var unit in commander.Comp.SelectedUnits.ToArray())
        {
            if (!Exists(unit))
            {
                commander.Comp.SelectedUnits.Remove(unit);
                continue;
            }

            var offset = new Vector2(index % columns, index / columns) - new Vector2((columns - 1) / 2f, (columns - 1) / 2f);
            _npc.SetBlackboard(unit, NPCBlackboard.FollowTarget, new EntityCoordinates(target.EntityId, target.Position + offset));
            if (TryComp<HTNComponent>(unit, out var htn))
            {
                _npc.WakeNPC(unit, htn);
                _htn.Replan(htn);
            }
            index++;
        }
    }
}
