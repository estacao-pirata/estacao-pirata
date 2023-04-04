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

cmd-ban_exemption_update-desc = Defina uma isenção para um tipo de banimento de um jogador.
cmd-ban_exemption_update-help = Uso: ban_exemption_update <player> <flag> [<flag> [...]]
    Especifique vários indicadores para conceder a um jogador múltiplos indicadores de isenção de banimento.
    Para remover todas as isenções, execute este comando e forneça "Nenhum" como único sinalizador.

cmd-ban_exemption_update-nargs = Pelo menos 2 argumentos esperados
cmd-ban_exemption_update-locate = Não foi possível localizar o jogador '{$player}'.
cmd-ban_exemption_update-invalid-flag = Indicador inválido '{$flag}'.
cmd-ban_exemption_update-success = Indicador de isenção de banimento atualizados para '{$player}' ({$uid}).
cmd-ban_exemption_update-arg-player = <player>
cmd-ban_exemption_update-arg-flag = <flag>

cmd-ban_exemption_get-desc = Mostrar isenções de banimento para um determinado jogador.
cmd-ban_exemption_get-help = Uso: ban_exemption_get <player>

cmd-ban_exemption_get-nargs = Esperado exatamente 1 argumento
cmd-ban_exemption_get-none = O usuário não está isento de banimentos.
cmd-ban_exemption_get-show = O usuário está isento das seguintes indicadores de banimento: {$flags}.
cmd-ban_exemption_get-arg-player = <player>
