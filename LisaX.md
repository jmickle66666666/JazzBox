# LisaX Script Documentation
## Semi-Auto-Generated

I recommend playing around with existing scripts as a way to learn how LisaX works! This documentation is probably more useful as a reference, but I hope it helps either way!

# Syntax

## Labels
Labels are the structure of a LisaX script; and are used for events and moving around your code. When a label is run, all code underneath the label is executed in order, until another label is found, at which point it stops.

A label is in the format `: [NAME]` where `[NAME]` is the name of your label. Use this name whenever jumping to the label.
For example:
```
print "Hello!"
goto MYLABEL

: MYLABEL
print "Also hello!"

# output:
# Hello!
# Also hello!
```

Labels can be jumped to using `if` and `goto`, and may also contain a `return` command, which jumps back to where the script was before it ran the `if` or `goto`.

This functionality can be used to implement reusable functions. For example:
```
print "Hello!"
goto MYLABEL
print "Goodbye!"

: MYLABEL
print "Also hello!"
return

# output:
# Hello!
# Also hello!
# Goodbye!
```

## Comments
As seen in the example above, any line of code beginning with `#` is ignored. This can be used to write comments or selectively disable a line of code for debugging.

## Values
Data is stored by setting or modifying stuff in named values. You can call these whatever you want and you can set them to whatever you want.
If the data you are setting it to looks like a number, you can often treat it like a number. Any mathematical operations will automatically try and use it as if it is a number.

```
set nicevalue 69
print nicevalue
if nicevalue 69 NICE

: NICE
print "Nice"

# output:
# 69
# Nice
```

When writing code, if you surround the name of a value with `$` and `;`, (like so: `$myvalue;`), then it will be replaced by what that value is. Here's an example:
```
set coolvalue 420
print coolvalue
print $coolvalue;

# output:
# coolvalue
# 420
```
As you can see, the first `print` is just treating `coolvalue` as the word itself. When using `$coolvalue;`, it is instead treated as `420`, which is what `coolvalue` is set to.

## Methods
Any line of code that isn't a label or a comment, is a method. All methods are in the format `[methodname] [arguments...]`.
The method arguments are separated by spaces ` `, and are used contextually depending on the method being called.

# Built-in functions

## end
Stops the script
Stops execution! Scripts will already stop when they run out of commands but this can be used to add clarity or add a breakpoint.

## inc
Usage: `inc [value]`
Increases a value by one. Useful for counters or loops. Equivalent to `add val 1`. If value doesn't exist, it is created and set to 1.

## dec
Usage: `dec [value]`
Decreases a value by one. Useful for counters or loops. Equivalent to `sub val 1`. If value doesn't exist, it is created and set to -1.

## add
Usage: `add [value] [amount]`
Adds amount to value. If value doesn't exist, it is created and set to `amount`.

## sub
Usage: `sub [value] [amount]`
Subtracts amount from value. If value doesn't exist, it is created and set to `-amount`.

## mul
Usage: `mul [value] [amount]`

## div
Usage: `div [value] [amount]`

## pow
Usage: `pow [value] [amount]`

## goto
Stops the script
Usage: `goto [label]`
Stops current label and runs the given one. Stores the current position, which can be returned to using `return`

## if
Usage: `if [value] [amount] [label]`
Checks if `value` is equal to `amount`, and if so, stops the current execution and runs the given label. Stores the current position, which can be returned to using `return`

## set
Usage: `set [value] [amount]`
Sets the given value to `amount`. If it doesn't exist it is created first.`

## return
Usage: `return`
Returns to the last time execution was jumped, from an `if` or `goto`. Used for running some code, then continuing where you left off.

## include
Usage: `include [path_to_script]`
Adds all the labels from another script into this script, so they can be called. Code outside of a label is not run automatically. Make sure the labels in the included script don't already exist in the current one.

## random
Usage: `random [value] [min] [max]`
Generates a random value between min and max, and stores it in `value`. Both min and max are inclusive, meaning they can be generated.
