import cv2
import os,socket,sys,time
import xgolib
import spidev as SPI
import xgoscreen.LCD_2inch as LCD_2inch
from PIL import Image,ImageDraw,ImageFont
from key import Button
import numpy as np
from flask import Flask, request, jsonify, send_file
import threading

imagesPath = "/home/pi/ai-dog/images/"
firmWare = None
libVersion = None

# Initialize Flask app
app = Flask(__name__)

# Define a route for handling POST requests
@app.route('/forward', methods=['POST'])
def move_forward():
    # Get JSON data from the request
    data = request.get_json()
    if not data or 'distance' not in data:
        return jsonify({"error": "Invalid data"}), 400
    
    distance = data['distance']
    
    # Perform action (simulating movement)
    print(f"Moving forward by {distance} cm")
    xgo.move_x_by(distance=distance)
    # Respond with an acknowledgment
    return jsonify({"status": "OK"}), 200

@app.route('/turn', methods=['POST'])
def turn():
    # Get JSON data from the request
    data = request.get_json()
    if not data or 'theta' not in data:
        return jsonify({"error": "Invalid data"}), 400

    theta = data['theta']
    xgo.turn_by(theta=theta, mintime=0.5, vyaw=10)

    # Respond with an acknowledgment
    return jsonify({"status": "OK"}), 200

@app.route('/sight', methods=['GET'])
def sight():
    global captureImage
    captureImage = True

    snapShotPath = imagesPath + "lastSnapshot.jpg"

    while(captureImage): 
        time.sleep(1)

    if os.path.exists(snapShotPath):
        return send_file(snapShotPath)
    else:
        return jsonify({"error": "No snapshot found"}), 400  

@app.route('/status', methods=['GET'])
def status():
    global firmWare, libVersion
    if firmWare is None or libVersion is None:
        firmWare = xgo.read_firmware()
        libVersion = xgo.read_lib_version()

    osName = "xgo1030"
    battery=xgo.read_battery()
    return jsonify({"status": "OK", "battery_percentage": battery, "firmware": firmWare, "library_version": libVersion, "os_name": osName})

@app.route('/shutdown', methods=['GET'])
def shutdown():
    global run_program
    run_program = False
    return jsonify({"status": "OK"}), 200

# Function to start the Flask server
def start_http_server():
    app.run(host='0.0.0.0', port=8080, debug=False)

os.system('sudo chmod 777 /dev/ttyAMA0')
xgo = xgolib.XGO(port = '/dev/ttyAMA0',version='xgomini')

display = LCD_2inch.LCD_2inch()
display.clear()
splash = Image.new("RGB", (display.height, display.width ),"black")
display.ShowImage(splash)
button=Button()
font = cv2.FONT_HERSHEY_SIMPLEX 
cap=cv2.VideoCapture(0)
# cap.set(cv2.CAP_PROP_FRAME_WIDTH,1024)
# cap.set(cv2.CAP_PROP_FRAME_HEIGHT,768)
cap.set(cv2.CAP_PROP_FRAME_WIDTH,640)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT,480)
cap.set(cv2.CAP_PROP_BRIGHTNESS,75)
cap.set(cv2.CAP_PROP_CONTRAST,75)

# cap.set(3,640)
# cap.set(4,480)
# cap.set(3,320)
# cap.set(4,240)
if(not cap.isOpened()):
    print("[camera.py:cam]:can't open this camera")

# Start the HTTP server in a new thread
server_thread = threading.Thread(target=start_http_server)
server_thread.daemon = True  # Allow thread to exit when the main program exits
server_thread.start()

captureImage = False
start_time = time.time()
run_program = True

while(run_program):    
    ret, img = cap.read() 
    if not ret:
        print("Ignoring empty camera frame")
        continue

    # Check if captureImage is triggered
    if captureImage:
        cv2.imwrite(imagesPath + 'lastSnapshot.jpg', img)
        captureImage = False

    # Only resize the image every 2 seconds
    elapsed_time = time.time() - start_time
    if elapsed_time >= 2:

        img = cv2.resize(img, (320, 240))
        start_time = time.time()  # Reset the start time to measure another 2 seconds

        # Convert to grayscale for processing
        img_ROI_gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        
        # Adjust color channels
        b, g, r = cv2.split(img)
        img = cv2.merge((r, g, b))

        # Display image
        imgok = Image.fromarray(img)
        display.ShowImage(imgok)

    # Quit when 'q' is pressed
    if (cv2.waitKey(1)) == ord('q'):
        break

    # Button press actions
    if button.press_a():
        print("Button A")
        captureImage = True

    if button.press_b():
        print("Button B")
        captureImage = True

    if button.press_c():
        print("Button C")
        break

    if button.press_d():
        print("Button D")
        break

print("Stopping, releasing resources")

cap.release()
cv2.destroyAllWindows() 
display.clear()
splash = Image.new("RGB", (display.height, display.width ),"black")
display.ShowImage(splash)
xgo.reset()
sys.exit()
