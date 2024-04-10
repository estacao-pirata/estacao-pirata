using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._EstacaoPirata.SiliconRepairable;

public abstract partial class SharedSiliconRepairableSystem : EntitySystem
{
    [Serializable, NetSerializable]
    protected sealed partial class RepairFinishedEvent : SimpleDoAfterEvent
    {
    }
}

