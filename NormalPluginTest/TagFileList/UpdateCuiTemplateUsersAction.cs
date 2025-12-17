using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;
using Corinth.Datastore;
using TAE.Shared.Tags;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class UpdateCuiTemplateUsersAction : FileAction
    {
    	private TagDatastore DataStore;

    	private List<TagInfo> TagInfos;

    	private ISourceControlProvider SourceControl;

    	public override string DisplayName => "Update Cui Template Users";

    	public override float GroupPriority => 0f;

    	public override float PriorityInGroup => 0f;

    	public UpdateCuiTemplateUsersAction(IPluginHost pluginHost, IEnumerable<string> filePaths)
    		: base(pluginHost, filePaths)
    	{
    		DataStore = new TagDatastore();
    		TagInfos = new List<TagInfo>();
    		SourceControl = base.PluginHost.FindSingleInterface<ISourceControlProvider>();
    	}

    	public override void Invoke()
    	{
    		if (base.FilePaths == null)
    		{
    			return;
    		}
    		string text = string.Join(Environment.NewLine, base.FilePaths.ToArray());
    		string[] array = TagPath.FromFilename(text).ToString().Split('.');
    		CollectPaths(array[0]);
    		List<string> list = new List<string>();
    		list.Add(text);
    		foreach (TagInfo tagInfo in TagInfos)
    		{
    			TagPath tagPath = TagPath.FromPathAndExtension(tagInfo.RelativePath, tagInfo.Extension);
    			list.Add(tagPath.Filename);
    		}
    		SourceControl.CheckOut(list);
    		ITagViewer tagViewer = base.PluginHost.FindSingleInterface<ITagViewer>();
    		foreach (string item in list)
    		{
    			tagViewer.ShowTagFileInSpecifiedView(item, fileExists: true, TagViewType.Gui, 0);
    			tagViewer.MarkAsDirty(item);
    			tagViewer.SaveTagFile(item, promptForConfirmation: false, forceSave: true);
    			tagViewer.CloseTagFile(item, force: true);
    		}
    		MessageBox.Show("Congratulations, all CUI files successfully.", "HOT CUI UPDATE", MessageBoxButton.OK);
    	}

    	private void CollectPaths(string tagName)
    	{
    		ITagViewer tagViewer = base.PluginHost.FindSingleInterface<ITagViewer>();
    		TagInfo[] parentReferences = DataStore.GetParentReferences(tagName, "cui_screen");
    		TagInfo[] array = parentReferences;
    		foreach (TagInfo tagInfo in array)
    		{
    			if (tagInfo.Extension.CompareTo("cui_screen") != 0 || TagInfos.Contains(tagInfo))
    			{
    				continue;
    			}
    			bool flag = false;
    			string text = "Struct:system/";
    			TagPath path = TagPath.FromPathAndExtension(tagInfo.RelativePath, tagInfo.Extension);
    			using (TagFile tagFile = new TagFile(path))
    			{
    				ITagFieldBlock tagFieldBlock = tagFile.SelectFieldType<ITagFieldBlock>(text + "Block:template instantiations");
    				foreach (ITagElement item in tagFieldBlock)
    				{
    					TagPath path2 = item.SelectFieldType<ITagFieldReference>("Reference:screen reference").Path;
    					if (path2 != null && path2.RelativePath == tagName)
    					{
    						flag = true;
    					}
    				}
    			}
    			if (flag)
    			{
    				TagInfos.Add(tagInfo);
    				CollectPaths(tagInfo.RelativePath);
    			}
    		}
    	}
    }
}
