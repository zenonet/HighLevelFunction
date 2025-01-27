<div align="center">

# HighLevelFunction

A high-level programming language that runs as a Mineraft Datapack.
HLF is statically typed and has syntax similar to C#.

[![Transpiler build](https://github.com/picoHz/taxy/actions/workflows/rust.yml/badge.svg)](https://github.com/picoHz/taxy/actions/workflows/rust.yml)
![GitHub License](https://img.shields.io/github/license/zenonet/HighLevelFunction)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/zenonet/HighLevelFunction/dotnet.yml)
[![Static Badge](https://img.shields.io/badge/WebEditor-available-blue)](https://zenonet.de/interactive/hlfTranspiler)

</div>

## Quickstart

To try it out, open [the web editor here](https://zenonet.de/interactive/hlfTranspiler). On the left, you have an integrated editor for HLF. On the right
side, you can see the McFunction code being generated in realtime.

Here's a Hello-World-script:
```c#
void main(){
    say("Hello world!");
}
```

## The editor

There is a web-based editor featuring syntax highlighting and real-time transpilation and error logging. The web-editor can make use of File-System-Access-APIs to transpile your code into a datapack and save it to your PC on the fly. This means, you can write code in your browser and just run `/reload` in your Minecraft instance to run it immediately. The best part: **you don't have to install anything**

### Editor Features

- Syntax highlighting
- Realtime transpilation
- Inline error annotations
- Hover info for function calls
- Hotkeys for commenting out

## Language Features

In constrast to other McFunction abstractions, HLF can handle values of a bunch of different types like Entities, BlockTypes and Vectors. This means that you don't have to understand how different datatypes work in Datapacks. You just write code and HLF handles the rest for you.

### Mathematics

HLF provides you with a fully fledged expression system meaning you can write mathematical expression like
in any other programming language. You can even do Vector calculations like dot-product and cross-product
and use trigonometric functions.

### Program like in any other language

In McFunction and many of its existing abstraction languages, you have to think in Minecraft. This means a paradigm shift for programmers just getting started with Minecraft Datapacks.
**HLF is designed to remove this paradigm shift** and making DataPack writing more accessible to people with previous programming experience.

### Advanced control flow

HLF provides users with advanced control flow system like for- and while-loops. You can even use `break`-statements in loops.

### Backwards-compatibility

We know that some people prefer older minecraft versions like 1.18. Because of this, HLF is built with backwards-compatibility in mind. We try to make every feature as backwards-compatible as possible. And if a feature is not supported in your target version, HLF will tell you.

$$
\sqrt{24}
$$

### Builtin Types

- Entity
- String
- Vector (3 dimensional)
- BlockType
- Integers
- Floats (behave like fixed-point-numbers right now though)


### Control-Flow Statements

- if
- while
- for
