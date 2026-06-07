#!/usr/bin/env python3
"""Export MC2 TGL binary shapes to simple Wavefront OBJ files.

This is a private-development bridge: it produces Unity-readable intermediate
assets from a local reference content pack, but the generated OBJ/TGA files
remain ignored and must not be committed or redistributed.
"""

from __future__ import annotations

import argparse
import json
import shutil
import struct
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


CURRENT_SHAPE_VERSION = 0xBAFDECAF
TEXTURE_RECORD_SIZE = 268
TYPE_VERTEX_SIZE = 28
TYPE_TRIANGLE_SIZE = 56
TINY_TEXTURE_SIZE = 12
NODE_ID_SIZE = 25
DEFAULT_UNITY_VISUAL_SCALE = 3.0
DEFAULT_UNITY_GROUND_OFFSET_Y = -0.5
PRIVATE_REFERENCE_PROVENANCE = {
    "status": "private-development-only",
    "redistribution": "not-public-safe",
    "replacementPolicy": "replace-with-project-owned-or-licensed-pack-before-public-release",
    "note": "Generated from a local private reference content pack for scale, pacing, and readability validation only.",
}
UNIT_ASSET_IDS = {
    "werewolf",
    "bushwacker",
    "urbanmech",
    "starslayer",
    "centipede",
    "harasser",
    "lrmc",
    "infantry",
    "poweredarmor",
}

UNITY_VISUAL_OVERRIDES: dict[str, dict[str, float]] = {
    "werewolf": {"unityScale": 3.0, "unityYawDegrees": 0.0, "groundOffsetY": -0.5},
    "bushwacker": {"unityScale": 3.0, "unityYawDegrees": 0.0, "groundOffsetY": -0.5},
    "urbanmech": {"unityScale": 3.0, "unityYawDegrees": 0.0, "groundOffsetY": -0.5},
    "starslayer": {"unityScale": 3.0, "unityYawDegrees": 0.0, "groundOffsetY": -0.5},
    "centipede": {"unityScale": 3.0, "unityYawDegrees": 0.0, "groundOffsetY": -0.5},
    "harasser": {"unityScale": 3.0, "unityYawDegrees": 0.0, "groundOffsetY": -0.5},
    "lrmc": {"unityScale": 3.0, "unityYawDegrees": 0.0, "groundOffsetY": -0.5},
}


@dataclass
class TextureRecord:
    name: str
    alpha: bool


@dataclass
class Vertex:
    position: tuple[float, float, float]
    normal: tuple[float, float, float]
    color: int


@dataclass
class Triangle:
    vertices: tuple[int, int, int]
    texture_index: int
    uvs: tuple[tuple[float, float], tuple[float, float], tuple[float, float]]
    normal: tuple[float, float, float]


@dataclass
class Node:
    node_type: int
    node_id: str
    parent_id: str
    node_center: tuple[float, float, float]
    relative_center: tuple[float, float, float]
    vertices: list[Vertex]
    triangles: list[Triangle]


@dataclass
class TglShape:
    path: Path
    textures: list[TextureRecord]
    nodes: list[Node]


class BinaryReader:
    def __init__(self, data: bytes) -> None:
        self.data = data
        self.offset = 0

    def read_u32(self) -> int:
        value = struct.unpack_from("<I", self.data, self.offset)[0]
        self.offset += 4
        return value

    def read_f32(self) -> float:
        value = struct.unpack_from("<f", self.data, self.offset)[0]
        self.offset += 4
        return value

    def read_bytes(self, count: int) -> bytes:
        value = self.data[self.offset : self.offset + count]
        self.offset += count
        return value

    def read_name(self, count: int) -> str:
        raw = self.read_bytes(count)
        return raw.split(b"\x00", 1)[0].decode("latin1", "replace")

    def skip(self, count: int) -> None:
        self.offset += count


