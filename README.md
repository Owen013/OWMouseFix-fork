![image](https://github.com/user-attachments/assets/a3af1d3f-f576-417b-89b4-7e0db5e77dc1)

this mod fixes mouse input in Outer Wilds.

i have no idea why this didn't already exist, this game came out six years ago. yall have just been living like this?

### explanation:

normally, handling of the X & Y mouse axes is split between the camera & character controllers respectively unless you're in ship free look, in which case the camera controller handles both axes. the physics rate is 60Hz by default, so your mouse movement would not be smooth when playing on a high refresh-rate monitor unless you raised the physics rate via secretsettings.txt. horizontal sensitivity was also calculated differently, making it worse.

this mod makes it so:
* the camera controller handles mouse input every frame instead of every physics tick
* the camera controller now always handles X mouse input even when not in ship free look
* the camera controller now clamps pitch to nearly -90 ⇔ 90, instead of -80 ⇔ 80
* every physics tick, the camera removes its horizontal rotation and transfers it to the character controller
* mouse input smoothing is now disabled
