import cv2

# Open camera (use the correct index for your device, typically 0 or 1)
cap = cv2.VideoCapture(-1)

if not cap.isOpened():
    print("Error: Could not open camera.")
    exit()

# Capture a frame
ret, frame = cap.read()

# Check if the frame was captured successfully
if not ret or frame is None:
    print("Error: Failed to capture image.")
    cap.release()
    exit()
