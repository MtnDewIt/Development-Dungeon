using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Corinth.Project;
using TAE.Shared.Tags;

namespace Bonobo.Plugins.TagFileList
{
    public class FavoriteTags
    {
    	public delegate void FavoritesEventHandler();

    	private const string FavoritesFile = "FavoriteTags.txt";

    	private static List<string> m_tagList;

    	public static List<string> TagList
    	{
    		get
    		{
    			if (m_tagList == null)
    			{
    				LoadFavorites();
    			}
    			return m_tagList;
    		}
    	}

    	public static BitmapImage IconAdd => new BitmapImage(new Uri("pack://application:,,,/NormalPlugin;component/TagFileList/Images/Star-Favorite.png", UriKind.Absolute));

    	public static BitmapImage IconRemove => new BitmapImage(new Uri("pack://application:,,,/NormalPlugin;component/TagFileList/Images/Star-Non-Favorite.png", UriKind.Absolute));

    	public static event FavoritesEventHandler Changed;

    	private static void LoadFavorites()
    	{
    		try
    		{
    			string currentProjectRoot = ProjectManager.GetCurrentProjectRoot();
    			if (currentProjectRoot == null)
    			{
    				return;
    			}
    			string path = Path.Combine(currentProjectRoot, "FavoriteTags.txt");
    			if (!File.Exists(path))
    			{
    				return;
    			}
    			m_tagList = new List<string>();
    			lock (m_tagList)
    			{
    				string[] array = File.ReadAllLines(path);
    				if (array == null)
    				{
    					return;
    				}
    				for (int i = 0; i < array.Length; i++)
    				{
    					string text = array[i].Trim();
    					if (text.Length > 0 && !text.StartsWith(";"))
    					{
    						m_tagList.Add(text.Trim());
    					}
    				}
    			}
    		}
    		catch (Exception ex)
    		{
    			MessageBox.Show($"Failed to load favorites file.\r\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
    		}
    	}

    	private static void SaveFavorites()
    	{
    		try
    		{
    			if (m_tagList == null)
    			{
    				return;
    			}
    			lock (m_tagList)
    			{
    				if (m_tagList.Count > 0)
    				{
    					string currentProjectRoot = ProjectManager.GetCurrentProjectRoot();
    					List<string> list = new List<string>();
    					list.Add("; User configuration file for Bonobo favorite tags");
    					list.Add("");
    					list.AddRange(m_tagList);
    					if (currentProjectRoot != null)
    					{
    						string path = Path.Combine(currentProjectRoot, "FavoriteTags.txt");
    						File.WriteAllLines(path, list.ToArray());
    					}
    				}
    			}
    		}
    		catch (Exception ex)
    		{
    			MessageBox.Show($"Failed to save favorites file.\r\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
    		}
    	}

    	public static string Unify(string tagFile)
    	{
    		if (!string.IsNullOrEmpty(tagFile))
    		{
    			string text = TagSystem.TagRootPath + "\\";
    			string text2 = (tagFile.StartsWith(text, StringComparison.OrdinalIgnoreCase) ? tagFile.Substring(text.Length) : tagFile);
    			return text2.ToLower();
    		}
    		return tagFile;
    	}

    	public static bool ContainsTag(string filename)
    	{
    		if (!string.IsNullOrEmpty(filename))
    		{
    			List<string> tagList = TagList;
    			if (tagList != null)
    			{
    				return tagList.Contains(Unify(filename));
    			}
    		}
    		return false;
    	}

    	public static void AddTagList(IEnumerable<string> tagFiles)
    	{
    		if (tagFiles == null)
    		{
    			return;
    		}
    		List<string> list = TagList;
    		if (list == null)
    		{
    			list = (m_tagList = new List<string>());
    		}
    		lock (list)
    		{
    			bool flag = false;
    			foreach (string tagFile in tagFiles)
    			{
    				string item = Unify(tagFile);
    				if (!list.Contains(item))
    				{
    					list.Add(item);
    					flag = true;
    				}
    			}
    			if (flag)
    			{
    				SaveFavorites();
    			}
    		}
    	}

    	public static void RemoveTagList(IEnumerable<string> tagFiles)
    	{
    		if (tagFiles == null)
    		{
    			return;
    		}
    		List<string> tagList = TagList;
    		if (tagList == null)
    		{
    			return;
    		}
    		lock (tagList)
    		{
    			bool flag = false;
    			foreach (string tagFile in tagFiles)
    			{
    				string item = Unify(tagFile);
    				if (tagList.Contains(item))
    				{
    					tagList.Remove(item);
    					flag = true;
    				}
    			}
    			if (flag)
    			{
    				FavoriteTags.Changed();
    				SaveFavorites();
    			}
    		}
    	}

    	public static void AddTag(string tagFilename)
    	{
    		if (!string.IsNullOrEmpty(tagFilename))
    		{
    			AddTagList(new List<string> { tagFilename });
    		}
    	}
    }
}
