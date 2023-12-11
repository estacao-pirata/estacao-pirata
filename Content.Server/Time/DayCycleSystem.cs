using Content.Server.Light.EntitySystems;
using Content.Shared.CCVar;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            _currentHour = _timeSystem!.GetStationTime().Hours;
            _isNight = false;
        }
        public override void Update(float frameTime)
        {
            _nightStartTime = _cfg.GetCVar(CCVars.NightStartTime);
            _nightEndTime = TimeSpan.FromHours(_nightStartTime + _cfg.GetCVar(CCVars.NightDuration)).Hours;
            _currentHour = _timeSystem!.GetStationTime().Hours;
            if (_cfg.GetCVar(CCVars.DayNightCycle))
            {
                if ((_currentHour >= _nightStartTime || _currentHour < _nightEndTime) && !_isNight)
                {
                    _isNight = true;
                    ShiftChange(_isNight, NightAlert!, Loc.GetString("time-night-shift-announcement"), Color.SkyBlue);
                }
                else if (_currentHour < _nightStartTime && _currentHour >= _nightEndTime && _isNight)
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

        public double CalculateDayLightLevel()
        {
            var time = _timeSystem!.GetStationTime().TotalSeconds;
            var wave_lenght = Math.Max(0, _cfg.GetCVar(CCVars.LightCycleDuration)) * 24;
            var crest = Math.Max(1, _cfg.GetCVar(CCVars.MaxLight));
            var shift = Math.Max(0, _cfg.GetCVar(CCVars.MinLight));
            var amplitude = Math.Max(0, _cfg.GetCVar(CCVars.LightAmplitude));
            var exponential = Math.Max(1, _cfg.GetCVar(CCVars.ExponentialConstant));
            return CalculateCurve(time, wave_lenght, crest, shift, amplitude, 2 * exponential);
        }

        public double CalculateColorLevel(int color)
        {
            double crest = 2;
            double shift = 0;
            var exponent = 2;
            var time = _timeSystem!.GetStationTime().TotalSeconds;
            var wave_lenght = Math.Max(0, _cfg.GetCVar(CCVars.LightCycleDuration)) * 24;
            var phase = 0d;
            var amplification = 1d;
            switch (color)
            {
                case 1:
                    shift = 0.8;
                    break;
                case 2:
                    shift = 0.75;
                    crest = 1;
                    amplification = 1.35;
                    exponent = 8;
                    break;
                case 3:
                    shift = 0.65;
                    crest = 1;
                    amplification = 1.25;
                    wave_lenght /= 2;
                    phase = wave_lenght / 2;
                    break;
            }
            return Math.Min(1.5, CalculateCurve(time, wave_lenght, crest, shift, amplification, exponent, phase));
        }

        public static double CalculateCurve(double x, double wave_lenght, double crest, double shift, double amplitude, double exponent, double phase = 0)
        {
            var sen = Math.Pow(Math.Sin((Math.PI * (phase + x)) / wave_lenght), exponent);
            var function = amplitude * (Math.Pow(crest - shift + 1, sen) - 1) + shift;
            return function;
        }
    }
}
