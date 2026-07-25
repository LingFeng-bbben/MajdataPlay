using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.UI.Inicators
{
    public class ListFilteredInicator : UIInicator
    {
        protected override void Awake()
        {
            base.Awake();
            var actionTo = ActionTo;
            if (actionTo == null)
            {
                actionTo = gameObject;
            }
            actionTo.SetActive(!string.IsNullOrEmpty(MajEnv.RuntimeConfig.List.OrderBy.Keyword));
        }
    }
}
