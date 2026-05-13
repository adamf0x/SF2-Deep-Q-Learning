from torch import nn
import torch.nn.functional as F
import numpy as np


class DQN(nn.Module):
    def __init__(self, env, hidden1_dim=256, hidden2_dim=256):
        super(DQN, self).__init__()

        state_dim = int(np.prod(env.observation_space.shape))

        self.hidden1 = nn.Linear(state_dim, hidden1_dim)
        self.hidden2 = nn.Linear(hidden1_dim, hidden2_dim)
        self.output = nn.Linear(hidden2_dim, env.action_space.n)

    def forward(self, x):
        x1 = F.relu(self.hidden1(x))
        x2 = F.relu(self.hidden2(x1))
        return self.output(x2)
