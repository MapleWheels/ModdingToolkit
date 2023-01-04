using System.Diagnostics;
using Barotrauma.Networking;

namespace ModdingToolkit;



public static class Utils
{
    public static class IO
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
                    LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: An argument is null. path: {fp ?? "null"} | Exception Details: {ane.Message}");
                    return IOActionResultState.FilePathNull;
                }
                catch (ArgumentException ae)
                {
                    LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: An argument is invalid. path: {fp ?? "null"} | Exception Details: {ae.Message}");
                    return IOActionResultState.FilePathInvalid;
                }
                catch (DirectoryNotFoundException dnfe)
                {
                    LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Cannot find directory. path: {fp ?? "null"} | Exception Details: {dnfe.Message}");
                    return IOActionResultState.DirectoryMissing;
                }
                catch (PathTooLongException ptle)
                {
                    LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: path length is over 200 characters. path: {fp ?? "null"} | Exception Details: {ptle.Message}");
                    return IOActionResultState.PathTooLong;
                }
                catch (NotSupportedException nse)
                {
                    LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Operation not supported on your platform/environment (permissions?). path: {fp ?? "null"}  | Exception Details: {nse.Message}");
                    return IOActionResultState.InvalidOperation;
                }
                catch (IOException ioe)
                {
                    LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: IO tasks failed (Operation not supported). path: {fp ?? "null"}  | Exception Details: {ioe.Message}");
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

            formattedFilePath = IO.PrepareFilePathString(path, file);
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
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: An argument is null. path: {formattedFilePath ?? "null"}  | Exception Details: {ane.Message}");
                return IOActionResultState.FilePathNull;
            }
            catch (ArgumentException ae)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: An argument is invalid. path: {formattedFilePath ?? "null"} | Exception Details: {ae.Message}");
                return IOActionResultState.FilePathInvalid;
            }
            catch (DirectoryNotFoundException dnfe)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Cannot find directory. path: {path ?? "null"} | Exception Details: {dnfe.Message}");
                return IOActionResultState.DirectoryMissing;
            }
            catch (PathTooLongException ptle)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: path length is over 200 characters. path: {formattedFilePath ?? "null"} | Exception Details: {ptle.Message}");
                return IOActionResultState.PathTooLong;
            }
            catch (NotSupportedException nse)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Operation not supported on your platform/environment (permissions?). path: {formattedFilePath ?? "null"} | Exception Details: {nse.Message}");
                return IOActionResultState.InvalidOperation;
            }
            catch (IOException ioe)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: IO tasks failed (Operation not supported). path: {formattedFilePath ?? "null"} | Exception Details: {ioe.Message}");
                return IOActionResultState.IOFailure;
            }
            catch (Exception e)
            {
                LuaCsSetup.PrintCsError($"Utils::CreateFilePath() | Exception: Unknown/Other Exception. path: {path ?? "null"} | Exception Details: {e.Message}");
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
                    File.WriteAllText(fp!, fileText);
                    return IOActionResultState.Success;
                }
                catch (ArgumentNullException ane)
                {
                    LuaCsSetup.PrintCsError($"Utils::WriteFileText() | Exception: An argument is null. path: {fp ?? "null"} | Exception Details: {ane.Message}");
                    return IOActionResultState.FilePathNull;
                }
                catch (ArgumentException ae)
                {
                    LuaCsSetup.PrintCsError($"Utils::WriteFileText() | Exception: An argument is invalid. path: {fp ?? "null"} | Exception Details: {ae.Message}");
                    return IOActionResultState.FilePathInvalid;
                }
                catch (DirectoryNotFoundException dnfe)
                {
                    LuaCsSetup.PrintCsError($"Utils::WriteFileText() | Exception: Cannot find directory. path: {fp ?? "null"} | Exception Details: {dnfe.Message}");
                    return IOActionResultState.DirectoryMissing;
                }
                catch (PathTooLongException ptle)
                {
                    LuaCsSetup.PrintCsError($"Utils::WriteFileText() | Exception: path length is over 200 characters. path: {fp ?? "null"} | Exception Details: {ptle.Message}");
                    return IOActionResultState.PathTooLong;
                }
                catch (NotSupportedException nse)
                {
                    LuaCsSetup.PrintCsError($"Utils::WriteFileText() | Exception: Operation not supported on your platform/environment (permissions?). path: {fp ?? "null"} | Exception Details: {nse.Message}");
                    return IOActionResultState.InvalidOperation;
                }
                catch (IOException ioe)
                {
                    LuaCsSetup.PrintCsError($"Utils::WriteFileText() | Exception: IO tasks failed (Operation not supported). path: {fp ?? "null"} | Exception Details: {ioe.Message}");
                    return IOActionResultState.IOFailure;
                }
                catch (Exception e)
                {
                    LuaCsSetup.PrintCsError($"Utils::WriteFileText() | Exception: Unknown/Other Exception. path: {fp ?? "null"} | ExceptionMessage: {e.Message}");
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

    public static class Networking
    {
        public static dynamic ReadNetValueFromType(IReadMessage msg, Type type)
        {
            if (type == typeof(bool)) return msg.ReadBoolean();
            if (type == typeof(byte)) return msg.ReadByte();
            if (type == typeof(sbyte)) return (sbyte)msg.ReadUInt16(); //converted up to preserve range
            if (type == typeof(char)) return (char)msg.ReadUInt16(); //utf-16b
            if (type == typeof(short)) return msg.ReadInt16();
            if (type == typeof(ushort)) return msg.ReadUInt16();
            if (type == typeof(int)) return msg.ReadInt32();
            if (type == typeof(uint)) return msg.ReadUInt32();
            if (type == typeof(long)) return msg.ReadInt64();
            if (type == typeof(ulong)) return msg.ReadUInt64();
            if (type == typeof(float)) return msg.ReadSingle();
            if (type == typeof(double)) return msg.ReadDouble();
            if (type == typeof(string)) return msg.ReadString();

            LuaCsSetup.PrintCsError(
                $"Utils::ReadNetValueFromType() | The Type of {type.Name} is unsupported by Barotrauma Networking!");
            return 0;
        }
        
        public static T ReadNetValueFromType<T>(IReadMessage msg) where T : IConvertible
        {
            Type type = typeof(T);
            if (type == typeof(bool)) return (T)(dynamic)msg.ReadBoolean();
            if (type == typeof(byte)) return (T)(dynamic)msg.ReadByte();
            if (type == typeof(sbyte)) return (T)(dynamic)msg.ReadUInt16(); //converted up to preserve range
            if (type == typeof(char)) return (T)(dynamic)msg.ReadUInt16(); //utf-16b
            if (type == typeof(short)) return (T)(dynamic)msg.ReadInt16();
            if (type == typeof(ushort)) return (T)(dynamic)msg.ReadUInt16();
            if (type == typeof(int)) return (T)(dynamic)msg.ReadInt32();
            if (type == typeof(uint)) return (T)(dynamic)msg.ReadUInt32();
            if (type == typeof(long)) return (T)(dynamic)msg.ReadInt64();
            if (type == typeof(ulong)) return (T)(dynamic)msg.ReadUInt64();
            if (type == typeof(float)) return (T)(dynamic)msg.ReadSingle();
            if (type == typeof(double)) return (T)(dynamic)msg.ReadDouble();
            if (type == typeof(string)) return (T)(dynamic)msg.ReadString();

            LuaCsSetup.PrintCsError(
                $"Utils::ReadNetValueFromType() | The Type of {type.Name} is unsupported by Barotrauma Networking!");
            return default!;
        }

        public static void WriteNetValueFromType(IWriteMessage msg, Type type, object value)
        {
            if (type == typeof(bool)) msg.WriteBoolean((bool)value);
            else if (type == typeof(byte)) msg.WriteByte((byte)value);
            else if (type == typeof(sbyte)) msg.WriteUInt16((ushort)value); //converted up to preserve range
            else if (type == typeof(char)) msg.WriteUInt16((char)value); //utf-16b
            else if (type == typeof(short)) msg.WriteInt16((short)value);
            else if (type == typeof(ushort)) msg.WriteUInt16((ushort)value);
            else if (type == typeof(int)) msg.WriteInt32((int)value);
            else if (type == typeof(uint)) msg.WriteUInt32((uint)value);
            else if (type == typeof(long)) msg.WriteInt64((long)value);
            else if (type == typeof(ulong)) msg.WriteUInt64((ulong)value);
            else if (type == typeof(float)) msg.WriteSingle((float)value);
            else if (type == typeof(double)) msg.WriteDouble((double)value);
            else if (type == typeof(string)) msg.WriteString((string)value);

            LuaCsSetup.PrintCsError(
                $"Utils::WriteNetValueFromType() | The Type of {type.Name} is unsupported by Barotrauma Networking!");
        }
        
        public static void WriteNetValueFromType<T>(IWriteMessage msg, T value) where T : IConvertible
        {
            Type type = typeof(T);
            if (type == typeof(bool)) msg.WriteBoolean((bool)(dynamic)value);
            else if (type == typeof(byte)) msg.WriteByte((byte)(dynamic)value);
            else if (type == typeof(sbyte)) msg.WriteUInt16((ushort)(dynamic)value); //converted up to preserve range
            else if (type == typeof(char)) msg.WriteUInt16((char)(dynamic)value); //utf-16b
            else if (type == typeof(short)) msg.WriteInt16((short)(dynamic)value);
            else if (type == typeof(ushort)) msg.WriteUInt16((ushort)(dynamic)value);
            else if (type == typeof(int)) msg.WriteInt32((int)(dynamic)value);
            else if (type == typeof(uint)) msg.WriteUInt32((uint)(dynamic)value);
            else if (type == typeof(long)) msg.WriteInt64((long)(dynamic)value);
            else if (type == typeof(ulong)) msg.WriteUInt64((ulong)(dynamic)value);
            else if (type == typeof(float)) msg.WriteSingle((float)(dynamic)value);
            else if (type == typeof(double)) msg.WriteDouble((double)(dynamic)value);
            else if (type == typeof(string)) msg.WriteString((string)(dynamic)value);

            LuaCsSetup.PrintCsError(
                $"Utils::WriteNetValueFromType() | The Type of {type.Name} is unsupported by Barotrauma Networking!");
        }
    }
}