using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;
using Corinth.Connections;
using Corinth.Project;
using Corinth.TicketTrack;

namespace Bonobo.Plugins.TagFileList
{
    public class TagFileTreeHelper
    {
    	public delegate void ThreadExecuteDelegate();

    	private class ItemContainerStatusContainer
    	{
    		public MultiSelectTreeViewItem Item { get; set; }

    		public string RemainingPath { get; set; }

    		public string SelectedFullPath { get; set; }

    		public ExpandPathCallback Callback { get; set; }

    		public Dispatcher Dispatcher { get; set; }

    		public void StatusChanged(object sender, EventArgs args)
    		{
    			if (Item.Status == GeneratorStatus.ContainersGenerated)
    			{
    				Item.StatusChanged -= StatusChanged;
    				Callback(Item.Items, RemainingPath, SelectedFullPath);
    			}
    		}
    	}

    	private delegate void ExpandPathCallback(IEnumerable<TagFileListFile> fileList, string pathToExpand, string selectedFullPath);

    	private SourceControlStateProvider sourceControlStateProvider = new SourceControlStateProvider();

    	private FileSystemStateProvider fileSystemStateProvider = new FileSystemStateProvider();

    	private IPluginHost pluginHost;

    	private Dispatcher dispatcher;

    	private SortedList<string, object> ignoreChangesFromFiles = new SortedList<string, object>();

    	private object ignoreChangesFromFilesLockObject = new object();

    	private bool isExpandingFromSettings;

    	private string tagsDirectory;

    	private FileListPlugin fileListPlugin;

    	public MultiSelectTreeView Tree { get; set; }

    	public TagFileListFolder TreeRoot
    	{
    		get
    		{
    			return (TagFileListFolder)Tree.Root;
    		}
    		set
    		{
    			TagFileListCollection tagFileListCollection = new TagFileListCollection();
    			tagFileListCollection.Add(value);
    			Tree.ItemsSource = tagFileListCollection;
    			Tree.Root = value;
    		}
    	}

    	public TagFileTreeHelper(IPluginHost pluginHost, FileListPlugin fileListPlugin, MultiSelectTreeView tree)
    	{
    		this.pluginHost = pluginHost;
    		this.fileListPlugin = fileListPlugin;
    		//ITagInformation tagInfoPlugin = this.pluginHost.FindSingleInterface<ITagInformation>();
    		//IAsyncSourceControlProvider asyncSourceControlProvider = this.pluginHost.FindSingleInterface<IAsyncSourceControlProvider>();
    		//sourceControlStateProvider.Initialize(tagInfoPlugin, asyncSourceControlProvider);
    		//fileSystemStateProvider.Initialize(tagInfoPlugin);
    		Tree = tree;
    		dispatcher = Dispatcher.CurrentDispatcher;
    		//tagsDirectory = ProjectManager.GetCurrentProjectTagsRoot();
    	}

    	private void ExecuteOnMainThread(ThreadExecuteDelegate methodToExecute)
    	{
    		if (dispatcher.CheckAccess())
    		{
    			methodToExecute();
    		}
    		else
    		{
    			dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(methodToExecute.Invoke));
    		}
    	}

    	public void RefreshFolder(TagFileListFolder folder)
    	{
    		BeginRefreshFolder(folder);
    		fileSystemStateProvider.RefreshFolderAsync(folder);
    		sourceControlStateProvider.RefreshFolderAsync(folder);
    	}

    	private void BeginRefreshFolders(IEnumerable<TagFileListFolder> folders)
    	{
    		ExecuteOnMainThread(delegate
    		{
    			foreach (TagFileListFolder folder in folders)
    			{
    				folder.Children.RaiseListChangedEvents = false;
    				foreach (TagFileListFile child in folder.Children)
    				{
    					child.State = SourceControlFileState.WaitingForState;
    				}
    				folder.Children.RaiseListChangedEvents = true;
    				folder.Children.ResetBindings();
    			}
    		});
    	}

