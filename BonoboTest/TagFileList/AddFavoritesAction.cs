using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class AddFavoritesAction : FileAction
    {
    	public override string DisplayName => "Add to Favorites";

    	public override float GroupPriority => 1200f;

    	public override float PriorityInGroup => 1f;

    	public override BitmapImage IconImage => FavoriteTags.IconAdd;

    	public AddFavoritesAction(IPluginHost pluginHost, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    	}

    	public override void Invoke()
    	{
    		FavoriteTags.AddTagList(base.FilePaths);
    	}
    }
}
