# Creating a Language Pack for UltrakULL

## 1. Pack Structure

Each language pack is a folder in `BepInEx/config/ultrakull/`. The folder name is the pack key (must be unique). You can use a language code like `ru-RU` or any readable name.

```
[pack-folder]/
├── translation.json        # Main translation file (required)
├── audio/                  # Dubbed voice lines (optional)
│   ├── *.ogg               # Main dubbing
│   └── Intermission1/      # Extra intermission dubbing
├── textures/               # Texture replacements (optional)
└── fonts/                  # Custom fonts (optional)
```

## 2. Creating translation.json

1. Copy `UltrakULL/json/templates/en-GB.json` from the repo as your base
2. Fill in `metadata`:
   - `languageCode` - Language code (e.g. `ru-RU`)
   - `languageName` - Display name in the menu (e.g. `Russian`)
   - `langAuthors` / `credits` - Translator credits
   - `translationVersion` - Your pack version
   - `rightToLeft` - Set to `true` only if absolutely necessary (RTL is still being reworked)
   - `minimumModVersion` - Minimum UltrakULL version tested
   - `fonts` - Font mappings if using custom fonts
3. Translate the rest of the JSON. Missing keys will be auto-filled from the built-in English template on load (a warning will be logged), but it's best to translate as much as possible.

## 3. Dubbing

See [Dubbing.md](./02-Dubbing.md) for details. Quick summary:
- Format: `.ogg` (recommended). `.mp3` and `.wav` are also supported
- Main dubbing goes in `audio/`
- Extra intermission dubbing: `audio/IntermissionX/[ObjectName]/line1.ogg`, `line2.ogg`, etc.
- Enable in `BepInEx/config/ultrakull/Settings.cfg`: `activeDubbing = True`, `extraDubbing = True`

## 4. Textures and Fonts

- `textures/` - Replace game textures (follow the same structure as expected by the mod)
- `fonts/` - Custom TrueType/OpenType fonts for TextMeshPro. Reference them in `metadata.fonts` if needed

## 5. Testing

1. Put your pack folder into `BepInEx/config/ultrakull/`
2. Launch the game - the mod will scan for language packs
3. Open **Options → Languages** and select your pack
4. If it doesn't appear or errors occur, check `BepInEx/LogOutput.log` or the BepInEx console
5. Validate JSON with [jsonlint.com](https://jsonlint.com/) if you see parsing errors

## Tips

- Use valid JSON
- Don't remove required `metadata` fields
- The pack folder name is the unique key
- When sharing, zip the entire pack folder
- Keep backups of your `translation.json` before testing big changes


