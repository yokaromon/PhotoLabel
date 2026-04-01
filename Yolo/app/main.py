from __future__ import annotations

from pathlib import Path

from fastapi import FastAPI, File, Form, HTTPException, Request, UploadFile
from fastapi.responses import FileResponse, HTMLResponse, JSONResponse, Response
from fastapi.staticfiles import StaticFiles
from fastapi.templating import Jinja2Templates
from pydantic import BaseModel, Field

from app.services.class_service import ClassService
from app.services.dataset_service import DatasetService
from app.services.file_service import FileService
from app.services.inference_service import InferenceService
from app.services.train_service import TrainService

APP_DIR = Path(__file__).resolve().parent
PROJECT_ROOT = APP_DIR.parent
WORKSPACE_ROOT = PROJECT_ROOT / "workspace"

file_service = FileService(WORKSPACE_ROOT)
class_service = ClassService(WORKSPACE_ROOT)
dataset_service = DatasetService(file_service, class_service)
train_service = TrainService(WORKSPACE_ROOT)
inference_service = InferenceService(WORKSPACE_ROOT)

app = FastAPI(title="License Plate YOLO Trainer")
app.mount("/photolabel/static", StaticFiles(directory=str(APP_DIR / "static")), name="static")
templates = Jinja2Templates(directory=str(APP_DIR / "templates"))



class ClassesRequest(BaseModel):
    classes: list[str]


class AnnotationBox(BaseModel):
    class_id: int = Field(ge=0)
    x_center: float = Field(ge=0.0, le=1.0)
    y_center: float = Field(ge=0.0, le=1.0)
    width: float = Field(ge=0.0, le=1.0)
    height: float = Field(ge=0.0, le=1.0)


class AnnotationRequest(BaseModel):
    boxes: list[AnnotationBox]


class PrepareDatasetRequest(BaseModel):
    train_ratio: float = Field(default=0.8, gt=0.0, lt=1.0)
    seed: int = 42


class TrainRequest(BaseModel):
    model: str = "yolov8n.pt"
    epochs: int = 50
    imgsz: int = 640
    batch: int = 8


@app.get("/photolabel", response_class=HTMLResponse)
async def index(request: Request) -> HTMLResponse:
    return templates.TemplateResponse("index.html", {"request": request})


@app.get("/photolabel/api/classes")
async def get_classes() -> JSONResponse:
    return JSONResponse({"classes": class_service.get_classes()})


@app.post("/photolabel/api/classes")
async def save_classes(payload: ClassesRequest) -> JSONResponse:
    try:
        classes = class_service.save_classes(payload.classes)
        return JSONResponse({"classes": classes})
    except ValueError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc


@app.post("/photolabel/api/load-images")
async def load_images(files: list[UploadFile] = File(...)) -> JSONResponse:
    try:
        files_data = [
            (f.filename or f.filename, await f.read())
            for f in files
            if f.filename and Path(f.filename).suffix.lower() in {".jpg", ".jpeg", ".png"}
        ]
        images = file_service.save_uploaded_images(files_data)
        return JSONResponse({
            "folder": str(file_service.get_loaded_folder()),
            "count": len(images),
            "images": [path.name for path in images],
            "classes": class_service.get_classes(),
        })
    except (ValueError, OSError) as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc


@app.get("/photolabel/api/images")
async def get_images() -> JSONResponse:
    images = file_service.get_loaded_images()
    return JSONResponse({
        "folder": str(file_service.get_loaded_folder()) if file_service.get_loaded_folder() else None,
        "count": len(images),
        "images": [{"index": idx, "name": path.name} for idx, path in enumerate(images)],
        "classes": class_service.get_classes(),
    })


@app.get("/photolabel/api/image/{index}")
async def get_image(index: int) -> FileResponse:
    try:
        return FileResponse(file_service.get_image_by_index(index))
    except IndexError as exc:
        raise HTTPException(status_code=404, detail=str(exc)) from exc


@app.get("/photolabel/api/annotations/{image_name}")
async def get_annotations(image_name: str) -> JSONResponse:
    try:
        return JSONResponse(dataset_service.read_annotations(image_name))
    except FileNotFoundError as exc:
        raise HTTPException(status_code=404, detail=str(exc)) from exc


