# ChainedEchoesRandomizer
Archipelago compatible (planned feature) randomizer for Chained Echoes

INSTALLATION

A) INSTALL ARCHIPELAGO & AP WORLD
- Step 1: Install Archipelago from it's website (Get Started/Setup Guides for more information): https://archipelago.gg/

- Step 2: Launch Archipelago

- Step 3: Download the latest Chained Echoes AP World & YAML file from: https://github.com/Samupo/ChainedEchoesAPWorld/releases/ Install the AP World into Archipelago using the Launcher.

- Step 3.5: For more complex setups or more information about setting up Archipelago, please refer to its documentation: https://archipelago.gg/tutorial/Archipelago/setup/en This README will only cover basic setup for single player.

- Step 4: Edit your YAML file. The most important part is to add a player name, the rest can be default. Make sure the YAML file is copied into Archipelago's Players folder (it can be renamed).

- Step 5: Generate an Archipelago Game using the Archipelago's Launcher Generate option. A zip file should be generated in the output folder.

- Step 6: Host the Archipelago Game using the Host option in the launcher and selecting the previously generated zip file.

B) MOD INSTALLATION
- Step 1: Install BepInEx 6.0.0-pre.2 (BepInEx-Unity.IL2CPP-win-x64-6.0.0-pre.2.zip) by extracting it to the game's folder:
	Download link: https://github.com/BepInEx/BepInEx/releases

- Step 2: Launch the game once. If everything is setup correctly a console window should launch alongside the game.

- Step 3: Copy the files included in the game's folder. The .txt should be on the root alongside the .exe of the game, while the .dll should be in BepInEx/plugins

- Step 4: Edit RandomizerOptions.txt to add your Archipelago Username and Password (nothing by default). If you are not hosting it on your own computer you'll need to change the Archipelago Server as well (see: https://archipelago.gg/tutorial/Archipelago/setup/en for more information), if you are hosting it and did not change the address, keep it as is.

- Step 5: Feel free to tweak the seed and other randomizer options in that file.

- Step 6: Make sure the Archipelago Game server is open (using the Host button) before starting the game.






KNOWN ISSUES
- If chests don't open, it's likely that Archipelago server is down or the server address has changed. If doing single player try reopening Archipelago and Hosting again.

- Some skills will freeze/crash the game when used by specific characters. If this happens consistently feel free to report the specific combination to the developer. Some skills may be to difficult to fix, though.