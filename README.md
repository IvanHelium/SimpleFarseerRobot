# SimpleFarseerRobot
Simple robot with sensors, UART, and control with keyboard and UART
Here are a few source codes showing how you can make a simple robot. 
The robot is equipped with a UART module and takes commands on the UART. 
After receiving the command, the control parameters are set via the motion start function. 
Then these parameters work in the update function until the time is up.
At the end of time, the robot's speed is set to zero.
Also, parameters such as the states of tactile sensors, the direction to the resource (modulo) 
and the direction sign (clockwise or counterclockwise), distance to the resource are sent to uart.
The sensor is created by cutting the texture first to the vertices, 
and then creating the figures from these vertices using the built-in functions in farseer engine.
The sensors are interrogated as follows:
1) we obtain a list of all contacts with surfaces
2) if there is a collision of one of the tactile sensors, 
expose 1 in the appropriate bit of the corresponding variable. I'll send it to uart
