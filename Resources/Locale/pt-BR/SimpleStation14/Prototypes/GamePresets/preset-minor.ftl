## minor

# Shown at the end of a round of minor antags
minor-round-end-result = {$minorCount ->
    [one] Houve um antag menor.
    *[other] Havia {$minorCount} antags menores.
}

# Shown at the end of a round of minor antags
minor-user-was-a-minor = [color=gray]{$user}[/color] era um antag menor.
minor-user-was-a-minor-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) era um antag menor.
minor-was-a-minor-named = [color=White]{$name}[/color] era um antag menor.

minor-user-was-a-minor-with-objectives = [color=gray]{$user}[/color] era um antagonista menor que tinha o seguinte objetivo:
minor-user-was-a-minor-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) era um antagonista menor que tinha o seguinte objetivo:
minor-was-a-minor-with-objectives-named = [color=White]{$name}[/color] era um antagonista menor que tinha o seguinte objetivo:

preset-minor-objective-issuer-freewill = [color=#87cefa]Free Will[/color]

minor-objective-condition-success = {$condition} | [color={$markupColor}]Success![/color]
minor-objective-condition-fail = {$condition} | [color={$markupColor}]Failure![/color] ({$progress}%)

preset-minor-title = antag menor
preset-minor-description = Este modo de jogo não deve ser usado..
preset-minor-not-enough-ready-players = Não há jogadores suficientes prontos para o jogo! Havia {$readyPlayersCount} jogadores prontos dos {$minimumPlayers} necessários.
preset-minor-no-one-ready = Nenhum jogador preparado! Não é possível iniciar o antag menor.

## minorRole

# minorRole
minor-role-greeting =
    Você tem vontade propria e não está ligado a nenhuma organização terrorista.
