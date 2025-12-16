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
    public class CuiWidgetFinderAction : FileAction
    {
    	public class ComponentInfo
    	{
    		public TagPath m_TagPath;

    		public List<string> m_Components;

    		public ComponentInfo()
    		{
    			m_Components = new List<string>();
    		}
    	}

    	public static string WIDGETNAMEPREFIX = "widget_name = ";

    	public static string WIDGETTYPEPREFIX = "widget_type = ";

    	private string m_SearchingWidgetName;

    	private string m_SearchingWidgetType;

    	private TagDatastore DataStore;

    	private List<TagInfo> TagInfos;

    	private ISourceControlProvider SourceControl;

    	public List<ComponentInfo> ComponentInfos;

    	public override string DisplayName => "Cui Widget Finder";

    	public override float GroupPriority => 0f;

    	public override float PriorityInGroup => -20f;

    	public CuiWidgetFinderAction(IPluginHost pluginHost, IEnumerable<string> filePaths)
    		: base(pluginHost, filePaths)
    	{
    		DataStore = new TagDatastore();
    		TagInfos = new List<TagInfo>();
    		SourceControl = base.PluginHost.FindSingleInterface<ISourceControlProvider>();
    		ComponentInfos = new List<ComponentInfo>();
    		m_SearchingWidgetName = "";
    		m_SearchingWidgetType = "";
    	}

    	public override void Invoke()
    	{
    		if (base.FilePaths == null)
    		{
    			return;
    		}
    		try
    		{
                using (TextReader textReader = new StreamReader("CUI_WidgetFinderIN.txt")) 
                {
                    for (string text = textReader.ReadLine(); text != null; text = textReader.ReadLine())
                    {
                        if (text.StartsWith(WIDGETNAMEPREFIX))
                        {
                            m_SearchingWidgetName = text.Substring(WIDGETNAMEPREFIX.Length);
                        }
                        else if (text.StartsWith(WIDGETTYPEPREFIX))
                        {
                            m_SearchingWidgetType = text.Substring(WIDGETTYPEPREFIX.Length);
                        }
                    }
                    if (m_SearchingWidgetName.CompareTo("") == 0 && m_SearchingWidgetType.CompareTo("") == 0)
                    {
                        MessageBox.Show("The search input file appears to be in the wrong format.  Here is the correct format:\n\nwidget_name = <name>\nwidget_type = <type>\n\nBoth name and type are optional, but at least one must be defined.", "Search Error", MessageBoxButton.OK);
                        return;
                    }
                    textReader.Close();
                }
    		}
    		catch (IOException)
    		{
    			MessageBox.Show("Be sure you have a file named CUI_WidgetFinderIN.txt in the main directory.", "Search Error", MessageBoxButton.OK);
    			return;
    		}
    		CollectComponents();
    		using (TextWriter textWriter = new StreamWriter("CUI_WidgetFinderOUT.txt"))
    		{
    			textWriter.WriteLine("The following cui files contain components that met the search criteria");
    			if (m_SearchingWidgetName.CompareTo("") != 0)
    			{
    				textWriter.WriteLine("widget_name = {0}", m_SearchingWidgetName);
    			}
    			if (m_SearchingWidgetType.CompareTo("") != 0)
    			{
    				textWriter.WriteLine("widget_type = {0}", m_SearchingWidgetType);
    			}
    			foreach (ComponentInfo componentInfo in ComponentInfos)
    			{
    				textWriter.WriteLine("\n===== {0} =====", componentInfo.m_TagPath.ToString());
    				textWriter.WriteLine("- {0} components", componentInfo.m_Components.Count);
    				int num = 1;
    				foreach (string component in componentInfo.m_Components)
    				{
    					textWriter.WriteLine("\t({0}) {1}", num, component);
    					num++;
    				}
    			}
    			textWriter.Close();
    		}
    		MessageBox.Show("Congratulations, all CUI files successfully.", "HOT COMPONENT ANALYSIS", MessageBoxButton.OK);
    	}

    	private void CollectComponents()
    	{
    		ITagViewer tagViewer = base.PluginHost.FindSingleInterface<ITagViewer>();
    		string[] extensions = new string[1] { "cui_screen" };
    		TagInfo[] tagsWithExtension = DataStore.GetTagsWithExtension(extensions);
    		TagInfo[] array = tagsWithExtension;
    		foreach (TagInfo tagInfo in array)
    		{
    			string text = "Struct:system/";
    			TagPath tagPath = TagPath.FromPathAndExtension(tagInfo.RelativePath, tagInfo.Extension);
    			if (!tagPath.IsTagAccessible() || !(tagInfo.RelativePath.Substring(0, 7) != "ui\\cui\\"))
    			{
    				continue;
    			}
                using (TagFile tagFile = new TagFile(tagPath)) 
                {
                    ComponentInfo componentInfo = new ComponentInfo();
                    componentInfo.m_TagPath = tagPath;
                    ITagFieldBlock tagFieldBlock = tagFile.SelectFieldType<ITagFieldBlock>(text + "Block:components");
                    foreach (ITagElement item in tagFieldBlock)
                    {
                        string data = item.SelectFieldType<ITagFieldElementStringID>("StringId:type").Data;
                        string data2 = item.SelectFieldType<ITagFieldElementStringID>("StringId:name").Data;
                        if ((m_SearchingWidgetName.CompareTo("") == 0 || m_SearchingWidgetName.CompareTo(data2) == 0) && (m_SearchingWidgetType.CompareTo("") == 0 || m_SearchingWidgetType.CompareTo(data) == 0))
                        {
                            componentInfo.m_Components.Add(data2);
                        }
                    }
                    if (componentInfo.m_Components.Count > 0)
                    {
                        ComponentInfos.Add(componentInfo);
                    }
                }
    		}
    	}
    }
}
