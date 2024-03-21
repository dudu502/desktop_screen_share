using UnityEngine;
using Think.Viewer.Manager;

namespace Think.Viewer.Module
{
    public class UIView : MonoBehaviour
    {
        [HideInInspector]
        public Layer CurrentLayer;
        private Animator _uiAnimator;
        [HideInInspector]
        public bool NeedTerminatePop { get; protected set; } = false;
        private byte _disableState = 0;
        private const string ANI_OPEN_NAME = "open";
        private const string ANI_CLOSE_NAME = "close";
        public virtual void OnInit()
        {
            _uiAnimator = GetComponent<Animator>();
            if (_uiAnimator != null)
                _uiAnimator.Play(ANI_OPEN_NAME);
        }

        public virtual void OnShow(object obj)
        {

        }
        public virtual void OnClose()
        {
            _disableState = 0;
            if (_uiAnimator != null)
                _uiAnimator.Play(ANI_CLOSE_NAME);
            else
                Destroy(gameObject);
        }

        public void Close()
        {
            ModuleManager.GetModule<UIModule>().Pop(CurrentLayer, this);
        }

        public void OnCloseAnimationStop()
        {
            if (_disableState == 0)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
        public virtual void OnPause()
        {
            _disableState = 1;
            if (_uiAnimator != null)
                _uiAnimator.Play(ANI_CLOSE_NAME);
            else
                gameObject.SetActive(false);
        }
        public virtual void OnTerminatePop()
        {
            // Need override
        }
        public virtual void OnResume()
        {
            gameObject.SetActive(true);
            if (_uiAnimator != null)
                _uiAnimator.Play(ANI_OPEN_NAME);
        }
    }
}
