using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class RemoveFavoritesAction : FileAction
    {
    	public override string DisplayName => "Remove from Favorites";

    	public override float GroupPriority => 1200f;

    	public override float PriorityInGroup => 0f;

    	public override BitmapImage IconImage => FavoriteTags.IconRemove;

    	public RemoveFavoritesAction(IPluginHost pluginHost, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    	}

    	public override void Invoke()
    	{
    		FavoriteTags.RemoveTagList(base.FilePaths);
    	}
    }
}
