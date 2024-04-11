using System.Diagnostics;
using Content.Server.Administration.Logs;
using Content.Server.Stack;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tools;
using Content.Shared._EstacaoPirata.SiliconRepairable;
using Content.Shared.Stacks;
using SharedToolSystem = Content.Shared.Tools.Systems.SharedToolSystem;

namespace Content.Server._EstacaoPirata.SiliconRepairable
{
    public sealed class SiliconRepairableSystem : SharedSiliconRepairableSystem
    {
        [Dependency] private readonly SharedToolSystem _toolSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger= default!;
        [Dependency] private readonly BlindableSystem _blindableSystem = default!;
        [Dependency] private readonly StackSystem _stackSystem = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<SiliconRepairableComponent, InteractUsingEvent>(Repair);
            SubscribeLocalEvent<SiliconRepairableComponent, SiliconRepairFinishedEvent>(OnRepairFinished);
        }

        private void OnRepairFinished(EntityUid uid, SiliconRepairableComponent component, SiliconRepairFinishedEvent args)
        {
            if (args.Cancelled)
                return;

            if (args.Used == null)
                return;

            if(!EntityManager.TryGetComponent(uid, out DamageableComponent? damageable))
                return;

            switch (args.DamageType)
            {
                case DamageType.Physical:
                case DamageType.Heat:
                {
                    var typesList = GetDamageTypeList(args.DamageType);

                    if (!VerifyIfDamaged(damageable, typesList))
                    {
                        return;
                    }

                    TryRepairDamageList(args.User, uid, damageable, component, typesList);

                    if (EntityManager.TryGetComponent(args.Used, out StackComponent? stack))
                    {

                        _stackSystem.SetCount((EntityUid) args.Used, _stackSystem.GetCount((EntityUid) args.Used) - 1, stack);
                    }


                    var str = Loc.GetString("comp-repairable-repair",
                        ("target", uid),
                        ("tool", args.Used!));
                    _popup.PopupEntity(str, uid, args.User);

                    if (VerifyIfDamaged(damageable, typesList))
                    {
                        if (args.Used.HasValue)
                        {
                            args.Handled = _toolSystem.UseTool(args.Used.Value, args.User, uid, args.Delay, args.QualityNeeded, new SiliconRepairFinishedEvent
                            {
                                QualityNeeded = args.QualityNeeded,
                                Delay = args.Delay,
                                DamageType = args.DamageType
                            });
                        }
                    }




                    break;
                }
                case DamageType.Blindness:
                {
                    if(!EntityManager.TryGetComponent(uid, out BlindableComponent? blindcomp))
                        return;

                    if (blindcomp is { EyeDamage: 0 })
                        return;

                    _blindableSystem.AdjustEyeDamage((uid, blindcomp), -blindcomp!.EyeDamage);

                    _adminLogger.Add(LogType.Healed, $"{ToPrettyString(args.User):user} repaired {ToPrettyString(uid):target}'s vision");

                    var str = Loc.GetString("comp-repairable-repair",
                        ("target", uid),
                        ("tool", args.Used!));
                    _popup.PopupEntity(str, uid, args.User);
                    break;
                }
            }



        }

        private bool VerifyIfDamaged(DamageableComponent damageable, List<string> damageTypes)
        {
            foreach (var damageType in damageTypes)
            {
                if (damageTypes != null && damageable.Damage.DamageDict.TryGetValue(damageType, out var damage))
                {
                    if (damage.Value > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static List<string> GetDamageTypeList(DamageType damage)
        {
            return damage switch
            {
                DamageType.Physical => ["Blunt", "Piercing", "Slash"],
                DamageType.Heat => ["Heat", "Shock"],
                DamageType.Blindness => [],
                _ => throw new ArgumentOutOfRangeException(nameof(damage), damage, null)
            };
        }

        private bool TryRepairDamageList(EntityUid user, EntityUid? uid, DamageableComponent damageable, SiliconRepairableComponent component, List<string> damageTypes)
        {
            var damageSpecifier = new DamageSpecifier();

            foreach (var type in damageTypes)
            {
                if (component.Damage != null && component.Damage.DamageDict.TryGetValue(type, out var damage))
                {
                    damageSpecifier.DamageDict.Add(type, damage);
                }
                else
                {
                    damageSpecifier.DamageDict.Add(type, FixedPoint2.New(- 40));
                }

            }

            var damageChanged = _damageableSystem.TryChangeDamage(uid, damageSpecifier, true, false, origin: user);
            _adminLogger.Add(LogType.Healed, $"{ToPrettyString(user):user} repaired {ToPrettyString(uid):target} by {damageChanged?.GetTotal()}");

            return false;
        }


        private async void Repair(EntityUid uid, SiliconRepairableComponent component, InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            // Only try repair the target if it is damaged
            TryComp(uid, out BlindableComponent? blindcomp);
            if(!EntityManager.TryGetComponent(uid, out DamageableComponent? damageable))
                return;

            DamageType damageType;
            string qualityNeeded;
            if (_toolSystem.HasQuality(args.Used, component.PhysicalQualityNeeded))
            {
                damageType = DamageType.Physical;
                qualityNeeded = component.PhysicalQualityNeeded;
            }
            else if (_toolSystem.HasQuality(args.Used, component.BurnQualityNeeded))
            {
                damageType = DamageType.Heat;
                qualityNeeded = component.BurnQualityNeeded;


            }
            else if (_toolSystem.HasQuality(args.Used, component.BlindQualityNeeded))
            {
                damageType = DamageType.Blindness;
                qualityNeeded = component.BlindQualityNeeded;

            }
            else
            {
                qualityNeeded = component.PhysicalQualityNeeded;
                damageType = DamageType.Physical;
            }

            Log.Debug($"AAAAAAAaaaa{damageType}");
            // Verifies if it is damaged by the specific damage type
            switch (damageType)
            {
                case DamageType.Physical:
                case DamageType.Heat:
                {
                    Log.Debug($"2{damageType}");
                    if (!VerifyIfDamaged(damageable, GetDamageTypeList((DamageType) damageType)))
                    {
                        Log.Debug($"3{damageType}");
                        return;
                    }
                    break;
                }
                case DamageType.Blindness:
                {
                    if (blindcomp is { EyeDamage: 0 })
                        return;
                    break;
                }
                default:
                {
                    return;
                }
            }
            Log.Debug($"4{damageType}");


            float delay = component.DoAfterDelay;

            // Add a penalty to how long it takes if the user is repairing itself
            if (args.User == args.Target)
            {
                if (!component.AllowSelfRepair)
                    return;

                delay *= component.SelfRepairPenalty;
            }
            Log.Debug($"5{damageType}");


            // Run the repairing doafter
            args.Handled = _toolSystem.UseTool(args.Used, args.User, uid, delay, qualityNeeded, new SiliconRepairFinishedEvent
            {
                QualityNeeded = qualityNeeded,
                Delay = delay,
                DamageType = damageType
            });

        }
    }
}
