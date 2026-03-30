from __future__ import annotations

import threading
from pathlib import Path
from typing import Any

from ultralytics import YOLO


class TrainService:
    def __init__(self, workspace_root: Path) -> None:
        self.workspace_root = workspace_root
        self._lock = threading.Lock()
        self._thread: threading.Thread | None = None
        self._status: dict[str, Any] = {
            "is_training": False,
            "status": "idle",
            "logs": [],
            "result": None,
            "error": None,
        }

    def get_status(self) -> dict[str, Any]:
        with self._lock:
            return {
                "is_training": self._status["is_training"],
                "status": self._status["status"],
                "logs": list(self._status["logs"]),
                "result": self._status["result"],
                "error": self._status["error"],
            }

    def start_training(self, data_yaml: Path, model: str, epochs: int, imgsz: int, batch: int) -> dict:
        with self._lock:
            if self._status["is_training"]:
                raise RuntimeError("Training is already running.")
            self._status = {
                "is_training": True,
                "status": "running",
                "logs": [],
                "result": None,
                "error": None,
            }

        self._append_log(f"Training started with model={model}, epochs={epochs}, imgsz={imgsz}, batch={batch}")
        self._thread = threading.Thread(
            target=self._run_training,
            args=(data_yaml, model, epochs, imgsz, batch),
            daemon=True,
        )
        self._thread.start()
        return self.get_status()

    def _run_training(self, data_yaml: Path, model: str, epochs: int, imgsz: int, batch: int) -> None:
        try:
            yolo = YOLO(model)
            self._append_log(f"Loaded model: {model}")
            results = yolo.train(
                data=str(data_yaml),
                epochs=epochs,
                imgsz=imgsz,
                batch=batch,
                project=str(self.workspace_root / "runs"),
                name="train",
                exist_ok=True,
            )
            save_dir = Path(str(results.save_dir)).resolve()
            result = {
                "success": True,
                "output_dir": str(save_dir),
                "best_pt": str((save_dir / "weights" / "best.pt").resolve()),
                "last_pt": str((save_dir / "weights" / "last.pt").resolve()),
            }
            self._append_log(f"Training completed: {save_dir}")
            with self._lock:
                self._status["is_training"] = False
                self._status["status"] = "completed"
                self._status["result"] = result
        except Exception as exc:
            self._append_log(f"Training failed: {exc}")
            with self._lock:
                self._status["is_training"] = False
                self._status["status"] = "failed"
                self._status["error"] = str(exc)

    def _append_log(self, message: str) -> None:
        with self._lock:
            self._status["logs"].append(message)
            self._status["logs"] = self._status["logs"][-200:]
