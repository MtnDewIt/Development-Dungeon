using Bonobo.Plugins.TagFileList;
using Bonobo.PluginSystem;
using Corinth.Project;
using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;

namespace Bonobo.Application
{
    public partial class MainWindow : Window
    {
        private TagFileListPanel tagFileListPanel;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) 
        {
            FileListPlugin plugin = new FileListPlugin(null);

            tagFileListPanel = new TagFileListPanel(null, plugin);
            mainGrid.Children.Add(tagFileListPanel);
        }
    }
}