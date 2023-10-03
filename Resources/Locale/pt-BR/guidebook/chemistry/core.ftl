guidebook-reagent-effect-description =
    {$chance ->
        [1] { $effect }
        *[other] Tem um { NATURALPERCENT($chance, 2) } chance para { $effect }
    }{ $conditionCount ->
        [0] .
        *[other] {" "}quando { $conditions }.
    }

guidebook-reagent-name = [bold][color={$color}]{CAPITALIZE($name)}[/color][/bold]
guidebook-reagent-recipes-header = Receita
guidebook-reagent-recipes-reagent-display = [bold]{$reagent}[/bold] \[{$ratio}\]
guidebook-reagent-recipes-mix = Misturar
guidebook-reagent-recipes-mix-and-heat = Mistura acima de {$temperature}K
guidebook-reagent-effects-header = Efeitos
guidebook-reagent-effects-metabolism-group-rate = [bold]{$group}[/bold] [color=gray]({$rate} unidades por segundo)[/color]
guidebook-reagent-physical-description = Parece ser {$description}.
