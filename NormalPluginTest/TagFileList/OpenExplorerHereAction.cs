using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class OpenExplorerHereAction : FileAction
    {
    	public override string DisplayName => "Find in Windows Explorer";

    	public override float GroupPriority => 700f;

    	public override float PriorityInGroup => 100f;

    	public override string GroupName => "tag location";

    	public OpenExplorerHereAction(IPluginHost pluginHost, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    	}

    	public override void Invoke()
    	{
    		List<string> list = new List<string>();
    		foreach (string filePath in base.FilePaths)
    		{
    			string text = null;
    			if (Directory.Exists(filePath))
    			{
    				text = filePath;
    			}
    			else if (Directory.Exists(Path.GetDirectoryName(filePath)))
    			{
    				text = Path.GetDirectoryName(filePath);
    			}
    			if (text != null)
    			{
    				Process.Start(new ProcessStartInfo("explorer.exe", text)
    				{
    					UseShellExecute = false
    				});
    			}
    			else
    			{
    				list.Add(filePath);
    			}
    		}
    		if (list.Count == 1)
    		{
    			MessageBox.Show("Directory doesn't exist.", base.PluginHost.FindSingleInterface<IBonoboApplication>().AppName);
    		}
    		else if (list.Count > 0)
    		{
    			MessageBox.Show("One or more directories don't exist.", base.PluginHost.FindSingleInterface<IBonoboApplication>().AppName);
    		}
    	}
    }
}
