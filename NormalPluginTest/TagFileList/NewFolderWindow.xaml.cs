using System.Windows;
using System.Windows.Markup;

namespace Bonobo.Plugins.TagFileList
{
    public partial class NewFolderWindow : Window, IComponentConnector
    {
    	public static DependencyProperty FileNameProperty = DependencyProperty.Register("FileName", typeof(string), typeof(NewFolderWindow));

    	public string FileName
    	{
    		get
    		{
    			return (string)GetValue(FileNameProperty);
    		}
    		set
    		{
    			SetValue(FileNameProperty, value);
    		}
    	}

    	public NewFolderWindow()
    	{
    		InitializeComponent();
    	}

    	private void window_Loaded(object sender, RoutedEventArgs e)
    	{
    		textFileName.Focus();
    	}

    	private void buttonOk_Click(object sender, RoutedEventArgs e)
    	{
    		base.DialogResult = true;
    		Close();
    	}
    }
}
