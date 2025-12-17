using System.Windows;
using System.Windows.Markup;
using Corinth.Utilities;

namespace Bonobo.NormalPlugin
{
    public partial class Generic : ResourceDictionary, IComponentConnector
    {
        public enum ResourceId
        {

        }

        public static ComponentResourceKey CreateKey(ResourceId value)
        {
            return new ComponentResourceKey(typeof(Generic), EnumParser.GetString(value));
        }
    }
}
