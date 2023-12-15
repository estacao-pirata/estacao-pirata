using Content.Server.Chat.Systems;
using Content.Server.Station.Components;
using Content.Shared.CCVar;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;

namespace Content.Server.Time
{
    public sealed partial class DayCycleSystem : EntitySystem
    {
        [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        private TimeSystem? _timeSystem;
        private int _currentHour;
        private double _lightLevel;
        private bool _isNight;
        private Dictionary<int, int[]>? _mapColor;
        public static SoundSpecifier? NightAlert;
        public static SoundSpecifier? DayAlert;
        public override void Initialize()
        {
            base.Initialize();
            NightAlert = new SoundPathSpecifier("/Audio/Announcements/nightshift.ogg");
            DayAlert = new SoundPathSpecifier("/Audio/Announcements/dayshift.ogg");
            _timeSystem = _entitySystem.GetEntitySystem<TimeSystem>();
            _currentHour = _timeSystem!.GetStationTime().Hours;
            _lightLevel = 1;
            _isNight = false;
            _mapColor = new Dictionary<int, int[]>();
        }
        public override void Update(float frameTime)
        {
            _currentHour = _timeSystem!.GetStationTime().Hours;
            foreach (var (comp, station) in EntityQuery<DayCycleComponent, StationMemberComponent>())
            {
                if (station != null && comp.isEnabled)
                {
                    if ((_currentHour >= comp.NightStartTime || _currentHour < TimeSpan.FromHours(comp.NightStartTime + comp.NightDuration).Hours) && !_isNight)
                    {
                        if (comp.IsAnnouncementEnabled)
                        {
                            _chatSystem.DispatchStationAnnouncement(station.Owner,
                            Loc.GetString("time-night-shift-announcement"),
                            Loc.GetString("comms-console-announcement-title-centcom"),
                            true, NightAlert, colorOverride: Color.SkyBlue);
                        }
                        _isNight = true;
                        ShiftChange(_isNight);
                    }
                    else if (_currentHour >= TimeSpan.FromHours(comp.NightStartTime + comp.NightDuration).Hours && _currentHour < comp.NightStartTime && _isNight)
                    {
                        if (comp.IsAnnouncementEnabled)
                        {
                            _chatSystem.DispatchStationAnnouncement(station.Owner,
                            Loc.GetString("time-day-shift-announcement"),
                            Loc.GetString("comms-console-announcement-title-centcom"),
                            true, DayAlert, colorOverride: Color.OrangeRed);
                        }
                        _isNight = false;
                        ShiftChange(_isNight);
                    }
                    var lightLevel = CalculateDayLightLevel(comp);
                    if (Math.Abs(lightLevel - _lightLevel) >= _cfg.GetCVar(CCVars.DeltaAdjust))
                    {
                        _lightLevel = lightLevel;
                        var red = 1.0;
                        var green = 1.0;
                        var blue = 1.0;
                        if (comp.IsColorEnabled)
                        {
                            red = CalculateColorLevel(comp, 1);
                            green = CalculateColorLevel(comp, 2);
                            blue = CalculateColorLevel(comp, 3);
                        }
                        var ev = new LightLevelChangeEvent(lightLevel, new double[] { red, green, blue }, station.Owner);
                        RaiseLocalEvent(ev);
                    }
                }
            }
            foreach (var (comp, map) in EntityQuery<DayCycleComponent, MapLightComponent>())
            {
                if (comp.isEnabled)
                {
                    if (!_mapColor!.ContainsKey(map.Owner.Id))
                    {
                        Color color = map.AmbientLightColor;
                        _mapColor.Add(map.Owner.Id, new int[] { color.RByte, color.GByte, color.BByte });
                    }
                    else
                    {
                        var lightLevel = CalculateDayLightLevel(comp);
                        if (Math.Abs(lightLevel - _lightLevel) >= _cfg.GetCVar(CCVars.DeltaAdjust))
                        {
                            var red = (int) Math.Min(_mapColor[map.Owner.Id][0], _mapColor[map.Owner.Id][0] * lightLevel);
                            var green = (int) Math.Min(_mapColor[map.Owner.Id][1], _mapColor[map.Owner.Id][1] * lightLevel);
                            var blue = (int) Math.Min(_mapColor[map.Owner.Id][2], _mapColor[map.Owner.Id][2] * lightLevel);
                            if (comp.IsColorEnabled)
                            {
                                red = (int) Math.Min(_mapColor[map.Owner.Id][0], red * CalculateColorLevel(comp, 1));
                                green = (int) Math.Min(_mapColor[map.Owner.Id][1], green * CalculateColorLevel(comp, 2));
                                blue = (int) Math.Min(_mapColor[map.Owner.Id][2], blue * CalculateColorLevel(comp, 3));
                            }
                            map.AmbientLightColor = System.Drawing.Color.FromArgb(red, green, blue);
                            Dirty(map.Owner, map);
                    }
                    }
                }

            }
        }

        private void ShiftChange(bool isNight)
        {
            var ev = new ShiftChangeEvent(isNight);
            RaiseLocalEvent(ev);
        }

        public double CalculateDayLightLevel(DayCycleComponent comp)
        {
            var time = _timeSystem!.GetStationTime().TotalSeconds;
            var wave_lenght = Math.Max(0, comp.CycleDuration) * 24;
            var crest = Math.Max(1, comp.PeakLightLevel);
            var shift = Math.Max(0, comp.BaseLightLevel);
            var exponential = 6;
            return CalculateCurve(time, wave_lenght, crest, shift, 2 * exponential);
        }
        public double CalculateColorLevel(DayCycleComponent comp, int color)
        {
            var crest = 1.0;
            var shift = 1.0;
            var exponent = 2.0;
            var time = _timeSystem!.GetStationTime().TotalSeconds;
            var wave_lenght = Math.Max(0, comp.CycleDuration) * 24;
            var phase = 0d;
            switch (color)
            {
                case 1:
                    crest = 1.65;
                    shift = 0.7;
                    exponent = 4;
                    break;
                case 2:
                    crest = 1.9;
                    shift = 0.7;
                    exponent = 8;
                    break;
                case 3:
                    crest = 3.75;
                    shift = 0.685;
                    exponent = 2;
                    wave_lenght /= 2;
                    phase = wave_lenght / 2;
                    break;
            }
            return CalculateCurve(time, wave_lenght, crest, shift, exponent, phase);
        }

        public static double CalculateCurve(double x, double wave_lenght, double crest, double shift, double exponent, double phase = 0)
        {
            var sen = Math.Pow(Math.Sin((Math.PI * (phase + x)) / wave_lenght), exponent);
            return ((crest - shift) * sen) + shift;
        }
    }
}
