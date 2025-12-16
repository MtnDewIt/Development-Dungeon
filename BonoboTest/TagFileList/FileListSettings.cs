using System;
using System.Collections.Generic;

namespace Bonobo.Plugins.TagFileList
{
    [Serializable]
    internal class FileListSettings
    {
    	public List<string> OpenedFolderPaths = new List<string>();

    	public string CurrentlySelectedPath;

    	public double WritableSplitterRatio = 1.0;
    }
}
