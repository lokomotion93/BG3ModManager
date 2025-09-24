global using Avalonia;
global using Avalonia.Controls;
global using Avalonia.ReactiveUI;
global using Avalonia.Input;

global using ReactiveUI;
global using ReactiveUI.Fody.Helpers;

global using Splat;

global using System;
global using System.Collections.Generic;

//global using System.IO;
global using Stream = System.IO.Stream;
global using FileOptions = System.IO.FileOptions;
global using FileAccess = System.IO.FileAccess;
global using FileMode = System.IO.FileMode;
global using FileShare = System.IO.FileShare;
global using MemoryStream = System.IO.MemoryStream;
global using StreamReader = System.IO.StreamReader;
global using StreamWriter = System.IO.StreamWriter;
global using SearchOption = System.IO.SearchOption;
global using IOException = System.IO.IOException;

global using System.Linq;
global using System.Reactive;
global using System.Reactive.Concurrency;
global using System.Reactive.Disposables;
global using System.Reactive.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using ModifierKeys = Avalonia.Input.KeyModifiers;
global using RxCommandUnit = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;
global using RxBoolCommandUnit = ReactiveUI.ReactiveCommand<System.Reactive.Unit, bool>;
global using Loca = ModManager.Locale.Resources;