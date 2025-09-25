
# 64-bit support 

### **Data Types Changed:**
- `int[] memory` → `long[] memory` (64-bit memory locations)
- `int accumulator` → `long accumulator` (64-bit accumulator)
- `Queue<int>` → `Queue<long>` (64-bit input queue)
- `List<int>` → `List<long>` (64-bit output list)

### **Numeric Range:**
- **Previous (32-bit)**: -2,147,483,648 to 2,147,483,647
- **New (64-bit)**: -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807

### **Updated Features:**
- All parsing operations now use `long.TryParse()`
- Bounds checking uses `long.MaxValue` and `long.MinValue`
- Sample programs include 64-bit example numbers
- Input prompts specify "64-bit integer"

## Example Usage:

**Addition Program:**
- Input: `5000000000000000000` and `1000000000000000000`
- Output: `6000000000000000000`

**Maximum Finder:**
- Try: `9223372036854775000`, `5000000000000000000`, `7000000000000000000`
- Output: `9223372036854775000`

The simulator now handles massive numbers while maintaining the same LMC instruction set architecture. Instructions still use 3-digit format (000-999), but data values can be any 64-bit signed integer!

## New 64-bit Sample Programs:

### **1. Add Two Large Numbers**
- Pre-loaded with 5 quintillion + 2 quintillion 
- Demonstrates massive number arithmetic

### **2. Count Down From a Billion**
- Counts from 1,000,000,000 down by 100,000,000 steps
- Shows large number loops and decrementation

### **3. Enhanced Maximum Finder**
- Pre-loaded with numbers near the 64-bit limit
- Fixed branch logic issues from previous version

### **4. Factorial Calculator** (NEW!)
- Calculates factorials iteratively 
- Pre-loaded to calculate 10! = 3,628,800
- Can handle up to 20! before overflow

### **5. Fibonacci Sequence Generator** (NEW!)
- Generates the first N Fibonacci numbers
- 64-bit allows much larger Fibonacci values
- Pre-set to generate first 15 numbers

### **6. Large Number Multiplication** (NEW!)
- Multiplies using repeated addition method
- Pre-loaded: 1 billion × 5,000 = 5 trillion
- Shows 64-bit multiplication capabilities

## Key Improvements:

- **Pre-loaded inputs** so programs run immediately
- **Realistic 64-bit test cases** with numbers in billions/trillions
- **Fixed branching logic** in the maximum finder
- **More complex algorithms** showcasing 64-bit arithmetic
- **Clear expected outputs** for verification

These programs demonstrate the power of 64-bit LMC simulation with numbers that would overflow traditional 32-bit systems. Try running them to see massive number computations in action!