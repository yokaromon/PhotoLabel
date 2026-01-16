# PhotoLabel（WinForms版）

## 目的
既存の ASP.NET Core Razor Pages アプリ「PictureSaver」（`Y:\Projects\job\int\aoyagi\source\PictureSaver`）の主要機能を、デスクトップ向け WinForms クライアントとして移植・運用する。

## 概要
画像の閲覧・整理・OCR・リネーム/移動をローカルで完結できるツール。  
ディレクトリツリー／サムネ一覧／プレビュー（WebView2）／OCR の 3 ペイン UI を中心に構成。

## 主要機能
- ディレクトリツリーの遅延展開とサムネイル一覧
- 画像プレビューと OCR 実行
- OCR 結果からの入力補完（Item1〜4）
- リネーム（テンプレート置換）と移動（MoveDir テンプレート）
- 画像のドラッグ&ドロップ（外部からの取り込み／外部アプリへの搬出）

## プログラム構造
- エントリポイント  
  - `Program.cs` が `FrmMain` を起動
- メイン画面  
  - `FrmMain.cs`  
    - ディレクトリツリー／サムネ一覧／プレビュー（WebView2）  
    - OCR 実行、入力補完、リネーム・移動、D&D を集約  
    - ウィンドウ/スプリッタ位置、入力値を `Config.ini` に保存
- 画像プレビュー専用ウィンドウ  
  - `FrmPicture.cs`  
    - 画像のみを大きく表示するシンプルビュー
- サムネカード  
  - `ThumbnailCard.cs`  
    - 画像メタ情報の表示、選択/ハイライト、リネーム UI  
    - サムネキャッシュは `%TEMP%/PhotoLabel/Thumbnails` に生成
- OCR 関連  
  - `Ocr/GoogleVisionClient.cs`  
    - Vision API に画像を送信し結果を解析  
  - `Ocr/OcrService.cs`  
    - OCR 実行、キャッシュ、置換の統合  
  - `Ocr/OcrCacheService.cs`  
    - 画像ハッシュ＋サイズで結果キャッシュ（既定 24h）  
  - `Ocr/ReplaceRuleStore.cs` / `Ocr/ReplaceService.cs`  
    - `ReplaceRules.dat` の置換ルール適用
- 設定読み書き  
  - `Tools/ParameterDict.cs`  
    - INI 風設定の読み書き

## 設定ファイル
`Config.ini` を起動時に読み込む。

```
[Config]
TargetDir=c:\temp\写真
VisionApiUrl=https://app.ykr.ltd/vision/ocr
ReplaceRulesPath=ReplaceRules.dat
MoveDir={TargetDir}/{日付:4-2}/{Item1:0-6}/{Item1}/{FileName}

[Items]
Item1=(\d{3})\s?([A-Z0-9]{7,9})(-\d+)?=$1$2$3
Item2=塗装,組立前,組立,COATING\s?THICKNESS\s?TESTER=テスター,外面,内面,駆動部,組立完成
Item3=第\d層=$0
Item4=塗装,組立前,組立,膜厚,膜厚ｱｯﾌﾟ,素地調整,素地調整前,素地調整後,性能試験,分解前, 分解中
```

## 注意点
- `screen.jpg` は参考用のためリポジトリに含めない。
- OCR 置換ルールは `ReplaceRules.dat` を使用する（JSON Lines 形式）。
