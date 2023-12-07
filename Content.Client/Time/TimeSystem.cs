using Robust.Shared.Timing;
using Content.Client.GameTicking.Managers;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;

namespace Content.Client.Time
{
    public sealed partial class TimeSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        private Random _random = default!;
        private ClientGameTicker _gameTicker = default!;
        private TimeSpan _stationTime;

        private DateTime _baseDateTime;
        private int _initialTime;
        private int _timeAcceleration;

        public override void Initialize()
        {
            IoCManager.InjectDependencies(this);
            _gameTicker = _entitySystem.GetEntitySystem<ClientGameTicker>();
            _stationTime = _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
            _initialTime = _cfg.GetCVar(CCVars.InitialTime);
            _timeAcceleration = _cfg.GetCVar(CCVars.TimeAcceleration);
            _random = new Random((int)_gameTicker.RoundStartTimeSpan.TotalSeconds);
            _baseDateTime = new DateTime().AddMonths((int)_random.NextInt64(11));
        }
        public TimeSpan GetStationSyncTime()
        {
            return _stationTime;
        }
        public TimeSpan GetStationRelativeTime()
        {
            return TimeSpan.FromSeconds(_initialTime + (GetStationSyncTime().TotalSeconds * _timeAcceleration));
        }
        public DateTime GetStationDate()
        {
            return _baseDateTime.Add(GetStationRelativeTime());
        }
        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            _initialTime = _cfg.GetCVar(CCVars.InitialTime);
            _timeAcceleration = _cfg.GetCVar(CCVars.TimeAcceleration);
            _random = new Random((int)_gameTicker.RoundStartTimeSpan.TotalSeconds);
            _baseDateTime = new DateTime().AddMonths((int)_random.NextInt64(11));
            _stationTime = _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
        }
    }
}