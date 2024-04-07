from pathlib import Path

import torch
from ultralytics import YOLO


def train():  # Load a model
    device = torch.device('cuda') if torch.cuda.is_available() else torch.device('cpu')
    model = YOLO('yolov8n.pt').cuda(device)
    model.train(data=r'C:\Users\awang\PycharmProjects\pythonProject3\dataset_mid3\data.yaml', epochs=40, imgsz=640,
                device=0, save=True)


def test():
    device = torch.device('cuda') if torch.cuda.is_available() else torch.device('cpu')
    model = YOLO('yolov8s-tsa-best1.pt').cuda(device)
    results = model.predict(r'./img.png', device=device)
    results[0].show()


def image_to_bboxes(image_path: str | Path) -> list:
    device = torch.device('cuda') if torch.cuda.is_available() else torch.device('cpu')
    model = YOLO(r'C:\Users\awang\PycharmProjects\pythonProject3\runs\detect\train5\weights\last.pt').cuda(device)

    results = model.predict(image_path, device=device, conf=0.20)
    return results


if __name__ == '__main__':
    # train()
    image_to_bboxes(r'./img/durian3.jpg')[0].show()
