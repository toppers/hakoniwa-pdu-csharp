# Migration Guide For Upstream Repositories

This document explains how the `hakoniwa-pdu-csharp` changes in `1.6.0` affect upstream repositories.

Main additions:

- compact PDU definition support
- packet v2 support

Relevant upstream repositories:

- `hakoniwa-sim-csharp`
  - <https://github.com/toppers/hakoniwa-sim-csharp>
- `hakoniwa-unity-drone`
  - <https://github.com/hakoniwalab/hakoniwa-unity-drone>

## Migration Matrix

| Item | Old | Recommended | Notes |
| --- | --- | --- | --- |
| PDU definition | legacy | compact | both supported |
| packet | v1 | v2 | default remains v1 |
| peer | `hakoniwa-webserver` | `hakoniwa-pdu-bridge-core` + `hakoniwa-pdu-endpoint` | v2 requires peer-side migration |
| declaration model | dynamic declare | declarative config | compatibility APIs remain |

## Packet Versions

### v1

Legacy compatibility packet format:

- 4 bytes: header length
- 4 bytes: robot name length
- variable: robot name
- 4 bytes: channel id
- remaining bytes: body

Assumptions:

- peer is `hakoniwa-webserver`
- dynamic `DeclarePduForRead` / `DeclarePduForWrite`

### v2

New standard packet format:

- 128 bytes: robot name area
- 176 bytes: fixed meta
- remaining bytes: body

Fixed meta includes:

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

Assumptions:

- peer is `hakoniwa-pdu-endpoint` / `hakoniwa-pdu-bridge-core`
- transfer model is declarative

Recommendation:

- keep v1 for backward compatibility
- prefer v2 for new integrations

## PDU Definition Formats

### legacy

Traditional `custom.json` style:

- `robots[].shm_pdu_readers`
- `robots[].shm_pdu_writers`

Good for:

- keeping old repositories unchanged
- quick regression verification

### compact

Newer format:

- `paths` points to shared `pdutypes`
- each robot uses `pdutypes_id`

Good for:

- shared PDU definition sets
- bridge / endpoint aligned setups
- new configurations

Recommendation:

- keep legacy for compatibility
- prefer compact for new definitions

## Where To Configure v2

In normal application code, packet version should not be selected per `DataPacket`.

Instead, configure it in the communication service initialization used by `PduManager`, typically through `comm_service_config.json`.

Example:

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

`comm_service_config.json` already existed before `1.6.0`.  
The new part is the `commRawVersion` field.

## Peer Selection

### For v1

Use the traditional `hakoniwa-webserver`-based setup.

References:

- <https://github.com/toppers/hakoniwa-webserver>
- <https://github.com/toppers/hakoniwa-webserver/blob/main/config/custom.json>

### For v2

Use the bridge/endpoint-based setup.

References:

- <https://github.com/hakoniwalab/hakoniwa-pdu-endpoint>
- <https://github.com/hakoniwalab/hakoniwa-pdu-endpoint/blob/main/config/sample/comm/hakoniwa/new-pdudef.json>
- <https://github.com/hakoniwalab/hakoniwa-pdu-bridge-core>

This is not only a packet format change. It is also a change in integration model:

- v1: dynamic declare at runtime
- v2: declarative transfer configuration

## About `DeclarePduForRead` / `DeclarePduForWrite`

These APIs remain for backward compatibility.

However, in v2 + bridge-core setups:

- transferred PDUs are already declared in config files
- dynamic declare is usually unnecessary

So the practical guidance is:

- keep the APIs for existing applications
- avoid depending on them in new declarative integrations

## Upstream Impact

### `hakoniwa-sim-csharp`

Main dependency points:

- `PduManager`
- `GetChannelId`
- `GetPduSize`
- `CreatePdu`
- `WritePdu`
- `FlushPdu`
- `ReadPdu`
- `DeclarePduForRead`
- `DeclarePduForWrite`

Impact summary:

- compact support is beneficial and should be transparent
- packet v2 has little direct effect because this repository mainly uses shared memory transport
- `DeclarePduForRead/Write` is still needed there today

### `hakoniwa-unity-drone`

This repository depends on both:

1. `hakoniwa-sim-csharp`
2. `hakoniwa-pdu-csharp` directly

Impact summary:

- compact support should be mostly transparent
- packet v2 matters because WebSocket-based communication is used directly in some paths
- keeping default packet version as v1 preserves existing behavior

## Recommended Migration Order

1. Update `hakoniwa-pdu-csharp` while keeping backward compatibility
2. Verify upstream repositories with legacy + v1 unchanged
3. Move PDU definitions from legacy to compact
4. Move the peer side from `hakoniwa-webserver` to `hakoniwa-pdu-bridge-core` + `hakoniwa-pdu-endpoint`
5. Switch `commRawVersion` to `v2`
6. Reduce dependency on dynamic declare only after that
