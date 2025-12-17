using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Bonobo.Plugins.TagFileList
{
    internal class MultiSelectTreeViewItemVisibilityConverter : IMultiValueConverter
    {
    	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    	{
    		Visibility visibility = (Visibility)values[0];
    		if (!(bool)values[1] || visibility == Visibility.Collapsed)
    		{
    			return Visibility.Collapsed;
    		}
    		return Visibility.Visible;
    	}

    	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    	{
    		throw new NotImplementedException();
    	}
    }
}
