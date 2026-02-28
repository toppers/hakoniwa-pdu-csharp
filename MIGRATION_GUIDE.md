# 上位リポジトリ向け移行ガイド

この文書は、`hakoniwa-pdu-csharp` の以下の変更を、上位リポジトリへどう波及させるかを整理するためのものです。

- compact 形式の PDU 定義ファイル対応
- 通信パケット v2 対応

対象は主に以下です。

- `hakoniwa-sim-csharp`
  - <https://github.com/toppers/hakoniwa-sim-csharp>
- `hakoniwa-unity-drone/simulation`
  - <https://github.com/hakoniwalab/hakoniwa-unity-drone>

## 移行マトリクス

| 項目 | 従来 | 新規推奨 | 備考 |
| --- | --- | --- | --- |
| PDU 定義 | legacy | compact | 両対応 |
| packet | v1 | v2 | デフォルトは v1 |
| 接続先 | `hakoniwa-webserver` | `hakoniwa-pdu-bridge-core` + `hakoniwa-pdu-endpoint` | v2 は接続先切替が前提 |
| 宣言方法 | 動的 declare | 宣言型設定 | API は互換維持 |

## packet version の位置づけ

### v1

v1 は既存互換のための packet 形式です。

構造:

- 4 byte: header length
- 4 byte: robot name length
- 可変長: robot name
- 4 byte: channel id
- 残り: body

前提:

- `hakoniwa-webserver` 系との接続
- 動的な `DeclarePduForRead` / `DeclarePduForWrite`

### v2

v2 は新しい標準 packet 形式です。

構造:

- 128 byte: robot name 領域
- 176 byte: fixed meta
- 残り: body

fixed meta には以下を含みます。

- `magicno`
- `version`
- `flags`
- `meta_request_type`
- `total_len`
- `body_len`
- `hako_time_us`
- `asset_time_us`
- `real_time_us`
- `channel_id`

前提:

- `hakoniwa-pdu-endpoint`
- `hakoniwa-pdu-bridge-core`
- 宣言型の転送定義

### 推奨

- `v1` は非推奨ではあるが、既存互換のため維持する
- 新規構成では `v2` を推奨する
- `v2` を使う場合は接続先も bridge / endpoint 系へ合わせる

## PDU 定義形式の位置づけ

### legacy

legacy は従来の `custom.json` 形式です。

特徴:

- `robots[].shm_pdu_readers` / `shm_pdu_writers` を直接列挙する
- robot ごとに定義を持つ
- 既存資産との互換性が高い

向いているケース:

- 既存の上位リポジトリをそのまま動かしたい
- まずは最小変更で回帰確認したい

### compact

compact は新しい定義形式です。

特徴:

- `paths` で `pdutypes` ファイルを参照する
- robot ごとは `pdutypes_id` だけを持つ
- 同じ PDU 型セットを複数 robot で共有しやすい
- endpoint / bridge 系の定義モデルと整合しやすい

向いているケース:

- 新規定義を作る
- 複数 robot で共通の PDU セットを使う
- bridge / endpoint 系へ寄せていく

### 推奨

- legacy は後方互換の入力形式として維持する
- compact を新しい標準形式として扱う
- 上位リポジトリでは、まず legacy のまま回帰確認し、その後 compact へ切り替える

## 参考設定ファイル

### legacy / v1 側

- <https://github.com/toppers/hakoniwa-webserver/blob/main/config/custom.json>
- <https://github.com/toppers/hakoniwa-webserver/blob/main/config/twin-custom.json>
- <https://github.com/toppers/hakoniwa-webserver>

### compact / v2 側

- <https://github.com/hakoniwalab/hakoniwa-pdu-endpoint/blob/main/config/sample/comm/hakoniwa/new-pdudef.json>
- <https://github.com/hakoniwalab/hakoniwa-pdu-endpoint/blob/main/config/sample/comm/hakoniwa/new-pdutypes.json>
- <https://github.com/hakoniwalab/hakoniwa-pdu-endpoint>
- <https://github.com/hakoniwalab/hakoniwa-pdu-bridge-core>

## 結論

上位リポジトリ側で当面守るべき前提は、以下の 3 点です。

1. `PduManager` の既存 public API は維持する
2. `DeclarePduForRead` / `DeclarePduForWrite` は互換 API として維持する
3. 通信 packet version のデフォルトは `v1` のままにする

この条件を守る限り、既存コードは基本的に壊れません。

## 1. `hakoniwa-sim-csharp` への影響

### 依存している主な API

`hakoniwa-sim-csharp` は以下に依存しています。

- `PduManager`
- `GetChannelId`
- `GetPduSize`
- `CreatePdu`
- `WritePdu`
- `FlushPdu`
- `ReadPdu`
- `DeclarePduForRead`
- `DeclarePduForWrite`

参照箇所:

- `HakoAssetImpl.cs`
- `HakoAsset.cs`
- `HakoCommunicationService.cs`

### compact 対応の影響

良い影響です。

- `PduManager` の既存 API を変えずに compact を読めるようにしていれば、そのまま取り込めます
- `GetChannelId` と `GetPduSize` が維持されていれば、`DeclarePduForRead/Write` 側も動きます

### packet v2 対応の影響

基本的に影響しません。

理由:

- `hakoniwa-sim-csharp` は `UDPCommunicationService` / `WebSocketCommunicationService` を使っていません
- `HakoCommunicationService.cs` で共有メモリ経由の read / write をしています

### 注意点

