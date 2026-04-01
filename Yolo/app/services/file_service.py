from __future__ import annotations

from pathlib import Path
from threading import Lock

IMAGE_EXTENSIONS = {".jpg", ".jpeg", ".png"}


class FileService:
    def __init__(self, workspace_root: Path) -> None:
        self.workspace_root = workspace_root
        self.source_images_dir = workspace_root / "source_images"
        self.dataset_dir = workspace_root / "dataset"
        self.runs_dir = workspace_root / "runs"
        self._images: list[Path] = []
        self._folder: Path | None = None
        self._lock = Lock()
        self.ensure_workspace()

    def ensure_workspace(self) -> None:
        for path in [
            self.workspace_root,
            self.source_images_dir,
            self.dataset_dir / "images" / "train",
            self.dataset_dir / "images" / "val",
            self.dataset_dir / "labels" / "train",
            self.dataset_dir / "labels" / "val",
            self.runs_dir,
        ]:
            path.mkdir(parents=True, exist_ok=True)

    def save_uploaded_images(self, files_data: list[tuple[str, bytes]]) -> list[Path]:
        """アップロードされた画像を source_images_dir に保存して登録する。"""
        saved: list[Path] = []
        for filename, data in files_data:
            # webkitdirectory はパス区切りを含む場合があるのでベース名のみ使用
            name = Path(filename).name
            dest = self.source_images_dir / name
            dest.write_bytes(data)
            saved.append(dest)

        saved.sort(key=lambda p: p.name)
        if not saved:
            raise ValueError("No image files were uploaded.")

        with self._lock:
            self._folder = self.source_images_dir
            self._images = saved
        return saved

    def get_loaded_images(self) -> list[Path]:
        with self._lock:
            return list(self._images)

    def get_loaded_folder(self) -> Path | None:
        with self._lock:
            return self._folder

    def get_image_by_index(self, index: int) -> Path:
        images = self.get_loaded_images()
        if index < 0 or index >= len(images):
            raise IndexError("Image index out of range.")
        return images[index]

    def get_image_by_name(self, image_name: str) -> Path:
        for path in self.get_loaded_images():
            if path.name == image_name:
                return path
        raise FileNotFoundError(f"Image is not loaded: {image_name}")

    @staticmethod
    def get_label_path(image_path: Path) -> Path:
        return image_path.with_suffix(".txt")
