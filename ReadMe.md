# Unity3D - RxStateMachine
![](Images/Multiple.gif)


State machine makes managing states easy, it is widely used on games and apps.
However, there are not many state machines that designed with front-end in mind.

Often, integrating multiple states with UI transition can be a huge hassle and needs extra code to make simple transitions working with states.
RxStateMachine is desgined with "front end first" and "Reactive" in mind.

RxStatemachine is built upon[ thefuntastic's finite state machine](https://github.com/thefuntastic/Unity3d-Finite-State-Machine).<br/>
Basic structure is mostly same but the core logic is implemented with [Unirx](https://github.com/neuecc/UniRx) (Reactive Extension for Unity)
**[Download Unirx](https://assetstore.unity.com/packages/tools/unirx-reactive-extensions-for-unity-17276) first to use RxStateMachine.**

Thanks to [thefuntastic](https://github.com/thefuntastic) and [neuecc](https://github.com/neuecc) for amazing statemachine and unirx!

## Features

#### 3 Transition Modes
* Safe
* OverWrite
* Blend
#### Flexible
* Support multiple statemachine instances
* Support Transition as routine or single call
#### Responsive and Resilent
* Notifies transition progress
* State Transition can be canceled
* no co-routine used, more reliable functionality

## Basic Usage

An example project is included (Unity 2017.3) to show the State Machine in action.

### Single Script Setup
To use the state machine you need a few simple steps

##### 1. Include the RxStateMachine package

```C#
using Bebimbop.Utilities.StateMachine;

public class MyStateMachineClass : MonoBehaviour { }
```

##### 2. Define your states using an Enum 

```C#
public enum State
{
    Init, 
    Play
}
```
##### 3. Create a variable to store a reference to the State Machine 

```C#
public StateMachine<State> MyStateMachine;
```

##### 4. Get a valid state machine for your MonoBehaviour

```C#
MyStateMachine = StateMachine<States>.Initialize(this);
```

This is where all of the magic in the StateMachine happens: in the background it inspects your MonoBehaviour (`this`) and looks for any methods described by the convention shown below.

You can call this at any time, but generally `Awake()` is a safe choice. 

##### 5. You are now ready to manage state by simply calling `ChangeState()`
```C#
MyStateMachine.ChangeState(States.Init);
```

##### 6. State callbacks are defined by underscore convention ( `StateName_Method` )

```C#
//for single frame transition
private void One_Enter()
{
  Debug.Log("State One Entered");
}

//for transiton over time
//float t is between 0 ~ 1 (Start ~ end)
private void Two_Enter(float t)
{
  Debug.Log( "Entering State Two.. Progress "  + t);
}
//output
//Entering State.. Progress 0
//Entering State.. Progress 0.43975
//...
//Entering State.. Progress 0.9476
//Entering State.. Progress 1

//OnCancel Method gets called when transtion interrupted
private void Two_OnCancle()
{
    
}

void Play_Update()
{
	Debug.Log("Game Playing");
}

void Play_Exit()
{
	Debug.Log("Game Over");
}
```
Currently supported methods are:

- `Enter`
- `Exit`
- `FixedUpdate`
- `Update`
- `LateUpdate`
- `Finally`

It should be easy enough to extend the source to include other Unity Methods such as OnTriggerEnter, OnMouseDown etc

These methods can be private or public. The methods themselves are all optional, so you only need to provide the ones you actually intend on using. 

Couroutines are supported on Enter and Exit, simply return `IEnumerator`. This can be great way to accommodate animations. Note: `FixedUpdate`, `Update` and `LateUpdate` calls won't execute while an Enter or Exit routine is running.

Finally is a special method guaranteed to be called after a state has exited. This is a good place to perform any hygiene operations such as removing event listeners. Note: Finally does not support coroutines.

##### Transitions

There is simple support for managing asynchronous state changes with long enter or exit coroutines.

```C#
fsm.ChangeState(States.MyNextState, StateTransition.Safe);
```

The default is `StateTransition.Safe`. This will always allows the current state to finish both it's enter and exit functions before transitioning to any new states.

```C#
fsm.ChangeState(States.MyNextState, StateTransition.Overwrite);
```

`StateMahcine.Overwrite` will cancel any current transitions, and call the next state immediately. This means any code which has yet to run in enter and exit routines will be skipped. If you need to ensure you end with a particular configuration, the finally function will always be called:

```C#
void MyCurrentState_Finally()
{
    //Reset object to desired configuration
}
```

##### Dependencies

There are no dependencies, but if you're working with the source files, the tests rely on the UnityTestTools package. These are non-essential, only work in the editor, and can be deleted if you so choose. 

## Upgrade Notes - March 2016 - v3.0

Version 3 brings with it a substantial redesign of the library to overcome limitations plaguing the previous iteration (now supports multiple states machines per component, instant Enter & Exit calls, more robust initialization, etc). As such there is a now a more semantic class organisation with `StateMachine` & `StateMachineRunner`. 

It is recommend you delete the previous package before upgrading, **but this will break your code!** 

To do a complete upgrade you will need to rewrite initialization as per above. You will also need to replace missing `StateEngine` component references with `StateMachineRunner` in the Unity editor. If you want a workaround in order to do a gradual upgrade without breaking changes, you can change the namespace of the `StateMachine` and `StateMachineRunner` and you will be able to use it alongside v2 code until you feel confident enough to do a full upgrade.  

## Implementation and Shortcomings

This implementation uses reflection to automatically bind the state methods callbacks for each state. This saves you having to write endless boilerplate and generally makes life a lot more pleasant. But of course reflection is slow, so we try minimize this by only doing it once during the call to `Initialize`. 

For most objects this won't be a problem, but note that if you are spawning many objects during game play it might pay to make use of an object pool, and initialize objects on start up instead. (This is generally good practice anyway). 

##### Manual Initialization
In performance critical situations (e.g. thousands of instances) you can optimize initialization further but manually configuring the StateMachineRunner component. You will need to manually add this to a GameObject and then call:
```C#
StateMachines<States> fsm = GetComponent<StateMachineRunner>().Initialize<States>(componentReference);
```

##### Memory Allocation Free?
This is designed to target mobile, as such should be memory allocation free. However the same rules apply as with the rest of unity in regards to using `IEnumerator` and Coroutines.  

##### Windows Store Platforms
Due to differences in the Windows Store flavour of .Net, this is currently incompatible. More details available in this [issue](https://github.com/thefuntastic/Unity3d-Finite-State-Machine/issues/4).


## Exmaples




