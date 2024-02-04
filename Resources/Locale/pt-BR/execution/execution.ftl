execution-verb-name = Executar
execution-verb-message = Use sua arma para executar alguém.

# All the below localisation strings have access to the following variables
# attacker (the person committing the execution)
# victim (the person being executed)
# weapon (the weapon used for the execution)

execution-popup-gun-initial-internal = Você prepara a boca de {THE($weapon)} contra a cabeça de {$victim}.
execution-popup-gun-initial-external = {$attacker} prepara a boca de {THE($weapon)} contra a cabeça de {$victim}.
execution-popup-gun-complete-internal = Você acertou {$victim} na cabeça!
execution-popup-gun-complete-external = {$attacker} atira {$victim} na cabeça!
execution-popup-gun-clumsy-internal = Você errou a cabeça de {$victim} e atirou no pé!
execution-popup-gun-clumsy-external = {$attacker} erra {$victim} e atira no pé de {POSS-ADJ($attacker)}!
execution-popup-gun-empty = {CAPITALIZE(THE($weapon))} cliques.

suicide-popup-gun-initial-internal = Você coloca o cano de {THE($weapon)} em sua boca.
suicide-popup-gun-initial-external = {$attacker} coloca a boca de {THE($weapon)} na boca de {POSS-ADJ($attacker)}.
suicide-popup-gun-complete-internal = Você dá um tiro na cabeça!
suicide-popup-gun-complete-external = {$attacker} atira {REFLEXIVE($attacker)} na cabeça!

execution-popup-melee-initial-internal = Você prepara {THE($weapon)} contra a garganta de {$victim}.
execution-popup-melee-initial-external = {$attacker} prepara {POSS-ADJ($attacker)} {$weapon} contra a garganta de {$victim}.
execution-popup-melee-complete-internal = Você cortou a garganta de {$victim}!
execution-popup-melee-complete-external = {$attacker} corta a garganta de {$victim}!

suicide-popup-melee-initial-internal = Você prepara {THE($weapon)} contra sua garganta.
suicide-popup-melee-initial-external = {$attacker} prepara {POSS-ADJ($attacker)} {$weapon} contra a garganta de {POSS-ADJ($attacker)}.
suicide-popup-melee-complete-internal = Você cortou sua garganta com {THE($weapon)}!
suicide-popup-melee-complete-external = {$attacker} corta a garganta de {POSS-ADJ($attacker)} com {THE($weapon)}!
