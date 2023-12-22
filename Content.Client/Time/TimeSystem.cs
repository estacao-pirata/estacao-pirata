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
        private DateTime _baseDateTime;
        private double _curTime;
        public override void Initialize()
        {
            base.Initialize();
            _curTime = 0;
            _gameTicker = _entitySystem.GetEntitySystem<ClientGameTicker>();
            _random = new Random((int) _gameTicker.RoundStartTimeSpan.TotalSeconds);
            _baseDateTime = new DateTime().AddMonths((int) _random.NextInt64(11));
        }

        public TimeSpan GetRoundDuration()
        {
            return _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
        }
        public TimeSpan GetStationTime()
        {
            return TimeSpan.FromSeconds(_curTime);
        }
        public DateTime GetStationDate()
        {
            return _baseDateTime.Add(GetStationTime());
        }

        private void SetStationTime(double time)
        {
            _curTime = time;
        }
        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            SetStationTime(GetRoundDuration().TotalSeconds * _cfg.GetCVar(CCVars.TimeScale) + _cfg.GetCVar(CCVars.InitialTime) * _cfg.GetCVar(CCVars.TimeScale));
        }
    }
}
