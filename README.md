<div align="center">

# HighLevelFunction

A high-level programming language that runs as a Mineraft Datapack.
HLF is statically typed and has syntax similar to C#.

[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/zenonet/HighLevelFunction/dotnet.yml)](https://github.com/zenonet/HighLevelFunction/actions/workflows/dotnet.yml)
[![GitHub License](https://img.shields.io/github/license/zenonet/HighLevelFunction)](https://github.com/zenonet/HighLevelFunction/blob/master/LICENSE)
[![WebTranspilerStatus](https://img.shields.io/badge/WebEditor-available-blue)](https://zenonet.de/interactive/hlfTranspiler)

</div>

## Overview
- High-level language which transpiles to complete Mineraft Datapacks
- Feature-rich web-based editor supporting syntax highlighting, realtime error annotations and hover info for symbols.
- Static type system minimizing possible runtime errors.
- Compiler (transpiler to be exact) runs fully in the web (no trust required)

## Quickstart

To try it out, open [the web editor here](https://zenonet.de/interactive/hlfTranspiler). On the left, you have an integrated editor for HLF. On the right
side, you can see the McFunction code being generated in realtime.

Make sure to check out some of the [example scripts](Examples). You can also directly load them in the [web-editor](https://zenonet.de/interactive/hlfTranspiler#examples)

Here's a little example of what HLF can do:
```c#
void load(){
    say("let's say something 5 times:");
    for(int i = 0; i < 5; i++){
        if(i == 3){
            say("Let's just skip the 3");
            continue;
        }
        say($"Now, we're at {i}"); // You can use string interpolation (inserting values into strings) like this
    }

    // let's calculate some stuff
    say($"2-8^2*3 = {2-pow(8, 2)*3}");


    // Swap two blocks
    Vector pos1 = Vector(127, 65, 2);
    Vector pos2 = Vector(126, 65, 2);

    BlockType block1 = getBlock(pos1);
    BlockType block2 = getBlock(pos2);

    setBlock(pos1, block2);
    setBlock(pos2, block1);
}
```

## The web-editor

There is a web-based editor featuring syntax highlighting and real-time transpilation and error logging. The web-editor can make use of File-System-Access-APIs to transpile your code into a datapack and save it to your PC on the fly. This means, you can write code in your browser and just run `/reload` in your Minecraft instance to run it immediately. The best part: **you don't have to install anything**

### Editor Features

- Syntax highlighting
- Realtime transpilation
- Saving datapack directly to your minecraft world using File-System-Access-API
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

HLF provides users with advanced control flow system like for- and while-loops. You can even use `break`- and `continue`-statements in loops.

### Backwards-compatibility

We know that some people prefer older minecraft versions like 1.18. Because of this, HLF is built with backwards-compatibility in mind. We try to make every feature as backwards-compatible as possible. And if a feature is not supported in your target version, HLF will tell you.

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
