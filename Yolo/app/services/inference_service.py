from __future__ import annotations

import io
from pathlib import Path
from typing import Any

from PIL import Image
from ultralytics import YOLO


class InferenceService:
    def __init__(self, workspace_root: Path) -> None:
        self.workspace_root = workspace_root
        self._model_cache: dict[str, YOLO] = {}

    def _resolve_model_path(self, model_path: str | None) -> Path:
        candidate = Path(model_path).expanduser() if model_path else self.workspace_root / "runs" / "train" / "weights" / "best.pt"
        if not candidate.exists():
            raise FileNotFoundError(f"Model file not found: {candidate}")
        return candidate.resolve()

    def _get_model(self, model_path: str | None) -> tuple[YOLO, Path]:
        resolved = self._resolve_model_path(model_path)
        key = str(resolved)
        if key not in self._model_cache:
            self._model_cache[key] = YOLO(str(resolved))
        return self._model_cache[key], resolved

    @staticmethod
    def _load_image(image_bytes: bytes) -> Image.Image:
        return Image.open(io.BytesIO(image_bytes)).convert("RGB")

    @staticmethod
    def _model_names(model: YOLO) -> dict[int, str]:
        names = model.names
        if isinstance(names, dict):
            return {int(key): str(value) for key, value in names.items()}
        return {index: str(value) for index, value in enumerate(names)}

    def _normalize_target_label(self, target_label: str | None, model: YOLO) -> int | None:
        if not target_label:
            return None
        if target_label.isdigit():
            return int(target_label)
        model_names = self._model_names(model)
        normalized = target_label.strip().lower()
        for class_id, class_name in model_names.items():
            if class_name.lower() == normalized:
                return class_id
        raise ValueError(f"Unsupported target_label: {target_label}")

    def detect_board(self, image_bytes: bytes, model_path: str | None = None, target_label: str | None = None) -> dict[str, Any]:
        image = self._load_image(image_bytes)
        model, resolved_model = self._get_model(model_path)
        results = model.predict(image, verbose=False)
        result = results[0]
        width, height = image.size
        model_names = self._model_names(model)
        target_class_id = self._normalize_target_label(target_label, model)

        detections = []
        if result.boxes is not None:
            for box in result.boxes:
                class_id = int(box.cls[0])
                if target_class_id is not None and class_id != target_class_id:
                    continue
                xyxy = box.xyxy[0].tolist()
                detections.append(
                    {
                        "class_id": class_id,
                        "class_name": model_names.get(class_id, f"class_{class_id}"),
                        "confidence": float(box.conf[0]),
                        "bbox": {
                            "x1": max(0, int(round(xyxy[0]))),
                            "y1": max(0, int(round(xyxy[1]))),
                            "x2": min(width, int(round(xyxy[2]))),
                            "y2": min(height, int(round(xyxy[3]))),
                        },
                    }
                )

        detections.sort(key=lambda item: item["confidence"], reverse=True)
        best = detections[0] if detections else None
        return {
            "found": bool(best),
            "bbox": best["bbox"] if best else None,
            "confidence": best["confidence"] if best else None,
            "class_id": best["class_id"] if best else None,
            "class_name": best["class_name"] if best else None,
            "detections": detections,
            "classes": [model_names[index] for index in sorted(model_names)],
            "image_width": width,
            "image_height": height,
            "model_path": str(resolved_model),
            "target_label": target_label,
        }

    def crop_board(
        self,
        image_bytes: bytes,
        model_path: str | None = None,
        margin_ratio: float = 0.0,
        target_label: str | None = None,
    ) -> tuple[bytes, dict[str, Any]]:
        detection = self.detect_board(
            image_bytes=image_bytes,
            model_path=model_path,
            target_label=target_label,
        )
        if not detection["found"]:
            raise ValueError("No target was detected in the image.")

        image = self._load_image(image_bytes)
        width, height = image.size
        bbox = detection["bbox"]
        box_width = bbox["x2"] - bbox["x1"]
        box_height = bbox["y2"] - bbox["y1"]
        margin_x = int(round(box_width * margin_ratio))
        margin_y = int(round(box_height * margin_ratio))

        left = max(0, bbox["x1"] - margin_x)
        top = max(0, bbox["y1"] - margin_y)
        right = min(width, bbox["x2"] + margin_x)
        bottom = min(height, bbox["y2"] + margin_y)

        cropped = image.crop((left, top, right, bottom))
        output = io.BytesIO()
        cropped.save(output, format="JPEG", quality=95)
        metadata = {
            **detection,
            "crop_bbox": {"x1": left, "y1": top, "x2": right, "y2": bottom},
            "content_type": "image/jpeg",
        }
        return output.getvalue(), metadata
