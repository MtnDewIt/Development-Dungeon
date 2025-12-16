using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class OpenInDefaultApplicationAction : FileAction
    {
    	public override string DisplayName => "Open with default application";

    	public override float GroupPriority => 100000f;

    	public override float PriorityInGroup => -100000f;

    	public OpenInDefaultApplicationAction(IPluginHost pluginHost, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    	}

    	public override void Invoke()
    	{
    		ITagBrowser tagBrowser = base.PluginHost.FindSingleInterface<ITagBrowser>();
    		if (tagBrowser == null)
    		{
    			return;
    		}
    		List<string> list = new List<string>();
    		ITagInformation tagInformation = base.PluginHost.FindSingleInterface<ITagInformation>();
    		foreach (string filePath in base.FilePaths)
    		{
    			if (File.Exists(filePath))
    			{
    				ProcessStartInfo startInfo = new ProcessStartInfo(filePath)
    				{
    					UseShellExecute = true,
    					ErrorDialog = true
    				};
    				Process.Start(startInfo);
    			}
    			else
    			{
    				list.Add(filePath);
    			}
    		}
    		if (list.Count > 0)
    		{
    			MessageBox.Show(string.Format("File(s) don't exist locally:\r\n\r\n{0}", string.Join("\r\n", list.ToArray())), base.PluginHost.FindSingleInterface<IBonoboApplication>().AppName);
    		}
    	}
    }
}
