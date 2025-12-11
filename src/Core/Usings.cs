global using ReactiveUI;
global using ReactiveUI.SourceGenerators;

global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;

//global using System.IO;
global using Stream = System.IO.Stream;
global using FileOptions = System.IO.FileOptions;
global using FileAccess = System.IO.FileAccess;
global using FileMode = System.IO.FileMode;
global using FileShare = System.IO.FileShare;
global using FileAttributes = System.IO.FileAttributes;
global using MemoryStream = System.IO.MemoryStream;
global using StreamReader = System.IO.StreamReader;
global using StreamWriter = System.IO.StreamWriter;
global using SearchOption = System.IO.SearchOption;
global using EnumerationOptions = System.IO.EnumerationOptions;
global using FileStreamOptions = System.IO.FileStreamOptions;
global using MatchCasing = System.IO.MatchCasing;
global using DriveType = System.IO.DriveType;
global using FileStream = System.IO.Abstractions.FileSystemStream;
global using FileInfo = System.IO.Abstractions.IFileInfo;
global using IFileSystemService = ModManager.Services.IFileSystemService;

global using FileSystemEventArgs = System.IO.FileSystemEventArgs;

global using IOException = System.IO.IOException;
global using FileNotFoundException = System.IO.FileNotFoundException;
global using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;

global using System.Linq;
global using System.Net.Http;
global using System.Reactive;
global using System.Reactive.Concurrency;
global using System.Reactive.Disposables;
global using System.Reactive.Disposables.Fluent;
global using System.Reactive.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Runtime.Serialization;

global using Splat;

global using RxCommandUnit = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;
global using ModioMod = Modio.Models.Mod;
global using ModioFile = Modio.Models.File;
global using Loca = ModManager.Locale.Resources;