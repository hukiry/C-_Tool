using SharpShell.Attributes;
using SharpShell.SharpPropertySheet;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;


namespace HukiryPropertySheet
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.FileExtension, ".hxp", ".sip")]
    [COMServerAssociation(AssociationType.AllFiles)]
    [COMServerAssociation(AssociationType.Directory)]
    public class HukiryPropertySheet : SharpPropertySheet
    {
        protected override bool CanShowSheet()
        {
            return SelectedItemPaths.Count() >= 1;
        }

        protected override IEnumerable<SharpPropertyPage> CreatePages()
        {
            var view = new FileInfoPropertySheet();
            return new[] { view };
        }
    }
}
