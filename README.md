# Ako
A simple config language. 

```ako
window [
    size 1280x720
    title "Game window"
    -closable
    +fullscreen
    
    # Set subsystem to null (use defaults)
    ;subsystem
    # Wayland subsystem settings.
    wayland.display "wayland-1"
]
```

## Root
Ako supports the root being a table or array, the default is a table and to make a root array just do.
```ako
[[
"my"
"array"
]]
```
Arrays are defined with double brackets and a space/new line between items in-scope.

## Types
- Null
- String
- Int
- Float
- ShortType
- Bool
- Table
- Array

Some values can be set before or after the name, currently bools and null can be set like this:
```ako
window.subsystem ; # Set subsystem to null 
;window.subsystem  # Same as above
window.enabled +   # Set window.enabled to true
-window.enabled    # same but set to false
```

## Todo
 - [ ] Vectors (800x600)
 - [ ] Schema 