using UnityEngine;
using Bebimbop.Utilities;
using Bebimbop.Utilities.StateMachine;

namespace Bebimbop.Example
{
    public class BlueState : MonoBehaviour
    {
        public StateMachine<GameManager.State> MainStateMachine;
       
        private void Start()
        {
            MainStateMachine = GameManager.Instance.MainStateMachine;   
            //to overwrite enter, exit rutine calls
            MainStateMachine.AddSubscriber(this);
            UiPanel.DisableCanvasGroup(true);
        }

        public CanvasGroup UiPanel;
        public RectTransform EnterProgress,ExitProgress;

        //overwrite enter routine
        private void BLUE_Enter(float t)
        {
            if (t == 0)
            {
                Debug.Log("Entering BLUE Time : " + Time.time);
                ExitProgress.sizeDelta = new Vector2(0,30);    
            }
            
            UiPanel.FadingIn(t);
            EnterProgress.sizeDelta = new Vector2(t.FromTo(0,1,0,1920),30);
            
            if (t == 1) EnterProgress.sizeDelta = new Vector2(0,30);
        }
        /// this gets called while transitioning when chage state with overwrite transition
        private void BLUE_EnterCancel()
        {
            Debug.Log("BLUE Enter cancled");
           UiReset();
        }

        private void BLUE_Exit(float t)
        {
            if (t == 0)
            {
                Debug.Log("Exiting BLUE Time : " + Time.time);
                EnterProgress.sizeDelta = new Vector2(0,30);
            }
            UiPanel.FadingOut(t);
            ExitProgress.sizeDelta = new Vector2(t.FromTo(0,1,0,1920),30);
            if(t == 1)  ExitProgress.sizeDelta = new Vector2(0,30);
        }
        private void BLUE_ExitCancel()
        {
            Debug.Log("BLUE Exit cancled");
            UiPanel.EnableCanvasGroup(true);
        }
        private void BLUE_Finally()
        {
            Debug.Log("BLUE Finally called");
            UiReset();
        }
        private void UiReset()
        {
            EnterProgress.sizeDelta = new Vector2(0,30);
            ExitProgress.sizeDelta = new Vector2(0,30);
            UiPanel.DisableCanvasGroup(true);
        }
    }
}

