using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client._KS14.FieldCommand;

/// <summary>Draws the live selection box directly in world space while the mouse is dragged.</summary>
public sealed class FieldCommanderSelectionOverlay : Overlay
{
    private readonly IEntityManager _entities;
    private readonly FieldCommanderSystem _fieldCommand;
    private readonly SharedTransformSystem _transform;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public FieldCommanderSelectionOverlay(IEntityManager entities, FieldCommanderSystem fieldCommand)
    {
        _entities = entities;
        _fieldCommand = fieldCommand;
        _transform = entities.System<SharedTransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_fieldCommand.DragStart is not { } start || _fieldCommand.DragEnd is not { } end)
            return;

        var startMap = _transform.ToMapCoordinates(start);
        var endMap = _transform.ToMapCoordinates(end);
        if (startMap.MapId != args.MapId || endMap.MapId != args.MapId)
            return;

        var box = new Box2(Vector2.Min(startMap.Position, endMap.Position), Vector2.Max(startMap.Position, endMap.Position));
        args.WorldHandle.DrawRect(box, Color.Cyan.WithAlpha(0.14f));
        args.WorldHandle.DrawRect(box, Color.Cyan.WithAlpha(0.9f), false);
    }
}
