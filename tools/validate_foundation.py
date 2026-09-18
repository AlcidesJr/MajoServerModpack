#!/usr/bin/env python3
"""Valida invariantes documentais da fundação MAJO-000 usando apenas stdlib."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]

REQUIRED = [
    "AGENTS.md",
    "README.md",
    "THIRD_PARTY_NOTICES.md",
    "docs/ARCHITECTURE.md",
    "docs/COMPATIBILITY.md",
    "docs/CONFLICTS.md",
    "docs/DECISIONS.md",
    "docs/DEPENDENCIES.md",
    "docs/FEATURE-CATALOG.md",
    "docs/INPUT.md",
    "docs/MODULE-ROADMAP.md",
    "docs/PATCH-OWNERSHIP.md",
    "docs/REFERENCE-PROJECTS.md",
    "docs/SECURITY-BASELINE.md",
    "docs/VERSIONING.md",
    "docs/WORKFLOW.md",
    "work/BOARD.md",
    "work/MAJO-000/TASK.md",
    "work/MAJO-000/PLAN.md",
    "work/MAJO-000/STATUS.md",
    "work/MAJO-000/EVIDENCE.md",
    "work/MAJO-000/REVIEW.md",
]

REFERENCE_PROJECTS = [
    "Grantapher/ValheimPlus",
    "Valheim-Modding/Jotunn",
    "fire-VA/FiresGhettoNetworking",
    "shudnal/ExtraSlots",
    "morda0511/WorkbenchesPlus",
    "AdvizeGH/Advize_ValheimMods",
    "sirskunkalot/PlanBuild",
    "blaxxun-boop/PassivePowers",
    "Turbero/valheim-DetailedLevels",
    "virtuaCode/valheim-mods",
    "ontrigger/ValheimPerformanceOptimizations",
    "gbahns/ValheimMods",
    "f00d4tehg0dz/valheim-webmap",
    "TayrusCz/GearAndStorage",
    "hldblc/ValheimAdminPanel",
]

ALLOWED_STATES = {
    "PLANNED", "INTAKE", "PLANNING", "READY", "IMPLEMENTING", "VERIFYING",
    "IN_REVIEW", "SECURITY_REVIEW", "READY_TO_MERGE", "MERGED", "CLOSEOUT",
    "DONE", "BLOCKED", "CANCELLED",
}

errors = []

for rel in REQUIRED:
    if not (ROOT / rel).is_file():
        errors.append(f"missing required file: {rel}")

def text(rel: str) -> str:
    path = ROOT / rel
    return path.read_text(encoding="utf-8") if path.is_file() else ""

catalog = text("docs/FEATURE-CATALOG.md")
refs = text("docs/REFERENCE-PROJECTS.md")
conflicts = text("docs/CONFLICTS.md")
deps = text("docs/DEPENDENCIES.md")
versioning = text("docs/VERSIONING.md")
status = text("work/MAJO-000/STATUS.md")
patch_ownership = text("docs/PATCH-OWNERSHIP.md")

for project in REFERENCE_PROJECTS:
    if project not in refs:
        errors.append(f"reference project missing: {project}")

if re.search(r"\bTBD\b", catalog):
    errors.append("FEATURE-CATALOG still contains TBD")

for section in range(1, 8):
    if f"## {section}." not in conflicts:
        errors.append(f"conflict category {section} missing")

for framework in ("BepInEx", "Jötunn"):
    if framework not in deps:
        errors.append(f"approved framework missing from DEPENDENCIES: {framework}")

for token in ("0.0.0", "0.0.1", "0.1.0", "1.0.0", "MAJOR.MINOR.PATCH"):
    if token not in versioning:
        errors.append(f"versioning token missing: {token}")

state_match = re.search(r"^STATE:\s*(\S+)", status, re.MULTILINE)
if not state_match or state_match.group(1) not in ALLOWED_STATES:
    errors.append("MAJO-000 STATUS has invalid/missing STATE")

if errors:
    print("MAJO-000 foundation validation: FAIL")
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("MAJO-000 foundation validation: PASS")
print(f"- required files: {len(REQUIRED)}")
print(f"- reference projects: {len(REFERENCE_PROJECTS)}")
print("- conflict categories: 7")
print("- feature catalog: no TBD")
print("- patch ownership: one owner per critical surface")
