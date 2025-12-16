using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Corinth.Connections;
using TAE.Shared;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("FullPath={FullPath}")]
    public class TagFileListFile : INotifyPropertyChanged
    {
    	private SourceControlFileState state;

    	private bool isWritable;

    	private bool isUpToDate;

    	private bool isGenerated;

    	private IEnumerable<string> checkedOutClients;

    	private IEnumerable<string> checkedOutForScratchClients;

    	private int fileUpdateCount;

    	public string Name { get; set; }

    	public string FullPath { get; set; }

    	public int Depth { get; set; }

    	public TagFileListFile Parent { get; set; }

    	public SourceControlFileState State
    	{
    		get
    		{
    			return state;
    		}
    		set
    		{
    			state = value;
    			if (state == SourceControlFileState.CheckedOutOnAnotherClient)
    			{
    				IsGenerated = Asset.IsGeneratedFile(FullPath);
    			}
    			else
    			{
    				IsGenerated = false;
    			}
    			OnStatusChanged(new PropertyChangedEventArgs("State"));
    			OnStatusChanged(new PropertyChangedEventArgs("CheckedOutToolTip"));
    		}
    	}

    	public bool IsWritable
    	{
    		get
    		{
    			return isWritable;
    		}
    		set
    		{
    			isWritable = value;
    			OnStatusChanged(new PropertyChangedEventArgs("IsWritable"));
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
    			OnStatusChanged(new PropertyChangedEventArgs("IsUpToDate"));
    			OnStatusChanged(new PropertyChangedEventArgs("CheckedOutToolTip"));
    		}
    	}

    	public bool IsGenerated
    	{
    		get
    		{
    			return isGenerated;
    		}
    		set
    		{
    			isGenerated = value;
    			OnStatusChanged(new PropertyChangedEventArgs("IsGenerated"));
    		}
    	}

    	public bool ExistsInFileSystem { get; set; }

    	public bool ExistsInSourceControl { get; set; }

    	public bool IsFileWaitingForUpdate => fileUpdateCount != 0;

    	public IEnumerable<string> CheckedOutClients
    	{
    		get
    		{
    			return checkedOutClients;
    		}
    		set
    		{
    			checkedOutClients = value;
    			OnStatusChanged(new PropertyChangedEventArgs("CheckedOutClients"));
    			OnStatusChanged(new PropertyChangedEventArgs("CheckedOutToolTip"));
    		}
    	}

    	public IEnumerable<string> CheckedOutForScratchClients
    	{
    		get
    		{
    			return checkedOutForScratchClients;
    		}
    		set
    		{
    			checkedOutForScratchClients = value;
    			OnStatusChanged(new PropertyChangedEventArgs("CheckedOutForScratchClients"));
    			OnStatusChanged(new PropertyChangedEventArgs("CheckedOutToolTip"));
    		}
    	}

    	public string CheckedOutToolTip
    	{
    		get
    		{
    			string empty = string.Empty;
    			StringBuilder stringBuilder = new StringBuilder();
    			if (!IsUpToDate)
    			{
    				stringBuilder.Append("File is not synced to latest!");
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
    				if (stringBuilder.Length > 0)
    				{
    					stringBuilder.Append("\n");
    				}
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
    				stringBuilder.Append("Checked for scratch out by:");
    				foreach (string checkedOutForScratchClient in CheckedOutForScratchClients)
    				{
    					stringBuilder.AppendFormat("{0}{1}", Environment.NewLine, checkedOutForScratchClient);
    				}
    			}
    			return stringBuilder.ToString();
    		}
    	}

    	public event PropertyChangedEventHandler PropertyChanged;

    	public TagFileListFile(string name, FileInfo fileInfo, TagFileListFile parent)
    	{
    		Name = name.ToLower();
    		FullPath = fileInfo.FullName.ToLower();
    		Parent = parent;
    		State = SourceControlFileState.Offline;
    		IsUpToDate = true;
    		if ((fileInfo.Attributes & FileAttributes.ReadOnly) == 0)
    		{
    			IsWritable = true;
    		}
    		IsGenerated = false;
    		if (parent != null)
    		{
    			Depth = parent.Depth + 1;
    		}
    	}

    	public TagFileListFile(string name, string fullPath, SourceControlFileState state, TagFileListFile parent, bool isUpToDate)
    	{
    		Name = name.ToLower();
    		FullPath = fullPath.ToLower();
    		Parent = parent;
    		State = state;
    		IsUpToDate = isUpToDate;
    		FileInfo fileInfo = new FileInfo(fullPath);
    		IsWritable = (fileInfo.Attributes & FileAttributes.ReadOnly) == 0;
    		if (State == SourceControlFileState.CheckedOutOnAnotherClient)
    		{
    			IsGenerated = Asset.IsGeneratedFile(fullPath);
    		}
    		else
    		{
    			IsGenerated = false;
    		}
    		if (parent != null)
    		{
    			Depth = parent.Depth + 1;
    		}
    	}

    	public TagFileListFile(string name, string fullPath, SourceControlFileState? state, bool? isWritable, TagFileListFile parent, bool? isUpToDate)
    	{
    		Name = name.ToLower();
    		FullPath = fullPath.ToLower();
    		Parent = parent;
    		IsUpToDate = true;
    		if (isUpToDate.HasValue)
    		{
    			IsUpToDate = isUpToDate.Value;
    		}
    		if (state.HasValue)
    		{
    			State = state.Value;
    		}
    		if (isWritable.HasValue)
    		{
    			IsWritable = isWritable.Value;
    		}
    		if (State == SourceControlFileState.CheckedOutOnAnotherClient)
    		{
    			IsGenerated = Asset.IsGeneratedFile(fullPath);
    		}
    		else
    		{
    			IsGenerated = false;
    		}
    		if (parent != null)
    		{
    			Depth = parent.Depth + 1;
    		}
    	}

    	protected void OnStatusChanged(PropertyChangedEventArgs e)
    	{
    		this.PropertyChanged?.Invoke(this, e);
    	}

    	public void FileSystemFileUpdateReceived()
    	{
    		fileUpdateCount++;
    	}

    	public void SourceControlFileUpdateReceived()
    	{
    		fileUpdateCount--;
    	}
    }
}
