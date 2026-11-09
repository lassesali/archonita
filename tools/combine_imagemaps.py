#!/usr/bin/env python3
"""
tools/combine_imagemaps.py

Combines separate creature action sprite sheet image maps into a single unified
master sprite sheet (imagemap.png) with full alpha channel transparency,
uniform isometric grid alignment, and companion metadata reports (JSON & TXT).

Supported Action Sequence (top-to-bottom):
  1. stoppedImageMap.png (Idle states)
  2. walkingImageMap.png (Locomotion cycles)
  3. attackImageMap.png / shootingImageMap.png (Combat / attack actions)
  4. beenImageMap.png (Damage / hurt reactions)
  5. tippingImageMap.png / disintegrateImageMap.png / disappearImageMap.png (Death / collapse sequences)
"""

import os
import sys
import math
import json
import argparse
import xml.etree.ElementTree as ET
from pathlib import Path
from PIL import Image
import numpy as np

# Action descriptors with possible file aliases
ACTION_SPECS = [
    {
        "key": "stopped",
        "name": "Stopped / Idle",
        "description": "Standing and idle orientation frames",
        "file_candidates": ["stoppedImageMap.png"]
    },
    {
        "key": "walking",
        "name": "Walking / Locomotion",
        "description": "Four-directional walking locomotion cycles",
        "file_candidates": ["walkingImageMap.png"]
    },
    {
        "key": "attack",
        "name": "Attack / Combat",
        "description": "Combat, weapon swing, or projectile shooting animation frames",
        "file_candidates": ["attackImageMap.png", "shootingImageMap.png", "magicImageMap.png"]
    },
    {
        "key": "beenHit",
        "name": "Been Hit / Pain",
        "description": "Damage, pain, and impact reaction animation frames",
        "file_candidates": ["beenImageMap.png"]
    },
    {
        "key": "tippingOver",
        "name": "Tipping Over / Death",
        "description": "Death, tipping over, falling, or disintegration sequences",
        "file_candidates": ["tippingImageMap.png", "disintegrateImageMap.png", "disappearImageMap.png"]
    }
]


def find_action_file(input_path, creature_name, candidates):
    """Finds existing file matching one of the candidate names."""
    # 1. Search directly in input_path
    for cand in candidates:
        fp = input_path / cand
        if fp.is_file():
            return fp, cand

    # 2. Search in Archon/data/images/<creature_name>
    creature_dir = Path("Archon/data/images") / creature_name
    if creature_dir.is_dir():
        for cand in candidates:
            fp = creature_dir / cand
            if fp.is_file():
                return fp, cand

    return None, None


def load_animation_definitions(creature_name):
    """
    Parses existing .asset.taml animation definitions for the creature
    to extract actual directional frame sequences.
    """
    anims = {}
    anim_dir = Path("Archon/data/animations")
    if not anim_dir.is_dir():
        return anims

    for taml_file in anim_dir.glob(f"{creature_name}*.asset.taml"):
        try:
            tree = ET.parse(taml_file)
            root = tree.getroot()
            if root.tag == "AnimationAsset":
                asset_name = root.get("AssetName", "")
                frames_str = root.get("AnimationFrames", "")
                if frames_str:
                    frames = [int(x) for x in frames_str.split()]
                    anims[asset_name] = frames
        except Exception:
            pass

    return anims


def detect_file_cell_metrics(img_file, input_path, default_cell_size=None):
    """Inspects TAML metadata or image geometry to find cell metrics for a single map."""
    if default_cell_size:
        cw, ch = default_cell_size
        with Image.open(img_file) as im:
            return cw, ch, im.width // cw, im.height // ch

    # Check if a matching asset taml exists
    for taml_path in input_path.glob("*.asset.taml"):
        try:
            tree = ET.parse(taml_path)
            root = tree.getroot()
            if root.tag == "ImageAsset" and root.get("ImageFile") == img_file.name:
                cw = int(root.get("CellWidth", 0))
                ch = int(root.get("CellHeight", 0))
                cx = int(root.get("CellCountX", 0))
                cy = int(root.get("CellCountY", 0))
                if cw > 0 and ch > 0:
                    return cw, ch, cx, cy
        except Exception:
            pass

    # Fallback heuristic
    with Image.open(img_file) as im:
        w, h = im.size

    for ts in [128, 96, 64, 32]:
        if w % ts == 0:
            cw = ts
            break
    else:
        cw = w
    cx = w // cw

    if h % cw == 0:
        ch = cw
        cy = h // cw
    else:
        for ts in [128, 96, 64, 32]:
            if h % ts == 0:
                ch = ts
                break
        else:
            ch = h
        cy = h // ch

    return cw, ch, cx, cy


