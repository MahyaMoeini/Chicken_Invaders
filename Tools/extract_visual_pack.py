"""Extract clean individual sprites from ChickenInvaders_VisualPack sheets."""
from pathlib import Path
from PIL import Image
import numpy as np
import hashlib
import json
import shutil

ROOT = Path(r"d:\Unity\projects\Chicken Invaders\Assets")
PACK = ROOT / "ChickenInvaders_VisualPack"
ART = ROOT / "Art"


def guid_for(name: str) -> str:
    return hashlib.md5(("ci-art-" + name.replace("\\", "/")).encode()).hexdigest()


def write_meta(png_path: Path, guid: str, ppu: int = 100) -> None:
    meta = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  - serializedVersion: 4
    buildTarget: Standalone
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: {guid}
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    png_path.with_suffix(".png.meta").write_text(meta, encoding="utf-8")


def find_blobs(im, min_w=30, min_h=30, alpha_thresh=18, min_count=200):
    arr = np.array(im)
    alpha = arr[:, :, 3]
    mask = alpha > alpha_thresh
    h, w = mask.shape
    visited = np.zeros_like(mask, dtype=bool)
    blobs = []
    for y in range(h):
        xs = np.where(mask[y] & ~visited[y])[0]
        for x in xs:
            if visited[y, x]:
                continue
            stack = [(int(x), int(y))]
            visited[y, x] = True
            minx = maxx = int(x)
            miny = maxy = int(y)
            count = 0
            while stack:
                cx, cy = stack.pop()
                count += 1
                for nx, ny in ((cx - 1, cy), (cx + 1, cy), (cx, cy - 1), (cx, cy + 1)):
                    if 0 <= nx < w and 0 <= ny < h and mask[ny, nx] and not visited[ny, nx]:
                        visited[ny, nx] = True
                        stack.append((nx, ny))
                        minx = min(minx, nx)
                        maxx = max(maxx, nx)
                        miny = min(miny, ny)
                        maxy = max(maxy, ny)
            bw = maxx - minx + 1
            bh = maxy - miny + 1
            if bw >= min_w and bh >= min_h and count >= min_count:
                blobs.append((minx, miny, bw, bh, count))
    return blobs


def crop_pad(im, box, pad=2):
    x, y, w, h, _ = box
    x0 = max(0, x - pad)
    y0 = max(0, y - pad)
    x1 = min(im.width, x + w + pad)
    y1 = min(im.height, y + h + pad)
    return im.crop((x0, y0, x1, y1))


def save_sprite(img, rel_name, ppu, manifest):
    path = ART / rel_name
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path)
    g = guid_for(rel_name.replace("\\", "/"))
    write_meta(path, g, ppu)
    manifest[rel_name.replace("\\", "/")] = g
    print("saved", rel_name, img.size, g)
    return g


