using Robust.Shared.Utility;
using System.Text.RegularExpressions;
using Content.Server.Speech;

namespace Pirata.Sotaque.Portunhol;

sealed class SotaqueCarioca : AccentEngine
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SotaquePortunholComponent, AccentGetEvent>(OnAccent);
    }
    override protected string path { get; set; } = "Resources/Prototypes/EstacaoPirata/Sotaques/Portunhol/portunhol.json";
    private void OnAccent(EntityUid uid, SotaquePortunholComponent component, AccentGetEvent args)
    {
        args.Message = Take(args.Message);
    }
    override protected string Parse(string message) {
        //cranking xenophobia to 101% YEEEEAHHHH
        Regex[] ogs = { new Regex(@"([cçCÇ])ão"), new Regex(@"([cçCÇ])ÃO"), new Regex(@"ão"), new Regex(@"ÃO"),
        new Regex(@"([aeiouAEIOU])r($|[\s])"), new Regex(@"([aeiouAEIOU])R($|[\s])")};
        string[] mods = {"ción", "CIÓN", "on", "ON", "$1ste$2", "$1STE$2" };
        List<string>moddedToken = new List<string>();
        string[] tokens = message.Split(' ');
        foreach(string token in tokens) {
            bool added = false;
            for(int i = 0; i < ogs.Length; i++) {
                if(ogs[i].IsMatch(token)) {
                    moddedToken.Add(ogs[i].Replace(token, mods[i]));
                    added = true;
                    break;
                }
            }
            if(!added)
                    moddedToken.Add(token);
                added = false;
        }

        string modded = string.Join(" ", moddedToken);
        return modded;
    }
}
