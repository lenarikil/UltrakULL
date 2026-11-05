# Discord logo asset

`UltrakULL/assets/discord_logo_white.png` is the official **white** Discord
symbol mark ("Clyde"), used by the translation browser's "open original post"
buttons. The buttons tint the icon through `Image.color`, so a white mark is
required (the blurple variant would fight the tint).

- Source: Discord's brand asset archive
  `https://cdn.discordapp.com/assets/content/80af2c38f13b4a7d2cb3572e1220f6e958d3c3aedccc7c7d3ddc9832f6b3d725.zip`
  (folder `Discord_Symbol_White`, file `Discord-Symbol-White.png`, 528x400).
- Usage guidelines: https://discord.com/branding
- The file is embedded into the mod DLL at build time
  (`<EmbeddedResource Include="assets\discord_logo_white.png" />` in
  `UltrakULL.csproj`) and loaded by `PrimitiveSprites.Get(Kind.Discord)`;
  the old procedural fallback remains for a missing resource.

This note documents a one-time manual asset step; the mod never downloads the
logo at runtime. To refresh the asset, re-download the archive, take the white
symbol PNG and copy it over `UltrakULL/assets/discord_logo_white.png`.
