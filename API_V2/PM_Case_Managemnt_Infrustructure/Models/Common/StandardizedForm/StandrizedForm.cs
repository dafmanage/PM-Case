namespace PM_Case_Managemnt_Infrustructure.Models.Common
{
    public class StandrizedForm : CommonModel
    {

        public StandrizedForm()
        {
            StandardizedFormDocuments = new HashSet<StandardizedFormDocuments>();
        }
        public string FormName { get; set; } = null!;



        public virtual ICollection<StandardizedFormDocuments> StandardizedFormDocuments { get; set; }

    }
}
