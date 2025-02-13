# HLF documentation

HLF is a high level programming language which compiled (technically transpiled) to McFunction (Minecraft Datapacks).

[![WebTranspilerStatus](https://img.shields.io/badge/WebEditor-available-blue)](https://zenonet.de/interactive/hlfTranspiler)

## Quickstart

To get started writing HLF-Datapacks, I highly recommend you try out some of the [example scripts](https://github.com/zenonet/HighLevelFunction/tree/master/Examples).
You can also load them directly from the web editor.<br>

<br>
Here is an example script written in HLF:

````csharp
// The load function is ran when the datapack is loaded
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


    // Swap two blocks:
    Vector pos1 = Vector(127, 65, 2);
    Vector pos2 = Vector(126, 65, 2);

    BlockType block1 = getBlock(pos1);
    BlockType block2 = getBlock(pos2);

    setBlock(pos1, block2);
    setBlock(pos2, block1);
}
````