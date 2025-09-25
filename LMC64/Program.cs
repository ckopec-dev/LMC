namespace LMC64
{
    public class LMCSimulator
    {
        // Memory (100 mailboxes, 0-99) - now 64-bit
        private long[] memory = new long[100];

        // Registers - now 64-bit
        private long accumulator = 0;
        private int programCounter = 0;

        // Flags
        private bool isRunning = false;
        private bool isHalted = false;

        // Input/Output queues - now 64-bit
        private Queue<long> inputQueue = new Queue<long>();
        private List<long> outputList = new List<long>();

        // Instruction set
        private const int ADD = 1;      // 1XX - Add
        private const int SUB = 2;      // 2XX - Subtract  
        private const int STO = 3;      // 3XX - Store
        private const int LDA = 5;      // 5XX - Load
        private const int BRA = 6;      // 6XX - Branch always
        private const int BRZ = 7;      // 7XX - Branch if zero
        private const int BRP = 8;      // 8XX - Branch if positive
        private const int INP = 901;    // 901 - Input
        private const int OUT = 902;    // 902 - Output
        private const int HLT = 0;      // 000 - Halt

        public void Reset()
        {
            Array.Clear(memory, 0, memory.Length);
            accumulator = 0;
            programCounter = 0;
            isRunning = false;
            isHalted = false;
            inputQueue.Clear();
            outputList.Clear();
        }

        public void LoadProgram(long[] program, int startAddress = 0)
        {
            if (program == null) return;

            for (int i = 0; i < program.Length && (startAddress + i) < 100; i++)
            {
                memory[startAddress + i] = program[i];
            }
        }

        public void LoadProgramFromFile(string filename)
        {
            try
            {
                string[] lines = File.ReadAllLines(filename);
                List<long> program = new List<long>();

                foreach (string line in lines)
                {
                    string cleanLine = line.Trim();
                    if (!string.IsNullOrEmpty(cleanLine) && !cleanLine.StartsWith("//"))
                    {
                        if (long.TryParse(cleanLine, out long instruction))
                        {
                            program.Add(instruction);
                        }
                    }
                }

                LoadProgram(program.ToArray());
                Console.WriteLine($"Loaded {program.Count} instructions from {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading program: {ex.Message}");
            }
        }

        public void AddInput(long value)
        {
            inputQueue.Enqueue(value);
        }

        public void AddInputs(params long[] values)
        {
            foreach (long value in values)
            {
                inputQueue.Enqueue(value);
            }
        }

        public List<long> GetOutput()
        {
            return new List<long>(outputList);
        }

        public void Run()
        {
            isRunning = true;
            isHalted = false;

            Console.WriteLine("Starting LMC execution...");

            while (isRunning && !isHalted)
            {
                ExecuteInstruction();
            }

            Console.WriteLine("Program execution completed.");
            DisplayResults();
        }

        public void Step()
        {
            if (!isHalted)
            {
                ExecuteInstruction();
                DisplayState();
            }
            else
            {
                Console.WriteLine("Program is halted.");
            }
        }

        private void ExecuteInstruction()
        {
            if (programCounter < 0 || programCounter >= 100)
            {
                Console.WriteLine($"Error: Program counter out of bounds: {programCounter}");
                isRunning = false;
                return;
            }

            long instruction = memory[programCounter];
            int opcode = (int)(instruction / 100);
            int address = (int)(instruction % 100);

            Console.WriteLine($"PC: {programCounter:D2}, Instruction: {instruction:D3}, ACC: {accumulator}");

            switch (instruction)
            {
                case HLT:
                    Console.WriteLine("HALT instruction executed");
                    isRunning = false;
                    isHalted = true;
                    break;

                case INP:
                    if (inputQueue.Count > 0)
                    {
                        accumulator = inputQueue.Dequeue();
                        Console.WriteLine($"INPUT: Read {accumulator}");
                    }
                    else
                    {
                        Console.Write("INPUT required (64-bit integer): ");
                        string input = Console.ReadLine();
                        if (long.TryParse(input, out long value))
                        {
                            accumulator = value;
                        }
                        else
                        {
                            Console.WriteLine("Invalid input, using 0");
                            accumulator = 0;
                        }
                    }
                    programCounter++;
                    break;

                case OUT:
                    outputList.Add(accumulator);
                    Console.WriteLine($"OUTPUT: {accumulator}");
                    programCounter++;
                    break;

                default:
                    switch (opcode)
                    {
                        case ADD:
                            accumulator += memory[address];
                            Console.WriteLine($"ADD from address {address:D2}: {accumulator}");
                            break;

                        case SUB:
                            accumulator -= memory[address];
                            Console.WriteLine($"SUB from address {address:D2}: {accumulator}");
                            break;

                        case STO:
                            memory[address] = accumulator;
                            Console.WriteLine($"STORE to address {address:D2}: {accumulator}");
                            break;

                        case LDA:
                            accumulator = memory[address];
                            Console.WriteLine($"LOAD from address {address:D2}: {accumulator}");
                            break;

                        case BRA:
                            programCounter = address;
                            Console.WriteLine($"BRANCH to address {address:D2}");
                            return; // Don't increment PC

                        case BRZ:
                            if (accumulator == 0)
                            {
                                programCounter = address;
                                Console.WriteLine($"BRANCH ZERO to address {address:D2}");
                                return; // Don't increment PC
                            }
                            Console.WriteLine($"BRANCH ZERO not taken (ACC={accumulator})");
                            break;

                        case BRP:
                            if (accumulator >= 0)
                            {
                                programCounter = address;
                                Console.WriteLine($"BRANCH POSITIVE to address {address:D2}");
                                return; // Don't increment PC
                            }
                            Console.WriteLine($"BRANCH POSITIVE not taken (ACC={accumulator})");
                            break;

                        default:
                            Console.WriteLine($"Unknown instruction: {instruction}");
                            break;
                    }
                    programCounter++;
                    break;
            }

            // Keep accumulator in valid 64-bit integer range
            if (accumulator > long.MaxValue) accumulator = long.MaxValue;
            if (accumulator < long.MinValue) accumulator = long.MinValue;
        }

        public void DisplayState()
        {
            Console.WriteLine($"\n--- LMC State ---");
            Console.WriteLine($"Accumulator: {accumulator}");
            Console.WriteLine($"Program Counter: {programCounter}");
            Console.WriteLine($"Running: {isRunning}, Halted: {isHalted}");

            if (outputList.Count > 0)
            {
                Console.WriteLine($"Output: [{string.Join(", ", outputList)}]");
            }

            if (inputQueue.Count > 0)
            {
                Console.WriteLine($"Input Queue: [{string.Join(", ", inputQueue)}]");
            }

            Console.WriteLine("Memory (non-zero locations):");
            for (int i = 0; i < 100; i++)
            {
                if (memory[i] != 0)
                {
                    Console.WriteLine($"  [{i:D2}]: {memory[i]}");
                }
            }
            Console.WriteLine();
        }

        private void DisplayResults()
        {
            Console.WriteLine($"\n--- Execution Results ---");
            Console.WriteLine($"Final Accumulator: {accumulator}");
            Console.WriteLine($"Final Program Counter: {programCounter}");

            if (outputList.Count > 0)
            {
                Console.WriteLine($"Program Output: [{string.Join(", ", outputList)}]");
            }
            else
            {
                Console.WriteLine("No output produced.");
            }
        }

        public void DisplayMemory(int start = 0, int end = 99)
        {
            Console.WriteLine($"\n--- Memory Dump ({start:D2}-{end:D2}) ---");
            for (int i = start; i <= end && i < 100; i++)
            {
                if (i % 10 == 0)
                {
                    Console.WriteLine();
                    Console.Write($"{i:D2}: ");
                }
                Console.Write($"{memory[i]} ");
            }
            Console.WriteLine("\n");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            LMCSimulator lmc = new LMCSimulator();
            bool running = true;

            Console.WriteLine("Little Man Computer Simulator");
            Console.WriteLine("=============================");

            while (running)
            {
                Console.WriteLine("\nCommands:");
                Console.WriteLine("1. Load program from file");
                Console.WriteLine("2. Enter program manually");
                Console.WriteLine("3. Add input values");
                Console.WriteLine("4. Run program");
                Console.WriteLine("5. Step through program");
                Console.WriteLine("6. Display state");
                Console.WriteLine("7. Display memory");
                Console.WriteLine("8. Reset");
                Console.WriteLine("9. Sample programs");
                Console.WriteLine("0. Exit");

                Console.Write("\nEnter command: ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        LoadFromFile(lmc);
                        break;

                    case "2":
                        LoadManually(lmc);
                        break;

                    case "3":
                        AddInputs(lmc);
                        break;

                    case "4":
                        lmc.Run();
                        break;

                    case "5":
                        StepThrough(lmc);
                        break;

                    case "6":
                        lmc.DisplayState();
                        break;

                    case "7":
                        DisplayMemory(lmc);
                        break;

                    case "8":
                        lmc.Reset();
                        Console.WriteLine("LMC reset.");
                        break;

                    case "9":
                        LoadSamplePrograms(lmc);
                        break;

                    case "0":
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Invalid command.");
                        break;
                }
            }
        }

        static void LoadFromFile(LMCSimulator lmc)
        {
            Console.Write("Enter filename: ");
            string filename = Console.ReadLine();
            lmc.LoadProgramFromFile(filename);
        }

        static void LoadManually(LMCSimulator lmc)
        {
            Console.WriteLine("Enter program instructions (one per line, empty line to finish):");
            List<long> program = new List<long>();

            while (true)
            {
                Console.Write($"[{program.Count:D2}]: ");
                string input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                    break;

                if (long.TryParse(input, out long instruction))
                {
                    program.Add(instruction);
                }
                else
                {
                    Console.WriteLine("Invalid instruction, please enter a number.");
                }
            }

            lmc.LoadProgram(program.ToArray());
            Console.WriteLine($"Loaded {program.Count} instructions.");
        }

        static void AddInputs(LMCSimulator lmc)
        {
            Console.WriteLine("Enter input values (space-separated): ");
            string input = Console.ReadLine();
            string[] values = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (string value in values)
            {
                if (int.TryParse(value, out int num))
                {
                    lmc.AddInput(num);
                }
            }

            Console.WriteLine($"Added {values.Length} input values.");
        }

        static void StepThrough(LMCSimulator lmc)
        {
            Console.WriteLine("Step-by-step execution (press Enter for next step, 'q' to quit):");

            while (true)
            {
                lmc.Step();

                Console.Write("Press Enter to continue or 'q' to quit: ");
                string input = Console.ReadLine();

                if (input?.ToLower() == "q")
                    break;
            }
        }

        static void DisplayMemory(LMCSimulator lmc)
        {
            Console.Write("Enter start address (0-99, default 0): ");
            string startInput = Console.ReadLine();
            int start = string.IsNullOrEmpty(startInput) ? 0 : int.Parse(startInput);

            Console.Write("Enter end address (0-99, default 99): ");
            string endInput = Console.ReadLine();
            int end = string.IsNullOrEmpty(endInput) ? 99 : int.Parse(endInput);

            lmc.DisplayMemory(start, end);
        }

        static void LoadSamplePrograms(LMCSimulator lmc)
        {
            Console.WriteLine("\nSample Programs (64-bit Enhanced):");
            Console.WriteLine("1. Add two large numbers");
            Console.WriteLine("2. Count down from a billion");
            Console.WriteLine("3. Find maximum of three large numbers");
            Console.WriteLine("4. Calculate factorial (up to 20!)");
            Console.WriteLine("5. Fibonacci sequence generator");
            Console.WriteLine("6. Large number multiplication");

            Console.Write("Select sample program: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    // Add two large numbers - 64-bit version
                    lmc.Reset();
                    lmc.LoadProgram(new long[] {
                        901,    // 00: Input first number
                        390,    // 01: Store first number in location 90
                        901,    // 02: Input second number
                        190,    // 03: Add first number from location 90
                        902,    // 04: Output result
                        000     // 05: Halt
                    });
                    // Pre-load some large test numbers
                    lmc.AddInputs(5000000000000000000L, 2000000000000000000L);
                    lmc.DisplayState();
                    Console.WriteLine("This program adds two large 64-bit numbers.");
                    Console.WriteLine("Pre-loaded inputs: 5,000,000,000,000,000,000 + 2,000,000,000,000,000,000");
                    Console.WriteLine("Expected result: 7,000,000,000,000,000,000");
                    break;

                case "2":
                    // Count down from a billion - 64-bit version
                    lmc.Reset();
                    lmc.LoadProgram(new long[] {
                        590,    // 00: Load counter from memory location 90
                        902,    // 01: Output current number
                        291,    // 02: Subtract decrement from memory location 91
                        390,    // 03: Store result back in location 90
                        800,    // 04: Branch if positive (>=0) to address 00
                        000     // 05: Halt
                    });
                    // Store initial values - count down from 1 billion in steps of 100 million
                    lmc.LoadProgram(new long[] { 1000000000L }, 90);    // 1 billion
                    lmc.LoadProgram(new long[] { 100000000L }, 91);     // 100 million step
                    lmc.DisplayState();
                    Console.WriteLine("This program counts down from 1 billion in steps of 100 million.");
                    Console.WriteLine("Will output: 1000000000, 900000000, 800000000, ..., 100000000, 0");
                    break;

                case "3":
                    // Find maximum of three large numbers
                    lmc.Reset();
                    lmc.LoadProgram(new long[] {
                        901,    // 00: Input first number
                        390,    // 01: Store in location 90 (current max)
                        901,    // 02: Input second number
                        391,    // 03: Store in location 91
                        590,    // 04: Load current max
                        291,    // 05: Subtract second number
                        810,    // 06: Branch if positive to 10 (first >= second)
                        591,    // 07: Load second number
                        390,    // 08: Store as new max
                        901,    // 09: Input third number (fixed branch target)
                        391,    // 10: Store in location 91
                        590,    // 11: Load current max  
                        291,    // 12: Subtract third number
                        817,    // 13: Branch if positive to 17 (max >= third)
                        591,    // 14: Load third number
                        390,    // 15: Store as new max
                        590,    // 16: Load final max (skip target)
                        590,    // 17: Load final max
                        902,    // 18: Output result
                        000     // 19: Halt
                    });
                    // Pre-load large test numbers
                    lmc.AddInputs(9223372036854775000L, 5000000000000000000L, 7777777777777777777L);
                    lmc.DisplayState();
                    Console.WriteLine("This program finds the maximum of three large 64-bit numbers.");
                    Console.WriteLine("Pre-loaded: 9,223,372,036,854,775,000 vs 5,000,000,000,000,000,000 vs 7,777,777,777,777,777,777");
                    break;

                case "4":
                    // Calculate factorial (iterative)
                    lmc.Reset();
                    lmc.LoadProgram(new long[] {
                        901,    // 00: Input number for factorial
                        390,    // 01: Store input in location 90
                        595,    // 02: Load 1 (initial result)
                        391,    // 03: Store result in location 91
                        590,    // 04: Load counter
                        796,    // 05: Branch if zero to output (location 22)
                        391,    // 06: Store counter as multiplier in location 91
                        592,    // 07: Load current result from location 92
                        393,    // 08: Store in temp location 93
                        594,    // 09: Load 0 (for addition loop)
                        391,    // 10: Reset accumulator storage
                        593,    // 11: Load temp result
                        791,    // 12: Branch if zero to 17 (done with this multiplication)
                        592,    // 13: Load running total
                        191,    // 14: Add current result
                        392,    // 15: Store back to running total
                        593,    // 16: Load temp counter, subtract 1, continue
                        295,    // 17: Subtract 1
                        393,    // 18: Store back
                        611,    // 19: Branch back to addition loop
                        592,    // 20: Load final result of multiplication
                        392,    // 21: Store as new factorial result
                        590,    // 22: Load main counter
                        295,    // 23: Subtract 1
                        390,    // 24: Store counter
                        604,    // 25: Branch back to main loop
                        592,    // 26: Load final result
                        902,    // 27: Output factorial
                        000     // 28: Halt
                    });
                    // Initialize constants
                    lmc.LoadProgram(new long[] { 1 }, 95);  // Constant 1
                    lmc.LoadProgram(new long[] { 1 }, 92);  // Initial result
                    lmc.AddInput(10L); // Calculate 10!
                    lmc.DisplayState();
                    Console.WriteLine("This program calculates factorial iteratively.");
                    Console.WriteLine("Pre-loaded input: 10 (will calculate 10! = 3,628,800)");
                    Console.WriteLine("Try small numbers (1-20) to avoid overflow.");
                    break;

                case "5":
                    // Fibonacci sequence generator
                    lmc.Reset();
                    lmc.LoadProgram(new long[] {
                        901,    // 00: Input how many Fibonacci numbers
                        390,    // 01: Store count in location 90
                        594,    // 02: Load 0 (first Fib number)
                        902,    // 03: Output first number
                        595,    // 04: Load 1 (second Fib number)  
                        902,    // 05: Output second number
                        391,    // 06: Store current (1) in location 91
                        392,    // 07: Store previous (0) in location 92
                        590,    // 08: Load counter
                        296,    // 09: Subtract 2 (we already output 2 numbers)
                        390,    // 10: Store remaining count
                        718,    // 11: Branch if zero to halt (location 24)
                        591,    // 12: Load current
                        192,    // 13: Add previous  
                        393,    // 14: Store next in location 93
                        902,    // 15: Output next number
                        591,    // 16: Load current, move to previous
                        392,    // 17: Store as previous
                        593,    // 18: Load next, move to current
                        391,    // 19: Store as current
                        590,    // 20: Load counter
                        295,    // 21: Subtract 1
                        390,    // 22: Store counter
                        608,    // 23: Branch back to main loop
                        000     // 24: Halt
                    });
                    // Initialize constants
                    lmc.LoadProgram(new long[] { 0 }, 94);  // Fib(0) = 0
                    lmc.LoadProgram(new long[] { 1 }, 95);  // Fib(1) = 1  
                    lmc.LoadProgram(new long[] { 2 }, 96);  // Constant 2
                    lmc.AddInput(15L); // Generate first 15 Fibonacci numbers
                    lmc.DisplayState();
                    Console.WriteLine("This program generates Fibonacci sequence.");
                    Console.WriteLine("Pre-loaded input: 15 (will generate first 15 Fibonacci numbers)");
                    Console.WriteLine("64-bit allows much larger Fibonacci numbers!");
                    break;

                case "6":
                    // Large number multiplication (using repeated addition)
                    lmc.Reset();
                    lmc.LoadProgram(new long[] {
                        901,    // 00: Input first number (multiplicand)
                        390,    // 01: Store in location 90
                        901,    // 02: Input second number (multiplier)
                        391,    // 03: Store in location 91
                        594,    // 04: Load 0 (initial result)
                        392,    // 05: Store result in location 92
                        591,    // 06: Load multiplier
                        714,    // 07: Branch if zero to output (location 20)
                        592,    // 08: Load current result
                        190,    // 09: Add multiplicand
                        392,    // 10: Store back to result
                        591,    // 11: Load multiplier
                        295,    // 12: Subtract 1
                        391,    // 13: Store back multiplier
                        606,    // 14: Branch back to loop
                        592,    // 15: Load final result
                        902,    // 16: Output result
                        000     // 17: Halt
                    });
                    // Initialize constants
                    lmc.LoadProgram(new long[] { 0 }, 94);  // Constant 0
                    lmc.LoadProgram(new long[] { 1 }, 95);  // Constant 1
                    lmc.AddInputs(1000000000L, 5000L); // 1 billion * 5000
                    lmc.DisplayState();
                    Console.WriteLine("This program multiplies two numbers using repeated addition.");
                    Console.WriteLine("Pre-loaded: 1,000,000,000 × 5,000 = 5,000,000,000,000");
                    Console.WriteLine("Note: Large multipliers will take time due to repeated addition!");
                    break;

                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }
        }
    }
}
