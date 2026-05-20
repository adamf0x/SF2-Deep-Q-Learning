from gymnasium import Env
from gymnasium.spaces import Discrete
from gymnasium import spaces
from enum import Enum
import json
import asyncio
import threading
from SocketClient import SocketConnection
import numpy as np


class SFEnv(Env):
    metadata = {"render_modes": ["human"], "render_fps": 60, "player1": True}
    special_moves = {
        "LHADOUKEN",
        "LSHORYUKEN",
        "LTATSU",
        "MHADOUKEN",
        "MSHORYUKEN",
        "MTATSU",
        "HHADOUKEN",
        "HSHORYUKEN",
        "HTATSU",
    }

    def __init__(self, websocket_uri="ws://localhost:8080", render_mode=None):
        self.websocket_uri = websocket_uri
        self.socket = None
        self.current_obs = None
        self.action_sent = False
        self.obs_received = threading.Event()
        self.frames_elapsed = 0

        self.observation_space = spaces.Box(low=0.0, high=1.0, shape=(32,))

        self.action_space = Discrete(29)
        assert render_mode is None or render_mode in self.metadata["render_modes"]
        self.render_mode = render_mode

        self.loop = None
        self.loop_thread = None
        self.window = None
        self.clock = None

    def start_event_loop(self):
        def run_loop():
            self.loop = asyncio.new_event_loop()
            asyncio.set_event_loop(self.loop)
            self.loop.run_forever()

        self.loop_thread = threading.Thread(target=run_loop, daemon=True)
        self.loop_thread.start()
        while self.loop is None:
            threading.Event().wait(0.01)

    def run_async(self, coroutine, timeout=2):
        if self.loop is None or self.loop.is_closed():
            raise RuntimeError("Event loop not running")

        future = asyncio.run_coroutine_threadsafe(coroutine, self.loop)

        try:
            return future.result(timeout=timeout)
        except Exception:
            print("Async operation failed")
            raise

    def ensure_connected(self):
        if self.socket is None or not self.socket.is_connected:
            self.start_event_loop()

            # Create and connect socket
            socket_future = asyncio.run_coroutine_threadsafe(
                self.async_connect(), self.loop
            )
            try:
                self.socket = socket_future.result()
            except Exception as e:
                raise RuntimeError(f"Failed to connect: {e}")

    async def async_connect(self):
        socket = SocketConnection(self.websocket_uri)
        await socket.connect()

        # Start receiving observations
        socket.start_receiving(self.handle_obs)

        # Wait for first observation
        await asyncio.sleep(0.1)
        return socket

    def step(self, action):
        if self.socket is None or not self.socket.is_connected:
            self.ensure_connected()

        self.obs_received.clear()

        action_name = PlayerActions(action).name

        self.run_async(self.socket.send_perform_action_request(action_name))

        if not self.obs_received.wait(timeout=30.0):
            raise TimeoutError("Did not receive observation in step")

        if self.current_obs is None:
            reward = 0
            terminated = False

        p1_health = self.current_obs["p1Health"]
        p2_health = self.current_obs["p2Health"]
        p1_round_wins = self.current_obs["p1RoundWins"]
        p2_round_wins = self.current_obs["p2RoundWins"]
        terminated = False
        if self.metadata["player1"]:
            if p2_round_wins >= 2:
                terminated = True
            if p1_health < 255 and p2_health < 255:
                if p1_health > p2_health:
                    reward = 1
                else:
                    reward = -1
            else:
                reward = 0
        else:
            if p1_round_wins >= 2:
                terminated = True
            if p1_health < 255 and p2_health < 255:
                if p1_health < p2_health:
                    reward = 1
                else:
                    reward = -1
            else:
                reward = 0

        info = {}
        if self.render_mode == "human":
            self.render()

        self.frames_elapsed += 1

        return self.process_obs(self.current_obs), reward, terminated, False, info

    def render(self):
        p1_health = self.current_obs["p1Health"]
        p2_health = self.current_obs["p2Health"]
        print(f"P1 Health: {p1_health}, P2 Health: {p2_health}")

    def reset(self, seed=None, options=None):
        super().reset(seed=seed)
        if self.socket is None or not self.socket.is_connected:
            self.ensure_connected()

        self.obs_received.clear()

        self.run_async(self.socket.send_reset())

        if not self.obs_received.wait(timeout=60):
            raise TimeoutError("Did not receive initial observation")

        info = {}

        if self.render_mode == "human":
            self.render()
        return self.process_obs(self.current_obs), info

    async def handle_obs(self, message):
        try:
            self.current_obs = json.loads(message)
            self.obs_received.set()
        except Exception as e:
            print(f"Error parsing observation: {e}")

    def close(self):
        """Clean up resources"""
        if self.socket:
            self.run_async(self.socket.close())

        if self.loop and self.loop.is_running():
            self.loop.call_soon_threadsafe(self.loop.stop)

        if self.loop_thread and self.loop_thread.is_alive():
            self.loop_thread.join(timeout=2.0)

    def process_obs(self, obs_dict):
        return np.array(
            [
                obs_dict["p1Health"] / 177.0,
                obs_dict["p2Health"] / 177.0,
                obs_dict["p1ButtonHitBox"] / 13.0,
                obs_dict["p2ButtonHitBox"] / 13.0,
                obs_dict["p1Action"] / 6.0,
                obs_dict["p2Action"] / 6.0,
                obs_dict["p1MoveDirection"] / 25.0,
                obs_dict["p2MoveDirection"] / 25.0,
                obs_dict["p1MovementState"] / 3.0,
                obs_dict["p2MovementState"] / 3.0,
                float(obs_dict["p1InAir"]),
                float(obs_dict["p2InAir"]),
                obs_dict["p1AttackInfo"] / 4.0,
                obs_dict["p2AttackInfo"] / 4.0,
                float(obs_dict["p1IsCrouching"]),
                float(obs_dict["p2IsCrouching"]),
                float(obs_dict["p1IsAttacking"]),
                float(obs_dict["p2IsAttacking"]),
                obs_dict["p1DistanceFromEnemy"] / 201.0,
                obs_dict["p2DistanceFromEnemy"] / 201.0,
                float(obs_dict["p1FacingLeft"]),
                float(obs_dict["p2FacingLeft"]),
                obs_dict["p1FireballPosition"] / 255.0,
                obs_dict["p2FireballPosition"] / 255.0,
                (obs_dict["p1XPos"] - 50) / 300.0,
                (obs_dict["p2XPos"] - 50) / 300.0,
                (obs_dict["p1YPos"] - 124) / 68.0,
                (obs_dict["p2YPos"] - 124) / 68.0,
                (obs_dict["p1YVelocity"] + 7) / 14.0,
                (obs_dict["p2YVelocity"] + 7) / 14.0,
                obs_dict["p1RoundWins"] / 3.0,
                obs_dict["p2RoundWins"] / 3.0,
            ],
            dtype=np.float32,
        )

    def round_active(
        self, round_timer: int | None, p1_health: int, p2_health: int
    ) -> bool:
        return (
            round_timer > 0
            and round_timer <= 152
            and p1_health != 255
            and p2_health != 255
        )


class PlayerActions(Enum):
    NONE = 0
    UP = 1
    DOWN = 2
    FORWARD = 3
    BACKWARD = 4
    UPFORWARD = 5
    UPBACKWARD = 6
    DOWNFORWARD = 7
    DOWNBACKWARD = 8
    LPUNCH = 9
    MPUNCH = 10
    HPUNCH = 11
    LKICK = 12
    MKICK = 13
    HKICK = 14
    CRLPUNCH = 15
    CRMPUNCH = 16
    CRHPUCNH = 17
    CRLKICK = 18
    CRMKICK = 19
    CRHKICK = 20
    LHADOUKEN = 21
    LSHORYUKEN = 22
    LTATSU = 23
    MHADOUKEN = 24
    MSHORYUKEN = 25
    MTATSU = 26
    HHADOUKEN = 27
    HSHORYUKEN = 28
    HTATSU = 29
