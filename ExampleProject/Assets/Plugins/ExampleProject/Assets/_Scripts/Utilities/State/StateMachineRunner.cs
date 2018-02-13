using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Bebimbop.Utilities.StateMachine
{
/// <summary>
/// StateMachineRunner :
/// Runs current state's update, fixed update, late update loop
/// </summary>
   	public class StateMachineRunner : MonoBehaviour
	{
		public static List<IStateMachine> StateMachineList = new List<IStateMachine>();

		private void FixedUpdate()
		{
			for (int i = 0; i < StateMachineList.Count; i++)
			{
				var fsm = StateMachineList[i];
				if(fsm.CurrentStateMap == null || fsm.CurrentStateMap.GetComponent()==null)return;
				if (!fsm.IsInTransition && fsm.CurrentStateMap.GetComponent().enabled)
				{
					fsm.CurrentStateMap.FixedUpdate();
				}
			}
		}
		private void Update()
		{
			for (int i = 0; i < StateMachineList.Count; i++)
			{
				var fsm = StateMachineList[i];
				if(fsm.CurrentStateMap == null || fsm.CurrentStateMap.GetComponent()==null)return;
				if (!fsm.IsInTransition && fsm.CurrentStateMap.GetComponent().enabled)
				{
					fsm.CurrentStateMap.Update();
				}
			}
		}
		private void LateUpdate()
		{
			for (int i = 0; i < StateMachineList.Count; i++)
			{
				var fsm = StateMachineList[i];
				if(fsm.CurrentStateMap == null || fsm.CurrentStateMap.GetComponent()==null)return;
				if (!fsm.IsInTransition && fsm.CurrentStateMap.GetComponent().enabled)
				{
					fsm.CurrentStateMap.LateUpdate();
				}
			}
		}
		private void OnDisable()
		{
			StateMachineList.Clear();
		}
	}

	/// <summary>
	/// StateMapping : holds enter,exit, update, fixedupdate, lateupdate delegates
	/// </summary>
	public class StateMapping
	{
		public object GetState() { return _state; }
		private readonly object _state;

		public MonoBehaviour GetComponent() { return _component; }
		private MonoBehaviour _component;
		
		public void SetComponent(MonoBehaviour c) { _component = c; }
		private readonly ReactiveProperty<StateMapping> _currentState;
		
		public StateMapping(object state, ReactiveProperty<StateMapping> callbackState)
		{
			_state = state;
			_currentState = callbackState;
		}
		
		public BoolReactiveProperty IsInTransition = new BoolReactiveProperty();	
		public UniRx.IObservable<float> CreateNewRoutine(float duration, bool isEnter)
		{
			if (isEnter)
			{
				_enterInQue = false;
				return 
				RoutineRunner(duration)
					.DoOnCancel(() =>
					{
						 OnCancel.Invoke();
						_enterInQue = true;
					})
					.DoOnSubscribe(()=>
					{
						IsInTransition.Value = true;
						_currentState.Value = this;
						
					})
					.Do(f =>{ if (duration > 0)_enterRoutine(f); })
					.DoOnCompleted(() =>
					{
						if (duration == 0)_enterCall();
						_enterInQue = true;
						IsInTransition.Value = false;
						
					});
			}
			_exitInQue = false;
			return 
				RoutineRunner(duration)
					.DoOnCancel(() =>
					{
						OnCancel.Invoke();
						_exitInQue = true;
					})
					.DoOnSubscribe(()=>IsInTransition.Value = true)
					.Do(f => { if (duration > 0) _exitRoutine(f); })
					.DoOnCompleted(() =>
					{
						if (duration == 0)_exitCall();
						_exitInQue = true;
						IsInTransition.Value = false;
					});
		}
#region Enter properties
		public bool HasEnterRoutine;
		private bool _enterInQue = true;
		public bool EnterInQue { get { return _enterInQue; } }
		private Action<float> _enterRoutine = DefaultRoutine;
		private Action _enterCall = DefaultVoid;

		public void SetEnterRoutine(Action<float> callback)
		{
		
			HasEnterRoutine = true;
			_enterRoutine = callback;
		}	
		public void SetEnterCall( Action callback)
		{
			HasEnterRoutine = false;
			_enterCall = callback;
		}

		private static void DefaultRoutine(float t) { }
		private static void DefaultVoid() { }
		#endregion
#region Exit properties
		
		public bool HasExitRoutine;
		public bool ExitInQue {get { return _exitInQue; }}
		private bool _exitInQue = true;
		
		private Action<float> _exitRoutine = DefaultRoutine;
		private Action _exitCall = DefaultVoid;
		public void SetExitRoutine(Action<float> callback)
		{
			HasExitRoutine = true;
			_exitRoutine = callback;
		}

		public void SetExitCall(Action callback)
		{
			HasExitRoutine = false;
			_exitCall = callback;
		}

#endregion
#region Finally, Update, LateUpdate, FixedUpdate properties
		public Action Update = DefaultVoid;
		public Action LateUpdate = DefaultVoid;
		public Action FixedUpdate = DefaultVoid;
		public Action OnCancel = DefaultVoid;
		public void SetCancel (Action callback) { OnCancel = callback; }
		public void SetUpdate (Action callback) { Update = callback; }
		public void SetLateUpdate (Action callback) { LateUpdate = callback; }
		public void SetFixedUpdate (Action callback) { FixedUpdate= callback; }
		private static UniRx.IObservable<float> RoutineRunner(float due)
		{
			return
				UniRx.Observable.Create<float>(obs =>
				{
					var timer = 0f;
					obs.OnNext(0);
					var observable = due > 0 ?
						UniRx.Observable
							.EveryUpdate()
							.Select(_ => timer += Time.deltaTime)
							.TakeWhile(x => x < due)
							.Select(x => x / due)
							.DoOnError(obs.OnError)
							.DoOnCompleted(()=>
							{
								obs.OnNext(1f);
								obs.OnCompleted();
							})
							.Subscribe(obs.OnNext)
						:
						UniRx.Observable.Empty<float>()
							.DoOnError(obs.OnError)
							.DoOnCompleted(() =>
							{
								obs.OnNext(1f);
								obs.OnCompleted();
							}).Subscribe(obs.OnNext);

					return 
						UniRx.Disposable.Create(() =>
						{
							observable.Dispose();
						});
				});
		}
#endregion
	}
}