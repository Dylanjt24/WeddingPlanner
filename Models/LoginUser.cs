using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    [NotMapped]
    public class LoginUser
    {
        [Display(Name = "Email")]
        public string LoginEmail { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string LoginPassword { get; set; }
    }
}