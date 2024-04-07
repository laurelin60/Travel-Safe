const WebSocket = require("ws");

const wss = new WebSocket.Server({ port: 8080 });
const clients = {};

wss.on("connection", (ws, req) => {
    console.log("Connection established with",req.socket.remoteAddress, ws._socket.address().port)
    const id = generateId();
    clients[id] = ws;

    ws.send(JSON.stringify({ type: "id", id }));

    ws.on("message", (message) => {
        console.log('Received message')
        message = JSON.parse(message);

        console.log(message)
        let senderId = "";

        if (message["data"]) {
            senderId = message["id"];
            sendToVr(senderId, {type: "yolo", data: message["data"]})
            console.log('sent to VR')
        }
    });

    ws.on("close", () => {
        delete clients[id];
    });
});

function generateId() {
    while (true) {
        const id = Math.random().toString(36).substring(7);
        if (!clients[id]) {
            return id;
        }
    }
}

function sendToVr(senderId, message) {
    for (const id in clients) {
        if (id !== senderId) {
            clients[id].send(JSON.stringify(message));
        }
    }
}

console.log("WebSocket server running on port 8080");