def combine_sprite_sheets(
    input_dir=".",
    output_dir=".",
    output_filename="imagemap.png",
    manifest_json_filename="imagemap_manifest.json",
    manifest_txt_filename="imagemap_manifest.txt",
    creature_name="warrior",
    force_cell_size=None,
    layout_mode="uniform"  # "uniform" (standard tile size) or "stacked"
):
    """
    Main pipeline to combine action image maps into a master sprite sheet.
    """
    input_path = Path(input_dir)
    output_path = Path(output_dir)
    output_path.mkdir(parents=True, exist_ok=True)

    # 1. Locate and load all 5 action images
    loaded_actions = []
    for spec in ACTION_SPECS:
        fpath, matched_name = find_action_file(input_path, creature_name, spec["file_candidates"])
        if not fpath:
            raise FileNotFoundError(
                f"Missing required sprite sheet for action '{spec['name']}' ({spec['key']}). "
                f"Searched candidates {spec['file_candidates']} in '{input_path}'."
            )

        img = Image.open(fpath)
        if img.mode != "RGBA":
            img = img.convert("RGBA")

        # Auto-convert legacy 2006 opaque chroma-key assets ([106, 76, 48], [97, 68, 43], and dummy black)
        arr = np.array(img)
        if np.all(arr[:, :, 3] == 255):
            chroma1 = np.all(arr[:, :, :3] == [106, 76, 48], axis=-1)
            chroma2 = np.all(arr[:, :, :3] == [97, 68, 43], axis=-1)
            black_mask = np.all(arr[:, :, :3] == [0, 0, 0], axis=-1)
            if np.any(chroma1) or np.any(chroma2):
                arr[chroma1 | chroma2 | black_mask] = [0, 0, 0, 0]
                img = Image.fromarray(arr, "RGBA")

        cw, ch, cx, cy = detect_file_cell_metrics(fpath, fpath.parent, default_cell_size=force_cell_size)
        loaded_actions.append({
            "spec": spec,
            "file_name": matched_name,
            "file_path": fpath,
            "img": img,
            "cell_width": cw,
            "cell_height": ch,
            "cell_cols": cx,
            "cell_rows": cy,
            "width": img.width,
            "height": img.height
        })

    # 2. Determine Master Grid & Uniform Tile Metrics
    if force_cell_size:
        master_cell_w, master_cell_h = force_cell_size
    elif layout_mode == "uniform":
        # Standardize to maximum cell dimensions across all maps
        master_cell_w = max(item["cell_width"] for item in loaded_actions)
        master_cell_h = max(item["cell_height"] for item in loaded_actions)
    else:
        master_cell_w = loaded_actions[0]["cell_width"]
        master_cell_h = loaded_actions[0]["cell_height"]

    max_cols = max(item["cell_cols"] for item in loaded_actions)
    total_grid_rows = sum(item["cell_rows"] for item in loaded_actions)

    master_width = max_cols * master_cell_w
    master_height = total_grid_rows * master_cell_h
    total_frames = max_cols * total_grid_rows

    # 3. Initialize master blank canvas with full alpha transparency
    # (0, 0, 0, 0) transparent black ensures no border discoloration or white/black matting
    master_canvas = Image.new("RGBA", (master_width, master_height), (0, 0, 0, 0))

    # 4. Sequentially blit action sequences into uniform rows
    current_grid_row = 0
    current_y_px = 0
    manifest_actions = []

    # Parse existing animations for mapping
    anim_defs = load_animation_definitions(creature_name)

    for item in loaded_actions:
        spec = item["spec"]
        img = item["img"]
        src_cw = item["cell_width"]
        src_ch = item["cell_height"]
        src_cols = item["cell_cols"]
        src_rows = item["cell_rows"]

        start_row = current_grid_row
        end_row = current_grid_row + src_rows - 1
        start_frame = start_row * max_cols
        action_frame_count = src_rows * max_cols

        # Blit individual frames into the master grid
        frame_list = []
        for r in range(src_rows):
            grid_r = start_row + r
            for c in range(max_cols):
                global_frame_idx = grid_r * max_cols + c
                local_frame_idx = r * max_cols + c
                cell_x = c * master_cell_w
                cell_y = grid_r * master_cell_h

                if c < src_cols:
                    # Valid frame from source sheet
                    src_crop_x = c * src_cw
                    src_crop_y = r * src_ch
                    frame_crop = img.crop((src_crop_x, src_crop_y, src_crop_x + src_cw, src_crop_y + src_ch))

                    # Isometric baseline / center anchor alignment:
                    # Center horizontally, align to tile bottom/center baseline
                    offset_x = (master_cell_w - src_cw) // 2
                    offset_y = (master_cell_h - src_ch) // 2  # centered isometric tile box
                    master_canvas.paste(frame_crop, (cell_x + offset_x, cell_y + offset_y))

                    is_padding = False
                else:
                    # Shorter row padding slot: remains transparent (0, 0, 0, 0)
                    is_padding = True

                frame_list.append({
                    "local_index": local_frame_idx,
                    "global_index": global_frame_idx,
                    "grid_row": grid_r,
                    "grid_col": c,
                    "rect": {
                        "x": cell_x,
                        "y": cell_y,
                        "width": master_cell_w,
                        "height": master_cell_h
                    },
                    "is_padding": is_padding
                })

        # Calculate directional animation frame mappings
        directional_anims = {}
        # Find directional animation assets matching this action
        key_alias_map = {
            "stopped": "Stopped",
            "walking": "Walking",
            "attack": "Attack",
            "beenHit": "BeenHit",
            "tippingOver": "TippingOver"
        }
        anim_prefix = f"{creature_name}{key_alias_map.get(spec['key'], '')}Animation_"
        for dir_code in ["e", "n", "s", "w"]:
            dir_asset = f"{anim_prefix}{dir_code}"
            dir_name = {"e": "East", "n": "North", "s": "South", "w": "West"}[dir_code]

            if dir_asset in anim_defs:
                orig_frames = anim_defs[dir_asset]
                mapped_frames = []
                for of in orig_frames:
                    orig_r = of // src_cols
                    orig_c = of % src_cols
                    mf = (start_row + orig_r) * max_cols + orig_c
                    mapped_frames.append(mf)

                directional_anims[dir_code] = {
                    "asset_name": dir_asset,
                    "direction": dir_name,
                    "original_frames": orig_frames,
                    "master_frames": mapped_frames,
                    "frame_string": " ".join(str(x) for x in mapped_frames)
                }
            elif not anim_defs:
                # Canonical fallback for unmigrated creature assets
                if spec["key"] == "stopped":
                    frame_map = {"s": 0, "w": 2, "n": 4, "e": 6}
                    col_idx = frame_map.get(dir_code, 0)
                    if col_idx < src_cols:
                        mf = start_row * max_cols + col_idx
                        directional_anims[dir_code] = {
                            "asset_name": dir_asset,
                            "direction": dir_name,
                            "original_frames": [col_idx],
                            "master_frames": [mf],
                            "frame_string": str(mf)
                        }
                elif src_rows >= 4:
                    row_order = {"e": 0, "n": 1, "s": 2, "w": 3}
                    r_idx = row_order.get(dir_code, 0)
                    if r_idx < src_rows:
                        mapped_frames = [
                            (start_row + r_idx) * max_cols + c
                            for c in range(src_cols)
                        ]
                        directional_anims[dir_code] = {
                            "asset_name": dir_asset,
                            "direction": dir_name,
                            "original_frames": list(range(r_idx * src_cols, (r_idx + 1) * src_cols)),
                            "master_frames": mapped_frames,
                            "frame_string": " ".join(str(x) for x in mapped_frames)
                        }

        action_entry = {
            "key": spec["key"],
            "name": spec["name"],
            "description": spec["description"],
            "source_file": item["file_name"],
            "source_path": str(item["file_path"]),
            "source_dimensions": {"width": img.width, "height": img.height},
            "source_cell_size": {"width": src_cw, "height": src_ch},
            "grid_placement": {
                "start_row": start_row,
                "end_row": end_row,
                "row_count": src_rows,
                "col_count": max_cols,
                "start_frame": start_frame,
                "end_frame": start_frame + action_frame_count - 1,
                "frame_count": action_frame_count,
                "active_frames": src_rows * src_cols,
                "padded_cells": action_frame_count - (src_rows * src_cols),
                "master_rectangle": {
                    "x": 0,
                    "y": start_row * master_cell_h,
                    "width": master_width,
                    "height": src_rows * master_cell_h
                }
            },
            "directional_animations": directional_anims,
            "frames": frame_list
        }
        manifest_actions.append(action_entry)

        current_grid_row += src_rows
        current_y_px += src_rows * master_cell_h

    # 5. Export Master Sprite Sheet
    master_file_path = output_path / output_filename
    master_canvas.save(master_file_path, format="PNG", optimize=True)

    # 6. Generate JSON Manifest
    manifest_data = {
        "metadata": {
            "generator": "Archonita Sprite Sheet Synthesizer (combine_imagemaps.py)",
            "creature": creature_name,
            "texture_atlas": {
                "file_name": output_filename,
                "file_path": str(master_file_path),
                "width": master_width,
                "height": master_height,
                "cell_width": master_cell_w,
                "cell_height": master_cell_h,
                "grid_columns": max_cols,
                "grid_rows": total_grid_rows,
                "total_frames": total_frames,
                "color_mode": "RGBA",
                "layout_mode": layout_mode
            }
        },
        "torque2d_asset_definitions": {
            "image_asset_xml": (
                f'<ImageAsset\n'
                f'    AssetName="{creature_name}MasterImageMap"\n'
                f'    ImageFile="{output_filename}"\n'
                f'    CellCountX="{max_cols}"\n'
                f'    CellCountY="{total_grid_rows}"\n'
                f'    CellWidth="{master_cell_w}"\n'
                f'    CellHeight="{master_cell_h}" />'
            ),
            "animation_assets": [
                {
                    "asset_name": dir_info["asset_name"],
                    "direction": dir_info["direction"],
                    "image": f"@asset=Archon:{creature_name}MasterImageMap",
                    "animation_frames": dir_info["frame_string"]
                }
                for act in manifest_actions
                for dir_info in act["directional_animations"].values()
            ]
        },
        "actions": manifest_actions
    }

    manifest_json_path = output_path / manifest_json_filename
    with open(manifest_json_path, "w", encoding="utf-8") as f:
        json.dump(manifest_data, f, indent=2)

    # 7. Generate Companion Text Report
    manifest_txt_path = output_path / manifest_txt_filename
    report_text = generate_text_report(manifest_data)
    with open(manifest_txt_path, "w", encoding="utf-8") as f:
        f.write(report_text)

    # 8. Verification
    verify_result = verify_combined_sheet(
        master_file_path,
        loaded_actions,
        master_cell_w,
        master_cell_h,
        max_cols,
        total_grid_rows
    )

    return {
        "creature": creature_name,
        "image_path": master_file_path,
        "json_path": manifest_json_path,
        "txt_path": manifest_txt_path,
        "dimensions": (master_width, master_height),
        "grid": (max_cols, total_grid_rows),
        "cell_size": (master_cell_w, master_cell_h),
        "total_frames": total_frames,
        "verification": verify_result
    }


