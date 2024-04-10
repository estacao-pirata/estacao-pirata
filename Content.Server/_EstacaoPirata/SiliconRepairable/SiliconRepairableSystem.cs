using Content.Server.Administration.Logs;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Repairable;
using Content.Shared.Tools;
using SharedToolSystem = Content.Shared.Tools.Systems.SharedToolSystem;

namespace Content.Server._EstacaoPirata.SiliconRepairable
{
    public sealed class SiliconRepairableSystem : SharedRepairableSystem
    {
        [Dependency] private readonly SharedToolSystem _toolSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger= default!;
        [Dependency] private readonly BlindableSystem _blindableSystem = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<SiliconRepairableComponent, InteractUsingEvent>(Repair);
            SubscribeLocalEvent<SiliconRepairableComponent, RepairFinishedEvent>(OnRepairFinished);
        }

        private void OnRepairFinished(EntityUid uid, SiliconRepairableComponent component, RepairFinishedEvent args)
        {


            if (args.Cancelled)
                return;
            TryComp(uid, out BlindableComponent? blindcomp);
            if(!EntityManager.TryGetComponent(uid, out DamageableComponent? damageable))
                return;

            if (damageable.TotalDamage == 0 && blindcomp is { EyeDamage: 0 })
                return;



            if (component.Damage != null)
            {
                var damageChanged = _damageableSystem.TryChangeDamage(uid, component.Damage, true, false, origin: args.User);
                _adminLogger.Add(LogType.Healed, $"{ToPrettyString(args.User):user} repaired {ToPrettyString(uid):target} by {damageChanged?.GetTotal()}");
            }

            else
            {
                if (blindcomp != null)
                    _blindableSystem.AdjustEyeDamage((uid, blindcomp), -blindcomp!.EyeDamage);
                // Repair all damage
                _damageableSystem.SetAllDamage(uid, damageable, 0);
                _adminLogger.Add(LogType.Healed, $"{ToPrettyString(args.User):user} repaired {ToPrettyString(uid):target} back to full health");
            }

            var str = Loc.GetString("comp-repairable-repair",
                ("target", uid),
                ("tool", args.Used!));
            _popup.PopupEntity(str, uid, args.User);
        }

        public async void Repair(EntityUid uid, SiliconRepairableComponent component, InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            // Only try repair the target if it is damaged
            TryComp(uid, out BlindableComponent? blindcomp);
            if(!EntityManager.TryGetComponent(uid, out DamageableComponent? damageable))
                return;
            if (damageable.TotalDamage == 0 && blindcomp is { EyeDamage: 0 })
                return;


            float delay = component.DoAfterDelay;

            // Add a penalty to how long it takes if the user is repairing itself
            if (args.User == args.Target)
            {
                if (!component.AllowSelfRepair)
                    return;

                delay *= component.SelfRepairPenalty;
            }

            // Run the repairing doafter
            args.Handled = _toolSystem.UseTool(args.Used, args.User, uid, delay, component.QualityNeeded, new RepairFinishedEvent());
        }
    }
}
