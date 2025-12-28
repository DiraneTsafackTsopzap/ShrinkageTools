using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorLayout.ExtensionsJavaScript;

    public static class JavaScriptExtensions
    {
    public static void PreventDoubleClickTextSelection(this IJSRuntime js, ElementReference element) =>
       _ = js.InvokeVoidAsync("app.preventDoubleClickTextSelection", element);


}