def parse_tgl(path: Path) -> TglShape:
    reader = BinaryReader(path.read_bytes())
    version = reader.read_u32()
    if version != CURRENT_SHAPE_VERSION:
        raise ValueError(f"{path} is not a supported TGL shape: 0x{version:08X}")

    node_count = reader.read_u32()
    texture_count = reader.read_u32()
    textures: list[TextureRecord] = []
    for _ in range(texture_count):
        start = reader.offset
        texture_name = reader.read_name(256)
        reader.read_u32()
        reader.read_u32()
        alpha = reader.read_bytes(1) != b"\x00"
        reader.offset = start + TEXTURE_RECORD_SIZE
        textures.append(TextureRecord(texture_name, alpha))

    nodes: list[Node] = []
    for _ in range(node_count):
        node_type = reader.read_u32()
        if node_type == 0:
            node_center = read_vec3(reader)
            relative_center = read_vec3(reader)
            node_id = reader.read_name(NODE_ID_SIZE)
            parent_id = reader.read_name(NODE_ID_SIZE)
            nodes.append(Node(node_type, node_id, parent_id, node_center, relative_center, [], []))
            continue

        if node_type != 1:
            raise ValueError(f"{path} has unknown TGL node type {node_type} at byte {reader.offset - 4}")

        vertex_count = reader.read_u32()
        vertices = [read_vertex(reader) for _ in range(vertex_count)]

        triangle_count = reader.read_u32()
        triangles = [read_triangle(reader) for _ in range(triangle_count)]

        tiny_texture_count = reader.read_u32()
        reader.skip(tiny_texture_count * TINY_TEXTURE_SIZE)

        node_center = read_vec3(reader)
        relative_center = read_vec3(reader)
        node_id = reader.read_name(NODE_ID_SIZE)
        parent_id = reader.read_name(NODE_ID_SIZE)
        nodes.append(Node(node_type, node_id, parent_id, node_center, relative_center, vertices, triangles))

    return TglShape(path, textures, nodes)


def read_vec3(reader: BinaryReader) -> tuple[float, float, float]:
    return reader.read_f32(), reader.read_f32(), reader.read_f32()


def read_vertex(reader: BinaryReader) -> Vertex:
    position = read_vec3(reader)
    normal = read_vec3(reader)
    color = reader.read_u32()
    return Vertex(position, normal, color)


def read_triangle(reader: BinaryReader) -> Triangle:
    vertex_indices = (reader.read_u32(), reader.read_u32(), reader.read_u32())
    texture_index = reader.read_u32()
    reader.read_u32()  # renderStateFlags
    uvs = (
        (reader.read_f32(), reader.read_f32()),
        (reader.read_f32(), reader.read_f32()),
        (reader.read_f32(), reader.read_f32()),
    )
    normal = read_vec3(reader)
    return Triangle(vertex_indices, texture_index, uvs, normal)


def texture_basename(texture_name: str) -> str:
    normalized = texture_name.replace("\\", "/")
    return normalized.rsplit("/", 1)[-1]


def make_material_name(texture_index: int, texture_name: str) -> str:
    base = texture_basename(texture_name)
    stem = Path(base).stem if base else f"texture_{texture_index}"
    safe = "".join(ch if ch.isalnum() or ch in "._-" else "_" for ch in stem)
    return f"mat_{texture_index}_{safe}"


def add_vec3(a: tuple[float, float, float], b: tuple[float, float, float]) -> tuple[float, float, float]:
    return a[0] + b[0], a[1] + b[1], a[2] + b[2]


def compute_offsets(nodes: Iterable[Node]) -> dict[str, tuple[float, float, float]]:
    by_id = {node.node_id: node for node in nodes if node.node_id}
    cache: dict[str, tuple[float, float, float]] = {}

    def offset_for(node: Node) -> tuple[float, float, float]:
        if node.node_id in cache:
            return cache[node.node_id]

        parent = by_id.get(node.parent_id)
        if parent is None or node.parent_id.lower() == "none":
            value = node.node_center
        else:
            value = add_vec3(offset_for(parent), node.relative_center)

        cache[node.node_id] = value
        return value

    for node in nodes:
        if node.node_id:
            offset_for(node)

    return cache


