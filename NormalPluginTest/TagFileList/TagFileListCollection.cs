using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using Corinth.Connections;
using Corinth.TicketTrack;

namespace Bonobo.Plugins.TagFileList
{
    public class TagFileListCollection : BindingList<TagFileListFile>
    {
    	public enum UpdateType
    	{
    		SourceControlFile = 0,
    		SourceControlDirectoryContents = 1,
    		FileSystemFile = 2,
    		FileSystemDirectoryContents = 3,
    		FileSystemFolders = 4
    	}

    	public class ListItemInfo
    	{
    		public string Name { get; set; }

    		public string Path { get; set; }

    		public bool? IsWritable { get; set; }

    		public bool? IsUpToDate { get; set; }

    		public SourceControlFileState? FileState { get; set; }

    		public TagFileListFile Parent { get; set; }

    		public bool IsFolder { get; set; }

    		public bool ItemExists { get; set; }

    		public IEnumerable<string> CheckedOutClients { get; set; }

    		public IEnumerable<string> CheckedOutForScratchClients { get; set; }

    		public ListItemInfo()
    		{
    		}

    		public ListItemInfo(string name, string path, bool? isWritable, SourceControlFileState? FileState, TagFileListFile parent, bool isFolder, bool? isUpToDate)
    		{
    			Name = name;
    			Path = path;
    			IsWritable = isWritable;
    			IsUpToDate = IsUpToDate;
    			this.FileState = FileState;
    			Parent = parent;
    			IsFolder = isFolder;
    		}

    		public ListItemInfo(string name, string path, bool? isWritable, TagFileListFile parent, bool isFolder, bool isUpToDate)
    			: this(name, path, isWritable, null, parent, isFolder, isUpToDate)
    		{
    		}

    		public ListItemInfo(string name, string path, SourceControlFileState? fileState, TagFileListFile parent, bool isFolder, bool isUpToDate)
    			: this(name, path, null, fileState, parent, isFolder, isUpToDate)
    		{
    		}
    	}

    	private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

    	private bool isDepotAvailable = true;

    	public bool IsDepotAvailable => isDepotAvailable;

    	public void SetDepotUnavailable()
    	{
    		if (dispatcher.CheckAccess())
    		{
    			SetDepotUnavailableInternal();
    			return;
    		}
    		dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    		{
    			SetDepotUnavailableInternal();
    		});
    	}

    	private void SetDepotUnavailableInternal()
    	{
    		isDepotAvailable = false;
    		base.RaiseListChangedEvents = false;
    		using (IEnumerator<TagFileListFile> enumerator = GetEnumerator())
    		{
    			while (enumerator.MoveNext())
    			{
    				TagFileListFile current = enumerator.Current;
    				current.State = SourceControlFileState.Offline;
    			}
    		}
    		base.RaiseListChangedEvents = true;
    		ResetBindings();
    	}

    	private int GetItemIndex(ListItemInfo itemToAddOrUpdate)
    	{
    		return GetItemIndex(itemToAddOrUpdate, 0);
    	}

    	private int GetItemIndex(ListItemInfo itemToAddOrUpdate, int startingIndex)
    	{
    		int num = startingIndex;
    		bool flag = false;
    		while (!flag)
    		{
    			if (num >= base.Count)
    			{
    				flag = true;
    				continue;
    			}
    			if (!itemToAddOrUpdate.IsFolder && base[num] is TagFileListFolder)
    			{
    				num++;
    				continue;
    			}
    			if (itemToAddOrUpdate.IsFolder && !(base[num] is TagFileListFolder))
    			{
    				flag = true;
    				continue;
    			}
    			int num2 = base[num].Name.ToLower().CompareTo(itemToAddOrUpdate.Name.ToLower());
    			if (num2 < 0)
    			{
    				num++;
    			}
    			else
    			{
    				flag = num2 <= 0 || true;
    			}
    		}
    		return num;
    	}

    	private void AddOrUpdateItem(ListItemInfo listItem, int listIndex, UpdateType type)
    	{
    		if (listIndex < base.Count && base[listIndex].Name.Equals(listItem.Name, StringComparison.InvariantCultureIgnoreCase))
    		{
    			if (listItem.IsWritable.HasValue)
    			{
    				base[listIndex].IsWritable = listItem.IsWritable.Value;
    			}
    			if (listItem.FileState.HasValue)
    			{
    				base[listIndex].State = listItem.FileState.Value;
    				base[listIndex].IsUpToDate = listItem.IsUpToDate.Value;
    			}
    			if (type == UpdateType.SourceControlFile || type == UpdateType.SourceControlDirectoryContents)
    			{
    				base[listIndex].SourceControlFileUpdateReceived();
    				base[listIndex].ExistsInSourceControl = listItem.ItemExists;
    				base[listIndex].CheckedOutClients = listItem.CheckedOutClients;
    				base[listIndex].CheckedOutForScratchClients = listItem.CheckedOutForScratchClients;
    			}
    			else if (type == UpdateType.FileSystemFile || type == UpdateType.FileSystemDirectoryContents)
    			{
    				base[listIndex].FileSystemFileUpdateReceived();
    				base[listIndex].ExistsInFileSystem = listItem.ItemExists;
    				if (!isDepotAvailable)
    				{
    					base[listIndex].State = SourceControlFileState.Offline;
    				}
    			}
    		}
    		else
    		{
    			if (listItem.IsFolder)
    			{
    				TagFileListFolder item = new TagFileListFolder(listItem.Name, listItem.Path, listItem.Parent);
    				Insert(listIndex, item);
    			}
    			else
    			{
    				TagFileListFile item2 = new TagFileListFile(listItem.Name, listItem.Path, listItem.FileState, listItem.IsWritable, listItem.Parent, listItem.IsUpToDate);
    				Insert(listIndex, item2);
    			}
    			if (type == UpdateType.SourceControlFile || type == UpdateType.SourceControlDirectoryContents)
    			{
    				base[listIndex].SourceControlFileUpdateReceived();
    				base[listIndex].ExistsInSourceControl = listItem.ItemExists;
    				base[listIndex].CheckedOutClients = listItem.CheckedOutClients;
    				base[listIndex].CheckedOutForScratchClients = listItem.CheckedOutForScratchClients;
    			}
    			else if (type == UpdateType.FileSystemFile || type == UpdateType.FileSystemDirectoryContents)
    			{
    				base[listIndex].FileSystemFileUpdateReceived();
    				base[listIndex].ExistsInFileSystem = listItem.ItemExists;
    				base[listIndex].State = (isDepotAvailable ? SourceControlFileState.WaitingForState : SourceControlFileState.Offline);
    			}
    		}
    	}

