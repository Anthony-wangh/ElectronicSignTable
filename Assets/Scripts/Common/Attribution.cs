using System;


public class PropertyValue<T>
{
    public PropertyValue(T t)
    {
        _v = t;
    }

    private T _v;

    private Action<T> _valueChanged;

    public T Value
    {
        get { return _v; }

        set
        {
            if (!_v.Equals(value))
            {
                _v = value;
                _valueChanged?.Invoke(_v);
            }
        }
    }


    public void Addlistener(Action<T> action, bool initValue = false)
    {
        _valueChanged += action;
        if (initValue)
            action?.Invoke(_v);
    }
}
