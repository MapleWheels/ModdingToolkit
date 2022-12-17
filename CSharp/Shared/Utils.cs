namespace ModdingToolkit;

public static class Utils
{
    public static string PrepareFilePathString(string filePath) =>
        PrepareFilePathString(Path.GetDirectoryName(filePath)!, Path.GetFileName(filePath));

    public static string PrepareFilePathString(string path, string fileName) => 
        Path.Combine(SanitizePath(path), SanitizeFileName(fileName));

    public static string SanitizeFileName(string fileName)
    {
        foreach (char c in Barotrauma.IO.Path.GetInvalidFileNameCharsCrossPlatform())
            fileName = fileName.Replace(c, '_');
        return fileName;
    }

    public static string SanitizePath(string path)
    {
        foreach (char c in Path.GetInvalidPathChars())
            path = path.Replace(c.ToString(), "_");
        return path.CleanUpPath();
    }

    public static IOActionResultState GetFileText(string filePath, out string? fileText)
    {
        fileText = null;
        IOActionResultState ioActionResultState = CreateFilePath(filePath, out var fp);
        if (ioActionResultState == IOActionResultState.Success)
        {
            try
            {
                fileText = File.ReadAllText(fp!);
                return IOActionResultState.Success;
            }
            catch (ArgumentNullException ane)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: An argument is null. path: {fp ?? "null"}");
                return IOActionResultState.FilePathNull;
            }
            catch (ArgumentException ae)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: An argument is invalid. path: {fp ?? "null"}");
                return IOActionResultState.FilePathInvalid;
            }
            catch (DirectoryNotFoundException dnfe)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Cannot find directory. path: {fp ?? "null"}");
                return IOActionResultState.DirectoryMissing;
            }
            catch (PathTooLongException ptle)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: path length is over 200 characters. path: {fp ?? "null"}");
                return IOActionResultState.PathTooLong;
            }
            catch (NotSupportedException nse)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Operation not supported on your platform/environment (permissions?). path: {fp ?? "null"}");
                return IOActionResultState.InvalidOperation;
            }
            catch (IOException ioe)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: IO tasks failed (Operation not supported). path: {fp ?? "null"}");
                return IOActionResultState.IOFailure;
            }
            catch (Exception e)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Unknown/Other Exception. path: {fp ?? "null"} | ExceptionMessage: {e.Message}");
                return IOActionResultState.UnknownError;
            }
        }

        return ioActionResultState;
    }

    public static IOActionResultState CreateFilePath(string filePath, out string? formattedFilePath)
    {
        string? file = Path.GetFileName(filePath);
        string? path = Path.GetDirectoryName(filePath)!;

        formattedFilePath = Utils.PrepareFilePathString(path, file);
        try
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (!File.Exists(formattedFilePath))
                File.WriteAllText(formattedFilePath, "");
            return IOActionResultState.Success;
        }
        catch (ArgumentNullException ane)
        {
            LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: An argument is null. path: {formattedFilePath ?? "null"}");
            return IOActionResultState.FilePathNull;
        }
        catch (ArgumentException ae)
        {
            LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: An argument is invalid. path: {formattedFilePath ?? "null"}");
            return IOActionResultState.FilePathInvalid;
        }
        catch (DirectoryNotFoundException dnfe)
        {
            LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Cannot find directory. path: {path ?? "null"}");
            return IOActionResultState.DirectoryMissing;
        }
        catch (PathTooLongException ptle)
        {
            LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: path length is over 200 characters. path: {formattedFilePath ?? "null"}");
            return IOActionResultState.PathTooLong;
        }
        catch (NotSupportedException nse)
        {
            LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Operation not supported on your platform/environment (permissions?). path: {formattedFilePath ?? "null"}");
            return IOActionResultState.InvalidOperation;
        }
        catch (IOException ioe)
        {
            LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: IO tasks failed (Operation not supported). path: {formattedFilePath ?? "null"}");
            return IOActionResultState.IOFailure;
        }
        catch (Exception e)
        {
            LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Unknown/Other Exception. path: {path ?? "null"} | ExceptionMessage: {e.Message}");
            return IOActionResultState.UnknownError;
        }
    }
    
    public static IOActionResultState WriteFileText(string filePath, string fileText)
    {
        IOActionResultState ioActionResultState = CreateFilePath(filePath, out var fp);
        if (ioActionResultState == IOActionResultState.Success)
        {
            try
            {
                File.WriteAllText(fp, fileText);
                return IOActionResultState.Success;
            }
            catch (ArgumentNullException ane)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: An argument is null. path: {fp ?? "null"}");
                return IOActionResultState.FilePathNull;
            }
            catch (ArgumentException ae)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: An argument is invalid. path: {fp ?? "null"}");
                return IOActionResultState.FilePathInvalid;
            }
            catch (DirectoryNotFoundException dnfe)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Cannot find directory. path: {fp ?? "null"}");
                return IOActionResultState.DirectoryMissing;
            }
            catch (PathTooLongException ptle)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: path length is over 200 characters. path: {fp ?? "null"}");
                return IOActionResultState.PathTooLong;
            }
            catch (NotSupportedException nse)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Operation not supported on your platform/environment (permissions?). path: {fp ?? "null"}");
                return IOActionResultState.InvalidOperation;
            }
            catch (IOException ioe)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: IO tasks failed (Operation not supported). path: {fp ?? "null"}");
                return IOActionResultState.IOFailure;
            }
            catch (Exception e)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Unknown/Other Exception. path: {fp ?? "null"} | ExceptionMessage: {e.Message}");
                return IOActionResultState.UnknownError;
            }
        }

        return ioActionResultState;
    }

    public enum IOActionResultState
    {
        Success, FilePathNull, FilePathInvalid, EntryMissing, DirectoryMissing, PathTooLong, InvalidOperation, IOFailure, UnknownError
    }
}