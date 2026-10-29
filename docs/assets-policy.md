# Asset Policy

## Allowed licenses

- CC0/public-domain assets.
- CC-BY assets when the exact attribution is recorded in `CREDITS.md` and shipped with the game.
- Purchased assets with a license that explicitly permits commercial game distribution by this project.
- Original assets made for *The LightHouse*.

Do not use non-commercial, no-derivatives, unclear, scraped, or attribution-free copies of assets whose original license cannot be verified. Record the asset name, original source, license, URL, and date added before committing it.

## Budgets

- The strict limits live in `docs/performance-budget.md` and apply to every imported asset.
- Environment and hero textures: maximum 1K; never 4K.
- Small props and repeated set dressing: 512 or lower. Skyboxes/HDRIs may use up to 2K.
- Prefer packed masks, shared trim sheets, material instances, mipmaps, and texture compression.
- The 14+ Drowned share an instanced material, stay under 5k triangles at LOD0, provide two LODs, and keep only about four full-detail figures near the player.
- Transparent fog and beam geometry must remain sparse and layered deliberately to control overdraw on Apple Silicon.

Validate licenses before download and profile representative scenes on the target MacBook Air M3 before increasing any budget.
