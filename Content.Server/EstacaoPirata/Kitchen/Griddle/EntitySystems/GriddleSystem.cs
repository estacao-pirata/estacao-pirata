using System.Linq;
using Content.Server.Temperature.Systems;
using Content.Shared.Kitchen;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.EstacaoPirata.Kitchen.Griddle;
using Content.Shared.StepTrigger.Components;
using Content.Shared.StepTrigger.Systems;

namespace Content.Server.EstacaoPirata.Kitchen.Griddle.EntitySystems;

/// <summary>
/// This handles...
/// </summary>
public sealed class GriddleSystem : EntitySystem
{
    /// <inheritdoc/>

    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly RecipeManager _recipeManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<GriddleComponent, StepTriggeredEvent>(OnStepTriggered);
        SubscribeLocalEvent<GriddleComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
    }

    public override void Update(float frameTime)
    {
        var enumerator = EntityQueryEnumerator<GriddleComponent, StepTriggerComponent, TransformComponent>();
        while (enumerator.MoveNext(out var uid, out _, out _, out _))
        {
            var entities = _lookup.GetEntitiesIntersecting(uid, LookupFlags.Dynamic);

            Log.Debug($"{entities.Count} entities on griddle {uid}");
            var beingGriddledEvent = new GriddleComponent.BeingGriddledEvent(uid, entities.First());
            RaiseLocalEvent(uid, beingGriddledEvent);
        }
    }

    private void OnStepTriggerAttempt(EntityUid uid, GriddleComponent component, ref StepTriggerAttemptEvent args)
    {
        //_popupSystem.PopupEntity("VOCE TENTOU PISAR EM CIMA", args.Tripper);
        args.Continue = true;
    }

    private void OnStepTriggered(EntityUid uid, GriddleComponent component, ref StepTriggeredEvent args)
    {
        _popupSystem.PopupEntity("VOCE PISOU EM CIMA", args.Tripper);
    }
}
