# hakoniwa-pdu-csharp

A pure C# package implementing Hakoniwa PDU (Protocol Data Unit) communication for Unity, .NET, and WebGL.

Japanese README: [README.md](./README.md)

## Installation

Add this package to `Packages/manifest.json`:

```json
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
"com.endel.nativewebsocket": "https://github.com/endel/NativeWebSocket.git#upm",
"com.hakoniwa-lab.hakoniwa-pdu": "https://github.com/toppers/hakoniwa-pdu-csharp.git#main"
```

For WebGL, `com.endel.nativewebsocket` is required.

## What Changed In 1.6.0

`1.6.0` keeps backward compatibility while adding support for the newer Hakoniwa integration model.

The two major additions are:

1. Compact PDU definition format
2. Packet v2 support

What this means for users:

- Existing code does not need immediate changes
- Legacy PDU definition files still work
- Packet v1 still works with existing setups
- Newer setups should prefer compact definitions and packet v2
- `DeclarePduForRead` / `DeclarePduForWrite` remain for compatibility, but are usually unnecessary in declarative bridge-based setups

In short, `1.6.0` is an extension release, not a forced migration release.

## API

Main APIs:

- [IPdu](Runtime/pdu/interfaces/IPdu.cs)
- [INamedPdu](Runtime/pdu/interfaces/INamedPdu.cs)
- [PDU message accessors](Runtime/pdu/msgs)
- [IPduManager](Runtime/pdu/interfaces/IPduManager.cs)

Typical send flow:

```csharp
INamedPdu npdu = pduManager.CreateNamedPdu("robotName", "position");
Twist pos = new Twist(npdu.Pdu);
pduManager.WriteNamedPdu(npdu);
await pduManager.FlushNamedPdu(npdu);
```

Typical receive flow:

```csharp
var pdu = pduManager.ReadPdu(robotName, pduName);
Twist twist = new Twist(pdu);
```

## Migration Summary

| Item | Old | Recommended | Notes |
| --- | --- | --- | --- |
| PDU definition | legacy | compact | both supported |
| packet | v1 | v2 | default is still v1 |
| peer | `hakoniwa-webserver` | `hakoniwa-pdu-bridge-core` + `hakoniwa-pdu-endpoint` | v2 assumes peer migration too |
| declaration model | dynamic declare | declarative config | APIs remain for compatibility |

Important points:

- Existing legacy / v1 setups can continue to run
- For packet v2, configure `commRawVersion` in `comm_service_config.json`
- In v2 + bridge-core setups, dynamic declare is usually not needed

Details:

- Japanese migration guide: [MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md)
- English migration guide: [MIGRATION_GUIDE_en.md](./MIGRATION_GUIDE_en.md)
