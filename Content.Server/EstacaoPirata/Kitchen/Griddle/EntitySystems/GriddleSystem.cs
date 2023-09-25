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

// TODO: refatorar o sistema para ser mais generico, seguindo a ideia do hot surface system, para que seja mais facil implementar qualquer entidade que seja capaz de grelhar/fritar

public sealed class GriddleSystem : SharedGriddleSystem
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
        SubscribeLocalEvent<GriddleComponent, ComponentInit>(OnComponentInit);
        //SubscribeLocalEvent<SearableComponent, AboveHotSurface>(OnStartBeingGriddled);
    }

    // TODO: pensar em como vai funcionar o sistema para "adicionar" calor ao item
    public override void Update(float frameTime)
    {
        HandleEntityInteractions();

        var griddles = EntityQueryEnumerator<GriddleComponent>();

         while (griddles.MoveNext(out var uid, out var griddleComponent))
         {
             TryUpdateVisualState(uid, griddleComponent); // TODO: melhorar chamada talvez

             if(_gameTimingSystem.CurTime < griddleComponent.NextSearTime || !_powerReceiverSystem.IsPowered(uid))
                 continue;

             UpdateNextSearTime(uid,griddleComponent);

             foreach (var item in griddleComponent.EntitiesOnTop)
             {
                 SearItem(uid,griddleComponent,item);
             }
         }
    }

    // TODO: fazer o log das acoes para controles de adm
    private void OnStartBeingGriddled(EntityUid uid, SearableComponent component, AboveHotSurface args)
    {
        if (args.HotSurface == null)
            return;

        string logString = args.Entering ? "entering" : "leaving";
        Log.Info($"{uid} is {logString} {args.HotSurface}");

        //RaiseNetworkEvent(new OnHotSurface(args.Occupant.Value, args.Entering), uid);

    }

    private void OnComponentInit(EntityUid uid, GriddleComponent component, ComponentInit args)
    {
        TryUpdateVisualState(uid, component);
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
    /// This method handles Griddle to Item interactions and vice versa
    /// </summary>
    private void HandleEntityInteractions()
    {
        var enumerator = EntityQueryEnumerator<GriddleComponent, TransformComponent>();

        while (enumerator.MoveNext(out var griddleUid, out var griddleComponent, out var transform))
        {
            if(!_powerReceiverSystem.IsPowered(griddleUid))
                continue;

            var enumeratorSearables = EntityQueryEnumerator<SearableComponent, TransformComponent>();

            while (enumeratorSearables.MoveNext(out var searableUid, out _, out _))
            {
                var griddleAabb = _lookup.GetWorldAABB(griddleUid, transform);
                var otherAabb = _lookup.GetWorldAABB(searableUid);

                // TODO: ver melhor esta coisa do valor 0.3
                if (!griddleComponent.EntitiesOnTop.Contains(searableUid) &&
                    griddleAabb.IntersectPercentage(otherAabb) >= 0.3)
                {
                    griddleComponent.EntitiesOnTop.Add(searableUid);
                    var aboveHotSurface = new AboveHotSurface(searableUid, griddleUid, true);
                    //RaiseLocalEvent(searableUid, aboveHotSurface);
                    RaiseNetworkEvent(aboveHotSurface);
                }

                else if (griddleComponent.EntitiesOnTop.Contains(searableUid) &&
                         griddleAabb.IntersectPercentage(otherAabb) < 0.3)
                {
                    griddleComponent.EntitiesOnTop.Remove(searableUid);
                    var aboveHotSurface = new AboveHotSurface(searableUid, griddleUid, false);
                    //RaiseLocalEvent(searableUid, aboveHotSurface);
                    RaiseNetworkEvent(aboveHotSurface);
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
            var delta = (component.TemperatureUpperLimit - temperatureComponent.CurrentTemperature) * (temperatureComponent.HeatCapacity * 0.06f); // TODO: ver melhor como obter valor

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

    private void TryUpdateVisualState(EntityUid uid, GriddleComponent griddleComponent)
    {
        var finalState = GriddleComponent.GriddleVisualState.Normal;
        if (_powerReceiverSystem.IsPowered(uid))
        {
            finalState = GriddleComponent.GriddleVisualState.Powered;
        }

        _appearance.SetData(uid, GriddleComponent.GriddleVisuals.VisualState, finalState);
    }
}
