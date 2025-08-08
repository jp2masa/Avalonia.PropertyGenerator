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
using System.Globalization;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Avalonia.PropertyGenerator.CSharp.Demo
{
    internal sealed partial class DemoControl : TemplatedControl
    {
        public static readonly StyledProperty<decimal> NumberProperty =
            AvaloniaProperty.Register<DemoControl, decimal>(nameof(Number));

        public static readonly StyledProperty<double?> NullableNumberProperty =
            AvaloniaProperty.Register<DemoControl, double?>(nameof(NullableNumber));

        [BackingField(Name = "m_text", Accessibility = BackingFieldAccessibility.Internal)]
        public static readonly DirectProperty<DemoControl, string?> TextProperty =
            AvaloniaProperty.RegisterDirect<DemoControl, string?>(nameof(Text), o => o.Text, (o, v) => o.Text = v);

        [Readonly]
        public static readonly DirectProperty<DemoControl, string> ReadonlyTextProperty =
            AvaloniaProperty.RegisterDirect<DemoControl, string>(nameof(ReadonlyText), o => o.ReadonlyText);

        public static readonly AttachedProperty<bool> BoolProperty =
            AvaloniaProperty.RegisterAttached<DemoControl, Control, bool>("Bool");

        public static readonly StyledProperty<string> ExistingStyledProperty =
            AvaloniaProperty.Register<DemoControl, string>(nameof(ExistingStyled));

        public string ExistingStyled => GetValue(ExistingStyledProperty);

        static DemoControl()
        {
            NumberProperty.Changed.AddClassHandler<DemoControl, decimal>(
                (sender, e) =>
                {
                    var str = e.NewValue.Value.ToString(CultureInfo.CurrentCulture);

                    sender.SetAndRaise(ReadonlyTextProperty, ref sender._readonlyText, str);
                    sender.SetValue(ExistingStyledProperty, str);
                }
            );
        }

        public DemoControl()
        {
            m_text = "Hello World!";
            _readonlyText = "0";
            SetValue(ExistingStyledProperty, "0");
        }
    }
}
```

### Generated code

(To make the code more readable, generated attributes such as `GeneratedCode` and `ExcludeFromCodeCoverage` were removed.)

```cs
namespace Avalonia.PropertyGenerator.CSharp.Demo
{
    partial class DemoControl
    {
        public decimal Number
        {
            get => GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        public double? NullableNumber
        {
            get => GetValue(NullableNumberProperty);
            set => SetValue(NullableNumberProperty, value);
        }

        internal string? m_text;

        public string? Text
        {
            get => m_text;
            set => SetAndRaise(TextProperty, ref m_text, value);
        }

        private string _readonlyText;

        public string ReadonlyText => _readonlyText;

        public static bool GetBool(global::Avalonia.AvaloniaObject obj) =>
            (obj ?? throw new global::System.ArgumentNullException(nameof(obj))).GetValue(BoolProperty);

        public static void SetBool(global::Avalonia.AvaloniaObject obj, bool value) =>
            (obj ?? throw new global::System.ArgumentNullException(nameof(obj))).SetValue(BoolProperty, value);
    }
}

```
