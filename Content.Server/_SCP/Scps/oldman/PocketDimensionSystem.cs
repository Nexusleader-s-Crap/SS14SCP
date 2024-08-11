using Content.Server._SCP.Scps.oldman.Components;
using Content.Server.GameTicking;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared._SCP.Scps.Oldman;
using Content.Shared.Humanoid;
using System.Numerics;
using Content.Shared.Actions;
using Robust.Shared.Audio.Systems;
using Content.Shared.Mind;
using Content.Shared.Coordinates;
using Content.Shared.Mobs;

namespace Content.Server._SCP.Scps.oldman;


public sealed class PocketDimensionSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly SharedOldManSystem _oldMan = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private ISawmill _sawmill = default!;

    public const string pocketDimensionMapPath = "/Maps/Test/admin_test_arena.yml";

    public override void Initialize()
    {
        _sawmill = _logManager.GetSawmill("pocketdimensionlogs");
        base.Initialize();

        SubscribeLocalEvent<PocketDimensionSenderComponent, MeleeHitEvent>(OnSend);

        SubscribeLocalEvent<PocketDimensionSenderComponent, OldManSpawn>(OnStartup);
        SubscribeLocalEvent<PocketDimensionSenderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PocketDimensionSenderComponent, TogglePocketDimensionDoAfter>(OnTogglePocketDimeison);
        SubscribeLocalEvent<PocketDimensionDwellerComponent, MobStateChangedEvent>(OnStateChange);
    }

    public void OnStateChange(EntityUid owner, PocketDimensionDwellerComponent comp, MobStateChangedEvent args)
    {
        if (!TryComp<TransformComponent>(owner, out var transform))
            return;
        if (args.NewMobState == MobState.Critical)
        {
            if (!TryComp<PocketDimensionSenderComponent>(comp.dimensionOwner, out var dimowner))
                return;
            _audio.PlayPvs(dimowner.puddleSound, transform.Coordinates);
            QueueDel(owner);
        }
    }
    private void OnSend(EntityUid owner, PocketDimensionSenderComponent comp, MeleeHitEvent args)
    {
        if (comp.pocketDimensionGrid == null)
            return;
        foreach (var entity in args.HitEntities)
        {
            if (HasComp<PocketDimensionDwellerComponent>(entity))
                return;
            if (!TryComp<HumanoidAppearanceComponent>(entity, out var person)) //Better check for a player
                return;
            if (HasComp<PocketDimensionSenderComponent>(entity))
                return;
            if (!TryComp<TransformComponent>(entity, out var transform))
                return;
            var dweller = AddComp<PocketDimensionDwellerComponent>(entity);
            dweller.dimensionOwner = owner;
            var puddle = Comp<CorrosivePuddleComponent>(SpawnAttachedTo(comp.PocketPuddle, transform.Coordinates));
            puddle.shouldDecay = true;
            _xformSystem.SetCoordinates(entity, new EntityCoordinates(comp.pocketDimensionGrid.Value, Vector2.Zero));
            EntityManager.EventBus.RaiseComponentEvent<EnterPocketDimension>(entity, dweller, new EnterPocketDimension());
        }
    }

    private void OnTogglePocketDimeison(EntityUid owner, PocketDimensionSenderComponent comp, TogglePocketDimensionDoAfter args)
    {
        if(!_oldMan.GetTraverseComponent(owner, out var traverse))
            return;

        if (args.Cancelled)
            return;

        if (comp.pocketDimensionGrid == null)
            return;

        if (!_mind.TryGetMind(owner, out var _, out var mind))
            return;
        if (mind.Session == null)
            return;

        _audio.PlayGlobal(comp.puddleSound, mind.Session);

        if (comp.inPocketDimension)
        {
            if(TryComp<CorrosivePuddleComponent>(comp.movePuddleEntity,out var puddle))
            {
                puddle.decayTimer = TimeSpan.FromSeconds(3f);
                puddle.shouldDecay = true;
            }
            _actions.SetCooldown(traverse.actionId, traverse.cooldownExit);
            _xformSystem.SetCoordinates(owner, comp.lastLocation);
        }
        else
        {
            if (!TryComp<TransformComponent>(owner, out var mover))
                return;
            comp.movePuddleEntity = SpawnAttachedTo(comp.PocketPuddle, mover.Coordinates);
            _actions.SetCooldown(traverse.actionId, traverse.cooldownEnter);
            comp.lastLocation = mover.Coordinates;
            _xformSystem.SetCoordinates(owner, new EntityCoordinates(comp.pocketDimensionGrid.Value, Vector2.Zero));
        }
        comp.inPocketDimension = !comp.inPocketDimension;
    }

    private void OnStartup(EntityUid owner, PocketDimensionSenderComponent comp, OldManSpawn args)
    {
        if (comp.pocketDimensionGrid == null)
        {
            var map = _mapManager.GetMapEntityId(_mapManager.CreateMap());
            _metaDataSystem.SetEntityName(map, "Pocket Dimension");

            var grids = _map.LoadMap(Comp<MapComponent>(map).MapId, pocketDimensionMapPath);
            if (grids.Count > 0)
            {
                _metaDataSystem.SetEntityName(grids[0], "Pocket Dimension Grid");
                comp.pocketDimensionGrid = grids[0];
            }
            comp.pocketDimensionMap = map;
        }
    }

    private void OnShutdown(EntityUid owner, PocketDimensionSenderComponent comp, ComponentShutdown args)
    {
        if (comp.pocketDimensionGrid == null)
            return;
        _mapManager.DeleteGrid(comp.pocketDimensionGrid.Value);
        _mapManager.DeleteMap(Comp<MapComponent>(comp.pocketDimensionMap).MapId);
    }

}
