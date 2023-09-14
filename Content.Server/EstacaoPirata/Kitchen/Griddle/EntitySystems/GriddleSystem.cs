using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.EstacaoPirata.Kitchen;
using Content.Shared.Kitchen;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.EstacaoPirata.Kitchen.Griddle;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Timing;

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
    [Dependency] private readonly IGameTiming _gameTimingSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GriddleComponent, StepTriggeredEvent>(OnStepTriggered);
        SubscribeLocalEvent<GriddleComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<GriddleComponent, GriddleComponent.BeingGriddledEvent>(OnStartBeingGriddled);
    }

    // TODO: pensar em como vai funcionar o sistema para "adicionar" calor ao item
    public override void Update(float frameTime)
    {
        HandleEntityInteractions();

        var griddles = EntityQueryEnumerator<GriddleComponent>();

         while (griddles.MoveNext(out var uid, out var griddleComponent))
         {
             if(_gameTimingSystem.CurTime < griddleComponent.NextSearTime || !_powerReceiverSystem.IsPowered(uid))
                 continue;

             UpdateNextSearTime(uid,griddleComponent);

             foreach (var item in griddleComponent.EntitiesOnTop)
             {
                 SearItem(uid,griddleComponent,item);
             }
         }
    }

    private void OnStartBeingGriddled(EntityUid uid, GriddleComponent component, GriddleComponent.BeingGriddledEvent args)
    {
        if (args.Occupant == null)
            return;

        if (args.Entering)
        {
            // Rodar codigo de entrada
            RaiseNetworkEvent(new GriddleComponent.EnterGriddleEvent(args.Occupant.Value), uid);
            Log.Debug($"{args.Occupant} is entering {uid}");
        }
        else
        {
            // Rodar codigo de saida
            RaiseNetworkEvent(new GriddleComponent.ExitGriddleEvent(args.Occupant.Value), uid);
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

    /// <summary>
    /// This method will handle Griddle to Item interactions and vice versa
    /// </summary>
    private void HandleEntityInteractions()
    {
        var enumerator = EntityQueryEnumerator<GriddleComponent, TransformComponent>();

        while (enumerator.MoveNext(out var uid, out var griddleComponent, out var transform))
        {
            if(!_powerReceiverSystem.IsPowered(uid))
                continue;

            var enumeratorSearables = EntityQueryEnumerator<SearableComponent, TransformComponent>();

            while (enumeratorSearables.MoveNext(out var searableUid, out _, out _))
            {
                var griddleAabb = _lookup.GetWorldAABB(uid, transform);
                var otherAabb = _lookup.GetWorldAABB(searableUid);

                // TODO: ver melhor esta coisa do valor 0.3
                if (!griddleComponent.EntitiesOnTop.Contains(searableUid) &&
                    griddleAabb.IntersectPercentage(otherAabb) >= 0.3)
                {
                    griddleComponent.EntitiesOnTop.Add(searableUid);
                    var beingGriddledEvent = new GriddleComponent.BeingGriddledEvent(searableUid, true);
                    RaiseLocalEvent(uid, beingGriddledEvent);
                }

                else if (griddleComponent.EntitiesOnTop.Contains(searableUid) &&
                         griddleAabb.IntersectPercentage(otherAabb) < 0.3)
                {
                    griddleComponent.EntitiesOnTop.Remove(searableUid);
                    var beingGriddledEvent = new GriddleComponent.BeingGriddledEvent(searableUid, false);
                    RaiseLocalEvent(uid, beingGriddledEvent);
                }
            }
        }
    }

    /// <summary>
    /// This is for...
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="item"></param>
    private void SearItem(EntityUid uid, GriddleComponent component, EntityUid item)
    {
        if (TryComp<TemperatureComponent>(item, out var temperatureComponent))
        {
            var delta = (component.TemperatureUpperLimit - temperatureComponent.CurrentTemperature) * (temperatureComponent.HeatCapacity * 0.01f); // TODO: ver melhor como obter valor

            // quando o item alcancar sua temperatura de sear, iniciar uma contagem de alguns segundos para trocar de sprite

            if (delta > 0f)
            {
                _temperature.ChangeHeat(item,delta,false,temperatureComponent);
            }
        }


    }

    /// <summary>
    /// This is for...
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    private void UpdateNextSearTime(EntityUid uid, GriddleComponent component)
    {
        component.NextSearTime = _gameTimingSystem.CurTime + component.SearInterval;
    }
}
