namespace System.Runtime.CompilerServices {
    /// <summary>
    /// Позволяет захватывать имя переменной, переданной в метод.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute {
        public CallerArgumentExpressionAttribute(string parameterName) {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}
