using Content.Server.Humanoid;
using Content.Server.Speech.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Humanoid;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Speech;

/// <summary>
///     Fer Fartin
/// </summary>
/// <remarks>
///     Or I guess other vocalizations, like laughing. If fun is ever legalized on the station.
/// </remarks>
public sealed class FartSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FartComponent, FartActionEvent>(OnActionPerform);
        SubscribeLocalEvent<FartComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FartComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, FartComponent component, ComponentStartup args)
    {
        if (component.FartAction == null
            && _proto.TryIndex(component.ActionId, out InstantActionPrototype? act))
        {
            component.FartAction = new(act);
        }

        if (component.FartAction != null)
            _actions.AddAction(uid, component.FartAction, null);
    }

    private void OnShutdown(EntityUid uid, FartComponent component, ComponentShutdown args)
    {
        if (component.FartAction != null)
            _actions.RemoveAction(uid, component.FartAction);
    }

    private void OnActionPerform(EntityUid uid, FartComponent component, FartActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryFart(uid, component);
    }

    public bool TryFart(EntityUid uid, FartComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        if (!_blocker.CanSpeak(uid))
            return false;

        var sex = Sex.Male; //the default is male because requiring humanoid appearance for this is dogshit
        if (TryComp(uid, out HumanoidComponent? humanoid))
            sex = humanoid.Sex;

        var scale = (float) _random.NextGaussian(1, FartComponent.Variation);
        var pitchedParams = component.AudioParams.WithPitchScale(scale);

        SoundSystem.Play(component.Fart.GetSound(), Filter.Pvs(uid), uid, pitchedParams);

        return true;
    }
}

