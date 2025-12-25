using Microsoft.Extensions.Primitives;

namespace BlazorLayout.Utilities;

public struct KeyValueAccumulator
{
    private Dictionary<string, StringValues> _accumulator;

    private Dictionary<string, List<string>> _expandingAccumulator;

    public bool HasValues => ValueCount > 0;

    public int KeyCount => _accumulator?.Count ?? 0;

    public int ValueCount { get; private set; }

    public void Append(string key, string value)
    {
        if (_accumulator == null)
        {
            _accumulator = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        }

        if (_accumulator.TryGetValue(key, out var value2))
        {
            if (value2.Count == 0)
            {
                _expandingAccumulator[key].Add(value);
            }
            else if (value2.Count == 1)
            {
                _accumulator[key] = new string[2]
                {
                    value2[0],
                    value
                };
            }
            else
            {
                _accumulator[key] = default(StringValues);
                if (_expandingAccumulator == null)
                {
                    _expandingAccumulator = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                }

                List<string> list = new List<string>(8);
                string[] array = value2.ToArray();
                list.Add(array[0]);
                list.Add(array[1]);
                list.Add(value);
                _expandingAccumulator[key] = list;
            }
        }
        else
        {
            _accumulator[key] = new StringValues(value);
        }

        ValueCount++;
    }

    public Dictionary<string, StringValues> GetResults()
    {
        if (_expandingAccumulator != null)
        {
            foreach (KeyValuePair<string, List<string>> item in _expandingAccumulator)
            {
                _accumulator[item.Key] = new StringValues(item.Value.ToArray());
            }
        }

        return _accumulator ?? new Dictionary<string, StringValues>(0, StringComparer.OrdinalIgnoreCase);
    }
}

