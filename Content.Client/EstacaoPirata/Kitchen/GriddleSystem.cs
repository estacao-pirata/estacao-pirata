using Content.Shared.EstacaoPirata.Kitchen.Griddle;
using Robust.Client.GameObjects;

namespace Content.Client.EstacaoPirata.Kitchen;

/// <inheritdoc/>
public sealed class GriddleSystem : SharedGriddleSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GriddleComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    // TODO: rework
    private void OnAppearanceChange(EntityUid uid, GriddleComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!args.AppearanceData.TryGetValue(GriddleComponent.GriddleVisuals.VisualState, out var visualStateObject) ||
            visualStateObject is not GriddleComponent.GriddleVisualState visualState)
        {
            visualState = GriddleComponent.GriddleVisualState.Normal;
        }

        UpdateAppearence(uid, visualState, component, args.Sprite);
    }

    // TODO: rework
    private static void UpdateAppearence(EntityUid uid, GriddleComponent.GriddleVisualState visualState, GriddleComponent component, SpriteComponent sprite)
    {
        switch (visualState)
        {
            case GriddleComponent.GriddleVisualState.Normal:
                SetLayerState(GriddleComponent.GriddleVisualLayers.Powered, component.PoweredState, sprite, false);
                break;
            case GriddleComponent.GriddleVisualState.Powered:
                SetLayerState(GriddleComponent.GriddleVisualLayers.Powered, component.PoweredState, sprite, true);
                break;
        }
    }

    // TODO: rework
    private static void SetLayerState(GriddleComponent.GriddleVisualLayers layer, string? state, SpriteComponent sprite, bool visible)
    {
        if (string.IsNullOrEmpty(state))
            return;

        sprite.LayerSetVisible(layer, visible);
        //sprite.LayerSetAutoAnimated(layer, true);
        sprite.LayerSetState(layer, state);
    }
}
