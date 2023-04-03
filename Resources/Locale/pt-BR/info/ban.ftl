# ban
cmd-ban-desc = Bane alguém.
cmd-ban-help = Uso: ban <nome ou ID do usuário> <razão> [duração em minutos, deixe vazio ou 0 para ban permanente]
cmd-ban-player = Não foi possível encontrar um usuário com esse nome.
cmd-ban-self = Você não pode se banir!
cmd-ban-hint = <nome/ID do usuário>
cmd-ban-hint-reason = <razão>
cmd-ban-hint-duration = [duração]

cmd-ban-hint-duration-1 = Permanenente
cmd-ban-hint-duration-2 = 1 dia
cmd-ban-hint-duration-3 = 3 dias
cmd-ban-hint-duration-4 = 1 semana
cmd-ban-hint-duration-5 = 2 semanas
cmd-ban-hint-duration-6 = 1 mês

# listbans
cmd-banlist-desc = Lista os bans ativos de um usuário.
cmd-banlist-help = Uso: banlist <nome ou ID do usuário>
cmd-banlist-empty = Sem bans ativos encontrados para {$user}
cmd-banlistF-hint = <nome/ID do usuário>

cmd-ban_exemption_update-desc = Set an exemption to a type of ban on a player.
cmd-ban_exemption_update-help = Usage: ban_exemption_update <player> <flag> [<flag> [...]]
    Specify multiple flags to give a player multiple ban exemption flags.
    To remove all exemptions, run this command and give "None" as only flag.

cmd-ban_exemption_update-nargs = Expected at least 2 arguments
cmd-ban_exemption_update-locate = Unable to locate player '{$player}'.
cmd-ban_exemption_update-invalid-flag = Invalid flag '{$flag}'.
cmd-ban_exemption_update-success = Updated ban exemption flags for '{$player}' ({$uid}).
cmd-ban_exemption_update-arg-player = <player>
cmd-ban_exemption_update-arg-flag = <flag>

cmd-ban_exemption_get-desc = Show ban exemptions for a certain player.
cmd-ban_exemption_get-help = Usage: ban_exemption_get <player>

cmd-ban_exemption_get-nargs = Expected exactly 1 argument
cmd-ban_exemption_get-none = User is not exempt from any bans.
cmd-ban_exemption_get-show = User is exempt from the following ban flags: {$flags}.
cmd-ban_exemption_get-arg-player = <player>