using Content.Server.Chat.Systems;
using Content.Server.Light.EntitySystems;
using Content.Server.Station.Components;
using Content.Shared.CCVar;
using Robust.Shared.Timing;
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
        [Dependency] private readonly IGameTiming _timing = default!;

        private TimeSystem? _timeSystem;
        private PoweredLightSystem? _lightSystem;
        private int _currentHour;
        private double _deltaTime;
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
            _lightSystem = _entitySystem.GetEntitySystem<PoweredLightSystem>();
            _currentHour = _timeSystem!.GetStationTime().Hours;
            _deltaTime = 0;
            _isNight = false;
            _mapColor = new Dictionary<int, int[]>();
        }
        public override void Update(float frameTime)
        {
            if (_deltaTime >= 1.0)
            {
                _deltaTime = 0;
                _currentHour = _timeSystem!.GetStationTime().Hours;
                // Itera sobre as estações com o componente de dia e noite. Esse componente deve ser adicionado na grid para funcionar nas lâmpadas.
                foreach (var (comp, station) in EntityQuery<DayCycleComponent, StationMemberComponent>())
                {
                    if (station != null && comp.isEnabled)
                    {
                        // Trecho do código responsável pelos alertas de dia e noite
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
                        }
                        // Aqui é calculado as curvas individuais de iluminação, que são repassadas pra uma instância da classe PoweredLightSystem.
                        var red = 1.0;
                        var green = 1.0;
                        var blue = 1.0;
                        if (comp.IsColorEnabled)
                        {
                            red = CalculateColorLevel(comp, 1);
                            green = CalculateColorLevel(comp, 2);
                            blue = CalculateColorLevel(comp, 3);
                        }
                        _lightSystem!.ChangeLights(Math.Min(comp.LightClip, CalculateLightLevel(comp)), new double[] { red, green, blue }, comp);
                    }
                }
                // Itera sobre mapas com o componente de dia e noite. Deve ser adicionado no mapa para funcionar adequadamente com o MapLight.
                foreach (var (comp, map) in EntityQuery<DayCycleComponent, MapLightComponent>())
                {
                    if (comp.isEnabled)
                    {
                        // Um dicionário para manter as cores originais do MapLight sem precisar acessar diretamente. É necessário separar esses valores.
                        if (!_mapColor!.ContainsKey(map.Owner.Id))
                        {
                            Color color = map.AmbientLightColor;
                            _mapColor.Add(map.Owner.Id, new int[] { color.RByte, color.GByte, color.BByte });
                        }
                        else
                        {
                            // Calcula as curvas individualmente.
                            var lightLevel = Math.Min(comp.LightClip, CalculateLightLevel(comp));
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
            else
            {
                _deltaTime += frameTime;
            }
        }

        // Calcula a curva da intensidade da iluminação, é o "dimming" das luzes em função do tempo.
        public double CalculateLightLevel(DayCycleComponent comp)
        {
            var time = _timeSystem!.GetStationTime().TotalSeconds;
            var wave_lenght = Math.Max(0, comp.CycleDuration) * 24;
            var crest = Math.Max(1, comp.PeakLightLevel);
            var shift = Math.Max(0, comp.BaseLightLevel);
            return CalculateCurve(time, wave_lenght, crest, shift, 6);
        }


        // Calcula a curva de cada cor, é o que determina a cor das luzes em função do tempo. 1 = Vermelho, 2 = Verde, 3 = Azul.
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
                    shift = 0.775;
                    exponent = 4;
                    break;
                case 2:
                    crest = 1.85;
                    shift = 0.775;
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

        /* Função matemática para gerar uma onda períodica que simula a transição da iluminação de uma estrela em função do tempo.
           x: é a varíavel independente, correspondente ao tempo, e o resultado em y é um valor oscilante que representa a iluminação.
           wave_lenght: correspondente ao comprimento de onda, ou seja, a duração do dia (em segundos).
           crest: corresponde a crista da onda, ou seja, seu valor máximo, o vértice da função em y. Ele é compensado em relação ao deslocamento vertical da função.
           shift: corresponde ao deslocamento vertical da função, ou seja, seu valor mínimo.
           exponent: é o grau do seno, quanto maior o valor mais "achatado" e "longo" é o vale da curva, enquanto a crista se torna mais "curta" e "íngreme".
           phase: ajusta a fase da função, na prática, serve pra transformar o seno em cosseno quando necessário.*/
        public static double CalculateCurve(double x, double wave_lenght, double crest, double shift, double exponent, double phase = 0)
        {
            var sen = Math.Pow(Math.Sin((Math.PI * (phase + x)) / wave_lenght), exponent);
            return ((crest - shift) * sen) + shift;
        }
    }
}
