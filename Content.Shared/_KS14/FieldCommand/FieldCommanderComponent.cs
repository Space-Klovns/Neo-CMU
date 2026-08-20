// SPDX-License-Identifier: MPL-2.0

using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._KS14.FieldCommand;

/// <summary>Stores the first corner while a commander is drawing a selection rectangle.</summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FieldCommanderComponent : Component
{
    [AutoNetworkedField]
    public HashSet<EntityUid> SelectedUnits = [];
}

/// <summary>Marks any HTN entity as selectable and commandable by a Field Commander.</summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FieldCommandableComponent : Component
{
}
