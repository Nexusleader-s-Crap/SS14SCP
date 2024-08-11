using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Server._SCP.Scps.oldman.Components;

[RegisterComponent,NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class TraversePocketDimensionActionComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public EntityUid actionId;

    [DataField]
    public TimeSpan cooldownEnter = TimeSpan.FromSeconds(10f);

    [DataField]
    public TimeSpan cooldownExit = TimeSpan.FromMinutes(1f);
}
