using Content.Shared._KS14.FieldCommand;
using Content.Shared.Actions;
using Content.Shared.Interaction;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Client._KS14.FieldCommand;

/// <summary>
/// Handles the local, responsive half of field-command drag selection.
/// The client selects and outlines immediately, then sends the finished rectangle
/// to the server for authoritative validation and command execution.
/// </summary>
public sealed partial class FieldCommanderSystem : SharedFieldCommanderSystem
{
    private static readonly ProtoId<ShaderPrototype> SelectionShader = "KsFieldCommanderSelectionOutline";

    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private IInputManager _input = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private IOverlayManager _overlays = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;

    private readonly HashSet<EntityUid> _outlined = [];
    private ShaderInstance _shader = default!;
    private EntityUid? _activeCommander;
    private EntityUid? _selectedCommander;
    private EntityUid? _selectionAction;
    private EntityCoordinates? _dragStart;

    internal EntityCoordinates? DragStart => _dragStart;
    internal EntityCoordinates? DragEnd { get; private set; }

    public override void Initialize()
    {
        base.Initialize();
        _shader = _prototypes.Index(SelectionShader).InstanceUnique();
        _overlays.AddOverlay(new FieldCommanderSelectionOverlay(EntityManager, this));
        SubscribeLocalEvent<FieldCommanderComponent, ComponentShutdown>(OnCommanderShutdown);
        CommandBinds.Builder
            .BindBefore(EngineKeyFunctions.Use, new PointerInputCmdHandler(OnUse, false, true), typeof(SharedInteractionSystem))
            .Register<FieldCommanderSystem>();
    }

    public override void Shutdown()
    {
        ClearOutline();
        CommandBinds.Unregister<FieldCommanderSystem>();
        base.Shutdown();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_selectedCommander is { } commander && TryComp<FieldCommanderComponent>(commander, out var component))
            UpdateOutline(component.SelectedUnits);
        else
            ClearOutline();
    }

    protected override void OnToggleSelection(Entity<FieldCommanderComponent> commander, ref FieldCommanderToggleSelectionEvent args)
    {
        base.OnToggleSelection(commander, ref args);

        if (!_timing.IsFirstTimePredicted)
            return;

        // Action state flips after event handling, so its current value is the old state.
        _selectedCommander = commander.Owner;
        _activeCommander = args.Action.Comp.Toggled ? null : commander.Owner;
        _selectionAction = args.Action.Owner;
        _dragStart = null;
        DragEnd = null;
    }

    private void OnCommanderShutdown(Entity<FieldCommanderComponent> commander, ref ComponentShutdown args)
    {
        if (_activeCommander == commander.Owner)
            _activeCommander = null;
        if (_selectedCommander == commander.Owner)
            _selectedCommander = null;
    }

    private bool OnUse(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        if (!_timing.IsFirstTimePredicted ||
            _activeCommander is not { } commander ||
            _player.LocalEntity != commander)
            return false;

        switch (args.State)
        {
            case BoundKeyState.Down:
                _dragStart = args.Coordinates;
                DragEnd = args.Coordinates;
                return true;
            case BoundKeyState.Up when _dragStart is { } start:
                DragEnd = args.Coordinates;
                if (TryComp<FieldCommanderComponent>(commander, out var component))
                    SelectUnits((commander, component), start, args.Coordinates);

                RaiseNetworkEvent(new FieldCommanderSelectRectangleEvent
                {
                    Commander = GetNetEntity(commander),
                    SelectionAction = GetNetEntity(_selectionAction!.Value),
                    Start = GetNetCoordinates(start),
                    End = GetNetCoordinates(args.Coordinates),
                });
                _dragStart = null;
                DragEnd = null;
                _activeCommander = null;
                if (_selectionAction is { } selectionAction)
                    _actions.SetToggled(selectionAction, false);
                return true;
            default:
                return true;
        }
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        if (_dragStart != null)
            DragEnd = _transform.ToCoordinates(_eye.PixelToMap(_input.MouseScreenPosition));
    }

    private void UpdateOutline(HashSet<EntityUid> selected)
    {
        foreach (var uid in _outlined.Where(uid => !selected.Contains(uid)).ToArray())
        {
            if (TryComp<SpriteComponent>(uid, out var sprite))
                sprite.PostShader = null;
            _outlined.Remove(uid);
        }

        foreach (var uid in selected)
        {
            if (!TryComp<SpriteComponent>(uid, out var sprite))
                continue;

            sprite.PostShader = _shader;
            _outlined.Add(uid);
        }
    }

    private void ClearOutline() => UpdateOutline([]);
}
