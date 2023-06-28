
## Traitor

# Shown at the end of a round of Traitor
traitor-round-end-result = {$traitorCount ->
    [one] Havia um traidor.
    *[other] Haviam {$traitorCount} traidores.
}

traitor-round-end-codewords = As palavras chave eram: [color=White]{$codewords}[/color].

# Shown at the end of a round of Traitor
traitor-user-was-a-traitor = [color=gray]{$user}[/color] era um traidor.
traitor-user-was-a-traitor-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) era um traidor.
traitor-was-a-traitor-named = [color=White]{$name}[/color] era um traidor.

traitor-user-was-a-traitor-with-objectives = [color=gray]{$user}[/color] era um traidor com os seguintes objetivos:
traitor-user-was-a-traitor-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) era um traidor que tinha como objetivos:
traitor-was-a-traitor-with-objectives-named = [color=White]{$name}[/color] era um traidor que tinha os seguintes objetivos:

preset-traitor-objective-issuer-syndicate = [color=#87cefa]O Sindicato[/color]
preset-traitor-objective-issuer-spiderclan = [color=#33cc00]Aranha Clã[/color]

# Shown at the end of a round of Traitor
traitor-objective-condition-success = {$condition} | [color={$markupColor}]Sucesso![/color]

# Shown at the end of a round of Traitor
traitor-objective-condition-fail = {$condition} | [color={$markupColor}]Falhou![/color] ({$progress}%)

traitor-title = Traitor
traitor-description = Há traidores entre nós...
traitor-not-enough-ready-players = Faltou mais jogadores prontos para a partida! Haviam {$readyPlayersCount} jogadores prontos de {$minimumPlayers} necessários. Impossível iniciar modo traidor.
traitor-no-one-ready = Nenhum jogador deu "pronto"! Impossível iniciar modo traidor.

## TraitorDeathMatch
traitor-death-match-title = Mata-mata de traidor
traitor-death-match-description = NÃO É BATTLE ROYALE! Não vai ganhar nada sobrevivendo escondido, procure seus oponentes, acumule TCs! Todos são traidores. Todo mundo quer matar uns aos outros.
traitor-death-match-station-is-too-unsafe-announcement = A estação está muito perigosa para continuar. Você tem um minuto.
traitor-death-match-end-round-description-first-line = Os PDAs se recuperaram depois...
traitor-death-match-end-round-description-entry = PDA de {$originalName}, com {$tcBalance} TC

## TraitorRole

# TraitorRole
traitor-role-greeting =
    Você é um agente do sindicato.
    Seus objetivos e palavras-código estão listados no menu do personagem.
    Use o uplink carregado em seu PDA para comprar as ferramentas necessárias para esta missão.
    Morte para Nanotrasen!
traitor-role-codewords =
    As palavras-código são:
    {$codewords}.
    As palavras-código podem ser usadas em conversas regulares para se identificar discretamente para outros agentes do sindicato.
    Ouça-os e mantenha-os em segredo.
traitor-role-uplink-code =
    Defina seu toque para as notas {$code} para bloquear ou desbloquear seu uplink.
    Lembre-se de trancá-lo depois, ou a equipe da estação também o abrirá facilmente!

# don't need all the flavour text for character menu
traitor-role-codewords-short =
    As palavras-código são:
    {$codewords}.
traitor-role-uplink-code-short = Seu código de uplink é {$code}.