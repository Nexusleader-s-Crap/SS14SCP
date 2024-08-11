using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Server._SCP.Scps.oldman.Components;

[RegisterComponent,NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class PocketDimensionSenderComponent : Component
{
    public EntityUid? pocketDimensionGrid;
    public EntityUid pocketDimensionMap;
    public EntityUid movePuddleEntity;
    public EntityCoordinates lastLocation;

    [AutoNetworkedField]
    public bool inPocketDimension = false;

    [DataField]
    public EntProtoId traversePocketAction = "ActionTraversePocketDimension";

    [DataField]
    public EntProtoId PocketPuddle = "OldManPuddle";

    [DataField]
    public SoundSpecifier puddleSound = new SoundPathSpecifier("/Audio/_SCP/Effects/106puddle.ogg");

    [DataField]
    public SoundSpecifier laughSound = new SoundPathSpecifier("/Audio/_SCP/Effects/106laugh.ogg");
}
