using System.Linq;
using Bebimbop.Utilities;
using Bebimbop.Utilities.StateMachine;
using UniRx;
using UnityEngine;

namespace Bebimbop.Example
{
	public class LeftState : MonoBehaviour
	{
		public CanvasGroup[] UiPanels;
		private StateMachine<Left> _leftStateMachine;

		private void Start () 
		{
			_leftStateMachine = StateMachine<Left>.Initialize(this);
			UiPanels.ToList().ForEach(x => x.DisableCanvasGroup(true));
			_leftStateMachine.SetMode(StateTransition.Blend);
			_leftStateMachine.ChangeState(Left.One);
		}

		public void ChangeState(int idx)
		{
			_leftStateMachine.ChangeState((Left)idx);
		}

		private void One_Enter(float t)
		{
			UiPanels[0].FadingIn(t);
		}
		private void One_Exit(float t)
		{
			UiPanels[0].FadingOut(t);
		}
		
		
		private void Two_Enter(float t)
		{
			UiPanels[1].FadingIn(t);
		}
		private void Two_Exit(float t)
		{
			UiPanels[1].FadingOut(t);
		}
		
		
		private void Three_Enter(float t)
		{
			UiPanels[2].FadingIn(t);
		}
		private void Three_Exit(float t)
		{
			UiPanels[2].FadingOut(t);
		}
		private enum Left
		{
			One = 0,Two,Three
		}
	}
}

