using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
public sealed partial class TogglePocketDimension : InstantActionEvent { }


[Serializable,NetSerializable]
public sealed partial class TogglePocketDimensionDoAfter : SimpleDoAfterEvent { }

public sealed partial class OldManSpawn : EventArgs { }
