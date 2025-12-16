using System.Collections.Generic;
using System.Diagnostics;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class FindInTreeViewAction : FileAction
    {
    	private TagFileListPanel panel;

    	public override string DisplayName => "Find in tree view";

    	public override float GroupPriority => 700f;

    	public override float PriorityInGroup => 600f;

    	public override string GroupName => "tag location";

    	public FindInTreeViewAction(IPluginHost pluginHost, TagFileListPanel panel, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    		this.panel = panel;
    	}

    	public override void Invoke()
    	{
    		foreach (string filePath in base.FilePaths)
    		{
    			panel.FindInTreeView(filePath);
    		}
    	}
    }
}
