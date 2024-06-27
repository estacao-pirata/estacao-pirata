using Content.Server.Atmos.Components;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;

namespace Content.Server._EstacaoPirata.ConditionalTemperatureAdjuster;

/// <summary>
/// This handles...
/// </summary>
public sealed class ConditionalTemperatureAdjusterSystem : EntitySystem
{

    [Dependency] private readonly IGameMapManager _gameMapManager = default!;
    [Dependency] private readonly TemperatureSystem _temperatureSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ConditionalTemperatureAdjusterComponent, ComponentInit>(OnInit);

    }
    private void OnInit(EntityUid uid, ConditionalTemperatureAdjusterComponent comp, ref ComponentInit _)
    {
        var mapName = _gameMapManager.GetSelectedMap()?.MapName;
        if (mapName == null)
            return;

        if (!TryComp(uid, out TemperatureComponent? temperatureComponent))
            return;


        foreach (var prototype in comp.Prototypes)
        {
            if (prototype.MapName != mapName)
                continue;

            temperatureComponent.ColdDamageThreshold = prototype.ColdDamageThreshold;
            temperatureComponent.HeatDamageThreshold = prototype.HeatDamageThreshold;
            RemComp<ConditionalTemperatureAdjusterComponent>(uid);
        }

    }
}
