using Content.Shared._SCP.Scps.Oldman.Components;
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
        SubscribeLocalEvent<PocketDimensionSenderComponent, CreateTeleportNodeDoAfterEvent>(OnCreateNode);
        SubscribeLocalEvent<PocketDimensionSenderComponent, DestroyTeleportNodeEvent>(OnDestroyNode);
        SubscribeLocalEvent<PocketDimensionSenderComponent, TraverseTeleportNodeDoAfterEvent>(OnTraverseNode);

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
            var puddle = Comp<CorrosivePuddleComponent>(SpawnAtPosition(comp.PocketPuddle, transform.Coordinates));
            puddle.shouldDecay = true;
            _xformSystem.SetCoordinates(entity, new EntityCoordinates(comp.pocketDimensionGrid.Value, Vector2.Zero));
            EntityManager.EventBus.RaiseComponentEvent<EnterPocketDimension>(entity, dweller, new EnterPocketDimension());
        }
    }

    private void OnTogglePocketDimeison(EntityUid owner, PocketDimensionSenderComponent comp, TogglePocketDimensionDoAfter args)
    {
        if(!_oldMan.GetAction<TraversePocketDimensionActionComponent>(owner, out var traverse, out var traverseId))
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
            _actions.SetCooldown(traverseId, traverse.cooldownExit);
            _xformSystem.SetCoordinates(owner, comp.lastLocation);
        }
        else
        {
            if (!TryComp<TransformComponent>(owner, out var mover))
                return;
            comp.movePuddleEntity = SpawnAtPosition(comp.PocketPuddle, mover.Coordinates);
            _actions.SetCooldown(traverseId, traverse.cooldownEnter);
            comp.lastLocation = mover.Coordinates;
            _xformSystem.SetCoordinates(owner, new EntityCoordinates(comp.pocketDimensionGrid.Value, Vector2.Zero));
        }
        comp.inPocketDimension = !comp.inPocketDimension;
    }

    private void OnCreateNode(EntityUid owner, PocketDimensionSenderComponent comp, CreateTeleportNodeDoAfterEvent args)
    {
        if (args.Cancelled||args.Handled)
            return;

        if (!_oldMan.GetAction<CreateTeleportNodeComponent>(owner, out var node, out var nodeId))
            return;

        if (!TryComp<TransformComponent>(owner, out var mover))
            return;

        _actions.SetCooldown(nodeId, node.cooldown);

        if (comp.teleportNode == null)
        {
            comp.teleportNode = SpawnAtPosition(comp.PocketPuddle, mover.Coordinates);
        }
        else
        {
            _xformSystem.SetCoordinates(comp.teleportNode.Value, mover.Coordinates);
        }

        comp.nodeLocation = mover.Coordinates;

        if (!_oldMan.GetAction<DestroyTeleportNodeComponent>(owner, out var _, out var _))
            _actions.AddAction(owner, comp.destroyNodeAction);
        if (!_oldMan.GetAction<TraverseTeleportNodeComponent>(owner, out var _, out var _))
            _actions.AddAction(owner, comp.traverseNodeAction);
    }

    public void OnDestroyNode(EntityUid owner, PocketDimensionSenderComponent comp, DestroyTeleportNodeEvent args)
    {
        if (comp.teleportNode == null)
            return;

        if (!TryComp<CorrosivePuddleComponent>(comp.teleportNode, out var puddle))
            return;
        comp.teleportNode = null;
        puddle.shouldDecay = true;

        if (_oldMan.GetAction<CreateTeleportNodeComponent>(owner, out var createcomp, out var create))
            _actions.SetCooldown(owner, createcomp.destroyCooldown);

        if (_oldMan.GetAction<DestroyTeleportNodeComponent>(owner, out var _, out var destroy))
            _actions.RemoveAction(destroy);
        if (_oldMan.GetAction<TraverseTeleportNodeComponent>(owner, out var _, out var traverse))
            _actions.RemoveAction(traverse);
    }

    public void OnTraverseNode(EntityUid owner, PocketDimensionSenderComponent comp, TraverseTeleportNodeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!_oldMan.GetAction<TraverseTeleportNodeComponent>(owner, out var node, out var nodeId))
            return;

        if (!TryComp<TransformComponent>(owner, out var mover))
            return;

        if (comp.teleportNode == null)
            return;

        _actions.SetCooldown(nodeId, node.teleportCooldown);

        _xformSystem.SetCoordinates(owner, comp.nodeLocation);

        if (!_mind.TryGetMind(owner, out var _, out var mind))
            return;
        if (mind.Session == null)
            return;

        _audio.PlayGlobal(comp.puddleSound, mind.Session);
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
