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
    
        /// use them properly for a reset or somehting else
        /// this gets called while transitioning when chage state with overwrite transition
        private void BLUE_OnCancel()
        {
            Debug.Log("BLUE  cancled");
            EnterProgress.sizeDelta = new Vector2(0,30);
            ExitProgress.sizeDelta = new Vector2(0,30);
            UiPanel.DisableCanvasGroup(true);
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
    }
}

