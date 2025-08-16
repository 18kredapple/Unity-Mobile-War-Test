using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class UIManagerScrollbar : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] UIManager UIManagerAsset;

        [Header("Resources")]
        [SerializeField] Image background;
        [SerializeField] Image bar;

        void Awake()
        {
            if (UIManagerAsset == null) { UIManagerAsset = Resources.Load<UIManager>("MUIP Manager"); }

            this.enabled = true;

            if (UIManagerAsset.enableDynamicUpdate == false)
            {
                UpdateScrollbar();
                this.enabled = false;
            }
        }

        void Update()
        {
            if (UIManagerAsset == null) { return; }
            if (UIManagerAsset.enableDynamicUpdate == true) { UpdateScrollbar(); }
        }

        void UpdateScrollbar()
        {
            background.color = UIManagerAsset.scrollbarBackgroundColor;
            bar.color = UIManagerAsset.scrollbarColor;
        }
    }
}