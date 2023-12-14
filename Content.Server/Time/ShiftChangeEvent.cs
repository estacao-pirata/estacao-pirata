using Robust.Shared.Audio;

namespace Content.Server.Time
{
    [Serializable]
    public sealed partial class ShiftChangeEvent : EntityEventArgs
    {
        public bool IsNight { get; }
        public ShiftChangeEvent(bool isNight)
        {
            IsNight = isNight;
        }
    }
}
