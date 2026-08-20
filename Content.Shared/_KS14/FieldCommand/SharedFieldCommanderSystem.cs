// SPDX-License-Identifier: MPL-2.0

using System.Numerics;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Shared._KS14.FieldCommand;

/// <summary>
/// Predicted RTS selection and formation-order logic. The authoritative server implementation
/// overrides <see cref="IssueMoveOrder"/> to inject destinations into HTN blackboards.
/// </summary>
public abstract partial class SharedFieldCommanderSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FieldCommanderComponent, FieldCommanderToggleSelectionEvent>(OnToggleSelection);
        SubscribeLocalEvent<FieldCommanderComponent, FieldCommanderClearSelectionEvent>(OnClearSelection);
        SubscribeLocalEvent<FieldCommanderComponent, FieldCommanderMoveOrderEvent>(OnMoveOrder);
    }

    protected virtual void OnToggleSelection(Entity<FieldCommanderComponent> commander, ref FieldCommanderToggleSelectionEvent args)
    {
        args.Handled = true;
        args.Toggle = true;
    }

    private void OnClearSelection(Entity<FieldCommanderComponent> commander, ref FieldCommanderClearSelectionEvent args)
    {
        if (args.Handled)
            return;

        commander.Comp.SelectedUnits.Clear();
        Dirty(commander);
        args.Handled = true;
    }

    /// <summary>Applies a completed drag rectangle. Called predictively on the client and authoritatively on the server.</summary>
    public void SelectUnits(Entity<FieldCommanderComponent> commander, EntityCoordinates start, EntityCoordinates end)
    {
        commander.Comp.SelectedUnits.Clear();
        var startMap = _transform.ToMapCoordinates(start);
        var endMap = _transform.ToMapCoordinates(end);
        if (startMap.MapId != endMap.MapId)
            return;

        var min = Vector2.Min(startMap.Position, endMap.Position);
        var max = Vector2.Max(startMap.Position, endMap.Position);
        var query = EntityQueryEnumerator<FieldCommandableComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            var coordinates = _transform.ToMapCoordinates(xform.Coordinates);
            if (coordinates.MapId != startMap.MapId ||
                coordinates.Position.X < min.X || coordinates.Position.X > max.X ||
                coordinates.Position.Y < min.Y || coordinates.Position.Y > max.Y)
                continue;

            commander.Comp.SelectedUnits.Add(uid);
        }

        Dirty(commander);
    }

    private void OnMoveOrder(Entity<FieldCommanderComponent> commander, ref FieldCommanderMoveOrderEvent args)
    {
        if (commander.Comp.SelectedUnits.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("field-commander-no-selection"), commander, args.Performer);
            return;
        }

        IssueMoveOrder(commander, args.Target);
    }

    protected virtual void IssueMoveOrder(Entity<FieldCommanderComponent> commander, EntityCoordinates target) { }
}
