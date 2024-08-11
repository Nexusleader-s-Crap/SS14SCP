using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;
using Robust.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;

namespace Content.Shared._SCP.Scps.Oldman;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class PocketDimensionDwellerComponent : Component
{
    [AutoNetworkedField]
    public EntityUid dimensionOwner;

    [DataField]
    public ProtoId<AlertPrototype> PocketDimensionAlert = "PocketDimension";

    public TimeSpan lastDamaged;

    [DataField]
    public TimeSpan damageInterval = TimeSpan.FromSeconds(5f);

    [DataField]
    public SoundSpecifier HitNoise = new SoundPathSpecifier("/Audio/_SCP/Effects/106noise.ogg");

    [DataField]
    public int damageOverTime = 1;

    [DataField]
    public ProtoId<DamageTypePrototype> damageProto = "Heat";
}
