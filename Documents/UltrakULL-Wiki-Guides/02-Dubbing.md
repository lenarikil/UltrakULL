# Dubbing Guide

## Audio Format
- Recommended: `.ogg`
- Also supported: `.mp3`, `.wav`

## File Structure
```
[pack-folder]/audio/
├── gabrielIntro1.ogg
├── minosPrimeIntro1.ogg
└── Intermission1/
    └── [ObjectName]/
        ├── line1.ogg
        └── line2.ogg
```

## Subtitle Timing Reference
These timings come from the game's dubbing setup. See the original [Dubbing.md](../../UltrakULL/docs/Dubbing.md) for the full table.

| Level/Character | File/Line | Subtitle Delays (seconds) | Notes |
|---|---|---|---|
| 3-2 Gabriel | gabrielIntro1 | 1.2;2.4;5.1;10;7 | ~1.2s echo fade-in at start |
| 3-2 Gabriel | gabrielDefeated | 0.1;1.4;4.15;6.4;9.2;13 | |
| 3-2 Gabriel | gabrielOutro | 0.3,3.3 | Reverb is applied by the game |
| 4-3 Mandalore | mandaloreIntro | 0.0 | Plays at the same time as owlIntro |
| 4-3 | owlIntro | 3.1 | ~3.1s silence at start |
| 5-3 | gabrielBoat | 0.0;2.0;5.0;6.5;9.5 | Contains rewind sound effect at the end |
| 6-1 | gabrielHeresyFirst | 2.0;5.0;6.5;9.5 | ~2s echo fade-in at start |
| 6-2 Gabriel (2nd) | gabrielSecondIntro | 1.85;3.3;4.7;6.3;10.1;12.7;15.3;18.5;22.3;25.8;28.8;30.5 | Echo fade-in at start |
| P-1 Minos Prime | minosPrimeIntro | 0.0;2.4;7.0;10.0;13.8;22.0;25.5;30.5;37.0;40.7;44.0 | |
| P-1 Minos Prime | minosPrimeDeathScream | - | Plays immediately after defeated, not tied to animation timing |
| P-2 Sisyphus Prime | sisyphusIntro | 0.0;1.7;6.6;10.3;15.8;17.2;22.0;25.9;31.4;35.0;37.4 | |

## Audio Processing (Audacity)

### Gabriel Reverb Effect
1. Open your Gabriel voice line in Audacity
2. Select the clip and go to `Effect > Reverse`
3. Go to `Effect > Reverb` with these settings:

| Setting | Value |
| --- | --- |
| Pre-Delay | 10ms |
| Reverberance | 70% |
| Damping | 40% |
| Tone Low | 100% |
| Tone High | 80% |
| Wet Gain | -1dB |
| Dry Gain | -1dB |
| Stereo Width | 100% |

4. Go to `Effect > Reverse` again

Other characters have specific processing details documented as they're discovered.

## Enabling Dubbing

Edit `BepInEx/config/ultrakull/Settings.cfg`:
```ini
[General]
activeDubbing = True
extraDubbing = True
```

## Placement
Place files in `BepInEx/config/ultrakull/[pack-folder]/audio/`

If a file is missing or fails to load, the game will fall back to the original English audio for that line.