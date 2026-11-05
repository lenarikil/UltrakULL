# AGENTS.md — UltrakULL

## Communication

**Always respond in the same language as the user's message.** Do not force Russian (or any other language). Follow the user's language throughout the conversation. When the user writes in Russian, reply in Russian; when they write in English, reply in English.

## Project

UltrakULL is a runtime translation and localization mod for the compiled ULTRAKILL game.

This is a **BepInEx/Harmony mod project**, not a normal Unity project.

The project modifies the running game through runtime patches, localization systems,
texture replacement, audio replacement, font handling, UI changes, and related utilities.

## Quick Reference

- **Build:** `dotnet build UltrakULL.sln -c Release` (requires `ULTRAKILLPath` env var)
- **Solution:** `UltrakULL.sln` → single project `UltrakULL/UltrakULL.csproj` (net472, C# 7.3)

