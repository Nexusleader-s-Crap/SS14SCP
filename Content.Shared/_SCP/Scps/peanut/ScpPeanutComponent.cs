using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server._SCP.Scps.peanut;

[RegisterComponent,NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ScpPeanutComponent : Component
{
    [DataField]
    public Dictionary<EntityUid, bool> isStaring = new Dictionary<EntityUid, bool>();

    [DataField]
    public int watchers = 0;

    [DataField]
    [AutoNetworkedField]
    public bool canMove = false;

    [DataField]
    public bool update = false;

    [DataField("snapDistance")]
    public float snapDistance = 0.5f;

    [DataField]
    public EntProtoId teleportAction = "ActionPeanutTeleport";
}
