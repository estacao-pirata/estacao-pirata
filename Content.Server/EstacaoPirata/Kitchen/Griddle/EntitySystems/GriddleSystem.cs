using System.Linq;
using Content.Server.Temperature.Systems;
using Content.Shared.EstacaoPirata.Kitchen;
using Content.Shared.Kitchen;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.EstacaoPirata.Kitchen.Griddle;
using Content.Shared.StepTrigger.Components;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Physics.Components;

namespace Content.Server.EstacaoPirata.Kitchen.Griddle.EntitySystems;

/// <summary>
/// This handles...
/// </summary>
public sealed class GriddleSystem : EntitySystem
{
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
        SubscribeLocalEvent<GriddleComponent, GriddleComponent.BeingGriddledEvent>(OnStartBeingGriddled);
    }



    public override void Update(float frameTime)
    {
        var enumerator = EntityQueryEnumerator<GriddleComponent, TransformComponent>();
        var enumeratorSearables = EntityQueryEnumerator<SearableComponent, TransformComponent>();

        while (enumerator.MoveNext(out var uid, out var griddleComponent,  out var transform))
        {
            //var entities = _lookup.GetEntitiesIntersecting(uid, LookupFlags.Dynamic);

            while (enumeratorSearables.MoveNext(out var searableUid, out var searableComponent, out var searableTransform))
            {
                var griddleAabb = _lookup.GetWorldAABB(uid, transform);
                var otherAabb = _lookup.GetWorldAABB(searableUid);

                if (!otherAabb.Intersects(griddleAabb))
                    continue;

                Log.Debug($"Searable: {searableUid}\nGriddle: {uid}");

                // TODO: ver melhor esta coisa do valor 0.3
                if (!griddleComponent.EntitiesOnTop.Contains(searableUid) && griddleAabb.IntersectPercentage(otherAabb) >= 0.3)
                {
                    griddleComponent.EntitiesOnTop.Add(searableUid);
                    var beingGriddledEvent = new GriddleComponent.BeingGriddledEvent(searableUid, true);
                    RaiseLocalEvent(uid, beingGriddledEvent);
                }

                else if (griddleComponent.EntitiesOnTop.Contains(searableUid) && griddleAabb.IntersectPercentage(otherAabb) < 0.3 )
                {
                    griddleComponent.EntitiesOnTop.Remove(searableUid);
                    var beingGriddledEvent = new GriddleComponent.BeingGriddledEvent(searableUid, false);
                    RaiseLocalEvent(uid, beingGriddledEvent);
                }
            }

            // if (entities.Any())
            // {
            //     var beingGriddledEvent = new GriddleComponent.BeingGriddledEvent(entities.First());
            //     RaiseLocalEvent(uid, beingGriddledEvent);
            // }
        }
    }

    private void OnStartBeingGriddled(EntityUid uid, GriddleComponent component, GriddleComponent.BeingGriddledEvent args)
    {
        if (args.Occupant == null)
            return;

        if (args.Entering)
        {
            Log.Debug($"{args.Occupant} is entering {uid}");
        }
        else
        {
            Log.Debug($"{args.Occupant} is leaving {uid}");
        }

        // if (!TryComp(args.Occupant, out GriddledComponent? comp))
        //     return;

        // Colocar o ocupante em uma lista
        // Esta lista pode estar no componente do griddle, pra guardar as coisas em cima dele

        // if (component.EntitiesOnTop.Contains(args.Occupant.Value))
        //     return;
        //
        // component.EntitiesOnTop.Add(args.Occupant.Value);

        // Dar ao ocupante um componente que indique que esta sendo griddled
        // Para quem tem esse componente, ficar checando se esta no mesmo grid de algum griddle?
        // Pra quem tem esse componente, adicionar hit box pra trigger para ver se esta em cima?

        //Log.Debug($"{args.Occupant} is on {uid}");
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
