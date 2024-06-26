
namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    public class OrganizationProfile : CommonModel
    {

        public string OrganizationNameEnglish { get; set; } = null!;

        public string OrganizationNameInLocalLanguage { get; set; } = null!;

        public string Logo { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;


        //public int SmsCode { get; set; }

        //public string UserName { get; set; } = null!;

        //public string Password { get; set; } = null!;
    }
}
