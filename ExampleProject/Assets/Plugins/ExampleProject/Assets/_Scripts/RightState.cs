using System.Linq;
using Bebimbop.Utilities;
using Bebimbop.Utilities.StateMachine;
using UnityEngine;

namespace Bebimbop.Example
{
	public class RightState : MonoBehaviour
	{
		public CanvasGroup[] UiPanels;
		private StateMachine<Right> _rightStateMachine;

		private void Start () 
		{
			_rightStateMachine = StateMachine<Right>.Initialize(this);
			UiPanels.ToList().ForEach(x => x.DisableCanvasGroup(true));
			_rightStateMachine.SetMode(StateTransition.Overwrite);
			_rightStateMachine.ChangeState(Right.One);
		}

		public void ChangeState(int idx)
		{
			_rightStateMachine.ChangeState((Right)idx);
		}

		//we done need Exit method since this example's transition mode is overwrite
		//but we use OnCancel method to reset canvas group objects
		private void One_Enter()
		{
			UiPanels[0].FadingIn(1);
		}
		private void One_OnCancel()
		{
			UiPanels[0].DisableCanvasGroup(true);
		}

		
		private void Two_Enter()
		{
			UiPanels[1].FadingIn(1);
		}
		private void Two_OnCancel()
		{
			UiPanels[1].DisableCanvasGroup(true);
		}
		
		
		private void Three_Enter()
		{
			UiPanels[2].FadingIn(1);
		}
		private void Three_OnCancel()
		{
			UiPanels[2].DisableCanvasGroup(true);
		}
		
		private enum Right
		{
			One = 0,Two,Three
		}
	}
}

