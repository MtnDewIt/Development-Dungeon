public class UnicodeStringData 
{
    public string StringIdName { get; set; }
    public string StringIdContent { get; set; }

    public UnicodeStringData(string stringIdName, string stringIdContent) 
    {
        StringIdName = stringIdName;
        StringIdContent = stringIdContent;
    }
}