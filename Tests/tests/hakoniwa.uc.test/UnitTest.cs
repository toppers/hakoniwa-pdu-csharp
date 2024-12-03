namespace hakoniwa.pdu.test;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using hakoniwa.environment.impl;
using hakoniwa.environment.interfaces;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.core;
using hakoniwa.pdu.msgs.geometry_msgs;

public class WebSocketCommunicationServiceTest
{

    private HttpListener? listener;

    public async Task StartWebSocketServer(CancellationToken cancellationToken)
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/echo/");
        listener.Start();
        Console.WriteLine("WebSocket Test Server started.");

        _ = Task.Run(async () =>
        {
            while (listener.IsListening && !cancellationToken.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    await EchoMessages(webSocketContext.WebSocket, cancellationToken);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }, cancellationToken);

        // サーバーがリッスン状態になるまでの待機を確認し、リスニング状態を出力
        while (!listener.IsListening)
        {
            await Task.Delay(100);  // 少しの遅延を繰り返し、リッスン状態を確認
        }
        Console.WriteLine("Server is now fully listening.");
    }

    private async Task EchoMessages(WebSocket webSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];
        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
            }
            else
            {
                Console.WriteLine($"Server received message: {result.Count}");
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Binary, true, cancellationToken);
            }
        }
    }

    public void StopWebSocketServer()
    {
        listener?.Stop();
        listener?.Close();
        Console.WriteLine("WebSocket Server stopped.");
    }
}

public class UnitTest
{
    private static readonly string testDir = "test_data";

    [Fact]
    public async Task Test_Twist()
    {
        var testServer = new WebSocketCommunicationServiceTest();
        using var cts = new CancellationTokenSource();
        await testServer.StartWebSocketServer(cts.Token);

        string robotName = "DroneTransporter";
        string pduName = "drone_pos";
        IEnvironmentService service = EnvironmentServiceFactory.Create("websocket_dotnet", "local", testDir);
        IPduManager mgr = new PduManager(service, testDir);
        await mgr.StartService();

        // PduManagerの作成テスト
        Assert.NotNull(mgr);

        /*
         * Create Test.
         */
        IPdu pdu = mgr.CreatePdu(robotName, pduName);
        Assert.NotNull(pdu);

        Twist twist = new Twist(pdu);
        //double x_val = pdu.GetData<IPdu>("linear").GetData<double>("x");
        double x_val = twist.linear.x;
        Assert.Equal(0, x_val);

        //double z_val = pdu.GetData<IPdu>("angular").GetData<double>("z");
        double z_val = twist.angular.z;
        Assert.Equal(0, z_val);

        /*
         * Write Test.
         */
        //pdu.GetData<IPdu>("linear").SetData<double>("x", 1.0);
        //pdu.GetData<IPdu>("angular").SetData<double>("z", -1.0);
        twist.linear.x = 1.0;
        twist.angular.z = -1.0;
        var key = mgr.WritePdu(robotName, pdu);

        await mgr.FlushPdu(robotName, pduName);

        IPdu tmp = mgr.ReadPdu(robotName, pduName);
        Assert.Null(tmp);

        await Task.Delay(500);

        /*
         * Read Test.
         */
        IPdu rpdu = mgr.ReadPdu(robotName, pduName);
        Assert.NotNull(rpdu);

        Twist rtwist = new Twist(rpdu);
        //double r_x_val = rpdu.GetData<IPdu>("linear").GetData<double>("x");
        double r_x_val = rtwist.linear.x;
        Assert.Equal(1.0, r_x_val);

        //double r_z_val = rpdu.GetData<IPdu>("angular").GetData<double>("z");
        double r_z_val = rtwist.angular.z;
        Assert.Equal(-1.0, r_z_val);

        rpdu = mgr.ReadPdu(robotName, pduName);
        Assert.Null(rpdu);

        mgr.StopService();
        testServer.StopWebSocketServer();
    }
}
