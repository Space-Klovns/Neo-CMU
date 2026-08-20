// SPDX-License-Identifier: MPL-2.0

using Content.Shared.Actions;
using Robust.Shared.Map;

namespace Content.Shared._KS14.FieldCommand;

using Robust.Shared.Serialization;

public sealed partial class FieldCommanderToggleSelectionEvent : InstantActionEvent;
public sealed partial class FieldCommanderClearSelectionEvent : InstantActionEvent;
public sealed partial class FieldCommanderMoveOrderEvent : WorldTargetActionEvent;

/// <summary>Client request sent after a completed RTS-style drag selection.</summary>
[Serializable, NetSerializable]
public sealed class FieldCommanderSelectRectangleEvent : EntityEventArgs
{
    public NetEntity Commander;
    public NetEntity SelectionAction;
    public NetCoordinates Start;
    public NetCoordinates End;
}
