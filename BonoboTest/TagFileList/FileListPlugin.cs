using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;
using Bonobo.PluginSystem.Custom.Windowing;
using Corinth.UI.Wpf;

namespace Bonobo.Plugins.TagFileList
{
    [BonoboPlugin("File list")]
    public class FileListPlugin : BonoboPlugin, IFileActionProvider, IFileList, IPluggablePanelProvider
    {
    	private class CopyPathsFileAction : FileAction
    	{
    		public override string DisplayName => "Copy paths";

    		public override float GroupPriority => 500f;

    		public override float PriorityInGroup => 800f;

    		public override string GroupName => "path utils";

    		public CopyPathsFileAction(IPluginHost pluginHost, IEnumerable<string> filePaths)
    			: base(pluginHost, filePaths)
    		{
    		}

    		public override void Invoke()
    		{
    			if (base.FilePaths != null)
    			{
    				string text = string.Join(Environment.NewLine, base.FilePaths.ToArray());
    				ClipboardWrapper.SetText(text);
    			}
    		}
    	}

    	private FileListSettings settings;

    	private TagFileListPanel internalPanel;

    	internal FileListSettings Settings => settings;

    	public FileListPlugin(IPluginHost pluginHost)
    		: base(pluginHost)
    	{
    		internalPanel = new TagFileListPanel(base.PluginHost, this);
    	}

    	public override void PostInitialize()
    	{
    		base.PostInitialize();
    		settings = base.PluginHost.FindSingleInterface<ISettingsStore>().LoadSettings<FileListSettings>(this);
    		if (settings == null)
    		{
    			settings = new FileListSettings();
    		}
    		base.PluginHost.FindSingleInterface<IMainMenu>().SetMenu(this, GetMenu());
    	}

    	public IEnumerable<PluggablePanel> GetControls()
    	{
    		return new PluggablePanel[1]
    		{
    			new PluggablePanel(internalPanel, AttachLocation.Left, 0f, AttachmentOptions.RequestFill)
    			{
    				PreferredSize = new Size(300.0, 0.0)
    			}
    		};
    	}

    	private IEnumerable<MenuItemDescription> GetMenu()
    	{
    		return new List<MenuItemDescription>
    		{
    			new MenuItemDescription("search for tag", menuSearchForTag_Click, null, onlyAvailableWhenPluginsUIIsFocused: false)
    		};
    	}

    	private void menuSearchForTag_Click(object sender, EventArgs e)
    	{
    		if (internalPanel != null)
    		{
    			internalPanel.SearchForTag();
    		}
    	}

    	public void SaveSettings()
    	{
    		base.PluginHost.FindSingleInterface<ISettingsStore>().SaveSettings(this, settings);
    	}

    	public IEnumerable<IFileAction> GetActions(IEnumerable<FileActionParameters> fileActionParamsList)
    	{
    		List<IFileAction> list = new List<IFileAction>();
    		list.Add(new CopyPathsFileAction(base.PluginHost, fileActionParamsList.Select((FileActionParameters a) => a.FileName)));
    		internalPanel.AddFileActions(fileActionParamsList, list);
    		return list;
    	}

    	public void SetFilterString(string fileFilter)
    	{
    		if (internalPanel != null)
    		{
    			internalPanel.SetFilterString(fileFilter);
    		}
    	}

    	public bool FindInTreeView(string fullPath)
    	{
    		if (internalPanel != null)
    		{
    			return internalPanel.FindInTreeView(fullPath);
    		}
    		return false;
    	}
    }
}
