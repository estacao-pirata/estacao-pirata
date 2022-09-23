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
///     Fer Laughin
/// </summary>
/// <remarks>
///     Or I guess other vocalizations, like laughing. If fun is ever legalized on the station.
/// </remarks>
public sealed class LaughSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LaughComponent, LaughActionEvent>(OnActionPerform);
        SubscribeLocalEvent<LaughComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<LaughComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, LaughComponent component, ComponentStartup args)
    {
        if (component.LaughAction == null
            && _proto.TryIndex(component.ActionId, out InstantActionPrototype? act))
        {
            component.LaughAction = new(act);
        }

        if (component.LaughAction != null)
            _actions.AddAction(uid, component.LaughAction, null);
    }

    private void OnShutdown(EntityUid uid, LaughComponent component, ComponentShutdown args)
    {
        if (component.LaughAction != null)
            _actions.RemoveAction(uid, component.LaughAction);
    }

    private void OnActionPerform(EntityUid uid, LaughComponent component, LaughActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryLaugh(uid, component);
    }

    public bool TryLaugh(EntityUid uid, LaughComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        if (!_blocker.CanSpeak(uid))
            return false;

        var sex = Sex.Male; //the default is male because requiring humanoid appearance for this is dogshit
        if (TryComp(uid, out HumanoidComponent? humanoid))
            sex = humanoid.Sex;

        var scale = (float) _random.NextGaussian(1, LaughComponent.Variation);
        var pitchedParams = component.AudioParams.WithPitchScale(scale);

        switch (sex)
        {
            case Sex.Male:
                SoundSystem.Play(component.MaleLaugh.GetSound(), Filter.Pvs(uid), uid, pitchedParams);
                break;
            case Sex.Female:
                SoundSystem.Play(component.FemaleLaugh.GetSound(), Filter.Pvs(uid), uid, pitchedParams);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return true;
    }
}