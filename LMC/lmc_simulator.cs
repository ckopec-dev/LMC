using System;
using System.Collections.Generic;

public class LittleManComputer
{
    private int[] memory = new int[100];  // 100 memory locations (00-99)
    private int accumulator = 0;          // Accumulator register
    private int programCounter = 0;       // Program counter
    private bool halted = false;          // Halt flag
    private Queue<int> inputQueue = new Queue<int>();
    private List<int> output = new List<int>();
    
    // Instruction opcodes
    private const int HLT = 000;  // Halt
    private const int ADD = 100;  // Add
    private const int SUB = 200;  // Subtract
    private const int STA = 300;  // Store
    private const int LDA = 500;  // Load
    private const int BRA = 600;  // Branch always
    private const int BRZ = 700;  // Branch if zero
    private const int BRP = 800;  // Branch if positive
    private const int INP = 901;  // Input
    private const int OUT = 902;  // Output
    
    public void LoadProgram(int[] program)
    {
        Reset();
        for (int i = 0; i < program.Length && i < memory.Length; i++)
        {
            memory[i] = program[i];
        }
    }
    
    public void Reset()
    {
        Array.Clear(memory, 0, memory.Length);
        accumulator = 0;
        programCounter = 0;
        halted = false;
        inputQueue.Clear();
        output.Clear();
    }
    
    public void AddInput(int value)
    {
        inputQueue.Enqueue(value);
    }
    
    public List<int> GetOutput()
    {
        return new List<int>(output);
    }
    
    public void Step()
    {
        if (halted || programCounter >= memory.Length)
            return;
            
        int instruction = memory[programCounter];
        int opcode = instruction / 100 * 100;  // Get hundreds digit
        int operand = instruction % 100;       // Get tens and units digits
        
        programCounter++;
        
        switch (opcode)
        {
            case HLT:
                halted = true;
                break;
                
            case ADD:
                accumulator += memory[operand];
                accumulator %= 1000;  // Keep within 3 digits
                break;
                
            case SUB:
                accumulator -= memory[operand];
                if (accumulator < 0)
                    accumulator += 1000;  // Handle negative wraparound
                break;
                
            case STA:
                memory[operand] = accumulator;
                break;
                
            case LDA:
                accumulator = memory[operand];
                break;
                
            case BRA:
                programCounter = operand;
                break;
                
            case BRZ:
                if (accumulator == 0)
                    programCounter = operand;
                break;
                
            case BRP:
                if (accumulator >= 0 && accumulator < 500)  // Positive in LMC
                    programCounter = operand;
                break;
                
            case INP:
                if (inputQueue.Count > 0)
                    accumulator = inputQueue.Dequeue();
                else
                    Console.WriteLine("Warning: No input available");
                break;
                
            case OUT:
                output.Add(accumulator);
                break;
                
            default:
                Console.WriteLine($"Unknown instruction: {instruction}");
                break;
        }
    }
    
    public void Run()
    {
        while (!halted && programCounter < memory.Length)
        {
            Step();
        }
    }
    
    public void PrintState()
    {
        Console.WriteLine($"Program Counter: {programCounter:D2}");
        Console.WriteLine($"Accumulator: {accumulator:D3}");
        Console.WriteLine($"Halted: {halted}");
        Console.WriteLine("Memory (non-zero locations):");
        for (int i = 0; i < memory.Length; i++)
        {
            if (memory[i] != 0)
                Console.WriteLine($"  {i:D2}: {memory[i]:D3}");
        }
        Console.WriteLine($"Output: [{string.Join(", ", output)}]");
    }
    
    public bool IsHalted => halted;
}

// Example usage and test programs
public class Program
{
    public static void Main()
    {
        var lmc = new LittleManComputer();
        
        Console.WriteLine("=== Little Man Computer Simulator ===\n");
        
        // Test 1: Simple addition program
        Console.WriteLine("Test 1: Add two numbers (5 + 3)");
        int[] addProgram = {
            901,  // INP - Input first number
            300,  // STA 00 - Store in location 00
            901,  // INP - Input second number
            100,  // ADD 00 - Add value from location 00
            902,  // OUT - Output result
            000   // HLT - Halt
        };
        
        lmc.LoadProgram(addProgram);
        lmc.AddInput(5);
        lmc.AddInput(3);
        lmc.Run();
        
        Console.WriteLine($"Output: {string.Join(", ", lmc.GetOutput())}");
        Console.WriteLine();
        
        // Test 2: Countdown program
        Console.WriteLine("Test 2: Countdown from 5");
        int[] countdownProgram = {
            505,  // LDA 05 - Load initial value
            902,  // OUT - Output current value
            200,  // SUB 06 - Subtract 1
            300,  // STA 05 - Store back
            700,  // BRZ 08 - Branch if zero to halt
            600,  // BRA 01 - Branch back to output
            000,  // HLT - Halt
            005,  // Data: initial count value
            001   // Data: decrement value
        };
        
        lmc.LoadProgram(countdownProgram);
        lmc.Run();
        
        Console.WriteLine($"Output: {string.Join(", ", lmc.GetOutput())}");
        Console.WriteLine();
        
        // Test 3: Step-by-step execution
        Console.WriteLine("Test 3: Step-by-step execution of simple program");
        int[] stepProgram = {
            501,  // LDA 01 - Load value from location 01
            902,  // OUT - Output the value
            000,  // HLT - Halt
            042   // Data: the number 42
        };
        
        lmc.LoadProgram(stepProgram);
        
        Console.WriteLine("Initial state:");
        lmc.PrintState();
        
        Console.WriteLine("\nAfter step 1 (LDA 01):");
        lmc.Step();
        lmc.PrintState();
        
        Console.WriteLine("\nAfter step 2 (OUT):");
        lmc.Step();
        lmc.PrintState();
        
        Console.WriteLine("\nAfter step 3 (HLT):");
        lmc.Step();
        lmc.PrintState();
    }
}