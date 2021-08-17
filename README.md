# Chess

This is a simple chess client written with
[Avalonia-UI](https://avaloniaui.net/) that interfaces with
[ChessEngine](https://github.com/rleathart/ChessEngine) (and hopefully other
engines in the future) to let you play chess!

![Image of a chess game](../assets/images/ChessSample.png)

# Building and Running

Building Chess and its dependencies has been tested with Clang 12 and dotnet
5.0.

On Windows, librgl requires that you have pthreads-win32 installed as a CMake
package. See [here](https://github.com/rleathart/pthreads-win32-CMake) for
details.

```
git clone https://github.com/ElwynJohn/Chess
cd Chess
git submodule update --init --recursive
cmake -B build -G Ninja -DBUILD_SHARED_LIBS=ON -DCMAKE_C_COMPILER=clang -DCMAKE_BUILD_TYPE=Release
cmake --build build
dotnet build/Chess.dll
```
