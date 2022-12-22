using System.Xml;
using System.Xml.Linq;
using ModdingToolkit.Config;

namespace ModdingToolkit;

public static class XMLDocumentHelper
{
    private static Dictionary<string, XDocument?> LoadedDocs = new();

    public static bool LoadOrCreateDocToCache(string filePath, out string? fp, bool reload = false, bool overwriteOnFail = false, Func<string>? generateNewDocString = null)
    {
        fp = Utils.PrepareFilePathString(Path.GetDirectoryName(filePath)!, Path.GetFileName(filePath));
        
        if (fp is null)
        {
            LuaCsSetup.PrintCsError($"XMLDocumentHelper::LoadOrCreateDocToCache() | Unable to parse file path.");
            return false;
        }

        if (!fp.EndsWith(".xml"))
        {
            LuaCsSetup.PrintCsError($"XMLDocumentHelper::LoadOrCreateDocToCache() | Filetype is not an XML document. | FP: {fp}");
            return false;
        }
        
        if (!LoadedDocs.ContainsKey(fp) || reload)
        {
            if (Utils.GetFileText(fp, out string? data) == Utils.IOActionResultState.Success)
            {
                try
                {
                    LoadedDocs[fp] = XDocument.Parse(data!);
                }
                catch
                {
                    LuaCsSetup.PrintCsError($"XMLDocumentHelper::LoadOrCreateDocToCache() | Failed to load data, generating new.");
                    try
                    {
                        LoadedDocs[fp] = XDocument.Parse(generateNewDocString?.Invoke() ?? throw new Exception());
                        if (overwriteOnFail)
                            SaveLoadedDocToDisk(fp);
                    }
                    catch
                    {
                        LuaCsSetup.PrintCsError($"XMLDocumentHelper::LoadOrCreateDocToCache() | XML data is not valid. | FP: {fp}");
                        LoadedDocs.Remove(fp);
                        return false;
                    }
                }
            }
        }

        return true;
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
    
    public static bool TrySetRefLoadedXmlDoc(string sanitizedFilePath, in XDocument document, bool addIfMissing = false)
    {
        if (!LoadedDocs.ContainsKey(sanitizedFilePath))
        {
            if (addIfMissing)
                LoadedDocs.Add(sanitizedFilePath, document);
            else
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
        
        var result = Utils.CreateFilePath(sfp, out sfp!);
        if (result == Utils.IOActionResultState.Success)
        {
            try
            {
                LoadedDocs[sanitizedFilePath]?.Save(sanitizedFilePath, SaveOptions.None);
            }
            catch (Exception e)
            {
                LuaCsSetup.PrintCsError($"XMLDocumentHelper::SaveLoadedDocToDisk() | Unknown error. Exception: {e.Message} | SFP: {sfp}");
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