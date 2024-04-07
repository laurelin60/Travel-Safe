import json
import asyncio
import mss
import numpy as np
from ultralytics import YOLO
from PIL import Image
import websockets
import time

print("Loading model...")
model = YOLO('../yolov8s.pt')  # load an official model
print("Model loaded!")

async def send_detections():
    uri = "ws://localhost:8080"  # WebSocket server URI
    async with websockets.connect(uri) as websocket:
        print('Connected to WebSocket server')

        t = time.time()
        avg = 0
        ct = 0
        ss_size = (640, 640)
        with mss.mss() as sct:  # sct is screenshot
            ss_monitor = sct.monitors[1]
            ss_left = ss_monitor["left"] + (ss_monitor["width"] - ss_size[0]) // 2
            top = ss_monitor["top"] + (ss_monitor["height"] - ss_size[1]) // 2
            ss_bbox = (ss_left, top, ss_left + ss_size[0], top + ss_size[1])

            print('Inside screenshot loop')

            image_path = 'dog.jpg'  # Hard-coded test image
            image = Image.open(image_path)
            screenshot_bgr = np.array(image)[:, :, :3]  # Convert the PIL image to a numpy array

            if screenshot_bgr is None or screenshot_bgr.size == 1:
                return

            raw_results = model(screenshot_bgr, imgsz=640, conf=0.55, verbose=False)  # predict on an image

            results = [{'class': r.names[int(r.boxes.cls[i].item())], 'conf': r.boxes.conf[i].item(),
                        'box': r.boxes.xywhn[i].tolist()}
                       for r in raw_results for i in range(len(r.boxes.cls))]

            if len(results) > 0:
                print('Results found: ', results)

                results_json = json.dumps(results)
                output = {"type": "message", "data": results_json}
                print(output)

                await websocket.send(json.dumps(output))

            else:
                print('No results')

            elapsed = (time.time() - t) * 1000
            avg = ((avg * ct) + elapsed) / (ct + 1)
            ct = ct + 1
            print("avg: " + str(avg))
            t = time.time()

if __name__ == "__main__":
    asyncio.run(send_detections())
    print("Done!")
