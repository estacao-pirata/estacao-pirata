objectives-round-end-result = {$count ->
    [one] Havia um {$agent}.
    *[other] Havia {$count} {MAKEPLURAL($agent)}.
}

objectives-player-user-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color])
objectives-player-user = [color=gray]{$user}[/color]
objectives-player-named = [color=White]{$name}[/color]

objectives-no-objectives = {$title} era um {$agent}.
objectives-with-objectives = {$title} era um {$agent} que tinha os seguintes objetivos:

objectives-condition-success = {$condition} | [color={$markupColor}]Sucesso![/color]
objectives-condition-fail = {$condition} | [color={$markupColor}]Falha![/color] ({$progress}%)
