using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;
using Corinth.Datastore;
using TAE.Shared.Tags;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class CuiOnDemandBitmapFinder : FileAction
    {
    	private TagDatastore DataStore;

    	private List<TagInfo> TagInfos;

    	public List<TagPath> BitmapPaths;

    	public override string DisplayName => "Cui OnDemand Bitmap Finder";

    	public override float GroupPriority => 0f;

    	public override float PriorityInGroup => -20f;

    	public CuiOnDemandBitmapFinder(IPluginHost pluginHost, IEnumerable<string> filePaths)
    		: base(pluginHost, filePaths)
    	{
    		DataStore = new TagDatastore();
    		TagInfos = new List<TagInfo>();
    		BitmapPaths = new List<TagPath>();
    	}

    	public override void Invoke()
    	{
    		if (base.FilePaths == null)
    		{
    			return;
    		}
    		FindOnDemandBitmaps();
    		using (TextWriter textWriter = new StreamWriter("CUI_OnDemandBitmaps.txt"))
    		{
    			int num = 1;
    			textWriter.WriteLine("The following bitmap tags have the On Demand flag set");
    			foreach (TagPath bitmapPath in BitmapPaths)
    			{
    				textWriter.WriteLine("({0})  {1}", num, bitmapPath.ToString());
    				num++;
    			}
    			textWriter.Close();
    		}
    		MessageBox.Show("Congratulations, all CUI files successfully.", "ON DEMAND BATMAN", MessageBoxButton.OK);
    	}

    	private void FindOnDemandBitmaps()
    	{
    		ITagViewer tagViewer = base.PluginHost.FindSingleInterface<ITagViewer>();
    		string[] extensions = new string[1] { "bitmap" };
    		TagInfo[] tagsWithExtension = DataStore.GetTagsWithExtension(extensions);
    		TagInfo[] array = tagsWithExtension;
    		foreach (TagInfo tagInfo in array)
    		{
    			TagPath tagPath = TagPath.FromPathAndExtension(tagInfo.RelativePath, tagInfo.Extension);
    			if (!tagPath.IsTagAccessible() || (!tagInfo.RelativePath.StartsWith("ui\\") && !tagInfo.RelativePath.StartsWith("ui\\cui\\")))
    			{
    				continue;
    			}
                using (TagFile tagFile = new TagFile(tagPath)) 
                {
                    ITagFieldFlags tagFieldFlags = tagFile.SelectFieldType<ITagFieldFlags>("WordFlags:Flags");
                    if (tagFieldFlags != null && tagFieldFlags.TestBit("only use on demand"))
                    {
                        BitmapPaths.Add(tagPath);
                    }
                }
    		}
    	}
    }
}
