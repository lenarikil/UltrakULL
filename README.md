[![Discord](https://img.shields.io/discord/1017473804592754778?label=UltrakULL%20Discord)](https://discord.gg/ZB7jk6Djv5 "Discord Invite")
![Version](https://img.shields.io/github/v/release/ClearwaterTM/UltrakULL)
![Licence](https://img.shields.io/github/license/ClearwaterTM/UltrakULL)

[![Typing SVG](https://readme-typing-svg.demolab.com?font=Fira+Code&weight=800&size=29&pause=1000&color=F7F7F7&background=DB20FF27&center=true&vCenter=true&repeat=false&width=1000&lines=This+project%2Fmod+was+created+with+the+partial+help+of+AI.+%F0%9F%A4%96)](https://git.io/typing-svg)

<img src="https://github.com/user-attachments/assets/98fd5921-0662-4e09-9ab4-b844e2bbf45e">

# UltrakULL ReFORKED

**UltrakULL** (ULTRAKILL Language Library) is a mod for ULTRAKILL that allows translating game text, voice lines, and replacing textures/fonts. Its main purpose is to bridge the localization gap until official translations are added.

## Features

- **Full game translation** - Nearly all in-game text is translated via JSON files
- **Multiple language packs** - Support for multiple translations side by side
- **In-game translation browser** - Built-in UI in the Options menu to browse, download and switch language packs
- **Instant language switching** - Change language on-the-fly without restarting the game
- **Dubbing support** - Replace voice lines with translated audio, including special effects for certain characters
- **Custom fonts** - Use custom fonts per language pack
- **Texture replacement** - Replace textures via the `textures/` folder in your language pack
- **Cyrillic support** - Works great with Russian, Ukrainian, Belarusian and other Cyrillic languages
- **Auto-fill missing keys** - When updating the mod, outdated language packs are automatically filled with values from the built-in English template

> Right-to-left language support is currently unstable and under rework.

## Download & Installation

**Recommended:** Install via [Thunderstore](https://thunderstore.io/c/ultrakill/p/UltrakULL/UltrakULL/) using [r2modmanPlus](https://github.com/ebkr/r2modmanPlus). Dependencies (including BepInEx) will be installed automatically.

**Manual:** Download the latest release from [GitHub Releases](https://github.com/lenarikil/UltrakULL/releases) and [BepInEx 5.4.21 x64](https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip). See the [installation guide](https://github.com/lenarikil/UltrakULL/wiki/Installation-and-Usage) or the guides in `Documents/UltrakULL-Wiki-Guides/`.

**Requirements:** Latest version of ULTRAKILL on Steam. Demo, GOG, or cracked versions are not guaranteed to be supported.

## Usage

1. Install the mod and launch the game
2. Go to **Options → Languages** - a new Languages tab with the translation browser will appear
3. Select any installed language pack from the list
4. The language applies instantly with no restart required

<img src="https://github.com/user-attachments/assets/d461c5a3-2de2-4d7c-b390-60235f70b21d" alt="UltrakULL Languages menu">

## Languages

See the release notes or [Discord community](https://discord.gg/ZB7jk6Djv5) for a list of available translations and their contributors. You can also create and use your own language packs.

## Creating Your Own Language Pack

Detailed guides have been moved out of the README to keep it clean:

- **[Creating a Language Pack](./Documents/UltrakULL-Wiki-Guides/01-Creating-a-Language-Pack.md)** - Structure, metadata, validation
- **[Dubbing](./Documents/UltrakULL-Wiki-Guides/02-Dubbing.md)** - Audio setup, subtitle timings, Audacity workflow
- **[Installation & Usage](./Documents/UltrakULL-Wiki-Guides/03-Installation-and-Usage.md)** - Setup, config, troubleshooting

*These files can be copied directly to the GitHub Wiki.*

## Troubleshooting

- **Mod doesn't load or breaks after ULTRAKILL update** - Major game updates may break the mod. Check the [Discord](https://discord.gg/ZB7jk6Djv5) for fixes.
- **Language doesn't appear in the menu** - Validate your `translation.json` with [jsonlint.com](https://jsonlint.com/). Make sure the pack is in `BepInEx/config/ultrakull/[pack-folder]/` with a valid `translation.json`. Check `BepInEx/LogOutput.log` or the BepInEx console for errors.
- **Dubbing issues** - Ensure `activeDubbing` and/or `extraDubbing` are enabled in `BepInEx/config/ultrakull/Settings.cfg`. Use `.ogg` format and correct file paths.
- **Other issues** - Ask for help in the [UltrakULL Discord](https://discord.gg/ZB7jk6Djv5).

## FAQ

- **Can I translate ULTRAKILL into my language?** Yes. Follow the guides above or reach out on Discord.
- **Will this affect my saves?** No, the mod only changes text, audio and textures.
- **Will this affect Cybergrind highscores?** No.
- **Is it compatible with other mods?** Not guaranteed. Mods that don't modify HUD messages usually work fine.

## For Developers

<details>
<summary>Build instructions</summary>

### Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with .NET Framework 4.7.2 targeting pack, or [JetBrains Rider](https://www.jetbrains.com/rider/)
- [.NET SDK](https://dotnet.microsoft.com/download) compatible with .NET Framework 4.7.2
- A legal copy of ULTRAKILL (Steam recommended)

### Setup & Build
1. Clone: `git clone https://github.com/lenarikil/UltrakULL`
2. Set environment variable `ULTRAKILLPath` to your ULTRAKILL installation folder
3. Open `UltrakULL.sln`
4. Build in Release: `dotnet build UltrakULL.sln -c Release`
5. Output goes to `UltrakULL/bin/Release/plugins/UltrakULL/`

### Notes
- Flags are embedded from `UltrakULL/flags/flags.zip`
- Templates and assets are copied during build
- Target framework: `net472`, C# `7.3`
</details>

## Credits & Links

- **Credits:** [CREDITS.md](./UltrakULL/CREDITS.md)
- **ULTRAKILL on Steam:** https://store.steampowered.com/app/1229490/ULTRAKILL/
- **New Blood Discord:** https://discord.gg/newblood
- **UltrakULL Discord:** https://discord.gg/ZB7jk6Djv5
- **Original Dubbing Docs:** [UltrakULL/docs/Dubbing.md](./UltrakULL/docs/Dubbing.md)

## Third-Party Dependencies

| Component | License | Source |
|-----------|---------|--------|
| **7-Zip (`7z.exe`)** | **GNU LGPL v2.1 (unRAR restricted)** | https://www.7-zip.org/, Igor Pavlov |
| **Discord logo (white symbol mark)** | Discord Brand Assets / Branding Guidelines | https://discord.com/branding; `assets/discord_logo_white.png` |
| **Warning icon (white caution mark)** | CityPNG Personal Use License (non-commercial only) | https://www.citypng.com/photo/19198/white-caution-mark-warning-sign-icon-hd-png; https://www.citypng.com/page/personal-use-license; `assets/warning_icon.png` |

**7-Zip usage note:** `7z.exe` is launched as a separate process via `System.Diagnostics.Process`. It is **not** compiled into or linked with UltrakULL. UltrakULL itself remains under the **MIT** license; this section only attributes the separately-distributed 7-Zip binary used for 7z/zip archive extraction.

**Discord logo note:** `assets/discord_logo_white.png` is the official white Discord symbol mark (“Clyde”). It is embedded into the mod DLL at build time and used by the translation browser’s “open original post” buttons, tinted through `Image.color`. See `tools/discord-logo.md` for provenance and refresh instructions.

**Warning icon note:** `assets/warning_icon.png` is the white caution/warning mark shown when a remote pack logo or gallery image fails to load. Source: CityPNG — https://www.citypng.com/photo/19198/white-caution-mark-warning-sign-icon-hd-png. See `UltrakULL/UI/TranslationBrowser/PrimitiveSprites.cs` for attribution.

Place `7z.exe` (and `LICENSE-7z.md`) in `BepInEx/plugins/UltrakULL/tools/7z.exe` (or make it available via PATH) so the translation pack installer can extract 7z and zip archives. Both files are included in this repository under `UltrakULL/tools/` and are copied to the plugin output on every Release build.
