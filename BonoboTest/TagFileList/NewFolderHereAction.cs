using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;
using Bonobo.PluginSystem.Custom.Windowing;
using Corinth.TicketTrack;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class NewFolderHereAction : FileAction
    {
    	public override string DisplayName => "New folder here...";

    	public override float GroupPriority => 700f;

    	public override float PriorityInGroup => 510f;

    	public override string GroupName => "tag location";

    	public NewFolderHereAction(IPluginHost pluginHost, IEnumerable<string> fullPaths)
    		: base(pluginHost, fullPaths)
    	{
    	}

    	public override void Invoke()
    	{
    		Assert.Check(base.FilePaths.Count() == 1);
    		string path = base.FilePaths.ElementAt(0);
    		NewFolderWindow newFolderWindow = new NewFolderWindow();
    		newFolderWindow.Owner = base.PluginHost.FindSingleInterface<IWindowManager>().OwnerWindow;
    		if (newFolderWindow.ShowDialog() != true || string.IsNullOrEmpty(newFolderWindow.FileName))
    		{
    			return;
    		}
    		string path2 = Path.Combine(path, newFolderWindow.FileName);
    		if (!Directory.Exists(path2))
    		{
    			try
    			{
    				Directory.CreateDirectory(path2);
    				return;
    			}
    			catch (IOException)
    			{
    				MessageBox.Show("Foundation wasn't able to create your folder.", base.PluginHost.FindSingleInterface<IBonoboApplication>().AppName);
    				return;
    			}
    		}
    		MessageBox.Show("The folder name you specified already exists.", base.PluginHost.FindSingleInterface<IBonoboApplication>().AppName);
    	}
    }
}
