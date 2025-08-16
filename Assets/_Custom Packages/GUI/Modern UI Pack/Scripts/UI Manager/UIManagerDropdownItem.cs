using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class UIManagerDropdownItem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] UIManager UIManagerAsset;
        public bool overrideColors = false;
        public bool overrideFonts = false;

        [Header("Resources")]
        [SerializeField] Image itemBackground;
        [SerializeField] Image itemIcon;
        [SerializeField] TextMeshProUGUI itemText;

        void Awake()
        {
            if (UIManagerAsset == null) { UIManagerAsset = Resources.Load<UIManager>("MUIP Manager"); }

            this.enabled = true;

            if (UIManagerAsset.enableDynamicUpdate == false)
            {
                UpdateDropdown();
                this.enabled = false;
            }
        }

        void Update()
        {
            if (UIManagerAsset == null) { return; }
            if (UIManagerAsset.enableDynamicUpdate == true) { UpdateDropdown(); }
        }

        void UpdateDropdown()
        {
            if (overrideFonts == false && itemText != null) { itemText.font = UIManagerAsset.dropdownItemFont; }
            if (overrideColors == false)
            {
                if (itemBackground != null) { itemBackground.color = UIManagerAsset.dropdownItemBackgroundColor; }
                if (itemIcon != null) { itemIcon.color = UIManagerAsset.dropdownItemPrimaryColor; }
                if (itemText != null) { itemText.color = UIManagerAsset.dropdownItemPrimaryColor; }
            }
        }
    }
}