    	private void BeginRefreshFolder(TagFileListFolder folder)
    	{
    		ExecuteOnMainThread(delegate
    		{
    			folder.Children.RaiseListChangedEvents = false;
    			foreach (TagFileListFile child in folder.Children)
    			{
    				child.State = SourceControlFileState.WaitingForState;
    			}
    			folder.Children.RaiseListChangedEvents = true;
    			folder.Children.ResetBindings();
    		});
    	}

    	private List<TagFileListFolder> GetAllExpandedSubfolders(TagFileListFolder parentFolder)
    	{
    		List<TagFileListFolder> list = new List<TagFileListFolder>();
    		foreach (TagFileListFile child in parentFolder.Children)
    		{
    			if (child is TagFileListFolder)
    			{
    				TagFileListFolder tagFileListFolder = (TagFileListFolder)child;
    				if (tagFileListFolder.IsExpanded)
    				{
    					list.Add(tagFileListFolder);
    					list.AddRange(GetAllExpandedSubfolders(tagFileListFolder));
    				}
    			}
    		}
    		return list;
    	}

    	public void RefreshFile(string filePath)
    	{
    		if (string.IsNullOrEmpty(filePath))
    		{
    			return;
    		}
    		bool flag;
    		lock (ignoreChangesFromFilesLockObject)
    		{
    			flag = ignoreChangesFromFiles.ContainsKey(filePath);
    		}
    		if (!flag)
    		{
    			TagFileListFolder tagFileListFolder = FindFileListFolder(Directory.GetParent(filePath).FullName);
    			if (tagFileListFolder != null)
    			{
    				sourceControlStateProvider.RefreshFileAsync(filePath, tagFileListFolder);
    				fileSystemStateProvider.RefreshFileAsync(filePath, tagFileListFolder);
    			}
    		}
    	}

    	public void RefreshFiles(IEnumerable<string> filePaths)
    	{
    		List<TagFileListFile> list = new List<TagFileListFile>();
    		List<TagFileListFolder> list2 = new List<TagFileListFolder>();
    		foreach (string filePath in filePaths)
    		{
    			TagFileListFile tagFileListFile = FindFileListFile(filePath);
    			if (tagFileListFile != null)
    			{
    				list.Add(tagFileListFile);
    				continue;
    			}
    			TagFileListFolder tagFileListFolder = FindFileListFolder(filePath);
    			if (tagFileListFolder != null)
    			{
    				list2.Add(tagFileListFolder);
    				list2.AddRange(GetAllExpandedSubfolders(tagFileListFolder));
    			}
    		}
    		if (list.Count > 0)
    		{
    			BeginRefreshFiles(list, (TagFileListFolder)list[0].Parent);
    		}
    		if (list2.Count > 0)
    		{
    			BeginRefreshFolders(list2);
    		}
    		foreach (TagFileListFile item in list)
    		{
    			RefreshFile(item.FullPath);
    		}
    		foreach (TagFileListFolder item2 in list2)
    		{
    			RefreshFolder(item2);
    		}
    	}

    	private void BeginRefreshFiles(IEnumerable<TagFileListFile> filesToRefresh, TagFileListFolder parent)
    	{
    		ExecuteOnMainThread(delegate
    		{
    			parent.Children.RaiseListChangedEvents = false;
    			foreach (TagFileListFile item in filesToRefresh)
    			{
    				item.State = SourceControlFileState.WaitingForState;
    			}
    			parent.Children.RaiseListChangedEvents = true;
    			parent.Children.ResetBindings();
    		});
    	}

    	public void FileActionStarting(IEnumerable<string> fileNames)
    	{
    		lock (ignoreChangesFromFilesLockObject)
    		{
    			foreach (string fileName in fileNames)
    			{
    				if (!string.IsNullOrEmpty(fileName) && !ignoreChangesFromFiles.ContainsKey(fileName))
    				{
    					ignoreChangesFromFiles.Add(fileName, null);
    				}
    			}
    		}
    	}

