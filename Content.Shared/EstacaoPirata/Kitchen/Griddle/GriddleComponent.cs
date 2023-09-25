using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.EstacaoPirata.Kitchen.Griddle;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GriddleComponent : Component
{
    // TODO: retirar daqui entidades que nao existem mais
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

    [DataField("normalState")]
    public string? NormalState;

    [DataField("poweredState")]
    public string? PoweredState;

    [Serializable, NetSerializable]
    public enum GriddleVisualState
    {
        Normal,
        Powered
    }

    public enum GriddleVisualLayers : byte
    {
        Base,
        Powered
    }

    [Serializable, NetSerializable]
    public enum GriddleVisuals
    {
        VisualState
    }
}
