using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [AddComponentMenu("Modern UI Pack/Layout Group/Radial Layout Group")]
    public class RadialLayoutGroup : LayoutGroup
    {
        public enum Direction { Clockwise = 0, Counterclockwise = 1, Bidirectional = 2 }

        public enum ConstraintMode { Interval = 0, Range = 1 }

        [SerializeField] Direction refLayoutDir;
        public Direction layoutDir { get { return refLayoutDir; } set { SetProperty(ref refLayoutDir, value); } }

        [SerializeField] float refRadiusStart = 200;
        public float radiusStart { get { return refRadiusStart; } set { SetProperty(ref refRadiusStart, value); } }

        [SerializeField] float refRadiusDelta;
        public float radiusDelta { get { return refRadiusDelta; } set { SetProperty(ref refRadiusDelta, value); } }

        [SerializeField] float refRadiusRange;
        public float radiusRange { get { return refRadiusRange; } set { SetProperty(ref refRadiusRange, value); } }

        [SerializeField] float refAngleDelta;
        public float angleDelta { get { return refAngleDelta; } set { SetProperty(ref refAngleDelta, value); } }

        [SerializeField] float refAngleStart;
        public float angleStart { get { return refAngleStart; } set { SetProperty(ref refAngleStart, value); } }

        [SerializeField] float refAngleCenter;
        public float angleCenter { get { return refAngleCenter; } set { SetProperty(ref refAngleCenter, value); } }

        [SerializeField] float refAngleRange = 200;
        public float angleRange { get { return refAngleRange; } set { SetProperty(ref refAngleRange, value); } }

        [SerializeField] bool refChildRotate = false;
        public bool childRotate { get { return refChildRotate; } set { SetProperty(ref refChildRotate, value); } }

        public override void CalculateLayoutInputVertical() { }
        public override void CalculateLayoutInputHorizontal() { }
        public override void SetLayoutHorizontal() { CalculateChildrenPositions(); }
        public override void SetLayoutVertical() { CalculateChildrenPositions(); }

        List<RectTransform> childList = new List<RectTransform>();
        List<ILayoutIgnorer> ignoreList = new List<ILayoutIgnorer>();

        void CalculateChildrenPositions()
        {
            this.m_Tracker.Clear();
            childList.Clear();

            for (int i = 0; i < this.transform.childCount; ++i)
            {
                RectTransform rect = this.transform.GetChild(i) as RectTransform;

                if (!rect.gameObject.activeSelf)
                    continue;

                ignoreList.Clear();
                rect.GetComponents(ignoreList);

                if (ignoreList.Count == 0)
                {
                    childList.Add(rect);
                    continue;
                }

                for (int j = 0; j < ignoreList.Count; j++)
                {
                    if (!ignoreList[j].ignoreLayout)
                    {
                        childList.Add(rect);
                        break;
                    }
                }

                ignoreList.Clear();
            }

            EnsureParameters(childList.Count);

            for (int i = 0; i < childList.Count; ++i)
            {
                var child = childList[i];
                float delta = i * angleDelta;
                float angle = layoutDir == Direction.Clockwise ? angleStart - delta : angleStart + delta;
                ProcessOneChild(child, angle, radiusStart + (i * radiusDelta));
            }

            childList.Clear();
        }

        void EnsureParameters(int childCount)
        {
            EnsureAngleParameters(childCount);
            EnsureRadiusParameters(childCount);
        }

        void EnsureAngleParameters(int childCount)
        {
            int intervalCount = childCount - 1;

            switch (layoutDir)
            {
                case Direction.Clockwise:
                    if (intervalCount > 0) { this.angleDelta = this.angleRange / intervalCount; }
                    else { this.angleDelta = 0; }
                    break;

                case Direction.Counterclockwise:
                    if (intervalCount > 0) { this.angleDelta = this.angleRange / intervalCount; }
                    else { this.angleDelta = 0; }
                    break;

                case Direction.Bidirectional:
                    if (intervalCount > 0) { this.angleDelta = this.angleRange / intervalCount; }
                    else { this.angleDelta = 0; }
                    this.angleStart = this.angleCenter - angleRange * 0.5f;
                    break;
            }
        }


        void EnsureRadiusParameters(int childCount)
        {
            int intervalCount = childCount - 1;

            switch (layoutDir)
            {
                case Direction.Clockwise:
                    if (intervalCount > 0) { this.radiusDelta = radiusRange / intervalCount; }
                    else { this.radiusDelta = 0; }
                    break;

                case Direction.Counterclockwise:

                case Direction.Bidirectional:
                    if (intervalCount > 0) { this.radiusDelta = radiusRange / intervalCount; }
                    else { this.radiusDelta = 0; }
                    break;
            }
        }

        static readonly Vector2 center = new Vector2(0.5f, 0.5f);

        void ProcessOneChild(RectTransform child, float angle, float radius)
        {
            Vector3 pos = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0.0f);
            child.localPosition = pos * radius;

            DrivenTransformProperties drivenProperties =
                DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Rotation | DrivenTransformProperties.Pivot;
            m_Tracker.Add(this, child, drivenProperties);

            child.anchorMin = center;
            child.anchorMax = center;
            child.pivot = center;

            if (this.childRotate) { child.localEulerAngles = new Vector3(0, 0, angle); }
            else { child.localEulerAngles = Vector3.zero; }
        }
    }
}
