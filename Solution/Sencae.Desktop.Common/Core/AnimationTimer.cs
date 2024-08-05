namespace Sencae.Desktop; 

public class AnimationTimer(float duration) {
    public float Duration { get; } = duration;
    public float CurrentTime { get; private set; }
    public float CurrentProgress { get; private set; }

    public void Update(float deltaTime) {
        CurrentTime = Math.Clamp(CurrentTime + deltaTime, 0, Duration);
        CurrentProgress = CurrentTime / Duration;
    }
}