    	public void FileActionFinished(IEnumerable<string> fileNames)
    	{
    		lock (ignoreChangesFromFilesLockObject)
    		{
    			foreach (string fileName in fileNames)
    			{
    				if (!string.IsNullOrEmpty(fileName))
    				{
    					ignoreChangesFromFiles.Remove(fileName);
    				}
    			}
    		}
    	}

    	public TagFileListFile FindFileListFile(string fullPath)
    	{
    		TagFileListFile foundItem = null;
    		ExecuteOnMainThread(delegate
    		{
    			foundItem = FindFileListFileInternal(fullPath, TreeRoot.Children);
    		});
    		return foundItem;
    	}

    	private TagFileListFile FindFileListFileInternal(string fullPath, IEnumerable<TagFileListFile> files)
    	{
    		foreach (TagFileListFile file in files)
    		{
    			if (file is TagFileListFolder tagFileListFolder)
    			{
    				TagFileListFile tagFileListFile = null;
    				if (fullPath.StartsWith(tagFileListFolder.FullPath, StringComparison.InvariantCultureIgnoreCase))
    				{
    					tagFileListFile = FindFileListFileInternal(fullPath, tagFileListFolder.Children);
    				}
    				if (tagFileListFile != null)
    				{
    					return tagFileListFile;
    				}
    			}
    			else if (file.FullPath.Equals(fullPath, StringComparison.InvariantCultureIgnoreCase))
    			{
    				return file;
    			}
    		}
    		return null;
    	}

    	public TagFileListFolder FindFileListFolder(string fullPath)
    	{
    		TagFileListFolder foundItem = null;
    		if (fullPath.Equals(tagsDirectory, StringComparison.InvariantCultureIgnoreCase))
    		{
    			foundItem = TreeRoot;
    		}
    		else
    		{
    			ExecuteOnMainThread(delegate
    			{
    				foundItem = FindFileListFolderInternal(fullPath, TreeRoot.Children);
    			});
    		}
    		return foundItem;
    	}

    	private TagFileListFolder FindFileListFolderInternal(string fullPath, IEnumerable<TagFileListFile> files)
    	{
    		foreach (TagFileListFile file in files)
    		{
    			if (file is TagFileListFolder tagFileListFolder)
    			{
    				if (file.FullPath.Equals(fullPath, StringComparison.InvariantCultureIgnoreCase))
    				{
    					return tagFileListFolder;
    				}
    				TagFileListFolder tagFileListFolder2 = null;
    				if (fullPath.StartsWith(tagFileListFolder.FullPath, StringComparison.InvariantCultureIgnoreCase))
    				{
    					tagFileListFolder2 = FindFileListFolderInternal(fullPath, tagFileListFolder.Children);
    				}
    				if (tagFileListFolder2 != null)
    				{
    					return tagFileListFolder2;
    				}
    			}
    		}
    		return null;
    	}

    	public void ExpandPath(string fullPath, string selectedFullPath)
    	{
    		string currentProjectTagsRoot = ProjectManager.GetCurrentProjectTagsRoot();
    		string fileName = Path.GetFileName(currentProjectTagsRoot);
    		Assert.Check(Tree != null && Tree.Root != null && Tree.Root.Name == fileName);
    		List<TagFileListFile> list = new List<TagFileListFile>();
    		list.Add(Tree.Root);
    		ExpandPathInternal(list, Path.Combine(fileName, fullPath), selectedFullPath);
    	}

