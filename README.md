[![Build status](https://ci.appveyor.com/api/projects/status/1ow4vfhp9t92k3bh/branch/master?svg=true)](https://ci.appveyor.com/project/jp2masa/avalonia-propertygenerator/branch/master)
[![NuGet](https://img.shields.io/nuget/v/jp2masa.Avalonia.PropertyGenerator.CSharp.svg)](https://www.nuget.org/packages/jp2masa.Avalonia.PropertyGenerator.CSharp/)
[![MyGet](https://img.shields.io/myget/jp2masa/vpre/jp2masa.Avalonia.PropertyGenerator.CSharp.svg?label=myget)](https://www.myget.org/feed/jp2masa/package/nuget/jp2masa.Avalonia.PropertyGenerator.CSharp)

# Avalonia.PropertyGenerator

Avalonia.PropertyGenerator generates the appropriate CLR members for Avalonia property definitions.

## Usage

1. Add reference to `jp2masa.Avalonia.PropertyGenerator.CSharp` package:

```xml
<PackageReference Include="jp2masa.Avalonia.PropertyGenerator.CSharp" Version="0.10.0-beta2" PrivateAssets="All" />
```

2. Declare Avalonia properties as usual, except the CLR members, which are now automatically generated!

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

        [BackingField(Name = "m_text", Accessibility = BackingFieldAccessibility.Internal)]
        public static readonly DirectProperty<DemoControl, string?> TextProperty =
            AvaloniaProperty.RegisterDirect<DemoControl, string?>(nameof(Text), o => o.Text, (o, v) => o.Text = v);

        public static readonly AttachedProperty<bool> BoolProperty =
            AvaloniaProperty.RegisterAttached<DemoControl, Control, bool>("Bool");

        public DemoControl()
        {
            m_text = "Hello World!";
        }
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

        internal string? m_text;

        public string? Text
        {
            get => m_text;
            set => SetAndRaise(TextProperty, ref m_text, value);
        }

        public static bool GetBool(Avalonia.IAvaloniaObject obj) => obj.GetValue(BoolProperty);

        public static void SetBool(Avalonia.IAvaloniaObject obj, bool value) => obj.SetValue(BoolProperty, value);
    }
}

```

## TODO

- Readonly direct properties
- Generate XML documentation
