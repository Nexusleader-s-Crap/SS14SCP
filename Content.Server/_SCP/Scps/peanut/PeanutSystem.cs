using Content.Server.Administration.Logs.Converters;
using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Shared._SCP.Actions;
using Content.Shared._SCP.Scps.peanut;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Administration;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Server._SCP.Scps.peanut;

public sealed class PeanutSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedActionFetchSystem _fetcher = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;

    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        _sawmill = _logManager.GetSawmill("peanutlogging");

        SubscribeLocalEvent<ScpPeanutComponent, TriggerEvent>(OnTrigger);
        SubscribeLocalEvent<ScpPeanutComponent, PeanutTeleportEvent>(OnTeleport);
    }

    public override void Update(float frameTime)
    {//TODO: Turn this into a component (The entire frozen checker)
        var query = EntityQueryEnumerator<ScpPeanutComponent>();
        while(query.MoveNext(out var peanutId, out var peanut))
        {
            if (peanut.update)
            {
                peanut.canMove = peanut.watchers == 0;
                peanut.update = false;

                _blocker.UpdateCanMove(peanutId);

                if (!peanut.canMove)
                {
                    EntityUid? teleport = _actions.AddAction(peanutId, peanut.teleportAction);
                    if (!teleport.HasValue)
                        return;

                    if(TryComp<PeanutTeleportActionComponent>(teleport.Value,out var actionComp))
                        _actions.SetCooldown(teleport, actionComp.teleportCooldown);
                }
                else
                {
                    if (!_fetcher.GetAction<PeanutTeleportActionComponent>(peanutId, out var action, out var actionId))
                        return;
                    _actions.RemoveAction(actionId);
                }

                continue;
            }

            peanut.watchers = 0;
            foreach (var item in peanut.isStaring)
            {
                if (!TryComp<MobStateComponent>(item.Key, out var state))
                    continue;
                if (state.CurrentState == Shared.Mobs.MobState.Dead
                    || state.CurrentState==Shared.Mobs.MobState.Critical)
                    continue;
                var direction = _transform.GetWorldPosition(peanutId) - _transform.GetWorldPosition(item.Key);

                direction.Normalize();

                var difference = Math.Min(
                    Math.Abs(direction.ToWorldAngle().Degrees - _transform.GetWorldPositionRotation(item.Key).WorldRotation.Degrees), //Replace this line with a proper LOS check once i figure it out
                    Math.Abs(_transform.GetWorldPositionRotation(item.Key).WorldRotation.Degrees - direction.ToWorldAngle().Degrees));

                _sawmill.Debug("Difference " + difference.ToString());

                if (difference < 45)
                {
                    peanut.watchers++;
                    _sawmill.Debug("FACING");
                }
                else
                    _sawmill.Debug("NOT FACING");
            }

            var canmove = peanut.watchers == 0;

            peanut.update = peanut.canMove != canmove;
        }
    }

    public void OnTeleport(EntityUid id, ScpPeanutComponent comp, WorldTargetActionEvent args)
    {
        if (comp.canMove)
            return;

        if (!_examine.InRangeUnOccluded(id, args.Target, 40))
            return;

        if(args.Target.GetTileRef(EntityManager) is { } tile &&
               !tile.IsSpace() &&
               tile.GetContentTileDefinition().Sturdy &&
               !_turf.IsTileBlocked(tile, Shared.Physics.CollisionGroup.Impassable))
        {

            if (!_fetcher.GetAction<PeanutTeleportActionComponent>(id, out var teleport, out var teleportId))
                return;

            _actions.SetCooldown(teleportId, teleport.teleportCooldown);
            _transform.SetCoordinates(id, args.Target);

            var peanutPos = _transform.GetWorldPosition(id);

            foreach(var item in comp.isStaring)
            {
                var currentPos = _transform.GetWorldPosition(item.Key);

                var distance = Vector2.Distance(currentPos, peanutPos);

                if (distance <= comp.snapDistance)
                {
                    var damager = Comp<MeleeWeaponComponent>(id);

                    _damage.TryChangeDamage(item.Key, damager.Damage);
                    return;
                }
            }
        }
    }

    public void OnTrigger(EntityUid id, ScpPeanutComponent comp, TriggerEvent args)
    {
        var transform = Transform(id);

        if (!TryComp<TriggerOnProximityComponent>(id, out var proximity))
            return;

        if (!args.User.HasValue)
            return;

        if (!HasComp<HumanoidAppearanceComponent>(args.User))
            return;

        if (_examine.InRangeUnOccluded(id, args.User.Value, proximity.Shape.Radius))
        {
            _sawmill.Info("Visual Range Aquired");
            comp.isStaring[args.User.Value] = false;
        }
        else
        {
            comp.isStaring.Remove(id);
        }
    }
}