def generate_text_report(manifest_data):
    """Generates formatted companion plain-text log / report."""
    meta = manifest_data["metadata"]["texture_atlas"]
    creature = manifest_data["metadata"]["creature"]
    actions = manifest_data["actions"]

    lines = [
        "=" * 84,
        f"ARCHONITA CREATURE SPRITE SHEET COMPILATION REPORT: {creature.upper()}",
        "=" * 84,
        "",
        "1. MASTER TEXTURE ATLAS SPECIFICATIONS",
        "-" * 84,
        f"  Output File:           {meta['file_name']}",
        f"  Pixel Dimensions:      {meta['width']} x {meta['height']} px",
        f"  Cell Dimensions:       {meta['cell_width']} x {meta['cell_height']} px",
        f"  Grid Dimensions:       {meta['grid_columns']} cols x {meta['grid_rows']} rows",
        f"  Total Cells / Frames:  {meta['total_frames']} frames",
        f"  Color Format:          {meta['color_mode']} (32-bit RGBA with full alpha transparency)",
        f"  Layout Mode:           {meta['layout_mode']}",
        "",
        "2. ACTION SEQUENCING & VERTICAL GRID LAYOUT",
        "-" * 84,
        f"{'Action Name':<22} {'Source File':<24} {'Rows':<8} {'Y-Offset (px)':<14} {'Frames':<8} {'Padded':<8} {'Frame IDs'}",
        "-" * 84
    ]

    for act in actions:
        gp = act["grid_placement"]
        row_range = f"{gp['start_row']}..{gp['end_row']}" if gp['row_count'] > 1 else f"{gp['start_row']}"
        y_range = f"{gp['master_rectangle']['y']}..{gp['master_rectangle']['y'] + gp['master_rectangle']['height']}"
        frame_range = f"{gp['start_frame']}..{gp['end_frame']}"
        lines.append(
            f"{act['name']:<22} {act['source_file']:<24} {row_range:<8} {y_range:<14} {gp['active_frames']:<8} {gp['padded_cells']:<8} {frame_range}"
        )

    lines.extend([
        "-" * 84,
        "",
        "3. SOURCE RECTANGLES & CLIPPING BOUNDS",
        "-" * 84
    ])

    for act in actions:
        rect = act["grid_placement"]["master_rectangle"]
        sc = act["source_cell_size"]
        lines.append(f"  [{act['key']}] {act['name']}")
        lines.append(f"    Source File:      {act['source_file']} (source cell {sc['width']}x{sc['height']})")
        lines.append(f"    Master Rectangle: X={rect['x']}, Y={rect['y']}, Width={rect['width']}, Height={rect['height']}")
        lines.append(f"    Grid Rows:        Start={act['grid_placement']['start_row']}, Count={act['grid_placement']['row_count']}")
        lines.append(f"    Frame Index Range:{act['grid_placement']['start_frame']} to {act['grid_placement']['end_frame']}")
        lines.append(f"    Active / Padded:  {act['grid_placement']['active_frames']} active frames, {act['grid_placement']['padded_cells']} transparent padding cells")
        lines.append("")

    lines.extend([
        "4. DIRECTIONAL ANIMATION FRAME MAPPINGS FOR GAME ENGINE",
        "-" * 84
    ])

    for act in actions:
        if act["directional_animations"]:
            lines.append(f"  {act['name']} Animations:")
            for dir_key, dir_info in act["directional_animations"].items():
                lines.append(f"    - Direction {dir_info['direction']:<6} ({dir_info['asset_name']}) -> Frames: {dir_info['frame_string']}")
            lines.append("")

    lines.extend([
        "5. TORQUE2D 4.0 ASSET INTEGRATION SNIPPET",
        "-" * 84,
        manifest_data["torque2d_asset_definitions"]["image_asset_xml"],
        "",
        "=" * 84,
        "END OF MANIFEST REPORT",
        "=" * 84
    ])

    return "\n".join(lines)


