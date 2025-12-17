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
    public class ShowFileHistoryAction : FileAction
    {
    	public override string DisplayName => "Revision History";

    	public override float GroupPriority => 970f;

    	public override float PriorityInGroup => -100f;

    	public override BitmapImage IconImage => SDIcons.GetSDIcon(SDActionIcons.RevisionHistory);

    	public ShowFileHistoryAction(IPluginHost pluginHost, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    	}

    	public override void Invoke()
    	{
    		base.PluginHost.FindSingleInterface<ISourceControlProvider>()?.ShowHistory(base.FilePaths.ToArray());
    	}
    }
}
