using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Bonobo.PluginSystem.Custom;
using Corinth.Connections;

namespace Bonobo.Plugins.TagFileList
{
    public class SourceControlStateProvider
    {
    	private class RefreshFilesThreadParameters
    	{
    		public string FilePath { get; set; }

    		public TagFileListFolder ParentFolder { get; set; }
    	}

    	private ITagInformation _tagInfoPlugin;

    	private IAsyncSourceControlProvider _asyncSourceControlProvider;

    	public void Initialize(ITagInformation tagInfoPlugin, IAsyncSourceControlProvider asyncSourceControlProvider)
    	{
    		_tagInfoPlugin = tagInfoPlugin;
    		_asyncSourceControlProvider = asyncSourceControlProvider;
    	}

    	public void RefreshFolderAsync(TagFileListFolder folderToRefresh)
    	{
    		ThreadPool.QueueUserWorkItem(RefreshFolderAsyncInternal, folderToRefresh);
    	}

    	private void RefreshFolderAsyncInternal(object folderToRefreshObject)
    	{
    		TagFileListFolder tagFileListFolder = folderToRefreshObject as TagFileListFolder;
    		try
    		{
    			IEnumerable<SourceControlFile> enumerable = from fileInfo in Observable.Single<IEnumerable<SourceControlFile>>(_asyncSourceControlProvider.GetFileStateForDirectoryAsync(new string[1] { tagFileListFolder.FullPath }))
    				orderby Path.GetFileName(fileInfo.FileName)
    				select fileInfo;
    			List<TagFileListCollection.ListItemInfo> list = new List<TagFileListCollection.ListItemInfo>();
    			foreach (SourceControlFile item in enumerable)
    			{
    				if (_tagInfoPlugin.IsTagExtensionValid(Path.GetExtension(item.FileName)))
    				{
    					TagFileListCollection.ListItemInfo listItemInfo = new TagFileListCollection.ListItemInfo();
    					listItemInfo.Name = Path.GetFileName(item.FileName);
    					listItemInfo.Path = item.FileName;
    					listItemInfo.IsWritable = item.IsWritable;
    					listItemInfo.IsUpToDate = item.IsUpToDate;
    					listItemInfo.FileState = item.State;
    					listItemInfo.Parent = tagFileListFolder;
    					listItemInfo.IsFolder = false;
    					listItemInfo.ItemExists = true;
    					listItemInfo.CheckedOutClients = item.CheckedOutClients;
    					listItemInfo.CheckedOutForScratchClients = item.CheckedOutForScratchClients;
    					list.Add(listItemInfo);
    				}
    			}
    			tagFileListFolder.Children.MergeDirectoryItems(tagFileListFolder, list, TagFileListCollection.UpdateType.SourceControlDirectoryContents);
    		}
    		catch
    		{
    			tagFileListFolder.Children.SetDepotUnavailable();
    		}
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
    		try
    		{
    			if (_tagInfoPlugin.IsTagExtensionValid(Path.GetExtension(refreshFilesThreadParameters.FilePath)))
    			{
    				SourceControlFile sourceControlFile = Observable.Single<IEnumerable<SourceControlFile>>(_asyncSourceControlProvider.GetFileStatesAsync(new string[1] { refreshFilesThreadParameters.FilePath })).SingleOrDefault();
    				TagFileListCollection.ListItemInfo listItemInfo = new TagFileListCollection.ListItemInfo();
    				listItemInfo.Name = Path.GetFileName(refreshFilesThreadParameters.FilePath);
    				listItemInfo.Path = refreshFilesThreadParameters.FilePath;
    				listItemInfo.IsWritable = sourceControlFile?.IsWritable ?? false;
    				listItemInfo.FileState = sourceControlFile?.State ?? SourceControlFileState.NotInDepot;
    				listItemInfo.IsUpToDate = sourceControlFile?.IsUpToDate ?? true;
    				listItemInfo.Parent = refreshFilesThreadParameters.ParentFolder;
    				listItemInfo.IsFolder = false;
    				listItemInfo.ItemExists = sourceControlFile != null;
    				listItemInfo.CheckedOutClients = sourceControlFile?.CheckedOutClients;
    				listItemInfo.CheckedOutForScratchClients = sourceControlFile?.CheckedOutForScratchClients;
    				refreshFilesThreadParameters.ParentFolder.Children.MergeSingleItem(listItemInfo, TagFileListCollection.UpdateType.SourceControlFile);
    			}
    		}
    		catch
    		{
    			refreshFilesThreadParameters.ParentFolder.Children.SetDepotUnavailable();
    		}
    	}
    }
}
