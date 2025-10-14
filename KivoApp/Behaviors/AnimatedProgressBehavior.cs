using Microsoft.Maui.Controls;

namespace KivoApp.Behaviors
{
    public class AnimatedProgressBehavior : Behavior<ProgressBar>
    {
        protected override void OnAttachedTo(ProgressBar progressBar)
        {
            base.OnAttachedTo(progressBar);
            progressBar.PropertyChanged += OnProgressChanged;
        }

        protected override void OnDetachingFrom(ProgressBar progressBar)
        {
            base.OnDetachingFrom(progressBar);
            progressBar.PropertyChanged -= OnProgressChanged;
        }

        private async void OnProgressChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProgressBar.Progress) && sender is ProgressBar pb)
            {
                await pb.ProgressTo(pb.Progress, 400, Easing.CubicInOut);
            }
        }
    }
}
