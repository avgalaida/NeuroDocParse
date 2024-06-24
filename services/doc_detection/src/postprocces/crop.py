from PIL import Image
import os

def CropImage(image_path, xyxy):
    try:
        print("xyxy: ", xyxy)
        img = Image.open(image_path)
        cropped_img = img.crop(xyxy)
        cropped_img_path = f"/tmp/cropped_{os.path.basename(image_path)}"
        cropped_img.save(cropped_img_path)
        print(f"Cropped image saved at {cropped_img_path}")
        return cropped_img_path
    except Exception as e:
        print(f"Error during cropping: {e}")
        return None