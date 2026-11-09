#!/usr/bin/env python3
"""
tests/test_combine_imagemaps.py

Unit and regression test suite for combine_imagemaps.py:
- Tests individual creature sprite sheet combinations (Warrior, Cleric, Archer, Paladin, Skeleton Warrior, Air Dragon, Red Dragon).
- Tests synthetic spritesheets with varying column widths and transparent padding.
- Tests batch processing of all 16 creatures in Archonita.
"""

import os
import sys
import json
import shutil
import tempfile
import unittest
from pathlib import Path
from PIL import Image
import numpy as np

# Add project root and tools to sys.path
PROJECT_ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(PROJECT_ROOT / "tools"))

from combine_imagemaps import (
    ACTION_SPECS,
    combine_sprite_sheets,
    verify_combined_sheet
)


class TestCombineImageMaps(unittest.TestCase):

    def setUp(self):
        self.temp_dir = tempfile.mkdtemp(prefix="test_imagemap_")
        self.temp_path = Path(self.temp_dir)

    def tearDown(self):
        shutil.rmtree(self.temp_dir, ignore_errors=True)

    def test_warrior_spritesheet_combination(self):
        """Test combination of real warrior sprite maps."""
        result = combine_sprite_sheets(
            input_dir=str(PROJECT_ROOT / "Archon/data/images/warrior"),
            output_dir=str(self.temp_path),
            output_filename="imagemap_warrior.png",
            manifest_json_filename="imagemap_warrior.json",
            manifest_txt_filename="imagemap_warrior.txt",
            creature_name="warrior"
        )
        self.assertEqual(result["dimensions"], (1248, 1248))
        self.assertEqual(result["grid"], (13, 13))
        self.assertEqual(result["cell_size"], (96, 96))
        self.assertEqual(result["total_frames"], 169)
        self.assertEqual(result["verification"]["status"], "PASSED")

    def test_cleric_spritesheet_combination(self):
        """Test combination of cleric sprite maps."""
        result = combine_sprite_sheets(
            input_dir=str(PROJECT_ROOT / "Archon/data/images/cleric"),
            output_dir=str(self.temp_path),
            output_filename="imagemap_cleric.png",
            manifest_json_filename="imagemap_cleric.json",
            manifest_txt_filename="imagemap_cleric.txt",
            creature_name="cleric"
        )
        self.assertEqual(result["dimensions"], (1664, 2176))
        self.assertEqual(result["grid"], (13, 17))
        self.assertEqual(result["cell_size"], (128, 128))
        self.assertEqual(result["total_frames"], 221)
        self.assertEqual(result["verification"]["status"], "PASSED")

    def test_archer_spritesheet_combination(self):
        """Test combination of archer sprite maps (shootingImageMap alias)."""
        result = combine_sprite_sheets(
            input_dir=str(PROJECT_ROOT / "Archon/data/images/archer"),
            output_dir=str(self.temp_path),
            output_filename="imagemap_archer.png",
            manifest_json_filename="imagemap_archer.json",
            manifest_txt_filename="imagemap_archer.txt",
            creature_name="archer"
        )
        self.assertEqual(result["dimensions"], (1664, 2176))
        self.assertEqual(result["grid"], (13, 17))
        self.assertEqual(result["cell_size"], (128, 128))
        self.assertEqual(result["total_frames"], 221)
        self.assertEqual(result["verification"]["status"], "PASSED")

    def test_paladin_spritesheet_combination(self):
        """Test combination of paladin sprite maps."""
        result = combine_sprite_sheets(
            input_dir=str(PROJECT_ROOT / "Archon/data/images/paladin"),
            output_dir=str(self.temp_path),
            output_filename="imagemap_paladin.png",
            manifest_json_filename="imagemap_paladin.json",
            manifest_txt_filename="imagemap_paladin.txt",
            creature_name="paladin"
        )
        self.assertEqual(result["dimensions"], (1664, 1920))
        self.assertEqual(result["grid"], (13, 15))
        self.assertEqual(result["cell_size"], (128, 128))
        self.assertEqual(result["total_frames"], 195)
        self.assertEqual(result["verification"]["status"], "PASSED")

    def test_skeletonwarrior_spritesheet_combination(self):
        """Test combination of skeletonwarrior sprite maps."""
        result = combine_sprite_sheets(
            input_dir=str(PROJECT_ROOT / "Archon/data/images/skeletonwarrior"),
            output_dir=str(self.temp_path),
            output_filename="imagemap_skel.png",
            manifest_json_filename="imagemap_skel.json",
            manifest_txt_filename="imagemap_skel.txt",
            creature_name="skeletonwarrior"
        )
        self.assertEqual(result["dimensions"], (1248, 1344))
        self.assertEqual(result["grid"], (13, 14))
        self.assertEqual(result["cell_size"], (96, 96))
        self.assertEqual(result["total_frames"], 182)
        self.assertEqual(result["verification"]["status"], "PASSED")

    def test_airdragon_spritesheet_combination(self):
        """Test combination of airdragon sprite maps with legacy chroma-key decoding."""
        result = combine_sprite_sheets(
            input_dir=str(PROJECT_ROOT / "Archon/data/images/airdragon"),
            output_dir=str(self.temp_path),
            output_filename="imagemap_airdragon.png",
            manifest_json_filename="imagemap_airdragon.json",
            manifest_txt_filename="imagemap_airdragon.txt",
            creature_name="airdragon"
        )
        self.assertEqual(result["dimensions"], (1280, 2176))
        self.assertEqual(result["grid"], (10, 17))
        self.assertEqual(result["cell_size"], (128, 128))
        self.assertEqual(result["total_frames"], 170)
        self.assertEqual(result["verification"]["status"], "PASSED")

    def test_reddragon_spritesheet_combination(self):
        """Test combination of reddragon sprite maps with legacy chroma-key decoding."""
        result = combine_sprite_sheets(
            input_dir=str(PROJECT_ROOT / "Archon/data/images/reddragon"),
            output_dir=str(self.temp_path),
            output_filename="imagemap_reddragon.png",
            manifest_json_filename="imagemap_reddragon.json",
            manifest_txt_filename="imagemap_reddragon.txt",
            creature_name="reddragon"
        )
        self.assertEqual(result["dimensions"], (1280, 2176))
        self.assertEqual(result["grid"], (10, 17))
        self.assertEqual(result["cell_size"], (128, 128))
        self.assertEqual(result["total_frames"], 170)
        self.assertEqual(result["verification"]["status"], "PASSED")

    def test_all_16_creatures_verification(self):
        """Verify that all 16 creature folders have valid compiled imagemap.png and manifests."""
        base_dir = PROJECT_ROOT / "Archon/data/images"
        creatures = sorted([
            d.name for d in base_dir.iterdir()
            if d.is_dir() and d.name not in ["1", "temp"]
        ])
        self.assertEqual(len(creatures), 16)
        for c in creatures:
            img_p = base_dir / c / "imagemap.png"
            json_p = base_dir / c / "imagemap_manifest.json"
            txt_p = base_dir / c / "imagemap_manifest.txt"
            self.assertTrue(img_p.is_file(), f"{c} is missing imagemap.png")
            self.assertTrue(json_p.is_file(), f"{c} is missing imagemap_manifest.json")
            self.assertTrue(txt_p.is_file(), f"{c} is missing imagemap_manifest.txt")
            self.assertGreater(img_p.stat().st_size, 0)
            self.assertGreater(json_p.stat().st_size, 0)
            self.assertGreater(txt_p.stat().st_size, 0)

    def test_synthetic_varying_row_widths_with_padding(self):
        """Test handling of maps with different row widths requiring transparent padding."""
        cell_w, cell_h = 32, 32
        test_maps = [
            ("stoppedImageMap.png", 6 * cell_w, 1 * cell_h),
            ("walkingImageMap.png", 12 * cell_w, 2 * cell_h),
            ("attackImageMap.png", 10 * cell_w, 2 * cell_h),
            ("beenImageMap.png", 8 * cell_w, 1 * cell_h),
            ("tippingImageMap.png", 12 * cell_w, 2 * cell_h)
        ]

        for fname, w, h in test_maps:
            img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
            arr = np.array(img)
            arr[2:h-2, 2:w-2] = [200, 100, 50, 255]
            Image.fromarray(arr).save(self.temp_path / fname)

        result = combine_sprite_sheets(
            input_dir=str(self.temp_path),
            output_dir=str(self.temp_path),
            force_cell_size=(cell_w, cell_h)
        )

        expected_w = 12 * cell_w
        expected_h = 8 * cell_h
        self.assertEqual(result["dimensions"], (expected_w, expected_h))
        self.assertEqual(result["grid"], (12, 8))
        self.assertEqual(result["total_frames"], 96)
        self.assertEqual(result["verification"]["status"], "PASSED")


if __name__ == "__main__":
    unittest.main()
