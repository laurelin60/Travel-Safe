import json
import time
import mss
import numpy as np
from ultralytics import YOLO
from PIL import Image
import websocket

def on_message(ws, message):
    print(f"Received message: {message}")

def on_error(ws, error):
    print('error found :(')
    print(error)

def on_close(ws, x, y):
    print("Closing connection!")

def on_open(ws):
    print("Loading model...")
    model = YOLO('../yolov8s.pt')  # load an official model
    print("Model loaded!")

    t = time.time()
    avg = 0
    ct = 0

    ss_size = (640, 640)
    with mss.mss() as sct:  # sct is screenshot
        # Get the dimensions of the primary monitor
        ss_monitor = sct.monitors[1]
        ss_left = ss_monitor["left"] + (ss_monitor["width"] - ss_size[0]) // 2
        top = ss_monitor["top"] + (ss_monitor["height"] - ss_size[1]) // 2
        ss_bbox = (ss_left, top, ss_left + ss_size[0], top + ss_size[1])

        print('Inside screenshot loop')

        image_path = 'dog.jpg'  # Hard-coded test image
        image = Image.open(image_path)
        screenshot_bgr = np.array(image)[:, :, :3]  # Convert the PIL image to a numpy array

        if screenshot_bgr is None or screenshot_bgr.size == 1:
            print("Invalid image. Exiting...")
            ws.close()
            return

        raw_results = model(screenshot_bgr, imgsz=640, conf=0.55, verbose=False)  # predict on an image

        results = [
            {'class': r.names[int(r.boxes.cls[i].item())], 'conf': r.boxes.conf[i].item(),
             'box': r.boxes.xywhn[i].tolist()}
            for r in raw_results for i in range(len(r.boxes.cls))]

        if len(results) > 0:
            print('Results found: ', results)

            results_dict = {
                "type": "message",
                "data": results
            }

            results_json = json.dumps(results_dict)
            print(results_json)

            if ws is not None and ws.sock is not None and ws.sock.connected:
                ws.send(results_json)
            else:
                print("WebSocket connection is not open.")
        else:
            print('No results')

        elapsed = (time.time() - t) * 1000
        avg = ((avg * ct) + elapsed) / (ct + 1)
        ct += 1
        print("avg: " + str(avg))

    ws.close()
    print("Done!")

if __name__ == "__main__":
    # websocket.enableTrace(True)
    ws = websocket.WebSocketApp("ws://localhost:8080",
                                on_message=on_message,
                                on_error=on_error,
                                on_close=on_close,
                                on_open=on_open)
    ws.run_forever()
