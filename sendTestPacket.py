#!/usr/bin/env python3

import socket
import binascii
import sys

UDP_IP = sys.argv[1]
UDP_PORT = 8160

payload = bytes.fromhex("003DCAFE0105000F33353230393330383634303336353508010000016B4F815B30010000000000000000000000000000000103021503010101425DBC000001")

print(f"connecting to {UDP_IP}:{UDP_PORT}")
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.connect((UDP_IP, UDP_PORT))
sock.send(payload)

res = sock.recv(1024)
print(binascii.hexlify(res, " "))
