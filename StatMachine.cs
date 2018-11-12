using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace RxDevKit.StateMachine
{
	public class StateMachine<Enum>: IObservable<Enum>
	{
		private readonly List<State<Enum>> _queue = new List<State<Enum>>();
		private AsyncMessageBroker _broker  = new AsyncMessageBroker();
		private bool _running = false;
		private Enum _nextState;
		//debug
		private Enum _current;
		//private	List<Enum> _history = new List<Enum>();
	
		public bool BlendMode = false;

		private readonly Subject<Enum> _currentState = new Subject<Enum>();
		private class State<Enum>
		{
			public Action Enter;
			public Action Exit;
			public Enum Estate;
			public readonly CompositeDisposable Disposable = new CompositeDisposable();
			public readonly BoolReactiveProperty HasEntered = new BoolReactiveProperty(false);
			public readonly BoolReactiveProperty HasExited = new BoolReactiveProperty(false);

		}
	
		public void ChangeState(Enum targetState)
		{
			if (_running)
			{
				Debug.Log("StateMachine: <color=red>Cannot change state while transitioning</color>");
				return;
			}
		
			_nextState = targetState;
			var state = new State<Enum>();
		
			state.Enter = () =>
			{
				_running = true;
				_broker.PublishAsync(new Tuple<Enum,bool>(targetState,true)).Subscribe(_ =>
				{
					_running = false;
					state.HasEntered.Value = true;
					_currentState.OnNext(state.Estate);
					
					_current = state.Estate;
					//_history.Add(state.Estate);

				}).AddTo(state.Disposable);
			};
			state.Exit = () =>
			{
				_running = true;
				_broker.PublishAsync(new Tuple<Enum,bool>(targetState,false)).Subscribe(_ =>
				{
					_running = false;
					state.HasExited.Value = true;

					if(!BlendMode)Next();
				}).AddTo(state.Disposable);
			};
			state.Estate = targetState;
			_queue.Add(state);
			Next();
		}

		private void OnCancel()
		{
			var state = new State<Enum>();
			state.Exit = () =>
			{
				_running = true;
				_broker.PublishAsync(new Tuple<Enum,bool>(_current,false)).Subscribe(_ =>
				{
					_running = false;
					state.HasExited.Value = true;

					if(!BlendMode)Next();
				}).AddTo(state.Disposable);
			};
			state.Estate = _current;
			state.HasEntered.Value = true;
			_queue.Add(state);
		}

		private void Next()
		{
			if(_queue.Count == 0 || _running) return;
			var targetState = _queue.First();
			if (BlendMode)
			{
				//blend mode : enter -> (exit,enter) ->
				if (targetState.HasEntered.Value == false)
				{
					targetState.Enter();
				}
				else
				{
					targetState.Exit();
					var nextTargetState = _queue[1];
					nextTargetState.Enter();
					nextTargetState.HasEntered.Where(b => b)
						.Subscribe(_ => _queue.RemoveAt(0));
				}
			}
			else
			{

				//regular mode : enter -> exit -> enter -> 
				var disposable  = new CompositeDisposable();
				if (targetState.HasEntered.Value == false)
				{

					targetState.Enter();
					if (!targetState.Estate.Equals(_nextState))
					{
						targetState.HasEntered
							.Where(b => b)
							.Subscribe(_=>Next()).AddTo(disposable);	
					}
				}
				else
				{
					if (_hasSubscribers)
					{
						targetState.Exit();
						targetState.HasExited
							.Where(b => b)
							.Subscribe(_=>
							{
								_queue.RemoveAt(0);
								disposable.Dispose();
							}).AddTo(disposable);	
					}
					else
					{
						_queue.RemoveAt(0);
						targetState.Exit();
						disposable.Dispose();

					}
					
				}
			}
		}

		public void Cancel()
		{
			_queue.ForEach(x =>x.Disposable.Dispose());
			_queue.Clear();
			_running = false;
			OnCancel();
		}

		private bool _hasSubscribers = false;
		public IDisposable OnEnter(Enum where, Func<IObservable<Unit>> asyncMessageReceiver)
		{
			_hasSubscribers = true;
			return _broker.Subscribe<Tuple<Enum, bool>>((sate) =>
			{
				if (sate.Item1.Equals(where) && sate.Item2)
				{
					return asyncMessageReceiver();    
				}
				else
				{
					return Observable.ReturnUnit();
				}
			});
		}
	
		public IDisposable OnExit(Enum where, Func<IObservable<Unit>> asyncMessageReceiver)
		{
			_hasSubscribers = true;
			return _broker.Subscribe<Tuple<Enum, bool>>((sate) =>
			{
				if (sate.Item1.Equals(where) && !sate.Item2)
				{
					return asyncMessageReceiver();    
				}
				else
				{
					return Observable.ReturnUnit();
				}
			});
		}

		public IDisposable Subscribe(IObserver<Enum> observer)
		{
			return _currentState.Subscribe(observer);
		}
	}
}