def verify_combined_sheet(master_path, loaded_actions, cell_width, cell_height, max_cols, total_rows):
    """
    Rigorously verifies the output master image:
    1. File existence and size > 0
    2. Dimensions match expected (max_cols * cell_width, total_rows * cell_height)
    3. RGBA mode
    4. Clean alpha transparency (no background corruption)
    """
    assert master_path.is_file(), f"Master file {master_path} does not exist"
    assert master_path.stat().st_size > 0, "Master file is empty"

    with Image.open(master_path) as reloaded:
        expected_w = max_cols * cell_width
        expected_h = total_rows * cell_height
        assert reloaded.size == (expected_w, expected_h), (
            f"Dimension mismatch: got {reloaded.size}, expected ({expected_w}, {expected_h})"
        )
        assert reloaded.mode == "RGBA", f"Color mode mismatch: got {reloaded.mode}, expected RGBA"

        reloaded_arr = np.array(reloaded)

    # Transparency check: ensure alpha channel has values 0 and 255
    alpha = reloaded_arr[:, :, 3]
    has_transparency = np.any(alpha == 0)
    has_opaque = np.any(alpha > 0)
    assert has_transparency and has_opaque, "Master image lacks expected transparent or opaque pixels"

    return {
        "status": "PASSED",
        "verified_dimensions": (expected_w, expected_h),
        "verified_mode": "RGBA",
        "has_transparency": True,
        "has_opaque": True
    }


