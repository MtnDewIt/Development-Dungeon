using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Bonobo.Plugins.TagFileList
{
    public class MultiSelectTreeViewItem : ListBoxItem
    {
    	private class ItemContainerGeneratorStatusChangedContainer
    	{
    		public MultiSelectTreeViewItem Item { get; set; }

    		public Dispatcher Dispatcher { get; set; }

    		public void StatusChanged(object sender, EventArgs args)
    		{
    			if (Item.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
    			{
    				Item.ItemContainerGenerator.StatusChanged -= StatusChanged;
    				Item.HasExpanded = true;
    				Item.Status = GeneratorStatus.ContainersGenerated;
    				Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    				{
    					Item.DoStatusChanged();
    				});
    			}
    		}
    	}

    	public static readonly RoutedEvent PreviewExpandedEvent;

    	public static readonly RoutedEvent PreviewCollapsedEvent;

    	public static readonly RoutedEvent ExpandedEvent;

    	public static readonly RoutedEvent CollapsedEvent;

    	public static DependencyProperty StatusProperty;

    	public static readonly RoutedEvent StatusChangedEvent;

    	public static DependencyProperty IsExpandedProperty;

    	public static DependencyProperty HasExpandedProperty;

    	public static DependencyProperty HasItemsProperty;

    	public static DependencyProperty IsSelectionActiveProperty;

    	public static DependencyProperty ParentItemProperty;

    	private MultiSelectTreeView parentTreeView;

    	private List<TagFileListFile> items = new List<TagFileListFile>();

    	public GeneratorStatus Status
    	{
    		get
    		{
    			return (GeneratorStatus)GetValue(StatusProperty);
    		}
    		set
    		{
    			SetValue(StatusProperty, value);
    		}
    	}

    	public bool IsExpanded
    	{
    		get
    		{
    			return (bool)GetValue(IsExpandedProperty);
    		}
    		set
    		{
    			SetValue(IsExpandedProperty, value);
    		}
    	}

    	public bool HasExpanded
    	{
    		get
    		{
    			return (bool)GetValue(HasExpandedProperty);
    		}
    		set
    		{
    			SetValue(HasExpandedProperty, value);
    		}
    	}

    	public bool HasItems
    	{
    		get
    		{
    			return (bool)GetValue(HasItemsProperty);
    		}
    		set
    		{
    			SetValue(HasItemsProperty, value);
    		}
    	}

    	public bool IsSelectionActive
    	{
    		get
    		{
    			return (bool)GetValue(IsSelectionActiveProperty);
    		}
    		set
    		{
    			SetValue(IsSelectionActiveProperty, value);
    		}
    	}

    	public MultiSelectTreeViewItem ParentItem
    	{
    		get
    		{
    			return (MultiSelectTreeViewItem)GetValue(ParentItemProperty);
    		}
    		set
    		{
    			SetValue(ParentItemProperty, value);
    		}
    	}

    	public IList<TagFileListFile> Items => items;

    	private ItemContainerGenerator ItemContainerGenerator => parentTreeView.ItemContainerGenerator;

    	private ItemsControl ItemsControl => parentTreeView;

    	public event RoutedEventHandler PreviewExpanded
    	{
    		add
    		{
    			AddHandler(PreviewExpandedEvent, value);
    		}
    		remove
    		{
    			RemoveHandler(PreviewExpandedEvent, value);
    		}
    	}

    	public event RoutedEventHandler PreviewCollapsed
    	{
    		add
    		{
    			AddHandler(PreviewCollapsedEvent, value);
    		}
    		remove
    		{
    			RemoveHandler(PreviewCollapsedEvent, value);
    		}
    	}

    	public event RoutedEventHandler Expanded
    	{
    		add
    		{
    			AddHandler(ExpandedEvent, value);
    		}
    		remove
    		{
    			RemoveHandler(ExpandedEvent, value);
    		}
    	}

    	public event RoutedEventHandler Collapsed
    	{
    		add
    		{
    			AddHandler(CollapsedEvent, value);
    		}
    		remove
    		{
    			RemoveHandler(CollapsedEvent, value);
    		}
    	}

    	public event RoutedEventHandler StatusChanged
    	{
    		add
    		{
    			AddHandler(StatusChangedEvent, value);
    		}
    		remove
    		{
    			RemoveHandler(StatusChangedEvent, value);
    		}
    	}

    	private static void OnIsExpandedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    	{
    		MultiSelectTreeViewItem multiSelectTreeViewItem = sender as MultiSelectTreeViewItem;
    		if (multiSelectTreeViewItem.IsExpanded)
    		{
    			multiSelectTreeViewItem.DoExpand();
    		}
    		else
    		{
    			multiSelectTreeViewItem.DoCollapse();
    		}
    	}

    	static MultiSelectTreeViewItem()
    	{
    		PreviewExpandedEvent = EventManager.RegisterRoutedEvent("PreviewExpanded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MultiSelectTreeViewItem));
    		PreviewCollapsedEvent = EventManager.RegisterRoutedEvent("PreviewCollapsed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MultiSelectTreeViewItem));
    		ExpandedEvent = EventManager.RegisterRoutedEvent("Expanded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MultiSelectTreeViewItem));
    		CollapsedEvent = EventManager.RegisterRoutedEvent("Collapsed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MultiSelectTreeViewItem));
    		StatusProperty = DependencyProperty.Register("Status", typeof(GeneratorStatus), typeof(MultiSelectTreeViewItem));
    		StatusChangedEvent = EventManager.RegisterRoutedEvent("StatusChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MultiSelectTreeViewItem));
    		IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(OnIsExpandedChanged));
    		HasExpandedProperty = DependencyProperty.Register("HasExpanded", typeof(bool), typeof(MultiSelectTreeViewItem));
    		HasItemsProperty = DependencyProperty.Register("HasItems", typeof(bool), typeof(MultiSelectTreeViewItem));
    		IsSelectionActiveProperty = DependencyProperty.Register("IsSelectionActive", typeof(bool), typeof(MultiSelectTreeViewItem));
    		ParentItemProperty = DependencyProperty.Register("ParentItem", typeof(MultiSelectTreeViewItem), typeof(MultiSelectTreeViewItem));
    		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(typeof(MultiSelectTreeViewItem)));
    	}

    	public MultiSelectTreeViewItem()
    	{
    	}

    	public MultiSelectTreeViewItem(MultiSelectTreeView parentTree)
    	{
    		Status = GeneratorStatus.NotStarted;
    		HasItems = true;
    		base.Loaded += MultiSelectTreeViewItem_Loaded;
    		base.MouseDoubleClick += MultiSelectTreeViewItem_MouseDoubleClick;
    		parentTreeView = parentTree;
    	}

    	public TagFileListFile ItemFromContainer(MultiSelectTreeViewItem container)
    	{
    		if (ItemContainerGenerator.ItemFromContainer(container) is TagFileListFile tagFileListFile && items.Contains(tagFileListFile))
    		{
    			return tagFileListFile;
    		}
    		return null;
    	}

    	public MultiSelectTreeViewItem ContainerFromItem(TagFileListFile item)
    	{
    		if (!items.Contains(item))
    		{
    			return null;
    		}
    		return ItemContainerGenerator.ContainerFromItem(item) as MultiSelectTreeViewItem;
    	}

    	public void DoExpand()
    	{
    		OnPreviewExpanded();
    		if (!HasExpanded)
    		{
    			ItemContainerGeneratorStatusChangedContainer itemContainerGeneratorStatusChangedContainer = new ItemContainerGeneratorStatusChangedContainer();
    			itemContainerGeneratorStatusChangedContainer.Item = this;
    			itemContainerGeneratorStatusChangedContainer.Dispatcher = base.Dispatcher;
    			ItemContainerGenerator.StatusChanged += itemContainerGeneratorStatusChangedContainer.StatusChanged;
    		}
    		RecursiveExpand(this);
    		OnExpanded();
    	}

    	public void DoCollapse()
    	{
    		OnPreviewCollapsed();
    		RecursiveCollapse(this);
    		OnCollapsed();
    	}

    	public void DoStatusChanged()
    	{
    		RoutedEventArgs e = new RoutedEventArgs(StatusChangedEvent);
    		RaiseEvent(e);
    	}

    	private void RecursiveExpand(MultiSelectTreeViewItem item)
    	{
    		if (!item.IsExpanded)
    		{
    			return;
    		}
    		bool flag = false;
    		int num = ItemContainerGenerator.IndexFromContainer(item);
    		TagFileListFile tagFileListFile = ItemContainerGenerator.ItemFromContainer(item) as TagFileListFile;
    		item.Items.Clear();
    		if (tagFileListFile is TagFileListFolder)
    		{
    			foreach (TagFileListFile child in (tagFileListFile as TagFileListFolder).Children)
    			{
    				item.Items.Add(child);
    			}
    			if (item.Items.Count == 0)
    			{
    				HasItems = false;
    			}
    			else
    			{
    				HasItems = true;
    			}
    		}
    		TagFileListFile[] array = item.Items.ToArray();
    		foreach (TagFileListFile item2 in array)
    		{
    			num++;
    			MultiSelectTreeViewItem multiSelectTreeViewItem = (MultiSelectTreeViewItem)ItemContainerGenerator.ContainerFromItem(item2);
    			if (multiSelectTreeViewItem != null)
    			{
    				RecursiveExpand(multiSelectTreeViewItem);
    				continue;
    			}
    			if (Status != GeneratorStatus.GeneratingContainers)
    			{
    				flag = true;
    				Status = GeneratorStatus.GeneratingContainers;
    			}
    			(parentTreeView.ItemsSource as TagFileListCollection).Insert(num, item2);
    		}
    		if (flag)
    		{
    			RoutedEventArgs e = new RoutedEventArgs(StatusChangedEvent);
    			RaiseEvent(e);
    			ItemContainerGeneratorStatusChangedContainer itemContainerGeneratorStatusChangedContainer = new ItemContainerGeneratorStatusChangedContainer();
    			itemContainerGeneratorStatusChangedContainer.Item = this;
    			itemContainerGeneratorStatusChangedContainer.Dispatcher = base.Dispatcher;
    			ItemContainerGenerator.StatusChanged += itemContainerGeneratorStatusChangedContainer.StatusChanged;
    		}
    	}

    	private void RecursiveCollapse(MultiSelectTreeViewItem item)
    	{
    		foreach (TagFileListFile item2 in item.Items)
    		{
    			if (parentTreeView.SelectedItems.Contains(item2))
    			{
    				parentTreeView.SelectedItems.Remove(item2);
    			}
    			MultiSelectTreeViewItem multiSelectTreeViewItem = (MultiSelectTreeViewItem)ItemContainerGenerator.ContainerFromItem(item2);
    			if (multiSelectTreeViewItem != null)
    			{
    				RecursiveCollapse(multiSelectTreeViewItem);
    			}
    			if (item.Content is TagFileListFolder && !(item.Content as TagFileListFolder).Children.Contains(item2))
    			{
    				(parentTreeView.ItemsSource as TagFileListCollection).Remove(item2);
    			}
    		}
    	}

    	private void RecursiveRemove(TagFileListFile item)
    	{
    		if (item == null)
    		{
    			return;
    		}
    		(parentTreeView.ItemsSource as TagFileListCollection).Remove(item);
    		if (!(item is TagFileListFolder))
    		{
    			return;
    		}
    		foreach (TagFileListFile child in (item as TagFileListFolder).Children)
    		{
    			RecursiveRemove(child);
    		}
    	}

    	private void RefreshFolder()
    	{
    		if (!(ItemContainerGenerator.ItemFromContainer(this) is TagFileListFolder tagFileListFolder))
    		{
    			return;
    		}
    		int num = ItemContainerGenerator.IndexFromContainer(this);
    		bool flag = false;
    		TagFileListFile[] array = Enumerable.ToArray(items);
    		foreach (TagFileListFile item in array)
    		{
    			if (!tagFileListFolder.Children.Contains(item))
    			{
    				if (Status != GeneratorStatus.GeneratingContainers)
    				{
    					flag = true;
    					Status = GeneratorStatus.GeneratingContainers;
    				}
    				RecursiveRemove(item);
    				items.Remove(item);
    			}
    		}
    		foreach (TagFileListFile child in tagFileListFolder.Children)
    		{
    			num++;
    			if (child is TagFileListFolder)
    			{
    				num += CountFolderItemsInTreeViewRecursive(child as TagFileListFolder);
    			}
    			if (!items.Contains(child))
    			{
    				if (Status != GeneratorStatus.GeneratingContainers)
    				{
    					flag = true;
    					Status = GeneratorStatus.GeneratingContainers;
    				}
    				(parentTreeView.ItemsSource as TagFileListCollection).Insert(num, child);
    				items.Add(child);
    			}
    		}
    		if (flag)
    		{
    			RoutedEventArgs e = new RoutedEventArgs(StatusChangedEvent);
    			RaiseEvent(e);
    			ItemContainerGeneratorStatusChangedContainer itemContainerGeneratorStatusChangedContainer = new ItemContainerGeneratorStatusChangedContainer();
    			itemContainerGeneratorStatusChangedContainer.Item = this;
    			itemContainerGeneratorStatusChangedContainer.Dispatcher = base.Dispatcher;
    			ItemContainerGenerator.StatusChanged += itemContainerGeneratorStatusChangedContainer.StatusChanged;
    		}
    		if (items.Count == 0)
    		{
    			HasItems = false;
    		}
    		else
    		{
    			HasItems = true;
    		}
    	}

    	private int CountFolderItemsInTreeViewRecursive(TagFileListFolder folder)
    	{
    		int num = 0;
    		foreach (TagFileListFile child in folder.Children)
    		{
    			if ((parentTreeView.ItemsSource as TagFileListCollection).Contains(child))
    			{
    				num++;
    				if (child is TagFileListFolder)
    				{
    					num += CountFolderItemsInTreeViewRecursive(child as TagFileListFolder);
    				}
    			}
    		}
    		return num;
    	}

    	private void MultiSelectTreeViewItem_Loaded(object sender, RoutedEventArgs e)
    	{
    		(sender as MultiSelectTreeViewItem).Loaded -= MultiSelectTreeViewItem_Loaded;
    		TagFileListFile tagFileListFile = (sender as MultiSelectTreeViewItem).Content as TagFileListFile;
    		if (!(tagFileListFile is TagFileListFolder))
    		{
    			HasItems = false;
    		}
    		else
    		{
    			(tagFileListFile as TagFileListFolder).ChildrenListChanged += MultiSelectTreeViewItem_ListChanged;
    		}
    		ParentItem = ItemContainerGenerator.ContainerFromItem(tagFileListFile.Parent) as MultiSelectTreeViewItem;
    		if (ParentItem != null)
    		{
    			MultiBinding multiBinding = new MultiBinding();
    			multiBinding.Converter = new MultiSelectTreeViewItemVisibilityConverter();
    			multiBinding.Bindings.Add(new Binding
    			{
    				Source = ParentItem,
    				Path = new PropertyPath(UIElement.VisibilityProperty)
    			});
    			multiBinding.Bindings.Add(new Binding
    			{
    				Source = ParentItem,
    				Path = new PropertyPath(IsExpandedProperty)
    			});
    			SetBinding(UIElement.VisibilityProperty, multiBinding);
    		}
    		object obj = base.Template.FindName("Expander", this);
    		if (obj != null && obj is ToggleButton)
    		{
    			(obj as ToggleButton).Checked += MultiSelectTreeViewItem_Checked;
    			(obj as ToggleButton).Unchecked += MultiSelectTreeViewItem_Unchecked;
    		}
    	}

    	private void MultiSelectTreeViewItem_MouseDoubleClick(object sender, MouseEventArgs e)
    	{
    		object obj = base.Template.FindName("Expander", this);
    		if (HasItems && e.MouseDevice.DirectlyOver != obj as ToggleButton)
    		{
    			IsExpanded = !IsExpanded;
    		}
    	}

    	private void MultiSelectTreeViewItem_Checked(object sender, RoutedEventArgs e)
    	{
    		IsExpanded = true;
    	}

    	private void MultiSelectTreeViewItem_Unchecked(object sender, RoutedEventArgs e)
    	{
    		IsExpanded = false;
    	}

    	private void MultiSelectTreeViewItem_ListChanged(object sender, ListChangedEventArgs e)
    	{
    		RefreshFolder();
    	}

    	private void OnPreviewExpanded()
    	{
    		RoutedEventArgs e = new RoutedEventArgs(PreviewExpandedEvent);
    		RaiseEvent(e);
    	}

    	private void OnPreviewCollapsed()
    	{
    		RoutedEventArgs e = new RoutedEventArgs(PreviewCollapsedEvent);
    		RaiseEvent(e);
    	}

    	private void OnExpanded()
    	{
    		RoutedEventArgs e = new RoutedEventArgs(ExpandedEvent);
    		RaiseEvent(e);
    	}

    	private void OnCollapsed()
    	{
    		RoutedEventArgs e = new RoutedEventArgs(CollapsedEvent);
    		RaiseEvent(e);
    	}
    }
}
