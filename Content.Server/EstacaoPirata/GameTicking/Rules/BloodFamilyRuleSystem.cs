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
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.EstacaoPirata.GameTicking.Rules;

public sealed class BloodFamilyRuleSystem : GameRuleSystem<BloodFamilyRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
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
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLatejoin);
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
        if (component.StartCandidates.Count < component.MinBloodFamily) // Setar os start candidates no OnPlayerJobsAssigned vulgo OnPlayerSpawn
        {
            component.SelectionStatus = BloodFamilyRuleComponent.SelectionState.Error;
            Log.Error("Tried to start Blood Family mode without enough candidates.");
            return;
        }

        var familyPool = FindPotentialFamilyMembers(component.StartCandidates, component);

        if (familyPool.Count < component.MinBloodFamily)
        {
            component.SelectionStatus = BloodFamilyRuleComponent.SelectionState.Error;
            Log.Error("Tried to start Blood Family mode without enough candidates.");
            return;
        }

        var numTeams = (int)Math.Ceiling((double)familyPool.Count / (double)component.MaxBloodFamily);

        var selectedFamily = PickFamilyMembers(familyPool, component);

        foreach (var player in selectedFamily)
        {
            MakeBloodFamiliar(player);
        }

        component.SelectionStatus = BloodFamilyRuleComponent.SelectionState.SelectionMade;

        // Adiciona objetivos dos membros da familia, isto é feito aqui porque é garantido que todos os membros já foram selecionados e garante
        // sincronia com os objetivos de cada jogador
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
            // var delay = TimeSpan.FromSeconds(10);
            var delay = TimeSpan.FromSeconds(
                _cfg.GetCVar(CCVars.TraitorStartDelay) +
                _random.NextFloat(0f, _cfg.GetCVar(CCVars.TraitorStartDelayVariance)));

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


    private void HandleLatejoin(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<BloodFamilyRuleComponent, GameRuleComponent>();

        while (query.MoveNext(out var uid, out var bloodFamily, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            if (bloodFamily.BloodFamilyMinds.Count >= bloodFamily.MaxPlayers)
                continue;
            if (!ev.LateJoin)
                continue;
            if (!ev.Profile.AntagPreferences.Contains(bloodFamily.BloodFamilyPrototypeId))
                continue;

            if (ev.JobId == null || !_prototypeManager.TryIndex<JobPrototype>(ev.JobId, out var job))
                continue;

            if (!job.CanBeAntag)
                continue;

            // Before the announcement is made, late-joiners are considered the same as players who readied.
            if (bloodFamily.SelectionStatus < BloodFamilyRuleComponent.SelectionState.SelectionMade)
            {
                bloodFamily.StartCandidates[ev.Player] = ev.Profile;
                continue;
            }

            // the nth player we adjust our probabilities around
            var target = bloodFamily.PlayersPerFamilyMember * bloodFamily.BloodFamilyMinds.Count + 1;

            var chance = 1f / bloodFamily.PlayersPerFamilyMember;

            // If we have too many traitors, divide by how many players below target for next traitor we are.
            if (ev.JoinOrder < target)
            {
                chance /= (target - ev.JoinOrder);
            }
            else // Tick up towards 100% chance.
            {
                chance *= ((ev.JoinOrder + 1) - target);
            }

            if (chance > 1)
                chance = 1;

            // Now that we've calculated our chance, roll and make them a traitor if we roll under.
            // You get one shot.
            if (_random.Prob(chance))
            {
                /*
                 * Chance de 50% para ou criar uma familia nova com 2 pessoas, ou com 3
                 */
                var probabilidade = _random.Prob(0.5f);
                var minBloodFamily = 0;

                if (probabilidade)
                {
                    minBloodFamily = bloodFamily.MinBloodFamily;
                }
                else
                {
                    minBloodFamily = bloodFamily.MinBloodFamily - 1;
                }

                /* Se a quantidade de pessoas esperando para serem familia forem maior ou igual ao minimo de pessoas necessárias para formar uma familia,
                 * forma uma familia nova com eles
                 */
                if (bloodFamily.BloodFamilyQueue.Count >= minBloodFamily)
                {
                    var divisionOfTeams = FindDivisionOfTeams(bloodFamily.BloodFamilyMinds.Count);

                    if (_mindSystem.TryGetMind(ev.Player, out var mindId, out var mindComponent))
                    {
                        bloodFamily.BloodFamilyQueue.Add(ev.Player, (mindId,mindComponent, divisionOfTeams.Count));
                    }

                    int index = 0;
                    var peopleToDelete = new Dictionary<IPlayerSession, (EntityUid, MindComponent, int)>();
                    foreach (var playerInQueue in bloodFamily.BloodFamilyQueue)
                    {
                        if (index > bloodFamily.MaxBloodFamily)
                            break;

                        MakeBloodFamiliar(playerInQueue);

                        peopleToDelete.Add(playerInQueue.Key,playerInQueue.Value);
                        index++;
                    }


                    // Da os objetivos para as pessoas que foram adicionadas e remove elas do queue
                    foreach (var player in peopleToDelete)
                    {
                        GiveObjectives(player.Value.Item1, player.Value.Item2, bloodFamily);

                        if (player.Value.Item2.OwnedEntity != null)
                        {
                            GiveImplants(player.Value.Item2.OwnedEntity.Value);
                        }

                        SendBloodFamilyBriefing(player.Value.Item1, player.Value.Item2);
                        bloodFamily.BloodFamilyQueue.Remove(player.Key);
                    }

                }
                else
                {
                    var divisionOfTeams = FindDivisionOfTeams(bloodFamily.BloodFamilyMinds.Count);

                    if (_mindSystem.TryGetMind(ev.Player, out var mindId, out var mindComponent))
                    {
                        bloodFamily.BloodFamilyQueue.Add(ev.Player, (mindId, mindComponent, divisionOfTeams.Count));
                    }
                }

                // Nao sei se vou deixar entrar num time ja existente, do jeito que funciona os objectives, nao tem como atualizar os objetivos de alg pra incluir
                // o familiar novo
            }
        }
    }

    /// <summary>
    /// Returns all the players that can be an antagonist and has Blood Family enabled in preferences
    /// </summary>
    /// <param name="candidates">All the candidates that will be checked upon</param>
    /// <param name="component">The Blood Family component</param>
    /// <returns></returns>
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

    /// <summary>
    /// Selects which players will be marked to become a blood family member and which team they will be a part of
    /// </summary>
    /// <param name="prefList">List of players who will possibly be picked to be an antag</param>
    /// <param name="component">Blood Family component</param>
    /// <returns></returns>
    public Dictionary<IPlayerSession, (EntityUid, MindComponent, int)> PickFamilyMembers(List<IPlayerSession> prefList, BloodFamilyRuleComponent component)
    {
        var results = new Dictionary<IPlayerSession, (EntityUid, MindComponent, int)>();
        if (prefList.Count <= 1)
        {
            Log.Info("Insufficient ready players to fill up with blood family members, stopping the selection.");
            return results;
        }

        /*
         * Checks if a player is eligible to become a blood family member
         */
        foreach (var player in prefList)
        {
            if (!FilterPossiblePlayers(player))
            {
                prefList.Remove(player);
            }
        }

        var familyPool = prefList.Count;

        var division = FindDivisionOfTeams(familyPool);

        int index = 0;
        foreach (var team in division)
        {
            for (int i = 0; i < team; i++)
            {
                var randomPlayer = _random.PickAndTake(prefList);
                if (_mindSystem.TryGetMind(randomPlayer, out var mindId, out var mindComponent))
                {
                    results.Add(randomPlayer,(mindId, mindComponent, index));
                    Log.Info($"Selected a {mindComponent.CharacterName} for team {index}.");
                }
            }
            Log.Info($"Team {index} closed.");
            index++;
        }

        return results;
    }

    // TODO: fazer isso funcionar pra qualquer numero maximo de jogadores por time (basicamente nao ser hardcoded)
    /// <summary>
    /// Creates a group division based on the number of blood family members
    /// </summary>
    /// <param name="numPlayers">Number of players</param>
    /// <returns>List containing the amount of players in every group
    /// Example: If numPlayers is 7, it will return [3,2,2], where every number inside the list is the amount of
    /// players in a group, and in this case, there are 3 groups</returns>
    private List<int> FindDivisionOfTeams(int numPlayers)
    {
        var times = new List<int>();

        while (numPlayers > 0)
        {
            if (numPlayers >= 3 && numPlayers != 3 + 1)
            {
                times.Add(3);
                numPlayers -= 3;
            }
            else
            {
                times.Add(2);
                numPlayers -= 2;
            }
        }

        return times;
    }

    /// <summary>
    /// Makes a player a blood family member, gives their mind the blood family prototype id
    /// </summary>
    /// <param name="playerMindAndTeam">A player's session, mind, mind component and team they belong to</param>
    /// <returns>True if it was successful, false if not</returns>
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
        bloodFamilyRuleEntity.BloodFamilyTeams.Add(playerMindAndTeam.Value.Item1, playerMindAndTeam.Value.Item3);

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

    /// <summary>
    /// Checks if a player has a mind, is already a traitor, is already a blood family member, and if the mind has an attached entity
    /// </summary>
    /// <param name="player"></param>
    /// <returns>True if all the checks passed</returns>
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

    /// <summary>
    /// Gives the blood family member its objetives
    /// </summary>
    /// <param name="mindId">Mind entity of a player</param>
    /// <param name="mind">Mind component of a player</param>
    /// <param name="ruleComponent">Blood Family Rule component</param>
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

        /*
         * Gives the blood family member the "keep your family alive" objective, all blood family members must have one
         */
        var familyObjective = _objectivesSystem.TryCreateObjective(mindId, mind, "FamilyAliveObjective");

        if (familyObjective != null)
        {
            _mindSystem.AddObjective(mindId, mind, familyObjective.Value);
        }
    }

    // TODO: Dar ao jogador uma linha de radio privada
    /// <summary>
    /// This will give a player its main source of communication between family members
    /// </summary>
    /// <param name="uid">Player's mind owned entity</param>
    private void GiveImplants(EntityUid uid)
    {
        // var proto = Spawn("SyndieRadioImplant");
        // if (!TryComp<SubdermalImplantComponent>(proto, out var comp))
        // {
        //     return;
        // }
        // _subdermalImplantSystem.ForceImplant(uid, proto, comp);

        _antagSelection.GiveAntagBagGear(uid, "EncryptionKeySyndie");
    }

    /// <summary>
    /// Gets all blood family members in a team
    /// </summary>
    /// <param name="ourMind">A player's mind component</param>
    /// <returns>List of players that are in the passed player's team (ourMind)</returns>
    public List<(EntityUid Id, MindComponent Mind)> GetOtherBloodFamilyMindsAliveAndConnectedSameTeam(MindComponent ourMind)
    {
        List<(EntityUid Id, MindComponent Mind)> allTraitors = new();
        foreach (var traitor in EntityQuery<BloodFamilyRuleComponent>())
        {
            foreach (var role in GetOtherBloodFamilyMindsAliveAndConnectedSameTeam(ourMind, traitor))
            {
                if (!allTraitors.Contains(role))
                    allTraitors.Add(role);
            }
        }

        return allTraitors;
    }

    /// <summary>
    /// Gets all blood family members in a team
    /// </summary>
    /// <param name="ourMind">A player's mind component</param>
    /// <param name="component">Blood Family Rule component</param>
    /// <returns>List of players that are in the passed player's team (ourMind)</returns>
    private List<(EntityUid Id, MindComponent Mind)> GetOtherBloodFamilyMindsAliveAndConnectedSameTeam(MindComponent ourMind, BloodFamilyRuleComponent component)
    {
        if (ourMind.CurrentEntity == null)
            return new List<(EntityUid Id, MindComponent Mind)>();

        var allPlayers = GetAllBloodFamilyMembersAliveAndConnected(component);

        var find = allPlayers.Find(n=>n.Mind.UserId == ourMind.UserId);

        var ourMindAndTeam = component.BloodFamilyTeams.Where(n=>n.Key == find.Id);
        var ourMindAndTeamReal = ourMindAndTeam.FirstOrDefault();

        var ourTeam = ourMindAndTeamReal.Value;

        var traitors = new List<(EntityUid Id, MindComponent Mind)>();
        foreach (var traitor in component.BloodFamilyTeams)
        {
            if(traitor.Value != ourTeam)
                continue;

            if (TryComp(traitor.Key, out MindComponent? mind) &&
                mind.OwnedEntity != null &&
                mind.Session != null &&
                mind != ourMind &&
                _mobStateSystem.IsAlive(mind.OwnedEntity.Value) &&
                mind.CurrentEntity == mind.OwnedEntity)
            {
                traitors.Add((traitor.Key, mind));
            }
        }

        return traitors;
    }

    // private List<(EntityUid Id, MindComponent Mind)> GetAllBloodFamilyMindsAliveAndConnectedSameTeam(MindComponent ourMind, BloodFamilyRuleComponent component)
    // {
    //     var traitors = new List<(EntityUid Id, MindComponent Mind)>();
    //     foreach (var traitor in component.BloodFamilyMinds)
    //     {
    //         if (TryComp(traitor, out MindComponent? mind) &&
    //             mind.OwnedEntity != null &&
    //             mind.Session != null &&
    //             _mobStateSystem.IsAlive(mind.OwnedEntity.Value) &&
    //             mind.CurrentEntity == mind.OwnedEntity)
    //         {
    //             traitors.Add((traitor, mind));
    //         }
    //     }
    //
    //     return traitors;
    // }

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

    /// <summary>
    /// Gets all players that are blood family members in the match
    /// </summary>
    /// <param name="component">Blood Family Rule component</param>
    /// <returns>List of players</returns>
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

    /// <summary>
    /// Sets the round end summary text
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="args"></param>
    private void OnObjectivesTextGetInfo(EntityUid uid, BloodFamilyRuleComponent component, ref ObjectivesTextGetInfoEvent args)
    {
        // TODO: nomes de familias, deixar melhor este texto "Havia 4 membro da Familias"
        args.Minds = component.BloodFamilyMinds;
        args.AgentName = Loc.GetString("blood-family-round-end-agent-name");
    }

    /// <summary>
    /// Sends the greeting text to a player
    /// </summary>
    /// <param name="mind"></param>
    /// <param name="ourMind"></param>
    private void SendBloodFamilyBriefing(EntityUid mind, MindComponent ourMind)
    {
        if (!_mindSystem.TryGetSession(mind, out var session))
            return;

        var family = GetOtherBloodFamilyMindsAliveAndConnectedSameTeam(ourMind);

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
