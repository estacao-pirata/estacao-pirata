using System.Linq;
using Content.Server.GameTicking;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Customization.Systems;
using Content.Shared.Players;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Utility;

namespace Content.Server.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly CharacterRequirementsSystem _characterRequirements = default!;
    [Dependency] private readonly SharedHandsSystem _sharedHandsSystem = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    // When the player is spawned in, add all trait components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        foreach (var traitId in args.Profile.TraitPreferences)
        {
            if (!_prototype.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
            {
                DebugTools.Assert($"No trait found with ID {traitId}!");
                return;
            }

            if (!_characterRequirements.CheckRequirementsValid(
                traitPrototype.Requirements,
                _prototype.Index<JobPrototype>(args.JobId ?? _prototype.EnumeratePrototypes<JobPrototype>().First().ID),
                args.Profile, _playTimeTracking.GetTrackerTimes(args.Player), args.Player.ContentData()?.Whitelisted ?? false, traitPrototype,
                EntityManager, _prototype, _configuration,
                out _))
                continue;

            AddTrait(args.Mob, traitPrototype);
        }
    }

    /// <summary>
    ///     Adds a single Trait Prototype to an Entity.
    /// </summary>
    public void AddTrait(EntityUid uid, TraitPrototype traitPrototype)
    {
        foreach (var function in traitPrototype.Functions) {
            function.OnPlayerSpawn(uid, _componentFactory, EntityManager, _serialization);
        AddTraitGear(uid, traitPrototype);
        }
    }

    /// <summary>
    ///     Adds item required by the Trait.
    /// </summary>
    public void AddTraitGear(EntityUid uid, TraitPrototype traitPrototype)
    {
        if ((object) traitPrototype.TraitGear != null)
        {
            if (!TryComp(uid, out HandsComponent? handsComponent))
                return;

            var coords = Transform(uid).Coordinates;
            var inhandEntity = EntityManager.SpawnEntity(traitPrototype.TraitGear, coords);
            _sharedHandsSystem.TryPickup(uid, inhandEntity, checkActionBlocker: false,
                handsComp: handsComponent);
        }
    }
}
