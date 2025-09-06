VRCBuildAutoAgree  Readme (Windows) 
Version: 1.0.0
────────────────────────────────────────────────────────
■ 使い方（初回セットアップ）
────────────────────────────────────────────────────────
1) OK 位置の記録
   メニュー「Tools → VRCBuildAutoAgree → Set OK Offset (3s countdown)」
   コンソールに「Capturing in 3...」と出たら、OK ボタンの位置にマウスを置くと
   3 秒後に自動保存されます。
   → コンソールに「Offset saved: (x, y)」と表示されれば成功。

2) 有効化
   メニュー「Tools → VRCBuildAutoAgree → Enable」
   → コンソール「[VRCBuildAutoAgree] Enabled.」

3) 実行
   VRChat SDK の Builder で「Build & Publish」をクリック。
   本ツールがモーダル出現を検知すると、記録した座標へ自動で左クリックします。
   成功時：コンソールに「[VRCBuildAutoAgree] Done!」

補助:
・クリックを即テストしたい場合
  「Tools → VRCBuildAutoAgree → Test Click Now」
・位置を取り直す場合
  「Set OK Offset (3s countdown)」を再実行
・オフセットを消去する場合
  「Reset Offset」
・停止したい場合
  「Disable」

────────────────────────────────────────────────────────
■ メニュー一覧
────────────────────────────────────────────────────────
Tools / VRCBuildAutoAgree / Enable                    … 有効化
Tools / VRCBuildAutoAgree / Disable                   … 無効化
Tools / VRCBuildAutoAgree / Set OK Offset (3s countdown) … 3 秒後のマウス位置で OK 座標を記録
Tools / VRCBuildAutoAgree / Reset Offset              … 記録した座標を削除
Tools / VRCBuildAutoAgree / Test Click Now            … その場で試しクリック

────────────────────────────────────────────────────────

────────────────────────────────────────────────────────
■ アンインストール
────────────────────────────────────────────────────────
・プロジェクトから「Assets/Editor/VRCBuildAutoAgree.cs」を削除
・（任意）「Assets/VRCBuildAutoAgree」や生成した .unitypackage を削除
・EditorPrefs に保存したオフセットは、Reset Offset 実行または再インストール時に上書きされます
────────────────────────────────────────────────────────

────────────────────────────────────────────────────────
■ 免責事項（Disclaimer）
────────────────────────────────────────────────────────
・本ツールの利用によって生じた如何なる損害に対しても、作者は一切の責任を負いません
・Unity / VRChat 等、各プラットフォームの利用規約とガイドラインを遵守してください
・アップロードするコンテンツの著作権・ライセンス遵守は利用者の責任です
────────────────────────────────────────────────────────
■ 連絡先
────────────────────────────────────────────────────────
X: @memeVRcaht
