import json
import socket
import struct
import time
import mss
import numpy as np
from ultralytics import YOLO
from websocket import create_connection
from PIL import Image

print("Loading model...")
model = YOLO('yolov8n-tsa-best3.pt') # load an official model
print("Model loaded!")
    

if __name__ == "__main__":
    print("Connecting to ws...")
    ws = create_connection("ws://localhost:8080")
    print('Connected to ws')

    t = time.time()
    avg = 0
    ct = 0
    last_ss = None
    ss_size = (1600, 1600)
    with mss.mss() as sct: # sct is screenshot
        # Get the dimensions of the primary monitor
        ss_monitor = sct.monitors[1]
        ss_left = ss_monitor["left"] + (ss_monitor["width"] - ss_size[0]) // 2
        top = ss_monitor["top"] + (ss_monitor["height"] - ss_size[1]) // 2
        ss_bbox = (ss_left, top, ss_left + ss_size[0], top + ss_size[1])

        while True:
            print('Inside screenshot loop')

            # image_path = 'dog2.jpg'  # Hard-coded test image
            # image = Image.open(image_path)
            # screenshot_bgr = np.array(image)[:, :, :3]  # Convert the PIL image to a numpy array
            screenshot_bgr = np.array(sct.grab(ss_bbox))[:, :, :3]  # Convert the PIL image to a numpy array

            # screenshot_bgr = np.array(sct.grab(ss_bbox))[:, :, :3]
            if screenshot_bgr is None:
                continue
            if screenshot_bgr.size == 1: # catches shit (kevin)
                continue

            raw_results = model(screenshot_bgr, imgsz = 640, conf = 0.05, verbose = False, half=True)  # predict on an image

            results = [
                {'class': r.names[int(r.boxes.cls[i].item())], 'conf': r.boxes.conf[i].item(),
                    'box': r.boxes.xywhn[i].tolist()}
                for r in raw_results for i in range(len(r.boxes.cls))]

            if len(results) > 0:
                print('Results found: ', results)

                results_json = json.dumps(results)

                ws.send(json.dumps({'data': results}))
            else:
                print('No results')

            elapsed = (time.time() - t) * 1000
            # print("elapsed: " + str(elapsed))
            avg = ((avg * ct) + elapsed) / (ct + 1)
            ct = ct + 1
            print("avg: " + str(avg))
            t = time.time()

    print("Done!")