using Content.Shared._SCP.Scps.Oldman.Components;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Effects;
using Content.Shared.Mind;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Marker;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Player;
using System.Numerics;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared._SCP.Scps.Oldman;

public sealed class SharedOldManSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MobStateSystem _state = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<PocketDimensionSenderComponent, ComponentStartup>(OnStartComponent);

        SubscribeLocalEvent<PocketDimensionSenderComponent, TogglePocketDimension>(OnTogglePocket);
        SubscribeLocalEvent<PocketDimensionSenderComponent, CreateTeleportNodeEvent>(OnCreateNode);
        SubscribeLocalEvent<PocketDimensionSenderComponent, TraverseTeleportNodeEvent>(OnTraverseNode);

        SubscribeLocalEvent<PocketDimensionDwellerComponent, EnterPocketDimension>(OnEnter);
    }

    #region startup
    public void OnEnter(EntityUid owner, PocketDimensionDwellerComponent comp, EnterPocketDimension args)
    {
        if (!TryComp<PocketDimensionSenderComponent>(comp.dimensionOwner, out var pocket))
            return;
        if (!_mind.TryGetMind(owner, out var _, out var mind))
            return;
        if (mind.Session == null)
            return;
        _alerts.ShowAlert(owner,comp.PocketDimensionAlert);
        _audio.PlayGlobal(pocket.laughSound, mind.Session);
    }

    public void OnStartComponent(EntityUid owner, PocketDimensionSenderComponent comp, ComponentStartup args)
    {
        _actions.AddAction(owner, comp.traversePocketAction);
        _actions.AddAction(owner, comp.createNodeAction);
        EntityManager.EventBus.RaiseComponentEvent<OldManSpawn>(owner, comp,new OldManSpawn());
    }

    #endregion

    #region actions
    public void OnTogglePocket(EntityUid owner, PocketDimensionSenderComponent pocket, TogglePocketDimension args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var ev = new TogglePocketDimensionDoAfter();
        var popup = "scp-oldman-traversepocket";

        var doAfterArgs = new DoAfterArgs(EntityManager, args.Performer, TimeSpan.FromSeconds(3f), ev, args.Performer)
        {
            BreakOnMove = true,
        };
        if (_doAfter.TryStartDoAfter(doAfterArgs))
            _popup.PopupPredicted(Loc.GetString(popup), args.Performer, args.Performer, PopupType.LargeCaution);
    }

    public void OnCreateNode(EntityUid owner, PocketDimensionSenderComponent pocket, CreateTeleportNodeEvent args)
    {
        if (args.Handled)
            return;
        args.Handled = true;

        if (pocket.inPocketDimension)
        {
            _popup.PopupClient(Loc.GetString("scp-oldman-node-inpocket"), args.Performer, args.Performer, PopupType.MediumCaution);
            return;
        }

        var ev = new CreateTeleportNodeDoAfterEvent();
        var popup = "scp-oldman-createnode";

        var doAfterArgs = new DoAfterArgs(EntityManager, args.Performer, TimeSpan.FromSeconds(3f), ev, args.Performer)
        {
            BreakOnMove = true,
        };
        if (_doAfter.TryStartDoAfter(doAfterArgs))
            _popup.PopupClient(Loc.GetString(popup), args.Performer, args.Performer, PopupType.Medium);
    }

    public void OnTraverseNode(EntityUid owner, PocketDimensionSenderComponent pocket, TraverseTeleportNodeEvent args)
    {
        if (args.Handled)
            return;
        args.Handled = true;

        if (pocket.inPocketDimension)
        {
            _popup.PopupClient(Loc.GetString("scp-oldman-node-inpocket"), args.Performer, args.Performer, PopupType.MediumCaution);
            return;
        }

        var ev = new TraverseTeleportNodeDoAfterEvent();
        var popup = "scp-oldman-traversepocket";

        var doAfterArgs = new DoAfterArgs(EntityManager, args.Performer, TimeSpan.FromSeconds(3f), ev, args.Performer)
        {
            BreakOnMove = true,
        };
        if (_doAfter.TryStartDoAfter(doAfterArgs))
            _popup.PopupPredicted(Loc.GetString(popup), args.Performer, args.Performer, PopupType.LargeCaution);
    }
    #endregion

    public bool GetAction<T>(EntityUid uid, [NotNullWhen(true)] out T? comp, out EntityUid id) where T : IComponent
    {
        foreach (var item in _actions.GetActions(uid))
        {
            if (TryComp<T>(item.Id, out var traverse))
            {
                id = item.Id;
                comp = traverse;
                return true;
            }
        }
        comp = default;
        id = uid;
        return false;
    }

    public override void Update(float frameTime)
    {
        var puddles = EntityQueryEnumerator<CorrosivePuddleComponent>();

        while (puddles.MoveNext(out var uid, out var puddle))
        {
            if (puddle.shouldDecay && !puddle.isDecaying)
            {
                puddle.decayStart = _timing.CurTime;
                puddle.isDecaying = true;
            }
            else if (puddle.isDecaying)
            {
                if (puddle.decayStart + puddle.decayTimer < _timing.CurTime)
                    QueueDel(uid);
            }
        }

        var people = EntityQueryEnumerator<PocketDimensionDwellerComponent>();

        while (people.MoveNext(out var uid, out var person))
        {
            if (person.lastDamaged + person.damageInterval < _timing.CurTime)
            {
                if (!TryComp<PocketDimensionSenderComponent>(person.dimensionOwner, out var _))
                    continue;
                if (!_prototypeManager.TryIndex(person.damageProto, out var damageType))
                    continue;

                var downer = Comp<MeleeWeaponComponent>(person.dimensionOwner);

                DamageSpecifier damages = new DamageSpecifier(damageType, person.damageOverTime);
                _damage.TryChangeDamage(uid, downer.Damage);

                _color.RaiseEffect(Color.Red, new List<EntityUid>() { uid }, Filter.Pvs(uid, entityManager: EntityManager));

                person.lastDamaged = _timing.CurTime;

                if (!_mind.TryGetMind(uid, out var _, out var mind))
                    continue;
                if (mind.Session == null)
                    continue;
                _audio.PlayGlobal(person.HitNoise, mind.Session);
            }
        }
    }
}

