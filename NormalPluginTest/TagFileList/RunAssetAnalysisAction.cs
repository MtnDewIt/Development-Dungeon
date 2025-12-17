using System.Collections.Generic;
using System.Diagnostics;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;
using Bonobo.PluginSystem.Custom.Asset;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class RunAssetAnalysisAction : FileAction
    {
    	public override string DisplayName => "Run analysis";

    	public override float GroupPriority => 800f;

    	public override float PriorityInGroup => 600f;

    	public override string GroupName => "tag tools";

    	public RunAssetAnalysisAction(IPluginHost pluginHost, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    	}

    	public override void Invoke()
    	{
    		IAssetAnalysis assetAnalysis = base.PluginHost.FindSingleInterface<IAssetAnalysis>();
    		if (assetAnalysis == null)
    		{
    			return;
    		}
    		ITagInformation tagInformation = base.PluginHost.FindSingleInterface<ITagInformation>();
    		foreach (string filePath in base.FilePaths)
    		{
    			assetAnalysis.AddAnalysisView(filePath);
    		}
    	}
    }
}
