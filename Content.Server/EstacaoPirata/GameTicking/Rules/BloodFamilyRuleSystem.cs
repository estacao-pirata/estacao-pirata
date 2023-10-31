using System.Linq;
using Content.Server.EstacaoPirata.GameTicking.Rules.Components;
using Content.Server.EstacaoPirata.Roles;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.NPC.Systems;
using Content.Server.Objectives;
using Content.Server.Roles;
using Content.Server.Shuttles.Components;
using Content.Shared.CCVar;
using Content.Shared.Objectives.Components;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Players;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.EstacaoPirata.GameTicking.Rules;

public sealed class BloodFamilyRuleSystem : GameRuleSystem<BloodFamilyRuleComponent>
{

    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!; // Usar em MakeBloodFamiliar() // TODO: criar um prototipo de objetivos pra familia
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartAttemptEvent>(OnStartAttempt);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnPlayerJobAssigned);
        // Vai ter late join?
    }

    protected override void ActiveTick(EntityUid uid, BloodFamilyRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.SelectionStatus == BloodFamilyRuleComponent.SelectionState.ReadyToSelect && _gameTiming.CurTime > component.AnnounceAt)
            DoBloodFamilyStart(component);
    }

    /// <summary>
    ///  Vai achar todos os possiveis familiares e chamar o MakeBloodFamiliar
    /// </summary>
    /// <param name="component"></param>
    private void DoBloodFamilyStart(BloodFamilyRuleComponent component)
    {
        Log.Debug("DoBloodFamilyStart chamado");
        if (component.StartCandidates.Count < component.MinBloodFamily) // Setar os start candidates no OnPlayerJobsAssigned vulgo OnPlayerSpawn
        {
            Log.Error("Tried to start Blood Family mode without enough candidates.");
            return;
        }

        var numFamily = MathHelper.Clamp(component.StartCandidates.Count / component.PlayersPerFamilyMember, 1, component.MaxBloodFamily);
        var familyPool = FindPotentialFamilyMembers(component.StartCandidates, component);
        var selectedFamily = PickFamilyMembers(numFamily, familyPool);

        foreach (var player in selectedFamily)
        {
            MakeBloodFamiliar(player);
        }

        // Adicionar game rule de traitor comum tbm
    }

    private void OnPlayerJobAssigned(RulePlayerJobsAssignedEvent ev)
    {
        var query = EntityQueryEnumerator<BloodFamilyRuleComponent, GameRuleComponent>(); // Faz a query da entidade que cuida do gamerule (tem so uma entidade eu acho)
        // Para testar usar o forcepreset
        while (query.MoveNext(out var uid, out var familiar, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;
            foreach (var player in ev.Players)
            {
                if (!ev.Profiles.ContainsKey(player.UserId))
                    continue;

                // Aqui pega-se cada jogador que deu ready e esta na partida

                familiar.StartCandidates[player] = ev.Profiles[player.UserId];
            }

            // Vai usar o delay do traitor por enquanto mesmo
            var delay = TimeSpan.FromSeconds(0);
            //     _cfg.GetCVar(CCVars.TraitorStartDelay) +
            //     _random.NextFloat(0f, _cfg.GetCVar(CCVars.TraitorStartDelayVariance)));

            familiar.AnnounceAt = _gameTiming.CurTime + delay;

            familiar.SelectionStatus = BloodFamilyRuleComponent.SelectionState.ReadyToSelect;
        }
    }

    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        Log.Debug("OnStartAttempt chamado");
        //throw new NotImplementedException();
    }

    public List<IPlayerSession> FindPotentialFamilyMembers(in Dictionary<IPlayerSession, HumanoidCharacterProfile> candidates, BloodFamilyRuleComponent component)
    {
        var list = new List<IPlayerSession>();
        var pendingQuery = GetEntityQuery<PendingClockInComponent>();

        foreach (var player in candidates.Keys)
        {
            // Role prevents antag.
            if (!_jobs.CanBeAntag(player))
            {
                continue;
            }

            // Latejoin
            if (player.AttachedEntity != null && pendingQuery.HasComponent(player.AttachedEntity.Value))
                continue;

            list.Add(player);
        }

        var prefList = new List<IPlayerSession>();

        foreach (var player in list)
        {
            var profile = candidates[player];
            if (profile.AntagPreferences.Contains(component.BloodFamilyPrototypeId))
            {
                prefList.Add(player);
            }
        }
        if (prefList.Count == 0)
        {
            Log.Info("Insufficient preferred blood family members, picking at random.");
            prefList = list;
        }
        return prefList;
    }

    public List<IPlayerSession> PickFamilyMembers(int familyMembersCount, List<IPlayerSession> prefList)
    {
        var results = new List<IPlayerSession>(familyMembersCount);
        if (prefList.Count == 0)
        {
            Log.Info("Insufficient ready players to fill up with blood family members, stopping the selection.");
            return results;
        }

        for (var i = 0; i < familyMembersCount; i++)
        {
            results.Add(_random.PickAndTake(prefList));
            Log.Info("Selected a preferred blood family member.");
        }
        return results;
    }

    public bool MakeBloodFamiliar(ICommonSession player)
    {
        var bloodFamilyRuleEntity = EntityQuery<BloodFamilyRuleComponent>().FirstOrDefault();
        if (bloodFamilyRuleEntity == null)
        {
            //todo fuck me this shit is awful
            //no i wont fuck you, erp is against rules
            GameTicker.StartGameRule("BloodFamily", out var ruleEntity);
            bloodFamilyRuleEntity = Comp<BloodFamilyRuleComponent>(ruleEntity);
        }

        if (!_mindSystem.TryGetMind(player, out var mindId, out var mind))
        {
            Log.Info("Failed getting mind for picked blood family member.");
            return false;
        }

        if (HasComp<TraitorRoleComponent>(mindId))
        {
            Log.Error($"Player {player.Name} is already a traitor.");
            return false;
        }

        if (HasComp<BloodFamilyRoleComponent>(mindId))
        {
            Log.Error($"Player {player.Name} is already a blood family member.");
            return false;
        }

        if (mind.OwnedEntity is not { } entity)
        {
            Log.Error("Mind picked for blood family member did not have an attached entity.");
            return false;
        }

        // Ainda a decidir se vao vir com um uplink sem TC, mas aqui ficaria o codigo de dar o uplink e dar o codigo para abrir o uplink

        // Prepare blood family role
        // var bloodFamilyRole = new BloodFamilyRoleComponent
        // {
        //     PrototypeId = bloodFamilyRuleEntity.BloodFamilyPrototypeId,
        // };

        // Assign blood family roles
        _roleSystem.MindAddRole(mindId, new BloodFamilyRoleComponent
        {
            PrototypeId = bloodFamilyRuleEntity.BloodFamilyPrototypeId
        });

        bloodFamilyRuleEntity.BloodFamilyMinds.Add(mindId);

        if (_mindSystem.TryGetSession(mindId, out var session))
        {
            // Notificate player about new role assignment
            _audioSystem.PlayGlobal(bloodFamilyRuleEntity.GreetSoundNotification, session);
        }

        // Change the faction
        _npcFaction.RemoveFaction(entity, "NanoTrasen", false);
        _npcFaction.AddFaction(entity, "Syndicate");

        // Give random objectives
        var maxDifficulty = _cfg.GetCVar(CCVars.TraitorMaxDifficulty);
        var maxPicks = bloodFamilyRuleEntity.MaxRandomObjectives;
        var difficulty = 0f;
        for (var pick = 0; pick < maxPicks && maxDifficulty > difficulty; pick++)
        {
            var objective = _objectives.GetRandomObjective(mindId, mind, "TraitorObjectiveGroups");
            if (objective == null)
                continue;

            _mindSystem.AddObjective(mindId, mind, objective.Value);
            difficulty += Comp<ObjectiveComponent>(objective.Value).Difficulty;
        }

        // Give keep family alive objective


        return true;
    }

    /// <summary>
    /// Envia o texto de greeting e quem sao os outros blood family members
    /// </summary>
    /// <param name="mind"></param>
    private void SendBloodFamilyBriefing(EntityUid mind)
    {

    }
}
