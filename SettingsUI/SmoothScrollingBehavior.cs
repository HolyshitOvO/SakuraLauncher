using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FrontendDemo
{
    /// <summary>
    /// 平滑滚动 scrollview，并不好用
    /// </summary>
    public static class SmoothScrollingBehavior
    {
        public const double ScrollMultiplier = 1.5; // 增大滚动距离的倍率

        public static readonly DependencyProperty EnableSmoothScrollingProperty =
            DependencyProperty.RegisterAttached(
                "EnableSmoothScrolling",
                typeof(bool),
                typeof(SmoothScrollingBehavior),
                new PropertyMetadata(false, OnEnableSmoothScrollingChanged));

        public static bool GetEnableSmoothScrolling(DependencyObject obj) =>
            (bool)obj.GetValue(EnableSmoothScrollingProperty);

        public static void SetEnableSmoothScrolling(DependencyObject obj, bool value) =>
            obj.SetValue(EnableSmoothScrollingProperty, value);

        /// <summary>
        /// 不好用
        /// </summary>
        public class CustomDecelerationEase : EasingFunctionBase
        {
            protected override double EaseInCore(double normalizedTime)
            {
                // 自定义减速曲线，这里用平方根实现
                return -1 * Math.Pow(1 - normalizedTime, 2); // 减速公式
            }

            protected override Freezable CreateInstanceCore()
            {
                return new CustomDecelerationEase();
            }
        }

        private static void OnEnableSmoothScrollingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer && e.NewValue is bool isEnabled && isEnabled)
            {
                scrollViewer.PreviewMouseWheel += (sender, args) =>
                {
                    args.Handled = true; // 阻止默认滚动行为
                    double fromOffset = scrollViewer.VerticalOffset;
                    double toOffset = scrollViewer.VerticalOffset - args.Delta * ScrollMultiplier; // 滚动速度调整

                    var animation = new DoubleAnimation
                    {
                        From = fromOffset,
                        To = toOffset,
                        Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                        EasingFunction = new QuadraticEase()
                    };
                    //var animation = new DoubleAnimation
                    //{
                    //    From = fromOffset,
                    //    To = toOffset,
                    //    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                    //    EasingFunction = new ExponentialEase
                    //    {
                    //        EasingMode = EasingMode.EaseOut, // 从快到慢
                    //        Exponent = 2 // 指数，越大减速越明显
                    //    }
                    //};


                    // 在动画过程中，持续更新滚动位置
                    animation.CurrentTimeInvalidated += (animSender, _) =>
                    {
                        if (animSender is AnimationClock clock && clock.CurrentProgress.HasValue)
                        {
                            double currentOffset = fromOffset + (toOffset - fromOffset) * clock.CurrentProgress.Value;
                            scrollViewer.ScrollToVerticalOffset(currentOffset);
                        }
                    };

                    scrollViewer.BeginAnimation(ScrollAnimationHelper.VerticalOffsetProperty, animation);
                };
            }
        }
    }

    public static class ScrollAnimationHelper
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "VerticalOffset",
                typeof(double),
                typeof(ScrollAnimationHelper),
                new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static double GetVerticalOffset(DependencyObject obj) =>
            (double)obj.GetValue(VerticalOffsetProperty);

        public static void SetVerticalOffset(DependencyObject obj, double value) =>
            obj.SetValue(VerticalOffsetProperty, value);

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
    }
}
