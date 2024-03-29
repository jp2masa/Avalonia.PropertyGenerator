using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Avalonia.PropertyGenerator.CSharp.Demo
{
    internal sealed partial class DemoControl : TemplatedControl
    {
        public static readonly StyledProperty<double> NumberProperty =
            AvaloniaProperty.Register<DemoControl, double>(nameof(Number));

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
            NumberProperty.Changed.AddClassHandler<DemoControl>(
                (sender, e) =>
                {
                    if (e.NewValue is double number)
                    {
                        sender.SetAndRaise(ReadonlyTextProperty, ref sender._readonlyText, number.ToString());
                        sender.SetValue(ExistingStyledProperty, number.ToString());
                    }
                }
            );
        }

        public DemoControl()
        {
            m_text = "Hello World!";
            _readonlyText = "0";
        }
    }
}
