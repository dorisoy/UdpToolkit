# UdpToolkit

`UdpToolkit` - library for building client/server applications interacting over UDP.

[Docs](https://udptoolkit.github.io/).

![cudes](https://user-images.githubusercontent.com/11910763/127455450-ce2f6b6a-abc9-4ccb-b030-a5b12ad23792.gif)


## Disclaimer

This project still under development and I see several stages for him:

- preview (current state)
- alpha
- beta
- production-ready release

## Features

- Small and simple
- Fully asynchronous via callbacks not freeze UI thread
- Unity support via `*.unitypackage`
- Dependency free
- Channeling (Reliable, Sequenced, Custom)
- P2P support
- Native sockets support (win/mac/linux)
- GC-Free**

** - List of requirements for GC-free behavior:
  1) All user-defined types must not contain strings, because no way for writing GC-free code for strings deserialization. 
  2) User-defined types must implement `IDisposable.Dispose`, this method would be called under the hood and return the object to the pool, see samples folder for details.
  3) Implementation of `ISerializer` must support deserialization to existing object from pool, for example [protobuf-net](https://github.com/protobuf-net/protobuf-net). 
