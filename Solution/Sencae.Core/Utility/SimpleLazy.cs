using System;

namespace Sencae;

public class SimpleLazy<T>(Func<T> valueFactory) {
    private bool _isCreated;
    private T _value = default!;

    public T Value => GetValue();

    public void IfCreated(Action<T> action) {
        if (_isCreated) {
            action(_value);
        }
    }

    private T GetValue() {
        if (_isCreated)
            return _value!;

        _value = valueFactory()!;
        _isCreated = true;

        return _value;
    }
}
