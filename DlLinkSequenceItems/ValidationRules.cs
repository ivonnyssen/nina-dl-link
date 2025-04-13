using System.Globalization;
using System.Windows.Controls;

namespace IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems {

    public class IntGreaterOrEqualToOne : ValidationRule {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (value == null) {
                return new ValidationResult(false, "Invalid integer value.");
            }
            switch (int.TryParse(value.ToString(), NumberStyles.Integer, cultureInfo, out int intValue)) {
                case true when intValue >= 1:
                    return ValidationResult.ValidResult;

                case false:
                    return new ValidationResult(false, "Invalid integer value.");

                default:
                    return new ValidationResult(false, "The number must be greater than or equal to 1.");
            }
        }
    }
}