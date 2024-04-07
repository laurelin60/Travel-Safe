import glob
import os
import shutil
from pathlib import Path


def coco_og_to_coco():
    labels_to_keep = {39: 'bottle', 43: 'knife', 76: 'scissors', 46: 'banana'}

    for dataset in ['train', 'val']:
        for images in glob.glob(f"./coco_og/{dataset}2017/*.jpg"):
            img_path = Path(images)
            label_path = Path(f"./coco_og/labels/{dataset}2017/{img_path.stem}.txt")

            if not label_path.exists():
                os.remove(img_path)
                continue

            with open(label_path, 'r') as f:
                lines = f.readlines()

            if not lines or len(lines[0]) < 3:
                os.remove(img_path)
                os.remove(label_path)
                continue

            labels = [int(l.split(" ")[0]) for l in lines]
            if not any(l in labels_to_keep.keys() for l in labels):
                continue

            labels = [l for l in labels if l in labels_to_keep.keys()]

            label_name = labels_to_keep[labels[0]]
            new_img_path = Path(f"./coco/{dataset}/{label_name}_{img_path.stem}.jpg")
            new_label_path = Path(f"./coco/labels/{dataset}/{label_name}_{img_path.stem}.txt")

            # copy the image and label to the new directory
            shutil.copy(img_path, new_img_path)
            with open(new_label_path, 'w') as f:
                filtered_lines = [l for l in lines if int(l.split(" ")[0]) in labels_to_keep]
                f.writelines(filtered_lines)


BASE_DIR = './dataset_mid'


def coco_to_pruned(num):
    labels = ['bottle', 'knife', 'scissors', 'durian', 'banana']
    datasets = ['train', 'val']

    for dataset in datasets:
        for label in labels:
            images = glob.glob(f"./coco/{dataset}/{label}*.jpg")
            images_to_keep = set(images[:num])

            for img in images_to_keep:
                img_path = Path(img)
                label_path = Path(f"./coco/labels/{dataset}/{img_path.stem}.txt")

                if not label_path.exists():
                    raise Exception

                new_img_path = Path(f"{BASE_DIR}/{dataset}/images/{img_path.name}")
                new_label_path = Path(f"{BASE_DIR}/{dataset}/labels/{img_path.stem}.txt")

                shutil.copy(img_path, new_img_path)
                shutil.copy(label_path, new_label_path)


def relabel():
    relabels = {0: 0, 39: 1, 43: 2, 76: 3, 46: 4}

    for directory in ['train', 'val']:
        label_files = glob.glob(f"./{BASE_DIR}/{directory}/labels/*.txt")

        for f in label_files:
            with open(f, 'r') as file:
                lines = file.readlines()
                lines = [l for l in lines if l.strip()]

            with open(f, 'w') as file:
                for line in lines:
                    label = int(line.split(" ")[0])
                    file.write(f"{relabels[label]} {' '.join(line.split(' ')[1:])}")


if __name__ == '__main__':
    # coco_og_to_coco()
    coco_to_pruned(300)
    relabel()
