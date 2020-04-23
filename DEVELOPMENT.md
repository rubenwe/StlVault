# Developer Docs
This document gives an outline on how to get started if you want to modify STLVault.

## Unity
This project is based on the [Unity](https://unity3d.com/) game engine. 
The version used for this project is 2019.3.

### Installing the editor
If you want to build the project, you need a version of Unity: 
1. Get a unity account
2. Install Unity Hub
3. Install a version of Unity from Unity Hub

Make sure to install IL2CPP Support for the OS you are working on.

#### IL2CPP and AoT 
Unity has two available scripting backends for the created binaries: 
- Mono, which is a Just-In-Time-enabled .NET runtime
- IL2CPP, which produces Ahead-of-Time compiled native binaries

IL2CPP can produce highly optimized native code and has shown vastly better
performance for the kind of workloads STLVault handles. Because of this and a
smaller size of the generated binaries, release builds will use IL2CPP.

This has some implications on for the code of STLVault:
- Code generation at runtime is not permitted (Reflection.Emit etc.)
- Code that is only called via reflection might be removed by the IL-Stripper
- Generics must be specialized at compile time

Always test your contributions via "File > Build and Run".
The Unity editor doesn't use IL2CPP so behavior can be quite different.

## Working on the project
A few things you should know about the project:

### Checking out the repository
This repository uses [git lfs](https://git-lfs.github.com/).
Make sure you have lfs installed and set up before cloning the repository.

### Git-Workflow
As specified in the main README, if you want to contribute it is best to
chat with the developers in the [discord](https://discord.gg/sexQM8R).

Create a pull requests from your fork:
- pull requests _MUST_ be based on commits from this repo's `master` branch
- pull requests _SHOULD_ be rebased onto the current master, if possible

Only contributions compatible with this repository's license can be accepted.

### Project Structure
The general layout of this repository is defined by the default structure of Unity projects. 
But there are some things worth knowing:

#### Scenes
Currently all content is part of the `MainScene`.
This is obviously not ideal an should be split up at some point.

#### UI Prefabs
Finished UI elements should be saved as a prefab in case the UI needs to be reconstructed.

### Patterns and Practices
The general coding approach in the repository is a flavor of the MVVM pattern.
Separation of the UI, the application code and the interaction with the
environment is important to me. Even if there currently aren't nearly enough
tests, this leads to the ability to introduce unit tests later on without
much problems.

#### Services
Everything that talks to the outside world (file system, server access, database)
should be isolated into a component in the services namespace. This will allow
us to swap them for another implementation (for testing) or to create easy plugin options.

#### Views
Views are the Unity-UI specific code that renders the view model data to the screen.
During initialization the Views are given a ViewModel they can bind to.
Binding is done either via direct event subscription or through the `BindTo` extension methods.

#### ViewModels
ViewModels contain the meat of the application logic. All code that reflects
how the application behaves and handles the internal interactions should go here.

The ViewModels are unaware of the Views they expose their surface as:
- `BindableProperty<T>`s for all properties that update at runtime
- `(IReadOnly)ObservableList<T>`s for collections that update at runtime
- `ICommand`s for all actions that can be triggered externally

Services are passed into ViewModels via constructor injection.

#### General
Currently everything is wired together in the `ViewInitializer` class.
That's probably your best point of entry.