@app.post("/photolabel/api/annotations/{image_name}")
async def save_annotations(image_name: str, payload: AnnotationRequest) -> JSONResponse:
    try:
        return JSONResponse(dataset_service.save_annotations(image_name, [box.model_dump() for box in payload.boxes]))
    except FileNotFoundError as exc:
        raise HTTPException(status_code=404, detail=str(exc)) from exc
    except ValueError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc


@app.post("/photolabel/api/prepare-dataset")
async def prepare_dataset(payload: PrepareDatasetRequest) -> JSONResponse:
    try:
        return JSONResponse(dataset_service.prepare_dataset(train_ratio=payload.train_ratio, seed=payload.seed))
    except ValueError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc


@app.post("/photolabel/api/train")
async def train(payload: TrainRequest) -> JSONResponse:
    data_yaml = file_service.dataset_dir / "data.yaml"
    if not data_yaml.exists():
        raise HTTPException(status_code=400, detail="Dataset is not prepared yet.")

    try:
        return JSONResponse(train_service.start_training(data_yaml=data_yaml, model=payload.model, epochs=payload.epochs, imgsz=payload.imgsz, batch=payload.batch))
    except RuntimeError as exc:
        raise HTTPException(status_code=409, detail=str(exc)) from exc


@app.get("/photolabel/api/train-status")
async def train_status() -> JSONResponse:
    return JSONResponse(train_service.get_status())


@app.post("/photolabel/api/detect-board")
async def detect_board(
    image: UploadFile = File(...),
    model_path: str | None = Form(default=None),
    target_label: str | None = Form(default=None),
) -> JSONResponse:
    image_bytes = await image.read()
    try:
        return JSONResponse(
            inference_service.detect_board(
                image_bytes=image_bytes,
                model_path=model_path,
                target_label=target_label,
            )
        )
    except (FileNotFoundError, ValueError) as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc


@app.post("/photolabel/api/crop-board")
async def crop_board(
    image: UploadFile = File(...),
    model_path: str | None = Form(default=None),
    margin_ratio: float = Form(default=0.0),
    target_label: str | None = Form(default=None),
) -> Response:
    image_bytes = await image.read()
    try:
        cropped_bytes, metadata = inference_service.crop_board(
            image_bytes=image_bytes,
            model_path=model_path,
            margin_ratio=margin_ratio,
            target_label=target_label,
        )
        return Response(
            content=cropped_bytes,
            media_type=metadata["content_type"],
            headers={
                "X-Detection-Found": "true",
                "X-Detection-Confidence": str(metadata["confidence"]),
                "X-Detection-Class-Id": str(metadata["class_id"]),
                "X-Detection-Class-Name": str(metadata["class_name"]),
                "X-BBox-X1": str(metadata["bbox"]["x1"]),
                "X-BBox-Y1": str(metadata["bbox"]["y1"]),
                "X-BBox-X2": str(metadata["bbox"]["x2"]),
                "X-BBox-Y2": str(metadata["bbox"]["y2"]),
                "X-Crop-X1": str(metadata["crop_bbox"]["x1"]),
                "X-Crop-Y1": str(metadata["crop_bbox"]["y1"]),
                "X-Crop-X2": str(metadata["crop_bbox"]["x2"]),
                "X-Crop-Y2": str(metadata["crop_bbox"]["y2"]),
                "X-Model-Path": metadata["model_path"],
            },
        )
    except (FileNotFoundError, ValueError) as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc


@app.get("/photolabel/api/health")
async def health() -> JSONResponse:
    return JSONResponse({"status": "ok"})


if __name__ == "__main__":
    import configparser
    import uvicorn

    _config = configparser.ConfigParser()
    _config.read(Path(__file__).resolve().parent.parent / "config.ini")

    _host = _config.get("server", "host", fallback="127.0.0.1")
    _port = _config.getint("server", "port", fallback=8000)

    uvicorn.run("app.main:app", host=_host, port=_port, reload=True)
