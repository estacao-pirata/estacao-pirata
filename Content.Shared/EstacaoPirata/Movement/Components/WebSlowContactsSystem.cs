using Content.Shared.Movement.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.Movement.Systems;

public sealed class WebSlowContactsSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifierSystem = default!;

    private HashSet<EntityUid> _toUpdate = new();
    private HashSet<EntityUid> _toRemove = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WebSlowContactsComponent, StartCollideEvent>(OnEntityEnter);
        SubscribeLocalEvent<WebSlowContactsComponent, EndCollideEvent>(OnEntityExit);
        SubscribeLocalEvent<WebSlowedByContactComponent, RefreshMovementSpeedModifiersEvent>(MovementSpeedCheck);

        SubscribeLocalEvent<WebSlowContactsComponent, ComponentHandleState>(OnHandleState);
        SubscribeLocalEvent<WebSlowContactsComponent, ComponentGetState>(OnGetState);

        UpdatesAfter.Add(typeof(SharedPhysicsSystem));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _toRemove.Clear();

        foreach (var ent in _toUpdate)
        {
            _speedModifierSystem.RefreshMovementSpeedModifiers(ent);
        }

        foreach (var ent in _toRemove)
        {
            RemComp<WebSlowedByContactComponent>(ent);
        }

        _toUpdate.Clear();
    }

    private void OnGetState(EntityUid uid, WebSlowContactsComponent component, ref ComponentGetState args)
    {
        args.State = new WebSlowContactsComponentState(component.WalkSpeedModifier, component.SprintSpeedModifier);
    }

    private void OnHandleState(EntityUid uid, WebSlowContactsComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not WebSlowContactsComponentState state)
            return;

        component.WalkSpeedModifier = state.WalkSpeedModifier;
        component.SprintSpeedModifier = state.SprintSpeedModifier;
    }

    private void MovementSpeedCheck(EntityUid uid, WebSlowedByContactComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (!EntityManager.TryGetComponent<PhysicsComponent>(uid, out var physicsComponent))
            return;

        var walkSpeed = 1.0f;
        var sprintSpeed = 1.0f;

        bool remove = true;
        foreach (var colliding in _physics.GetContactingEntities(physicsComponent))
        {
            var ent = colliding.Owner;
            if (!TryComp<WebSlowContactsComponent>(ent, out var slowContactsComponent))
                continue;

            walkSpeed = Math.Min(walkSpeed, slowContactsComponent.WalkSpeedModifier);
            sprintSpeed = Math.Min(sprintSpeed, slowContactsComponent.SprintSpeedModifier);
            remove = false;
        }

        args.ModifySpeed(walkSpeed, sprintSpeed);

        // no longer colliding with anything
        if (remove)
            _toRemove.Add(uid);
    }

    private void OnEntityExit(EntityUid uid, WebSlowContactsComponent component, ref EndCollideEvent args)
    {
        var otherUid = args.OtherFixture.Body.Owner;
        if (HasComp<MovementSpeedModifierComponent>(otherUid))
            _toUpdate.Add(otherUid);
    }

    private void OnEntityEnter(EntityUid uid, WebSlowContactsComponent component, ref StartCollideEvent args)
    {
        var otherUid = args.OtherFixture.Body.Owner;
        if (!HasComp<MovementSpeedModifierComponent>(otherUid))
            return;
        if (HasComp<BypassWebComponent>(otherUid))
            return;

        EnsureComp<WebSlowedByContactComponent>(otherUid);
        _toUpdate.Add(otherUid);
    }
}
