# VRCBuildAutoAgree

**VRChat SDK の「Build & Publish」を押したときに表示される「Copyright ownership agreement」を、自動で同意して閉じる** Unity Editor 拡張です。  
Windows 専用。あらかじめ記録した **OK ボタンの位置**を、モーダルの出現を検知してから **左クリック**します。セットアップから実行まで、すべて Editor メニューだけで操作できます。

> ⚠️ 自動化はあくまで便利機能です。**権利を有するコンテンツのみ**をアップロードしてください。利用は**自己責任**でお願いします。

---

## 特徴

- **Build & Publish のクリックを自動検知**（UI Toolkit のルートでキャプチャ）
- **モーダル出現を検出**（タイトル／本文）してからクリック（最大待機 5 秒）
- クリック方式は **左クリック固定**（安定挙動）
- **3 秒カウントダウン**で OK 位置を簡単記録
- **オフセット未設定時はガイド表示**して安全に中断
- コンソール表示は**ユーザー向けログ**のみ：
  - 成功: `[VRCBuildAutoAgree] Done!`
  - 失敗: `[VRCBuildAutoAgree] Failed: <理由>`
  - 進行状況（カウントダウン、フック設置、検知、クリック座標など）も表示

---

## 動作環境

- **OS**: Windows 10 / 11  
- **Unity**: 2021.3 LTS / 2022.3 LTS 以降（UI Toolkit Editor）  
- **VRChat SDK**: Creator Companion で導入済みの SDK（Builder に「Build & Publish」があるもの）

> macOS は未対応（`user32.dll` による OS 入力送出を使用）。要望があれば別ブランチで検討します。

---

## インストール

### 1) `.unitypackage` で導入（推奨）
1. `VRCBuildAutoAgree-<version>.unitypackage` をダブルクリック（または _Assets → Import Package → Custom Package…_）。
2. `Assets/Editor/VRCBuildAutoAgree.cs` がインポートされていることを確認。

### 2) スクリプトで導入
1. プロジェクトに `Assets/Editor/` フォルダがなければ作成。
2. `VRCBuildAutoAgree.cs` を `Assets/Editor/` に配置。

---

## 使い方

1. **OK 位置の記録**  
   _Tools → VRCBuildAutoAgree → Set OK Offset (3s countdown)_  
   コンソールに `Capturing in 3...` と表示されたら **OK ボタンの位置にマウスを置く** → 自動保存。

2. **有効化**  
   _Tools → VRCBuildAutoAgree → Enable_

3. **Build & Publish を実行**  
   Builder のボタンを押すと、モーダルを検出後に自動クリック。  
   成功するとコンソールに **`[VRCBuildAutoAgree] Done!`** と出ます。

> 位置がズレたら、再度 **Set OK Offset** を実行（ウィンドウ配置や DPI 変更でズレます）。

---

## メニュー

- **Tools / VRCBuildAutoAgree / Enable** …… 有効化  
- **Tools / VRCBuildAutoAgree / Disable** …… 無効化  
- **Tools / VRCBuildAutoAgree / Set OK Offset (3s countdown)** …… 3 秒後のマウス位置を保存  
- **Tools / VRCBuildAutoAgree / Reset Offset** …… 記録位置をクリア  
- **Tools / VRCBuildAutoAgree / Test Click Now** …… その場で試しクリック

---

## トラブルシュート

- **Done! が出るのに押せていない**  
  - `Set OK Offset` を取り直し（DPI / レイアウト変更の影響を受けます）  
  - クリック前待機 `ClickDelayMs`（既定 60ms）を増やす（`VRCBuildAutoAgree.cs` 上部の定数）

- **Failed: OKボタンの位置が未設定です…**  
  - まずは _Set OK Offset (3s countdown)_ を実行してから Build してください。

- **VRChat SDK ウィンドウのタイトルが違う**  
  - コード内の `TARGET_WINDOW_TITLE` を実際のタイトルに合わせて変更してください。

---

## セキュリティ / プライバシー

- ネットワーク通信は行いません。  
- Windows のネイティブ API（`user32.SendInput`）で Editor ウィンドウにマウス入力を送出します。  
- データの収集は行いません。

---

## 免責事項（Disclaimer）

- 本ツールの利用により発生した **いかなる損害** についても、作者は一切の責任を負いません。  
- Unity / VRChat 等、各プラットフォームの **利用規約に従って** ご利用ください。  
- アップロードするコンテンツの **著作権・ライセンスの遵守** は利用者の責任です。

---

## ライセンス / 権利表記

Copyright (c) 2025 meme 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

---

## 変更履歴（テンプレ）

- **1.0.0**  
  - 初版：Build 検知 / モーダル検出 / 左クリック / 3s オフセット記録 / 未設定ガイド / 進行ログ

