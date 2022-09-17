using Content.Server.Actions;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Nutrition.Components;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server.Prawn
{
    public sealed class PrawnSystem : EntitySystem
    {
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly ActionsSystem _action = default!;
        [Dependency] private readonly AtmosphereSystem _atmos = default!;
        [Dependency] private readonly TransformSystem _xform = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PrawnComponent, ComponentStartup>(OnStartup);

            SubscribeLocalEvent<PrawnComponent, PrawnRaiseArmyActionEvent>(OnRaiseArmy);
            SubscribeLocalEvent<PrawnComponent, PrawnDomainActionEvent>(OnDomain);
        }

        private void OnStartup(EntityUid uid, PrawnComponent component, ComponentStartup args)
        {
            _action.AddAction(uid, component.ActionRaiseArmy, null);
            _action.AddAction(uid, component.ActionDomain, null);
        }

        /// <summary>
        /// Summons an allied prawn servant, costing a small amount of thirst
        /// </summary>
        private void OnRaiseArmy(EntityUid uid, PrawnComponent component, PrawnRaiseArmyActionEvent args)
        {
            if (args.Handled)
                return;

            if (!TryComp<ThirstComponent>(uid, out var thirst))
                return;

            //make sure the thirst doesn't go into the negatives
            if (thirst.CurrentThirst < component.ThirstPerArmyUse)
            {
                _popup.PopupEntity(Loc.GetString("prawn-too-hungry"), uid, Filter.Entities(uid));
                return;
            }
            args.Handled = true;
            thirst.CurrentThirst -= component.ThirstPerArmyUse;
            Spawn(component.ArmyMobSpawnId, Transform(uid).Coordinates); //spawn the little mouse boi
        }

        /// <summary>
        /// uses thirst to release a specific amount of miasma into the air. This heals the rat king
        /// and his servants through a specific metabolism.
        /// </summary>
        private void OnDomain(EntityUid uid, PrawnComponent component, PrawnDomainActionEvent args)
        {
            if (args.Handled)
                return;

            if (!TryComp<ThirstComponent>(uid, out var thirst))
                return;

            //make sure the thirst doesn't go into the negatives
            if (thirst.CurrentThirst < component.ThirstPerDomainUse)
            {
                _popup.PopupEntity(Loc.GetString("prawn-too-hungry"), uid, Filter.Entities(uid));
                return;
            }
            args.Handled = true;
            thirst.CurrentThirst -= component.ThirstPerDomainUse;

            _popup.PopupEntity(Loc.GetString("prawn-domain-popup"), uid, Filter.Pvs(uid));

            var transform = Transform(uid);
            var indices = _xform.GetGridOrMapTilePosition(uid, transform);
            var tileMix = _atmos.GetTileMixture(transform.GridUid, transform.MapUid, indices, true);
            tileMix?.AdjustMoles(Gas.Miasma, component.MolesMiasmaPerDomain);
        }
    }

    public sealed class PrawnRaiseArmyActionEvent : InstantActionEvent { };
    public sealed class PrawnDomainActionEvent : InstantActionEvent { };
};
