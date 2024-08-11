using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Server._SCP.Scps.oldman.Components;

[RegisterComponent,NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class PocketDimensionSenderComponent : Component
{
    public EntityUid? pocketDimensionGrid;
    public EntityUid pocketDimensionMap;
    public EntityCoordinates lastLocation;

    [AutoNetworkedField]
    public bool inPocketDimension = false;

    [AutoNetworkedField]
    public bool traversing = false;

    [DataField]
    public EntProtoId traversePocketAction = "ActionTraversePocketDimension";
}