def main():
    if ART.exists():
        shutil.rmtree(ART, ignore_errors=True)
    ART.mkdir(parents=True, exist_ok=True)
    manifest = {}

    # Player sheet
    im = Image.open(PACK / "Sprites/Player/player_and_combat_sheet.png").convert("RGBA")
    blobs = find_blobs(im, 28, 28)
    ships = sorted([b for b in blobs if b[3] > 280 and b[2] > 140], key=lambda b: b[0])
    lasers = sorted(
        [b for b in blobs if 100 < b[3] < 370 and b[2] < 180 and 350 < b[1] < 800],
        key=lambda b: b[0],
    )
    explosions = sorted(
        [b for b in blobs if b[1] > 750 and b[0] < 700 and 100 < b[2] < 220],
        key=lambda b: b[0],
    )
    exhaust = sorted([b for b in blobs if b[1] > 800 and b[0] > 900], key=lambda b: b[0])
    rocket = [b for b in blobs if 100 < b[2] < 130 and b[3] > 240 and 400 < b[1] < 750]

    if len(ships) >= 2:
        save_sprite(crop_pad(im, ships[0]), "Player/ship_healthy.png", 64, manifest)
        save_sprite(crop_pad(im, ships[1]), "Player/ship_damaged.png", 64, manifest)
    for i, b in enumerate(lasers[:3]):
        save_sprite(crop_pad(im, b), f"Player/laser_{i + 1}.png", 64, manifest)
    if rocket:
        save_sprite(crop_pad(im, rocket[0]), "Player/missile.png", 64, manifest)
    for i, b in enumerate(explosions[:4]):
        save_sprite(crop_pad(im, b), f"VFX/explosion_{i}.png", 64, manifest)
    for i, b in enumerate(exhaust[:4]):
        save_sprite(crop_pad(im, b), f"VFX/exhaust_{i}.png", 64, manifest)

    # Props
    im = Image.open(PACK / "Sprites/Props/eggs_asteroids_pickups_sheet.png").convert("RGBA")
    blobs = find_blobs(im, 40, 40, min_count=500)
    mapping = []
    for b in blobs:
        x, y, w, h, c = b
        cx = x + w / 2
        cy = y + h / 2
        if cy < 360 and cx < 450 and w > 180:
            mapping.append(("Props/egg.png", b, 80))
        elif cy < 360 and 450 < cx < 850:
            mapping.append(("Props/egg_cracked.png", b, 80))
        elif cy < 360 and cx > 850:
            mapping.append(("Props/egg_fire.png", b, 80))
        elif 360 < cy < 700 and cx < 450:
            mapping.append(("Props/asteroid_large.png", b, 80))
        elif 360 < cy < 700 and 450 < cx < 800:
            mapping.append(("Props/asteroid_medium.png", b, 80))
        elif 360 < cy < 700 and 800 < cx < 1100:
            mapping.append(("Props/asteroid_small.png", b, 100))
        elif 360 < cy < 700 and cx > 1100:
            mapping.append(("Props/asteroid_tiny.png", b, 100))
        elif cy > 700 and cx < 300:
            mapping.append(("Pickups/weapon_gift.png", b, 80))
        elif cy > 700 and 280 < cx < 450:
            mapping.append(("Pickups/food_drumstick.png", b, 80))
        elif cy > 700 and 450 < cx < 650:
            mapping.append(("Pickups/food_burger.png", b, 80))
        elif cy > 700 and 650 < cx < 900:
            mapping.append(("Pickups/bomb.png", b, 80))
        elif cy > 700 and 900 < cx < 1150:
            mapping.append(("Pickups/heart.png", b, 80))
        elif cy > 700 and cx > 1150:
            mapping.append(("Pickups/food_meter.png", b, 80))

    seen = set()
    for rel, b, ppu in mapping:
        if rel in seen:
            continue
        seen.add(rel)
        save_sprite(crop_pad(im, b), rel, ppu, manifest)

    # Enemy stages
    stages = {
        "normal": "Stage1/stage1_simple_chickens_sheet.png",
        "armored": "Stage2/stage2_armored_chickens_sheet.png",
        "alien": "Stage3/stage3_alien_chickens_sheet.png",
    }
    for label, relpath in stages.items():
        im = Image.open(PACK / "Sprites/Enemies" / relpath).convert("RGBA")
        blobs = find_blobs(im, 100, 100, min_count=800)
        blobs_sorted = sorted(blobs, key=lambda b: -(b[2] * b[3]))
        boss = blobs_sorted[0]
        chickens = [b for b in blobs_sorted[1:] if b[2] * b[3] < boss[2] * boss[3] * 0.5]
        chickens.sort(key=lambda b: (round((b[1] + b[3] / 2) / 40), b[0]))
        rows = []
        for b in chickens:
            cy = b[1] + b[3] / 2
            if not rows or abs(cy - rows[-1][0]) > 60:
                rows.append([cy, [b]])
            else:
                rows[-1][1].append(b)
        for r in rows:
            r[1].sort(key=lambda b: b[0])
        if rows:
            for i, b in enumerate(rows[0][1][:4]):
                save_sprite(crop_pad(im, b), f"Enemies/{label}/chicken_{i}.png", 80, manifest)
            if len(rows) > 1:
                for i, b in enumerate(rows[1][1][:4]):
                    save_sprite(crop_pad(im, b), f"Enemies/{label}/chicken_alt_{i}.png", 80, manifest)
        save_sprite(crop_pad(im, boss), f"Enemies/{label}/boss.png", 64, manifest)

    # UI
    im = Image.open(PACK / "UI/ui_kit_sheet.png").convert("RGBA")
    blobs = find_blobs(im, 80, 60, min_count=1000)
    title = [b for b in blobs if b[1] < 80 and b[2] > 600]
    if title:
        save_sprite(crop_pad(im, title[0]), "UI/title_banner.png", 100, manifest)

    btns = sorted(
        [b for b in blobs if (b[0] + b[2] / 2) < 420 and 80 < b[3] < 140 and b[2] > 300],
        key=lambda b: b[1],
    )
    btn_names = ["btn_new_game", "btn_continue", "btn_quit", "btn_resume", "btn_main_menu"]
    for i, b in enumerate(btns[:5]):
        save_sprite(crop_pad(im, b), f"UI/{btn_names[i]}.png", 100, manifest)

    lvls = sorted(
        [b for b in blobs if 420 < (b[0] + b[2] / 2) < 850 and 70 < b[3] < 110 and b[2] > 200],
        key=lambda b: b[1],
    )
    for i, b in enumerate(lvls[:3]):
        save_sprite(crop_pad(im, b), f"UI/btn_level_{i + 1}.png", 100, manifest)

    banners = sorted(
        [b for b in blobs if (b[0] + b[2] / 2) > 850 and b[2] > 400],
        key=lambda b: b[1],
    )
    ban_names = [
        "banner_round_complete",
        "banner_stage_complete",
        "banner_game_over",
        "banner_victory",
    ]
    for i, b in enumerate(banners[:4]):
        save_sprite(crop_pad(im, b), f"UI/{ban_names[i]}.png", 100, manifest)

    # HUD bars (left column mid)
    huds = sorted(
        [b for b in blobs if (b[0] + b[2] / 2) < 450 and 350 < b[1] < 900 and b[2] > 300 and b[3] < 140],
        key=lambda b: b[1],
    )
    hud_names = ["hud_score", "hud_bombs", "hud_lives", "hud_panel"]
    for i, b in enumerate(huds[:4]):
        save_sprite(crop_pad(im, b), f"UI/{hud_names[i]}.png", 100, manifest)

    # Backgrounds
    for src_name, dest_name in [
        ("level1_earth_orbit_bg.png", "Backgrounds/bg_level1.png"),
        ("level2_jupiter_asteroid_bg.png", "Backgrounds/bg_level2.png"),
        ("level3_deep_galaxy_bg.png", "Backgrounds/bg_level3.png"),
    ]:
        dest = ART / dest_name
        dest.parent.mkdir(parents=True, exist_ok=True)
        Image.open(PACK / "Backgrounds" / src_name).convert("RGBA").save(dest)
        g = guid_for(dest_name)
        write_meta(dest, g, ppu=50)
        manifest[dest_name] = g
        print("bg", dest_name, g)

    (ART / "sprite_guid_manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    print("TOTAL", len(manifest))


if __name__ == "__main__":
    main()
