# InisLand build validation — 2026-09-08

- Source version: 0.5.0. Existing portable: 0.3.0.
- `Sfx.cs` now maps UI cues to reviewed CC0 clips in `Resources/Audio/generated`; no `AudioClip.Create`, `MakeSine`, or `MakeSweep` remains in source.
- Existing 0.3.0 portable stayed alive for 20 seconds.
- A new export was not produced: the required Unity 2022.3.62f3 installation has no `Unity.exe`. Unity 6000 was used only on an isolated temporary copy; its headless entitlement was unavailable, so no compile-success claim is made.
