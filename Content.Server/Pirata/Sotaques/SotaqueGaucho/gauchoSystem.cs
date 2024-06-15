using Robust.Shared.Utility;
using System.Text.RegularExpressions;
using Content.Server.Speech;

namespace Pirata.Sotaque.Gaucho;

sealed class SotaqueCarioca : AccentEngine
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SotaqueGauchoComponent, AccentGetEvent>(OnAccent);
        getPath();
        Log.Debug(file);
    }
    override protected string path { get; set; } = "/Prototypes/EstacaoPirata/Sotaques/Gaucho";
    private void OnAccent(EntityUid uid, SotaqueGauchoComponent component, AccentGetEvent args)
    {
        args.Message = Take(args.Message);
    }
    override protected string Parse(string message) {
        //cranking xenophobia to 101% YEEEEAHHHH
        Regex[] ogs = { new Regex(@"([^BCDFGHJKLMNPQRSTVXZbcdfghjklmnpqrstvzx])t([AEIOUaeiou])"), new Regex(@"([^BCDFGHJKLMNPQRSTVXZbcdfghjklmnpqrstvzx])T([AEIOUaeiou])")};
        string[] mods = { "$1tch$2", "$1Tch$2"};
        List<string>moddedToken = new List<string>();
        string[] tokens = message.Split(' ');
        foreach(string token in tokens) {
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
        return modded;
    }
}
