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
