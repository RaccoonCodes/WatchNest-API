using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using WatchNest.Attribute;

namespace WatchNest.DTO
{
    public class RequestDTO<T>
    {
        [DefaultValue(0)] //for swagger
        public int PageIndex { get; set; } = 0; //starting page of entity

        [DefaultValue(10)]
        [Range(1, 100)]
        public int PageSize { get; set; } = 10; // max number of entity per page

        [DefaultValue("TitleWatched")]
        [SortColumnValidator(typeof(SeriesDTO))]
        public string? SortColumn { get; set; } = "TitleWatched";

        [DefaultValue("ASC")]
        [SortOrderValidator]
        public string? SortOrder { get; set; } = "ASC";
        [DefaultValue(null)]
        public string? FilterQuery { get; set; } = null; //Filter for looking keywords
        [DefaultValue(null)]
        [Required]
        public string? UserID { get; set; } = null;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validator = new SortColumnValidatorAttribute(typeof(T));
            var result = validator.GetValidationResult(SortColumn, validationContext);
            return (result != null) ? new[] { result } : Array.Empty<ValidationResult>();
        }
    }
}
