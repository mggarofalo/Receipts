namespace Domain;

public record ValidationWarning(string Property, string Message, ValidationWarningSeverity Severity);

public enum ValidationWarningSeverity
{
	Info,
	Warning
}
