### UI

chat-manager-max-message-length = Sua mensagem excedeu o limite de {$maxMessageLength} caractéres
chat-manager-ooc-chat-enabled-message = Chat OOC foi habilitado.
chat-manager-ooc-chat-disabled-message = Chat OOC foi desabilitado.
chat-manager-looc-chat-enabled-message = Chat LOOC foi habilitado.
chat-manager-looc-chat-disabled-message = Chat LOOC foi desabilitado.
chat-manager-dead-looc-chat-enabled-message = Jogadores mortos agora podem usar LOOC.
chat-manager-dead-looc-chat-disabled-message = Jogadores mortos não podem mais usar LOOC.
chat-manager-crit-looc-chat-enabled-message = Jogadores em crit agora podem usar LOOC.
chat-manager-crit-looc-chat-disabled-message = Jogadores em crit não podem mais usar LOOC.
chat-manager-admin-ooc-chat-enabled-message = Chat Admin OOC foi habilitado.
chat-manager-admin-ooc-chat-disabled-message = Chat Admin OOC foi desabilitado.

chat-manager-max-message-length-exceeded-message = Sua mensagem excedeu o limite de {$limit} caractéres
chat-manager-no-headset-on-message = Você não tem um headset ligado!
chant-manager-no-radio-key = Nenhuma chave de canal selecionada!
chat-manager-no-such-channel = Não existe canal com a chave '{$key}'!
chat-manager-whisper-headset-on-message = Você não pode sussurrar no rádio!

chat-manager-server-wrap-message = [bold]{$message}[/bold]
chat-manager-sender-announcement-wrap-message = [font size=14][bold]Anúncio de {$sender}:[/font][font size=12]
                                                {$message}[/bold][/font]
chat-manager-entity-say-wrap-message = [bold]{$entityName}[/bold] {$verb}: [font={$fontType} size={$fontSize}]"{$message}"[/font]
chat-manager-entity-say-bold-wrap-message = [bold]{$entityName}[/bold] {$verb}: [font={$fontType} size={$fontSize}][bold]"{$message}"[/bold][/font]

chat-manager-entity-whisper-wrap-message = [font size=11][italic]{$entityName} sussura, "{$message}"[/italic][/font]
chat-manager-entity-whisper-unknown-wrap-message = [font size=11][italic]Alguém sussura, "{$message}"[/italic][/font]

# THE() is not used here because the entity and its name can technically be disconnected if a nameOverride is passed...
chat-manager-entity-me-wrap-message = [italic]{ PROPER($entity) ->
    *[false] a {$entityName} {$message}[/italic]
     [true] {$entityName} {$message}[/italic]
    }

chat-manager-entity-looc-wrap-message = LOOC: [bold]{$entityName}:[/bold] {$message}
chat-manager-send-ooc-wrap-message = OOC: [bold]{$playerName}:[/bold] {$message}
chat-manager-send-ooc-patron-wrap-message = OOC: [bold][color={$patronColor}]{$playerName}[/color]:[/bold] {$message}

chat-manager-send-dead-chat-wrap-message = {$deadChannelName}: [bold]{$playerName}:[/bold] {$message}
chat-manager-send-admin-dead-chat-wrap-message = {$adminChannelName}: [bold]({$userName}):[/bold] {$message}
chat-manager-send-admin-chat-wrap-message = {$adminChannelName}: [bold]{$playerName}:[/bold] {$message}
chat-manager-send-admin-announcement-wrap-message = [bold]{$adminChannelName}: {$message}[/bold]

chat-manager-send-hook-ooc-wrap-message = OOC: [bold](D){$senderName}:[/bold] {$message}

chat-manager-dead-channel-name = MORTO
chat-manager-admin-channel-name = ADMIN

## Speech verbs for chat

chat-speech-verb-suffix-exclamation = !
chat-speech-verb-suffix-exclamation-strong = !!
chat-speech-verb-suffix-question = ?

chat-speech-verb-default = diz
chat-speech-verb-exclamation = exclama
chat-speech-verb-exclamation-strong = grita
chat-speech-verb-question = pergunta

chat-speech-verb-insect-1 = chia
chat-speech-verb-insect-2 = vibra

chat-speech-verb-slime = pia

chat-speech-verb-robotic = afirma
chat-speech-verb-reptilian = sibila

chat-speech-verb-skeleton = cocalha

chat-speech-verb-small-mob = guincha

chat-speech-verb-large-mob = ruge

chat-speech-verb-monkey = chimpers

chat-speech-verb-cluwne-1 = ri
chat-speech-verb-cluwne-2 = gargalha
chat-speech-verb-cluwne-3 = dá risada