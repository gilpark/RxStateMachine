using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Bebimbop.Utilities
{
    public static class ExtensionMethods
    {
        
        public static void FadingIn(this CanvasGroup c, float t, Action extrafunc = null, bool interactable = true,bool keepOn = true)
        {
            if (t == 0) c.gameObject.SetActive(true);
            if (t == 1)
            {
                c.interactable = interactable;
                c.blocksRaycasts = interactable;
                c.gameObject.SetActive(keepOn);
                if (extrafunc != null) extrafunc.Invoke();
            }
            c.alpha = t;
        }
     
        public static void FadingOut(this CanvasGroup c, float t, Action extrafunc = null, bool interactable = false,bool keepOn = false)
        {
            if (t == 0) c.gameObject.SetActive(true);
            if (t == 1)
            {
                c.interactable = interactable;
                c.blocksRaycasts = interactable;
                c.gameObject.SetActive(keepOn);
                if (extrafunc != null) extrafunc.Invoke();
            }
            c.alpha = t.FromTo(0, 1, 1, 0);
        }
        
        public static void DisableCanvasGroup(this CanvasGroup c, bool setoZero)
        {
            c.interactable = false;
            c.blocksRaycasts = false;
            if (setoZero) c.alpha = 0f;
        }
        public static void EnableCanvasGroup(this CanvasGroup c, bool setoOne)
        {
            c.gameObject.SetActive(true);
            c.interactable = true;
            c.blocksRaycasts = true;
            if (setoOne) c.alpha = 1f;
        }
        
        public static float FromTo(this float _f, float _fromMin, float _fromMax, float _toMin, float _toMax)
        {
            float returnFloat = (((_toMax - _toMin) * (_f - _fromMin)) / (_fromMax - _fromMin)) + _toMin;
            return returnFloat;
        }

    }
}