    	private void ExpandPathInternal(IEnumerable<TagFileListFile> fileList, string pathToExpand, string selectedFullPath)
    	{
    		pathToExpand = pathToExpand.Trim().ToLower();
    		if (pathToExpand.IndexOf(Path.DirectorySeparatorChar) == -1)
    		{
    			foreach (TagFileListFile file in fileList)
    			{
    				MultiSelectTreeViewItem multiSelectTreeViewItem = (MultiSelectTreeViewItem)Tree.ItemContainerGenerator.ContainerFromItem(file);
    				if (multiSelectTreeViewItem != null)
    				{
    					if (file.Name.ToLower() == pathToExpand)
    					{
    						multiSelectTreeViewItem.IsExpanded = true;
    					}
    					if (selectedFullPath != null)
    					{
    						if (file.FullPath.ToLower() == selectedFullPath)
    						{
    							Tree.SelectedItems.Clear();
    							multiSelectTreeViewItem.BringIntoView();
    							multiSelectTreeViewItem.IsSelected = true;
    						}
    						else if (multiSelectTreeViewItem.Status != GeneratorStatus.ContainersGenerated && pathToExpand.Length > 0)
    						{
    							ItemContainerStatusContainer itemContainerStatusContainer = new ItemContainerStatusContainer();
    							itemContainerStatusContainer.Item = multiSelectTreeViewItem;
    							itemContainerStatusContainer.RemainingPath = ((pathToExpand.IndexOf(Path.DirectorySeparatorChar) == -1) ? "" : pathToExpand.Substring(pathToExpand.IndexOf(Path.DirectorySeparatorChar) + 1));
    							itemContainerStatusContainer.SelectedFullPath = selectedFullPath;
    							itemContainerStatusContainer.Callback = ExpandPathInternal;
    							multiSelectTreeViewItem.StatusChanged += itemContainerStatusContainer.StatusChanged;
    						}
    					}
    				}
    			}
    			return;
    		}
    		string value = pathToExpand.Substring(0, pathToExpand.IndexOf(Path.DirectorySeparatorChar));
    		foreach (TagFileListFile file2 in fileList)
    		{
    			MultiSelectTreeViewItem multiSelectTreeViewItem2 = (MultiSelectTreeViewItem)Tree.ItemContainerGenerator.ContainerFromItem(file2);
    			if (multiSelectTreeViewItem2 == null)
    			{
    				continue;
    			}
    			TagFileListFolder tagFileListFolder = file2 as TagFileListFolder;
    			if (file2.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase) && tagFileListFolder != null)
    			{
    				string text = pathToExpand.Substring(pathToExpand.IndexOf(Path.DirectorySeparatorChar) + 1);
    				if (!multiSelectTreeViewItem2.IsExpanded)
    				{
    					multiSelectTreeViewItem2.IsExpanded = true;
    					multiSelectTreeViewItem2.Status = GeneratorStatus.NotStarted;
    				}
    				if (multiSelectTreeViewItem2.Status != GeneratorStatus.ContainersGenerated)
    				{
    					ItemContainerStatusContainer itemContainerStatusContainer2 = new ItemContainerStatusContainer();
    					itemContainerStatusContainer2.Item = multiSelectTreeViewItem2;
    					itemContainerStatusContainer2.RemainingPath = text;
    					itemContainerStatusContainer2.SelectedFullPath = selectedFullPath;
    					itemContainerStatusContainer2.Callback = ExpandPathInternal;
    					multiSelectTreeViewItem2.StatusChanged += itemContainerStatusContainer2.StatusChanged;
    				}
    				else
    				{
    					ExpandPathInternal(multiSelectTreeViewItem2.Items, text, selectedFullPath);
    				}
    			}
    		}
    	}

    	public void RefreshTree()
    	{
    		lock (this)
    		{
    			if (Tree.IsLoaded)
    			{
    				//string currentProjectTagsRoot = ProjectManager.GetCurrentProjectTagsRoot();
    				//TreeRoot = new TagFileListFolder(Path.GetFileName(currentProjectTagsRoot), currentProjectTagsRoot, null);
    				Assert.Check(!isExpandingFromSettings);
    				isExpandingFromSettings = true;
    				Tree.ItemContainerGenerator.StatusChanged += tagsRootTviItemContainerGenerator_StatusChanged;
    			}
    		}
    	}

    	private void tagsRootTviItemContainerGenerator_StatusChanged(object sender, EventArgs e)
    	{
    		ItemContainerGenerator itemContainerGenerator = (ItemContainerGenerator)sender;
    		if (itemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
    		{
    			return;
    		}
    		itemContainerGenerator.StatusChanged -= tagsRootTviItemContainerGenerator_StatusChanged;
    		dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    		{
    			lock (this)
    			{
    				ExpandFolderListBasedOnSettings();
    				isExpandingFromSettings = false;
    			}
    		});
    	}

    	public void UpdateTreeExpandedStateSettings(string fullPath, bool isExpanded)
    	{
    		if (isExpandingFromSettings)
    		{
    			return;
    		}
    		if (fileListPlugin.Settings.OpenedFolderPaths.Contains(fullPath))
    		{
    			if (!isExpanded)
    			{
    				fileListPlugin.Settings.OpenedFolderPaths.Remove(fullPath);
    				for (int i = 0; i < fileListPlugin.Settings.OpenedFolderPaths.Count; i++)
    				{
    					string text = fileListPlugin.Settings.OpenedFolderPaths[i];
    					if (text.StartsWith(fullPath, StringComparison.InvariantCultureIgnoreCase) && fileListPlugin.Settings.OpenedFolderPaths.Contains(text))
    					{
    						fileListPlugin.Settings.OpenedFolderPaths.RemoveAt(i--);
    					}
    				}
    			}
    		}
    		else if (isExpanded)
    		{
    			fileListPlugin.Settings.OpenedFolderPaths.Add(fullPath);
    		}
    		fileListPlugin.SaveSettings();
    	}

    	private void ExpandFolderListBasedOnSettings()
    	{
    		if (Debugger.IsAttached)
    		{
    			ExpandFolderListBasedOnSettingsInternal();
    			return;
    		}
    		try
    		{
    			ExpandFolderListBasedOnSettingsInternal();
    		}
    		catch (Exception ex)
    		{
    			Console.WriteLine(ex.ToString());
    		}
    	}

    	private void ExpandFolderListBasedOnSettingsInternal()
    	{
    		string lastSelectedFileSetting = GetLastSelectedFileSetting();
    		ExpandPath("", lastSelectedFileSetting);
    		string text = tagsDirectory + Path.DirectorySeparatorChar;
    		List<string> list = new List<string>();
    		foreach (string openedFolderPath in fileListPlugin.Settings.OpenedFolderPaths)
    		{
    			if (openedFolderPath.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
    			{
    				ExpandPath(openedFolderPath.Substring(text.Length).ToLowerInvariant(), lastSelectedFileSetting);
    			}
    			else
    			{
    				list.Add(openedFolderPath);
    			}
    		}
    		foreach (string item in list)
    		{
    			fileListPlugin.Settings.OpenedFolderPaths.Remove(item);
    		}
    	}

    	public void SaveLastSelectedFileSetting(string fullPath)
    	{
    		fileListPlugin.Settings.CurrentlySelectedPath = fullPath;
    		fileListPlugin.SaveSettings();
    	}

    	private string GetLastSelectedFileSetting()
    	{
    		string result = null;
    		if (fileListPlugin.Settings != null && !string.IsNullOrEmpty(fileListPlugin.Settings.CurrentlySelectedPath))
    		{
    			result = fileListPlugin.Settings.CurrentlySelectedPath;
    		}
    		return result;
    	}

    	private void SetSelectedFile(string fullPath)
    	{
    		TagFileListFile tagFileListFile = FindFileListFile(fullPath);
    		if (tagFileListFile != null)
    		{
    			MultiSelectTreeViewItem multiSelectTreeViewItem = (MultiSelectTreeViewItem)Tree.ItemContainerGenerator.ContainerFromItem(tagFileListFile);
    			if (multiSelectTreeViewItem != null)
    			{
    				multiSelectTreeViewItem.IsSelected = true;
    			}
    		}
    	}
    }
}
