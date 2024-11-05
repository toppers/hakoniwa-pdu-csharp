#if UNITY_WEBGL
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket;
using hakoniwa.environment.interfaces;
using System;
using hakoniwa.environment.impl;
using UnityEngine;

public class WebGLSocketCommunicationService : ICommunicationService, IDisposable
{
    private readonly string serverUri;
    private WebSocket webSocket;
    private ICommunicationBuffer buffer;
    private bool isServiceEnabled = false;


    public WebSocket GetWebSocket()
    {
        return webSocket;
    }
    public WebGLSocketCommunicationService(string serverUri)
    {
        this.serverUri = serverUri;
    }

    public bool IsServiceEnabled()
    {
        return isServiceEnabled;
    }

    async public Task<bool> SendData(string robotName, int channelId, byte[] pdu_data)
    {
        if (!isServiceEnabled || webSocket.State != WebSocketState.Open)
        {
            Debug.Log($"send error: isServiceEnabled={isServiceEnabled} webSocket.State ={webSocket.State }");
            return false;
        }

        IDataPacket packet = new DataPacket()
        {
            RobotName = robotName,
            ChannelId = channelId,
            BodyData = pdu_data
        };

        var data = packet.Encode();

        try
        {
            await webSocket.Send(data);
            return true;
        }
        catch (WebSocketException ex)
        {
            Debug.Log($"Failed to send data: {ex.Message}");
            return false;
        }
    }

    async public Task<bool> StartService(ICommunicationBuffer comm_buffer, string uri = null)
    {
        if (isServiceEnabled)
        {
            return false;
        }
        if (uri != null) {
            this.serverUri = uri;
        }
        buffer = comm_buffer;
        webSocket = new WebSocket(serverUri);

        webSocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        webSocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        webSocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        webSocket.OnMessage += (bytes) =>
        {
            //Debug.Log("OnMessage event triggered!");

            try
            {
                // 4バイトのヘッダーを読み取り、メッセージ全体の長さを取得
                if (bytes.Length < 4)
                {
                    Debug.LogWarning("Header is incomplete.");
                    return;
                }

                int totalLength = BitConverter.ToInt32(bytes, 0);
                //Debug.Log("Total Length: " + totalLength);

                // データの長さが一致する場合のみ処理を続ける
                if (bytes.Length == 4 + totalLength)
                {
                    //DumpDataBuffer(completeData);

                    IDataPacket packet = DataPacket.Decode(bytes);
                    buffer.PutPacket(packet);
                    //Debug.Log("Data received and processed.");
                }
                else
                {
                    Debug.LogWarning("Received data length mismatch.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Receive error: {ex.Message}");
            }
        };


        try
        {
            Debug.Log("Start Connect");
            await webSocket.Connect();
        }
        catch (WebSocketException ex)
        {
            Debug.Log($"WebSocket connection error: {ex.Message}");
            isServiceEnabled = false;
            return false;
        }
        catch (Exception ex)
        {
            Debug.Log($"Unexpected connection error: {ex.Message}");
            isServiceEnabled = false;
            return false;
        }

        isServiceEnabled = true;
        return true;
    }


    private void DumpDataBuffer(byte[] dataBuffer)
    {
        Debug.Log("Data Buffer Dump:");
        for (int i = 0; i < dataBuffer.Length; i++)
        {
            Debug.Log($"Byte {i}: {dataBuffer[i]:X2}");
        }
    }

    public bool StopService()
    {
        Debug.Log("Stop Service");
        if (!isServiceEnabled)
        {
            return false;
        }

        isServiceEnabled = false;

        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            webSocket.Close();
        }

        return true;
    }
    public void Dispose()
    {
        StopService();
        webSocket = null;
    }
    public string GetServerUri()
    {
        return serverUri;
    }
}
#endif
