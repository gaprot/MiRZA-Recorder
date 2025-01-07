# MiRZA-Recorder

## 概要

MiRZAでユーザーが体験している映像の録画機能を提供するUnityパッケージです｡  
物理カメラの映像とUnityシーン上のカメラの映像を合成して録画する機能をアプリ内に実装することができます｡

## 動作環境

- Unity 2022.3 LTS
- OpenXR Plugin 1.10.0
- ARFoundation 5.1.5

## インストール

1. MiRZA用のUnityプロジェクトセットアップ
   - [MiRZA開発者向けセットアップガイド](https://www.devices.nttqonoq.com/developer/doc/setup/setup-guide)の手順に従ってプロジェクトを設定してください。
2. 本パッケージのインストール
   `manifest.json`に以下を追加してください：
   ```json
   {
     "dependencies": {
       "com.upft.mr-recorder": "https://github.com/gaprot/MiRZA-Recorder.git?path=Assets/Upft/MRRecorder"
     }
   }
   ```
3. (オプション) サンプルのインポート
   - Unity Package Managerから本パッケージのサンプルをインポートします｡ 簡単なUIを含むPrefabが含まれています｡

## 使い方

> MiRZA用の基本的なプロジェクト､ シーンのセットアップが完了していることを前提とします｡  
> [Camera Access](https://www.devices.nttqonoq.com/developer/doc/features/camera-frame-access/) の設定も必要です｡

1. ARCameraManagerが有効であることを確認してください｡
2. サンプルのRecorder Prefabをシーンに配置します｡
3. `RecorderExample`コンポーネントの`SceneCamera`, `CameraManager`にそれぞれ撮影に使用するシーン上のカメラとARCameraManagerを参照させます｡
4. 実機で実行します｡

赤いボタンを押すと録画開始､ 緑のボタンを押すと録画停止されます｡  
録画した動画は､ /storage/emulated/0/Android/data/com.hoge.huga/files 内に保存されます｡  
adbコマンドを使って取り出すことができます｡

```
adb shell ls /storage/emulated/0/Android/data/com.hoge.huga/files
adb pull /storage/emulated/0/Android/data/com.hoge.huga/files/recorded.mp4
```

録画品質はLow, Medium, Highから選択することができ､ それぞれの解像度は以下の通りです｡

| 録画品質 | 解像度      |
| -------- | ----------- |
| Low      | 720 x 480   |
| Medium   | 1280 x 720  |
| High     | 1920 x 1080 |

## 注意事項

- リアルタイムにフレーム画像の合成･エンコードを行うため､ 負荷が大きいです｡ 利用するコンテンツに合わせて録画品質を調整してください｡
- 音声の録音には未対応です｡
