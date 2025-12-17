using System.Collections.Generic;
using System.Diagnostics;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class ForceXSyncFileAction : FileAction
    {
    	public override string DisplayName => "Force XSync";

    	public override string GroupName => "view content";

    	public override float GroupPriority => 200f;

    	public override float PriorityInGroup => 900f;

    	public ForceXSyncFileAction(IPluginHost pluginHost, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    	}

    	public override void Invoke()
    	{
    		base.PluginHost.FindSingleInterface<IXSyncer>()?.XSyncFileList(base.FilePaths, forceXSync: true);
    	}
    }
}
