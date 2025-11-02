/// <summary>
/// Enum defining variable data types for dialogue system
/// </summary>
public enum VariableDataType {
    Bool,
    Int,
    Float,
    String
}

/// <summary>
/// Enum defining condition types for variable comparison
/// </summary>
public enum ConditionType {
    Equal,
    NotEqual,
    Greater,
    GreaterOrEqual,
    Less,
    LessOrEqual
}

/// <summary>
/// Enum defining modification types for variables
/// </summary>
public enum ModificationType {
    Set,
    Increase,
    Decrease,
    Toggle
}