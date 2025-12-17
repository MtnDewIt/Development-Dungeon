using System.Collections.Generic;
using System.Diagnostics;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class DisplayMemoryFootprintAction : FileAction
    {
    	public override string DisplayName => "Display memory footprint";

    	public override float GroupPriority => 800f;

    	public override float PriorityInGroup => 600f;

    	public override string GroupName => "tag tools";

    	public override bool IsEnabled
    	{
    		get
    		{
    			IMemoryFootprintPlugin memoryFootprintPlugin = base.PluginHost.FindSingleInterface<IMemoryFootprintPlugin>();
    			if (memoryFootprintPlugin == null)
    			{
    				return false;
    			}
    			return !memoryFootprintPlugin.IsBusyBuildingMemeoryFootprint();
    		}
    	}

    	public DisplayMemoryFootprintAction(IPluginHost pluginHost, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    	}

    	public override void Invoke()
    	{
    		IMemoryFootprintPlugin memoryFootprintPlugin = base.PluginHost.FindSingleInterface<IMemoryFootprintPlugin>();
    		if (memoryFootprintPlugin == null)
    		{
    			return;
    		}
    		ITagInformation tagInformation = base.PluginHost.FindSingleInterface<ITagInformation>();
    		foreach (string filePath in base.FilePaths)
    		{
    			memoryFootprintPlugin.AddAssetToFootptrint(tagInformation.GetRelativePathWithExtension(filePath));
    		}
    	}
    }
}
