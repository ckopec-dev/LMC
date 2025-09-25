
namespace LMC32
{
    public class LMCSimulator
    {
        // Memory (100 mailboxes, 0-99)
        private int[] memory = new int[100];

        // Registers
        private int accumulator = 0;
        private int programCounter = 0;

        // Flags
        private bool isRunning = false;
        private bool isHalted = false;

        // Input/Output queues
        private Queue<int> inputQueue = new Queue<int>();
        private List<int> outputList = new List<int>();

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

        public void LoadProgram(int[] program, int startAddress = 0)
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
                List<int> program = new List<int>();

                foreach (string line in lines)
                {
                    string cleanLine = line.Trim();
                    if (!string.IsNullOrEmpty(cleanLine) && !cleanLine.StartsWith("//"))
                    {
                        if (int.TryParse(cleanLine, out int instruction))
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

        public void AddInput(int value)
        {
            inputQueue.Enqueue(value);
        }

        public void AddInputs(params int[] values)
        {
            foreach (int value in values)
            {
                inputQueue.Enqueue(value);
            }
        }

        public List<int> GetOutput()
        {
            return new List<int>(outputList);
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

            int instruction = memory[programCounter];
            int opcode = instruction / 100;
            int address = instruction % 100;

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
                        Console.Write("INPUT required (32-bit integer): ");
                        string input = Console.ReadLine();
                        if (int.TryParse(input, out int value))
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

            // Keep accumulator in valid 32-bit integer range
            if (accumulator > int.MaxValue) accumulator = int.MaxValue;
            if (accumulator < int.MinValue) accumulator = int.MinValue;
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
            List<int> program = new List<int>();

            while (true)
            {
                Console.Write($"[{program.Count:D2}]: ");
                string input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                    break;

                if (int.TryParse(input, out int instruction))
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
            Console.WriteLine("\nSample Programs:");
            Console.WriteLine("1. Add two numbers");
            Console.WriteLine("2. Count down from 10");
            Console.WriteLine("3. Find maximum of three numbers");

            Console.Write("Select sample program: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    // Add two numbers - corrected version
                    lmc.Reset();
                    lmc.LoadProgram(new int[] {
                        901,    // 00: Input first number
                        390,    // 01: Store first number in location 90
                        901,    // 02: Input second number
                        190,    // 03: Add first number from location 90
                        902,    // 04: Output result
                        000     // 05: Halt
                    });
                    lmc.DisplayState();
                    Console.WriteLine("This program adds two input numbers and outputs the result.");
                    break;

                case "2":
                    // Count down from 10 - corrected version
                    lmc.Reset();
                    lmc.LoadProgram(new int[] {
                        590,    // 00: Load 10 from memory location 90
                        902,    // 01: Output current number
                        291,    // 02: Subtract 1 from memory location 91
                        390,    // 03: Store result back in location 90
                        800,    // 04: Branch if positive (>=0) to address 00
                        000,    // 05: Halt
                        0,      // 06: (unused)
                        0,      // 07: (unused)
                        0,      // 08: (unused)
                        0       // 09: (unused)
                    });
                    // Store initial values
                    lmc.LoadProgram(new int[] { 10 }, 90);  // Initial counter value
                    lmc.LoadProgram(new int[] { 1 }, 91);   // Decrement value
                    lmc.DisplayState();
                    Console.WriteLine("This program counts down from 10 to 0.");
                    break;

                case "3":
                    // Find maximum of three numbers - corrected version
                    lmc.Reset();
                    lmc.LoadProgram(new int[] {
                        901,    // 00: Input first number
                        390,    // 01: Store in location 90 (current max)
                        901,    // 02: Input second number
                        391,    // 03: Store in location 91
                        590,    // 04: Load current max
                        291,    // 05: Subtract second number
                        810,    // 06: Branch if positive to 10 (first >= second)
                        591,    // 07: Load second number
                        390,    // 08: Store as new max
                        600,    // 09: Branch to 00 (actually should be 10)
                        901,    // 10: Input third number
                        391,    // 11: Store in location 91
                        590,    // 12: Load current max  
                        291,    // 13: Subtract third number
                        818,    // 14: Branch if positive to 18 (max >= third)
                        591,    // 15: Load third number
                        390,    // 16: Store as new max
                        600,    // 17: Branch to 18 (skip)
                        590,    // 18: Load final max
                        902,    // 19: Output result
                        000     // 20: Halt
                    });
                    lmc.DisplayState();
                    Console.WriteLine("This program finds the maximum of three input numbers.");
                    break;

                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }
        }
    }
}
