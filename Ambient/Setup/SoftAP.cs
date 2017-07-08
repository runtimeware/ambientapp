using Ambient.Models;

namespace Ambient.Setup
{
    public class SoftAP
    {
        #region Private Static Members

        private static SoftAPResult softAPResult = null;

        #endregion

        #region Internal Static Properties

        public static SoftAPResult SoftAPResult
        {
            get
            {
                if (softAPResult == null)
                    ResetSoftAPResult();

                return softAPResult;
            }
        }

        #endregion

        #region Public Static Methods

        public static void ResetSoftAPResult()
        {
            softAPResult = new SoftAPResult();
        }

        public static void Start()
        {
            ResetSoftAPResult();
            SoftAPConfig.ResetSoftAPData();
        }

        #endregion
    }
}
