namespace Sencae.Desktop;

public record AppWindowContext(
    IntPtr Renderer,
    SharedResources SharedResources,
    NotificationArea NotificationArea
);