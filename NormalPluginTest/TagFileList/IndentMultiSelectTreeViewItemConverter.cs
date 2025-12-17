using System;
using System.Globalization;
using System.Windows.Data;

namespace Bonobo.Plugins.TagFileList
{
    [ValueConversion(typeof(int), typeof(int))]
    internal class IndentMultiSelectTreeViewItemConverter : IValueConverter
    {
    	public const int DepthPixelMultiplyer = 14;

    	public object Convert(object v, Type targetType, object parameter, CultureInfo culture)
    	{
    		return 14 * (int)v;
    	}

    	public object ConvertBack(object v, Type targetType, object parameter, CultureInfo culture)
    	{
    		throw new NotSupportedException();
    	}
    }
}
