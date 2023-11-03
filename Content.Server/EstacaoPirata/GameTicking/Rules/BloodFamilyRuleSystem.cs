using System.Linq;
using System.Text;
using Content.Server.Antag;
using Content.Server.Chat.Managers;
using Content.Server.EstacaoPirata.GameTicking.Rules.Components;
using Content.Server.EstacaoPirata.Roles;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.NPC.Systems;
using Content.Server.Objectives;
using Content.Server.Radio.Components;
using Content.Server.Roles;
using Content.Server.Shuttles.Components;
using Content.Shared.CCVar;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
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
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectivesSystem = default!;
    //[Dependency] private readonly SubdermalImplantSystem _subdermalImplantSystem = default!;
    [Dependency] private readonly AntagSelectionSystem _antagSelection = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartAttemptEvent>(OnStartAttempt);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnPlayerJobAssigned);
        SubscribeLocalEvent<BloodFamilyRuleComponent, ObjectivesTextGetInfoEvent>(OnObjectivesTextGetInfo);
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
        //var numFamily = MathHelper.Clamp(component.StartCandidates.Count / component.PlayersPerFamilyMember, 1, component.MaxBloodFamily);
        var familyPool = FindPotentialFamilyMembers(component.StartCandidates, component);

        // Se a quantidade de pessoas que colocaram como preferencia Blood Family na criacao de personagem for menor que a quantidade esperada de membros da familia
        // seta a quantidade de membro esperados para a mesma de jogadores que escolheram ser membros da familia.
        // if (numFamily > familyPool.Count)
        //     numFamily = familyPool.Count;

        var numTeams = (int)Math.Ceiling((double)familyPool.Count / (double)component.MaxBloodFamily);

        var selectedFamily = PickFamilyMembers(familyPool, numTeams, component);

        //var sessionAndMind = new Dictionary<IPlayerSession, (EntityUid, MindComponent, int)>();

        // foreach (var player in selectedFamily)
        // {
        //     if (_mindSystem.TryGetMind(player, out var mindId, out var mindComponent))
        //     {
        //         sessionAndMind.Add(player, (mindId,mindComponent));
        //     }
        // }

        foreach (var player in selectedFamily)
        {
            MakeBloodFamiliar(player);
        }

        component.SelectionStatus = BloodFamilyRuleComponent.SelectionState.SelectionMade;

        // Adiciona objetivos dos membros da familia, isto é feito aqui porque é garantido que todos os membros já foram selecionados e garante
        // sincronismo com os objetivos de cada jogador
        var allFamily = GetAllBloodFamilyMembersAliveAndConnected(component);

        foreach (var member in allFamily)
        {
            GiveObjectives(member.Id, member.Mind, component);

            // TODO: implementar implante de radio DE VERDADE
            if (member.Mind.OwnedEntity != null)
            {
                GiveImplants(member.Mind.OwnedEntity.Value);
            }

            SendBloodFamilyBriefing(member.Id, member.Mind);
        }
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
            var delay = TimeSpan.FromSeconds(10);
            // var delay = TimeSpan.FromSeconds( // TimeSpan.FromSeconds(20);
            //     _cfg.GetCVar(CCVars.TraitorStartDelay) +
            //     _random.NextFloat(0f, _cfg.GetCVar(CCVars.TraitorStartDelayVariance)));

            familiar.AnnounceAt = _gameTiming.CurTime + delay;

            familiar.SelectionStatus = BloodFamilyRuleComponent.SelectionState.ReadyToSelect;
        }
    }

    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        var query = EntityQueryEnumerator<BloodFamilyRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var famComp, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            var minPlayers = famComp.MinPlayers;
            if (!ev.Forced && ev.Players.Length < minPlayers)
            {
                _chatManager.SendAdminAnnouncement(Loc.GetString("blood-family-not-enough-ready-players",
                    ("readyPlayersCount", ev.Players.Length), ("minimumPlayers", minPlayers)));
                ev.Cancel();
                continue;
            }

            if (ev.Players.Length == 0)
            {
                _chatManager.DispatchServerAnnouncement(Loc.GetString("blood-family-no-one-ready"));
                ev.Cancel();
            }
        }
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

    public Dictionary<IPlayerSession, (EntityUid, MindComponent, int)> PickFamilyMembers(List<IPlayerSession> prefList, int numberOfFamilies, BloodFamilyRuleComponent component)
    {
        var results = new Dictionary<IPlayerSession, (EntityUid, MindComponent, int)>();
        if (prefList.Count == 0)
        {
            Log.Info("Insufficient ready players to fill up with blood family members, stopping the selection.");
            return results;
        }

        foreach (var player in prefList)
        {
            if (!FilterPossiblePlayers(player))
            {
                prefList.Remove(player);
            }
        }

        var familyPool = prefList.Count;

        for (var i = 0; i < numberOfFamilies; i++)
        {
            for (var j = 0; j < component.MaxBloodFamily; j++)
            {
                var randomPlayer = _random.PickAndTake(prefList);
                if (_mindSystem.TryGetMind(randomPlayer, out var mindId, out var mindComponent))
                {
                    familyPool -= 1;
                    var newNumberOfFamilies = (int) Math.Ceiling((double) familyPool / (double) component.MaxBloodFamily);
                    results.Add(randomPlayer,(mindId, mindComponent, numberOfFamilies-newNumberOfFamilies));

                    Log.Info($"Selected a preferred blood family member for team {numberOfFamilies-newNumberOfFamilies}.");
                    if ((newNumberOfFamilies < numberOfFamilies) && results.Count > 1)
                    {
                        Log.Info($"Team {numberOfFamilies-newNumberOfFamilies} closed.");
                        break;
                    }
                }
            }
        }
        return results;
    }

    public bool MakeBloodFamiliar(KeyValuePair<IPlayerSession, (EntityUid, MindComponent,int)> playerMindAndTeam)
    {
        var bloodFamilyRuleEntity = EntityQuery<BloodFamilyRuleComponent>().FirstOrDefault();
        if (bloodFamilyRuleEntity == null)
        {
            //todo fuck me this shit is awful
            //no i wont fuck you, erp is against rules
            GameTicker.StartGameRule("BloodFamily", out var ruleEntity);
            bloodFamilyRuleEntity = Comp<BloodFamilyRuleComponent>(ruleEntity);
        }

        // Ainda a decidir se vao vir com um uplink sem TC, mas aqui ficaria o codigo de dar o uplink e dar o codigo para abrir o uplink

        // Assign blood family roles
        _roleSystem.MindAddRole(playerMindAndTeam.Value.Item1, new BloodFamilyRoleComponent
        {
            PrototypeId = bloodFamilyRuleEntity.BloodFamilyPrototypeId
        });

        bloodFamilyRuleEntity.BloodFamilyMinds.Add(playerMindAndTeam.Value.Item1);
        bloodFamilyRuleEntity.BloodFamilyTeams.Add(playerMindAndTeam.Value.Item3 ,playerMindAndTeam.Value.Item1);

        if (_mindSystem.TryGetSession(playerMindAndTeam.Value.Item1, out var session))
        {
            // Notificate player about new role assignment
            _audioSystem.PlayGlobal(bloodFamilyRuleEntity.GreetSoundNotification, session);
        }

        if (playerMindAndTeam.Value.Item2.OwnedEntity != null)
        {
            // Change the faction
            _npcFaction.RemoveFaction(playerMindAndTeam.Value.Item2.OwnedEntity.Value, "NanoTrasen", false);
            _npcFaction.AddFaction(playerMindAndTeam.Value.Item2.OwnedEntity.Value, "Syndicate");
        }
        else
        {
            Log.Error($"Couldn't change {playerMindAndTeam.Value.Item1}'s faction to Syndicate");
        }

        return true;
    }

    private bool FilterPossiblePlayers(ICommonSession player)
    {
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

        return true;
    }

    private void GiveObjectives(EntityUid mindId, MindComponent mind, BloodFamilyRuleComponent ruleComponent)
    {
        var maxDifficulty = _cfg.GetCVar(CCVars.TraitorMaxDifficulty);
        var maxPicks = ruleComponent.MaxRandomObjectives;
        var difficulty = 0f;
        for (var pick = 0; pick < maxPicks && maxDifficulty > difficulty; pick++)
        {
            var objective = _objectives.GetRandomObjective(mindId, mind, "BloodFamilyObjectiveGroups");
            if (objective == null)
                continue;

            _mindSystem.AddObjective(mindId, mind, objective.Value);
            difficulty += Comp<ObjectiveComponent>(objective.Value).Difficulty;
        }

        var familyObjective = _objectivesSystem.TryCreateObjective(mindId, mind, "FamilyAliveObjective");

        if (familyObjective != null)
        {
            _mindSystem.AddObjective(mindId, mind, familyObjective.Value);
        }
    }

    private void GiveImplants(EntityUid uid)
    {
        // Hack ferrado
        // var intrinsicRadioTransmitter = AddComp<IntrinsicRadioTransmitterComponent>(uid);
        // var activeRadio = AddComp<ActiveRadioComponent>(uid);
        // var intrinsicRadioRec = AddComp<IntrinsicRadioReceiverComponent>(uid);
        // activeRadio.Channels.Add("Syndicate");
        // intrinsicRadioTransmitter.Channels.Add("Syndicate");

        // var proto = Spawn("SyndieRadioImplant");
        // if (!TryComp<SubdermalImplantComponent>(proto, out var comp))
        // {
        //     return;
        // }
        // _subdermalImplantSystem.ForceImplant(uid, proto, comp);

        _antagSelection.GiveAntagBagGear(uid, "EncryptionKeySyndie");
    }

    public List<(EntityUid Id, MindComponent Mind)> GetOtherBloodFamilyMindsAliveAndConnected(MindComponent ourMind)
    {
        List<(EntityUid Id, MindComponent Mind)> allTraitors = new();
        foreach (var traitor in EntityQuery<BloodFamilyRuleComponent>())
        {
            foreach (var role in GetOtherBloodFamilyMindsAliveAndConnected(ourMind, traitor))
            {
                if (!allTraitors.Contains(role))
                    allTraitors.Add(role);
            }
        }

        return allTraitors;
    }

    private List<(EntityUid Id, MindComponent Mind)> GetOtherBloodFamilyMindsAliveAndConnected(MindComponent ourMind, BloodFamilyRuleComponent component)
    {
        var traitors = new List<(EntityUid Id, MindComponent Mind)>();
        foreach (var traitor in component.BloodFamilyMinds)
        {
            if (TryComp(traitor, out MindComponent? mind) &&
                mind.OwnedEntity != null &&
                mind.Session != null &&
                mind != ourMind &&
                _mobStateSystem.IsAlive(mind.OwnedEntity.Value) &&
                mind.CurrentEntity == mind.OwnedEntity)
            {
                traitors.Add((traitor, mind));
            }
        }

        return traitors;
    }

    private List<(EntityUid Id, MindComponent Mind)> GetAllBloodFamilyMembersAliveAndConnected(BloodFamilyRuleComponent component)
    {
        var traitors = new List<(EntityUid Id, MindComponent Mind)>();
        foreach (var traitor in component.BloodFamilyMinds)
        {
            if (TryComp(traitor, out MindComponent? mind) &&
                mind.OwnedEntity != null &&
                mind.Session != null &&
                _mobStateSystem.IsAlive(mind.OwnedEntity.Value) &&
                mind.CurrentEntity == mind.OwnedEntity)
            {
                traitors.Add((traitor, mind));
            }
        }

        return traitors;
    }

    private void OnObjectivesTextGetInfo(EntityUid uid, BloodFamilyRuleComponent component, ref ObjectivesTextGetInfoEvent args)
    {
        args.Minds = component.BloodFamilyMinds;
        args.AgentName = Loc.GetString("blood-family-round-end-agent-name");
    }

    /// <summary>
    /// Envia o texto de greeting e quem são os outros blood family members
    /// </summary>
    /// <param name="mind"></param>
    /// <param name="ourMind"></param>
    private void SendBloodFamilyBriefing(EntityUid mind, MindComponent ourMind)
    {
        if (!_mindSystem.TryGetSession(mind, out var session))
            return;

        var family = GetOtherBloodFamilyMindsAliveAndConnected(ourMind);

        var targetsNames = new StringBuilder("");
        int index = 0;
        foreach (var player in family)
        {
            var targetName = player.Mind.CharacterName;

            if (index > 0 && index < family.Count)
                targetsNames.Append("e ");
            targetsNames.Append(targetName);
            targetsNames.Append(' ');

            index++;
        }

        _chatManager.DispatchServerMessage(session, Loc.GetString("blood-family-role-greeting",("familyNames",targetsNames)));
    }
}
