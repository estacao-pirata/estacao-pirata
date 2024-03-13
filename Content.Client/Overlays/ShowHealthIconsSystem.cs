using Content.Client.Atmos.Rotting;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Damage;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Overlays;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Client.Overlays;

/// <summary>
/// Shows a healthy icon on mobs.
/// </summary>
public sealed class ShowHealthIconsSystem : EquipmentHudSystem<ShowHealthIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeMan = default!;

    public HashSet<string> DamageContainers = new();

    [ValidatePrototypeId<StatusIconPrototype>]
    private const string HealthIconFine = "HealthIconFine";

    [ValidatePrototypeId<StatusIconPrototype>]
    private const string HealthIconCritical = "HealthIconCritical";

    [ValidatePrototypeId<StatusIconPrototype>]
    private const string HealthIconDead = "HealthIconDead";

    [ValidatePrototypeId<StatusIconPrototype>]
    private const string HealthIconDecomposing = "HealthIconDecomposing";

    private readonly Dictionary<MobState, string> _stateIcons = new()
    {
        { MobState.Alive, HealthIconFine },
        { MobState.Critical, HealthIconCritical },
        { MobState.Dead, HealthIconDead }
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageableComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);

    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<ShowHealthIconsComponent> component)
    {
        base.UpdateInternal(component);

        foreach (var damageContainerId in component.Components.SelectMany(x => x.DamageContainers))
        {
            DamageContainers.Add(damageContainerId);
        }
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        DamageContainers.Clear();
    }

    private void OnGetStatusIconsEvent(Entity<DamageableComponent> entity, ref GetStatusIconsEvent args)
    {
        if (!IsActive || args.InContainer || args.HasStealthComponent)
            return;

        var healthIcons = DecideHealthIcons(entity);

        args.StatusIcons.AddRange(healthIcons);
    }

    private IReadOnlyList<StatusIconPrototype> DecideHealthIcons(Entity<DamageableComponent> entity)
    {
        var damageableComponent = entity.Comp;

        if (damageableComponent.DamageContainerID == null ||
            !DamageContainers.Contains(damageableComponent.DamageContainerID))
        {
            return Array.Empty<StatusIconPrototype>();
        }

        var result = new List<StatusIconPrototype>();

        // Here you could check health status, diseases, mind status, etc. and pick a good icon, or multiple depending on whatever.
        if (damageableComponent?.DamageContainerID == "Biological")
        {
            if (TryComp<MobStateComponent>(entity, out var state))
            {
                // Since there is no MobState for a rotting mob, we have to deal with this case first.
                if (HasComp<RottingComponent>(entity) && _prototypeMan.TryIndex<StatusIconPrototype>(HealthIconDecomposing, out var rottingIcon))
                {
                    result.Add(rottingIcon);
                }
                else if (_stateIcons.TryGetValue(state.CurrentState, out var value) && _prototypeMan.TryIndex<StatusIconPrototype>(value, out var icon))
                    result.Add(icon);
            }
        }

        return result;
    }
}
