# LMC

A Little Man Computer (LMC) simulator in C#. This includes the basic instruction set and a simple interface to load and execute programs.

**Key Features:**
- **Full LMC instruction set**: HLT, ADD, SUB, STA, LDA, BRA, BRZ, BRP, INP, OUT
- **100 memory locations** (00-99) as per LMC specification
- **Accumulator and Program Counter** registers
- **Input/Output handling** with queues and lists
- **Step-by-step execution** capability
- **State inspection** methods

**Instructions Supported:**
- `000` - HLT (Halt)
- `1XX` - ADD (Add from memory location XX)
- `2XX` - SUB (Subtract from memory location XX)
- `3XX` - STA (Store accumulator to location XX)
- `5XX` - LDA (Load from memory location XX)
- `6XX` - BRA (Branch to location XX)
- `7XX` - BRZ (Branch if zero to location XX)
- `8XX` - BRP (Branch if positive to location XX)
- `901` - INP (Input to accumulator)
- `902` - OUT (Output from accumulator)

**Example Programs Included:**
1. **Addition**: Adds two input numbers
2. **Countdown**: Counts down from 5 to 0
3. **Step-by-step demo**: Shows how to execute instructions one at a time

The simulator handles three-digit arithmetic with proper wraparound, maintains program state, and provides debugging capabilities. You can load programs, add inputs, run them completely, or step through instruction by instruction.

Here are several ways to compile and run the C# Little Man Computer program:

## Method 1: Using .NET CLI (Recommended)

**Prerequisites:** Install [.NET SDK](https://dotnet.microsoft.com/download) (free)

```bash
# Save the code to a file
# Create a new console project
dotnet new console -n LittleManComputer
cd LittleManComputer

# Replace the contents of Program.cs with the LMC code
# Then compile and run
dotnet run
```

Or compile to an executable:
```bash
dotnet build
# Or for a self-contained executable:
dotnet publish -c Release --self-contained -r win-x64
```

## Method 2: Single File Compilation

Save the code as `LMC.cs` and use:
```bash
# Compile
dotnet build LMC.cs -o LMC.exe

# Or compile and run directly
dotnet run LMC.cs
```

## Method 3: Visual Studio

1. Open Visual Studio
2. Create new **Console App (.NET)** project
3. Replace the Program.cs content with the LMC code
4. Press **F5** or click **Start** to run
5. Or **Build > Build Solution** to compile

## Method 4: Visual Studio Code

1. Install C# extension
2. Save code as `Program.cs`
3. Open terminal in VS Code
4. Run: `dotnet new console --force` (creates project files)
5. Run: `dotnet run`

## Method 5: Command Line (Windows)

If you have .NET Framework installed:
```cmd
csc Program.cs
Program.exe
```

## Method 6: Online Compiler

You can also run it online at:
- [.NET Fiddle](https://dotnetfiddle.net/)
- [Replit](https://replit.com/)
- [OnlineGDB](https://www.onlinegdb.com/online_csharp_compiler)

Just paste the code and click run!

The **dotnet CLI method** is most common for modern C# development and works on Windows, macOS, and Linux.
