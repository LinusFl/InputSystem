
A small gesture recognizer


Shake mouse interaction

The interaction is about moving the mouse swiftly left and right multiple times back and forth. For the interaction to be performed (per default) eight consecutive turns (called swerves) is required within 0.8s of each other.


My fork of InputSystem repository can be found at:
https://github.com/LinusFl/InputSystem


The following files have the most relevant updates:

Assets\Tests\InputSystem\CoreTests_Actions_Interactions.cs:493          - One unit test added: Actions_CanPerformShakeInteraction()
Packages\com.unity.inputsystem\InputSystem\InputManager.cs:1969         - Shake interaction registered
Packages\com.unity.inputsystem\InputSystem\InputSystem.cs:3101          - Temporary patch to workaround troublesome exception

Assets\InputSystem_Actions.inputactions                                 - Added a Bounce action with Shake interaction
Assets\Readme.txt                                                       - This file
Assets\TestSmallGestureRecognizer.unity                                 - A scene to test the interaction
Assets\Scripts\Sphere.cs                                                - Script for game object Sphere that bounces on shake performed
Packages\com.unity.inputsystem\InputSystem\Actions\Interactions\ShakeMouseInteraction.cs    - Implementation of shake interaction



I approached the problem by first reading up on the references given to get a better understanding. Then aimed to see something very simple work the full circle for a game object. I used MultiTapInteraction as a basis for my shake as it seemed to have a fairly similar functionality. It took a bit longer than expected to get the full circle up so after a while I switched to see some results in a unit test instead, but I went quite quickly back to the full circle approach since an understanding for what the real data looks like was so valuable. After I reached the full circle, and a clearer picture, I used the unit test to iron out the real implementation of the interaction.


I had some hurdles on the way, for example:
* I started out with an action map for my game object, but after a while I realized a project global one would be better. In the switch I first missed to change map for the game object and then also the Fire action was replaced with an Attack one. That caused me some confusion.
* For action triggering I wanted to use event triggering in Player Input but it did not work for me so I switched to do it on update instead.
* I tried to get the performed signal mapped to a button in another action (use an action as input to another), but eventually that did not seem possible.
* From the start I, for some strange reason, used the DebugView tool to look at log output. That worked well for logs in game object start but not for those in update. This also caused me confusion. The normal unity console works fine of course, I should have used from the start...
* The biggest hurdle was an exception that suddenly started to appear ("Attempted to set property InputSystem.actions during Play-mode which is not supported. Assigning this property is only allowed in Edit-mode."). Eventually I got rid of it after first switching to InputSystem version 7.0 (which caused errors) and then back to 1.11.2. Later on I got the problem back and instead patched InputSystem.cs (see file list above).


If I had more time to work on this I would:
* Create more unit tests
* Shake interaction code could be made clearer and with less duplication
* The interaction functionality needs tuning, for example detecting "large" mouse movements is very simplistic (essentially any change is now large)
* It seems nicer to use context.ControlIsActuated for detecting the movement, but I guess that requires some custom control or similar for this interaction. Is it easy to make one?


Linus Flygar
