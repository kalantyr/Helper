using System.Collections.Generic;

namespace Helper.Model
{
    public class ActionResult
    {
        public string Error { get; private set; }

        public string Info { get; private set; }

        public string Warning { get; private set; }

        public IReadOnlyCollection<ActionResult> InneResults { get; private set; }

        private ActionResult() { }

        public static ActionResult FromError(string error, IReadOnlyCollection<ActionResult> inneResults = null)
        {
            return new ActionResult
            {
                Error = error,
                InneResults = inneResults ?? new ActionResult[0]
            };
        }

        public static ActionResult Success(string info, IReadOnlyCollection<ActionResult> inneResults = null)
        {
            return new ActionResult
            {
                Info = info,
                InneResults = inneResults ?? new ActionResult[0]
            };
        }
    }
}
