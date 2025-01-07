# hakoniwa-pdu-charp
A pure C# package implementing Hakoniwa PDU (Protocol Data Unit) for seamless communication in Hakoniwa simulations. Compatible with .NET and WebGL, it enables integration with AR, XR, and real robotic systems. Designed for flexibility without Unity dependencies, it provides a robust foundation for cross-layer communication.

# インストール方法

## Unityプロジェクトにインストールする場合

1. Unityプロジェクトを開きます。
2. `Packages/manifest.json` を開きます。
3. `"dependencies"` に以下の行を追加します。

```json
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.endel.nativewebsocket": "https://github.com/endel/NativeWebSocket.git#upm",
    "com.hakoniwa-lab.hakoniwa-pdu": "https://github.com/toppers/hakoniwa-pdu-csharp.git#main",
```


## 補足

WebGL向けには、以下をインストールする必要があります。

```json
"com.endel.nativewebsocket": "https://github.com/endel/NativeWebSocket.git#upm"
```
