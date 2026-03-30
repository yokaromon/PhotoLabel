from __future__ import annotations

import random
import shutil

import yaml

from .class_service import ClassService
from .file_service import FileService


class DatasetService:
    def __init__(self, file_service: FileService, class_service: ClassService) -> None:
        self.file_service = file_service
        self.class_service = class_service

    def read_annotations(self, image_name: str) -> dict:
        image_path = self.file_service.get_image_by_name(image_name)
        label_path = self.file_service.get_label_path(image_path)
        classes = self.class_service.get_classes()
        class_id_to_name = {index: name for index, name in enumerate(classes)}
        if not label_path.exists():
            return {"image_name": image_name, "boxes": [], "classes": classes}

        boxes = []
        for line in label_path.read_text(encoding="utf-8").splitlines():
            parts = line.strip().split()
            if len(parts) != 5:
                continue
            class_id, x_center, y_center, width, height = parts
            class_index = int(float(class_id))
            boxes.append(
                {
                    "class_id": class_index,
                    "class_name": class_id_to_name.get(class_index, f"class_{class_index}"),
                    "x_center": float(x_center),
                    "y_center": float(y_center),
                    "width": float(width),
                    "height": float(height),
                }
            )
        return {"image_name": image_name, "boxes": boxes, "classes": classes}

    def save_annotations(self, image_name: str, boxes: list[dict]) -> dict:
        image_path = self.file_service.get_image_by_name(image_name)
        label_path = self.file_service.get_label_path(image_path)
        classes = self.class_service.get_classes()

        lines = []
        for box in boxes:
            class_id = int(box["class_id"])
            if class_id < 0 or class_id >= len(classes):
                raise ValueError(f"Unsupported class_id: {class_id}")
            lines.append(
                " ".join(
                    [
                        str(class_id),
                        f"{float(box['x_center']):.6f}",
                        f"{float(box['y_center']):.6f}",
                        f"{float(box['width']):.6f}",
                        f"{float(box['height']):.6f}",
                    ]
                )
            )

        label_path.write_text("\n".join(lines), encoding="utf-8")
        return {
            "image_name": image_name,
            "saved_count": len(lines),
            "label_path": str(label_path),
            "classes": classes,
        }

    def prepare_dataset(self, train_ratio: float = 0.8, seed: int = 42) -> dict:
        images = self.file_service.get_loaded_images()
        labeled_pairs = []
        for image_path in images:
            label_path = self.file_service.get_label_path(image_path)
            if label_path.exists() and label_path.read_text(encoding="utf-8").strip():
                labeled_pairs.append((image_path, label_path))

        if not labeled_pairs:
            raise ValueError("No labeled images were found. Save at least one annotation first.")

        classes = self.class_service.get_classes()
        dataset_dir = self.file_service.dataset_dir
        if dataset_dir.exists():
            shutil.rmtree(dataset_dir)

        for subset in ["train", "val"]:
            (dataset_dir / "images" / subset).mkdir(parents=True, exist_ok=True)
            (dataset_dir / "labels" / subset).mkdir(parents=True, exist_ok=True)

        shuffled = labeled_pairs[:]
        random.Random(seed).shuffle(shuffled)

        train_count = max(1, int(len(shuffled) * train_ratio))
        if len(shuffled) > 1:
            train_count = min(train_count, len(shuffled) - 1)

        train_items = shuffled[:train_count]
        val_items = shuffled[train_count:] if len(shuffled) > 1 else shuffled
        if not val_items:
            val_items = train_items[-1:]
            train_items = train_items[:-1] or train_items

        for split_name, items in {"train": train_items, "val": val_items}.items():
            for image_path, label_path in items:
                shutil.copy2(image_path, dataset_dir / "images" / split_name / image_path.name)
                shutil.copy2(label_path, dataset_dir / "labels" / split_name / label_path.name)

        data_yaml = {
            "path": str(dataset_dir.resolve()),
            "train": "images/train",
            "val": "images/val",
            "names": {index: name for index, name in enumerate(classes)},
        }
        data_yaml_path = dataset_dir / "data.yaml"
        data_yaml_path.write_text(yaml.safe_dump(data_yaml, sort_keys=False), encoding="utf-8")

        return {
            "train_count": len(train_items),
            "val_count": len(val_items),
            "dataset_dir": str(dataset_dir),
            "data_yaml": str(data_yaml_path),
            "classes": classes,
        }
