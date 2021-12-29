# IMPORTANT (PLEASE READ)
This is a deprecated piece of software. This is left available primarily for tool-assisted speedrunners that require a stable build of Dolphin that doesn't support Riivolution patches (if you want to run on Dolphin just to play a mod casually, you can right-click a game and select "Start with Riivolution patches" in the latest beta and development releases).

This tool has a low compatibility rate; it was created with NewerSMBW's patch as a reference and thus anything that works differently from NewerSMBW is likely to not work. Newer itself is also incompatible with USB Loaders, so this tool will not help you run Newer on one, although it may be helpful for running other simple mods on USB Loaders.

I (Meatball132) intend to create a better tool some time during the year 2022, hopefully sooner than later. Newer will also receive an update alongside the tool with support for USB Loaders.

# Asu's Riivolution Universal ISO Builder
A tool to patch Nintendo Wii ISO files using Riivolution XML files.

# Usage
UniversalISOBuilder.exe \<ISO Path\> \<Riivolution XML file path\> \<Root folder path\> \<Output ISO/WBFS path\> [options]

UniversalISOBuilder.exe [options]

UniversalISOBuilder.exe

(Note: In the 2nd and 3rd cases, you will be asked for the file paths.)

# Options
--silent                  -\> Prevents from displaying any console outputs apart from the necessary ones

--always-single-choice    -\> Enables by default any option that has only one choice

--never-single-choice     -\> Disable by default any option that has only one choice

--title-id \<TitleID\>      -\> Changes the TitleID of the output rom; Replace with dots the characters that should be kept

--keep-extracted-iso      -\> Prevents the extractedISO folder from being deleted after the end of the process

# Support
Need any help? Feel free to contact me on discord: \<b\>Asu-chan#2929\</b\>
