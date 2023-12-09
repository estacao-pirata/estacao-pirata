using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Server.Time
{
    [Serializable]
    public sealed partial class ShiftChangeEvent : EntityEventArgs
    {
        public bool IsNight { get; }

        public bool IsDispatchActivated { get; }
        public SoundSpecifier Sound { get; }
        public string Message { get; }

        public Color Color { get; }
        public ShiftChangeEvent(bool isNight, bool isDispatchActivated, SoundSpecifier sound, string message, Color color)
        {
            IsNight = isNight;
            IsDispatchActivated = isDispatchActivated;
            Sound = sound;
            Message = message;
            Color = color;
        }
    }
}
