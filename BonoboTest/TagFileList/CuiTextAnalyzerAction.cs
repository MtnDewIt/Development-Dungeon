using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Bonobo.PluginSystem;
using Bonobo.PluginSystem.Custom;
using Corinth.Datastore;
using TAE.Shared.Tags;

namespace Bonobo.Plugins.TagFileList
{
    [DebuggerDisplay("Name={DisplayName}")]
    public class CuiTextAnalyzerAction : FileAction
    {
    	public class TextInfo
    	{
    		public TagPath m_TagPath;

    		public List<string> m_Components;

    		public TextInfo()
    		{
    			m_Components = new List<string>();
    		}
    	}

    	public class StyleUsageInfo
    	{
    		public Dictionary<TagPath, List<string>> m_TagFileToComponentsDictionary;

    		public StyleUsageInfo()
    		{
    			m_TagFileToComponentsDictionary = new Dictionary<TagPath, List<string>>();
    		}
    	}

    	private TagDatastore DataStore;

    	private List<TagInfo> TagInfos;

    	private ISourceControlProvider SourceControl;

    	public List<TextInfo> DeprecatedTextInfos;

    	public List<TextInfo> StyleOverrideInfos;

    	public Dictionary<string, StyleUsageInfo> StyleUsageInfos;

    	public override string DisplayName => "Cui Text Analyzer";

    	public override float GroupPriority => 0f;

    	public override float PriorityInGroup => -10f;

    	public CuiTextAnalyzerAction(IPluginHost pluginHost, IEnumerable<string> filePaths)
    		: base(pluginHost, filePaths)
    	{
    		DataStore = new TagDatastore();
    		TagInfos = new List<TagInfo>();
    		SourceControl = base.PluginHost.FindSingleInterface<ISourceControlProvider>();
    		DeprecatedTextInfos = new List<TextInfo>();
    		StyleUsageInfos = new Dictionary<string, StyleUsageInfo>();
    		StyleOverrideInfos = new List<TextInfo>();
    	}

    	public override void Invoke()
    	{
    		if (base.FilePaths == null)
    		{
    			return;
    		}
    		string filename = string.Join(Environment.NewLine, base.FilePaths.ToArray());
    		string[] array = TagPath.FromFilename(filename).ToString().Split('.');
    		CollectPaths("ui\\");
    		using (TextWriter textWriter = new StreamWriter("CUIFONT_DeprecatedTextWidgets.txt"))
    		{
    			textWriter.WriteLine("The following cui files are using deprecated text_widget components");
    			foreach (TextInfo deprecatedTextInfo in DeprecatedTextInfos)
    			{
    				textWriter.WriteLine("\n===== {0} =====", deprecatedTextInfo.m_TagPath.ToString());
    				textWriter.WriteLine("- {0} components", deprecatedTextInfo.m_Components.Count);
    				int num = 1;
    				foreach (string component in deprecatedTextInfo.m_Components)
    				{
    					textWriter.WriteLine("\t({0}) {1}", num, component);
    					num++;
    				}
    			}
    			textWriter.Close();
    		}
    		using (TextWriter textWriter2 = new StreamWriter("CUIFONT_StyleUsages.txt"))
    		{
    			List<KeyValuePair<string, StyleUsageInfo>> list = StyleUsageInfos.ToList();
    			list.Sort((KeyValuePair<string, StyleUsageInfo> firstPair, KeyValuePair<string, StyleUsageInfo> nextPair) => firstPair.Value.m_TagFileToComponentsDictionary.Count.CompareTo(nextPair.Value.m_TagFileToComponentsDictionary.Count));
    			textWriter2.WriteLine("Here is a breakdown of the style usages");
    			foreach (KeyValuePair<string, StyleUsageInfo> item in list)
    			{
    				StyleUsageInfo value = item.Value;
    				textWriter2.WriteLine("\n===== {0} =====", item.Key);
    				textWriter2.WriteLine("- {0} files", value.m_TagFileToComponentsDictionary.Count);
    				foreach (KeyValuePair<TagPath, List<string>> item2 in value.m_TagFileToComponentsDictionary)
    				{
    					List<string> value2 = item2.Value;
    					textWriter2.WriteLine("\n\tFile: {0}", item2.Key);
    					textWriter2.WriteLine("\t- {0} components", value2.Count);
    					int num2 = 1;
    					foreach (string item3 in value2)
    					{
    						textWriter2.WriteLine("\t\t({0}) {1}", num2, item3);
    						num2++;
    					}
    				}
    			}
    			textWriter2.Close();
    		}
    		using (TextWriter textWriter3 = new StreamWriter("CUIFONT_StyleTextsOverriddenFontId.txt"))
    		{
    			textWriter3.WriteLine("The following cui files contain style text widgets with overridden font IDs");
    			foreach (TextInfo styleOverrideInfo in StyleOverrideInfos)
    			{
    				textWriter3.WriteLine("\n===== {0} =====", styleOverrideInfo.m_TagPath.ToString());
    				textWriter3.WriteLine("- {0} components", styleOverrideInfo.m_Components.Count);
    				int num3 = 1;
    				foreach (string component2 in styleOverrideInfo.m_Components)
    				{
    					textWriter3.WriteLine("\t({0}) {1}", num3, component2);
    					num3++;
    				}
    			}
    			textWriter3.Close();
    		}
    		MessageBox.Show("Congratulations, all CUI files successfully.", "HOT TEXT ANALYSIS", MessageBoxButton.OK);
    	}

