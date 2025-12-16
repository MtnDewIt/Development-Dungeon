using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Bonobo.PluginSystem.Custom;

namespace Bonobo.Plugins.TagFileList
{
    public class FileSystemStateProvider
    {
    	private class RefreshFilesThreadParameters
    	{
    		public string FilePath { get; set; }

    		public TagFileListFolder ParentFolder { get; set; }
    	}

    	private ITagInformation tagInfoPlugin;

    	public void Initialize(ITagInformation tagInfoPlugin)
    	{
    		this.tagInfoPlugin = tagInfoPlugin;
    	}

    	public void RefreshFolderAsync(TagFileListFolder folderToRefresh)
    	{
    		ThreadPool.QueueUserWorkItem(RefreshFolderAsyncInternal, folderToRefresh);
    	}

    	private void RefreshFolderAsyncInternal(object folderToRefreshObject)
    	{
    		TagFileListFolder tagFileListFolder = folderToRefreshObject as TagFileListFolder;
    		if (!Directory.Exists(tagFileListFolder.FullPath))
    		{
    			return;
    		}
    		List<TagFileListCollection.ListItemInfo> list = new List<TagFileListCollection.ListItemInfo>();
    		try
    		{
    			IEnumerable<string> enumerable = from folderName in Directory.GetDirectories(tagFileListFolder.FullPath)
    				orderby Path.GetFileName(folderName)
    				select folderName;
    			foreach (string item in enumerable)
    			{
    				TagFileListCollection.ListItemInfo listItemInfo = new TagFileListCollection.ListItemInfo();
    				listItemInfo.Name = Path.GetFileName(item);
    				listItemInfo.Path = item;
    				listItemInfo.IsUpToDate = true;
    				listItemInfo.IsWritable = null;
    				listItemInfo.FileState = null;
    				listItemInfo.Parent = tagFileListFolder;
    				listItemInfo.IsFolder = true;
    				listItemInfo.ItemExists = true;
    				list.Add(listItemInfo);
    			}
    		}
    		catch
    		{
    		}
    		List<TagFileListCollection.ListItemInfo> list2 = new List<TagFileListCollection.ListItemInfo>();
    		try
    		{
    			IEnumerable<FileInfo> enumerable2 = from fileInfo in new DirectoryInfo(tagFileListFolder.FullPath).GetFiles()
    				orderby fileInfo.Name
    				select fileInfo;
    			foreach (FileInfo item2 in enumerable2)
    			{
    				if (tagInfoPlugin.IsTagExtensionValid(Path.GetExtension(item2.Name)))
    				{
    					TagFileListCollection.ListItemInfo listItemInfo2 = new TagFileListCollection.ListItemInfo();
    					listItemInfo2.Name = item2.Name;
    					listItemInfo2.Path = item2.FullName;
    					listItemInfo2.IsWritable = (item2.Attributes & FileAttributes.ReadOnly) == 0;
    					listItemInfo2.FileState = null;
    					listItemInfo2.IsUpToDate = true;
    					listItemInfo2.Parent = tagFileListFolder;
    					listItemInfo2.IsFolder = false;
    					listItemInfo2.ItemExists = true;
    					list2.Add(listItemInfo2);
    				}
    			}
    		}
    		catch
    		{
    		}
    		List<TagFileListCollection.ListItemInfo> list3 = new List<TagFileListCollection.ListItemInfo>();
    		list3.AddRange(list);
    		list3.AddRange(list2);
    		tagFileListFolder.Children.MergeDirectoryItems(tagFileListFolder, list3, TagFileListCollection.UpdateType.FileSystemDirectoryContents);
    	}

    	public void RefreshFileAsync(string filePath, TagFileListFolder parentFolder)
    	{
    		RefreshFilesThreadParameters refreshFilesThreadParameters = new RefreshFilesThreadParameters();
    		refreshFilesThreadParameters.FilePath = filePath;
    		refreshFilesThreadParameters.ParentFolder = parentFolder;
    		ThreadPool.QueueUserWorkItem(RefreshFileAsyncInternal, refreshFilesThreadParameters);
    	}

    	private void RefreshFileAsyncInternal(object parametersObject)
    	{
    		RefreshFilesThreadParameters refreshFilesThreadParameters = parametersObject as RefreshFilesThreadParameters;
    		string fileName = Path.GetFileName(refreshFilesThreadParameters.FilePath);
    		FileInfo fileInfo = null;
    		TagFileListCollection.ListItemInfo listItemInfo = new TagFileListCollection.ListItemInfo();
    		listItemInfo.Name = fileName;
    		listItemInfo.Path = refreshFilesThreadParameters.FilePath;
    		listItemInfo.FileState = null;
    		listItemInfo.IsUpToDate = true;
    		listItemInfo.Parent = refreshFilesThreadParameters.ParentFolder;
    		listItemInfo.IsFolder = false;
    		if (Directory.Exists(refreshFilesThreadParameters.ParentFolder.FullPath) && tagInfoPlugin.IsTagExtensionValid(Path.GetExtension(fileName)))
    		{
    			try
    			{
    				FileInfo[] files = new DirectoryInfo(refreshFilesThreadParameters.ParentFolder.FullPath).GetFiles(fileName);
    				fileInfo = ((files.Length == 1) ? files[0] : null);
    				listItemInfo.IsWritable = fileInfo != null && (fileInfo.Attributes & FileAttributes.ReadOnly) == 0;
    				listItemInfo.ItemExists = fileInfo != null;
    			}
    			catch
    			{
    				listItemInfo.IsWritable = false;
    				listItemInfo.ItemExists = false;
    			}
    			refreshFilesThreadParameters.ParentFolder.Children.MergeSingleItem(listItemInfo, TagFileListCollection.UpdateType.FileSystemFile);
    		}
    	}
    }
}