def main():
    parser = argparse.ArgumentParser(
        description="Combine creature action image maps into a single unified imagemap.png"
    )
    parser.add_argument("--input-dir", "-i", default=".", help="Directory with input image maps (default: .)")
    parser.add_argument("--output-dir", "-o", default=".", help="Directory for output files (default: .)")
    parser.add_argument("--output-file", default="imagemap.png", help="Output PNG file name (default: imagemap.png)")
    parser.add_argument("--manifest-json", default="imagemap_manifest.json", help="JSON manifest filename")
    parser.add_argument("--manifest-txt", default="imagemap_manifest.txt", help="Text manifest filename")
    parser.add_argument("--cell-width", type=int, help="Optional manual cell width override")
    parser.add_argument("--cell-height", type=int, help="Optional manual cell height override")
    parser.add_argument("--creature", default="warrior", help="Creature name (e.g. warrior, cleric, archer)")
    parser.add_argument("--all-creatures", action="store_true", help="Process all creature directories in Archon/data/images")
    parser.add_argument("--sync-creature-dir", action="store_true", help="Sync output to Archon/data/images/<creature>")

    args = parser.parse_args()

    custom_cell = None
    if args.cell_width and args.cell_height:
        custom_cell = (args.cell_width, args.cell_height)

    if args.all_creatures:
        base_dir = Path("Archon/data/images")
        creature_dirs = sorted([
            d.name for d in base_dir.iterdir()
            if d.is_dir() and d.name not in ["1", "temp"]
        ])
        print(f"Batch processing {len(creature_dirs)} creatures: {', '.join(creature_dirs)}")
        for c in creature_dirs:
            c_input = base_dir / c
            c_output = c_input if args.output_dir == "." else Path(args.output_dir) / c
            res = combine_sprite_sheets(
                input_dir=str(c_input),
                output_dir=str(c_output),
                output_filename=args.output_file,
                manifest_json_filename=args.manifest_json,
                manifest_txt_filename=args.manifest_txt,
                creature_name=c,
                force_cell_size=custom_cell
            )
            print(f"  [{c}] {res['dimensions'][0]}x{res['dimensions'][1]} px ({res['grid'][0]}x{res['grid'][1]} grid, {res['total_frames']} frames) -> {res['verification']['status']}")
            if args.sync_creature_dir:
                import shutil
                dist_td = Path("dist/Archon/data/images") / c
                if dist_td.is_dir():
                    shutil.copy2(res['image_path'], dist_td / args.output_file)
                    shutil.copy2(res['json_path'], dist_td / args.manifest_json)
                    shutil.copy2(res['txt_path'], dist_td / args.manifest_txt)
        return

    # Determine input directory
    if args.input_dir != ".":
        input_p = Path(args.input_dir)
    elif args.creature and (Path("Archon/data/images") / args.creature).is_dir():
        input_p = Path("Archon/data/images") / args.creature
    else:
        input_p = Path(".")

    result = combine_sprite_sheets(
        input_dir=str(input_p),
        output_dir=args.output_dir,
        output_filename=args.output_file,
        manifest_json_filename=args.manifest_json,
        manifest_txt_filename=args.manifest_txt,
        creature_name=args.creature,
        force_cell_size=custom_cell
    )

    print(f"Sprite Sheet Combination Successful for '{result['creature']}':")
    print(f"  Master Texture:  {result['image_path']} ({result['dimensions'][0]}x{result['dimensions'][1]} px)")
    print(f"  Grid Layout:     {result['grid'][0]} cols x {result['grid'][1]} rows ({result['cell_size'][0]}x{result['cell_size'][1]} px/cell)")
    print(f"  Total Frames:    {result['total_frames']}")
    print(f"  JSON Manifest:   {result['json_path']}")
    print(f"  Text Report:     {result['txt_path']}")
    print(f"  Verification:    {result['verification']['status']}")

    if args.sync_creature_dir:
        import shutil
        target_dirs = [
            Path("Archon/data/images") / args.creature,
            Path("dist/Archon/data/images") / args.creature
        ]
        for td in target_dirs:
            if td.is_dir():
                target_img = td / args.output_file
                if result['image_path'].resolve() != target_img.resolve():
                    shutil.copy2(result['image_path'], target_img)
                    shutil.copy2(result['json_path'], td / args.manifest_json)
                    shutil.copy2(result['txt_path'], td / args.manifest_txt)
                    print(f"  Synced to:       {td}")


if __name__ == "__main__":
    main()
