using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.SimpleStation14.Silicon;

[Serializable, NetSerializable]
<<<<<<< Updated upstream
public sealed class BatteryDrinkerDoAfterEvent : SimpleDoAfterEvent
=======
public sealed partial class BatteryDrinkerDoAfterEvent : SimpleDoAfterEvent
>>>>>>> Stashed changes
{
    public BatteryDrinkerDoAfterEvent()
    {
    }
}
