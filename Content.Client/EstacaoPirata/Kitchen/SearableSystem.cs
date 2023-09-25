using System.Numerics;
using Content.Shared.EstacaoPirata.Kitchen;
using Content.Shared.EstacaoPirata.Kitchen.Griddle;
using Content.Shared.Popups;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client.EstacaoPirata.Kitchen;

/// <inheritdoc/>
public sealed class SearableSystem : SharedSearableSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        //base.Initialize();

        SubscribeNetworkEvent<AboveHotSurface>(OnAboveHotSurface);
    }

    private void OnAboveHotSurface(AboveHotSurface ev)
    {
    }

    private static readonly Animation SearingSmokeAnimation = new()
    {
        Length = TimeSpan.FromSeconds(0.8f),
        AnimationTracks =
        {
            new AnimationTrackComponentProperty()
            {
                ComponentType = typeof(SpriteComponent),
                Property = nameof(SpriteComponent.Offset),
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                    new AnimationTrackProperty.KeyFrame(new Vector2(0f, -0.3f), 0.3f),
                    new AnimationTrackProperty.KeyFrame(new Vector2(0f, 20f), 0.5f),
                }
            }
        }
    };
}
