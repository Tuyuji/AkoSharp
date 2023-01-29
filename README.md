# AkoSharp
A C# implementation of the Ako config language.

For info about Ako see [Ako's README](https://github.com/Tuyuji/Ako/blob/main/README.md)

## Getting started
```csharp
using AkoSharp;

var root = Deserializer.FromString(File.ReadAllText("myfile.ako"));
Vector3 windowSize = root["window"]["size"];
```