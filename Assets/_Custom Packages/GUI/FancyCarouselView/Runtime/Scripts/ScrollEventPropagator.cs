using System;
using System.Collections.Generic;
using FancyScrollView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FancyCarouselView.Runtime.Scripts
{
    [RequireComponent(typeof(Scroller))]
    public class ScrollEventPropagator : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        IEnumerable<IBeginDragHandler> _beginDragHandlers;
        IEnumerable<IDragHandler> _dragHandlers;
        IEnumerable<IEndDragHandler> _endDragHandlers;
        bool _isEnabled;
        Scroller _scroller;

        void Start()
        {
            _scroller = GetComponent<Scroller>();
            var parentScrollRect = GetComponentInParent<ScrollRect>();
            if (parentScrollRect != null)
            {
                _beginDragHandlers = parentScrollRect.GetComponents<IBeginDragHandler>();
                _dragHandlers = parentScrollRect.GetComponents<IDragHandler>();
                _endDragHandlers = parentScrollRect.GetComponents<IEndDragHandler>();
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (_scroller.ScrollDirection == ScrollDirection.Vertical
                && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
            {
                _isEnabled = true;
            }
            else if (_scroller.ScrollDirection == ScrollDirection.Horizontal
                     && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
            {
                _isEnabled = true;
            }
            else
            {
                _isEnabled = false;
            }

            if (!_isEnabled || _beginDragHandlers == null)
            {
                return;
            }

            foreach (var handler in _beginDragHandlers)
            {
                handler.OnBeginDrag(eventData);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!_isEnabled || _dragHandlers == null)
            {
                return;
            }

            foreach (var handler in _dragHandlers)
            {
                handler.OnDrag(eventData);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!_isEnabled || _endDragHandlers == null)
            {
                return;
            }

            foreach (var handler in _endDragHandlers)
            {
                handler.OnEndDrag(eventData);
            }
        }
    }
}