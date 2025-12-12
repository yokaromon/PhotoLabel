# PhotoLabel 移植方針（WinForms版）

## 目的
既存の ASP.NET Core Razor Pages アプリ「PictureSaver」（`Y:\Projects\job\int\aoyagi\source\PictureSaver`）の主要機能をデスクトップ向け WinForms クライアントに移植し、画像の閲覧・整理・OCR・リネーム/移動をローカルで完結できるようにする。

## 参照するソース機能
- 3ペインUI: ディレクトリツリー／画像一覧（サムネ・メタ情報）／プレビュー＋OCR。
- ファイル操作: リネーム（テンプレート/重複回避）、移動（テンプレート展開＋自動ディレクトリ作成）、ごみ箱移動、Explorer起動、ZIP相当（デスクトップでは任意）。
- 入力エリア駆動の命名: Configの入力定義からコントロール生成、プレースホルダー展開でファイル名/移動先を生成。
- OCR: Vision API（またはプロキシ）呼び出し、前処理、キャッシュ、置換ルール適用、結果から入力欄自動補完。
- アップロード/ドラッグ&ドロップ: デスクトップではファイルコピーとして実装。
- 設定駆動: Root/Trashパス、FileName/MoveDirectoryテンプレート、InputArea定義。

## フェーズ分割
1) **基盤・設定**
   - `appsettings.json` 相当を WinForms 設定に移植（Root/Trash パス、テンプレート、InputArea）。
   - 設定ロード/検証とエラーハンドリングを実装。
   - DirectoryItem/Input 定義モデルを用意。

2) **ディレクトリツリー＆画像一覧**
   - Root からの遅延ロードツリー（隠し除外）。
   - 画像拡張子フィルタ（HEIC/HEIF含む）、EXIF DateTaken優先ソート。
   - サムネ生成・キャッシュ（`%Temp%/PhotoLabel/Thumbnails`）。
   - 複数選択（Shift/Ctrl）と選択状態表示。

3) **プレビュー＆基本操作**
   - プレビューとメタ情報（パス・サイズ・日時）。
   - Explorerで選択を開く。
   - ごみ箱移動（重複時は連番）。
   - ZIP保存相当を検討（任意）。

4) **入力エリア＆命名**
   - Config定義から TextBox/ComboBox/Editable ComboBox（履歴付き）を生成。
   - 入力値から自動ファイル名生成＋手動上書き可。
   - リネーム（複数は連番＋重複回避）、移動（プレースホルダ展開＋自動作成）。
   - プレースホルダ: `{RootDirectory}`, `{FileName}`, `{yyyyMMdd}`, `{<Title>}`, `{Title:start-length}`。

5) **OCR＆置換ルール**
   - OCRサービス連携（APIキー or プロキシ）、前処理、進捗UI。
   - ハッシュ+サイズキーのメモリ/ディスクキャッシュ（24h）を実装。
   - 置換ルール（SQLite）を順序適用。デスクトップ用CRUDダイアログ。
   - OCR結果から入力欄を正規表現/履歴で自動補完。

6) **ドラッグ&ドロップ／インポートUX**
   - 画像一覧へのドラッグ&ドロップで現在ディレクトリへコピー（重複回避）。
   - ツリー/一覧のリフレッシュと空ディレクトリ掃除。

7) **仕上げ・堅牢化**
   - スプリッタ幅などUI状態の永続化。
   - ログ/例外ハンドリング、OCRキャンセル、プログレス表示。
   - 大量ファイル/ロック/長パス/HEICなどの動作確認。

## 注意点
- 文字化けしている `appsettings.json` の日本語ラベルは移植時に正しいUTF-8へ修正。
- `screen.jpg` はリポジトリに含めない（参考イメージのみ）。
- ソース参照: `PictureSaver` の Services（DirectoryService, ImageService, GoogleCloudVisionService, OcrCacheService, ReplaceService, OcrService）と `wwwroot/js` のUIロジック、`Pages/Index`/`ReplaceRules` の動作。***
