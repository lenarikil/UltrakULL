# Installation & Usage

## Requirements
- A legitimate copy of ULTRAKILL on Steam (Demo, GOG, or pirated versions are not guaranteed to be supported)

## Installation Methods

### Thunderstore (Recommended)
1. Install [r2modmanPlus](https://github.com/ebkr/r2modmanPlus)
2. Search for `UltrakULL` in Thunderstore and install it to your mod profile
3. Launch the game through r2modmanPlus

### Manual Installation
1. Download [BepInEx 5.4.21 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21)
2. Extract BepInEx into your ULTRAKILL installation folder
3. Launch the game once and quit to the main menu so BepInEx generates the required folders
4. Download the latest UltrakULL release from [GitHub Releases](https://github.com/lenarikil/UltrakULL/releases) or the [UltrakULL Discord](https://discord.gg/ZB7jk6Djv5)
5. Extract the contents of the release into your BepInEx folder (overwrite if prompted)

## Usage

1. After loading, go to **Options → Languages**
2. The Languages tab with the translation browser will show all installed language packs from `BepInEx/config/ultrakull/`
3. Click on a pack to select it - the language changes instantly with no restart
4. Use "Open Language Folder" to quickly access your language packs directory

## Configuration

Config file: `BepInEx/config/ultrakull/Settings.cfg`

- `LastLanguage` - The pack folder name of the last selected language
- `activeDubbing` - Enable main voice line dubbing
- `extraDubbing` - Enable extra intermission dubbing

## Troubleshooting

- **BepInEx console**: Enable logging in `BepInEx/config/BepInEx.cfg` (`UnityLogListening = true`, `LogConsoleToUnityLog = true`) or via r2modmanPlus Config Editor
- **Language pack not showing**: Check JSON validity, folder structure, and ensure `translation.json` exists. Check `BepInEx/LogOutput.log` for errors
- **Mod broken after game update**: ULTRAKILL updates often break mods. Wait for an UltrakULL update
- **Dubbing not playing**: Verify settings in `Settings.cfg`, use supported audio formats, and check file paths

## Compatibility
Compatibility with other mods is not guaranteed. Mods that don't interfere with HUD messages generally work fine.