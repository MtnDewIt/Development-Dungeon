using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;
using Corinth.Connections;
using Corinth.Datastore;
using Corinth.Linq;
using Corinth.Project;
using Corinth.TicketTrack;
using Corinth.UI.Wpf;
using Microsoft.Practices.Composite.Presentation.Commands;
using TAE.Shared;
using TAE.Shared.Tags;

namespace Bonobo.Plugins.TagFileList
{
    public partial class TagFileListPanel : UserControl, IComponentConnector, IStyleConnector
    {
    	private class TagInfoWithStatus : TagInfo, INotifyPropertyChanged
    	{
    		private SourceControlFileState state;

    		private bool isUpToDate;

    		public SourceControlFileState State
    		{
    			get
    			{
    				return state;
    			}
    			set
    			{
    				state = value;
    				if (this.PropertyChanged != null)
    				{
    					this.PropertyChanged(this, new PropertyChangedEventArgs("CheckedOutToolTip"));
    				}
    			}
    		}

    		public bool IsUpToDate
    		{
    			get
    			{
    				return isUpToDate;
    			}
    			set
    			{
    				isUpToDate = value;
    				if (this.PropertyChanged != null)
    				{
    					this.PropertyChanged(this, new PropertyChangedEventArgs("CheckedOutToolTip"));
    				}
    			}
    		}

    		public bool IsWritable { get; set; }

    		public bool IsGenerated { get; set; }

    		public ICollection<string> CheckedOutClients { get; private set; }

    		public ICollection<string> CheckedOutForScratchClients { get; private set; }

    		public string CheckedOutToolTip
    		{
    			get
    			{
    				string empty = string.Empty;
    				StringBuilder stringBuilder = new StringBuilder();
    				if (!IsUpToDate)
    				{
    					stringBuilder.Append("File not synced to latest!");
    				}
    				if (State == SourceControlFileState.CheckedOutOnThisClient)
    				{
    					stringBuilder.Append("File checked out by you");
    				}
    				if (State == SourceControlFileState.CheckedOutOnThisClientForScratch)
    				{
    					stringBuilder.Append("File checked out for scratch by you");
    				}
    				if (State == SourceControlFileState.MarkedForAdd)
    				{
    					stringBuilder.Append("File is marked for add");
    				}
    				if (CheckedOutClients != null)
    				{
    					stringBuilder.Append("Checked out by:");
    					foreach (string checkedOutClient in CheckedOutClients)
    					{
    						stringBuilder.AppendFormat("{0}{1}", Environment.NewLine, checkedOutClient);
    					}
    				}
    				if (CheckedOutForScratchClients != null)
    				{
    					if (stringBuilder.Length > 0)
    					{
    						stringBuilder.Append("\n");
    					}
    					stringBuilder.Append("Checked out for scratch by:");
    					foreach (string checkedOutForScratchClient in CheckedOutForScratchClients)
    					{
    						stringBuilder.AppendFormat("{0}{1}", Environment.NewLine, checkedOutForScratchClient);
    					}
    				}
    				return stringBuilder.ToString();
    			}
    		}

    		public event PropertyChangedEventHandler PropertyChanged;

    		public TagInfoWithStatus(int id, string tagFullName, string tagName, string type, string extension)
    			: base(id, tagFullName, tagName, type, extension)
    		{
    			((BindingList<string>)(CheckedOutClients = new BindingList<string>())).ListChanged += checkedOutClientsBindingList_ListChanged;
    			((BindingList<string>)(CheckedOutForScratchClients = new BindingList<string>())).ListChanged += checkedOutClientsBindingList_ListChanged;
    		}

    		private void checkedOutClientsBindingList_ListChanged(object sender, ListChangedEventArgs e)
    		{
    			if (this.PropertyChanged != null)
    			{
    				this.PropertyChanged(this, new PropertyChangedEventArgs("CheckedOutToolTip"));
    			}
    		}
    	}

    	private class TagInfoSortItem
    	{
    		public string Name { get; set; }

    		public TagInfoComparer Comparer { get; set; }
    	}

    	private readonly IPluginHost pluginHost;

    	private readonly FileListPlugin plugin;

    	private string tagsDirectory;

    	private readonly string searchNowText;

    	private bool searchReady = false;

    	private bool m_bEverShowedWritableList = false;

    	private readonly TagDatastore tagDatastore = new TagDatastore();

    	private string lastFilter = "";

    	private FileSystemWatcher watcher;

    	private FileSystemWatcher watcherForTagsDirectory;

    	private TagFileTreeHelper tagFileTreeHelper;

    	private DispatcherTimer startSearchTimer;

    	private IEnumerable<Regex> writableFilesToIgnore;

    	public static DependencyProperty IsShowingTagListProperty = DependencyProperty.Register("IsShowingTagList", typeof(bool), typeof(TagFileListPanel));

    	public static DependencyProperty IsShowingFavoritesProperty = DependencyProperty.Register("IsShowingFavorites", typeof(bool), typeof(TagFileListPanel));

    	public static DependencyProperty DoesSearchBoxContainsUserTextProperty = DependencyProperty.Register("DoesSearchBoxContainsUserText", typeof(bool), typeof(TagFileListPanel));

    	public static DependencyProperty IsShowingSearchResultsProperty = DependencyProperty.Register("IsShowingSearchResults", typeof(bool), typeof(TagFileListPanel));

    	public static DependencyProperty IsShowingSearchErrorProperty = DependencyProperty.Register("IsShowingSearchError", typeof(bool), typeof(TagFileListPanel));

    	public static DependencyProperty IsShowingWritableFilesProperty = DependencyProperty.Register("IsShowingWritableFiles", typeof(bool), typeof(TagFileListPanel));

    	public static DependencyProperty IsSearchInProgressProperty = DependencyProperty.Register("IsSearchInProgress", typeof(bool), typeof(TagFileListPanel));

    	public static DependencyProperty IsFindWritableFilesInProgressProperty = DependencyProperty.Register("IsFindWritableFilesInProgress", typeof(bool), typeof(TagFileListPanel));

    	public static DependencyProperty IsFindCheckedOutFilesInProgressProperty = DependencyProperty.Register("IsFindCheckedOutFilesInProgress", typeof(bool), typeof(TagFileListPanel));

    	public static DependencyProperty IsFavoritesInProgressProperty = DependencyProperty.Register("IsFavoritesInProgress", typeof(bool), typeof(TagFileListPanel));

    	private ManualResetEvent m_waitHandle = new ManualResetEvent(initialState: false);

    	public bool IsShowingTagList
    	{
    		get
    		{
    			return (bool)GetValue(IsShowingTagListProperty);
    		}
    		set
    		{
    			SetValue(IsShowingTagListProperty, value);
    		}
    	}

    	public bool IsShowingFavorites
    	{
    		get
    		{
    			return (bool)GetValue(IsShowingFavoritesProperty);
    		}
    		set
    		{
    			SetValue(IsShowingFavoritesProperty, value);
    		}
    	}

