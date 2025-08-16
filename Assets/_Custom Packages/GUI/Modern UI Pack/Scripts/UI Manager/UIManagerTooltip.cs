using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class UIManagerTooltip : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] UIManager UIManagerAsset;

        [Header("Resources")]
        [SerializeField] Image background;
        [SerializeField] TextMeshProUGUI text;

        void Awake()
        {
            if (UIManagerAsset == null) { UIManagerAsset = Resources.Load<UIManager>("MUIP Manager"); }

            this.enabled = true;

            if (UIManagerAsset.enableDynamicUpdate == false)
            {
                UpdateTooltip();
                this.enabled = false;
            }
        }

        void Update()
        {
            if (UIManagerAsset == null) { return; }
            if (UIManagerAsset.enableDynamicUpdate == true) { UpdateTooltip(); }
        }

        void UpdateTooltip()
        {
            background.color = UIManagerAsset.tooltipBackgroundColor;
            text.color = UIManagerAsset.tooltipTextColor;
            text.font = UIManagerAsset.tooltipFont;
            text.fontSize = UIManagerAsset.tooltipFontSize;
        }
    }
}