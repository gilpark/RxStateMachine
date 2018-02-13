using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UniRx;
using UnityEngine;
using Object = System.Object;
using LinkedObservable = M1.Utilities.Rx.LinkedObservable;

//todo subscribtion timing // for other scripts to subscribe state changes
namespace Bebimbop.Utilities.StateMachine
	{
		/// <summary>
		/// Transition Method
		/// Safe : wait till running routine is done
		/// Overwrite : cancle running routine then start new
		/// Blend : Run Exit, Enter routine at the same time
		/// </summary>
		public enum StateTransition
		{
			Safe,
			Overwrite,
			Blend
		}

		/// <summary>
		/// SateMachine interface
		/// </summary>
		public interface IStateMachine
		{
			StateMapping CurrentStateMap { get; }
			bool IsInTransition { get; }
		}

		/// <summary>
		/// Finite StateMachine
		/// </summary>
		/// <typeparam name="T">any Enums</typeparam>
		public class StateMachine<T> : IStateMachine where T : struct, IConvertible, IComparable
		{
			/// <summary>
			/// State Change event
			/// </summary>
			public event Action<object> Changed;
			private AsyncSubject<Unit> _initNotifyer = new AsyncSubject<Unit>();
			private static float _enterDuration = 0.5f;
			private static float _exitDuration = 0.5f;
			
	
			
			/// <summary>
			/// Add Subscriber to call when its enter, exit, updates happends
			/// </summary>
			/// <param name="component">The Subscriber</param>
			public void AddSubscriber(MonoBehaviour component)
			{
				_initNotifyer.Subscribe(_ =>
				{
					AddSubscriber_Internal(component);
				});
			}

			private Dictionary<object, StateMapping> _stateLookup;
			private void AddSubscriber_Internal(MonoBehaviour component)
			{
				var methods = component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly |
				                                             BindingFlags.Public |
				                                             BindingFlags.NonPublic);
				var separator = "_".ToCharArray();
				
				for (int i = 0; i < methods.Length; i++)
				{
					if (methods[i].GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Length != 0) { continue;}

					var names = methods[i].Name.Split(separator);
					
				
					if (names.Length <= 1) { continue; }

					Enum key;
					try { key = (Enum) Enum.Parse(typeof(T), names[0]); }
					catch (ArgumentException) { continue; }// methods that don't have enum name

					var targetState = _stateLookup[key];
					
					targetState.SetComponent(component);
				
					switch (names[1])
					{
						case "Enter":

							if (methods[i].GetParameters().Length > 0 && methods[i].GetParameters()[0].ParameterType == typeof(float))
							{
								targetState.SetEnterRoutine(CreateDelegate<Action<float>>(methods[i], component));
							}
							else
							{
								targetState.SetEnterCall(CreateDelegate<Action>(methods[i], component));
							}
							break;
						case "OnCancel":
							targetState.SetCancel(CreateDelegate<Action>(methods[i], component));
							break;
						case "Exit":
							if (methods[i].GetParameters().Length > 0 && methods[i].GetParameters()[0].ParameterType == typeof(float))
							{
								targetState.SetExitRoutine(CreateDelegate<Action<float>>(methods[i], component));
							}
							else
							{
								targetState.SetExitCall(CreateDelegate<Action>(methods[i], component));
							}
							break;		
						case "Update":
							targetState.SetUpdate(CreateDelegate<Action>(methods[i], component));
							break;
						case "LateUpdate":
							targetState.SetLateUpdate(CreateDelegate<Action>(methods[i], component));
							break;
						case "FixedUpdate":
							targetState.SetFixedUpdate(CreateDelegate<Action>(methods[i], component));
							break;
						
					}
				}

			}
			
			private V CreateDelegate<V>(MethodInfo method, Object target) where V : class
			{
				var ret = (Delegate.CreateDelegate(typeof(V), target, method) as V);

				if (ret == null)
				{
					throw new ArgumentException("Unabled to create delegate for method called " + method.Name);
				}
				return ret;
			}
			
			private readonly Queue<StateMapping> _tempStates = new Queue<StateMapping>();
			private MonoBehaviour _component;
			
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="component"> the subscriber </param>
			/// <exception cref="ArgumentException"> Enum provided to Initialize must have at least 1 visible definition </exception>
			public StateMachine(MonoBehaviour component)
			{
				//to track what is running state
				_stateStream.Subscribe(st =>
				{
					//StateStream.OnNext(st.GetState());
					if(Changed!=null)Changed.Invoke(st.GetState());
					if (_tempStates.Count < 1)
					{
						_currentState = st;
						_tempStates.Enqueue(_currentState);
					}
					else
					{
						_lastState = _tempStates.Dequeue();
						_currentState = st;
						_tempStates.Enqueue(_currentState);
					}
				}).AddTo(component);
				_component = component;

				//Define States
				var values = Enum.GetValues(typeof(T));
				if (values.Length < 1)
				{
					throw new ArgumentException("Enum provided to Initialize must have at least 1 visible definition");
				}
				_stateLookup = new Dictionary<object, StateMapping>();
				for (int i = 0; i < values.Length; i++)
				{
					var mapping = new StateMapping((Enum) values.GetValue(i), _stateStream);
					_stateLookup.Add(mapping.GetState(), mapping);
				}

				AddSubscriber_Internal(component);
				_initNotifyer.OnNext(Unit.Default);
				_initNotifyer.OnCompleted();
			}

			/// <summary>
			/// Change default duration
			/// </summary>
			/// <param name="enterDuration">the enter duration</param>
			/// <param name="exitDuratio">the exit duration</param>
			public void SetDuration(float enterDuration, float exitDuratio)
			{
				_enterDuration = enterDuration;
				_exitDuration = exitDuratio;
			}

			private StateTransition _transition = StateTransition.Safe;
			/// <summary>
			/// Change default duration
			/// </summary>
			/// <param name="enterDuration">the enter duration</param>
			/// <param name="exitDuratio">the exit duration</param>
			public void SetMode(StateTransition transition)
			{
				_transition = transition;
			}
			
			/// <summary>
			/// Change State
			/// </summary>
			/// <param name="newState">the target state</param>
			/// <param name="transition">the transition method</param>
			public void ChangeState(T newState)
			{
				ChangeState(newState, _transition, _enterDuration, _exitDuration);
			}
			
			/// <summary>
			/// Change State
			/// </summary>
			/// <param name="newState">the target state</param>
			/// <param name="transition">the transition method</param>
			public void ChangeState(T newState, StateTransition transition)
			{
				ChangeState(newState, transition, _enterDuration, _exitDuration);
			}
			
			/// <summary>
			///  Change State
			/// </summary>
			/// <param name="newState">the target state</param>
			/// <param name="transition">the transition method</param>
			/// <param name="enterDuration">the enter duration</param>
			/// <param name="exitDuration">the exit durtation</param>
			public void ChangeState(T newState, StateTransition transition, float enterDuration, float exitDuration)
			{
				_initNotifyer.DoOnCompleted(() =>
				{
					ChangeStateInternal(newState, transition, enterDuration, exitDuration);

				}).Subscribe();
			}
			
			private StateMapping _StateInQeue;
			private StateMapping _lastState;
			private StateMapping _currentState;
			private LinkedObservable _queHistory = new LinkedObservable();
			private ReactiveProperty<StateMapping> _stateStream = new ReactiveProperty<StateMapping>();
			/// <summary>
			/// StateChange stream
			/// </summary>
			public Subject<object> StateStream = new Subject<object>();
			private void ChangeStateInternal(T newState, StateTransition transition, float enterDuration, float exitDuration)
			{
				if (_stateLookup == null)
				{
					throw new Exception("States have not been configured, please call initialized before trying to set state");
				}

				if (!_stateLookup.ContainsKey(newState))
				{
					throw new Exception("No state with the name " + newState.ToString() +
					                    " can be found. Please make sure you are called the correct type the statemachine was initialized with");
				}

				var nextState = _stateLookup[newState];
				
				if (_currentState!=null && _currentState == nextState) return;

				if (transition.Equals(StateTransition.Safe))
				{
					if (_currentState == null) //for initial run 
					{
						_queHistory.Add(nextState.CreateNewRoutine(nextState.HasEnterRoutine ? enterDuration : 0f, true));
						_StateInQeue = nextState;
						return;
					}
				
					if(_currentState.ExitInQue) //if current one has exit routine waiting
					{
						_queHistory.Add(_currentState.CreateNewRoutine(_currentState.HasExitRoutine ? exitDuration : 0f, false));
						_queHistory.Add(nextState.CreateNewRoutine(nextState.HasEnterRoutine ? enterDuration : 0f, true));
						_StateInQeue = nextState;
					}
					else //already in current  exit routine
					{
						if (!_StateInQeue.GetState().Equals(nextState.GetState())) //if next state is not in qeue
						{
							_queHistory.Add(nextState.CreateNewRoutine(nextState.HasEnterRoutine ? enterDuration : 0f, true));
							_StateInQeue = nextState;
						}
					}
				}
				else if (transition.Equals(StateTransition.Overwrite))
				{
					if (_currentState == null) //for initial run 
					{
						_queHistory.Add(nextState.CreateNewRoutine(nextState.HasEnterRoutine ? enterDuration : 0f, true));
						_StateInQeue = nextState;
					}
					else
					{
						//todo test more on OnCancle.Invoke
						_StateInQeue.OnCancel.Invoke(); //invoke cancle incase previouse enter routine is done, which means call cancle to reset ui stuff b4 call next state enter
						_queHistory.Cancle();
						_queHistory.Add(nextState.CreateNewRoutine(nextState.HasEnterRoutine ? enterDuration : 0f, true));
						_StateInQeue = nextState;
					}
				}
				else //blend
				{
					if (_currentState == null) //for initial run 
					{
						_queHistory.Add(nextState.CreateNewRoutine(nextState.HasEnterRoutine ? enterDuration : 0f, true));
						_StateInQeue = nextState;
						return;
					}
					if(_currentState.ExitInQue) //if current one has exit routine, waiting
					{
						_queHistory.AddPair(_currentState.CreateNewRoutine(_currentState.HasExitRoutine ? exitDuration : 0f, false),
											nextState.CreateNewRoutine(nextState.HasEnterRoutine ? enterDuration : 0f, true));
						_StateInQeue = nextState;
					}
				}
			}
			/// <summary>
			/// the last state, can be null
			/// </summary>
			public T LastState
			{
				get
				{
					if (_lastState == null) return default(T);

					return (T) _lastState.GetState();
				}
			}
			/// <summary>
			/// the current state, can be null
			/// </summary>
			public T State
			{
				get
				{
					if (_currentState == null) return default(T);

					return (T) _currentState.GetState();
				}
			}

			public bool IsInTransition
			{
				get
				{
					if (_currentState == null) return false;
					
					return _currentState.IsInTransition.Value;
				}
			}
			public StateMapping CurrentStateMap
			{
				get { return _currentState; }
			}

			//Static Methods

			/// <summary>
			/// Inspects a MonoBehaviour for state methods as definied by the supplied Enum, 
			/// and returns a stateMachine instance used to trasition states.
			/// </summary>
			/// <param name="component">the Subscriber</param>
			/// <returns>StateMachine</returns>
			public static StateMachine<T> Initialize(MonoBehaviour component)
			{
				var engine = component.GetComponent<StateMachineRunner>();
				if (engine == null) component.gameObject.AddComponent<StateMachineRunner>();
				
				var fsm = new StateMachine<T>(component);
				StateMachineRunner.StateMachineList.Add(fsm);
				return fsm;

			}

			/// <summary>
			/// Inspects a MonoBehaviour for state methods as definied by the supplied Enum, 
			/// and returns a stateMachine instance used to trasition states. 
			/// </summary>
			public static StateMachine<T> Initialize(MonoBehaviour component, T startState)
			{
				var engine = component.GetComponent<StateMachineRunner>();
				if (engine == null) component.gameObject.AddComponent<StateMachineRunner>();

				var fsm = new StateMachine<T>(component);
				StateMachineRunner.StateMachineList.Add(fsm);
				fsm.ChangeState(startState);
				return fsm;
			}


			protected virtual void OnChanged(T obj)
			{
				var handler = Changed;
				if (handler != null) handler(obj);
			}
		}

	}

