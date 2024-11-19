### Localization for engine console commands

## generic command errors

cmd-parse-failure-mapid = {$arg} não é um MapId válido.

cmd-error-file-not-found = Could not find file: {$file}.
cmd-error-dir-not-found = Could not find directory: {$dir}.
cmd-failure-no-attached-entity = There is no entity attached to this shell.

## 'oldhelp' command
cmd-oldhelp-desc = Display general help or help text for a specific command
cmd-oldhelp-help = Usage: help [command name]
    When no command name is provided, displays general-purpose help text. If a command name is provided, displays help text for that command.

cmd-oldhelp-no-args = To display help for a specific command, write 'help <command>'. To list all available commands, write 'list'. To search for commands, use 'list <filter>'.
cmd-oldhelp-unknown = Unknown command: { $command }
cmd-oldhelp-top = { $command } - { $description }
cmd-oldhelp-invalid-args = Invalid amount of arguments.
cmd-oldhelp-arg-cmdname = [command name]

## 'setambientlight' command
cmd-set-ambient-light-desc = Allows you to set the ambient light for the specified map, in SRGB.
cmd-set-ambient-light-help = setambientlight [mapid] [r g b a]
cmd-set-ambient-light-parse = Unable to parse args as a byte values for a color.

cmd-ldrsc-desc = Pre-caches a resource.
cmd-ldrsc-help = Usage: ldrsc <path> <type>

cmd-rldrsc-desc = Reloads a resource.
cmd-rldrsc-help = Usage: rldrsc <path> <type>

cmd-gridtc-desc = Gets the tile count of a grid.
cmd-gridtc-help = Usage: gridtc <gridId>


# Client-side commands
cmd-guidump-desc = Dump GUI tree to /guidump.txt in user data.
cmd-guidump-help = Usage: guidump

cmd-uitest-desc = Open a dummy UI testing window
cmd-uitest-help = Usage: uitest

## 'uitest2' command
cmd-uitest2-desc = Opens a UI control testing OS window
cmd-uitest2-help = Usage: uitest2 <tab>
cmd-uitest2-arg-tab = <tab>
cmd-uitest2-error-args = Expected at most one argument
cmd-uitest2-error-tab = Invalid tab: '{$value}'
cmd-uitest2-title = UITest2


cmd-setclipboard-desc = Sets the system clipboard
cmd-setclipboard-help = Usage: setclipboard <text>

cmd-getclipboard-desc = Gets the system clipboard
cmd-getclipboard-help = Usage: Getclipboard

cmd-togglelight-desc = Toggles light rendering.
cmd-togglelight-help = Usage: togglelight

cmd-togglefov-desc = Toggles fov for client.
cmd-togglefov-help = Usage: togglefov

