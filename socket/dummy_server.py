import socket
import struct
import json

def receive_all(sock, n):
    """Helper function to ensure n bytes are read."""
    data = bytearray()
    while len(data) < n:
        packet = sock.recv(n - len(data))
        if not packet:
            return None
        data.extend(packet)
    return data

def main():
    server_ip = '0.0.0.0'
    server_port = 38496

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
        server_socket.bind((server_ip, server_port))
        server_socket.listen(1)
        print(f'Listening for incoming connections on {server_ip}:{server_port}...')

        client_socket, client_address = server_socket.accept()
        with client_socket:
            print(f'Connection established with {client_address}')

            while True:
                # First, read the length of the JSON message (4 bytes, big endian)
                length_data = receive_all(client_socket, 4)
                if not length_data:
                    print('Client disconnected.')
                    break

                # Unpack the length
                (length,) = struct.unpack('>I', length_data)
                print(f'Message length: {length}')

                # Then, based on the length, read the JSON message
                message_data = receive_all(client_socket, length)
                if not message_data:
                    print('Client disconnected.')
                    break

                # Decode the JSON message
                message = message_data.decode('utf-8')
                data = json.loads(message)
                print(f'Received data: {data}')

if __name__ == '__main__':
    main()