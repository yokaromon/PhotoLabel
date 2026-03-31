# PhotoLabel

画像フォルダを一覧表示し、OCR結果を使ってリネーム・移動・削除を行うWinFormsアプリケーション。

## 何をするものか
本プロジェクトは、所定のフォルダのファイル構成をデータベース化して、それをUIにより抽出する機能を提供する。

## 処理概要
Config.iniで対象フォルダとOCR設定を読み込み、ツリー/サムネイルを生成してユーザ操作に応じてローカルファイルを更新し、必要に応じてOCRを実行する。
- 起動: Program.Main → FrmMain_LoadでConfig.iniを読み込みTargetDirを確定し、ツリーとサムネイルを初期表示する。
- 起動: InitializeOcrServicesでVisionApiUrl/ReplaceRulesPathを読み込みOCRサービスを初期化する。
- ユーザ操作: サムネイル選択でRunOcrAsyncを実行し、OCR結果からItem1-4を補完してスナップショットを保存する。
- ユーザ操作: リネーム/移動/削除ボタンで選択ファイルを処理し、UIとプレビューを更新する。
- ドラッグ&ドロップ: フォルダツリー/サムネイル領域でコピー/移動を行い、必要に応じてツリーと一覧を再描画する。

## Overview
- Domain: 画像管理/ラベリング
- Architecture: WinForms単体アプリ + 外部OCR API
- Languages: C#, .NET

## Tech Stack
- Backend: ローカルファイル操作 + OCRサービス(HTTP)
- Frontend: Windows Forms (WinForms)
- DB: なし
- Auth: なし
- Infra: ローカルPC、外部OCR API

## Entry Points
- Backend: Program.cs
- Frontend: FrmMain.cs

## Top-Level Structure
```
Ocr/
Tools/
setup/
サンプル/
Config.ini
Program.cs
FrmMain.cs
FrmMain.Designer.cs
FrmPicture.cs
FrmPicture.Designer.cs
ThumbnailCard.cs
PhotoLabel.csproj
PhotoLabel.sln
ReplaceRules.dat
README.md
```

## Core Modules
- Config.ini: TargetDir/VisionApiUrl/MoveDir/Itemパターン設定
- ReplaceRules.dat: OCR置換ルール(JSON Lines)

## Backend Modules
- PhotoLabel.Ocr (Ocr): OCR実行、キャッシュ、置換ルール
- Tools (Tools): INI読み書きや共通ユーティリティ

## Frontend Components
- FrmMain (FrmMain.cs): page
- FrmPicture (FrmPicture.cs): page
- ThumbnailCard (ThumbnailCard.cs): component

## Source of Truth
- Business logic: FrmMain.cs
- API: Ocr/GoogleVisionClient.cs
- UI state: FrmMain.cs

## Generated
- Generated at: 02/09/2026 14:36:57
- Generator: analyze.bat v1.0

