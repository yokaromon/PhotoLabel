from __future__ import annotations

import json
from pathlib import Path

DEFAULT_CLASSES = ["Panel", "Machine", "Sheet"]


class ClassService:
    def __init__(self, workspace_root: Path) -> None:
        self.workspace_root = workspace_root
        self.classes_path = workspace_root / "classes.json"
        self._ensure_default_classes()

    def _ensure_default_classes(self) -> None:
        self.workspace_root.mkdir(parents=True, exist_ok=True)
        if not self.classes_path.exists():
            self.save_classes(DEFAULT_CLASSES)

    def get_classes(self) -> list[str]:
        if not self.classes_path.exists():
            return DEFAULT_CLASSES[:]
        data = json.loads(self.classes_path.read_text(encoding="utf-8"))
        classes = [str(name).strip() for name in data.get("classes", []) if str(name).strip()]
        return classes or DEFAULT_CLASSES[:]

    def save_classes(self, classes: list[str]) -> list[str]:
        normalized = []
        seen = set()
        for name in classes:
            cleaned = str(name).strip()
            if not cleaned:
                continue
            lowered = cleaned.lower()
            if lowered in seen:
                continue
            seen.add(lowered)
            normalized.append(cleaned)

        if not normalized:
            raise ValueError("At least one class label is required.")

        self.classes_path.write_text(
            json.dumps({"classes": normalized}, ensure_ascii=False, indent=2),
            encoding="utf-8",
        )
        return normalized

    def class_name_to_id(self) -> dict[str, int]:
        return {name: index for index, name in enumerate(self.get_classes())}

    def class_id_to_name(self) -> dict[int, str]:
        return {index: name for index, name in enumerate(self.get_classes())}