    	public bool DoesSearchBoxContainsUserText
    	{
    		get
    		{
    			return (bool)GetValue(DoesSearchBoxContainsUserTextProperty);
    		}
    		set
    		{
    			SetValue(DoesSearchBoxContainsUserTextProperty, value);
    		}
    	}

    	public bool IsShowingSearchResults
    	{
    		get
    		{
    			return (bool)GetValue(IsShowingSearchResultsProperty);
    		}
    		set
    		{
    			SetValue(IsShowingSearchResultsProperty, value);
    		}
    	}

    	public bool IsShowingSearchError
    	{
    		get
    		{
    			return (bool)GetValue(IsShowingSearchErrorProperty);
    		}
    		set
    		{
    			SetValue(IsShowingSearchErrorProperty, value);
    		}
    	}

    	public bool IsShowingWritableFiles
    	{
    		get
    		{
    			return (bool)GetValue(IsShowingWritableFilesProperty);
    		}
    		set
    		{
    			SetValue(IsShowingWritableFilesProperty, value);
    		}
    	}

    	public bool IsSearchInProgress
    	{
    		get
    		{
    			return (bool)GetValue(IsSearchInProgressProperty);
    		}
    		set
    		{
    			SetValue(IsSearchInProgressProperty, value);
    		}
    	}

    	public bool IsFindWritableFilesInProgress
    	{
    		get
    		{
    			return (bool)GetValue(IsFindWritableFilesInProgressProperty);
    		}
    		set
    		{
    			SetValue(IsFindWritableFilesInProgressProperty, value);
    		}
    	}

    	public bool IsFindCheckedOutFilesInProgress
    	{
    		get
    		{
    			return (bool)GetValue(IsFindCheckedOutFilesInProgressProperty);
    		}
    		set
    		{
    			SetValue(IsFindCheckedOutFilesInProgressProperty, value);
    		}
    	}

    	public bool IsFavoritesInProgress
    	{
    		get
    		{
    			return (bool)GetValue(IsFavoritesInProgressProperty);
    		}
    		set
    		{
    			SetValue(IsFavoritesInProgressProperty, value);
    		}
    	}

    	public ICommand DragTagFileListFileCommand { get; protected set; }

    	public ICommand DragTagInfoCommand { get; protected set; }

