using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Server._SCP.Scps.oldman.Components;

[RegisterComponent]
[Access(typeof(PocketDimensionSystem))]
public sealed partial class PocketDimensionSenderComponent : Component
{
    public EntityUid? pocketDimensionGrid;
    public EntityUid pocketDimensionMap;
    public bool inPocketDimension = false;
    public EntityCoordinates lastLocation;

    public EntityUid? pocketDimensionAction;

    [DataField]
    public EntProtoId enterPocketAction = "ActionEnterPocket";

    [DataField]
    public EntProtoId exitPocketAction = "ActionExitPocket";
}

public sealed partial class TogglePocketDimension : InstantActionEvent
{

}
