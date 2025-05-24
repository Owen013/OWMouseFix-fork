# Outer Wilds Mouse Fix

this mod fixes mouse input in Outer Wilds. i have no idea how this doesn't already exist, this game came out six years ago. yall have just been living like this?

### explanation:

vertical mouse input is read every game frame by the camera controller, and horizontal mouse input is read every physics frame (which != game frame) by the character controller directly for turning. by default physics updates at a fixed rate of 60Hz, so your horizontal mouse movement would not be smooth when playing on a high refresh-rate monitor unless you raised the physics rate via secretsettings.txt. it also calculated sensitivity differently from the camera controller, so increasing your game physics rate would make this sensitivity discrepancy worse.

both axes are handled by the camera controller as it should be, but only in ship free look. this mod makes it so that both axes are always handled by the camera controller, and every physics frame the camera removes its horizontal rotation and transfers it to the character controller.
