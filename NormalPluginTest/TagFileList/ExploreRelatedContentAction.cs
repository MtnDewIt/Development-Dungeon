using System.Collections.Generic;
using System.Diagnostics;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class ExploreRelatedContentAction : FileAction
    {
    	public override string DisplayName => "Explore related content";

    	public override float GroupPriority => 800f;

    	public override float PriorityInGroup => 600f;

    	public override string GroupName => "tag tools";

    	public ExploreRelatedContentAction(IPluginHost pluginHost, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    	}

    	public override void Invoke()
    	{
    		IContentExplorerPlugin contentExplorerPlugin = base.PluginHost.FindSingleInterface<IContentExplorerPlugin>();
    		if (contentExplorerPlugin == null)
    		{
    			return;
    		}
    		ITagInformation tagInformation = base.PluginHost.FindSingleInterface<ITagInformation>();
    		foreach (string filePath in base.FilePaths)
    		{
    			contentExplorerPlugin.AddExplorerView(tagInformation.GetRelativePathWithExtension(filePath));
    		}
    	}
    }
}
