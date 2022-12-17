using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ModdingToolkit.Config;

public static class XMLDocumentHelper
{
    private static Dictionary<string, XDocument?> LoadedDocs = new();

    public static string? LoadOrCreateDocToCache(string filePath, bool reload = false, bool overwriteOnFail = false)
    {
        string? fp = Utils.PrepareFilePathString(Path.GetDirectoryName(filePath)!, Path.GetFileName(filePath));
        
        if (fp is null)
        {
            LuaCsSetup.PrintCsError($"XMLDocumentHelper::LoadDocument() | Unable to parse file path.");
            return null;
        }

        if (!fp.EndsWith(".xml"))
        {
            LuaCsSetup.PrintCsError($"XMLDocumentHelper::LoadDocument() | Filetype is not an XML document.");
            return null;
        }
        
        if (!LoadedDocs.ContainsKey(fp) || reload)
        {
            if (Utils.GetFileText(fp, out string? data) == Utils.IOActionResultState.Success)
            {
                try
                {
                    if (!data.IsNullOrWhiteSpace())
                        LoadedDocs[fp] = XDocument.Parse(data!);
                    else
                    {
                        LoadedDocs[fp] = new XDocument();
                        if (overwriteOnFail)
                        {
                            SaveLoadedDocToDisk(fp);
                        }
                    }
                }
                catch (XmlException xme)
                {
                    LuaCsSetup.PrintCsError($"XMLDocumentHelper::LoadDocument() | XML data is not valid.");
                    LoadedDocs.Remove(fp);
                    return null;
                }
            }
        }

        return fp;
    }

    internal static bool UnloadCache(bool saveToDisk = false, bool force = false)
    {
        if (saveToDisk)
        {
            var r = SaveAllDocsToDisk();
            if (r.Any())
            {
                foreach (var state in r)
                {
                    LuaCsSetup.PrintCsError(
                        $"XMLDocumentHelper::UnloadCache() | Error while writing to disk pathKey: {state.Key} , State: {state.Value.ToString()}");
                }
                if (force)
                {
                    LoadedDocs.Clear();
                    return true;
                }
                return false;
            }
        }
        LoadedDocs.Clear();
        return true;
    }

    public static bool TryGetLoadedXmlDoc(string sanitizedFilePath, out XDocument? document)
    {
        if (!LoadedDocs.ContainsKey(sanitizedFilePath))
        {
            document = null;
            return false;
        }

        document = LoadedDocs[sanitizedFilePath];
        return true;
    }
    
    public static bool TrySetRefLoadedXmlDoc(string sanitizedFilePath, XDocument document)
    {
        if (!LoadedDocs.ContainsKey(sanitizedFilePath))
        {
            return false;
        }
        LoadedDocs[sanitizedFilePath] = document;
        return true;
    }

    
    public static Utils.IOActionResultState SaveLoadedDocToDisk(string sanitizedFilePath)
    {
        string sfp = sanitizedFilePath;
        
        if (!LoadedDocs.ContainsKey(sfp) || LoadedDocs[sfp] is null)
            return Utils.IOActionResultState.EntryMissing;
        
        var result = Utils.CreateFilePath(sfp, out sfp);
        if (result == Utils.IOActionResultState.Success)
        {
            try
            {
                LoadedDocs[sfp!]?.Save(sfp!, SaveOptions.None);
            }
            catch (Exception e)
            {
                LuaCsSetup.PrintCsError($"XMLDocumentHelper::SaveLoadedDocToDisk() | Unknown error. Exception: {e.Message}");
                return Utils.IOActionResultState.UnknownError;
            }
        }
        return result;
    }
    
    /// <summary>
    /// Saves all loaded XDocuments to disk.
    /// </summary>
    /// <returns>A dictionary containing the IOResults for any docs that failed to save.</returns>
    internal static Dictionary<string, Utils.IOActionResultState> SaveAllDocsToDisk()
    {
        Dictionary<string, Utils.IOActionResultState> dict = new();
        foreach (KeyValuePair<string,XDocument?> loadedDoc in LoadedDocs)
        {
            var result = SaveLoadedDocToDisk(loadedDoc.Key);
            if (result != Utils.IOActionResultState.Success)
                dict.Add(loadedDoc.Key, result);
        }
        return dict;
    }
    
}