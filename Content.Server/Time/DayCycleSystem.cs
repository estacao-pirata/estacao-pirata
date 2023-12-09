using Content.Shared.CCVar;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;

namespace Content.Server.Time
{
     public sealed partial class DayCycleSystem : EntitySystem
     {
        [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        private TimeSystem? _timeSystem;
        private int _nightStartTime;
        private int _nightEndTime;
        private int _currentHour;
        private bool _isNight;
        public static SoundSpecifier? NightAlert;
        public static SoundSpecifier? DayAlert;
        public override void Initialize()
        {
            base.Initialize();
            NightAlert = new SoundPathSpecifier("/Audio/Announcements/nightshift.ogg");
            DayAlert = new SoundPathSpecifier("/Audio/Announcements/dayshift.ogg");
            _timeSystem = _entitySystem.GetEntitySystem<TimeSystem>();
            _isNight = false;
        }
        public override void Update(float frameTime)
        {
            _nightStartTime = _cfg.GetCVar(CCVars.NightStartTime);
            _nightEndTime = TimeSpan.FromHours(_nightStartTime + _cfg.GetCVar(CCVars.NightDuration)).Hours;
            _currentHour = _timeSystem!.GetStationTime().Hours;
            if (_cfg.GetCVar(CCVars.DayNightCycle))
            {
                if ((_currentHour >= _nightStartTime || _currentHour <= _nightEndTime) && !_isNight)
                {
                    _isNight = true;
                    ShiftChange(_isNight, NightAlert!, Loc.GetString("time-night-shift-announcement"), Color.SkyBlue);
                }
                else if (_currentHour < _nightStartTime && _currentHour > _nightEndTime && _isNight)
                {
                    _isNight = false;
                    ShiftChange(_isNight, DayAlert!, Loc.GetString("time-day-shift-announcement"), Color.OrangeRed);
                }
            }
        }

        private void ShiftChange(bool isNight, SoundSpecifier alertSound, string message, Color color)
        {
            var ev = new ShiftChangeEvent(isNight, _cfg.GetCVar(CCVars.ShiftAnnouncement), alertSound, message, color);
            RaiseLocalEvent(ev);
        }
    }
}
