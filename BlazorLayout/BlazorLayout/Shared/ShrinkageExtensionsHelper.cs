namespace BlazorLayout.Shared;


    public static class ShrinkageExtensionsHelper
    {
    public static string FormatAsTime(string input)
    {
        var digits = new string((input ?? "").Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(digits))
            return "00:00";

        if (!int.TryParse(digits, out var number))
            return "00:00";

        if (digits.Length <= 3)
        {
            var ts = TimeSpan.FromMinutes(number);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}";
        }

        var hours = number / 100;
        var minutes = number % 100;
        if (minutes >= 60)
        {
            hours += minutes / 60;
            minutes %= 60;
        }

        return $"{hours:D2}:{minutes:D2}";
    }

}

