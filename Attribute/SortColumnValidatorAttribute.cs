using System.ComponentModel.DataAnnotations;

namespace WatchNest.Attribute
{
    public class SortColumnValidatorAttribute : ValidationAttribute
    {
        public Type EntityType { get; set; }
        public SortColumnValidatorAttribute(Type entityType) : base("Value must match an existing column.")
            => (EntityType) = (entityType);

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (EntityType != null)
            {
                var strValue = value as string;
                //checks that it is not null or empty and that it ensures that EntityType matches at least one
                //property strValue
                var property = EntityType.GetProperties()
                                     .FirstOrDefault(p => p.Name.Equals(strValue, StringComparison.OrdinalIgnoreCase));

                if (property != null && (property.PropertyType == typeof(string) || property.PropertyType == typeof(int)))
                {
                    return ValidationResult.Success;
                }
            }
            return new ValidationResult(ErrorMessage);
        }
    }
}
