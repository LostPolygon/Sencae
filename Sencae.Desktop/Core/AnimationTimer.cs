namespace Sencae.Desktop; 

public class AnimationTimer {
    public float Duration { get; }
    public float CurrentTime { get; private set; }
    public float CurrentProgress { get; private set; }

    public AnimationTimer(float duration) {
        Duration = duration;
    }

    public void Update(float deltaTime) {
        CurrentTime = Math.Clamp(CurrentTime + deltaTime, 0, Duration);
        CurrentProgress = CurrentTime / Duration;
    }
}
