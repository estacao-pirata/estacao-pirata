using Content.Shared.EstacaoPirata.Kitchen.Griddle;
using Content.Shared.Popups;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared.EstacaoPirata.Kitchen;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedSearableSystem : EntitySystem
{
    [Dependency] private readonly ISerializationManager _serManager = default!;
    [Dependency] private readonly AnimationPlayerSystem _player = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        //SubscribeNetworkEvent<GriddleComponent.BeingGriddledEvent>(OnBeingGriddled);
        //SubscribeLocalEvent<AboveHotSurface>(OnHotSurface);
    }

    // private void OnHotSurface(AboveHotSurface args)
    // {
    //     Log.Debug("Teste onhotsurface");
    // }

    private void OnBeingGriddled(GriddleComponent.BeingGriddledEvent ev)
    {
        if (ev.Occupant == null)
            return;

        _popup.PopupClient("Subiu no griddle", ev.Occupant.Value, ev.Occupant.Value);
    }

    /// <summary>
    /// Tells clients to play the searing smoke animation.
    /// </summary>
    [Serializable, NetSerializable]
    protected sealed class SearingSmokeAnimationMessage : EntityEventArgs
    {
        public EntityUid Entity;
        public EntityCoordinates Coordinates;
    }
}