def convert_position(
    value: tuple[float, float, float],
    *,
    scale: float,
    flip_x: bool,
) -> tuple[float, float, float]:
    x, y, z = value
    if flip_x:
        x = -x
    return x * scale, y * scale, z * scale


def write_obj(
    shape: TglShape,
    output_dir: Path,
    *,
    scale: float,
    flip_x: bool,
    include_helper_geometry: bool,
    copy_textures: bool,
    texture_root: Path | None,
) -> dict[str, object]:
    output_dir.mkdir(parents=True, exist_ok=True)
    shape_name = shape.path.stem.lower()
    unity_override = UNITY_VISUAL_OVERRIDES.get(
        shape_name,
        {
            "unityScale": DEFAULT_UNITY_VISUAL_SCALE,
            "unityYawDegrees": 0.0,
            "groundOffsetY": DEFAULT_UNITY_GROUND_OFFSET_Y,
        },
    )
    obj_path = output_dir / f"{shape_name}.obj"
    mtl_path = output_dir / f"{shape_name}.mtl"

    offsets = compute_offsets(shape.nodes)
    shape_nodes = [
        node
        for node in shape.nodes
        if node.node_type == 1
        and node.vertices
        and node.triangles
        and (include_helper_geometry or not is_helper_geometry(node))
    ]
    material_names = [
        make_material_name(index, texture.name)
        for index, texture in enumerate(shape.textures)
    ]

    copied_textures: list[str] = []
    copied_texture_paths: list[str] = []
    texture_records: list[dict[str, object]] = []
    warnings: list[str] = []
    with mtl_path.open("w", encoding="utf-8", newline="\n") as mtl:
        for index, texture in enumerate(shape.textures):
            material_name = material_names[index]
            base_name = texture_basename(texture.name)
            copied_output_path = ""
            texture_warning = ""
            mtl.write(f"newmtl {material_name}\n")
            mtl.write("Kd 1.0 1.0 1.0\n")
            mtl.write("Ka 0.15 0.15 0.15\n")
            if base_name:
                if copy_textures and texture_root is not None:
                    source = find_texture(texture_root, base_name)
                    if source is not None:
                        target = output_dir / source.name
                        if not target.exists():
                            shutil.copy2(source, target)
                        copied_textures.append(source.name)
                        copied_output_path = str(target.resolve())
                        copied_texture_paths.append(copied_output_path)
                        base_name = source.name
                    else:
                        texture_warning = f"Missing texture source for {texture.name}"
                        warnings.append(texture_warning)
                mtl.write(f"map_Kd {base_name}\n")
                if texture.alpha:
                    mtl.write(f"map_d {base_name}\n")
            mtl.write("\n")
            texture_records.append(
                {
                    "textureId": index,
                    "sourceName": texture.name,
                    "fileName": base_name,
                    "materialId": material_name,
                    "alpha": texture.alpha,
                    "copied": bool(copied_output_path),
                    "outputPath": copied_output_path,
                    "warning": texture_warning,
                }
            )

    vertex_cursor = 1
    uv_cursor = 1
    normal_cursor = 1
    total_triangles = 0
    total_vertices = 0
    with obj_path.open("w", encoding="utf-8", newline="\n") as obj:
        obj.write(f"mtllib {mtl_path.name}\n")
        obj.write(f"o {shape_name}\n")

        for node in shape_nodes:
            offset = offsets.get(node.node_id, node.node_center)
            obj.write(f"g {node.node_id or 'shape'}\n")

            for vertex in node.vertices:
                position = add_vec3(vertex.position, offset)
                x, y, z = convert_position(position, scale=scale, flip_x=flip_x)
                obj.write(f"v {x:.6f} {y:.6f} {z:.6f}\n")

            for vertex in node.vertices:
                nx, ny, nz = convert_position(vertex.normal, scale=1.0, flip_x=flip_x)
                obj.write(f"vn {nx:.6f} {ny:.6f} {nz:.6f}\n")

            face_vertices: list[str] = []
            for triangle in node.triangles:
                for uv in triangle.uvs:
                    obj.write(f"vt {uv[0]:.6f} {1.0 - uv[1]:.6f}\n")

                material_index = triangle.texture_index
                if 0 <= material_index < len(material_names):
                    obj.write(f"usemtl {material_names[material_index]}\n")

                order = (2, 1, 0) if flip_x else (0, 1, 2)
                refs = []
                for corner in order:
                    vertex_index = vertex_cursor + triangle.vertices[corner]
                    uv_index = uv_cursor + corner
                    normal_index = normal_cursor + triangle.vertices[corner]
                    refs.append(f"{vertex_index}/{uv_index}/{normal_index}")
                face_vertices.append("f " + " ".join(refs))
                uv_cursor += 3
                total_triangles += 1

            if face_vertices:
                obj.write("\n".join(face_vertices))
                obj.write("\n")

            vertex_cursor += len(node.vertices)
            normal_cursor += len(node.vertices)
            total_vertices += len(node.vertices)

    helper_node_names = sorted(
        {
            node.node_id
            for node in shape.nodes
            if node.node_id and (node.node_type == 0 or is_helper_geometry(node))
        }
    )
    node_buckets = {
        "cockpit": section_node_names(shape_nodes, "cockpit"),
        "leftArm": section_node_names(shape_nodes, "left_arm"),
        "rightArm": section_node_names(shape_nodes, "right_arm"),
        "leftLeg": section_node_names(shape_nodes, "left_leg"),
        "rightLeg": section_node_names(shape_nodes, "right_leg"),
        "torso": section_node_names(shape_nodes, "torso"),
        "shape": [node.node_id for node in shape_nodes if node.node_id],
        "helper": helper_node_names,
    }
    summary = {
        "assetId": shape_name,
        "assetClass": asset_class_for(shape_name),
        "provenance": PRIVATE_REFERENCE_PROVENANCE,
        "sourceName": shape.path.stem,
        "source": str(shape.path),
        "sourcePath": str(shape.path.resolve()),
        "outputDir": str(output_dir.resolve()),
        "obj": str(obj_path.resolve()),
        "mtl": str(mtl_path.resolve()),
        "generatedPaths": {
            "outputDir": str(output_dir.resolve()),
            "obj": str(obj_path.resolve()),
            "mtl": str(mtl_path.resolve()),
            "summary": str((output_dir / f"{shape_name}.summary.json").resolve()),
            "textures": sorted(set(copied_texture_paths)),
        },
        "ignoredOutputRoot": str(output_dir.resolve()),
        "nodes": len(shape.nodes),
        "nodeCount": len(shape.nodes),
        "shapeNodes": len(shape_nodes),
        "shapeNodeCount": len(shape_nodes),
        "nodeBuckets": node_buckets,
        "cockpitNodeNames": node_buckets["cockpit"],
        "leftArmNodeNames": node_buckets["leftArm"],
        "rightArmNodeNames": node_buckets["rightArm"],
        "leftLegNodeNames": node_buckets["leftLeg"],
        "rightLegNodeNames": node_buckets["rightLeg"],
        "torsoNodeNames": node_buckets["torso"],
        "shapeNodeNames": node_buckets["shape"],
        "helperNodeNames": node_buckets["helper"],
        "skippedHelperShapeNodes": len(
            [
                node
                for node in shape.nodes
                if node.node_type == 1 and node.vertices and node.triangles and is_helper_geometry(node)
            ]
        )
        if not include_helper_geometry
        else 0,
        "vertices": total_vertices,
        "triangles": total_triangles,
        "materialIds": material_names,
        "textureRecords": texture_records,
        "textures": [texture_basename(texture.name) for texture in shape.textures],
        "textureIds": [index for index, _ in enumerate(shape.textures)],
        "copiedTextures": sorted(set(copied_textures)),
        "copiedTexturePaths": sorted(set(copied_texture_paths)),
        "scale": scale,
        "flipX": flip_x,
        "unityScale": unity_override["unityScale"],
        "unityYawDegrees": unity_override["unityYawDegrees"],
        "groundOffsetY": unity_override["groundOffsetY"],
        "warnings": warnings,
    }
    (output_dir / f"{shape_name}.summary.json").write_text(
        json.dumps(summary, indent=2),
        encoding="utf-8",
    )
    return summary


