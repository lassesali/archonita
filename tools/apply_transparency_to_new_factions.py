# tools/apply_transparency_to_new_factions.py
"""
Converts the legacy 2006 opaque brown background ([106, 76, 48], [97, 68, 43], and black padding)
to 100% alpha transparency for all individual action image maps in the Dragonfolk and Gremlin factions.
"""

from pathlib import Path
from PIL import Image
import numpy as np

NEW_CREATURES = [
    'dragonfolk', 'reddragon', 'airdragon', 'golddragon',
    'gremlin', 'hornedoneye', 'mistress', 'demon'
]

CHROMA_COLORS = [
    [106, 76, 48],  # Red Dragon, Air Dragon, Gold Dragon, Gremlin, Demon
    [97, 68, 43],   # Dragon Folk, Horned One-Eye, Mistress
    [0, 0, 0]       # Black padding slots
]


def process_image(img_path: Path):
    im = Image.open(img_path).convert("RGBA")
    arr = np.array(im)

    # Build mask of all background chroma colors
    bg_mask = np.zeros(arr.shape[:2], dtype=bool)
    for color in CHROMA_COLORS:
        match = np.all(arr[:, :, :3] == color, axis=-1)
        bg_mask |= match

    # Set alpha channel to 0 for background pixels
    # Setting [0, 0, 0, 0] avoids any color bleeding / fringing on edges
    arr[bg_mask] = [0, 0, 0, 0]

    out_im = Image.fromarray(arr, "RGBA")
    out_im.save(img_path, "PNG", optimize=True)
    return np.sum(bg_mask), arr.shape[0] * arr.shape[1]


def main():
    root = Path("Archon/data/images")
    dist_root = Path("dist/Archon/data/images")

    total_files = 0
    for creature in NEW_CREATURES:
        cdir = root / creature
        dist_cdir = dist_root / creature
        if not cdir.is_dir():
            print(f"Skipping missing directory: {cdir}")
            continue

        for png_file in cdir.glob("*.png"):
            if png_file.name == "imagemap.png":
                continue

            bg_px, total_px = process_image(png_file)
            total_files += 1
            print(f"[{creature}] {png_file.name}: {bg_px}/{total_px} pixels ({bg_px/total_px*100:.1f}%) converted to transparent alpha")

            # Also sync directly to dist directory if present
            dist_png = dist_cdir / png_file.name
            if dist_png.parent.is_dir():
                dist_im = Image.open(png_file)
                dist_im.save(dist_png, "PNG", optimize=True)

    print(f"\nSuccessfully converted {total_files} sprite sheet files across 8 new creatures to full alpha transparency.")


if __name__ == "__main__":
    main()
