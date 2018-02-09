using Bebimbop.Example;
using UnityEngine;
using Bebimbop.Utilities;
using Bebimbop.Utilities.StateMachine;
using UnityEngine.UI;

namespace Bebimbop.Example
{
    public class GreenState : MonoBehaviour
    {
        public StateMachine<GameManager.State> MainStateMachine;
        public Image Background;
        public Color StartColor, EndColor;
        
        private void Start()
        {
            MainStateMachine = GameManager.Instance.MainStateMachine;   
            //to overwrite enter, exit rutine calls
            MainStateMachine.AddSubscriber(this);
            UiPanel.DisableCanvasGroup(true);
        }
    
        public CanvasGroup UiPanel;
        private void GREEN_Enter()
        {
            Debug.Log("Entering GREEN Time : " + Time.time);
            UiPanel.FadingIn(1);
        }
    
        private void GREEN_OnCancel()
        {
            Debug.Log("GREEN  cancled");
            UiPanel.DisableCanvasGroup(true);
        }
  
        private void GREEN_Exit()
        {
            Debug.Log("Exiting GREEN Time : " + Time.time);
            UiPanel.FadingOut(1);
        }


        private float t;
        private bool _ranFor1Frame = false;
        //overwrite Update,fixedupdate,lateupdate
        private void GREEN_Update()
        {
            if(!_ranFor1Frame) Debug.Log("Exmaple02 Update - Time :" + Time.time);
            t += Time.deltaTime;
            Background.color = Color.Lerp(StartColor, EndColor, Mathf.Sin(t).FromTo(-1,1,0,1));
        }
        private void GREEN_FixedUpdate()
        {
            if(!_ranFor1Frame) Debug.Log("Exmaple02 FixedUpdate - Time :" + Time.time);
            if (!_ranFor1Frame) _ranFor1Frame = true;
        }
        private void GREEN_LateUpdate()
        {
            if(!_ranFor1Frame) Debug.Log("Exmaple02 LateUpdate - Time :" + Time.time);
        }
        
    }    
}