def is_helper_geometry(node: Node) -> bool:
    node_id = node.node_id.lower()
    parent_id = node.parent_id.lower()
    helper_prefixes = (
        "spotlight",
        "slcircle",
        "los_",
        "_pab",
    )
    helper_parents = (
        "spot_",
        "spotlight",
    )
    return node_id.startswith(helper_prefixes) or parent_id.startswith(helper_parents)


def section_node_names(nodes: Iterable[Node], section: str) -> list[str]:
    names: list[str] = []
    for node in nodes:
        if node.node_id and node_matches_section(node.node_id, section):
            names.append(node.node_id)
    return names


def node_matches_section(node_id: str, section: str) -> bool:
    normalized = normalize_node_id(node_id)
    if section == "cockpit":
        return contains_any(normalized, ("cockpit", "canopy", "head", "pilot"))

    if section == "left_arm":
        if contains_any(
            normalized,
            ("rightarm", "rarm", "ruarm", "rlarm", "rgun", "rhand", "rmlauncher", "weaponrightarm"),
        ):
            return False
        return contains_any(
            normalized,
            ("leftarm", "larm", "luarm", "llarm", "lgun", "lhand", "lmlauncher", "weaponleftarm"),
        )

    if section == "right_arm":
        if contains_any(
            normalized,
            ("leftarm", "larm", "luarm", "llarm", "lgun", "lhand", "lmlauncher", "weaponleftarm"),
        ):
            return False
        return contains_any(
            normalized,
            ("rightarm", "rarm", "ruarm", "rlarm", "rgun", "rhand", "rmlauncher", "weaponrightarm"),
        )

    if section == "left_leg":
        if contains_any(normalized, ("rightleg", "rleg", "rlleg", "rmleg", "ruleg", "rfoot", "rtoe", "rankle")):
            return False
        return contains_any(normalized, ("leftleg", "lleg", "llleg", "lmleg", "luleg", "lfoot", "ltoe", "lankle"))

    if section == "right_leg":
        if contains_any(normalized, ("leftleg", "lleg", "llleg", "lmleg", "luleg", "lfoot", "ltoe", "lankle")):
            return False
        return contains_any(normalized, ("rightleg", "rleg", "rlleg", "rmleg", "ruleg", "rfoot", "rtoe", "rankle"))

    if section == "torso":
        return contains_any(normalized, ("torso", "centertorso", "hip", "hips"))

    return False


