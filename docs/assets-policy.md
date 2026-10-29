# Asset Policy

## Allowed licenses

- CC0/public-domain assets.
- CC-BY assets when the exact attribution is recorded in `CREDITS.md` and shipped with the game.
- Purchased assets with a license that explicitly permits commercial game distribution by this project.
- Original assets made for *The LightHouse*.

Do not use non-commercial, no-derivatives, unclear, scraped, or attribution-free copies of assets whose original license cannot be verified. Record the asset name, original source, license, URL, and date added before committing it.

## Budgets

- Environment and hero textures: maximum 2K per texture.
- Small props and repeated set dressing: 1K or lower.
- Prefer packed masks, shared trim sheets, material instances, mipmaps, and texture compression.
- The 14+ Drowned must share materials and rigs where possible. Target roughly 8–15k triangles per nearby figure and 2–5k for distance variants; use LODs and avoid unique 2K maps per figure.
- Transparent fog and beam geometry must remain sparse and layered deliberately to control overdraw on Apple Silicon.

Validate licenses before download and profile representative scenes on the target MacBook Air M3 before increasing any budget.