    	public void MergeDirectoryItems(TagFileListFolder directory, List<ListItemInfo> items, UpdateType type)
    	{
    		Assert.Check(type != UpdateType.FileSystemFile && type != UpdateType.SourceControlFile);
    		if (dispatcher.CheckAccess())
    		{
    			MergeDirectoryItemsInternal(directory, items, type);
    			return;
    		}
    		dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    		{
    			MergeDirectoryItemsInternal(directory, items, type);
    		});
    	}

    	private void MergeDirectoryItemsInternal(TagFileListFolder directory, List<ListItemInfo> items, UpdateType type)
    	{
    		switch (type)
    		{
    		case UpdateType.FileSystemDirectoryContents:
    			directory.FileSystemFolderUpdateReceived();
    			break;
    		case UpdateType.SourceControlDirectoryContents:
    			directory.SourceControlFolderUpdateReceived();
    			break;
    		}
    		base.RaiseListChangedEvents = false;
    		if (type == UpdateType.SourceControlDirectoryContents)
    		{
    			isDepotAvailable = true;
    		}
    		int num = 0;
    		foreach (ListItemInfo item in items)
    		{
    			num = GetItemIndex(item, num);
    			AddOrUpdateItem(item, num, type);
    		}
    		for (int i = 0; i < base.Count; i++)
    		{
    			TagFileListFile tagFileListFile = base[i];
    			TagFileListFolder tagFileListFolder = tagFileListFile.Parent as TagFileListFolder;
    			if (!tagFileListFolder.IsFolderWaitingForUpdate && tagFileListFile.IsFileWaitingForUpdate && !(tagFileListFile is TagFileListFolder))
    			{
    				if (!tagFileListFile.ExistsInSourceControl)
    				{
    					tagFileListFile.SourceControlFileUpdateReceived();
    				}
    				else if (!tagFileListFile.ExistsInFileSystem)
    				{
    					tagFileListFile.FileSystemFileUpdateReceived();
    				}
    			}
    			if (!tagFileListFile.IsFileWaitingForUpdate)
    			{
    				if (!tagFileListFile.ExistsInFileSystem && tagFileListFile.ExistsInSourceControl)
    				{
    					tagFileListFile.State = SourceControlFileState.MissingFromClientDisk;
    				}
    				else if (tagFileListFile.ExistsInFileSystem && !tagFileListFile.ExistsInSourceControl)
    				{
    					tagFileListFile.State = SourceControlFileState.NotInDepot;
    				}
    				else if (!tagFileListFile.ExistsInFileSystem && !tagFileListFile.ExistsInSourceControl)
    				{
    					RemoveAt(i--);
    				}
    			}
    		}
    		base.RaiseListChangedEvents = true;
    		ResetBindings();
    	}

    	public void MergeSingleItem(ListItemInfo item, UpdateType type)
    	{
    		Assert.Check(type == UpdateType.FileSystemFile || type == UpdateType.SourceControlFile);
    		if (dispatcher.CheckAccess())
    		{
    			MergeSingleItemInternal(item, type);
    			return;
    		}
    		dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
    		{
    			MergeSingleItemInternal(item, type);
    		});
    	}

    	private void MergeSingleItemInternal(ListItemInfo item, UpdateType type)
    	{
    		int itemIndex = GetItemIndex(item);
    		AddOrUpdateItem(item, itemIndex, type);
    		TagFileListFile tagFileListFile = base[itemIndex];
    		if (!tagFileListFile.IsFileWaitingForUpdate)
    		{
    			if (!tagFileListFile.ExistsInFileSystem && tagFileListFile.ExistsInSourceControl)
    			{
    				tagFileListFile.State = SourceControlFileState.MissingFromClientDisk;
    			}
    			else if (tagFileListFile.ExistsInFileSystem && !tagFileListFile.ExistsInSourceControl)
    			{
    				tagFileListFile.State = SourceControlFileState.NotInDepot;
    			}
    			else if (!tagFileListFile.ExistsInFileSystem && !tagFileListFile.ExistsInSourceControl)
    			{
    				RemoveAt(itemIndex);
    			}
    		}
    	}
    }
}
