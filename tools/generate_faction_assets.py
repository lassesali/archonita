#!/usr/bin/env python3
"""
tools/generate_faction_assets.py

Generates Torque2D 4.0 declarative <ImageAsset> and <AnimationAsset> TAML descriptors
for all units in the new Dragonfolk and Gremlin factions:
  Dragonfolk Faction: dragonfolk, reddragon, airdragon, golddragon
  Gremlin Faction:    gremlin, hornedoneye, mistress, demon
"""

from pathlib import Path
from PIL import Image

CREATURE_SPECS = [
    # Dragonfolk Faction
    {"name": "dragonfolk", "cw": 128, "ch": 128},
    {"name": "reddragon", "cw": 128, "ch": 128},
    {"name": "airdragon", "cw": 128, "ch": 128},
    {"name": "golddragon", "cw": 128, "ch": 128},
    # Gremlin Faction
    {"name": "gremlin", "cw": 96, "ch": 96},
    {"name": "hornedoneye", "cw": 128, "ch": 128},
    {"name": "mistress", "cw": 96, "ch": 96, "tipping_cw": 128, "tipping_ch": 128},
    {"name": "demon", "cw": 96, "ch": 96}
]

ACTION_MAPS = [
    ("Stopped", ["stoppedImageMap.png"], 1),
    ("Walking", ["walkingImageMap.png"], 1),
    ("Attack", ["attackImageMap.png", "shootingImageMap.png"], 0),
    ("BeenHit", ["beenImageMap.png"], 0),
    ("TippingOver", ["tippingImageMap.png", "disappearImageMap.png", "disintegrateImageMap.png"], 0)
]


def generate_assets():
    archon_dir = Path("Archon")
    dist_dir = Path("dist/Archon")
    target_bases = [archon_dir, dist_dir]

    total_images = 0
    total_anims = 0

    for spec in CREATURE_SPECS:
        cname = spec["name"]
        default_cw = spec["cw"]
        default_ch = spec["ch"]

        for act_key, fnames, cycle in ACTION_MAPS:
            # Find file
            img_file = None
            for fn in fnames:
                candidate = archon_dir / "data/images" / cname / fn
                if candidate.is_file():
                    img_file = fn
                    break

            if not img_file:
                print(f"Warning: {cname} missing {act_key}")
                continue

            # Determine cell size for this image
            cw = default_cw
            ch = default_ch
            if act_key == "Stopped":
                if "stopped_cw" in spec:
                    cw = spec["stopped_cw"]
                if "stopped_ch" in spec:
                    ch = spec["stopped_ch"]
            elif act_key == "TippingOver" and "tipping_cw" in spec:
                cw = spec["tipping_cw"]
                ch = spec["tipping_ch"]

            img_p = archon_dir / "data/images" / cname / img_file
            with Image.open(img_p) as im:
                w, h = im.size

            cols = w // cw
            rows = h // ch if h % ch == 0 else 1

            # 1. ImageAsset
            image_asset_name = f"{cname}{act_key}ImageMap"
            image_xml = (
                f'<ImageAsset\n'
                f'    AssetName="{image_asset_name}"\n'
                f'    ImageFile="{img_file}"\n'
                f'    CellCountX="{cols}"\n'
                f'    CellCountY="{rows}"\n'
                f'    CellWidth="{cw}"\n'
                f'    CellHeight="{ch}" />\n'
            )

            for tb in target_bases:
                img_taml_path = tb / "data/images" / cname / f"{image_asset_name}.asset.taml"
                img_taml_path.parent.mkdir(parents=True, exist_ok=True)
                with open(img_taml_path, "w", encoding="utf-8") as f:
                    f.write(image_xml)
            total_images += 1

            # 2. AnimationAsset (4 directions: e, n, s, w)
            anim_frames_per_dir = {}
            if act_key == "Stopped":
                # Standard stopped frame mapping: S=0, W=2, N=4, E=6
                anim_frames_per_dir["s"] = "0"
                anim_frames_per_dir["w"] = "2"
                anim_frames_per_dir["n"] = "4"
                anim_frames_per_dir["e"] = "6"
            elif cols == 10 and rows >= 4:
                # 10-column dragon/beast grid: each row is a direction
                anim_frames_per_dir["e"] = " ".join(str(i) for i in range(0, cols))
                anim_frames_per_dir["n"] = " ".join(str(i) for i in range(cols, 2 * cols))
                anim_frames_per_dir["s"] = " ".join(str(i) for i in range(2 * cols, 3 * cols))
                anim_frames_per_dir["w"] = " ".join(str(i) for i in range(3 * cols, 4 * cols))
            else:
                # 13-column humanoid/demon grid
                f_count = {
                    "Walking": 8,
                    "Attack": 9 if rows <= 3 else 13,
                    "BeenHit": 7,
                    "TippingOver": 9 if rows <= 3 else 13
                }.get(act_key, 8)

                for d_idx, d_code in enumerate(["e", "n", "s", "w"]):
                    start_f = d_idx * f_count
                    end_f = min(start_f + f_count, cols * rows)
                    anim_frames_per_dir[d_code] = " ".join(str(i) for i in range(start_f, end_f))

            for dir_code, frames_str in anim_frames_per_dir.items():
                anim_asset_name = f"{cname}{act_key}Animation_{dir_code}"
                anim_xml = (
                    f'<AnimationAsset\n'
                    f'    AssetName="{anim_asset_name}"\n'
                    f'    Image="@asset=Archon:{image_asset_name}"\n'
                    f'    AnimationFrames="{frames_str}"\n'
                    f'    AnimationTime="1"\n'
                    f'    AnimationCycle="{cycle}" />\n'
                )

                for tb in target_bases:
                    anim_taml_path = tb / "data/animations" / f"{anim_asset_name}.asset.taml"
                    anim_taml_path.parent.mkdir(parents=True, exist_ok=True)
                    with open(anim_taml_path, "w", encoding="utf-8") as f:
                        f.write(anim_xml)
                total_anims += 1

    print(f"Asset Generation Complete:")
    print(f"  Created {total_images} ImageAsset TAML files.")
    print(f"  Created {total_anims} AnimationAsset TAML files.")


if __name__ == "__main__":
    generate_assets()
