using Content.Server.Explosion.EntitySystems;
using Content.Shared.Examine;

namespace Content.Server._SCP.Scps.peanut;

public sealed class PeanutSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ScpPeanutComponent, TriggerEvent>(OnTrigger);
    }

    public void OnTrigger(EntityUid owner, ScpPeanutComponent comp, TriggerEvent args)
    {
        
    }
}
