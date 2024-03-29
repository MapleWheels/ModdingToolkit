﻿using System.Diagnostics;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using ModdingToolkit.Networking;

namespace ModdingToolkit;

public static class Utils
{
    #region LOGGING

    public static class Logging
    {
        public static void PrintMessage(string s)
        {
#if SERVER
            Barotrauma.ModUtils.Logging.PrintMessage(s);
#else
            Barotrauma.ModUtils.Logging.PrintMessage(s);
#endif
        }
        
        public static void PrintError(string s)
        {
#if SERVER
            Barotrauma.ModUtils.Logging.PrintError(s);
#else
            Barotrauma.ModUtils.Logging.PrintError(s);
#endif
        }
    }

    #endregion
    
    #region FILE_IO

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
                    Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: An argument is null. path: {fp ?? "null"} | Exception Details: {ane.Message}");
                    return IOActionResultState.FilePathNull;
                }
                catch (ArgumentException ae)
                {
                    Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: An argument is invalid. path: {fp ?? "null"} | Exception Details: {ae.Message}");
                    return IOActionResultState.FilePathInvalid;
                }
                catch (DirectoryNotFoundException dnfe)
                {
                    Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: Cannot find directory. path: {fp ?? "null"} | Exception Details: {dnfe.Message}");
                    return IOActionResultState.DirectoryMissing;
                }
                catch (PathTooLongException ptle)
                {
                    Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: path length is over 200 characters. path: {fp ?? "null"} | Exception Details: {ptle.Message}");
                    return IOActionResultState.PathTooLong;
                }
                catch (NotSupportedException nse)
                {
                    Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: Operation not supported on your platform/environment (permissions?). path: {fp ?? "null"}  | Exception Details: {nse.Message}");
                    return IOActionResultState.InvalidOperation;
                }
                catch (IOException ioe)
                {
                    Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: IO tasks failed (Operation not supported). path: {fp ?? "null"}  | Exception Details: {ioe.Message}");
                    return IOActionResultState.IOFailure;
                }
                catch (Exception e)
                {
                    Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: Unknown/Other Exception. path: {fp ?? "null"} | ExceptionMessage: {e.Message}");
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
                Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: An argument is null. path: {formattedFilePath ?? "null"}  | Exception Details: {ane.Message}");
                return IOActionResultState.FilePathNull;
            }
            catch (ArgumentException ae)
            {
                Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: An argument is invalid. path: {formattedFilePath ?? "null"} | Exception Details: {ae.Message}");
                return IOActionResultState.FilePathInvalid;
            }
            catch (DirectoryNotFoundException dnfe)
            {
                Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: Cannot find directory. path: {path ?? "null"} | Exception Details: {dnfe.Message}");
                return IOActionResultState.DirectoryMissing;
            }
            catch (PathTooLongException ptle)
            {
                Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: path length is over 200 characters. path: {formattedFilePath ?? "null"} | Exception Details: {ptle.Message}");
                return IOActionResultState.PathTooLong;
            }
            catch (NotSupportedException nse)
            {
                Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: Operation not supported on your platform/environment (permissions?). path: {formattedFilePath ?? "null"} | Exception Details: {nse.Message}");
                return IOActionResultState.InvalidOperation;
            }
            catch (IOException ioe)
            {
                Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: IO tasks failed (Operation not supported). path: {formattedFilePath ?? "null"} | Exception Details: {ioe.Message}");
                return IOActionResultState.IOFailure;
            }
            catch (Exception e)
            {
                Utils.Logging.PrintError($"Utils::CreateFilePath() | Exception: Unknown/Other Exception. path: {path ?? "null"} | Exception Details: {e.Message}");
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
                    Utils.Logging.PrintError($"Utils::WriteFileText() | Exception: An argument is null. path: {fp ?? "null"} | Exception Details: {ane.Message}");
                    return IOActionResultState.FilePathNull;
                }
                catch (ArgumentException ae)
                {
                    Utils.Logging.PrintError($"Utils::WriteFileText() | Exception: An argument is invalid. path: {fp ?? "null"} | Exception Details: {ae.Message}");
                    return IOActionResultState.FilePathInvalid;
                }
                catch (DirectoryNotFoundException dnfe)
                {
                    Utils.Logging.PrintError($"Utils::WriteFileText() | Exception: Cannot find directory. path: {fp ?? "null"} | Exception Details: {dnfe.Message}");
                    return IOActionResultState.DirectoryMissing;
                }
                catch (PathTooLongException ptle)
                {
                    Utils.Logging.PrintError($"Utils::WriteFileText() | Exception: path length is over 200 characters. path: {fp ?? "null"} | Exception Details: {ptle.Message}");
                    return IOActionResultState.PathTooLong;
                }
                catch (NotSupportedException nse)
                {
                    Utils.Logging.PrintError($"Utils::WriteFileText() | Exception: Operation not supported on your platform/environment (permissions?). path: {fp ?? "null"} | Exception Details: {nse.Message}");
                    return IOActionResultState.InvalidOperation;
                }
                catch (IOException ioe)
                {
                    Utils.Logging.PrintError($"Utils::WriteFileText() | Exception: IO tasks failed (Operation not supported). path: {fp ?? "null"} | Exception Details: {ioe.Message}");
                    return IOActionResultState.IOFailure;
                }
                catch (Exception e)
                {
                    Utils.Logging.PrintError($"Utils::WriteFileText() | Exception: Unknown/Other Exception. path: {fp ?? "null"} | ExceptionMessage: {e.Message}");
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
    
    #endregion

    #region NETWORKING

    internal static class Networking
    {
        #region MESSAGE_UTILS

        public static object ReadNetValueFromType(IReadMessage msg, Type type)
        {
            if (type == typeof(bool)) return msg.ReadBoolean();
            if (type == typeof(byte)) return msg.ReadByte();
            if (type == typeof(sbyte)) return Convert.ToSByte(msg.ReadInt16()); //converted up to preserve range
            if (type == typeof(char)) return Convert.ToChar(msg.ReadUInt16()); //utf-16b
            if (type == typeof(short)) return msg.ReadInt16();
            if (type == typeof(ushort)) return msg.ReadUInt16();
            if (type == typeof(int)) return msg.ReadInt32();
            if (type == typeof(uint)) return msg.ReadUInt32();
            if (type == typeof(long)) return msg.ReadInt64();
            if (type == typeof(ulong)) return msg.ReadUInt64();
            if (type == typeof(float)) return msg.ReadSingle();
            if (type == typeof(double)) return msg.ReadDouble();
            if (type == typeof(string)) return msg.ReadString();
            if (type == typeof(NetworkEventId)) return (NetworkEventId)Enum.Parse(type, msg.ReadByte().ToString());
            if (type.IsEnum)
            {
                try
                {
                    var etype = (EnumNetworkType)Enum.Parse(typeof(EnumNetworkType), msg.ReadByte().ToString());
                    switch (etype)
                    {
                        case EnumNetworkType.Byte: return Enum.Parse(type, msg.ReadByte().ToString());
                        case EnumNetworkType.Short: return Enum.Parse(type, msg.ReadInt16().ToString());
                        case EnumNetworkType.Int: return Enum.Parse(type, msg.ReadInt32().ToString());
                        case EnumNetworkType.Long: return Enum.Parse(type, msg.ReadInt64().ToString());
                        case EnumNetworkType.String: return Enum.Parse(type, msg.ReadString());
                    }
                }
                catch (Exception e)
                {
                    Logging.PrintError($"Utils.Net...::ReadNetValueFromType() | {e.Message}");
                    return 0;
                }
            }

            Logging.PrintError(
                $"Utils::ReadNetValueFromType() | The Type of {type.Name} is unsupported by Barotrauma Networking!");
            return 0;
        }
        
        public static T ReadNetValueFromType<T>(IReadMessage msg) where T : IConvertible
        {
            Type type = typeof(T);
            if (type == typeof(bool)) return (T)(object)msg.ReadBoolean();
            if (type == typeof(byte)) return (T)(object)msg.ReadByte();
            if (type == typeof(sbyte)) return (T)(object)Convert.ToSByte(msg.ReadInt16()); //converted up to preserve range
            if (type == typeof(char)) return (T)(object)Convert.ToChar(msg.ReadUInt16()); //utf-16
            if (type == typeof(short)) return (T)(object)msg.ReadInt16();
            if (type == typeof(ushort)) return (T)(object)msg.ReadUInt16();
            if (type == typeof(int)) return (T)(object)msg.ReadInt32();
            if (type == typeof(uint)) return (T)(object)msg.ReadUInt32();
            if (type == typeof(long)) return (T)(object)msg.ReadInt64();
            if (type == typeof(ulong)) return (T)(object)msg.ReadUInt64();
            if (type == typeof(float)) return (T)(object)msg.ReadSingle();
            if (type == typeof(double)) return (T)(object)msg.ReadDouble();
            if (type == typeof(string)) return (T)(object)msg.ReadString();
            if (type == typeof(NetworkEventId)) return (T)Enum.Parse(type, msg.ReadByte().ToString());
            if (type.IsEnum)
            {
                try
                {
                    var etype = (EnumNetworkType)Enum.Parse(typeof(EnumNetworkType), msg.ReadByte().ToString());
                    switch (etype)
                    {
                        case EnumNetworkType.Byte: return (T)Enum.Parse(type, msg.ReadByte().ToString());
                        case EnumNetworkType.Short: return (T)Enum.Parse(type, msg.ReadInt16().ToString());
                        case EnumNetworkType.Int: return (T)Enum.Parse(type, msg.ReadInt32().ToString());
                        case EnumNetworkType.Long: return (T)Enum.Parse(type, msg.ReadInt64().ToString());
                        case EnumNetworkType.String: return (T)Enum.Parse(type, msg.ReadString());
                        default: return default!;
                    }
                }
                catch (Exception e)
                {
                    Logging.PrintError($"Utils.Net...::ReadNetValueFromType<{typeof(T)}>() | {e.Message}");
                    return default!;
                }
            }

            Logging.PrintError(
                $"Utils::ReadNetValueFromType() | The Type of {type.Name} is unsupported by Barotrauma Networking!");
            return default!;
        }

        public static T ReadNetValueFromType<T>(INetReadMessage msg) where T : IConvertible
        {
            return ReadNetValueFromType<T>(msg.Message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="type">Note: Type must implement IConvertible interface</param>
        /// <param name="value"></param>
        public static void WriteNetValueFromType(IWriteMessage msg, Type type, object value)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (value is null)
            {
                Logging.PrintError(
                    $"Utils::WriteNetValueFromType() | The value was null for the type of {type.Name}.");
                return;
            }
            Debug.Assert(value is not null);

            if (type == typeof(bool)) msg.WriteBoolean((bool)value);
            else if (type == typeof(byte)) msg.WriteByte((byte)value);
            else if (type == typeof(sbyte)) msg.WriteInt16(Convert.ToInt16(value)); //converted up to preserve range
            else if (type == typeof(char)) msg.WriteUInt16(Convert.ToUInt16(value)); //utf-16b
            else if (type == typeof(short)) msg.WriteInt16((short)value);
            else if (type == typeof(ushort)) msg.WriteUInt16((ushort)value);
            else if (type == typeof(int)) msg.WriteInt32((int)value);
            else if (type == typeof(uint)) msg.WriteUInt32((uint)value);
            else if (type == typeof(long)) msg.WriteInt64((long)value);
            else if (type == typeof(ulong)) msg.WriteUInt64((ulong)value);
            else if (type == typeof(float)) msg.WriteSingle((float)value);
            else if (type == typeof(double)) msg.WriteDouble((double)value);
            else if (type == typeof(string)) msg.WriteString((string)value);
            else if (type == typeof(NetworkEventId)) msg.WriteByte(Convert.ToByte(value));
            else if (type.IsEnum)
            {
                // try to find the smallest signed data type we can pack the Enum into. Default to string on failure.
                bool err = false;
                long min = 0, max = 0;
                foreach (var o in Enum.GetValues(type))
                {
                    try
                    {
                        long v = Convert.ToInt64(o);
                        if (max < v)
                            max = v;
                        else if (v < min)
                            min = v;
                    }
                    catch
                    {
                        err = true;
                        break;
                    }
                }

                try
                {
                    if (err)    //default to string transmission
                    {
                        msg.WriteByte(Convert.ToByte(EnumNetworkType.String));
                        msg.WriteString(Convert.ToString(value)!);
                        return;
                    }

                    if (byte.MinValue <= min && max <= byte.MaxValue)
                    {
                        msg.WriteByte(Convert.ToByte(EnumNetworkType.Byte));
                        msg.WriteByte(Convert.ToByte(value));
                        return;
                    }
                
                    if (short.MinValue <= min && max <= short.MaxValue)
                    {
                        msg.WriteByte(Convert.ToByte(EnumNetworkType.Short));
                        msg.WriteInt16(Convert.ToInt16(value));
                        return;
                    }
                
                    if (int.MinValue <= min && max <= int.MaxValue)
                    {
                        msg.WriteByte(Convert.ToByte(EnumNetworkType.Int));
                        msg.WriteInt32(Convert.ToInt32(value));
                        return;
                    }
                
                    msg.WriteByte(Convert.ToByte(EnumNetworkType.Long));
                    msg.WriteInt64(Convert.ToInt64(value));
                    return;
                }
                catch
                {
                    msg.WriteByte(Convert.ToByte(EnumNetworkType.String));
                    msg.WriteString(value.ToString());
                    return;
                }
            }
            else
            {
                Logging.PrintError(
                    $"Utils::WriteNetValueFromType() | The Type of {type.Name} is unsupported by Barotrauma Networking!");
            }
        }
        
        public static void WriteNetValueFromType<T>(IWriteMessage msg, T value) where T : IConvertible
        {
            WriteNetValueFromType(msg, typeof(T), value);
        }

        public static void WriteNetValueFromType<T>(INetWriteMessage msg, T value) where T : IConvertible
        {
            WriteNetValueFromType(msg.Message, value);
        }

        public enum EnumNetworkType : byte
        {
            Byte = 0, Short, Int, Long, String
        }

        #endregion

        #region NETMGR_UTILS

        public static INetVar<T> CreateNetVar<T>(ItemComponent component, string className, string varName, T value, 
            NetworkSync syncMode) where T : IConvertible
        {
            NetVar<T> netVar = new NetVar<T>();

            int index = 0;
            for (int i = 0; i < component.Item.Components.Count; i++)
            {
                if (component.Item.Components[i] == component)
                {
                    index = i;
                    break;
                }
            }
            netVar.Initialize($"{className}_{component.Item.ID}_{index}", varName, value);

            if (!GameMain.IsMultiplayer || syncMode is NetworkSync.NoSync)
                return netVar;

            NetworkingManager.RegisterNetConfigInstance(netVar, syncMode);

            return netVar;
        }
        
        public static INetVar<T> CreateNetVar<T>(Item item, string className, string varName, T value, 
            NetworkSync syncMode) where T : IConvertible
        {
            NetVar<T> netVar = new NetVar<T>();
            
            netVar.Initialize($"{className}_{item.ID}_NoCompID", varName, value);

            if (!GameMain.IsMultiplayer || syncMode is NetworkSync.NoSync)
                return netVar;

            NetworkingManager.RegisterNetConfigInstance(netVar, syncMode);

            return netVar;
        }
        
        public static INetVar<T> CreateNetVar<T>(string baseName, string varName, T value, 
            NetworkSync syncMode) where T : IConvertible
        {
            NetVar<T> netVar = new NetVar<T>();

            int indexV = 0;
            foreach (char c in baseName)
            {
                indexV += Convert.ToInt32(c);
            }
            
            netVar.Initialize($"{baseName}_CID{indexV}", varName, value);

            if (!GameMain.IsMultiplayer || syncMode is NetworkSync.NoSync)
                return netVar;

            NetworkingManager.RegisterNetConfigInstance(netVar, syncMode);

            return netVar;
        }

        #endregion
    }

    #endregion

    #region GAME

    public static class Game
    {
        public static bool IsRoundInProgress()
        {
#if CLIENT
            if (Screen.Selected is not null
                && Screen.Selected.IsEditor)
                return false;
#endif
            return GameMain.GameSession is not null && Level.Loaded is not null;
        }
    }

    #endregion
}