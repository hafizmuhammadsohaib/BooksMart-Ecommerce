using BooksMart.Models.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BooksMart.Models.ViewModels
{
    public class RoleManagementVM
    {
        public ApplicationUser ApplicationUser { get; set; }
        public IEnumerable<SelectListItem> RolesList { get; set; }
        public IEnumerable<SelectListItem> CompaniesList { get; set; }
    }
}
