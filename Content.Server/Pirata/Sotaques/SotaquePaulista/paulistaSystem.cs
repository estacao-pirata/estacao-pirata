using Robust.Shared.Utility;
using System.Text.RegularExpressions;
using Content.Server.Speech;
using System.Linq;

namespace Pirata.Sotaque.Paulista;

sealed class SotaqueCarioca : AccentEngine
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SotaquePaulistaComponent, AccentGetEvent>(OnAccent);
        getPath();
    }
    override protected string path { get; set; } = "/Prototypes/EstacaoPirata/Sotaques/Paulista";
    private void OnAccent(EntityUid uid, SotaquePaulistaComponent component, AccentGetEvent args)
    {
        args.Message = Take(args.Message);
    }
    override protected string Parse(string message) {
        //cranking xenophobia to 101% YEEEEAHHHH
         Regex[] ogs = {
            new Regex(@"([bcdfghjklmnpqrtvxzBCDEFGHJKLMNPQRTVXZ\s\W])o($|[\s\W])"),
            new Regex(@"([bcdfghjklmnpqrtvxzBCDEFGHJKLMNPQRTVXZ\s\W])O($|[\s\W])"),
            new Regex(@"([AEOU])([Nn])([bcdfghjklmnpqrtvxzBCDEFGHJKLMNPQRTVXZ\s\W])"),
            new Regex(@"([aeou])([Nn])([bcdfghjklmnpqrtvxzBCDEFGHJKLMNPQRTVXZ\s\W])")
        };
        List<string> subs = new List<string>()
        {
            "né?",
            "meo,",
            "né meo?"
        };
        string[] mods = { "$1ou$2", "$1OU$2", "$1i$2$3", "$1i$2$3" };
        List<string>moddedToken = new List<string>();
        string[] tokens = message.Split(' ');
        foreach(string token in tokens) {
            if(_random.Next(21) >= 20) {
                moddedToken.Add(Pick(subs));
            }
            bool added = false;
            string currentToken = token;
            for(int i = 0; i < ogs.Length; i++) {
                if(ogs[i].IsMatch(token)) {
                    currentToken = ogs[i].Replace(currentToken, mods[i]);
                    added = true;
                }
            }
            if(!added) {
                moddedToken.Add(token);
            } else {
                moddedToken.Add(currentToken);
            }
                added = false;
        }
        string modded = string.Join(" ", moddedToken);
        modded = char.ToUpper(modded[0]) + modded.Substring(1);
        return modded;
    }
}
