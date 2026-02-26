global using Splat;

global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using ReactiveUI;
global using ReactiveUI.SourceGenerators;

global using RxCommandUnit = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

//23.1.0-beta.1 - 9308d79 Replace RxApp schedulers with RxSchedulers throughout codebase (#4213)
global using RxApp = ReactiveUI.RxSchedulers;