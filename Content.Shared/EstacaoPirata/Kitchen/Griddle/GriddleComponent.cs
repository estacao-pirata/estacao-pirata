using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.EstacaoPirata.Kitchen.Griddle;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed class GriddleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> EntitiesOnTop = new List<EntityUid>();

    [DataField("temperatureUpperLimit"), ViewVariables(VVAccess.ReadOnly)]
    public float TemperatureUpperLimit = 463.15f;

    [DataField("temperature"), ViewVariables(VVAccess.ReadOnly)]
    public float Temperature = 300f;

    [DataField("cookTimeMultiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float CookTimeMultiplier = 1;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("searInterval")]
    public TimeSpan SearInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// When will the deep fryer layer on the next stage of crispiness?
    /// </summary>
    [DataField("nextSearTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextSearTime { get; set; }

    [Serializable, NetSerializable]
    public sealed class BeingGriddledEvent : HandledEntityEventArgs
    {
        public EntityUid? Occupant;
        public bool Entering;

        public BeingGriddledEvent(EntityUid? occupant, bool entering)
        {
            Occupant = occupant;
            Entering = entering;
        }
    }

    [Serializable, NetSerializable]
    public sealed class EnterGriddleEvent : EntityEventArgs
    {
        public EntityUid Occupant;

        public EnterGriddleEvent(EntityUid occupant)
        {
            Occupant = occupant;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ExitGriddleEvent : EntityEventArgs
    {
        public EntityUid Occupant;

        public ExitGriddleEvent(EntityUid occupant)
        {
            Occupant = occupant;
        }
    }
}
