/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System.Collections.Generic;
using UnityEngine;

namespace FancyScrollView
{
    /// <summary>
    /// Abstract base class to implement a scroll view.
    /// Supports infinite scrolling and snapping.
    /// Use <see cref="FancyScrollView{TItemData}"/> if <see cref="FancyScrollView{TItemData, TContext}.Context"/> is not needed.
    /// </summary>
    /// <typeparam name="TItemData">Data type of the items.</typeparam>
    /// <typeparam name="TContext"><see cref="Context"/> type.</typeparam>
    public abstract class FancyScrollView<TItemData, TContext> : MonoBehaviour where TContext : class, new()
    {
        /// <summary>
        /// The space between cells.
        /// </summary>
        [SerializeField, Range(1e-2f, 1f)] protected float cellInterval = 0.2f;

        /// <summary>
        /// The scroll position reference.
        /// </summary>
        /// <remarks>
        /// For example, setting <c>0.5</c> for the scroll position of <c>0</c> will place the first cell in the center.
        /// </remarks>
        [SerializeField, Range(0f, 1f)] protected float scrollOffset = 0.5f;

        /// <summary>
        /// Whether to loop the cells.
        /// </summary>
        /// <remarks>
        /// If <c>true</c>, the first cell will appear after the last cell, and vice versa, enabling infinite scrolling.
        /// </remarks>
        [SerializeField] protected bool loop = false;

        /// <summary>
        /// The parent <c>Transform</c> for the cells.
        /// </summary>
        [SerializeField] protected Transform cellContainer = default;

        readonly IList<FancyCell<TItemData, TContext>> pool = new List<FancyCell<TItemData, TContext>>();

        /// <summary>
        /// Whether the scroll view has been initialized.
        /// </summary>
        protected bool initialized;

        /// <summary>
        /// The current scroll position.
        /// </summary>
        protected float currentPosition;

        /// <summary>
        /// The Prefab for the cells.
        /// </summary>
        protected abstract GameObject CellPrefab { get; }

        /// <summary>
        /// The data list for the items.
        /// </summary>
        protected IList<TItemData> ItemsSource { get; set; } = new List<TItemData>();

        /// <summary>
        /// The instance of <typeparamref name="TContext"/>.
        /// The same instance is shared between the cells and the scroll view. Used for passing information and maintaining state.
        /// </summary>
        protected TContext Context { get; } = new TContext();

        /// <summary>
        /// Initializes the scroll view.
        /// </summary>
        /// <remarks>
        /// Called just before the first cells are created.
        /// </remarks>
        protected virtual void Initialize() { }

        /// <summary>
        /// Updates the display based on the given item list.
        /// </summary>
        /// <param name="itemsSource">The item list.</param>
        protected virtual void UpdateContents(IList<TItemData> itemsSource)
        {
            ItemsSource = itemsSource;
            Refresh();
        }

        /// <summary>
        /// Forces a layout update for the cells.
        /// </summary>
        protected virtual void Relayout() => UpdatePosition(currentPosition, false);

        /// <summary>
        /// Forces a layout and content update.
        /// </summary>
        protected virtual void Refresh() => UpdatePosition(currentPosition, true);

        /// <summary>
        /// Updates the scroll position.
        /// </summary>
        /// <param name="position">The scroll position.</param>
        protected virtual void UpdatePosition(float position) => UpdatePosition(position, false);

        void UpdatePosition(float position, bool forceRefresh)
        {
            if (!initialized)
            {
                Initialize();
                initialized = true;
            }

            currentPosition = position;

            var p = position - scrollOffset / cellInterval;
            var firstIndex = Mathf.CeilToInt(p);
            var firstPosition = (Mathf.Ceil(p) - p) * cellInterval;

            if (firstPosition + pool.Count * cellInterval < 1f)
            {
                ResizePool(firstPosition);
            }

            UpdateCells(firstPosition, firstIndex, forceRefresh);
        }

        void ResizePool(float firstPosition)
        {
            Debug.Assert(CellPrefab != null);
            Debug.Assert(cellContainer != null);

            var addCount = Mathf.CeilToInt((1f - firstPosition) / cellInterval) - pool.Count;
            for (var i = 0; i < addCount; i++)
            {
                var cell = Instantiate(CellPrefab, cellContainer).GetComponent<FancyCell<TItemData, TContext>>();
                if (cell == null)
                {
                    throw new MissingComponentException(string.Format(
                        "FancyCell<{0}, {1}> component not found in {2}.",
                        typeof(TItemData).FullName, typeof(TContext).FullName, CellPrefab.name));
                }

                cell.SetContext(Context);
                cell.Initialize();
                cell.SetVisible(false);
                pool.Add(cell);
            }
        }

        void UpdateCells(float firstPosition, int firstIndex, bool forceRefresh)
        {
            for (var i = 0; i < pool.Count; i++)
            {
                var index = firstIndex + i;
                var position = firstPosition + i * cellInterval;
                var cell = pool[CircularIndex(index, pool.Count)];

                if (loop)
                {
                    index = CircularIndex(index, ItemsSource.Count);
                }

                if (index < 0 || index >= ItemsSource.Count || position > 1f)
                {
                    cell.SetVisible(false);
                    continue;
                }

                if (forceRefresh || cell.Index != index || !cell.IsVisible)
                {
                    cell.Index = index;
                    cell.SetVisible(true);
                    cell.UpdateContent(ItemsSource[index]);
                }

                cell.UpdatePosition(position);
            }
        }

        int CircularIndex(int i, int size) => size < 1 ? 0 : i < 0 ? size - 1 + (i + 1) % size : i % size;

#if UNITY_EDITOR
        bool cachedLoop;
        float cachedCellInterval, cachedScrollOffset;

        void LateUpdate()
        {
            if (cachedLoop != loop ||
                cachedCellInterval != cellInterval ||
                cachedScrollOffset != scrollOffset)
            {
                cachedLoop = loop;
                cachedCellInterval = cellInterval;
                cachedScrollOffset = scrollOffset;

                UpdatePosition(currentPosition);
            }
        }
#endif
    }

    /// <summary>
    /// Context class for <see cref="FancyScrollView{TItemData}"/>.
    /// </summary>
    public sealed class NullContext { }

    /// <summary>
    /// Abstract base class to implement a scroll view.
    /// Supports infinite scrolling and snapping.
    /// </summary>
    /// <typeparam name="TItemData"></typeparam>
    /// <seealso cref="FancyScrollView{TItemData, TContext}"/>
    public abstract class FancyScrollView<TItemData> : FancyScrollView<TItemData, NullContext> { }
}