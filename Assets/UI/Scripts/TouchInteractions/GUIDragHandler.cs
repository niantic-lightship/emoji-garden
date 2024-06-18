// Copyright 2022-2024 Niantic.
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Vector2 = UnityEngine.Vector2;

namespace Niantic.Lightship.AR.Samples
{
    public class GUIDragHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private Vector2 _startPoint;
        private Vector2 _endPoint;
        
        public enum DraggedDirection
        {
            Up,
            Down,
            Right,
            Left
        }

        public UnityEvent<Vector2, DraggedDirection> DragDidEnd;
        public UnityEvent<Vector2, DraggedDirection> DragIsActive; 
        
        private DraggedDirection GetDragDirection(Vector2 dragVector)
        {
            float positiveX = Mathf.Abs(dragVector.x);
            float positiveY = Mathf.Abs(dragVector.y);
            DraggedDirection draggedDir;
            if (positiveX > positiveY)
            {
                draggedDir = (dragVector.x > 0) ? DraggedDirection.Right : DraggedDirection.Left;
            }
            else
            {
                draggedDir = (dragVector.y > 0) ? DraggedDirection.Up : DraggedDirection.Down;
            }

            return draggedDir;
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPoint = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _endPoint = eventData.position;
            var dragVector = (_endPoint - _startPoint).normalized;

            var direction = GetDragDirection(dragVector);
            DragDidEnd?.Invoke(dragVector, direction);

        }
       
        public void OnDrag(PointerEventData eventData)
        {
            //calculate drag distance and vector
            var _currentPoint = eventData.position;
            var dragVector = (_currentPoint - _startPoint);
            var dragDirection = GetDragDirection(dragVector.normalized);
            DragIsActive?.Invoke(dragVector, dragDirection);
        }
    }
}