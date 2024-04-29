import socket
import sys
import time
import shutil
import random

HOST = "127.0.0.1"  # Standard loopback interface address (localhost)
PORT = int(sys.argv[1])  # Port to listen on (non-privileged ports are > 1023)
ISSENDER = sys.argv[2]

time.sleep(1)
print("Connected to EPN")

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    print("Opening socket on port", PORT)
    s.bind((HOST, PORT))
    s.listen()
    conn, addr = s.accept()
    with conn:
        print(f"Connected to HandSHAKE Simulator")
        if ISSENDER == "-s":
            hold = input()
            data = bytes("Encrpyting PAtestimage.png", 'ascii')
            conn.sendall(data)
            time.sleep(18.4)

            data = bytes("Sending contract to 5656", 'ascii')
            conn.sendall(data)
            shutil.copyfile("/mnt/c/Users/Nathaniel/HandSHAKE/Assets/Scripts/PAencrypt.png", "/mnt/c/Users/Nathaniel/HandSHAKE/Assets/Scripts/Sat2/PAencrypt.png")
            shutil.copyfile("/mnt/c/Users/Nathaniel/HandSHAKE/Assets/Scripts/PAencrypt.png", "/mnt/c/Users/Nathaniel/HandSHAKE/Assets/Scripts/Station-Ithaca/PAencrypt.png")
            
            shutil.copyfile("/mnt/c/Users/Nathaniel/HandSHAKE/Assets/Scripts/Sat1/PAtestimage.json", "/mnt/c/Users/Nathaniel/HandSHAKE/Assets/Scripts/Sat2/PAencrypt.json")
            shutil.copyfile("/mnt/c/Users/Nathaniel/HandSHAKE/Assets/Scripts/Sat1/PAtestimage.json", "/mnt/c/Users/Nathaniel/HandSHAKE/Assets/Scripts/Station-Ithaca/PAencrypt.json")

            hold = input()

        else:
            time.sleep(random.uniform(30.0, 32.9))
            data = bytes("Contract recieved", 'ascii')
            conn.sendall(data)
            time.sleep(0.2)
            data = bytes("Verified consensus routine", 'ascii')
            conn.sendall(data)

            hold = input()