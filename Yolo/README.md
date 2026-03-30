# license-plate-yolo-trainer

静止画用の最小 YOLO 学習アプリです。FastAPI ベースの簡易アノテーション UI、dataset 自動生成、Ultralytics YOLO 学習、黒板検出用の推論 API を含みます。

## セットアップ

### 1. poetry で依存関係を入れる

```powershell
poetry install
```

### 2. 起動方法

```powershell
poetry run uvicorn app.main:app --reload
```

### 3. ブラウザでのアクセス方法

ブラウザで `http://127.0.0.1:8000` を開きます。

## ディレクトリ構成

```text
Yolo/
  app/
    main.py
    services/
      dataset_service.py
      file_service.py
      inference_service.py
      train_service.py
    static/
      app.js
      style.css
    templates/
      index.html
  workspace/
    source_images/
    dataset/
    runs/
  pyproject.toml
  README.md
```

`workspace/source_images` に画像を置けば、そのまま読み込み対象として使えます。

## 学習データの作り方

1. 画像を `workspace/source_images` へ置く
2. 画面上で画像フォルダを空欄のまま `読み込み`
3. canvas 上で矩形をドラッグして描画
4. `保存` を押す
5. `Dataset生成` を押す
6. `学習開始` を押す

## YOLO ラベル形式

各画像と同じフォルダに同名の `.txt` を保存します。

```text
class_id x_center y_center width height
```

- `class_id` は固定で `0`
- `x_center`, `y_center`, `width`, `height` は画像サイズ基準の正規化値

## 出力先

- dataset: `workspace/dataset`
- 学習結果: `workspace/runs/train`
- 学習済みモデルの既定パス: `workspace/runs/train/weights/best.pt`

## FastAPI エンドポイント設計

### 学習 UI 用 API

- `POST /api/load-images`
  - 画像フォルダを読み込む
- `GET /api/images`
  - 読み込み済み画像一覧を返す
- `GET /api/image/{index}`
  - 指定 index の画像本体を返す
- `GET /api/annotations/{image_name}`
  - YOLO 形式ラベルを返す
- `POST /api/annotations/{image_name}`
  - YOLO 形式ラベルを保存する
- `POST /api/prepare-dataset`
  - train/val と `data.yaml` を生成する
- `POST /api/train`
  - YOLO 学習をバックグラウンド開始する
- `GET /api/train-status`
  - 学習状態、簡易ログ、出力先を返す

### C# 連携用の黒板検出 API

#### `POST /api/detect-board`

用途:
- C# 側で bbox を受け取り、自前で切り出しや OCR 前処理をしたい場合

リクエスト:
- `multipart/form-data`
- `image`: 画像ファイル
- `model_path`: 任意。省略時は `workspace/runs/train/weights/best.pt`

レスポンス:
- `application/json`
- 最良 1 件の bbox を返す

レスポンス例:

```json
{
  "found": true,
  "bbox": {
    "x1": 120,
    "y1": 80,
    "x2": 980,
    "y2": 620
  },
  "confidence": 0.93,
  "image_width": 1280,
  "image_height": 720,
  "model_path": "Y:\\Projects\\job\\int\\aoyagi\\source\\PhotoLabel\\Yolo\\workspace\\runs\\train\\weights\\best.pt"
}
```

#### `POST /api/crop-board`

用途:
- C# 側で切り出し済み画像をそのまま OCR に回したい場合

リクエスト:
- `multipart/form-data`
- `image`: 画像ファイル
- `model_path`: 任意
- `margin_ratio`: 任意。検出矩形の外側に足す余白率。例 `0.05`

レスポンス:
- `image/jpeg`
- bbox や confidence はレスポンスヘッダに設定

主なレスポンスヘッダ:
- `X-Detection-Confidence`
- `X-BBox-X1`, `X-BBox-Y1`, `X-BBox-X2`, `X-BBox-Y2`
- `X-Crop-X1`, `X-Crop-Y1`, `X-Crop-X2`, `X-Crop-Y2`
- `X-Model-Path`

## C# 呼び出し例

### bbox 返却 API を呼ぶ例

```csharp
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

var client = new HttpClient();
using var form = new MultipartFormDataContent();
using var stream = File.OpenRead(@"C:\work\input.jpg");
using var fileContent = new StreamContent(stream);
fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
form.Add(fileContent, "image", "input.jpg");
form.Add(new StringContent(@"Y:\Projects\job\int\aoyagi\source\PhotoLabel\Yolo\workspace\runs\train\weights\best.pt"), "model_path");

var response = await client.PostAsync("http://127.0.0.1:8000/api/detect-board", form);
response.EnsureSuccessStatusCode();

var json = await response.Content.ReadAsStringAsync();
var result = JsonSerializer.Deserialize<DetectBoardResponse>(json, new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
});

if (result?.Found == true && result.Bbox != null)
{
    Console.WriteLine($"bbox=({result.Bbox.X1},{result.Bbox.Y1})-({result.Bbox.X2},{result.Bbox.Y2})");
    Console.WriteLine($"confidence={result.Confidence}");
}

public sealed class DetectBoardResponse
{
    public bool Found { get; set; }
    public BoundingBox? Bbox { get; set; }
    public double? Confidence { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public string? ModelPath { get; set; }
}

public sealed class BoundingBox
{
    public int X1 { get; set; }
    public int Y1 { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }
}
```

### crop 画像返却 API を呼ぶ例

```csharp
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

var client = new HttpClient();
using var form = new MultipartFormDataContent();
using var stream = File.OpenRead(@"C:\work\input.jpg");
using var fileContent = new StreamContent(stream);
fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
form.Add(fileContent, "image", "input.jpg");
form.Add(new StringContent("0.05"), "margin_ratio");

var response = await client.PostAsync("http://127.0.0.1:8000/api/crop-board", form);
response.EnsureSuccessStatusCode();

var croppedBytes = await response.Content.ReadAsByteArrayAsync();
await File.WriteAllBytesAsync(@"C:\work\cropped-board.jpg", croppedBytes);

var confidence = response.Headers.TryGetValues("X-Detection-Confidence", out var confValues)
    ? confValues.FirstOrDefault()
    : null;

var x1 = response.Headers.GetValues("X-Crop-X1").First();
var y1 = response.Headers.GetValues("X-Crop-Y1").First();
var x2 = response.Headers.GetValues("X-Crop-X2").First();
var y2 = response.Headers.GetValues("X-Crop-Y2").First();

Console.WriteLine($"crop saved: confidence={confidence}, crop=({x1},{y1})-({x2},{y2})");
```

## 実装メモ

- 推論 API は内部で `app/services/inference_service.py` を利用
- 外部公開は HTTP API、内部実装はサービスクラス化、という分離にしている
- `detect-board` は JSON を返し、`crop-board` は JPEG バイナリを返す

## 未実装または簡略化している点

- 学習中の詳細エポック進捗までは表示していません。現状は開始、完了、失敗などの簡易ログです
- 推論クラス名は学習 UI の `plate` 前提のままです。黒板用途では学習データ側を黒板で揃えて使ってください
