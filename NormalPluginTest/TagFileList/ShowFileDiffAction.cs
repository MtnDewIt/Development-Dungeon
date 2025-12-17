using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;
using Corinth.SourceDepot;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class ShowFileDiffAction : FileAction
    {
    	public override string DisplayName => "Diff vs Previous...";

    	public override float GroupPriority => 970f;

    	public override float PriorityInGroup => 1000f;

    	public override BitmapImage IconImage => SDIcons.GetSDIcon(SDActionIcons.Diff);

    	public ShowFileDiffAction(IPluginHost pluginHost, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    	}

    	public override void Invoke()
    	{
    		base.PluginHost.FindSingleInterface<ISourceControlProvider>()?.ShowDiff(base.FilePaths.ToArray());
    	}
    }
}
