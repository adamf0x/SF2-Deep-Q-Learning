# SF2 Deep Q-Learning

Implementation of the Deep Q-Learning algorithm trained on the Street Fighter 2 Turbo arcade mode. 

## Running

If you want to train your own model, first you will need to download the [Bizhawk Emulator](https://tasvideos.org/Bizhawk). You will also need a Street Fighter 2 Turbo (USA) ROM, I won't tell you where exactly to acquire this but it should be relatively easy to find after a quick Google search. Do note that a different version of Street Fighter 2 will result in the model not training correctly as the memory addresses will be incorrect. ROM version correctness can be verified using BizHawk's ram search feature. Guides for how to do this should be easy to find online or in BizHawk's documentation.

This [guide](https://github.com/TASEmulators/BizHawk-ExternalTools/wiki/Development-quickstart) can be followed to set up the C# script to compile to a dll that BizHawk can use for sending and receiving observations and actions to the reinforcement learning environment.

Install the dependencies for the project and run `Agent.py` to launch BizHawk with the external tool and begin training. The timeout after the BizHawk process is started in `Agent.py` may need to be adjusted so that the socket connection doesnt timeout before the process has started.

## License

[MIT](https://choosealicense.com/licenses/mit/)