    	private void CollectPaths(string tagName)
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
                    List<string> list = new List<string>();
                    TextInfo textInfo = new TextInfo();
                    textInfo.m_TagPath = tagPath;
                    ITagFieldBlock tagFieldBlock = tagFile.SelectFieldType<ITagFieldBlock>(text + "Block:components");
                    foreach (ITagElement item3 in tagFieldBlock)
                    {
                        string data = item3.SelectFieldType<ITagFieldElementStringID>("StringId:type").Data;
                        string data2 = item3.SelectFieldType<ITagFieldElementStringID>("StringId:name").Data;
                        if (data == "text_widget")
                        {
                            textInfo.m_Components.Add(data2);
                        }
                        if (data == "style_text_widget")
                        {
                            list.Add(data2);
                        }
                    }
                    if (textInfo.m_Components.Count > 0)
                    {
                        DeprecatedTextInfos.Add(textInfo);
                    }
                    TextInfo textInfo2 = new TextInfo();
                    textInfo2.m_TagPath = tagPath;
                    if (list.Count > 0)
                    {
                        ITagFieldBlock tagFieldBlock2 = tagFile.SelectFieldType<ITagFieldBlock>(text + "Block:overlays");
                        foreach (ITagElement item4 in tagFieldBlock2)
                        {
                            string data3 = item4.SelectFieldType<ITagFieldElementStringID>("StringId:resolution").Data;
                            ITagFieldBlock tagFieldBlock3 = item4.SelectFieldType<ITagFieldBlock>("Block:components");
                            foreach (ITagElement item5 in tagFieldBlock3)
                            {
                                string data4 = item5.SelectFieldType<ITagFieldElementStringID>("StringId:name").Data;
                                if (!list.Contains(data4))
                                {
                                    continue;
                                }
                                ITagFieldStruct tagFieldStruct = item5.SelectFieldType<ITagFieldStruct>("Struct:property values");
                                foreach (ITagFieldStructElement item6 in tagFieldStruct)
                                {
                                    foreach (ITagElement item7 in item6.SelectFieldType<ITagFieldBlock>("Block:string_id properties"))
                                    {
                                        string data5 = item7.SelectFieldType<ITagFieldElementStringID>("StringId:name").Data;
                                        string data6 = item7.SelectFieldType<ITagFieldElementStringID>("StringId:value").Data;
                                        if (data5 == "prop_style_name")
                                        {
                                            if (!StyleUsageInfos.ContainsKey(data6))
                                            {
                                                StyleUsageInfos[data6] = new StyleUsageInfo();
                                            }
                                            if (!StyleUsageInfos[data6].m_TagFileToComponentsDictionary.ContainsKey(tagPath))
                                            {
                                                StyleUsageInfos[data6].m_TagFileToComponentsDictionary[tagPath] = new List<string>();
                                            }
                                            string item = data4 + " (" + data3 + ")";
                                            if (!StyleUsageInfos[data6].m_TagFileToComponentsDictionary[tagPath].Contains(item))
                                            {
                                                StyleUsageInfos[data6].m_TagFileToComponentsDictionary[tagPath].Add(item);
                                            }
                                        }
                                    }
                                    foreach (ITagElement item8 in item6.SelectFieldType<ITagFieldBlock>("Block:long properties"))
                                    {
                                        string data7 = item8.SelectFieldType<ITagFieldElementStringID>("StringId:name").Data;
                                        if (data7 == "prop_font_id")
                                        {
                                            string item2 = data4 + " (" + data3 + ")";
                                            textInfo2.m_Components.Add(item2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (textInfo2.m_Components.Count > 0)
                    {
                        StyleOverrideInfos.Add(textInfo2);
                    }
                }
    		}
    	}
    }
}
