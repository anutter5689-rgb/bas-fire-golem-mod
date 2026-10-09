# Building the Fire Golem mod (Windows PC required)

## What you need
1. **Blade & Sorcery 1.0** on Steam (PCVR).
2. **Unity 2021.3.38f1** (exact version matters - asset bundles built on the
   wrong Unity version won't load).
3. The official **B&S SDK** (via the in-game mod manager, or the modding
   wiki's SDK setup guide).
4. **.NET SDK** (any recent one) for compiling the DLL.

## Steps
1. Clone this repo into your SDK's `Mods` source folder.
2. Edit `FireGolem.csproj` - set `BasGameDir` to your game install path.
3. `dotnet build` - drop the resulting `FireGolem.dll` next to `manifest.json`.
4. Fill in the JSON files (see the comments in `JSON/`) using your dumped
   `bas.jsondb` as the source for the vanilla golem fields.
5. Optional but recommended: in the SDK, recolor the golem material
   (ember/orange crystal glow) and replace the placeholder sphere fireball
   with a proper fire projectile prefab + the game's fire spell effects.

## Tuning cheat-sheet (all in FireGolemController.cs)
- `fireballCooldown` - seconds between single fireballs
- `specialCooldown` - seconds between flame volleys (the special)
- `volleyCount` / `volleySpreadDegrees` - special shape
- `groundFireDuration` / `groundFireDps` - how the ground burn feels

## Known TODOs before release
- Hook projectiles to the golem's throw animation frames.
- Replace placeholder effect IDs with real catalog EffectData ids.
- Replace placeholder projectile mesh with SDK fireball prefab.
- Verify ThunderRoad API signatures against the installed SDK version.
