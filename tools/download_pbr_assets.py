import urllib.request
import json
import os
import sys

HEADERS = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)'}

def download_file(url, target_path):
    if os.path.exists(target_path) and os.path.getsize(target_path) > 1000:
        print(f"Already exists: {target_path}")
        return True
    print(f"Downloading: {url} -> {target_path}")
    os.makedirs(os.path.dirname(target_path), exist_ok=True)
    try:
        req = urllib.request.Request(url, headers=HEADERS)
        with urllib.request.urlopen(req, timeout=30) as resp, open(target_path, 'wb') as out:
            out.write(resp.read())
        print(f"Saved: {target_path} ({os.path.getsize(target_path)} bytes)")
        return True
    except Exception as e:
        print(f"Error downloading {url}: {e}", file=sys.stderr)
        return False

def get_asset_files(asset_id):
    url = f"https://api.polyhaven.com/files/{asset_id}"
    req = urllib.request.Request(url, headers=HEADERS)
    with urllib.request.urlopen(req, timeout=15) as resp:
        return json.loads(resp.read().decode('utf-8'))

# 1. PBR Textures to download
TEXTURES = {
    'castle_wall_slates': 'LighthouseStone',
    'brown_mud_rocks_01': 'CoastalRock',
    'wood_planks_grey': 'WeatheredPlanks',
    'corrugated_iron': 'RustedIron',
    'blue_plaster_weathered': 'WeatheredPlaster',
}

base_tex_dir = "Assets/_Project/Art/Textures/PBR"

for asset_id, folder_name in TEXTURES.items():
    print(f"\n--- Fetching texture {asset_id} ({folder_name}) ---")
    dest_dir = os.path.join(base_tex_dir, folder_name)
    try:
        info = get_asset_files(asset_id)
        # We need Diffuse, nor_gl, Rough, AO
        maps = {
            'diff': info.get('Diffuse', {}).get('1k', {}).get('jpg', {}).get('url'),
            'nor': info.get('nor_gl', {}).get('1k', {}).get('jpg', {}).get('url'),
            'rough': info.get('Rough', {}).get('1k', {}).get('jpg', {}).get('url'),
            'ao': info.get('AO', {}).get('1k', {}).get('jpg', {}).get('url'),
        }
        for map_type, url in maps.items():
            if url:
                ext = url.split('.')[-1]
                filename = f"{folder_name}_{map_type}.{ext}"
                download_file(url, os.path.join(dest_dir, filename))
            else:
                print(f"Warning: No 1k JPG for {map_type} in {asset_id}")
    except Exception as e:
        print(f"Failed texture {asset_id}: {e}")

# 2. 3D FBX Models to download
MODELS = {
    'barrel_03': 'MaritimeBarrel',
    'coast_rocks_01': 'CoastalRock_01',
    'hanging_industrial_lamp': 'IndustrialHangingLamp',
}

base_model_dir = "Assets/_Project/Art/Models"

for asset_id, folder_name in MODELS.items():
    print(f"\n--- Fetching model {asset_id} ({folder_name}) ---")
    dest_dir = os.path.join(base_model_dir, folder_name)
    try:
        info = get_asset_files(asset_id)
        fbx_info = info.get('fbx', {}).get('1k', {}).get('fbx', {})
        fbx_url = fbx_info.get('url')
        if fbx_url:
            fbx_path = os.path.join(dest_dir, f"{folder_name}.fbx")
            download_file(fbx_url, fbx_path)
            # Download model textures
            includes = fbx_info.get('include', {})
            for rel_name, tex_data in includes.items():
                tex_url = tex_data.get('url')
                tex_filename = os.path.basename(rel_name)
                # Avoid downloading heavy EXR if jpg exists
                if tex_filename.endswith('.exr'):
                    continue
                download_file(tex_url, os.path.join(dest_dir, tex_filename))
    except Exception as e:
        print(f"Failed model {asset_id}: {e}")

# 3. Audio Assets from Wikimedia Commons
AUDIO_DIR = "Assets/_Project/Audio"
AUDIO_FILES = {
    'Ambience_Ocean_Waves.ogg': 'https://upload.wikimedia.org/wikipedia/commons/f/f1/Oceanwavescrushing.ogg',
    'Ambience_Rain_Storm.ogg': 'https://upload.wikimedia.org/wikipedia/commons/e/ee/Rain_and_thunder.ogg',
    'Sfx_Lighthouse_Foghorn.ogg': 'https://upload.wikimedia.org/wikipedia/commons/2/29/WWS_Foghorn.ogg',
    'Sfx_Radio_Static.ogg': 'https://upload.wikimedia.org/wikipedia/commons/7/7b/Whitenoisesound.ogg',
}

print("\n--- Fetching Audio Assets ---")
for filename, url in AUDIO_FILES.items():
    dest_path = os.path.join(AUDIO_DIR, filename)
    download_file(url, dest_path)

print("\nAsset download process completed successfully.")
