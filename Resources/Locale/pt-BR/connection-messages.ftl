whitelist-not-whitelisted = Você não está na whitelist.
whitelist-not-whitelisted-rp = Você não está na whitelist. Para entrar na whitelist, visite nosso Discord (que pode ser encontrado em https://estacaopirata.com/).

command-whitelistadd-description = Adiciona o jogador na whitelist.
command-whitelistadd-help = whitelistadd <username>
command-whitelistadd-existing = {$username} já está na whitelist!
command-whitelistadd-added = {$username} adicionado à whitelist
command-whitelistadd-not-found = Usuário '{$username}' não encontrado

command-whitelistremove-description = Remove o jogador da whitelist.
command-whitelistremove-help = whitelistremove <username>
command-whitelistremove-existing = {$username} não está na whitelist!
command-whitelistremove-removed = {$username} removido da whitelist
command-whitelistremove-not-found = Usuário '{$username}' não encontrado

command-kicknonwhitelisted-description = Expulsar todos os jogadores que não estão na whitelist.
command-kicknonwhitelisted-help = kicknonwhitelisted

ban-banned-permanent = Este ban só será removido através de apelo.
ban-banned-permanent-appeal = Este ban só será removido através de apelo através do link {$link}
ban-expires = Este ban dura {$duration} minutos e irá expirar em {$time} UTC.
ban-banned-1 = Você ou outro usuário desse computador ou conexão estão banidos aqui.
ban-banned-2 = O motivo do ban é: "{$reason}"
ban-banned-3 = Tentativas de contornar o ban tal como criar uma conta nova serão registradas.


soft-player-cap-full = O servidor está cheio!
panic-bunker-account-denied = Este servidor está em modo Pânico e você foi rejeitado. Entre em contato com o administrador do servidor para obter ajuda.
panic-bunker-account-denied-reason = Este servidor está em modo Pânico e você foi rejeitado. Razão: "{$reason}"
panic-bunker-account-reason-account = Tempo de vida da conta precisa ser maior que {$minutes} minutes
panic-bunker-account-reason-overall = O tempo mínimo requerido total de jogo é {$hours} horas

whitelist-playercount-invalid = {$min ->
    [0] A whitelist para este servidor só se aplica com menos de {$max} jogadores.
    *[other] A whitelist para este servidor só se aplica com mais de {$min} {$max ->
        [2147483647] -> jogadores, então talvez você possa entrar mais tarde.
        *[other] -> jogadores e menos de {$max} jogadores, então talvez você possa entrar mais tarde.
    }
}