cmd-togglehardfov-desc = Toggles hard fov for client. (for debugging space-station-14#2353)
cmd-togglehardfov-help = Usage: togglehardfov

cmd-toggleshadows-desc = Toggles shadow rendering.
cmd-toggleshadows-help = Usage: toggleshadows

cmd-togglelightbuf-desc = Toggles lighting rendering. This includes shadows but not FOV.
cmd-togglelightbuf-help = Usage: togglelightbuf

cmd-chunkinfo-desc = Gets info about a chunk under your mouse cursor.
cmd-chunkinfo-help = Usage: chunkinfo

cmd-rldshader-desc = Reloads all shaders.
cmd-rldshader-help = Usage: rldshader

cmd-cldbglyr-desc = Toggle fov and light debug layers.
cmd-cldbglyr-help= Usage: cldbglyr <layer>: Toggle <layer>
    cldbglyr: Turn all Layers off

cmd-key-info-desc = Keys key info for a key.
cmd-key-info-help = Usage: keyinfo <Key>

## 'bind' command
cmd-bind-desc = Binds an input key combination to an input command.
cmd-bind-help = Usage: bind { cmd-bind-arg-key } { cmd-bind-arg-mode } { cmd-bind-arg-command }
    Note that this DOES NOT automatically save bindings.
    Use the 'svbind' command to save binding configuration.

cmd-bind-arg-key = <KeyName>
cmd-bind-arg-mode = <BindMode>
cmd-bind-arg-command = <InputCommand>

cmd-net-draw-interp-desc = Toggles the debug drawing of the network interpolation.
cmd-net-draw-interp-help = Usage: net_draw_interp

cmd-net-watch-ent-desc = Dumps all network updates for an EntityId to the console.
cmd-net-watch-ent-help = Usage: net_watchent <0|EntityUid>

cmd-net-refresh-desc = Requests a full server state.
cmd-net-refresh-help = Usage: net_refresh

cmd-net-entity-report-desc = Toggles the net entity report panel.
cmd-net-entity-report-help = Usage: net_entityreport

cmd-fill-desc = Fill up the console for debugging.
cmd-fill-help = Fills the console with some nonsense for debugging.

cmd-cls-desc = Clears the console.
cmd-cls-help = Clears the debug console of all messages.

cmd-sendgarbage-desc = Sends garbage to the server.
cmd-sendgarbage-help = The server will reply with 'no u'

cmd-loadgrid-desc = Loads a grid from a file into an existing map.
cmd-loadgrid-help = loadgrid <MapID> <Path> [x y] [rotation] [storeUids]

cmd-loc-desc = Prints the absolute location of the player's entity to console.
cmd-loc-help = loc

cmd-tpgrid-desc = Teleports a grid to a new location.
cmd-tpgrid-help = tpgrid <gridId> <X> <Y> [<MapId>]

cmd-rmgrid-desc = Removes a grid from a map. You cannot remove the default grid.
cmd-rmgrid-help = rmgrid <gridId>

cmd-mapinit-desc = Runs map init on a map.
cmd-mapinit-help = mapinit <mapID>

cmd-lsmap-desc = Lists maps.
cmd-lsmap-help = lsmap

cmd-lsgrid-desc = Lists grids.
cmd-lsgrid-help = lsgrid

cmd-addmap-desc = Adds a new empty map to the round. If the mapID already exists, this command does nothing.
cmd-addmap-help = addmap <mapID> [initialize]

cmd-rmmap-desc = Removes a map from the world. You cannot remove nullspace.
cmd-rmmap-help = rmmap <mapId>

cmd-savegrid-desc = Serializes a grid to disk.
cmd-savegrid-help = savegrid <gridID> <Path>

cmd-testbed-desc = Loads a physics testbed on the specified map.
cmd-testbed-help = testbed <mapid> <test>

cmd-saveconfig-desc = Saves the client configuration to the config file.
cmd-saveconfig-help = saveconfig

## 'addcomp' command
cmd-addcomp-desc = Adds a component to an entity.
cmd-addcomp-help = addcomp <uid> <componentName>
cmd-addcompc-desc = Adds a component to an entity on the client.
cmd-addcompc-help = addcompc <uid> <componentName>

## 'rmcomp' command
cmd-rmcomp-desc = Removes a component from an entity.
cmd-rmcomp-help = rmcomp <uid> <componentName>
cmd-rmcompc-desc = Removes a component from an entity on the client.
cmd-rmcompc-help = rmcomp <uid> <componentName>

## 'addview' command
cmd-addview-desc = Allows you to subscribe to an entity's view for debugging purposes.
cmd-addview-help = addview <entityUid>
cmd-addviewc-desc = Allows you to subscribe to an entity's view for debugging purposes.
cmd-addviewc-help = addview <entityUid>

## 'removeview' command
cmd-removeview-desc = Allows you to unsubscribe to an entity's view for debugging purposes.
cmd-removeview-help = removeview <entityUid>

## 'loglevel' command
cmd-loglevel-desc = Changes the log level for a provided sawmill.
cmd-loglevel-help = Usage: loglevel <sawmill> <level>
      sawmill: A label prefixing log messages. This is the one you're setting the level for.
      level: The log level. Must match one of the values of the LogLevel enum.

cmd-testlog-desc = Writes a test log to a sawmill.
cmd-testlog-help = Usage: testlog <sawmill> <level> <message>
    sawmill: A label prefixing the logged message.
    level: The log level. Must match one of the values of the LogLevel enum.
    message: The message to be logged. Wrap this in double quotes if you want to use spaces.

## 'vv' command
cmd-vv-desc = Opens View Variables.
cmd-vv-help = Usage: vv <entity ID|IoC interface name|SIoC interface name>

## 'showvelocities' command
cmd-showvelocities-desc = Displays your angular and linear velocities.
cmd-showvelocities-help = Usage: showvelocities

## 'setinputcontext' command
cmd-setinputcontext-desc = Sets the active input context.
cmd-setinputcontext-help = Usage: setinputcontext <context>

## 'forall' command
cmd-forall-desc = Runs a command over all entities with a given component.
cmd-forall-help = Usage: forall <bql query> do <command...>

## 'delete' command
cmd-delete-desc = Deletes the entity with the specified ID.
cmd-delete-help = delete <entity UID>

# System commands
cmd-showtime-desc = Shows the server time.
cmd-showtime-help = showtime

cmd-restart-desc = Gracefully restarts the server (not just the round).
cmd-restart-help = restart

cmd-shutdown-desc = Gracefully shuts down the server.
cmd-shutdown-help = shutdown

cmd-netaudit-desc = Prints into about NetMsg security.
cmd-netaudit-help = netaudit

# Player commands
cmd-tp-desc = Teleports a player to any location in the round.
cmd-tp-help = tp <x> <y> [<mapID>]

cmd-tpto-desc = Teleports the current player or the specified players/entities to the location of the first player/entity.
cmd-tpto-help = tpto <username|uid> [username|uid]...
cmd-tpto-destination-hint = destination (uid or username)
cmd-tpto-victim-hint = entity to teleport (uid or username)
cmd-tpto-parse-error = Cant resolve entity or player: {$str}

cmd-listplayers-desc = Lists all players currently connected.
cmd-listplayers-help = listplayers

cmd-kick-desc = Kicks a connected player out of the server, disconnecting them.
cmd-kick-help = kick <PlayerIndex> [<Reason>]

# Spin command
cmd-spin-desc = Causes an entity to spin. Default entity is the attached player's parent.
cmd-spin-help = spin velocity [drag] [entityUid]

# Localization command
cmd-rldloc-desc = Reloads localization (client & server).
cmd-rldloc-help = Usage: rldloc

# Debug entity controls
cmd-spawn-desc = Spawns an entity with specific type.
cmd-spawn-help = spawn <prototype> OR spawn <prototype> <relative entity ID> OR spawn <prototype> <x> <y>
cmd-cspawn-desc = Spawns a client-side entity with specific type at your feet.
cmd-cspawn-help = cspawn <entity type>

cmd-scale-desc = Increases or decreases an entity's size naively.
cmd-scale-help = scale <entityUid> <float>

cmd-dumpentities-desc = Dump entity list.
cmd-dumpentities-help = Dumps entity list of UIDs and prototype.

cmd-getcomponentregistration-desc = Gets component registration information.
cmd-getcomponentregistration-help = Usage: getcomponentregistration <componentName>

cmd-showrays-desc = Toggles debug drawing of physics rays. An integer for <raylifetime> must be provided.
cmd-showrays-help = Usage: showrays <raylifetime>

cmd-disconnect-desc = Immediately disconnect from the server and go back to the main menu.
cmd-disconnect-help = Usage: disconnect

cmd-entfo-desc = Displays verbose diagnostics for an entity.
cmd-entfo-help = Usage: entfo <entityuid>
    The entity UID can be prefixed with 'c' to convert it to a client entity UID.

cmd-fuck-desc = Throws an exception
cmd-fuck-help = Throws an exception

cmd-showpos-desc = Enables debug drawing over all entity positions in the game.
cmd-showpos-help = Usage: showpos

cmd-sggcell-desc = Lists entities on a snap grid cell.
cmd-sggcell-help = Usage: sggcell <gridID> <vector2i>\nThat vector2i param is in the form x<int>,y<int>.

cmd-overrideplayername-desc = Changes the name used when attempting to connect to the server.
cmd-overrideplayername-help = Usage: overrideplayername <name>

cmd-showanchored-desc = Shows anchored entities on a particular tile
cmd-showanchored-help = Usage: showanchored

cmd-dmetamem-desc = Dumps a type's members in a format suitable for the sandbox configuration file.
cmd-dmetamem-help = Usage: dmetamem <type>

cmd-showchunkbb-desc = Displays chunk bounds for the purposes of rendering.
cmd-showchunkbb-help = Usage: showchunkbb <type>

cmd-launchauth-desc = Load authentication tokens from launcher data to aid in testing of live servers.
cmd-launchauth-help = Usage: launchauth <account name>

cmd-lightbb-desc = Toggles whether to show light bounding boxes.
cmd-lightbb-help = Usage: lightbb

cmd-monitorinfo-desc = Monitors info
cmd-monitorinfo-help = Usage: monitorinfo <id>

cmd-setmonitor-desc = Set monitor
cmd-setmonitor-help = Usage: setmonitor <id>

cmd-physics-desc = Shows a debug physics overlay. The arg supplied specifies the overlay.
cmd-physics-help = Usage: physics <aabbs / com / contactnormals / contactpoints / distance / joints / shapeinfo / shapes>

cmd-hardquit-desc = Kills the game client instantly.
cmd-hardquit-help = Kills the game client instantly, leaving no traces. No telling the server goodbye.

cmd-quit-desc = Shuts down the game client gracefully.
cmd-quit-help = Properly shuts down the game client, notifying the connected server and such.

cmd-csi-desc = Opens a C# interactive console.
cmd-csi-help = Usage: csi

cmd-scsi-desc = Opens a C# interactive console on the server.
cmd-scsi-help = Usage: scsi

cmd-watch-desc = Opens a variable watch window.
cmd-watch-help = Usage: watch

cmd-showspritebb-desc = Toggle whether sprite bounds are shown
cmd-showspritebb-help = Usage: showspritebb

cmd-togglelookup-desc = Shows / hides entitylookup bounds via an overlay.
cmd-togglelookup-help = Usage: togglelookup

cmd-net_entityreport-desc = Toggles the net entity report panel.
cmd-net_entityreport-help = Usage: net_entityreport

cmd-net_refresh-desc = Requests a full server state.
cmd-net_refresh-help = Usage: net_refresh

cmd-net_graph-desc = Toggles the net statistics pannel.
cmd-net_graph-help = Usage: net_graph

cmd-net_watchent-desc = Dumps all network updates for an EntityId to the console.
cmd-net_watchent-help = Usage: net_watchent <0|EntityUid>

cmd-net_draw_interp-desc = Toggles the debug drawing of the network interpolation.
cmd-net_draw_interp-help = Usage: net_draw_interp <0|EntityUid>

cmd-vram-desc = Displays video memory usage statics by the game.
cmd-vram-help = Usage: vram

cmd-showislands-desc = Shows the current physics bodies involved in each physics island.
cmd-showislands-help = Usage: showislands

cmd-showgridnodes-desc = Shows the nodes for grid split purposes.
cmd-showgridnodes-help = Usage: showgridnodes

cmd-profsnap-desc = Make a profiling snapshot.
cmd-profsnap-help = Usage: profsnap

cmd-devwindow-desc = Dev Window
cmd-devwindow-help = Usage: devwindow

cmd-testopenfile-desc = Open file
cmd-testopenfile-help = Usage: testopenfile

cmd-scene-desc = Immediately changes the UI scene/state.
cmd-scene-help = Usage: scene <className>

cmd-szr_stats-desc = Report serializer statistics.
cmd-szr_stats-help = Usage: szr_stats

cmd-hwid-desc = Returns the current HWID (HardWare ID).
cmd-hwid-help = Usage: hwid

cmd-vvread-desc = Retrieve a path's value using VV (View Variables).
cmd-vvread-help = Usage: vvread <path>

cmd-vvwrite-desc = Modify a path's value using VV (View Variables).
cmd-vvwrite-help = Usage: vvwrite <path>

cmd-vvinvoke-desc = Invoke/Call a path with arguments using VV.
cmd-vvinvoke-help = Usage: vvinvoke <path> [arguments...]

cmd-dump_dependency_injectors-desc = Dump IoCManager's dependency injector cache.
cmd-dump_dependency_injectors-help = Usage: dump_dependency_injectors
cmd-dump_dependency_injectors-total-count = Total count: { $total }

cmd-dump_netserializer_type_map-desc = Dump NetSerializer's type map and serializer hash.
cmd-dump_netserializer_type_map-help = Usage: dump_netserializer_type_map

cmd-hub_advertise_now-desc = Immediately advertise to the master hub server
cmd-hub_advertise_now-help = Usage: hub_advertise_now

cmd-echo-desc = Echo arguments back to the console
cmd-echo-help = Usage: echo "<message>"

## 'vfs_ls' command
cmd-vfs_ls-desc = List directory contents in the VFS.
cmd-vfs_ls-help = Usage: vfs_list <path>
    Example:
    vfs_list /Assemblies

cmd-vfs_ls-err-args = Need exactly 1 argument.
cmd-vfs_ls-hint-path = <path>

cmd-reloadtiletextures-desc = Reloads the tile texture atlas to allow hot reloading tile sprites
cmd-reloadtiletextures-help = Usage: reloadtiletextures
