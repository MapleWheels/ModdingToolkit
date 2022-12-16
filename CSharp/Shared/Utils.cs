namespace ModdingToolkit;

public static class Utils
{
    public static string? PrepareFilePathString(string path, string fileName)
    {
        var inv_p = Path.GetInvalidPathChars();
        var inv_f = Barotrauma.IO.Path.GetInvalidFileNameCharsCrossPlatform();

        string newPath = path;
        foreach (char c in inv_p)
        {
            newPath = newPath.Replace(c.ToString(), "_");
        }

        string newFileName = fileName;
        foreach (char c in inv_f)
        {
            newFileName = newFileName.Replace(c, '_');
        }

        return Path.Combine(newPath, newFileName).CleanUpPath();
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

        string? fp = Utils.PrepareFilePathString(path, file);
        formattedFilePath = fp;
        try
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (!File.Exists(fp))
                File.WriteAllText(fp, "");
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
            LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Cannot find directory. path: {path ?? "null"}");
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
            LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Unknown/Other Exception. path: {path ?? "null"} | ExceptionMessage: {e.Message}");
            return IOActionResultState.UnknownError;
        }
    }

    public enum IOActionResultState
    {
        Success, FilePathNull, FilePathInvalid, DirectoryMissing, PathTooLong, InvalidOperation, IOFailure, UnknownError
    }
}