using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

public class WebSocketClient : MonoBehaviour
{
    private WebSocket _ws;
    private string _clientId;

    public GameObject circle;

    private readonly Queue<string> _messageQueue = new();

    private void Start()
    {
        SetCircleColor(Color.white);
        const string url = "ws://localhost:8080";

        Debug.Log("Connecting to " + url);

        _ws = new WebSocket(url);

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
        try
        {
            Debug.Log("WebSocket message received: " + data);
            // Set circle color to yellow temporarily when message is received

            //parse the message as a JSON object
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

            switch (json["type"])
            {
                case "id":
                    _clientId = json["id"];
                    Debug.Log("Set Client ID: " + _clientId);
                    break;
                case "yolo":
                    Debug.Log("Received yolo");
                    Debug.Log(json["data"]);
                    break;
            }

            // SocketTransferIndicator();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to parse WebSocket message: " + ex.Message);
        }
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
