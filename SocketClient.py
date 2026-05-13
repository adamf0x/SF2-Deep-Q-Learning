import websockets
import asyncio


class SocketConnection:
    def __init__(self, uri):
        print("Opening websocket connection")
        self.uri = uri
        self.ws = None
        self.is_connected = False
        self.receive_task = None
        self.current_obs = None

    async def send_reset(self):
        if self.ws and self.is_connected:
            return await self.ws.send("reset")
        return False

    async def send_perform_action_request(self, action):
        if self.ws and self.is_connected:
            return await self.ws.send(action)
        return False

    async def send_player(self, player1):
        if self.ws and self.is_connected:
            return await self.ws.send(player1)
        return False

    async def connect(self):
        self.ws = await websockets.connect(
            self.uri, max_size=2**23, max_queue=1000, ping_interval=None
        )
        self.is_connected = True
        print("Connected")

    async def receive_state(self, callback):
        while self.is_connected:
            try:
                stateStr = await asyncio.wait_for(self.ws.recv(), timeout=1.0)
                await callback(stateStr)

            except asyncio.TimeoutError:
                continue

            except websockets.exceptions.ConnectionClosed:
                print("Connection closed")
                self.is_connected = False
                break

            except Exception as e:
                print(f"client error: {e}")
                self.is_connected = False
                break

    def start_receiving(self, callback):
        self.receive_task = asyncio.create_task(self.receive_state(callback))

    async def close(self):
        self.is_connected = False
        if self.receive_task:
            self.receive_task.cancel()
        if self.ws:
            await self.ws.close()
            print("Connection closed")
