const state = {
  images: [],
  currentIndex: 0,
  boxes: [],
  classes: [],
  image: null,
  isDrawing: false,
  startPoint: null,
  previewBox: null,
  inference: {
    file: null,
    image: null,
    detection: null,
    cropUrl: null,
  },
};

const classColors = ["#c84c1a", "#2b7a78", "#7b5ea7", "#c49a1a", "#ca3c70", "#3a86ff"];
const canvas = document.getElementById("imageCanvas");
const ctx = canvas.getContext("2d");
const inferenceCanvas = document.getElementById("inferenceCanvas");
const inferenceCtx = inferenceCanvas.getContext("2d");
const cropPreview = document.getElementById("cropPreview");

async function fetchJson(url, options = {}) {
  const response = await fetch(url, options);
  const data = await response.json();
  if (!response.ok) {
    throw new Error(data.detail || "Request failed");
  }
  return data;
}

function classColor(classId) {
  return classColors[classId % classColors.length];
}

function className(classId) {
  return state.classes[classId] || `class_${classId}`;
}

function currentSelectedClassId() {
  const value = document.getElementById("currentClassSelect").value;
  return Number(value || 0);
}

function getCanvasPoint(event) {
  const rect = canvas.getBoundingClientRect();
  const scaleX = canvas.width / rect.width;
  const scaleY = canvas.height / rect.height;
  return {
    x: (event.clientX - rect.left) * scaleX,
    y: (event.clientY - rect.top) * scaleY,
  };
}

function populateSelect(select, options, includeAny = false, selectedValue = null) {
  const previousValue = selectedValue ?? select.value;
  select.innerHTML = "";
  if (includeAny) {
    const anyOption = document.createElement("option");
    anyOption.value = "";
    anyOption.textContent = "Any";
    select.appendChild(anyOption);
  }
  options.forEach((name, index) => {
    const option = document.createElement("option");
    option.value = String(index);
    option.textContent = name;
    select.appendChild(option);
  });
  if ([...select.options].some((option) => option.value === String(previousValue))) {
    select.value = String(previousValue);
  } else if (select.options.length > 0) {
    select.selectedIndex = 0;
  }
}

function applyClasses(classes) {
  state.classes = classes;
  document.getElementById("classPatternsInput").value = classes.join(", ");
  populateSelect(document.getElementById("currentClassSelect"), classes, false, 0);
  populateSelect(document.getElementById("inferenceTargetLabel"), classes, true, document.getElementById("inferenceTargetLabel").value);
  state.boxes = state.boxes.map((box) => ({ ...box, class_id: Math.min(box.class_id, Math.max(classes.length - 1, 0)) }));
  updateBoxCount();
  renderBoxList();
  drawCanvas();
}

function updateBoxCount() {
  document.getElementById("boxCount").textContent = `矩形数: ${state.boxes.length}`;
}

function renderBoxList() {
  const boxList = document.getElementById("boxList");
  boxList.innerHTML = "";
  if (!state.boxes.length) {
    const empty = document.createElement("p");
    empty.className = "box-empty";
    empty.textContent = "まだ矩形はありません。";
    boxList.appendChild(empty);
    return;
  }

  state.boxes.forEach((box, index) => {
    const row = document.createElement("div");
    row.className = "box-row";

    const badge = document.createElement("span");
    badge.className = "box-badge";
    badge.style.backgroundColor = classColor(box.class_id);
    badge.textContent = `#${index + 1}`;

    const select = document.createElement("select");
    state.classes.forEach((name, classId) => {
      const option = document.createElement("option");
      option.value = String(classId);
      option.textContent = name;
      if (classId === box.class_id) {
        option.selected = true;
      }
      select.appendChild(option);
    });
    select.addEventListener("change", (event) => {
      state.boxes[index].class_id = Number(event.target.value);
      renderBoxList();
      drawCanvas();
    });

    const meta = document.createElement("div");
    meta.className = "box-meta";
    meta.textContent = `center=(${box.x_center.toFixed(3)}, ${box.y_center.toFixed(3)}) size=(${box.width.toFixed(3)}, ${box.height.toFixed(3)})`;

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.textContent = "削除";
    removeButton.addEventListener("click", () => {
      state.boxes.splice(index, 1);
      updateBoxCount();
      renderBoxList();
      drawCanvas();
    });

    row.appendChild(badge);
    row.appendChild(select);
    row.appendChild(meta);
    row.appendChild(removeButton);
    boxList.appendChild(row);
  });
}

