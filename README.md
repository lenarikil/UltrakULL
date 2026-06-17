[![Discord](https://img.shields.io/discord/1017473804592754778?label=UltrakULL%20Discord)](https://discord.gg/ZB7jk6Djv5 "Discord Invite")
![Version](https://img.shields.io/github/v/release/ClearwaterTM/UltrakULL)
![Licence](https://img.shields.io/github/license/ClearwaterTM/UltrakULL)

[![Typing SVG](https://readme-typing-svg.demolab.com?font=Fira+Code&weight=800&size=29&letterSpacing=&pause=1000&color=F7F7F7&background=DB20FF27&center=true&vCenter=true&repeat=false&width=1000&lines=This+project%2Fmod+was+created+with+the+partial+help+of+AI.+%F0%9F%A4%96)](https://git.io/typing-svg)
<img src="https://github.com/user-attachments/assets/98fd5921-0662-4e09-9ab4-b844e2bbf45e">

# **UltrakULL ReFORKED**

**UltrakULL** (ULTRAKILL Language Library) is a modification (mod) for ULTRAKILL that allows for modification of the game's text strings, voiceovers and textures effectively allowing for translation and localization into various languages.\
This mod's primary purpose is to bridge the gap for localization and translation until ULTRAKILL receives official translations.

# Features

- Translates the entire game from English to any language
- Support for multiple languages
- Easy font changing
- Replacing certain textures by simply changing the .png files, without having to modify the game files
- JSON formatting of language files allows for easy-to-understand, simple-to-do modification of strings
- Change languages directly in-game without having to restart game
- Dubbing support allows for translated spoken dialogue
- ~~Supports right-to-left languages such as Arabic and Persian~~ **(At the moment, the feature has a large number of critical bugs. It will be reworked in the future)**
- Cyrillic character support for languages such as Russian, Ukrainian and Belarusian


# Download & Installation

UltrakULL can be obtained either through the [Releases page](https://github.com/lenarikil/UltrakULL/releases) (*recommended*),
or via the [UltrakULL Discord.](https://discord.gg/ZB7jk6Djv5)

The only requirement is to have the latest version of ULTRAKILL on Steam. <br>**Support for Demo, GOG.com, and Hacked versions is NOT guaranteed**

## Installation via Thunderstore

UltrakULL is available as a [ThunderStore mod.](https://thunderstore.io/c/ultrakill/p/UltrakULL/UltrakULL/)

Download and installation can be done automatically via the [R2ModManager](https://github.com/ebkr/r2modmanPlus/releases/download/v3.1.42/r2modman-3.1.42.exe).

(Downloading the mod via R2ModManager will also automatically download and install the required BepInEx dependencies.)

## Installation via GitHub

Installing UltrakULL via GitHub is divided into 2 parts:
- Installing BepInEx, the modding framework
- Installing UltrakULL, the mod itself

#### Installing BepInEx:
- Download [BepInEx 5.4.21 64-bit here.](https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip)
- Extract the contents of BepInEx to where your ULTRAKILL install folder is located.
- Launch ULTRAKILL once so BepInEx can generate the required files and folders in the install folder. Quit the game once it has loaded to the main menu.

#### Installing UltrakULL:
- Extract the contents of UltrakULL to your BepInEx folder. Overwrite any files if prompted.


### Usage

- If the mod has loaded correctly, you will see a new "Languages" tab in the Options menu.
  
<img src="https://github.com/user-attachments/assets/d461c5a3-2de2-4d7c-b390-60235f70b21d" alt="drawing">

  
- In the "Languages" tab, you can view the translations installed locally in the mod folder, and you can select any available language based on the language files found by UltrakULL and load them into the game.

### Troubleshooting

If the mod does not appear to load or work correctly, or errors occur in-game, please open the BepInEx console.

#### Opening via R2ModManager
Open your mod profile in R2ModManager, and go to Config Editor -> BepInEx.cfg -> Edit Config -> Scroll to "Logging". Set both options to True.

#### Opening via GitHub
Navigate to BepInEx/config/BepInEx.cfg, and open it in any text editor of your choosing. Scroll down to Logging and set
"UnityLogListening" and "LogConsoleToUnityLog" to True.

Restart the game after applying either steps for your use case.


# Languages

## Available languages and their creators, before the mod's transition to the SDK platform
| Language                                 | Contributors                                                                                                                       |
|------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------|
| English (U.S)                            | Hakita & New Blood                                                                                                                 |
| Brazilian Portugese (Portugês do Brasil) | Veni, Jackie, MKaid, hebert, FNChannel, Spooky, Soulvender, RAYLANDER                                                              |
| Czech (Čeština)                          | Mina                                                                                                                               |
| Filipino (Pilipino)                      | mxkyle, MecanicWithAPistol, FinnianNiko                                                                                            |
| French (Français)                        | Clearwater, ZedDev, Frizou, osokour, Tamary, Uranus, Lays                                                                          |
| German (Deutsch)                         | Distrilul, JESTERB0T, Liquid Lest, Psychologemelone44, Termi2, Fabidelune, Madeleine                                               |
| Korean (한국어)                           | ARSE™, Susu                                                                                                                        |
| Russian (русский)                        | Nessie_A_WA97, D4N5T3P, Edith Bagel, lrddd, Brainy-Stormie, TwinT, towelie84, mctaylors, Solidus Cumcer, Filin, Ega1232387, Khowst |
| Spanish (Español)                        | LambCS, Philia, Lukah, Amarok_Lc, Santy, Radripizza, j(LRC), LEVIBOT                                                               |
| Simplified Chinese (简体中文)             | Hydracerynitis, ciinore, duke325, ponyweeb, Skugra, GoGoblin                                                                       |
| Turkish (Türkçe)                         | Legitname1337, Ömer Talha, RTE, Ray_, legio, Scape, Neige,$ERTU$TAUPTOWN                                                           |

# Troubleshooting

### ULTRAKILL received an update, and UltrakULL is now broken/not working correctly.
As is the case with most updates for other games,
any and all updates and hotfixes to ULTRAKILL will almost certainly break mod functionality
to some degree. Work to future-proof the mod as much as possible is done to minimise such occurrences,
but if an update breaks the mod, it will be fixed as quickly as possible.

### My language does not appear as selectable in-game in the language tab.

Language files are formatted in JSON. If it does not appear as available, it is either not formatted correctly
or does not match the minimum version required by the mod.\
To check if a file is formatted correctly, open [JSONLint](https://jsonlint.com/) in your browser, copy and paste the contents
of your file into the window and click on "Validate JSON". \
If the file is not formatted correctly, JSONLint will report any errors.
Errors can be forwarded to the UltrakULL Discord's troubleshooting channel for assistance.

### My problem is not listed here.

A dedicated troubleshooting and support channel can be found at the [UltrakULL Discord](https://discord.gg/ZB7jk6Djv5).

# FAQ

### Can I translate ULTRAKILL into my native language with this mod?

Indeed you can! Thanks to this mod, ULTRAKILL has already been translated into various foreign languages,
including French, Brazilian Portugese, Traditional Chinese, with many other languages also in development at the time of release.\
If you wish to contribute to, or begin work on a new or existing translation or language, feel free to stop by and inquire
at the [UltrakULL Discord.](https://discord.gg/ZB7jk6Djv5)

### Will this mod affect my saves?

No, this mod merely changes text in the game. It does not alter your saves in any way.

### Will this mod prevent Cybergrind highscores?

No, for the same reason as above. It does not alter any gameplay aspects that would give an unfair advantage in any way, and as such,
will be safe to set Cybergrind highscores with.
If for some reason your Cybergrind highscores are not being submitted, and you are sure they should be doing so, feel free to shoot a message
on our Discord and I will take a look at it.

### Can voice lines from characters be translated?

As of UltrakULL v1.1.0, dubbing support is available for speaking characters! To learn more about how to add your own lines, check the [dubbing documentation](https://github.com/lenarikil/UltrakULL/blob/master/UltrakULL/docs/Dubbing.md).

### Where can I follow UltrakULL's development?
I usually like to post updates and news about development in the **[UltrakULL Discord](https://discord.gg/ZB7jk6Djv5)**

### Is UltrakULL compatible with other mods?
I cannot guarantee mod compatability with other mods. Mods that do not use the HUD message display functionality
should work just fine though.


# Documentation

GitHub documentation coming in future. Until then, documentation on how to create your own language
can be found in the [UltrakULL Discord.](https://discord.gg/ZB7jk6Djv5)

# Building
###  (This info is for developers. If you only want to play/use the mod, you do not need to read this.)
1) Clone the repository.<br>`git clone https://github.com/lenarikil/UltrakULL`
2) Set ULTRAKILLPath as an environment variable, which points to your game installation. <br> This is used to automatically acquire any necessary .dll files from the game location to build the mod.
3) Open the project solution in the IDE of your choice (Visual Studio, Rider, etc.)
4) Build the solution. The solution will automatically set up the folder structure, and will drop compiled mod as a DLL.dll file into BepInEx/plugins/UltrakULL.<br>
If you have any language templates or dubbing audio files, the project will automatically copy those to config/ultrakull.
5) Drop all the generated files and folders into: `[Your Steam folder]/steamapps/common/ULTRAKILL/`<br> Overwrite any files if prompted.

# Credits & Contributors
View [CREDITS.md](./UltrakULL/CREDITS.md) for full crediting information.

# Links

ULTRAKILL Steam page: https://store.steampowered.com/app/1229490/ULTRAKILL/ \
ULTRAKILL/New Blood Discord:  https://discord.gg/newblood \
UltrakULL Discord: https://discord.gg/ZB7jk6Djv5
