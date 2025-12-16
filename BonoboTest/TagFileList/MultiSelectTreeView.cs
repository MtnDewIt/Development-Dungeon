using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Bonobo.Plugins.TagFileList
{
    public class MultiSelectTreeView : ListBox
    {
    	private TagFileListFile root;

    	public TagFileListFile Root
    	{
    		get
    		{
    			return root;
    		}
    		set
    		{
    			root = value;
    		}
    	}

    	public MultiSelectTreeViewItem RootItem => base.ItemContainerGenerator.ContainerFromItem(Root) as MultiSelectTreeViewItem;

    	static MultiSelectTreeView()
    	{
    		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectTreeView), new FrameworkPropertyMetadata(typeof(MultiSelectTreeView)));
    	}

    	public MultiSelectTreeView()
    	{
    		base.SelectionChanged += MultiSelectTreeView_SelectionChanged;
    	}

    	protected override DependencyObject GetContainerForItemOverride()
    	{
    		return new MultiSelectTreeViewItem(this);
    	}

    	protected override bool IsItemItsOwnContainerOverride(object item)
    	{
    		return item is MultiSelectTreeViewItem;
    	}

    	private void MultiSelectTreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    	{
    		base.SelectionChanged -= MultiSelectTreeView_SelectionChanged;
    		List<object> list = new List<object>();
    		foreach (object selectedItem in base.SelectedItems)
    		{
    			list.Add(selectedItem);
    		}
    		foreach (object item in list)
    		{
                UIElement uIElement = base.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
                if (uIElement == null || uIElement.Visibility == Visibility.Collapsed) 
                {
                    base.SelectedItems.Remove(item);
                }
    		}
    		base.SelectionChanged += MultiSelectTreeView_SelectionChanged;
    	}
    }
}
