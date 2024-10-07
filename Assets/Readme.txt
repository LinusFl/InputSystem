
A small gesture recognizer


Shake mouse interaction

The interaction is about moving the mouse swiftly left and right multiple times back and forth. For the interaction to be performed (per default) eight consecutive turns (called swerves) is required within 0.8s of each other.


My fork of InputSystem repository can be found at:
https://


The following files have the most relevant updates:

Assets\Tests\InputSystem\CoreTests_Actions_Interactions.cs:493          - One unit test added: Actions_CanPerformShakeInteraction()
Packages\com.unity.inputsystem\InputSystem\InputManager.cs:1969         - Shake interaction registered
Packages\com.unity.inputsystem\InputSystem\InputSystem.cs:3101          - Ugly patch to workaround troublesome exception

Assets\InputSystem_Actions.inputactions                                 - Added a Bounce action with Shake interaction
Assets\Readme.txt                                                       - This file
Assets\TestSmallGestureRecognizer.unity                                 - A scene to test the interaction
Assets\Scripts\Sphere.cs                                                - Script for game object Sphere that bounces on shake performed
Packages\com.unity.inputsystem\InputSystem\Actions\Interactions\ShakeMouseInteraction.cs    - Implementation of shake interaction

