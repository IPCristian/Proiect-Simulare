# Proiect-Simulare

1. RL via MLAgents

In the incipient training stage, the model is trained with the Agents class on HeuristicOnly mode and tested on InferenceOnly. Additionally, the ball and target were fixed and no labyrinth was employed, while having only one instance. The vanilla heuristic simply added a reward when the ball was touched and penalized the model when a wall was hit. The training process was fast, though the ball rarely hit the target with an accuracy of around 5-10%. Due to lack of computational power, only 3 epochs were given and the learning rate was kept at 1e-4 for all types of configurations for experimental integrity

The major upgrade was adding 25 training instances, so that the model learns much quicker. No major hyper-parameter optimization was performed, in order to maintain a coherent review of the approaches. In this scenario, the training time was much higher, though the testing accuracy was at least 50-60% with a small standard deviation. 

A labyrinth is created with prefab walls to increase the difficulty of the task, while also randomizing the initial positions of the ball and target. The previous model is initially tested with this configuration, but yielded small accuracies of 10-15%. The significant improvement was changing the reward function as follows.
  
  - A reward_coefficient is initialized at 2.0f and penalize_coefficient at 1.0f. They both continously modify the scale of different events to not overfit the model. It was hypothesised that an important heuristic should keep track if the ball reached the target faster than previous iterations. Thus, the previous distance traveled is fed to the next iteration and when the model achieved a faster speed during an instance, the reward coefficient gets a higher value, while also raising the penalize coefficient when any wall is hit. This determines the model to learn the fastest path to the target and minimize the training time. An intricate addition to the reward function was also considering the perfect duration of the ball reaching the target. In order to find the initial optimal time, the distance between the spawning points of both the target and the ball will by divided to the generic movement speed of the ball, determining the optimal time. This is determined at the start of each episode. The reason why th e optimal time is also considered is that spawning the ball near the target will automatically cause a record duration. Thus, by also considering the optimal time to the target, the reward function can simply be determined by the error between the current duration and optimal duration, while multiplying with the reward coefficient to avoid overfitting, as stated above and finally incrementing it by 0.5f. Furthermore, the model is penalized when not achieving a better time, but the penalize coefficient is not increased. This is done so that the model is still partially rewarded for reaching the target, but also to mitigate overfitting as well. Analogous, the same reward function but negated is employed when the ball hits a wall, while also increasing the penalize coefficient. 

Overall, this reward function displayed an accuracy of around 70% when walls are in place, whilst reaching a record accuracy of over 90-95% without walls. An encountered issue is that it was attempted to fine-tune the previous model on the walls configuration, but due to lack of computational power, the model simply could not train. It is evident that in better circumstances, even for the difficult wall environment and randomized positions, the model will achieve near 95% accuracy. The loss functions are congruent with the results and are minimized when the new reward function is applied. Still, the reward function cannot be used to its full capacity which is unfortunate, since fine-tuning will not work even for 1 epoch and the function is designed for at least 10 epochs.

2. UI implementation 

We implemented a user interface (UI) component that aims to present relevant information and options in an organized and easy-to-use manner. This part of the interface has an introductory menu that introduces the composition of the team and then gives the user more options to customize the game experience. Through this menu, the user can randomize the start of players (AI agents), randomize the objective, disable the maze, change the player's speed and the minimum distance between the player and the objective. The arguments are essentially allowing the user to adjust the difficulty of the environment.

Once all the settings have been configured to the user's preferences, he can turn on the system to observe the movements of the trained model. During viewing, the user has the possibility to change the perspective of the observation camera. In top-down mode, the camera presents a top view of the map, allowing the user to observe player and objective movements in a simplified and clear way. Instead, the user can switch to a rotating camera, which provides a wide perspective of the entire map, allowing detailed observation of the movements and spatial relationships between players and the objective.

3. Post-processing with shaders and special effects

The effects are global and consist of the following:
  - chromatic aberration
  - color adjustment with high exposure
  - bloom with a green tint
The effects are enabled once an agent reaches its goal and are shown for 3 seconds

Shaders:
Two shader graphs have been created to achieve the visual effects on the goal and the walls. The first shader uses a fresnel effect combined with a time remap to simulate a glowing effect.

The second one uses a Voronoi node and a Radial Shear node. The result is then mixed with two different shades of blue to achieve the desired effect. Both shaders are parameterized.

This will illustrate a water-like effect on the walls in the arena and the advantages are two-fold. The visibility of the minutia in the ball's path are enhanced, but also created a pleasant aesthetic of the canvas.
