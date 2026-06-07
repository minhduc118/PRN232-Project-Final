using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.Utils
{
    public class FutureOrPresentDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime dateValue)
            {
                return dateValue.Date >= DateTime.Now.Date;
            }
            return true;
        }
    }
}