function drawCanvas() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);
  ctx.fillStyle = "#ece8df";
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  if (state.image) {
    ctx.drawImage(state.image, 0, 0, canvas.width, canvas.height);
  }

  ctx.lineWidth = 2;
  state.boxes.forEach((box) => {
    const x = (box.x_center - box.width / 2) * canvas.width;
    const y = (box.y_center - box.height / 2) * canvas.height;
    ctx.strokeStyle = classColor(box.class_id);
    ctx.strokeRect(x, y, box.width * canvas.width, box.height * canvas.height);
    ctx.fillStyle = classColor(box.class_id);
    ctx.fillRect(x, Math.max(0, y - 22), 120, 20);
    ctx.fillStyle = "#ffffff";
    ctx.font = "12px sans-serif";
    ctx.fillText(className(box.class_id), x + 6, Math.max(14, y - 8));
  });

  if (state.previewBox) {
    ctx.strokeStyle = classColor(currentSelectedClassId());
    ctx.strokeRect(state.previewBox.x, state.previewBox.y, state.previewBox.width, state.previewBox.height);
  }
}

function drawInferenceCanvas() {
  inferenceCtx.clearRect(0, 0, inferenceCanvas.width, inferenceCanvas.height);
  inferenceCtx.fillStyle = "#ece8df";
  inferenceCtx.fillRect(0, 0, inferenceCanvas.width, inferenceCanvas.height);

  if (!state.inference.image) {
    return;
  }

  inferenceCtx.drawImage(state.inference.image, 0, 0, inferenceCanvas.width, inferenceCanvas.height);

  if (!state.inference.detection?.detections?.length) {
    return;
  }

  const scaleX = inferenceCanvas.width / state.inference.detection.image_width;
  const scaleY = inferenceCanvas.height / state.inference.detection.image_height;
  state.inference.detection.detections.forEach((item) => {
    const bbox = item.bbox;
    inferenceCtx.lineWidth = 3;
    inferenceCtx.strokeStyle = classColor(item.class_id);
    inferenceCtx.strokeRect(
      bbox.x1 * scaleX,
      bbox.y1 * scaleY,
      (bbox.x2 - bbox.x1) * scaleX,
      (bbox.y2 - bbox.y1) * scaleY,
    );
    const labelX = bbox.x1 * scaleX;
    const labelY = Math.max(0, bbox.y1 * scaleY - 24);
    inferenceCtx.fillStyle = classColor(item.class_id);
    inferenceCtx.fillRect(labelX, labelY, 180, 20);
    inferenceCtx.fillStyle = "#ffffff";
    inferenceCtx.font = "12px sans-serif";
    inferenceCtx.fillText(`${item.class_name} ${item.confidence.toFixed(3)}`, labelX + 6, labelY + 14);
  });
}

function updateInferenceText(lines) {
  document.getElementById("inferenceResult").textContent = lines.join("\n");
}

function setInferenceStatus(text) {
  document.getElementById("inferenceStatus").textContent = text;
}

async function loadClasses() {
  const data = await fetchJson("/api/classes");
  applyClasses(data.classes);
  document.getElementById("classesStatus").textContent = `利用中クラス: ${data.classes.join(", ")}`;
}

async function saveClasses() {
  const classes = document.getElementById("classPatternsInput").value.split(",").map((item) => item.trim()).filter(Boolean);
  const data = await fetchJson("/api/classes", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ classes }),
  });
  applyClasses(data.classes);
  document.getElementById("classesStatus").textContent = `クラスを更新しました: ${data.classes.join(", ")}`;
}

async function loadAnnotations(imageName) {
  const data = await fetchJson(`/api/annotations/${encodeURIComponent(imageName)}`);
  applyClasses(data.classes || state.classes);
  state.boxes = (data.boxes || []).map((box) => ({
    class_id: Number(box.class_id ?? 0),
    x_center: box.x_center,
    y_center: box.y_center,
    width: box.width,
    height: box.height,
  }));
  state.previewBox = null;
  updateBoxCount();
  renderBoxList();
}

async function loadImageAt(index) {
  if (!state.images.length) {
    return;
  }
  state.currentIndex = index;
  const imageInfo = state.images[index];
  document.getElementById("imageStatus").textContent = `${index + 1} / ${state.images.length}`;
  document.getElementById("imageName").textContent = imageInfo.name;

  const image = new Image();
  image.onload = async () => {
    state.image = image;
    await loadAnnotations(imageInfo.name);
    drawCanvas();
  };
  image.src = `/api/image/${index}`;
}

async function loadImages() {
  const folderPath = document.getElementById("folderPath").value.trim();
  const data = await fetchJson("/api/load-images", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ folder_path: folderPath || null }),
  });
  applyClasses(data.classes || state.classes);
  state.images = data.images.map((name, index) => ({ name, index }));
  await loadImageAt(0);
}