`DeclarePduForRead/Write` はここではまだ必要です。  
将来 bridge/endpoint ベースの宣言型へ移行しても、sim-csharp 側には互換レイヤを残す前提で考えるべきです。

## 2. `hakoniwa-unity-drone/simulation` への影響

### 依存の種類

このプロジェクトは 2 系統で依存しています。

1. `hakoniwa-sim-csharp` 経由
2. `hakoniwa-pdu-csharp` 直接依存

参照箇所:

- `manifest.json`
- `ARBridge.cs`
- `WebServerBridge.cs`
- `DronePlayerDevice.cs`
- `DroneAvatarWeb.cs`
- `DefaultCameraController.cs`

### compact 対応の影響

良い影響です。

- `PduManager` の public API を変えていないため、読み込み対象の `custom.json` 相当を compact へ置き換えてもコード変更は最小で済みます

### packet v2 対応の影響

このプロジェクトには影響します。

理由:

- `ARBridge.cs` は `websocket_dotnet` を直接使っています
- `WebServerBridge.cs` も `websocket_dotnet` を直接使っています

ただし、`hakoniwa-pdu-csharp` 側で packet version のデフォルトを `v1` に維持しているため、既存設定のままでも従来動作は維持できます。

### 接続先の考え方

上位リポジトリ側では、接続先の整理も必要です。

- `v1` を使う場合:
  - 既存の `hakoniwa-webserver` 系接続を前提にできる
- `v2` を使う場合:
  - `hakoniwa-pdu-bridge-core` + `hakoniwa-pdu-endpoint` 構成を前提にする

これは packet 形式の差だけではなく、接続思想の差です。

- v1:
  - 実行時に `READ_FOR_xxx` / `WRITE_FOR_xxx` をやり取りする動的宣言型
- v2:
  - どの PDU をどう流すかを bridge / endpoint の設定ファイルで決める宣言型

そのため、`v2` にする時は「packet version を変える」だけでなく、「接続先と運用モデルも bridge / endpoint へ寄せる」と考えるのが正しいです。

### `comm_service_config.json` の移行

既存の設定はそのまま使えます。

例:

```json
{
  "udp": {
    "localPort": 54001,
    "remotePort": 54002,
    "remoteIPAddress": "127.0.0.1"
  },
  "WebSocket": {
    "ServerURI": "ws://localhost:8765"
  }
}
```

packet v2 を使う場合だけ、`commRawVersion` を追加します。

```json
{
  "udp": {
    "localPort": 54001,
    "remotePort": 54002,
    "remoteIPAddress": "127.0.0.1",
    "commRawVersion": "v2"
  },
  "WebSocket": {
    "ServerURI": "ws://localhost:8765",
    "commRawVersion": "v2"
  }
}
```

`commRawVersion` は packet 単位ではなく、`PduManager` が内部で使う通信サービスの初期化設定として指定します。  
通常は `EnvironmentServiceFactory.Create(...)` で読み込まれる `comm_service_config.json` に設定します。

### 注意点

`simulation` 内でも `DeclarePduForRead/Write` を広く使っています。  
この API を削除、または no-op 化すると影響が大きいです。

ただし、新しい bridge / endpoint 構成では、アプリケーション設計上この API は不要です。  
つまり、

- 既存コードではまだ必要
- 新規の v2 / bridge-core 構成では原則不要

という二重運用になります。

## 3. 推奨移行順

上位リポジトリに対しては、以下の順序を推奨します。

1. まずは `hakoniwa-pdu-csharp` を既存 API 互換のまま更新する
2. 上位リポジトリ側は packet version を `v1` のまま据え置いて回帰確認する
3. PDU 定義ファイルを legacy から compact へ切り替える
4. 接続先を `hakoniwa-webserver` 系から `hakoniwa-pdu-bridge-core` 系へ切り替える
5. bridge / endpoint 側が v2 で揃った段階で、`commRawVersion` を `v2` に切り替える
6. `DeclarePduForRead/Write` 依存の見直しは最後に行う

## 4. 上位リポジトリ向けチェックリスト

### `hakoniwa-sim-csharp`

- `PduManager` の生成コードがそのまま動くか
- `GetChannelId` / `GetPduSize` の解決結果が変わらないか
- `DeclarePduForRead/Write` の初期化がそのまま通るか

### `hakoniwa-unity-drone/simulation`

- `ARBridge` / `WebServerBridge` の起動がそのまま通るか
- `ReadPdu` / `WritePdu` / `FlushPdu` の通信が従来通り動くか
- `comm_service_config.json` 未変更時に従来通り接続できるか
- `commRawVersion: "v2"` 指定時に endpoint / bridge 側と接続できるか
- `v2` 利用時に接続先が `hakoniwa-webserver` のままになっていないか

## 5. 当面の運用方針

- compact は新しい標準形式として扱う
- legacy は後方互換の入力形式として維持する
- packet v2 は新しい標準形式として扱う
- packet v1 は既存上位リポジトリのために維持する
- `DeclarePduForRead/Write` は互換 API として維持する

## 6. bridge / endpoint 詳細

`v2` の設定詳細や bridge / endpoint の JSON 構成については、以下を参照してください。

- <https://github.com/toppers/hakoniwa-webserver>
- <https://github.com/hakoniwalab/hakoniwa-pdu-bridge-core>

ここで見るべきポイントは以下です。

- `hakoniwa-webserver` は v1 / 動的 declare 前提の従来構成
- `bridge.json` で何をいつ転送するかを宣言する
- `endpoint_container.json` で endpoint と transport を宣言する
- アプリケーション側で動的に read/write 宣言しなくても、bridge が転送を担う
