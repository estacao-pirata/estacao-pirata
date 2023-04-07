## wizard
# Shown at the end of a round of wizard
wizard-round-end-result = {$wizardCount ->
    [one] Havia um mago.
    *[other] Havia {$wizardCount} magos.
}

# Shown at the end of a round of wizard
wizard-user-was-a-wizard = [color=gray]{$user}[/color] era um mago.
wizard-user-was-a-wizard-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) era um mago.
wizard-was-a-wizard-named = [color=White]{$name}[/color] era um mago.

wizard-user-was-a-wizard-with-objectives = [color=gray]{$user}[/color] era um mago que tinha os seguintes objetivos:
wizard-user-was-a-wizard-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) era um mago que tinha os seguintes objetivos:
wizard-was-a-wizard-with-objectives-named = [color=White]{$name}[/color] era um mago que tinha os seguintes objetivos:

preset-wizard-objective-issuer-wizfeds = [color=#87cefa]A Federação dos Magos[/color]

wizard-objective-condition-success = {$condition} | [color={$markupColor}]Success![/color]
wizard-objective-condition-fail = {$condition} | [color={$markupColor}]Failure![/color] ({$progress}%)

preset-wizard-title = wizard
preset-wizard-description = Os magos estão escondidos entre a tripulação da estação, encontre e lide com eles antes que se tornem muito poderosos.
preset-wizard-not-enough-ready-players = Não há jogadores suficientes preparados para o jogo! Havia {$readyPlayersCount} jogadores prontos para {$minimumPlayers} necessário.
preset-wizard-no-one-ready = Nenhum jogador pronto! Não é possível iniciar o assistente.

## wizardRole

# wizardRole
wizard-role-greeting =
    Você é um agente disfarçado da Wizard Federation.
    Seus objetivos e palavras-código estão listados no menu do personagem.
    Use o uplink carregado em seu PDA para comprar as ferramentas necessárias para esta missão.
    Você pode obter mais moeda completando as missões do Oráculo.
    Morte ao Nanotrasen!
wizard-role-codewords =
    As palavras-código são:
    {$codewords}.
    As palavras-código podem ser usadas em conversas regulares para se identificar discretamente para outros agentes.
    Ouça-os e mantenha-os em segredo.