async function saveAnnotations() {
  const current = state.images[state.currentIndex];
  if (!current) {
    return;
  }
  await fetchJson(`/api/annotations/${encodeURIComponent(current.name)}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ boxes: state.boxes }),
  });
}

async function prepareDataset() {
  const data = await fetchJson("/api/prepare-dataset", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({}),
  });
  document.getElementById("datasetStatus").textContent = `train=${data.train_count}, val=${data.val_count}, classes=${data.classes.join(", ")}`;
}

async function startTraining() {
  await fetchJson("/api/train", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      model: document.getElementById("modelInput").value,
      epochs: Number(document.getElementById("epochsInput").value),
      imgsz: Number(document.getElementById("imgszInput").value),
      batch: Number(document.getElementById("batchInput").value),
    }),
  });
}

async function refreshTrainingStatus() {
  const status = await fetchJson("/api/train-status");
  const lines = [`status: ${status.status}`];
  if (status.result) {
    lines.push(`output_dir: ${status.result.output_dir}`);
    lines.push(`best.pt: ${status.result.best_pt}`);
    lines.push(`last.pt: ${status.result.last_pt}`);
  }
  if (status.error) {
    lines.push(`error: ${status.error}`);
  }
  if (status.logs?.length) {
    lines.push("", ...status.logs);
  }
  document.getElementById("trainLog").textContent = lines.join("\n");
}

function getInferenceFormData() {
  if (!state.inference.file) {
    throw new Error("推論画像を選択してください。");
  }
  const formData = new FormData();
  formData.append("image", state.inference.file);

  const modelPath = document.getElementById("inferenceModelPath").value.trim();
  if (modelPath) {
    formData.append("model_path", modelPath);
  }

  const targetLabelValue = document.getElementById("inferenceTargetLabel").value;
  if (targetLabelValue !== "") {
    formData.append("target_label", targetLabelValue);
  }
  return formData;
}

async function detectBoard() {
  const formData = getInferenceFormData();
  setInferenceStatus("bbox を取得しています...");
  const response = await fetch("/api/detect-board", {
    method: "POST",
    body: formData,
  });
  const data = await response.json();
  if (!response.ok) {
    throw new Error(data.detail || "detect-board failed");
  }

  state.inference.detection = data;
  drawInferenceCanvas();

  const lines = [
    `found: ${data.found}`,
    `selected target: ${data.target_label || "Any"}`,
    `detections: ${data.detections.length}`,
    `model_path: ${data.model_path}`,
  ];
  data.detections.forEach((item, index) => {
    lines.push(`${index + 1}. ${item.class_name} conf=${item.confidence.toFixed(3)} bbox=(${item.bbox.x1}, ${item.bbox.y1}) - (${item.bbox.x2}, ${item.bbox.y2})`);
  });
  updateInferenceText(lines);
  setInferenceStatus(data.found ? "bbox を取得しました。" : "対象は見つかりませんでした。");
}

async function cropBoard() {
  const formData = getInferenceFormData();
  formData.append("margin_ratio", document.getElementById("marginRatioInput").value || "0");
  setInferenceStatus("crop 画像を取得しています...");

  const response = await fetch("/api/crop-board", {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    const data = await response.json();
    throw new Error(data.detail || "crop-board failed");
  }

  const blob = await response.blob();
  if (state.inference.cropUrl) {
    URL.revokeObjectURL(state.inference.cropUrl);
  }
  state.inference.cropUrl = URL.createObjectURL(blob);
  cropPreview.src = state.inference.cropUrl;

  updateInferenceText([
    `class: ${response.headers.get("X-Detection-Class-Name") ?? "-"} (${response.headers.get("X-Detection-Class-Id") ?? "-"})`,
    `confidence: ${response.headers.get("X-Detection-Confidence") ?? "-"}`,
    `bbox: (${response.headers.get("X-BBox-X1")}, ${response.headers.get("X-BBox-Y1")}) - (${response.headers.get("X-BBox-X2")}, ${response.headers.get("X-BBox-Y2")})`,
    `crop: (${response.headers.get("X-Crop-X1")}, ${response.headers.get("X-Crop-Y1")}) - (${response.headers.get("X-Crop-X2")}, ${response.headers.get("X-Crop-Y2")})`,
    `model_path: ${response.headers.get("X-Model-Path") ?? "-"}`,
  ]);
  setInferenceStatus("crop 画像を取得しました。");
}

function loadInferencePreview(file) {
  state.inference.file = file;
  state.inference.detection = null;
  if (state.inference.cropUrl) {
    URL.revokeObjectURL(state.inference.cropUrl);
    state.inference.cropUrl = null;
  }
  cropPreview.removeAttribute("src");
  updateInferenceText(["結果はまだありません。"]);

  const image = new Image();
  image.onload = () => {
    state.inference.image = image;
    drawInferenceCanvas();
  };
  image.src = URL.createObjectURL(file);
  setInferenceStatus(`推論画像: ${file.name}`);
}

canvas.addEventListener("mousedown", (event) => {
  state.isDrawing = true;
  state.startPoint = getCanvasPoint(event);
});

canvas.addEventListener("mousemove", (event) => {
  if (!state.isDrawing || !state.startPoint) {
    return;
  }
  const currentPoint = getCanvasPoint(event);
  state.previewBox = {
    x: Math.min(state.startPoint.x, currentPoint.x),
    y: Math.min(state.startPoint.y, currentPoint.y),
    width: Math.abs(currentPoint.x - state.startPoint.x),
    height: Math.abs(currentPoint.y - state.startPoint.y),
  };
  drawCanvas();
});

canvas.addEventListener("mouseup", (event) => {
  if (!state.isDrawing || !state.startPoint) {
    state.isDrawing = false;
    return;
  }

  const currentPoint = getCanvasPoint(event);
  state.previewBox = {
    x: Math.min(state.startPoint.x, currentPoint.x),
    y: Math.min(state.startPoint.y, currentPoint.y),
    width: Math.abs(currentPoint.x - state.startPoint.x),
    height: Math.abs(currentPoint.y - state.startPoint.y),
  };

  const { x, y, width, height } = state.previewBox;
  if (width > 3 && height > 3) {
    state.boxes.push({
      class_id: currentSelectedClassId(),
      x_center: (x + width / 2) / canvas.width,
      y_center: (y + height / 2) / canvas.height,
      width: width / canvas.width,
      height: height / canvas.height,
    });
  }
  state.isDrawing = false;
  state.startPoint = null;
  state.previewBox = null;
  updateBoxCount();
  renderBoxList();
  drawCanvas();
});

document.getElementById("loadImagesBtn").addEventListener("click", async () => {
  try {
    await loadImages();
  } catch (error) {
    alert(error.message);
  }
});

document.getElementById("saveClassesBtn").addEventListener("click", async () => {
  try {
    await saveClasses();
  } catch (error) {
    alert(error.message);
  }
});

document.getElementById("prevBtn").addEventListener("click", async () => {
  if (state.currentIndex > 0) {
    await loadImageAt(state.currentIndex - 1);
  }
});

document.getElementById("nextBtn").addEventListener("click", async () => {
  if (state.currentIndex < state.images.length - 1) {
    await loadImageAt(state.currentIndex + 1);
  }
});

document.getElementById("saveBtn").addEventListener("click", async () => {
  try {
    await saveAnnotations();
    alert("保存しました。");
  } catch (error) {
    alert(error.message);
  }
});

document.getElementById("undoBtn").addEventListener("click", () => {
  state.boxes.pop();
  updateBoxCount();
  renderBoxList();
  drawCanvas();
});

document.getElementById("clearBtn").addEventListener("click", () => {
  state.boxes = [];
  updateBoxCount();
  renderBoxList();
  drawCanvas();
});

document.getElementById("prepareDatasetBtn").addEventListener("click", async () => {
  try {
    await prepareDataset();
  } catch (error) {
    alert(error.message);
  }
});

document.getElementById("trainBtn").addEventListener("click", async () => {
  try {
    await startTraining();
  } catch (error) {
    alert(error.message);
  }
});

document.getElementById("inferenceImageInput").addEventListener("change", (event) => {
  const [file] = event.target.files || [];
  if (!file) {
    return;
  }
  loadInferencePreview(file);
});

document.getElementById("detectBoardBtn").addEventListener("click", async () => {
  try {
    await detectBoard();
  } catch (error) {
    setInferenceStatus(error.message);
  }
});

document.getElementById("cropBoardBtn").addEventListener("click", async () => {
  try {
    await cropBoard();
  } catch (error) {
    setInferenceStatus(error.message);
  }
});

setInterval(() => {
  refreshTrainingStatus().catch(() => {});
}, 3000);

loadClasses().catch(() => {
  document.getElementById("classesStatus").textContent = "クラス一覧の読み込みに失敗しました。";
});
updateInferenceText(["結果はまだありません。"]);
renderBoxList();
drawCanvas();
drawInferenceCanvas();
