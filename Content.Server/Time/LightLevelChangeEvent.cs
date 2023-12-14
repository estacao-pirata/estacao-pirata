using Robust.Shared.Audio;

namespace Content.Server.Time
{
    [Serializable]
    public sealed partial class LightLevelChangeEvent : EntityEventArgs
    {
        public double LightLevel { get; }
        public double[] ColorLevel { get; }

        public EntityUid Entity { get; }

        public LightLevelChangeEvent(double lightLevel, double[] colorLevel, EntityUid entity)
        {
            LightLevel = lightLevel;
            ColorLevel = colorLevel;
            Entity = entity;
        }
    }
}
