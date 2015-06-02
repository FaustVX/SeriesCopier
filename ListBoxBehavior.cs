using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SeriesCopier
{
    public static class ListBoxBehavior
    {
        #region ScrollOnNewItem

        private static readonly Dictionary<ListBox, Capture> Associations = new Dictionary<ListBox, Capture>();

        public static bool GetScrollOnNewItem(DependencyObject obj)
            => (bool) obj.GetValue(ScrollOnNewItemProperty);

        public static void SetScrollOnNewItem(DependencyObject obj, bool value)
            => obj.SetValue(ScrollOnNewItemProperty, value);

        public static readonly DependencyProperty ScrollOnNewItemProperty =
            DependencyProperty.RegisterAttached(
                "ScrollOnNewItem",
                typeof (bool),
                typeof (ListBoxBehavior),
                new UIPropertyMetadata(false, OnScrollOnNewItemChanged));

        public static void OnScrollOnNewItemChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var listBox = d as ListBox;
            if (listBox == null)
                return;
            bool oldValue = (bool) e.OldValue, newValue = (bool) e.NewValue;
            if (newValue == oldValue)
                return;
            if (newValue)
            {
                listBox.Loaded += ListBox_Loaded;
                listBox.Unloaded += ListBox_Unloaded;
                var itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
                itemsSourcePropertyDescriptor.AddValueChanged(listBox, ListBox_ItemsSourceChanged);
            }
            else
            {
                listBox.Loaded -= ListBox_Loaded;
                listBox.Unloaded -= ListBox_Unloaded;
                if (Associations.ContainsKey(listBox))
                    Associations[listBox].Dispose();
                var itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
                itemsSourcePropertyDescriptor.RemoveValueChanged(listBox, ListBox_ItemsSourceChanged);
            }
        }

        private static void ListBox_ItemsSourceChanged(object sender, EventArgs e)
        {
            var listBox = (ListBox) sender;
            if (Associations.ContainsKey(listBox))
                Associations[listBox].Dispose();
            Associations[listBox] = new Capture(listBox);
        }

        private static void ListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox) sender;
            if (Associations.ContainsKey(listBox))
                Associations[listBox].Dispose();
            listBox.Unloaded -= ListBox_Unloaded;
        }

        private static void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox) sender;
            var incc = listBox.Items as INotifyCollectionChanged;
            if (incc == null) return;
            listBox.Loaded -= ListBox_Loaded;
            Associations[listBox] = new Capture(listBox);
        }

        private class Capture : IDisposable
        {
            private readonly ListBox listBox;
            private readonly INotifyCollectionChanged incc;

            public Capture(ListBox listBox)
            {
                this.listBox = listBox;
                incc = listBox.ItemsSource as INotifyCollectionChanged;
                if (incc != null)
                {
                    incc.CollectionChanged += incc_CollectionChanged;
                }
            }

            private void incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    listBox.ScrollIntoView(e.NewItems[0]);
                    listBox.SelectedItem = e.NewItems[0];
                }
            }

            public void Dispose()
            {
                if (incc != null)
                    incc.CollectionChanged -= incc_CollectionChanged;
            }
        }

        #endregion

        #region VerticalScrollGroup

        private static readonly Dictionary<ScrollViewer, string> _verticalScrollViewers = new Dictionary<ScrollViewer, string>();

        private static readonly Dictionary<string, double> _verticalScrollOffsets = new Dictionary<string, double>();

        public static readonly DependencyProperty VerticalScrollGroupProperty =
            DependencyProperty.RegisterAttached(
                "VerticalScrollGroup",
                typeof (string),
                typeof (ListBoxBehavior),
                new PropertyMetadata(OnVerticalScrollGroupChanged));

        public static void SetVerticalScrollGroup(DependencyObject obj, string scrollGroup)
            => obj.SetValue(VerticalScrollGroupProperty, scrollGroup);

        public static string GetVerticalScrollGroup(DependencyObject obj)
            => (string) obj.GetValue(VerticalScrollGroupProperty);

        private static void OnVerticalScrollGroupChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;
            if (scrollViewer != null)
            {
                if (!string.IsNullOrEmpty((string) e.OldValue))
                    // Remove scrollviewer
                    if (_verticalScrollViewers.ContainsKey(scrollViewer))
                    {
                        scrollViewer.ScrollChanged -= ScrollViewer_VerticalScrollChanged;
                        _verticalScrollViewers.Remove(scrollViewer);
                    }

                if (!string.IsNullOrEmpty((string) e.NewValue))
                {
                    // If group already exists, set scrollposition of 
                    // new scrollviewer to the scrollposition of the group
                    if (_verticalScrollOffsets.Keys.Contains((string) e.NewValue))
                        scrollViewer.ScrollToHorizontalOffset(
                            _verticalScrollOffsets[(string) e.NewValue]);
                    else
                        _verticalScrollOffsets.Add((string) e.NewValue,
                            scrollViewer.HorizontalOffset);

                    // Add scrollviewer
                    _verticalScrollViewers.Add(scrollViewer, (string) e.NewValue);
                    scrollViewer.ScrollChanged += ScrollViewer_VerticalScrollChanged;
                }
            }
        }

        private static void ScrollViewer_VerticalScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                var changedScrollViewer = sender as ScrollViewer;
                VerticalScroll(changedScrollViewer);
            }
        }

        private static void VerticalScroll(ScrollViewer changedScrollViewer)
        {
            var group = _verticalScrollViewers[changedScrollViewer];
            _verticalScrollOffsets[group] = changedScrollViewer.HorizontalOffset;

            foreach (var scrollViewer in _verticalScrollViewers.Where((s) => s.Value == group && s.Key != changedScrollViewer))
                if (scrollViewer.Key.VerticalOffset != changedScrollViewer.VerticalOffset)
                    scrollViewer.Key.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
        }

        #endregion

        #region HorizontalScrollGroup

        private static readonly Dictionary<ScrollViewer, string> _horizontalScrollViewers = new Dictionary<ScrollViewer, string>();

        private static readonly Dictionary<string, double> _horizontalScrollOffsets = new Dictionary<string, double>();

        public static readonly DependencyProperty HorizontalScrollGroupProperty =
            DependencyProperty.RegisterAttached(
                "HorizontalScrollGroup",
                typeof(string),
                typeof(ListBoxBehavior),
                new PropertyMetadata(OnHorizontalScrollGroupChanged));

        public static void SetHorizontalScrollGroup(DependencyObject obj, string scrollGroup)
            => obj.SetValue(HorizontalScrollGroupProperty, scrollGroup);

        public static string GetHorizontalScrollGroup(DependencyObject obj)
            => (string)obj.GetValue(HorizontalScrollGroupProperty);

        private static void OnHorizontalScrollGroupChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;
            if (scrollViewer != null)
            {
                if (!string.IsNullOrEmpty((string)e.OldValue))
                    // Remove scrollviewer
                    if (_horizontalScrollViewers.ContainsKey(scrollViewer))
                    {
                        scrollViewer.ScrollChanged -= ScrollViewer_HorizontalScrollChanged;
                        _horizontalScrollViewers.Remove(scrollViewer);
                    }

                if (!string.IsNullOrEmpty((string)e.NewValue))
                {
                    // If group already exists, set scrollposition of 
                    // new scrollviewer to the scrollposition of the group
                    if (_horizontalScrollOffsets.Keys.Contains((string)e.NewValue))
                        scrollViewer.ScrollToHorizontalOffset(
                            _horizontalScrollOffsets[(string)e.NewValue]);
                    else
                        _horizontalScrollOffsets.Add((string)e.NewValue,
                            scrollViewer.HorizontalOffset);

                    // Add scrollviewer
                    _horizontalScrollViewers.Add(scrollViewer, (string)e.NewValue);
                    scrollViewer.ScrollChanged += ScrollViewer_HorizontalScrollChanged;
                }
            }
        }

        private static void ScrollViewer_HorizontalScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange != 0)
            {
                var changedScrollViewer = sender as ScrollViewer;
                HorizontalScroll(changedScrollViewer);
            }
        }

        private static void HorizontalScroll(ScrollViewer changedScrollViewer)
        {
            var group = _horizontalScrollViewers[changedScrollViewer];
            _horizontalScrollOffsets[group] = changedScrollViewer.HorizontalOffset;

            foreach (var scrollViewer in _horizontalScrollViewers.Where((s) => s.Value == group && s.Key != changedScrollViewer))
                if (scrollViewer.Key.HorizontalOffset != changedScrollViewer.HorizontalOffset)
                    scrollViewer.Key.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
        }

        #endregion
    }
}