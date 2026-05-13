import gymnasium as gym
from gymnasium.envs.registration import register
import subprocess
from DQN import DQN
import torch
from ExperienceReplay import ReplayMemory
import itertools
import yaml
import random
from torch import nn
import os
import numpy as np
import time

device = "cuda" if torch.cuda.is_available() else "cpu"

register(id="sf-automation-v0", entry_point="SFEnv:SFEnv")

RUNS_DIR = "runs"
os.makedirs(RUNS_DIR, exist_ok=True)


class Agent:
    def __init__(self, hyperparameter_set):
        with open("hyperparameters.yaml", "r") as file:
            all_hyperparameter_sets = yaml.safe_load(file)
            hyperparameters = all_hyperparameter_sets[hyperparameter_set]

        self.hyperparameters_set = hyperparameter_set

        self.replay_memory_size = hyperparameters[
            "replay_memory_size"
        ]  # size of replay memory
        self.mini_batch_size = hyperparameters[
            "mini_batch_size"
        ]  # size of the training data set sampled from the replay memory
        self.epsilon_init = hyperparameters["epsilon_init"]  # 1 = 100% random actions
        self.epsilon_decay = hyperparameters["epsilon_decay"]  # epsilon decay rate
        self.epsilon_min = hyperparameters["epsilon_min"]  # minimum epsilon value
        self.learning_rate_a = hyperparameters["learning_rate_a"]
        self.discount_factor_g = hyperparameters["discount_factor_g"]
        self.network_sync_rate = hyperparameters["network_sync_rate"]

        self.MODEL_FILE = os.path.join(RUNS_DIR, f"{self.hyperparameters_set}.pt")

        self.loss_fn = nn.MSELoss()
        self.optimizer = None

    def run(self, is_training=True, useModel=False, render=False, player1=True):
        p = subprocess.Popen(["bash", "-c", "./launch.sh"])
        time.sleep(
            3
        )  # this is not an ideal solution and may need to be adjusted, but its the easiest
        env = gym.make("sf-automation-v0")
        env.metadata["player1"] = player1
        rewards_per_episode = []
        epsilon_history = []
        policy_net = DQN(env).to(device)

        if is_training:
            memory = ReplayMemory(self.replay_memory_size)
            epsilon = self.epsilon_init
            target_net = DQN(env).to(device)
            target_net.load_state_dict(policy_net.state_dict())

            step_count = 0

            self.optimizer = torch.optim.Adam(
                policy_net.parameters(), lr=self.learning_rate_a
            )
            best_reward = -9999999999
        elif is_training and useModel:
            memory = ReplayMemory(self.replay_memory_size)
            epsilon = self.epsilon_init
            target_net = DQN(env).to(device)
            target_net.load_state_dict(policy_net.state_dict())

            step_count = 0

            self.optimizer = torch.optim.Adam(
                policy_net.parameters(), lr=self.learning_rate_a
            )
            best_reward = -9999999999
            policy_net.load_state_dict(torch.load(self.MODEL_FILE))
        else:
            policy_net.load_state_dict(torch.load(self.MODEL_FILE))
            policy_net.eval()

        for episode in itertools.count():
            state, _ = env.reset()
            state = torch.tensor(state, dtype=torch.float, device=device)
            terminated = False
            episode_reward = 0.0
            while not terminated:
                if is_training and random.random() < epsilon:
                    action = env.action_space.sample()
                    action = torch.tensor(action, dtype=torch.int64, device=device)
                else:
                    with torch.no_grad():
                        action = policy_net(state.unsqueeze(dim=0)).squeeze().argmax()

                new_state, reward, terminated, _, _ = env.step(action.item())
                episode_reward += reward
                new_state = torch.tensor(new_state, dtype=torch.float, device=device)
                reward = torch.tensor(reward, dtype=torch.float, device=device)

                if is_training:
                    memory.append((state, action, new_state, reward, terminated))

                    step_count += 1
                state = new_state

            rewards_per_episode.append(episode_reward)
            if is_training:
                if episode_reward > best_reward:
                    print(f"Saved model with new best reward: {episode_reward: 0.1f}")
                    torch.save(policy_net.state_dict(), self.MODEL_FILE)
                    best_reward = episode_reward
                if len(memory) > self.mini_batch_size:
                    # Sample from the memory
                    mini_batch = memory.sample(self.mini_batch_size)
                    self.optimize(mini_batch, policy_net, target_net)

                    if episode % 10 == 0:
                        print(
                            f"Average previous 10 episodes reward at episode {episode}: {np.mean(rewards_per_episode)}"
                        )
                    epsilon = max(epsilon * self.epsilon_decay, self.epsilon_min)
                    epsilon_history.append(epsilon)

                    if step_count > self.network_sync_rate:
                        target_net.load_state_dict(policy_net.state_dict())
                        step_count = 0

        env.close()
        p.terminate

    def optimize(self, mini_batch, policy_net, target_net):
        # Transpose the list of experiences and separate each element
        states, actions, new_states, rewards, terminations = zip(*mini_batch)

        # Stack tensors to create batch tensors
        # tensor([[1,2,3]])
        states = torch.stack(states)

        actions = torch.stack(actions)

        new_states = torch.stack(new_states)

        rewards = torch.stack(rewards)
        terminations = torch.tensor(terminations).float().to(device)

        with torch.no_grad():
            # Calculate target Q values (expected returns)
            target_q = (
                rewards
                + (1 - terminations)
                * self.discount_factor_g
                * target_net(new_states).max(dim=1)[0]
            )

        # Calcuate Q values from current policy
        current_q = (
            policy_net(states)
            .gather(dim=1, index=actions.unsqueeze(dim=1).long())
            .squeeze()
        )

        loss = self.loss_fn(current_q, target_q)

        self.optimizer.zero_grad()
        loss.backward()
        self.optimizer.step()


if __name__ == "__main__":
    agent = Agent("sf2[1]")
    agent.run(is_training=True)
