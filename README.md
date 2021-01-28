[![Build status](https://ci.appveyor.com/api/projects/status/1ow4vfhp9t92k3bh/branch/master?svg=true)](https://ci.appveyor.com/project/jp2masa/avalonia-propertygenerator/branch/master)
[![MyGet](https://img.shields.io/myget/jp2masa/vpre/jp2masa.Avalonia.PropertyGenerator.CSharp.svg?label=myget)](https://www.myget.org/feed/jp2masa/package/nuget/jp2masa.Avalonia.PropertyGenerator.CSharp)

# Avalonia.PropertyGenerator

Avalonia.PropertyGenerator generates the appropriate CLR members for Avalonia property definitions.

## Example

### Source

```cs
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Avalonia.PropertyGenerator.CSharp.Demo
{
    internal sealed partial class DemoControl : TemplatedControl
    {
        public static readonly StyledProperty<double> NumberProperty =
            AvaloniaProperty.Register<DemoControl, double>(nameof(Number));

        public static readonly DirectProperty<DemoControl, string?> TextProperty =
            AvaloniaProperty.RegisterDirect<DemoControl, string?>(nameof(Text), o => o.Text, (o, v) => o.Text = v);

        public static readonly AttachedProperty<bool> BoolProperty =
            AvaloniaProperty.RegisterAttached<DemoControl, Control, bool>("Bool");
    }
}
```

### Generated code

```cs
namespace Avalonia.PropertyGenerator.CSharp.Demo
{
    partial class DemoControl
    {
        public double Number
        {
            get => GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        private string? _text;

        public string? Text
        {
            get => _text;
            set => SetAndRaise(TextProperty, ref _text, value);
        }

        public static bool GetBool(IAvaloniaObject obj) => obj.GetValue(BoolProperty);

        public static void SetBool(IAvaloniaObject obj, bool value) => obj.SetValue(BoolProperty, value);
    }
}

```

## TODO

- Readonly direct properties
- Custom backing field name and accessibility for direct properties
- Generate XML documentation