def normalize_node_id(node_id: str) -> str:
    return "".join(ch.lower() for ch in node_id if ch.isalnum())


def contains_any(value: str, needles: Iterable[str]) -> bool:
    return any(needle in value for needle in needles)


def find_texture(texture_root: Path, base_name: str) -> Path | None:
    candidate = texture_root / base_name
    if candidate.exists():
        return candidate

    lower = base_name.lower()
    for path in texture_root.rglob("*"):
        if path.is_file() and path.name.lower() == lower:
            return path
    return None


def normalize_asset_id(value: str) -> str:
    return Path(value.strip()).stem.lower()


def asset_class_for(asset_id: str) -> str:
    normalized = normalize_asset_id(asset_id)
    if normalized in UNIT_ASSET_IDS:
        return "unit"
    return "prop"


def find_tgl_source(input_root: Path, name: str) -> Path | None:
    direct = input_root / f"{name}.tgl"
    if direct.exists():
        return direct

    normalized = normalize_asset_id(name)
    for path in input_root.glob("*.tgl"):
        if path.stem.lower() == normalized:
            return path

    return None


def parse_names(raw_names: list[str]) -> list[str]:
    names: list[str] = []
    for raw in raw_names:
        for item in raw.split(","):
            name = item.strip()
            if name:
                names.append(name)
    return names


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--input-root",
        type=Path,
        default=Path("analysis-output/fst-unpack/tgl.fst/data/tgl"),
        help="Directory containing unpacked .tgl files.",
    )
    parser.add_argument(
        "--output-root",
        type=Path,
        default=Path("analysis-output/tgl-obj"),
        help="Directory for generated OBJ/MTL outputs.",
    )
    parser.add_argument(
        "--manifest-path",
        type=Path,
        default=None,
        help="Manifest output path. Defaults to <output-root>/manifest.json.",
    )
    parser.add_argument(
        "--name",
        action="append",
        default=[],
        help="Shape name without .tgl. Can be repeated or comma-separated.",
    )
    parser.add_argument("--scale", type=float, default=0.025, help="Scale applied to exported vertices.")
    parser.add_argument("--no-flip-x", action="store_true", help="Do not flip the original X axis for OBJ output.")
    parser.add_argument(
        "--include-helper-geometry",
        action="store_true",
        help="Include spotlight/LOS/PAB helper geometry in the OBJ. Disabled by default for unit visuals.",
    )
    parser.add_argument("--copy-textures", action="store_true", help="Copy referenced local TGA files beside OBJ output.")
    parser.add_argument(
        "--texture-root",
        type=Path,
        default=None,
        help="Texture search root. Defaults to <input-root>/128 when copying textures.",
    )
    args = parser.parse_args()

    names = parse_names(args.name)
    if not names:
        parser.error("At least one --name is required, for example --name werewolf")

    input_root = args.input_root
    output_root = args.output_root
    texture_root = args.texture_root or (input_root / "128")
    summaries = []
    missing_sources = []
    manifest_warnings = []

    for name in names:
        source = find_tgl_source(input_root, name)
        if source is None:
            requested_path = input_root / f"{name}.tgl"
            missing = {
                "assetId": normalize_asset_id(name),
                "requestedName": name,
                "assetClass": asset_class_for(name),
                "sourcePath": str(requested_path.resolve()),
                "warning": f"Missing TGL source: {requested_path}",
            }
            missing_sources.append(missing)
            manifest_warnings.append(missing["warning"])
            print(f"warning: {missing['warning']}", file=sys.stderr)
            continue

        shape = parse_tgl(source)
        summary = write_obj(
            shape,
            output_root / source.stem.lower(),
            scale=args.scale,
            flip_x=not args.no_flip_x,
            include_helper_geometry=args.include_helper_geometry,
            copy_textures=args.copy_textures,
            texture_root=texture_root,
        )
        summaries.append(summary)
        print(
            f"exported {source.stem}: "
            f"{summary['shapeNodes']} shape nodes, "
            f"{summary['vertices']} vertices, "
            f"{summary['triangles']} triangles -> {summary['obj']}"
        )
        for warning in summary.get("warnings", []):
            manifest_warnings.append(f"{summary['assetId']}: {warning}")
            print(f"warning: {summary['assetId']}: {warning}", file=sys.stderr)

    manifest_path = args.manifest_path or (output_root / "manifest.json")
    output_root.mkdir(parents=True, exist_ok=True)
    manifest_path.parent.mkdir(parents=True, exist_ok=True)
    manifest_path.write_text(
        json.dumps(
            {
                "schema": "mc2-reference-visual-manifest-v1",
                "manifestVersion": 2,
                "generatedBy": "scripts/content-pack/export_tgl_to_obj.py",
                "provenance": PRIVATE_REFERENCE_PROVENANCE,
                "inputRoot": str(input_root.resolve()),
                "outputRoot": str(output_root.resolve()),
                "textureRoot": str(texture_root.resolve()),
                "requestedAssets": [normalize_asset_id(name) for name in names],
                "exportCount": len(summaries),
                "missingSourceCount": len(missing_sources),
                "missingSources": missing_sources,
                "warnings": manifest_warnings,
                "exports": summaries,
            },
            indent=2,
        ),
        encoding="utf-8",
    )
    print(f"manifest: {manifest_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