    	public TagFileListPanel(IPluginHost pluginHost, FileListPlugin plugin)
    	{
    		DragTagFileListFileCommand = (ICommand)new DelegateCommand<UIElement>((Action<UIElement>)DragDropHelper_DragTagFileListFile);
    		DragTagInfoCommand = (ICommand)new DelegateCommand<UIElement>((Action<UIElement>)DragDropHelper_DragTagInfo);
    		InitializeComponent();
    		this.pluginHost = pluginHost;
    		this.plugin = plugin;
    		if (txtFind.Text.Length == 0)
    		{
    			txtFind.Text = "<search for tag>";
    		}
    		searchNowText = txtFind.Text;
    		startSearchTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(350.0), DispatcherPriority.Normal, startSearchTimer_Tick, base.Dispatcher);
    		startSearchTimer.IsEnabled = false;
    		listWritableSorting.ItemsSource = new List<TagInfoSortItem>
    		{
    			new TagInfoSortItem
    			{
    				Name = "Name",
    				Comparer = new TagInfoComparer(new TagSortInfo[2]
    				{
    					new TagSortInfo(TagSortType.TagName, descending: false),
    					new TagSortInfo(TagSortType.TagExtension, descending: false)
    				})
    			},
    			new TagInfoSortItem
    			{
    				Name = "Extension",
    				Comparer = new TagInfoComparer(new TagSortInfo[3]
    				{
    					new TagSortInfo(TagSortType.TagExtension, descending: false),
    					new TagSortInfo(TagSortType.TagName, descending: false),
    					new TagSortInfo(TagSortType.TagFullName, descending: false)
    				})
    			},
    			new TagInfoSortItem
    			{
    				Name = "Full path",
    				Comparer = new TagInfoComparer(new TagSortInfo[1]
    				{
    					new TagSortInfo(TagSortType.TagFullName, descending: false)
    				})
    			}
    		};
    		listWritableSorting.SelectedIndex = 0;
    		listSearchResultsSorting.ItemsSource = new List<TagInfoSortItem>
    		{
    			new TagInfoSortItem
    			{
    				Name = "Extension",
    				Comparer = new TagInfoComparer(new TagSortInfo[3]
    				{
    					new TagSortInfo(TagSortType.TagExtension, descending: false),
    					new TagSortInfo(TagSortType.TagName, descending: false),
    					new TagSortInfo(TagSortType.TagFullName, descending: false)
    				})
    			},
    			new TagInfoSortItem
    			{
    				Name = "Name",
    				Comparer = new TagInfoComparer(new TagSortInfo[2]
    				{
    					new TagSortInfo(TagSortType.TagName, descending: false),
    					new TagSortInfo(TagSortType.TagExtension, descending: false)
    				})
    			},
    			new TagInfoSortItem
    			{
    				Name = "Full path",
    				Comparer = new TagInfoComparer(new TagSortInfo[1]
    				{
    					new TagSortInfo(TagSortType.TagFullName, descending: false)
    				})
    			}
    		};
    		listSearchResultsSorting.SelectedIndex = 0;
    		listFavoritesSorting.ItemsSource = new List<TagInfoSortItem>
    		{
    			new TagInfoSortItem
    			{
    				Name = "Extension",
    				Comparer = new TagInfoComparer(new TagSortInfo[3]
    				{
    					new TagSortInfo(TagSortType.TagExtension, descending: false),
    					new TagSortInfo(TagSortType.TagName, descending: false),
    					new TagSortInfo(TagSortType.TagFullName, descending: false)
    				})
    			},
    			new TagInfoSortItem
    			{
    				Name = "Name",
    				Comparer = new TagInfoComparer(new TagSortInfo[2]
    				{
    					new TagSortInfo(TagSortType.TagName, descending: false),
    					new TagSortInfo(TagSortType.TagExtension, descending: false)
    				})
    			},
    			new TagInfoSortItem
    			{
    				Name = "Full path",
    				Comparer = new TagInfoComparer(new TagSortInfo[1]
    				{
    					new TagSortInfo(TagSortType.TagFullName, descending: false)
    				})
    			}
    		};
    		listFavoritesSorting.SelectedIndex = 0;
    		writableFilesToIgnore = new List<Regex>
    		{
    			new Regex("\\\\local\\\\\\w+\\.scenario_structure_bsp$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
    			new Regex("\\\\local\\\\\\w+\\.scenario_structure_lighting_info$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
    			new Regex("\\\\local\\\\\\w+\\.structure_seams$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    		};
    	}

    	public void SetFilterString(string fileFilter)
    	{
    		txtFind.Text = fileFilter;
    	}

    	private void UserControl_Loaded(object sender, RoutedEventArgs e)
    	{
    		tagsDirectory = ProjectManager.GetCurrentProjectTagsRoot();
    		tagFileTreeHelper = new TagFileTreeHelper(pluginHost, plugin, tree);
    		tagFileTreeHelper.RefreshTree();
    		IsShowingTagList = true;
    		searchReady = true;
    		if (Directory.Exists(tagsDirectory))
    		{
    			SetupWatcher();
    		}
    		else
    		{
    			SetupWatcherForTagsDirectory();
    		}
    		FavoriteTags.Changed += Favorites_Changed;
    	}

    	private void SetupWatcher()
    	{
    		watcher = new FileSystemWatcher(tagsDirectory);
    		watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.LastWrite;
    		watcher.IncludeSubdirectories = true;
    		watcher.Changed += watcher_Changed;
    		watcher.Created += watcher_Changed;
    		watcher.Deleted += watcher_Changed;
    		watcher.EnableRaisingEvents = true;
    	}

    	private void SetupWatcherForTagsDirectory()
    	{
    		watcherForTagsDirectory = new FileSystemWatcher(ProjectManager.GetCurrentProjectRoot());
    		watcherForTagsDirectory.NotifyFilter = NotifyFilters.DirectoryName;
    		watcherForTagsDirectory.Filter = Path.GetFileName(tagsDirectory);
    		watcherForTagsDirectory.Created += watcherForTagsDirectory_Created;
    		watcherForTagsDirectory.EnableRaisingEvents = true;
    	}

    	private void watcherForTagsDirectory_Created(object sender, FileSystemEventArgs e)
    	{
    		SetupWatcher();
    		watcherForTagsDirectory.EnableRaisingEvents = false;
    		watcherForTagsDirectory.Dispose();
    	}

    	private void Favorites_Changed()
    	{
    		if (IsShowingFavorites)
    		{
    			RefreshFavoritesList();
    		}
    	}

    	private void watcher_Changed(object sender, FileSystemEventArgs e)
    	{
    		if (Directory.Exists(e.FullPath))
    		{
    			TagFileListFolder tagFileListFolder = tagFileTreeHelper.FindFileListFolder(Path.GetDirectoryName(e.FullPath));
    			if (tagFileListFolder != null)
    			{
    				bool flag = true;
    				TagFileListFolder tagFileListFolder2 = tagFileListFolder;
    				while (tagFileListFolder2 != null && flag)
    				{
    					flag = tagFileListFolder2.IsExpanded;
    					tagFileListFolder2 = tagFileListFolder2.Parent as TagFileListFolder;
    				}
    				if (flag)
    				{
    					tagFileTreeHelper.RefreshFolder(tagFileListFolder);
    				}
    			}
    		}
    		else
    		{
    			tagFileTreeHelper.RefreshFile(e.FullPath);
    		}
    	}

    	private void tree_PreviewExpanded(object sender, RoutedEventArgs e)
    	{
    		MultiSelectTreeViewItem container = (MultiSelectTreeViewItem)e.OriginalSource;
            TagFileListFolder tagFileListFolder = tree.ItemContainerGenerator.ItemFromContainer(container) as TagFileListFolder;
            if (tagFileListFolder != null && !tagFileListFolder.IsExpanded)
    		{
    			tagFileListFolder.IsExpanded = true;
    			if (Directory.Exists(tagFileListFolder.FullPath))
    			{
    				tagFileTreeHelper.UpdateTreeExpandedStateSettings(tagFileListFolder.FullPath, isExpanded: true);
    				tagFileTreeHelper.RefreshFolder(tagFileListFolder);
    			}
    		}
    	}

    	private void tree_PreviewCollapsed(object sender, RoutedEventArgs e)
    	{
    		MultiSelectTreeViewItem multiSelectTreeViewItem = (MultiSelectTreeViewItem)e.OriginalSource;
            TagFileListFolder tagFileListFolder = tree.ItemContainerGenerator.ItemFromContainer(multiSelectTreeViewItem) as TagFileListFolder;
            if (tagFileListFolder != null && tagFileListFolder.IsExpanded)
    		{
    			tagFileListFolder.IsExpanded = false;
    			tagFileListFolder.Children.Clear();
    			multiSelectTreeViewItem.HasItems = true;
    			multiSelectTreeViewItem.HasExpanded = false;
    			tagFileTreeHelper.UpdateTreeExpandedStateSettings(tagFileListFolder.FullPath, isExpanded: false);
    		}
    	}

    	private void tree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    	{
    		MultiSelectTreeViewItem parentTreeViewItem = GetParentTreeViewItem(e.OriginalSource as DependencyObject);
    		if (parentTreeViewItem == null)
    		{
    			e.Handled = true;
    			return;
    		}
    		TagFileListFile tagFileListFile = (TagFileListFile)parentTreeViewItem.Content;
    		if (tagFileListFile != null && !(tagFileListFile is TagFileListFolder))
    		{
    			ShowTag(tagFileListFile.FullPath, null, 0);
    		}
    	}

    	private void tree_KeyDown(object sender, KeyEventArgs e)
    	{
    		if (e.Key == Key.Return || e.Key == Key.Return)
    		{
    			List<TagFileListFile> list = new List<TagFileListFile>();
    			foreach (TagFileListFile selectedItem in tree.SelectedItems)
    			{
    				list.Add(selectedItem);
    			}
    			foreach (TagFileListFile item in list)
    			{
    				if (tree.ItemContainerGenerator.ContainerFromItem(item) is MultiSelectTreeViewItem multiSelectTreeViewItem)
    				{
    					if (item is TagFileListFolder)
    					{
    						multiSelectTreeViewItem.IsExpanded = !multiSelectTreeViewItem.IsExpanded;
    					}
    					else
    					{
    						ShowTag(item.FullPath, null, 0);
    					}
    				}
    			}
    			e.Handled = true;
    		}
    		else
    		{
    			if (e.Key != Key.C || (e.KeyboardDevice.Modifiers & ModifierKeys.Control) <= ModifierKeys.None)
    			{
    				return;
    			}
    			List<string> list2 = new List<string>();
    			foreach (TagFileListFile item2 in tree.SelectedItems.OfType<TagFileListFile>())
    			{
    				ITagInformation tagInformation = pluginHost.FindSingleInterface<ITagInformation>();
    				list2.Add(tagInformation.GetRelativePathWithExtension(item2.FullPath));
    			}
    			if (list2.Count > 0)
    			{
    				ClipboardWrapper.SetText(string.Join("\r\n", list2.ToArray()));
    			}
    			e.Handled = true;
    		}
    	}

    	private void tree_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    	{
    		MultiSelectTreeViewItem parentTreeViewItem = GetParentTreeViewItem(e.OriginalSource as DependencyObject);
    		if (parentTreeViewItem == null)
    		{
    			e.Handled = true;
    			return;
    		}
    		if (!tree.SelectedItems.Contains((TagFileListFile)parentTreeViewItem.Content))
    		{
    			tree.SelectedItems.Add((TagFileListFile)parentTreeViewItem.Content);
    		}
    		List<TagFileListFile> list = new List<TagFileListFile>();
    		foreach (TagFileListFile selectedItem in tree.SelectedItems)
    		{
    			list.Add(selectedItem);
    		}
    		if (list.Count > 0 && !ShowContextMenu(list, tree.ContextMenu))
    		{
    			e.Handled = true;
    		}
    	}

    	public bool FindInTreeView(string fullPath)
    	{
    		ITagInformation tagInformation = pluginHost.FindSingleInterface<ITagInformation>();
    		string relativePathWithExtension = tagInformation.GetRelativePathWithExtension(fullPath);
    		if (!string.IsNullOrEmpty(relativePathWithExtension))
    		{
    			tabControl.SelectedItem = tabFileTree;
    			tagFileTreeHelper.ExpandPath(relativePathWithExtension, fullPath);
    			return true;
    		}
    		return false;
    	}

    	private void CommandRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
    	{
    		if (!IsShowingSearchResults)
    		{
    			pluginHost.FindSingleInterface<ISourceControlProvider>().RefreshAvailability();
    			tagFileTreeHelper.RefreshTree();
    		}
    	}

    	private static FrameworkElement GetParentOfType(DependencyObject child, Type type, Type typeToEndWith)
    	{
    		if (child == null || typeToEndWith.GetType().IsAssignableFrom(child.GetType()))
    		{
    			return null;
    		}
    		if (type.GetType().IsAssignableFrom(child.GetType()))
    		{
    			return (FrameworkElement)child;
    		}
    		return GetParentOfType(VisualTreeHelper.GetParent(child), type, typeToEndWith);
    	}

    	private static MultiSelectTreeViewItem GetParentTreeViewItem(DependencyObject child)
    	{
    		if (child == null || child is MultiSelectTreeView)
    		{
    			return null;
    		}
    		if (child is MultiSelectTreeViewItem)
    		{
    			return child as MultiSelectTreeViewItem;
    		}
    		return GetParentTreeViewItem(VisualTreeHelper.GetParent(child));
    	}

    	private static ListBoxItem GetParentListBoxItem(DependencyObject child)
    	{
    		if (child == null || child is ListBox)
    		{
    			return null;
    		}
    		if (child is ListBoxItem)
    		{
    			return child as ListBoxItem;
    		}
    		return GetParentListBoxItem(VisualTreeHelper.GetParent(child));
    	}

    	private void ShowTag(string fileName, IFileAction action, int? uiGroupIndex)
    	{
    		if (File.Exists(fileName))
    		{
    			ITagViewer tagViewer = pluginHost.FindSingleInterface<ITagViewer>();
    			if (tagViewer != null)
    			{
    				if (action != null)
    				{
    					action.Invoke();
    				}
    				else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
    				{
    					tagViewer.ShowTagFileInAlternateView(fileName, fileExists: true, uiGroupIndex);
    				}
    				else
    				{
    					tagViewer.ShowTagFile(fileName, fileExists: true, uiGroupIndex);
    				}
    			}
    		}
    		else
    		{
    			MessageBox.Show("This file doesn't exist on your computer.  Try refreshing the file list or syncing the file from the depot.");
    		}
    	}

    	private void txtFind_GotFocus(object sender, RoutedEventArgs e)
    	{
    		if (txtFind.Text == searchNowText)
    		{
    			DoesSearchBoxContainsUserText = true;
    			txtFind.Text = "";
    		}
    	}

    	private void txtFind_LostFocus(object sender, RoutedEventArgs e)
    	{
    		if (txtFind.Text.Trim().Length == 0)
    		{
    			ClearSearch();
    		}
    	}

    	private void ClearSearch()
    	{
    		DoesSearchBoxContainsUserText = false;
    		txtFind.Text = searchNowText;
    	}

    	private void txtFind_TextChanged(object sender, TextChangedEventArgs e)
    	{
    		if (startSearchTimer != null && searchReady)
    		{
    			IsShowingSearchResults = txtFind.Text.Trim().Length > 0 && txtFind.Text != searchNowText;
    			IsShowingTagList = !IsShowingSearchResults;
    			IsShowingSearchError = false;
    			IsSearchInProgress = true;
    			if (IsShowingSearchResults)
    			{
    				tabControl.SelectedItem = tabSearchResults;
    			}
    			SetSearchResults(null, lastFilter, forceRefresh: true);
    			startSearchTimer.Stop();
    			startSearchTimer.Start();
    		}
    	}

    	private void startSearchTimer_Tick(object sender, EventArgs args)
    	{
    		startSearchTimer.Stop();
    		StartSearch();
    	}

    	private void StartSearch()
    	{
    		List<string[]> list = new List<string[]>();
    		string text = txtFind.Text.ToLower();
    		string[] array = text.Split('|');
    		string[] array2 = array;
    		foreach (string text2 in array2)
    		{
    			list.Add(text2.Trim().Split(' '));
    		}
    		lastFilter = text;
    		if (!string.IsNullOrEmpty(text))
    		{
    			tagDatastore.BeginGetTagsWithAdvancedFilter(list.ToArray(), delegate(IAsyncResult ar)
    			{
    				TagInfo[] searchResults = tagDatastore.EndGetTagsWithAdvancedFilter(ar);
    				if (searchResults != null)
    				{
    					ITagInformation tagInformation = pluginHost.FindSingleInterface<ITagInformation>();
    					if (tagInformation != null)
    					{
    						List<TagInfo> list2 = new List<TagInfo>();
    						TagInfo[] array3 = searchResults;
    						foreach (TagInfo tagInfo in array3)
    						{
    							if (tagInformation.IsTagExtensionValid(tagInfo.Extension))
    							{
    								list2.Add(tagInfo);
    							}
    						}
    						searchResults = list2.ToArray();
    					}
    				}
    				base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    				{
    					SetSearchResults(searchResults, (string)ar.AsyncState, forceRefresh: false);
    				});
    				base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    				{
    					IsSearchInProgress = false;
    				});
    			}, text);
    		}
    		else
    		{
    			SetSearchResults(null, text, forceRefresh: true);
    			IsSearchInProgress = false;
    		}
    	}

    	private void SetSearchResults(TagInfo[] results, string filterUsed, bool forceRefresh)
    	{
    		if (!forceRefresh && filterUsed != lastFilter)
    		{
    			return;
    		}
    		if (results != null)
    		{
    			TagInfoComparer comparer = ((TagInfoSortItem)listSearchResultsSorting.SelectedItem).Comparer;
    			Array.Sort(results, comparer);
    			listSearchResults.ItemsSource = null;
    			listSearchResults.ItemsSource = results;
    			if (listSearchResults.Items.Count > 0)
    			{
    				listSearchResults.ScrollIntoView(listSearchResults.Items[0]);
    			}
    		}
    		else
    		{
    			listSearchResults.ItemsSource = null;
    		}
    	}

    	private void SetWritableResults(TagInfo[] results)
    	{
    		if (results != null)
    		{
    			TagInfoComparer comparer = ((TagInfoSortItem)listWritableSorting.SelectedItem).Comparer;
    			Array.Sort(results, comparer);
    			listWritableFiles.ItemsSource = results.Where((TagInfo a) => !writableFilesToIgnore.Any((Regex regex) => regex.IsMatch(a.RelativePathWithExtension)));
    			CollectionViewSource.GetDefaultView(results).Refresh();
    			if (listWritableFiles.Items.Count > 0)
    			{
    				listWritableFiles.ScrollIntoView(listWritableFiles.Items[0]);
    			}
    		}
    		else
    		{
    			listWritableFiles.ItemsSource = null;
    		}
    	}

    	private void SetCheckedOutResults(TagInfo[] results)
    	{
    		if (results != null)
    		{
    			TagInfoComparer comparer = ((TagInfoSortItem)listWritableSorting.SelectedItem).Comparer;
    			Array.Sort(results, comparer);
    			listCheckedOutFiles.ItemsSource = results;
    			CollectionViewSource.GetDefaultView(results).Refresh();
    			if (listCheckedOutFiles.Items.Count > 0)
    			{
    				listCheckedOutFiles.ScrollIntoView(listCheckedOutFiles.Items[0]);
    			}
    		}
    		else
    		{
    			listCheckedOutFiles.ItemsSource = null;
    		}
    	}

    	private void SetFavoritesResults(TagInfo[] results)
    	{
    		if (results != null)
    		{
    			TagInfoComparer comparer = ((TagInfoSortItem)listFavoritesSorting.SelectedItem).Comparer;
    			Array.Sort(results, comparer);
    			listFavorites.ItemsSource = null;
    			listFavorites.ItemsSource = results;
    			if (listFavorites.Items.Count > 0)
    			{
    				listFavorites.ScrollIntoView(listFavorites.Items[0]);
    			}
    		}
    		else
    		{
    			listFavorites.ItemsSource = null;
    		}
    	}

    	private void tagInfoList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    	{
    		ListBox listBox = (ListBox)sender;
    		if (listBox.SelectedItem != null && e.ChangedButton == MouseButton.Left)
    		{
    			TagInfo tagInfo = (TagInfo)listBox.SelectedItem;
    			if (tagInfo != null)
    			{
    				string currentProjectTagsRoot = ProjectManager.GetCurrentProjectTagsRoot();
    				string fileName = Path.Combine(currentProjectTagsRoot, tagInfo.RelativePathWithExtension);
    				ShowTag(fileName, null, 0);
    			}
    		}
    	}

    	private void tagInfoList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    	{
    		ListBox listBox = (ListBox)sender;
    		ListBoxItem parentListBoxItem = GetParentListBoxItem(e.OriginalSource as DependencyObject);
    		if (parentListBoxItem == null)
    		{
    			e.Handled = true;
    			return;
    		}
    		if (!listBox.SelectedItems.Contains((TagInfo)parentListBoxItem.Content))
    		{
    			listBox.SelectedItems.Add((TagInfo)parentListBoxItem.Content);
    		}
    		if (listBox.SelectedItems.Count <= 0)
    		{
    			return;
    		}
    		List<string> list = new List<string>();
    		string currentProjectTagsRoot = ProjectManager.GetCurrentProjectTagsRoot();
    		foreach (TagInfo selectedItem in listBox.SelectedItems)
    		{
    			list.Add(Path.Combine(currentProjectTagsRoot, selectedItem.RelativePathWithExtension));
    		}
    		if (!ShowContextMenu(list, listBox.ContextMenu))
    		{
    			e.Handled = true;
    		}
    	}

    	private bool ShowContextMenu(IEnumerable<TagFileListFile> selectedFiles, ContextMenu contextMenu)
    	{
    		contextMenu.Items.Clear();
    		List<FileActionParameters> list = new List<FileActionParameters>();
    		foreach (TagFileListFile selectedFile in selectedFiles)
    		{
    			list.Add(new FileActionParameters
    			{
    				FileName = selectedFile.FullPath,
    				IsFolder = (selectedFile is TagFileListFolder),
    				FileExists = ((selectedFile is TagFileListFolder) ? Directory.Exists(selectedFile.FullPath) : File.Exists(selectedFile.FullPath)),
    				FileState = selectedFile.State,
    				IsUpToDate = selectedFile.IsUpToDate,
    				IsTagFile = true,
    				IsWritable = selectedFile.IsWritable
    			});
    		}
    		IFileActionController fileActionController = pluginHost.FindSingleInterface<IFileActionController>();
    		IEnumerable<IFileAction> enumerable = fileActionController.AddFileActionsToMenu(contextMenu, list);
    		foreach (IFileAction item in enumerable)
    		{
    			item.FileActionStarting.Subscribe(FileActionStarting);
    			item.FileActionFinished.Subscribe(FileActionFinished);
    		}
    		return contextMenu.Items.Count > 0;
    	}

    	private bool ShowContextMenu(IEnumerable<string> selectedFiles, ContextMenu contextMenu)
    	{
    		contextMenu.Items.Clear();
    		List<FileActionParameters> list = new List<FileActionParameters>();
    		foreach (string selectedFile in selectedFiles)
    		{
    			list.Add(new FileActionParameters
    			{
    				FileName = selectedFile,
    				IsFolder = false,
    				FileExists = File.Exists(selectedFile),
    				IsTagFile = true,
    				IsWritable = (File.Exists(selectedFile) && (File.GetAttributes(selectedFile) & FileAttributes.ReadOnly) == 0),
    				IsUpToDate = true
    			});
    		}
    		IFileActionController fileActionController = pluginHost.FindSingleInterface<IFileActionController>();
    		IEnumerable<IFileAction> enumerable = fileActionController.AddFileActionsToMenu(contextMenu, list);
    		foreach (IFileAction item in enumerable)
    		{
    			item.FileActionStarting.Subscribe(FileActionStarting);
    			item.FileActionFinished.Subscribe(FileActionFinished);
    		}
    		return contextMenu.Items.Count > 0;
    	}

    	private void FileActionStarting(FileActionEventArgs args)
    	{
    		tagFileTreeHelper.FileActionStarting(args.FilePaths);
    	}

    	private void FileActionFinished(FileActionEventArgs args)
    	{
    		tagFileTreeHelper.FileActionFinished(args.FilePaths);
    		tagFileTreeHelper.RefreshFiles(args.FilePaths);
    		if (IsShowingWritableFiles)
    		{
    			RefreshWritableList();
    		}
    	}

    	public void AddFileActions(IEnumerable<FileActionParameters> fileActionParamsList, List<IFileAction> actions)
    	{
    		IEnumerable<string> enumerable = fileActionParamsList.Select((FileActionParameters a) => a.FileName);
    		IEnumerable<string> enumerable2 = from a in fileActionParamsList
    			where !a.IsFolder
    			select a.FileName;
    		bool flag = fileActionParamsList.Any((FileActionParameters a) => !a.IsFolder && a.IsTagFile);
    		if (flag)
    		{
    			List<string> list = new List<string>();
    			List<string> list2 = new List<string>();
    			foreach (string item in enumerable2)
    			{
    				string text = FavoriteTags.Unify(item);
    				if (!FavoriteTags.ContainsTag(text))
    				{
    					list.Add(text);
    				}
    				else
    				{
    					list2.Add(text);
    				}
    			}
    			if (list.Count > 0)
    			{
    				actions.Add(new AddFavoritesAction(pluginHost, list));
    			}
    			if (list2.Count > 0)
    			{
    				actions.Add(new RemoveFavoritesAction(pluginHost, list2));
    			}
    		}
    		FileActionParameters[] filesThatNeedSourceControlState = fileActionParamsList.Where((FileActionParameters file) => !file.FileState.HasValue).ToArray();
    		if (filesThatNeedSourceControlState.Length != 0)
    		{
    			ISourceControlProvider sourceControlProvider = pluginHost.FindSingleInterface<ISourceControlProvider>();
    			IEnumerable<SourceControlFile> fileStates = sourceControlProvider.GetFileStates(filesThatNeedSourceControlState.Select((FileActionParameters file) => file.FileName));
    			int i;
    			for (i = 0; i < filesThatNeedSourceControlState.Length; i++)
    			{
    				SourceControlFile sourceControlFile = fileStates.SingleOrDefault((SourceControlFile a) => a.FileName.Equals(filesThatNeedSourceControlState[i].FileName, StringComparison.InvariantCultureIgnoreCase));
    				if (sourceControlFile != null)
    				{
    					filesThatNeedSourceControlState[i].FileState = sourceControlFile.State;
    					filesThatNeedSourceControlState[i].IsUpToDate = sourceControlFile.IsUpToDate;
    				}
    				else
    				{
    					filesThatNeedSourceControlState[i].FileState = SourceControlFileState.NotInDepot;
    					filesThatNeedSourceControlState[i].IsUpToDate = true;
    				}
    			}
    		}
    		if (enumerable2.Any() && !flag)
    		{
    			actions.Add(new OpenInDefaultApplicationAction(pluginHost, enumerable2));
    		}
    		if (fileActionParamsList.Count() == 1 && enumerable2.Count() == 1)
    		{
    			actions.Add(new ShowFileDiffAction(pluginHost, enumerable2));
    			string filename = string.Join(Environment.NewLine, enumerable2.ToArray());
    			string[] array = ((TagPath.FromFilename(filename) != null) ? TagPath.FromFilename(filename).ToString().Split('.') : null);
    			if (array != null && array.Count() > 1 && array[1].CompareTo("cui_screen") == 0)
    			{
    				actions.Add(new UpdateCuiTemplateUsersAction(pluginHost, enumerable2));
    				actions.Add(new CuiTextAnalyzerAction(pluginHost, enumerable2));
    				actions.Add(new CuiWidgetFinderAction(pluginHost, enumerable2));
    				actions.Add(new CuiOnDemandBitmapFinder(pluginHost, enumerable2));
    			}
    		}
    		if (flag && fileActionParamsList.Count() == 1 && !fileActionParamsList.First().IsFolder)
    		{
    			actions.Add(new FindInTreeViewAction(pluginHost, this, enumerable));
    		}
    		actions.Add(new OpenExplorerHereAction(pluginHost, enumerable));
    		if (flag)
    		{
    			actions.Add(new XSyncFileAction(pluginHost, enumerable2));
    			actions.Add(new ForceXSyncFileAction(pluginHost, enumerable2));
    		}
    		if (fileActionParamsList.Count() != 1)
    		{
    			return;
    		}
    		Assert.Check(enumerable.Count() == 1);
    		if (fileActionParamsList.First().IsFolder)
    		{
    			Assert.Check(enumerable2.Count() == 0);
    			actions.Add(new NewFolderHereAction(pluginHost, enumerable));
    			return;
    		}
    		Assert.Check(enumerable2.Count() == 1);
    		actions.Add(new ShowFileHistoryAction(pluginHost, enumerable2));
    		if (flag)
    		{
    			IBonoboApplication bonoboApplication = pluginHost.FindSingleInterface<IBonoboApplication>();
    			actions.Add(new ExploreRelatedContentAction(pluginHost, enumerable2));
    			string path = enumerable2.First();
    			string extension = Path.GetExtension(path);
    			if (extension.Length != 0 && Asset.IsValidAssetType(extension.Substring(1).ToLower()))
    			{
    				actions.Add(new DisplayMemoryFootprintAction(pluginHost, enumerable2));
    				actions.Add(new RunAssetAnalysisAction(pluginHost, enumerable2));
    			}
    		}
    	}

    	private void txtFind_KeyDown(object sender, KeyEventArgs e)
    	{
    		if (e.Key == Key.Return || e.Key == Key.Return)
    		{
    			if (IsShowingSearchResults && listSearchResults.Items.Count > 0)
    			{
    				TagInfo tagInfo = (TagInfo)listSearchResults.Items[0];
    				string currentProjectTagsRoot = ProjectManager.GetCurrentProjectTagsRoot();
    				string fileName = Path.Combine(currentProjectTagsRoot, tagInfo.RelativePathWithExtension);
    				ShowTag(fileName, null, 0);
    			}
    			e.Handled = true;
    		}
    		else if (e.Key == Key.Down)
    		{
    			if (IsShowingSearchResults && listSearchResults.HasItems)
    			{
    				listSearchResults.SelectedIndex = 0;
    				listSearchResults.ScrollIntoView(listSearchResults.SelectedItem);
    				((FrameworkElement)listSearchResults.ItemContainerGenerator.ContainerFromItem(listSearchResults.SelectedItem)).Focus();
    			}
    			e.Handled = true;
    		}
    		else if (e.Key == Key.Up)
    		{
    			if (IsShowingSearchResults && listSearchResults.HasItems)
    			{
    				listSearchResults.SelectedIndex = listSearchResults.Items.Count - 1;
    				listSearchResults.ScrollIntoView(listSearchResults.SelectedItem);
    				((FrameworkElement)listSearchResults.ItemContainerGenerator.ContainerFromItem(listSearchResults.SelectedItem)).Focus();
    			}
    			e.Handled = true;
    		}
    		else if (e.Key == Key.Escape)
    		{
    			txtFind.Text = "";
    			tabControl.SelectedItem = tabFileTree;
    			e.Handled = true;
    		}
    	}

    	private void tagInfoList_KeyDown(object sender, KeyEventArgs e)
    	{
    		ListBox listBox = (ListBox)sender;
    		if (e.Key == Key.Return || e.Key == Key.Return)
    		{
    			List<TagInfo> list = new List<TagInfo>(listBox.SelectedItems.OfType<TagInfo>());
    			string currentProjectTagsRoot = ProjectManager.GetCurrentProjectTagsRoot();
    			foreach (TagInfo item in list)
    			{
    				string fileName = Path.Combine(currentProjectTagsRoot, item.RelativePathWithExtension);
    				ShowTag(fileName, null, 0);
    			}
    			e.Handled = true;
    		}
    		else
    		{
    			if (e.Key != Key.C || (e.KeyboardDevice.Modifiers & ModifierKeys.Control) <= ModifierKeys.None)
    			{
    				return;
    			}
    			List<string> list2 = new List<string>();
    			foreach (TagInfo selectedItem in listBox.SelectedItems)
    			{
    				list2.Add(selectedItem.RelativePathWithExtension);
    			}
    			if (list2.Count > 0)
    			{
    				ClipboardWrapper.SetText(string.Join("\r\n", list2.ToArray()));
    			}
    			e.Handled = true;
    		}
    	}

    	private void buttonFindClear_Click(object sender, RoutedEventArgs e)
    	{
    		ClearSearch();
    		tabControl.SelectedItem = tabFileTree;
    	}

    	private void ButtonFindInTreeView_Click(object sender, RoutedEventArgs e)
    	{
    		Button button = (Button)e.OriginalSource;
    		TagInfo tagInfo = (TagInfo)button.Tag;
    		string currentProjectTagsRoot = ProjectManager.GetCurrentProjectTagsRoot();
    		string fullPath = Path.Combine(currentProjectTagsRoot, tagInfo.RelativePathWithExtension);
    		if (!FindInTreeView(fullPath))
    		{
    			button.IsEnabled = false;
    		}
    		e.Handled = true;
    	}

    	private void DragDropHelper_DragTagFileListFile(UIElement dragElement)
    	{
    		IEnumerable<TagFileListFile> source = (from object item in tree.SelectedItems
    			where item is TagFileListFile && !(item is TagFileListFolder)
    			select item).Cast<TagFileListFile>();
    		if (source.Any())
    		{
    			DoDrag(source.Select((TagFileListFile file) => file.FullPath).ToArray());
    		}
    	}

    	private void DragDropHelper_DragTagInfo(UIElement dragElement)
    	{
    		ListBox parent = WpfHelper.GetParent<ListBox>(dragElement as FrameworkElement);
    		if (parent == null)
    		{
    			return;
    		}
    		string tagsDirectory = ProjectManager.GetCurrentProjectTagsRoot();
    		IEnumerable<TagInfo> source = (from object item in parent.SelectedItems
    			where item is TagInfo
    			select item).Cast<TagInfo>();
    		if (source.Any())
    		{
    			DoDrag(source.Select((TagInfo tagInfo) => Path.Combine(tagsDirectory, tagInfo.RelativePathWithExtension)).ToArray());
    		}
    	}

    	private void DoDrag(string[] fileNames)
    	{
    		if (fileNames.Length != 0)
    		{
    			StringBuilder stringBuilder = new StringBuilder();
    			foreach (string value in fileNames)
    			{
    				stringBuilder.AppendLine(value);
    			}
    			IDragDropHandler dragDropHandler = pluginHost.FindSingleInterface<IDragDropHandler>();
    			if (dragDropHandler != null)
    			{
    				CaptureMouse();
    				Dictionary<DragDropTypes, object> dictionary = new Dictionary<DragDropTypes, object>();
    				dictionary[DragDropTypes.FileDrop] = fileNames;
    				dictionary[DragDropTypes.InternalReference] = fileNames[0];
    				dictionary[DragDropTypes.Text] = stringBuilder.ToString();
    				dragDropHandler.DragDataObject(this, dictionary);
    				ReleaseMouseCapture();
    			}
    		}
    	}

    	private void buttonHelp_Click(object sender, RoutedEventArgs e)
    	{
    		popupHelp.IsOpen = !popupHelp.IsOpen;
    		buttonHelp.CaptureMouse();
    	}

    	private void buttonHelp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    	{
    		if (buttonHelp.IsMouseCaptured)
    		{
    			popupHelp.IsOpen = false;
    			buttonHelp.ReleaseMouseCapture();
    			e.Handled = true;
    		}
    	}

    	private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    	{
    		if (e.OriginalSource == tabControl)
    		{
    			IsShowingSearchResults = tabControl.SelectedItem == tabSearchResults;
    			IsShowingTagList = tabControl.SelectedItem == tabFileTree;
    			IsShowingWritableFiles = tabControl.SelectedItem == tabWritable;
    			IsShowingFavorites = tabControl.SelectedItem == tabFavorites;
    			if (IsShowingWritableFiles)
    			{
    				RefreshWritableList();
    			}
    			if (IsShowingFavorites)
    			{
    				RefreshFavoritesList();
    			}
    		}
    	}

    	public void SearchForTag()
    	{
    		txtFind.Focus();
    	}

    	private void tree_SelectionChanged(object sender, SelectionChangedEventArgs e)
    	{
    		if (tree.SelectedItems.Count > 0)
    		{
    			TagFileListFile tagFileListFile = (TagFileListFile)tree.SelectedItem;
    			tagFileTreeHelper.SaveLastSelectedFileSetting(tagFileListFile.FullPath);
    		}
    	}

    	private void RefreshFavoritesList()
    	{
    		lock (m_waitHandle)
    		{
    			IsFavoritesInProgress = true;
    			Stopwatch timer = Stopwatch.StartNew();
    			List<string> favorites = FavoriteTags.TagList;
    			ThreadPool.QueueUserWorkItem(delegate
    			{
    				long elapsedMilliseconds = timer.ElapsedMilliseconds;
    				List<TagInfo> localTags = new List<TagInfo>();
    				List<string> list = ((favorites != null) ? new List<string>(favorites) : new List<string>());
    				for (int i = 0; i < list.Count; i++)
    				{
    					string path = list[i];
    					string extension = Path.GetExtension(path).Substring(1);
    					string directoryName = Path.GetDirectoryName(path);
    					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
    					string tagName = Path.Combine(directoryName, fileNameWithoutExtension);
    					m_waitHandle.Reset();
    					tagDatastore.BeginGetTagInfo(tagName, extension, delegate(IAsyncResult getTagAsyncResult)
    					{
    						TagInfo searchResult = tagDatastore.EndGetTagInfo(getTagAsyncResult);
    						if (searchResult != null)
    						{
    							base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    							{
    								localTags.Add(searchResult);
    								m_waitHandle.Set();
    							});
    						}
    					}, null);
    					m_waitHandle.WaitOne();
    				}
    				base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    				{
    					SetFavoritesResults((localTags.Count > 0) ? localTags.ToArray() : null);
    				});
    				base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    				{
    					IsFavoritesInProgress = false;
    				});
    			});
    		}
    	}

    	private void RefreshWritableList()
    	{
    		if (!m_bEverShowedWritableList)
    		{
    			IsFindWritableFilesInProgress = true;
    			IsFindCheckedOutFilesInProgress = true;
    			SetWritableResults(null);
    			SetCheckedOutResults(null);
    		}
    		Stopwatch timer = Stopwatch.StartNew();
    		ThreadPool.QueueUserWorkItem(delegate
    		{
    			long elapsedMilliseconds = timer.ElapsedMilliseconds;
    			Thread.Sleep(500);
    			tagDatastore.BeginGetWritableTags(delegate(IAsyncResult getWritableTagsAsyncResult)
    			{
    				long elapsedMilliseconds2 = timer.ElapsedMilliseconds;
    				TagInfo[] array = tagDatastore.EndGetWritableTags(getWritableTagsAsyncResult);
    				int num = array.Count();
    				int num2 = num;
    				if (array != null)
    				{
    					ITagInformation tagInformation = pluginHost.FindSingleInterface<ITagInformation>();
    					if (tagInformation != null)
    					{
    						List<TagInfo> list = new List<TagInfo>();
    						TagInfo[] array2 = array;
    						foreach (TagInfo tagInfo in array2)
    						{
    							if (tagInformation.IsTagExtensionValid(tagInfo.Extension))
    							{
    								list.Add(tagInfo);
    							}
    						}
    						array = list.ToArray();
    						num2 = array.Count();
    					}
    				}
    				long elapsedMilliseconds3 = timer.ElapsedMilliseconds;
    				ISourceControlProvider sourceControlProvider = pluginHost.FindSingleInterface<ISourceControlProvider>();
    				string tagsDirectory = ProjectManager.GetCurrentProjectTagsRoot();
    				IEnumerable<SourceControlFile> source = new SourceControlFile[0];
    				if (array.Count() > 0)
    				{
    					source = sourceControlProvider.GetFileStates(array.Select((TagInfo a) => Path.Combine(tagsDirectory, a.RelativePathWithExtension)).ToArray());
    				}
    				List<TagInfo> writableResults = new List<TagInfo>();
    				List<TagInfo> checkedOutResults = new List<TagInfo>();
    				foreach (TagInfo tagInfo2 in array)
    				{
    					string searchResultPath = Path.Combine(tagsDirectory, tagInfo2.RelativePathWithExtension);
    					SourceControlFile sourceControlFile = source.Where((SourceControlFile f) => f.FileName.Equals(searchResultPath, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
    					TagInfoWithStatus tagInfoWithStatus = new TagInfoWithStatus(tagInfo2.Id, tagInfo2.RelativePath, tagInfo2.ShortName, tagInfo2.TagType, tagInfo2.Extension)
    					{
    						IsWritable = true,
    						IsGenerated = false,
    						IsUpToDate = true
    					};
    					if (sourceControlFile != null)
    					{
    						tagInfoWithStatus.State = sourceControlFile.State;
    						tagInfoWithStatus.IsUpToDate = sourceControlFile.IsUpToDate;
    						if (sourceControlFile.CheckedOutClients != null)
    						{
    							tagInfoWithStatus.CheckedOutClients.AddRange(sourceControlFile.CheckedOutClients);
    						}
    						if (sourceControlFile.CheckedOutForScratchClients != null)
    						{
    							tagInfoWithStatus.CheckedOutForScratchClients.AddRange(sourceControlFile.CheckedOutForScratchClients);
    						}
    					}
    					else
    					{
    						tagInfoWithStatus.State = SourceControlFileState.NotInDepot;
    					}
    					if (tagInfoWithStatus.State == SourceControlFileState.CheckedOutOnThisClient || tagInfoWithStatus.State == SourceControlFileState.CheckedOutOnThisClientForScratch || tagInfoWithStatus.State == SourceControlFileState.MarkedForAdd)
    					{
    						checkedOutResults.Add(tagInfoWithStatus);
    					}
    					else
    					{
    						writableResults.Add(tagInfoWithStatus);
    					}
    				}
    				long elapsedMilliseconds4 = timer.ElapsedMilliseconds;
    				if (m_bEverShowedWritableList)
    				{
    					base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    					{
    						IsFindWritableFilesInProgress = true;
    						IsFindCheckedOutFilesInProgress = true;
    						SetWritableResults(null);
    						SetCheckedOutResults(null);
    						Thread.Sleep(100);
    					});
    				}
    				base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    				{
    					SetWritableResults(writableResults.ToArray());
    					SetCheckedOutResults(checkedOutResults.ToArray());
    				});
    				base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    				{
    					IsFindWritableFilesInProgress = false;
    					IsFindCheckedOutFilesInProgress = false;
    					m_bEverShowedWritableList = true;
    				});
    				UsageDataClient.AddEventFireAndForget("list writable files", $"{num} results, {num2} filtered, {writableResults.Count} writable, {checkedOutResults.Count} checked out");
    			}, null);
    		});
    	}

    	private void writableGridSplitter_Loaded(object sender, RoutedEventArgs e)
    	{
    		if (plugin.Settings.WritableSplitterRatio == 0.0)
    		{
    			plugin.Settings.WritableSplitterRatio = 1.0;
    		}
    		double num = 7.0;
    		double value = Math.Max(1.0 / num, Math.Min(num, plugin.Settings.WritableSplitterRatio));
    		writableGrid.RowDefinitions[0].Height = new GridLength(1.0, GridUnitType.Star);
    		writableGrid.RowDefinitions[2].Height = new GridLength(value, GridUnitType.Star);
    		listCheckedOutFiles.Focus();
    	}

    	private void writableGridSplitter_Unloaded(object sender, RoutedEventArgs e)
    	{
    		plugin.Settings.WritableSplitterRatio = writableGrid.RowDefinitions[2].Height.Value / writableGrid.RowDefinitions[0].Height.Value;
    		plugin.SaveSettings();
    	}

    	private void listWritableSorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
    	{
    		SetWritableResults(((IEnumerable<TagInfo>)listWritableFiles.ItemsSource)?.ToArray());
    		SetCheckedOutResults(((IEnumerable<TagInfo>)listCheckedOutFiles.ItemsSource)?.ToArray());
    	}

    	private void listSearchResultsSorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
    	{
    		SetSearchResults((TagInfo[])listSearchResults.ItemsSource, lastFilter, forceRefresh: true);
    	}

    	private void listFavoritesSorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
    	{
    		SetFavoritesResults((TagInfo[])listFavorites.ItemsSource);
    	}
    }
}
