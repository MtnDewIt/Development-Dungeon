using System.ComponentModel;
using System.Diagnostics;
using Corinth.Connections;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("FullPath={FullPath}")]
    public class TagFileListFolder : TagFileListFile
    {
    	private int folderUpdateCount;

    	private readonly TagFileListCollection children = new TagFileListCollection();

    	private bool isExpanded;

    	public bool IsExpanded
    	{
    		get
    		{
    			return isExpanded;
    		}
    		set
    		{
    			isExpanded = value;
    			OnStatusChanged(new PropertyChangedEventArgs("IsExpanded"));
    		}
    	}

    	public TagFileListCollection Children => children;

    	public bool IsFolderWaitingForUpdate => folderUpdateCount != 0;

    	public event ListChangedEventHandler ChildrenListChanged;

    	public TagFileListFolder(string name, string fullPath, TagFileListFile parent)
    		: base(name, fullPath, SourceControlFileState.Offline, false, parent, true)
    	{
    		children.ListChanged += OnListChanged;
    	}

    	protected void OnListChanged(object sender, ListChangedEventArgs e)
    	{
    		if (this.ChildrenListChanged != null)
    		{
    			this.ChildrenListChanged(sender, e);
    		}
    	}

    	public void FileSystemFolderUpdateReceived()
    	{
    		folderUpdateCount++;
    	}

    	public void SourceControlFolderUpdateReceived()
    	{
    		folderUpdateCount--;
    	}
    }
}
