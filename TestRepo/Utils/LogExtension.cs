﻿using System.Runtime.CompilerServices;

namespace TestRepo.Utils;

public static partial class LogExtension
{
    [LoggerMessage(
        LogLevel.Trace,
        Message = "Fail to Initial database, {reason}. \n At {memberName} int {filePath}, line {line}"
    )]
    public static partial void InitializeDatabaseFail(
        this ILogger logger,
        string reason,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int line = 0
    );

    [LoggerMessage(
        LogLevel.Trace,
        Message = "Fail to read {entityName} from Database, {reason}. \n At {memberName} int {filePath}, line {line}"
    )]
    public static partial void ReadFromDatabaseFail(
        this ILogger logger,
        string entityName,
        string reason,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int line = 0
    );

    [LoggerMessage(
        LogLevel.Trace,
        Message = "Fail to write {entityName} to Database, {reason}. \n At {memberName} int {filePath}, line {line}"
    )]
    public static partial void WriteToDatabaseFail(
        this ILogger logger,
        string entityName,
        string reason,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int line = 0
    );
}
