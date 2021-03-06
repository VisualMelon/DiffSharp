# DiffSharp 

[![Build Status](https://travis-ci.org/DiffSharp/DiffSharp.svg?branch=dev)](https://travis-ci.org/DiffSharp/DiffSharp)
[![codecov](https://codecov.io/gh/DiffSharp/DiffSharp/branch/dev/graph/badge.svg)](https://codecov.io/gh/DiffSharp/DiffSharp)

This is the development branch of DiffSharp 1.0.0.

You can clone this repository to your machine as follows:
```
git clone --branch dev https://github.com/DiffSharp/DiffSharp.git
cd DiffSharp
```

## Run tests

Required:
- Install [.NET Core SDK](https://dotnet.microsoft.com/download) for your system

Use the following command in the root directory of this repository:
```
dotnet test
```

## Build DiffSharp in Docker

Required:
- Install [Docker](https://hub.docker.com/search/?type=edition&offering=community) for your system

Build a Docker image called `diffsharp`. This will work without any local .NET Core installation and build DiffSharp inside the image.
```
docker build -t diffsharp .
```

Use the following to instantiate a Docker container from the `diffsharp` image and run the tests inside:
```
docker run --rm diffsharp dotnet test
```

## Getting Started

After building, to use DiffSharp in this development branch you must reference an appropriate configuration.

Reference one of

* `DiffSharp-cpu`
* `DiffSharp-cuda-win`
* `DiffSharp-cuda-linux`

Then use:

    dsharp.config(backend=Backend.Torch)

Alternatively use the reference backend via `DiffSharp-reference`.

## Developing DiffSharp Libraries

To develop libraries built on DiffSharp which are designed for general use (not just in your own code), do the following:

1. reference `DiffSharp.Core` in your library code
2. reference `DiffSharp.Backends.Reference` in your correctness testing code.
3. reference `DiffSharp.Backends.Torch` and `libtorch-cpu` in your CPU performence testing code.
4. reference `DiffSharp.Backends.Torch` and `libtorch-cuda` in your GPU performence testing code.

## Using CI build packages

To consume CI build packages in a .NET Interactive Jupyter notebook ([![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/dotnet/interactive/master?urlpath=lab)) you can do

    #i "nuget:https://ci.appveyor.com/nuget/diffsharp"

In F# 5.0 scripts (with `--langversion:preview`) do:

    #r "nuget:RestoreSources=https://ci.appveyor.com/nuget/diffsharp"

Then add a reference to the version you want, tha package numbers can be found in the "artifacts" tabs of [the DiffSharp CI builds](https://ci.appveyor.com/project/dsyme/diffsharp/history).

    #r "nuget: DiffSharp-reference,0.9.5-preview-NNNN"

or

    #r "nuget: DiffSharp-cpu,0.9.5-preview-NNNN"
    dsharp.config(backend=Backend.Torch)

or 

    #r "nuget: DiffSharp-cuda-linux,0.9.5-preview-NNNN"
    dsharp.config(backend=Backend.Torch)

or

    #r "nuget: DiffSharp-cuda-windows,0.9.5-preview-NNNN"
    dsharp.config(backend=Backend.Torch)



## Building against locally built TorchSharp packages


To add features you may have extend TorchSharp to make extra features of LibTorch available.

The build is set up to look for a parallel build of TorchSharp, e.g.

    C:\GitHub\dsyme\DiffSharp
    C:\GitHub\dsyme\TorchSharp

To build, test and pack TorchSharp in that repo do this:

    .\build build
    .\build test
    .\build pack

You will see something like this

    Packing LibTorch.Cuda.10.2.Redist nupkg (takes a long time!)...
    Successfully created package 'C:\GitHub\dsyme\TorchSharp\bin/packages/Debug/TorchSharp.0.3.0-local-Debug-20200520.nupkg'.
    Successfully created package 'C:\GitHub\dsyme\TorchSharp\bin/packages/Debug/LibTorch.Cuda.10.2.Redist.0.3.0-local-Debug-20200520.nupkg'.
    Successfully created package 'C:\GitHub\dsyme\TorchSharp\bin/packages/Debug/LibTorch.Redist.0.3.0-local-Debug-20200520.nupkg'.

with warning:

    warning : Packages will be incomplete and unusable on other platforms...

To consume the packages into DiffSharp adust TorchSharpVersion in DIrectory.Build.props.

When rebuilding the TorchSHarp you will need to vlear your package cache to pick up the new nuget package with the same version id, e.g.

    rmdir /q /s %USERPROFILE%\.nuget\packages\torchsharp
    rmdir /q /s %USERPROFILE%\.nuget\packages\LibTorch.Redist
    rmdir /q /s %USERPROFILE%\.nuget\packages\LibTorch.Cuda.10.2.Redist
    dotnet restore

The LibTorch packages are quite large and you may need to watch disk space.




