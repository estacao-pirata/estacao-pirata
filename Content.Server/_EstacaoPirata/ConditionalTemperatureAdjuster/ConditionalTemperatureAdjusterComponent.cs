using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Server._EstacaoPirata.ConditionalTemperatureAdjuster;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class ConditionalTemperatureAdjusterComponent : Component
{
    [DataField("maps", required: true)]
    public List<TemperatureConfigurationData> Prototypes = default!;
}


[DataDefinition]
public sealed partial class TemperatureConfigurationData
{
    [DataField("mapName", required: true)]
    public string MapName = default!;

    [DataField]
    public float HeatDamageThreshold = 360f;

    [DataField]
    public float ColdDamageThreshold = 260f;
}
