using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
public sealed partial class TogglePocketDimension : InstantActionEvent { }


[Serializable,NetSerializable]
public sealed partial class TogglePocketDimensionDoAfter : SimpleDoAfterEvent { }

public sealed partial class OldManSpawn : EventArgs { }

[NetSerializable, Serializable]
public sealed partial class EnterPocketDimension : EventArgs { }

public sealed partial class CreateTeleportNodeEvent : InstantActionEvent { }
[Serializable, NetSerializable]
public sealed partial class CreateTeleportNodeDoAfterEvent : SimpleDoAfterEvent { }
public sealed partial class DestroyTeleportNodeEvent : InstantActionEvent { }
public sealed partial class TraverseTeleportNodeEvent : InstantActionEvent { }
[Serializable, NetSerializable]
public sealed partial class TraverseTeleportNodeDoAfterEvent : SimpleDoAfterEvent { }
