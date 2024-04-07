using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;

public class WSThingy : MonoBehaviour
{
    public string serverUrl;
    private WebSocket _ws;
    private string _clientId;

    public GameObject circle;

    private readonly Queue<string> _messageQueue = new();

    private void Start()
    {
        SetCircleColor(Color.white);
        Debug.Log("Connecting to " + serverUrl);

        _ws = new WebSocket(serverUrl);

        _ws.OnMessage += OnMessage;
        _ws.OnOpen += OnOpen;
        _ws.OnError += OnError;
        _ws.OnClose += OnClose;

        _ws.Connect();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        _ws?.Close();
    }

    private void Update()
    {
        while (_messageQueue.Count > 0)
        {
            ProcessMessage(_messageQueue.Dequeue());
        }
    }

    private void OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("WebSocket connection opened");

        // Set circle color to green when WebSocket is connected
        SetCircleColor(Color.green);
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        _messageQueue.Enqueue(e.Data);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Debug.LogError("WebSocket error: " + e.Message);

        // Set circle color to red on error
        SetCircleColor(Color.red);
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("WebSocket connection closed with reason: " + e.Reason);

        // Set circle color to red on close
        SetCircleColor(Color.red);
    }

    private void ProcessMessage(string data)
    {
        Debug.Log("WebSocket message received: " + data);
        // Set circle color to yellow temporarily when message is received

        //parse the message as a JSON object
        var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

        Debug.Log("Thingy: " + json);

        switch (json["type"])
        {
            case "yolo":
                Debug.Log("Received yolo");
                /**
                * class: string;
                * conf: number;
                * box: list[number] (len 4)
                */

                Debug.Log(json["data"]);
                JArray a = (JArray)json["data"];
                var b = a.ToObject(typeof(Dictionary<string, object>[]));
                Dictionary<string, object>[] c = (Dictionary<string, object>[])b;
                UIRenderer.detections = c;
                UIRenderer.lastDetectionTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                break;
        }

        // SocketTransferIndicator();
    }

    private void SocketTransferIndicator()
    {
        SetCircleColor(Color.yellow);
        Invoke(nameof(ResetCircleColor), 0.5f);
    }

    private void SetCircleColor(Color color)
    {
        if (circle == null)
            return;
        var component = circle.GetComponent<Renderer>();
        if (component != null)
        {
            component.material.color = color;
        }
    }

    private void ResetCircleColor()
    {
        SetCircleColor(_ws.IsAlive ? Color.green : Color.red);
    }

    // private void SendGameStart()
    // {
    //     Debug.Log("Sending gameStart message to server");
    //     var message = new Dictionary<string, string>
    //     {
    //         { "type", "startGame" }, { "id", _clientId }
    //     };
    //     var json = JsonConvert.SerializeObject(message);
    //     _ws.Send(json);
    //     SocketTransferIndicator();
    // }

    private void SendGameEnd()
    {
        var message = new Dictionary<string, string> { { "type", "gameEnd" }, { "id", _clientId } };
        var json = JsonConvert.SerializeObject(message);

        _ws.Send(json);
        SocketTransferIndicator();
    }

    private void SendNextLetter()
    {
        var message = new Dictionary<string, string>
        {
            { "type", "nextLetter" },
            { "id", _clientId }
        };
        var json = JsonConvert.SerializeObject(message);

        _ws.Send(json);
        SocketTransferIndicator();
    }
}